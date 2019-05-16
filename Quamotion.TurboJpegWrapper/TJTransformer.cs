// <copyright file="TJTransformer.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Class for loseless transform jpeg images.
    /// </summary>
    public class TJTransformer : IDisposable
    {
        private readonly object @lock = new object();
        private IntPtr transformHandle;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="TJTransformer"/> class.
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created.
        /// </exception>
        public TJTransformer()
        {
            this.transformHandle = TurboJpegImport.TjInitTransform();

            if (this.transformHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="TJTransformer"/> class.
        /// </summary>
        ~TJTransformer()
        {
            this.Dispose(false);
        }

        /// <summary>Transforms input image into one or several destinations.</summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes).</param>
        /// <param name="transforms">Array of transform descriptions to be applied to the source image. </param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Array of transformed jpeg images.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Transforms can not be empty.</exception>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails. </exception>
        public byte[][] Transform(IntPtr jpegBuf, ulong jpegBufSize, TJTransformDescription[] transforms, TJFlags flags)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            if (transforms.Length == 0)
            {
                throw new ArgumentException("Transforms can not be empty", nameof(transforms));
            }

            // ReSharper disable once ExceptionNotDocumented
            var count = transforms.Length;
            var destBufs = new IntPtr[count];
            var destSizes = new ulong[count];

            int subsampl;
            int colorspace;
            int width;
            int height;
            var funcResult = TurboJpegImport.TjDecompressHeader(
                this.transformHandle,
                jpegBuf,
                jpegBufSize,
                out width,
                out height,
                out subsampl,
                out colorspace);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            Size mcuSize;
            if (!TurboJpegImport.MCUSizes.TryGetValue((TJSubsamplingOption)subsampl, out mcuSize))
            {
                throw new TJException("Unable to read Subsampling Options from jpeg header");
            }

            var tjTransforms = new TjTransform[count];
            for (var i = 0; i < count; i++)
            {
                var x = CorrectRegionCoordinate(transforms[i].Region.X, mcuSize.Width);
                var y = CorrectRegionCoordinate(transforms[i].Region.Y, mcuSize.Height);
                var w = CorrectRegionSize(transforms[i].Region.X, x, transforms[i].Region.W, width);
                var h = CorrectRegionSize(transforms[i].Region.Y, y, transforms[i].Region.H, height);

                tjTransforms[i] = new TjTransform
                {
                    Op = (int)transforms[i].Operation,
                    Options = (int)transforms[i].Options,
                    R = new TJRegion
                    {
                        X = x,
                        Y = y,
                        W = w,
                        H = h,
                    },
                    Data = transforms[i].CallbackData,
                    CustomFilter = transforms[i].CustomFilter,
                };
            }

            var transformsPtr = TJUtils.StructArrayToIntPtr(tjTransforms);
            try
            {
                funcResult = TurboJpegImport.TjTransform(
                    this.transformHandle,
                    jpegBuf,
                    jpegBufSize,
                    count,
                    destBufs,
                    destSizes,
                    transformsPtr,
                    (int)flags);

                if (funcResult == -1)
                {
                    TJUtils.GetErrorAndThrow();
                }

                var result = new List<byte[]>();
                for (var i = 0; i < destBufs.Length; i++)
                {
                    var ptr = destBufs[i];
                    var size = destSizes[i];
                    var item = new byte[size];
                    Marshal.Copy(ptr, item, 0, (int)size);
                    result.Add(item);

                    TurboJpegImport.TjFree(ptr);
                }

                return result.ToArray();
            }
            finally
            {
                TJUtils.FreePtr(transformsPtr);
            }
        }

        /// <summary>Transforms input image into one or several destinations.</summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="transforms">Array of transform descriptions to be applied to the source image. </param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Array of transformed jpeg images.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Transforms can not be empty.</exception>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails. </exception>
        public unsafe byte[][] Transform(byte[] jpegBuf, TJTransformDescription[] transforms, TJFlags flags)
        {
            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            if (transforms.Length == 0)
            {
                throw new ArgumentException("Transforms can not be empty", nameof(transforms));
            }

            fixed (byte* jpegPtr = jpegBuf)
            {
                return this.Transform((IntPtr)jpegPtr, (ulong)jpegBuf.Length, transforms, flags);
            }
        }

        /// <summary>
        /// Releases resources.
        /// </summary>
        /// <filterpriority>2.</filterpriority>
        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            lock (this.@lock)
            {
                if (this.isDisposed)
                {
                    return;
                }

                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Correct region coordinate to be evenly divisible by the MCU block dimension.
        /// </summary>
        /// <returns>
        /// The aligned region coordinate.
        /// </returns>
        private static int CorrectRegionCoordinate(int desiredCoordinate, int mcuBlockSize)
        {
            var realCoordinate = desiredCoordinate;
            var remainder = realCoordinate % mcuBlockSize;
            if (remainder != 0)
            {
                realCoordinate = realCoordinate - remainder < 0 ? 0 : realCoordinate - remainder;
            }

            return realCoordinate;
        }

        private static int CorrectRegionSize(int desiredCoordinate, int realCoordinate, int desiredSize, int imageSize)
        {
            var delta = desiredCoordinate - realCoordinate;
            if (desiredCoordinate == realCoordinate)
            {
                if (realCoordinate + desiredSize < imageSize)
                {
                    return desiredSize;
                }
                else
                {
                    return imageSize - realCoordinate;
                }
            }
            else
            {
                if (realCoordinate + delta + desiredSize < imageSize)
                {
                    return desiredSize + delta;
                }
                else
                {
                    return imageSize - realCoordinate;
                }
            }
        }

        private void Dispose(bool callFromUserCode)
        {
            if (callFromUserCode)
            {
                this.isDisposed = true;
            }

            // If for whathever reason, the handle was not initialized correctly (e.g. an exception
            // in the constructor), we shouldn't free it either.
            if (this.transformHandle != IntPtr.Zero)
            {
                TurboJpegImport.TjDestroy(this.transformHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                this.transformHandle = IntPtr.Zero;
            }
        }
    }
}
