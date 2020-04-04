// <copyright file="TurboJpegImport.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// P/Invoke declarations for turbojpeg.
    /// </summary>
    internal static class TurboJpegImport
    {
        /// <summary>
        /// Pixel size (in bytes) for a given pixel format.
        /// </summary>
        public static readonly Dictionary<TJPixelFormat, int> PixelSizes = new Dictionary<TJPixelFormat, int>
        {
            { TJPixelFormat.RGB, 3 },
            { TJPixelFormat.BGR, 3 },
            { TJPixelFormat.RGBX, 4 },
            { TJPixelFormat.BGRX, 4 },
            { TJPixelFormat.XBGR, 4 },
            { TJPixelFormat.XRGB, 4 },
            { TJPixelFormat.Gray, 1 },
            { TJPixelFormat.RGBA, 4 },
            { TJPixelFormat.BGRA, 4 },
            { TJPixelFormat.ABGR, 4 },
            { TJPixelFormat.ARGB, 4 },
            { TJPixelFormat.CMYK, 4 },
        };

        /// <summary>
        /// MCU block width (in pixels) for a given level of chrominance subsampling.
        /// MCU block sizes:
        /// <list type="bullet">
        /// <item><description>8x8 for no subsampling or grayscale</description></item>
        /// <item><description>16x8 for 4:2:2</description></item>
        /// <item><description>8x16 for 4:4:0</description></item>
        /// <item><description>16x16 for 4:2:0</description></item>
        /// <item><description>32x8 for 4:1:1</description></item>
        /// </list>
        /// </summary>
        public static readonly Dictionary<TJSubsamplingOption, Size> MCUSizes = new Dictionary<TJSubsamplingOption, Size>
        {
            { TJSubsamplingOption.Gray, new Size(8, 8) },
            { TJSubsamplingOption.Chrominance444, new Size(8, 8) },
            { TJSubsamplingOption.Chrominance422, new Size(16, 8) },
            { TJSubsamplingOption.Chrominance420, new Size(16, 16) },
            { TJSubsamplingOption.Chrominance440, new Size(8, 16) },
            { TJSubsamplingOption.Chrominance411, new Size(32, 8) },
        };

        public const string UnmanagedLibrary = "turbojpeg";

#if NET45
        static TurboJpegImport()
        {
            Load();
        }
#endif

#if !NET45 && !NETSTANDARD2_0
        static TurboJpegImport()
        {
            LibraryResolver.EnsureRegistered();
        }
#endif

        /// <summary>
        /// Gets a value indicating whether the turbojpeg native library could be found.
        /// </summary>
        public static bool LibraryFound
        {
            get;
            private set;
        }
#if !NET45
        = true;
#endif

#if NET45
        /// <summary>
        /// Attempts to load the native library.
        /// </summary>
        public static void Load()
        {
            Load(AppDomain.CurrentDomain.SetupInformation.ApplicationBase);
        }

        /// <summary>
        /// Attempst to load the native library.
        /// </summary>
        /// <param name="directory">
        /// The path to the directory in which the native library is located.
        /// </param>
        public static void Load(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!Directory.Exists(directory))
            {
                throw new ArgumentOutOfRangeException(nameof(directory), $"The directory '{directory}' does not exist.");
            }

            // When the library is first called, call LoadLibrary with the full path to the
            // path of the various libaries, to make sure they are loaded from the exact
            // path we specify.

            // Any load errors would also be caught by us here, making it easier to troubleshoot.
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string nativeLibrariesDirectory;

                if (Environment.Is64BitProcess)
                {
                    nativeLibrariesDirectory = Path.Combine(directory, "win7-x64");
                }
                else
                {
                    nativeLibrariesDirectory = Path.Combine(directory, "win7-x86");
                }

                if (!Directory.Exists(nativeLibrariesDirectory))
                {
                    throw new ArgumentOutOfRangeException(nameof(directory), $"The directory '{directory}' does not contain a subdirectory for the current architecture. The directory '{nativeLibrariesDirectory}' does not exist.");
                }

                string path = Path.Combine(nativeLibrariesDirectory, $"{UnmanagedLibrary}.dll");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"Could not load libturbojpeg from {path}", path);
                }

                // Attempt to load the libraries. If they are not found, throw an error.
                // See also http://blogs.msdn.com/b/adam_nathan/archive/2003/04/25/56643.aspx for
                // more information about GetLastWin32Error
                IntPtr result = NativeMethods.LoadLibrary(path);
                if (result == IntPtr.Zero)
                {
                    var lastError = Marshal.GetLastWin32Error();
                    var error = new Win32Exception(lastError);
                    throw error;
                }

                LibraryFound = true;
            }
            else
            {
                throw new NotSupportedException("Quamotion.TurboJpegWrapper is supported on Windows (.NET FX, .NET Core), Linux (.NET Core) and OS X (.NET Core)");
            }
        }
