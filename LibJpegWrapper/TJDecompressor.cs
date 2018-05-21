using System;
using System.Drawing;
using System.Drawing.Imaging;
// ReSharper disable MemberCanBePrivate.Global

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format
    /// </summary>
    public class TJDecompressor : IDisposable
    {
        private static ColorPalette _grayscalePalette;
        private IntPtr _decompressorHandle;
        private bool _isDisposed;
        private readonly object _lock;

        /// <summary>
        /// Static constructor to create grayscale palette object
        /// </summary>
        static TJDecompressor()
        {
            using (var bitmap = new Bitmap(1, 1, PixelFormat.Format8bppIndexed)) // a ColorPalette cannot be created. Therefore, create a dummy Bitmap to get a template from
            {
                _grayscalePalette = bitmap.Palette;
                for (var index = 0; index < _grayscalePalette.Entries.Length; ++index) // set the Color entries to grayscale
                    _grayscalePalette.Entries[index] = Color.FromArgb(index, index, index);
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="TJDecompressor"/>
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created
        /// </exception>
        public TJDecompressor()
        {
            _lock = new object();
            _decompressorHandle = TurboJpegImport.tjInitDecompress();

            if (_decompressorHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <param name="width">Width of image in pixels</param>
        /// <param name="height">Height of image in pixels</param>
        /// <param name="stride">Bytes per line in the destination image</param>
        /// <returns>Raw pixel data of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        public byte[] Decompress(IntPtr jpegBuf, ulong jpegBufSize, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            int outBufSize;
            GetImageInfo(jpegBuf, jpegBufSize, destPixelFormat, out width, out height, out stride, out outBufSize);

            using (var outBuf = new TJUnmanagedMemory(outBufSize))
            {
                DecompressInternal(jpegBuf, jpegBufSize, outBuf, outBufSize, destPixelFormat, flags, width, height, stride);
                return outBuf;
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <param name="outBuf">Buffer of unmanaged memory to hold the decompressed image</param>
        /// <param name="outBufSize">Size of the output buffer (in bytes)</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <param name="width">Width of image in pixels</param>
        /// <param name="height">Height of image in pixels</param>
        /// <param name="stride">Bytes per line in the destination image</param>
        /// <returns>Raw pixel data of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        public void Decompress(IntPtr jpegBuf, ulong jpegBufSize, IntPtr outBuf, int outBufSize, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            int bufSize;
            GetImageInfo(jpegBuf, jpegBufSize, destPixelFormat, out width, out height, out stride, out bufSize);

            if (outBufSize < bufSize)
            {
                throw new ArgumentOutOfRangeException(nameof(outBufSize));
            }

            DecompressInternal(jpegBuf, jpegBufSize, outBuf, outBufSize, destPixelFormat, flags, width, height, stride);
        }

        /// <summary>
        /// Internal method to decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <param name="outBuf">Buffer of unmanaged memory to hold the decompressed image</param>
        /// <param name="outBufSize">Size of the output buffer (in bytes)</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat" /> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags" /> "flags"</param>
        /// <param name="width">Width of image in pixels</param>
        /// <param name="height">Height of image in pixels</param>
        /// <param name="stride">Bytes per line in the destination image</param>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        private void DecompressInternal(IntPtr jpegBuf, ulong jpegBufSize, IntPtr outBuf, int outBufSize, TJPixelFormats destPixelFormat, TJFlags flags, int width, int height, int stride)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var funcResult = TurboJpegImport.tjDecompress(
                _decompressorHandle,
                jpegBuf,
                jpegBufSize,
                outBuf,
                width,
                stride,
                height,
                (int)destPixelFormat,
                (int)flags);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <param name="width">Width of image in pixels</param>
        /// <param name="height">Height of image in pixels</param>
        /// <param name="stride">Bytes per line in the destination image</param>
        /// <returns>Raw pixel data of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        public byte[] Decompress(byte[] jpegBuf, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var jpegBufSize = (ulong)jpegBuf.Length;
            using (var jpegBufPtr = new TJUnmanagedMemory(jpegBuf))
                return Decompress(jpegBufPtr, jpegBufSize, destPixelFormat, flags, out width, out height, out stride);
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <returns>Decompressed image of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public Bitmap Decompress(IntPtr jpegBuf, ulong jpegBufSize, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var targetFormat = TJUtils.ConvertPixelFormat(destPixelFormat);
            int width;
            int height;
            int stride;
            int outBufSize;
            GetImageInfo(jpegBuf, jpegBufSize, targetFormat, out width, out height, out stride, out outBufSize);

            var result = new Bitmap(width, height, destPixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, destPixelFormat);
            try
            {
                // BitmapData.Stride may be negative if the bitmap is bottom up. In this case, BitmapData.Scan0 is the last scan line -> recalculate start of unmanaged memory
                stride = Math.Abs(dstData.Stride); // use actual stride from GDI Bitmap
                var dstPtr = dstData.Stride > 0 ? dstData.Scan0 : dstData.Scan0 - ((dstData.Height - 1) * stride);
                DecompressInternal(jpegBuf, jpegBufSize, dstPtr, stride * height, targetFormat, flags, width, height, stride);
            }
            finally
            {
                result.UnlockBits(dstData);
            }
            if (destPixelFormat == PixelFormat.Format8bppIndexed)
            {
                result.Palette = _grayscalePalette;
            }
            return result;
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes)</param>
        /// <returns>Decompressed image of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public Bitmap Decompress(IntPtr jpegBuf, ulong jpegBufSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            TJPixelFormats targetFormat;
            var flags = TJFlags.BOTTOMUP;
            int width;
            int height;
            int stride;
            int outBufSize;
            GetImageInfo(jpegBuf, jpegBufSize, out targetFormat, out width, out height, out stride, out outBufSize);

            var destPixelFormat = TJUtils.ConvertPixelFormat(targetFormat);
            var result = new Bitmap(width, height, destPixelFormat);
            var dstData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, destPixelFormat);
            try
            {
                // BitmapData.Stride may be negative if the bitmap is bottom up. In this case, BitmapData.Scan0 is the last scan line -> recalculate start of unmanaged memory
                stride = Math.Abs(dstData.Stride); // use actual stride from GDI Bitmap
                var dstPtr = dstData.Stride > 0 ? dstData.Scan0 : dstData.Scan0 - ((dstData.Height - 1) * stride);
                DecompressInternal(jpegBuf, jpegBufSize, dstPtr, stride * height, targetFormat, flags, width, height, stride);
            }
            finally
            {
                result.UnlockBits(dstData);
            }
            if (destPixelFormat == PixelFormat.Format8bppIndexed)
            {
                result.Palette = _grayscalePalette;
            }
            return result;
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <returns>Decompressed image of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public Bitmap Decompress(byte[] jpegBuf, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var jpegBufSize = (ulong)jpegBuf.Length;
            using (var jpegBufPtr = new TJUnmanagedMemory(jpegBuf))
                return Decompress(jpegBufPtr, jpegBufSize, destPixelFormat, flags);
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <returns>Decompressed image of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public Bitmap Decompress(byte[] jpegBuf)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var jpegBufSize = (ulong)jpegBuf.Length;
            using (var jpegBufPtr = new TJUnmanagedMemory(jpegBuf))
                return Decompress(jpegBufPtr, jpegBufSize);
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Buffer containing a JPEG image.  This buffer is not modified.
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image
        /// </param>
        /// <param name="stride">
        /// Pointer to an integer variable that will receive the stride (in bytes) of the JPEG image.
        /// </param>
        /// <param name="bufSize">
        /// The size of a buffer that can receive the uncompressed JPEG image.
        /// </param>
        public void GetImageInfo(byte[] jpegBuf, TJPixelFormats destPixelFormat, out int width, out int height, out int stride, out int bufSize)
        {
            var jpegBufSize = (ulong)jpegBuf.Length;
            using (var jpegBufPtr = new TJUnmanagedMemory(jpegBuf))
                GetImageInfo(jpegBufPtr, jpegBufSize, destPixelFormat, out width, out height, out stride, out bufSize);
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing a JPEG image.  This buffer is not modified.
        /// </param>
        /// <param name="jpegBufSize">
        /// Size of the JPEG image (in bytes)
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image
        /// </param>
        /// <param name="stride">
        /// Pointer to an integer variable that will receive the stride (in bytes) of the JPEG image.
        /// </param>
        /// <param name="bufSize">
        /// The size of a buffer that can receive the uncompressed JPEG image.
        /// </param>
        public void GetImageInfo(IntPtr jpegBuf, ulong jpegBufSize, TJPixelFormats destPixelFormat, out int width, out int height, out int stride, out int bufSize)
        {
            int subsampl;
            int colorspace;

            var funcResult = TurboJpegImport.tjDecompressHeader(_decompressorHandle, jpegBuf, jpegBufSize,
                out width, out height, out subsampl, out colorspace);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[destPixelFormat]);
            bufSize = stride * height;
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Buffer containing a JPEG image. This buffer is not modified.
        /// </param>
        /// <param name="destPixelFormat">
        /// Pointer to a variable that will receive the pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image
        /// </param>
        /// <param name="stride">
        /// Pointer to an integer variable that will receive the stride (in bytes) of the JPEG image.
        /// </param>
        /// <param name="bufSize">
        /// The size of a buffer that can receive the uncompressed JPEG image.
        /// </param>
        public void GetImageInfo(byte[] jpegBuf, out TJPixelFormats destPixelFormat, out int width, out int height, out int stride, out int bufSize)
        {
            var jpegBufSize = (ulong)jpegBuf.Length;
            using (var jpegBufPtr = new TJUnmanagedMemory(jpegBuf))
                GetImageInfo(jpegBufPtr, jpegBufSize, out destPixelFormat, out width, out height, out stride, out bufSize);
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing a JPEG image. This buffer is not modified.
        /// </param>
        /// <param name="jpegBufSize">
        /// Size of the JPEG image (in bytes)
        /// </param>
        /// <param name="destPixelFormat">
        /// Pointer to a variable that will receive the pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image
        /// </param>
        /// <param name="stride">
        /// Pointer to an integer variable that will receive the stride (in bytes) of the JPEG image.
        /// </param>
        /// <param name="bufSize">
        /// The size of a buffer that can receive the uncompressed JPEG image.
        /// </param>
        public void GetImageInfo(IntPtr jpegBuf, ulong jpegBufSize, out TJPixelFormats destPixelFormat, out int width, out int height, out int stride, out int bufSize)
        {
            int subsampl;
            int colorspace;

            var funcResult = TurboJpegImport.tjDecompressHeader(_decompressorHandle, jpegBuf, jpegBufSize,
                out width, out height, out subsampl, out colorspace);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            // Since JPEG can only be 8 or 24 bit, select TJPF_GRAY when both colorspace and subsampling say that there is no color information, otherwise use normal windows TJPF_BGR format
            destPixelFormat = (TJColorSpaces)colorspace == TJColorSpaces.TJCS_GRAY && (TJSubsamplingOptions)subsampl == TJSubsamplingOptions.TJSAMP_GRAY ? TJPixelFormats.TJPF_GRAY : TJPixelFormats.TJPF_BGR;
            stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[destPixelFormat]);
            bufSize = stride * height;
        }

        /// <summary>
        /// Given the size of an image, determines the size of a decompressed image.
        /// </summary>
        /// <param name="height">
        /// The height of the image.
        /// </param>
        /// <param name="width">
        /// The width of the image.
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <returns>
        /// The size of a buffer that can hold the uncompressed image.
        /// </returns>
        public int GetBufferSize(int height, int width, TJPixelFormats destPixelFormat)
        {
            int stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[destPixelFormat]);
            return stride * height;
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

            // If for whathever reason, the handle was not initialized correctly (e.g. an exception
            // in the constructor), we shouldn't free it either.
            if (_decompressorHandle != IntPtr.Zero)
            {
                TurboJpegImport.tjDestroy(_decompressorHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                _decompressorHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~TJDecompressor()
        {
            Dispose(false);
        }
    }
}
