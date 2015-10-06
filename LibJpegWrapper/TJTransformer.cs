using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TS.NativeTools;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Class for loseless transform jpeg images
    /// </summary>
    public class TJTransformer
    {
        private readonly IntPtr _transformHandle;
        private bool _isDisposed;
        private readonly object _lock = new object();

        static TJTransformer()
        {
            TJUtils.SetUnmanagedDllPath();
        }

        /// <summary>
        /// Creates new instance of <see cref="TJTransformer"/>
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created
        /// </exception>
        public TJTransformer()
        {
            _transformHandle = TurboJpegImport.tjInitTransform();
            if (_transformHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>Transforms input image into one or several destinations</summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <param name="transforms">Array of transform descriptions to be applied to the source image </param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <returns>Array of transformed jpeg images</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Transforms can not be empty</exception>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails </exception>
        public byte[][] Transform(IntPtr jpegBuf, ulong jpegBufSize, TJTransformDescription[] transforms, TJFlags flags)
        {
            if (transforms == null)
                throw new ArgumentNullException("transforms");
            if (transforms.Length == 0)
                throw new ArgumentException("Transforms can not be empty", "transforms");

            // ReSharper disable once ExceptionNotDocumented
            var count = transforms.Length;
            var destBufs = new IntPtr[count];
            var destSizes = new ulong[count];

            var tjTransforms = new tjtransform[count];
            for (var i = 0; i < count; i++)
            {
                tjTransforms[i] = new tjtransform
                {
                    op = (int)transforms[i].Operation,
                    options = (int)transforms[i].Options,
                    r = transforms[i].Region
                };
            }
            var transformsPtr = InteropUtils.StructArrayToIntPtr(tjTransforms);
            try
            {
                var funcResult = TurboJpegImport.tjTransform(_transformHandle, jpegBuf, jpegBufSize, count, destBufs,
                    destSizes, transformsPtr, (int)flags);
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

                    TurboJpegImport.tjFree(ptr);
                }
                return result.ToArray();

            }
            finally
            {
                InteropUtils.FreePtr(transformsPtr);
            }
        }
        /// <summary>Transforms input image into one or several destinations</summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="transforms">Array of transform descriptions to be applied to the source image </param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <returns>Array of transformed jpeg images</returns>
        /// <exception cref="ArgumentNullException"><paramref name="transforms"/> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentException">Transforms can not be empty</exception>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails </exception>
        public unsafe byte[][] Transform(byte[] jpegBuf, TJTransformDescription[] transforms, TJFlags flags)
        {
            if (transforms == null)
                throw new ArgumentNullException("transforms");
            if (transforms.Length == 0)
                throw new ArgumentException("Transforms can not be empty", "transforms");

            fixed (byte* jpegPtr = jpegBuf)
            {
                return Transform((IntPtr) jpegPtr, (ulong) jpegBuf.Length, transforms, flags);
            }
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {

            if (_isDisposed)
                return;

            lock (_lock)
            {
                if (_isDisposed)
                    return;

                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        private void Dispose(bool callFromUserCode)
        {
            if (callFromUserCode)
            {
                _isDisposed = true;
            }
            TurboJpegImport.tjDestroy(_transformHandle);
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~TJTransformer()
        {
            Dispose(false);
        }
    }
}