#endif

        /// <summary>
        /// This is port of TJPAD macros from turbojpeg.h
        /// Pad the given width to the nearest 32-bit boundary.
        /// </summary>
        /// <param name="width">Width.</param>
        /// <returns>Padded width.</returns>
        public static int TJPAD(int width)
        {
            return (width + 3) & (~3);
        }

        /// <summary>
        /// This is port of TJSCALED macros from turbojpeg.h
        /// Compute the scaled value of <paramref name="dimension"/> using the given scaling factor.
        /// </summary>
        /// <param name="dimension">Dimension to scale.</param>
        /// <param name="scalingFactor">Scaling factor.</param>
        /// <returns>
        /// The scaled value of <paramref name="dimension"/> using the given scaling factor.
        /// </returns>
        public static int TJSCALED(int dimension, TjScalingFactor scalingFactor)
        {
            return ((dimension * scalingFactor.Num) + scalingFactor.Denom - 1) / scalingFactor.Denom;
        }

        /// <summary>
        /// Create a TurboJPEG compressor instance.
        /// </summary>
        /// <returns>
        /// handle to the newly-created instance, or <see cref="IntPtr.Zero"/>
        /// if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitCompress")]
        public static extern IntPtr TjInitCompress();

        /// <summary>
        /// Compress an RGB, grayscale, or CMYK image into a JPEG image.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG compressor or transformer instance.</param>
        ///
        /// <param name="srcBuf">
        /// Pointer to an image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.
        /// </param>
        ///
        /// <param name="width">Width (in pixels) of the source image.</param>
        ///
        /// <param name="pitch">
        /// Bytes per line in the source image.
        /// Normally, this should be <c>width * tjPixelSize[pixelFormat]</c> if the image is unpadded,
        /// or <c>TJPAD(width * tjPixelSize[pixelFormat])</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * tjPixelSize[pixelFormat]</c>.
        /// </param>
        ///
        /// <param name="height">Height (in pixels) of the source image.</param>
        ///
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="TJPixelFormat"/> "Pixel formats").</param>
        ///
        /// <param name="jpegBuf">
        /// Address of a pointer to an image buffer that will receive the JPEG image.
        /// TurboJPEG has the ability to reallocate the JPEG buffer
        /// to accommodate the size of the JPEG image.  Thus, you can choose to:
        /// <list type="number">
        /// <item>
        /// <description>pre-allocate the JPEG buffer with an arbitrary size using <see cref="TjAlloc"/> and let TurboJPEG grow the buffer as needed</description>
        /// </item>
        /// <item>
        /// <description>set <paramref name="jpegBuf"/> to NULL to tell TurboJPEG to allocate the buffer for you</description>
        /// </item>
        /// <item>
        /// <description>pre-allocate the buffer to a "worst case" size determined by calling <see cref="TjBufSize"/>.
        /// This should ensure that the buffer never has to be re-allocated (setting <see cref="TJFlags.NOREALLOC"/> guarantees this.).</description>
        /// </item>
        /// </list>
        /// If you choose option 1, <paramref name="jpegSize"/> should be set to the size of your pre-allocated buffer.
        /// In any case, unless you have set <see cref="TJFlags.NOREALLOC"/>,
        /// you should always check <paramref name="jpegBuf"/> upon return from this function, as it may have changed.
        /// </param>
        ///
        /// <param name="jpegSize">
        /// Pointer to an unsigned long variable that holds the size of the JPEG image buffer.
        /// If <paramref name="jpegBuf"/> points to a pre-allocated buffer,
        /// then <paramref name="jpegSize"/> should be set to the size of the buffer.
        /// Upon return, <paramref name="jpegSize"/> will contain the size of the JPEG image (in bytes.)
        /// If <paramref name="jpegBuf"/> points to a JPEG image buffer that is being
        /// reused from a previous call to one of the JPEG compression functions,
        /// then <paramref name="jpegSize"/> is ignored.
        /// </param>
        ///
        /// <param name="jpegSubsamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        ///
        /// <param name="jpegQual">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        ///
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        ///
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjCompress2")]
        public static extern int TjCompress2(IntPtr handle, IntPtr srcBuf, int width, int pitch, int height, int pixelFormat, ref IntPtr jpegBuf, ref ulong jpegSize, int jpegSubsamp, int jpegQual, int flags);

        /// <summary>
        /// The maximum size of the buffer (in bytes) required to hold a JPEG image with
        /// the given parameters.  The number of bytes returned by this function is
        /// larger than the size of the uncompressed source image.  The reason for this
        /// is that the JPEG format uses 16-bit coefficients, and it is thus possible
        /// for a very high-quality JPEG image with very high-frequency content to
        /// expand rather than compress when converted to the JPEG format.  Such images
        /// represent a very rare corner case, but since there is no way to predict the
        /// size of a JPEG image prior to compression, the corner case has to be handled.
        /// </summary>
        /// <param name="width">Width (in pixels) of the image.</param>
        /// <param name="height">Height (in pixels) of the image.</param>
        /// <param name="jpegSubsamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image(see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <returns>
        /// The maximum size of the buffer (in bytes) required to hold the image,
        /// or -1 if the arguments are out of bounds.
        /// </returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjBufSize")]
        public static extern long TjBufSize(int width, int height, int jpegSubsamp);

        /// <summary>
        ///  Create a TurboJPEG decompressor instance.
        /// </summary>
        /// <returns>A handle to the newly-created instance, or NULL if an error occurred(see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitDecompress")]
        public static extern IntPtr TjInitDecompress();

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG decompressor or transformer instance.</param>
        /// <param name="jpegBuf">Pointer to a buffer containing a JPEG image.  This buffer is not modified.</param>
        /// <param name="jpegSize">Size of the JPEG image (in bytes).</param>
        /// <param name="width">Pointer to an integer variable that will receive the width (in pixels) of the JPEG image.</param>
        /// <param name="height">Pointer to an integer variable that will receive the height (in pixels) of the JPEG image.</param>
        /// <param name="jpegSubsamp">
        /// Pointer to an integer variable that will receive the level of chrominance subsampling used
        /// when the JPEG image was compressed (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="jpegColorspace">Pointer to an integer variable that will receive one of the JPEG colorspace constants,
        /// indicating the colorspace of the JPEG image(see <see cref="TJColorSpace"/> "JPEG colorspaces".)</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        public static int TjDecompressHeader(
            IntPtr handle,
            IntPtr jpegBuf,
            ulong jpegSize,
            out int width,
            out int height,
            out int jpegSubsamp,
            out int jpegColorspace)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return TjDecompressHeader3_x86(
                        handle,
                        jpegBuf,
                        (uint)jpegSize,
                        out width,
                        out height,
                        out jpegSubsamp,
                        out jpegColorspace);
                case 8:
                    return TjDecompressHeader3_x64(
                        handle,
                        jpegBuf,
                        jpegSize,
                        out width,
                        out height,
                        out jpegSubsamp,
                        out jpegColorspace);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        /// <summary>
        /// Returns a list of fractional scaling factors that the JPEG decompressor in this implementation of TurboJPEG supports.
        /// </summary>
        /// <param name="numscalingfactors">Pointer to an integer variable that will receive the number of elements in the list.</param>
        /// <returns>A pointer to a list of fractional scaling factors, or <see cref="IntPtr.Zero"/> if an error is encountered (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjGetScalingFactors")]
        public static extern IntPtr TjGetScalingFactors(out int numscalingfactors);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecodeYUVPlanes")]
        public static unsafe extern int TjDecodeYUVPlanes(
            IntPtr handle,
            byte** srcPlanes,
            int* strides,
            int subsamp,
            IntPtr dstBuf,
            int width,
            int pitch,
            int height,
            int pixelFormat,
            int flags);

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG decompressor or transformer instance.</param>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegSize">Size of the JPEG image (in bytes).</param>
        /// <param name="dstBuf">
        /// Pointer to an image buffer that will receive the decompressed image.
        /// This buffer should normally be <c> pitch * scaledHeight</c> bytes in size,
        /// where <c>scaledHeight</c> can be determined by calling <see cref="TJSCALED"/> with the JPEG image height and one of the scaling factors returned by <see cref="TjGetScalingFactors"/>.
        /// The <paramref name="dstBuf"/> pointer may also be used to decompress into a specific region of a larger buffer.
        /// </param>
        /// <param name="width">
        /// Desired width (in pixels) of the destination image.
        /// If this is different than the width of the JPEG image being decompressed, then TurboJPEG will use scaling in the JPEG decompressor to generate the largest possible image that will fit within the desired width.
        /// If <paramref name="width"/> is set to 0, then only the height will be considered when determining the scaled image size.
        /// </param>
        /// <param name="pitch">
        /// Bytes per line in the destination image.  Normally, this is <c>scaledWidth* tjPixelSize[pixelFormat]</c> if the decompressed image is unpadded, else <c>TJPAD(scaledWidth * tjPixelSize[pixelFormat])</c> if each line of the decompressed image is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// <remarks>Note: <c>scaledWidth</c> can be determined by calling <see cref="TJSCALED"/> with the JPEG image width and one of the scaling factors returned by <see cref="TjGetScalingFactors"/>
        /// </remarks>
        /// You can also be clever and use the pitch parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to <c>scaledWidth* tjPixelSize[pixelFormat]</c>.
        /// </param>
        /// <param name="height">
        /// Desired height (in pixels) of the destination image.
        /// If this is different than the height of the JPEG image being decompressed, then TurboJPEG will use scaling in the JPEG decompressor to generate the largest possible image that will fit within the desired height.
        /// If <paramref name="height"/> is set to 0, then only the width will be considered when determining the scaled image size.
        /// </param>
        /// <param name="pixelFormat">Pixel format of the destination image (see <see cref="TJPixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        public static int TjDecompress(
            IntPtr handle,
            IntPtr jpegBuf,
            ulong jpegSize,
            IntPtr dstBuf,
            int width,
            int pitch,
            int height,
            int pixelFormat,
            int flags)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return TjDecompress2_x86(handle, jpegBuf, (uint)jpegSize, dstBuf, width, pitch, height, pixelFormat, flags);
                case 8:
                    return TjDecompress2_x64(handle, jpegBuf, jpegSize, dstBuf, width, pitch, height, pixelFormat, flags);

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        /// <summary>
        /// Allocate an image buffer for use with TurboJPEG.  You should always use
        /// this function to allocate the JPEG destination buffer(s) for <see cref="TjCompress2"/>
        /// and <see cref="TjTransform"/> unless you are disabling automatic buffer
        /// (re)allocation (by setting <see cref="TJFlags.NOREALLOC"/>.)
        /// </summary>
        /// <param name="bytes">The number of bytes to allocate.</param>
        /// <returns>A pointer to a newly-allocated buffer with the specified number of bytes.</returns>
        /// <seealso cref="TjFree"/>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjAlloc")]
        public static extern IntPtr TjAlloc(int bytes);

        /// <summary>
        /// Free an image buffer previously allocated by TurboJPEG.  You should always
        /// use this function to free JPEG destination buffer(s) that were automatically
        /// (re)allocated by <see cref="TjCompress2"/> or <see cref="TjTransform"/> or that were manually
        /// allocated using <see cref="TjAlloc"/>.
        /// </summary>
        /// <param name="buffer">Address of the buffer to free.</param>
        /// <seealso cref="TjAlloc"/>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjFree")]
        public static extern void TjFree(IntPtr buffer);

        /// <summary>
        /// Create a new TurboJPEG transformer instance.
        /// </summary>
        /// <returns>@return a handle to the newly-created instance, or NULL if an error occurred(see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjInitTransform")]
        public static extern IntPtr TjInitTransform();

        /// <summary>
        /// Losslessly transform a JPEG image into another JPEG image.  Lossless
        /// transforms work by moving the raw DCT coefficients from one JPEG image
        /// structure to another without altering the values of the coefficients.  While
        /// this is typically faster than decompressing the image, transforming it, and
        /// re-compressing it, lossless transforms are not free.  Each lossless
        /// transform requires reading and performing Huffman decoding on all of the
        /// coefficients in the source image, regardless of the size of the destination
        /// image.  Thus, this function provides a means of generating multiple
        /// transformed images from the same source or  applying multiple
        /// transformations simultaneously, in order to eliminate the need to read the
        /// source coefficients multiple times.
        /// </summary>
        /// <param name="handle">A handle to a TurboJPEG transformer instance.</param>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing the JPEG source image to transform.This buffer is not modified.
        /// </param>
        /// <param name="jpegSize">Size of the JPEG source image (in bytes).</param>
        /// <param name="n">The number of transformed JPEG images to generate.</param>
        /// <param name="dstBufs">
        /// Pointer to an array of n image buffers. <paramref name="dstBufs"/>[i] will receive a JPEG image that has been transformed using the parameters in <paramref name="transforms"/>[i]
        /// TurboJPEG has the ability to reallocate the JPEG buffer
        /// to accommodate the size of the JPEG image.  Thus, you can choose to:
        /// <list type="number">
        /// <item>
        /// <description>pre-allocate the JPEG buffer with an arbitrary size using <see cref="TjAlloc"/> and let TurboJPEG grow the buffer as needed</description>
        /// </item>
        /// <item>
        /// <description>set <paramref name="dstBufs"/>[i] to NULL to tell TurboJPEG to allocate the buffer for you</description>
        /// </item>
        /// <item>
        /// <description>pre-allocate the buffer to a "worst case" size determined by calling <see cref="TjBufSize"/>.
        /// This should ensure that the buffer never has to be re-allocated (setting <see cref="TJFlags.NOREALLOC"/> guarantees this.).</description>
        /// </item>
        /// </list>
        /// If you choose option 1, <paramref name="dstSizes"/>[i] should be set to the size of your pre-allocated buffer.
        /// In any case, unless you have set <see cref="TJFlags.NOREALLOC"/>,
        /// you should always check <paramref name="dstBufs"/>[i] upon return from this function, as it may have changed.
        /// </param>
        /// <param name="dstSizes">
        /// Pointer to an array of <paramref name="n"/> unsigned long variables that will
        /// receive the actual sizes (in bytes) of each transformed JPEG image.
        /// If <paramref name="dstBufs"/>[i] points to a pre-allocated buffer,
        /// then <paramref name="dstSizes"/>[i] should be set to the size of the buffer.
        /// Upon return, <paramref name="dstSizes"/>[i] will contain the size of the JPEG image (in bytes.)
        /// </param>
        /// <param name="transforms">
        /// Pointer to an array of <see cref="TurboJpegWrapper.TjTransform"/> structures, each of
        /// which specifies the transform parameters and/or cropping region for the
        /// corresponding transformed output image.
        /// </param>
        /// <param name="flags">flags the bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        public static int TjTransform(
            IntPtr handle,
            IntPtr jpegBuf,
            ulong jpegSize,
            int n,
            IntPtr[] dstBufs,
            ulong[] dstSizes,
            IntPtr transforms,
            int flags)
        {
            var intSizes = new uint[dstSizes.Length];
            for (var i = 0; i < dstSizes.Length; i++)
            {
                intSizes[i] = (uint)dstSizes[i];
            }

            int result;
            switch (IntPtr.Size)
            {
                case 4:
                    result = TjTransform_x86(handle, jpegBuf, (uint)jpegSize, n, dstBufs, intSizes, transforms, flags);
                    break;
                case 8:
                    result = TjTransform_x64(handle, jpegBuf, jpegSize, n, dstBufs, intSizes, transforms, flags);
                    break;
                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }

            for (var i = 0; i < dstSizes.Length; i++)
            {
                dstSizes[i] = intSizes[i];
            }

            return result;
        }

        /// <summary>
        /// Destroy a TurboJPEG compressor, decompressor, or transformer instance.
        /// </summary>
        /// <param name="handle">a handle to a TurboJPEG compressor, decompressor or transformer instance.</param>
        /// <returns>0 if successful, or -1 if an error occurred (see <see cref="TjGetErrorStr"/>).</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDestroy")]
        public static extern int TjDestroy(IntPtr handle);

        /// <summary>
        /// Returns a descriptive error message explaining why the last command failed.
        /// </summary>
        /// <returns>A descriptive error message explaining why the last command failed.</returns>
        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjGetErrorStr")]
        public static extern IntPtr TjGetErrorStr();

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompressHeader3")]
        private static extern int TjDecompressHeader3_x86(IntPtr handle, IntPtr jpegBuf, uint jpegSize, out int width, out int height, out int jpegSubsamp, out int jpegColorspace);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompressHeader3")]
        private static extern int TjDecompressHeader3_x64(IntPtr handle, IntPtr jpegBuf, ulong jpegSize, out int width, out int height, out int jpegSubsamp, out int jpegColorspace);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompress2")]
        private static extern int TjDecompress2_x86(IntPtr handle, IntPtr jpegBuf, uint jpegSize, IntPtr dstBuf, int width, int pitch, int height, int pixelFormat, int flags);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjDecompress2")]
        private static extern int TjDecompress2_x64(IntPtr handle, IntPtr jpegBuf, ulong jpegSize, IntPtr dstBuf, int width, int pitch, int height, int pixelFormat, int flags);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjTransform")]
        private static extern int TjTransform_x86(
            IntPtr handle,
            IntPtr jpegBuf,
            uint jpegSize,
            int n,
            IntPtr[] dstBufs,
            uint[] dstSizes,
            IntPtr transforms,
            int flags);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjTransform")]
        private static extern int TjTransform_x64(
            IntPtr handle,
            IntPtr jpegBuf,
            ulong jpegSize,
            int n,
            IntPtr[] dstBufs,
            uint[] dstSizes,
            IntPtr transforms,
            int flags);

        public static unsafe int TjCompressFromYUVPlanes(
            IntPtr handle,
            byte** srcPlanes,
            int width,
            int* strides,
            int height,
            int subsamp,
            ref IntPtr jpegBuf,
            ref uint jpegSize,
            int jpegQual,
            int flags)
        {
            switch (IntPtr.Size)
            {
                case 4:
                    return TjCompressFromYUVPlanes_x86(handle, srcPlanes, width, strides, height, subsamp, ref jpegBuf, ref jpegSize, jpegQual, flags);

                case 8:
                    ulong s = (ulong)jpegSize;
                    int ret = TjCompressFromYUVPlanes_x64(handle, srcPlanes, width, strides, height, subsamp, ref jpegBuf, ref s, jpegQual, flags);
                    jpegSize = (uint)s;
                    return ret;

                default:
                    throw new InvalidOperationException("Invalid platform. Can not find proper function");
            }
        }

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjCompressFromYUVPlanes")]
        private static unsafe extern int TjCompressFromYUVPlanes_x86(
            IntPtr handle,
            byte** srcPlanes,
            int width,
            int* strides,
            int height,
            int subsamp,
            ref IntPtr jpegBuf,
            ref uint jpegSize,
            int jpegQual,
            int flags);

        [DllImport(UnmanagedLibrary, CallingConvention = CallingConvention.Cdecl, EntryPoint = "tjCompressFromYUVPlanes")]
        private static unsafe extern int TjCompressFromYUVPlanes_x64(
            IntPtr handle,
            byte** srcPlanes,
            int width,
            int* strides,
            int height,
            int subsamp,
            ref IntPtr jpegBuf,
            ref ulong jpegSize,
            int jpegQual,
            int flags);
    }
}
