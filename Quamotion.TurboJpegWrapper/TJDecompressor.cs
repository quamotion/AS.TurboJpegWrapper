using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format.
    /// </summary>
    public class TJDecompressor : IDisposable
    {
        private IntPtr decompressorHandle = IntPtr.Zero;
        private bool isDisposed;
        private readonly object @lock = new object();

        /// <summary>
        /// Creates new instance of <see cref="TJDecompressor"/>.
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created.
        /// </exception>
        public TJDecompressor()
        {
            this.decompressorHandle = TurboJpegImport.TjInitDecompress();

            if (this.decompressorHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes).</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <param name="width">Width of image in pixels.</param>
        /// <param name="height">Height of image in pixels.</param>
        /// <param name="stride">Bytes per line in the destination image.</param>
        /// <returns>Raw pixel data of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        public unsafe byte[] Decompress(IntPtr jpegBuf, ulong jpegBufSize, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            int outBufSize;
            this.GetImageInfo(jpegBuf, jpegBufSize, destPixelFormat, out width, out height, out stride, out outBufSize);

            var buf = new byte[outBufSize];

            fixed (byte* bufPtr = buf)
            {
                this.Decompress(jpegBuf, jpegBufSize, (IntPtr)bufPtr, outBufSize, destPixelFormat, flags, out width, out height, out stride);
            }

            return buf;
        }

        public unsafe void Decompress(IntPtr jpegBuf, ulong jpegBufSize, IntPtr outBuf, int outBufSize, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("this");
            }

            int subsampl;
            int colorspace;
            var funcResult = TurboJpegImport.TjDecompressHeader(this.decompressorHandle, jpegBuf, jpegBufSize,
                out width, out height, out subsampl, out colorspace);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            var targetFormat = destPixelFormat;
            stride = TurboJpegImport.TJPAD(width * TurboJpegImport.PixelSizes[targetFormat]);
            var bufSize = stride * height;

            if (outBufSize < bufSize)
            {
                throw new ArgumentOutOfRangeException(nameof(outBufSize));
            }

            funcResult = TurboJpegImport.TjDecompress(
                this.decompressorHandle,
                jpegBuf,
                jpegBufSize,
                outBuf,
                width,
                stride,
                height,
                (int)targetFormat,
                (int)flags);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <param name="width">Width of image in pixels.</param>
        /// <param name="height">Height of image in pixels.</param>
        /// <param name="stride">Bytes per line in the destination image.</param>
        /// <returns>Raw pixel data of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        public unsafe byte[] Decompress(byte[] jpegBuf, TJPixelFormats destPixelFormat, TJFlags flags, out int width, out int height, out int stride)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("this");
            }

            var jpegBufSize = (ulong)jpegBuf.Length;
            fixed (byte* jpegPtr = jpegBuf)
            {
                return this.Decompress((IntPtr)jpegPtr, jpegBufSize, destPixelFormat, flags, out width, out height, out stride);
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes).</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Decompressed image of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed.</exception>
        public unsafe Bitmap Decompress(IntPtr jpegBuf, ulong jpegBufSize, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("this");
            }

            var targetFormat = TJUtils.ConvertPixelFormat(destPixelFormat);
            int width;
            int height;
            int stride;
            var buffer = this.Decompress(jpegBuf, jpegBufSize, targetFormat, flags, out width, out height, out stride);
            Bitmap result;
            fixed (byte* bufferPtr = buffer)
            {
                result = new Bitmap(width, height, stride, destPixelFormat, (IntPtr)bufferPtr);
                if (destPixelFormat == PixelFormat.Format8bppIndexed)
                {
                    result.Palette = this.FixPaletteToGrayscale(result.Palette);
                }
            }

            return result;
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Decompressed image of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed.</exception>
        public unsafe Bitmap Decompress(byte[] jpegBuf, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (this.isDisposed)
            {
                throw new ObjectDisposedException("this");
            }

            var jpegBufSize = (ulong)jpegBuf.Length;
            fixed (byte* jpegPtr = jpegBuf)
            {
                return this.Decompress((IntPtr)jpegPtr, jpegBufSize, destPixelFormat, flags);
            }
        }

        /// <summary>
        /// Retrieve information about a JPEG image without decompressing it.
        /// </summary>
        /// <param name="jpegBuf">
        /// Pointer to a buffer containing a JPEG image.  This buffer is not modified.
        /// </param>
        /// <param name="jpegBufSize">
        /// Size of the JPEG image (in bytes).
        /// </param>
        /// <param name="destPixelFormat">
        /// The pixel format of the uncompressed image.
        /// </param>
        /// <param name="width">
        /// Pointer to an integer variable that will receive the width (in pixels) of the JPEG image.
        /// </param>
        /// <param name="height">
        /// Pointer to an integer variable that will receive the height (in pixels) of the JPEG image.
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

            var funcResult = TurboJpegImport.TjDecompressHeader(this.decompressorHandle, jpegBuf, jpegBufSize,
                out width, out height, out subsampl, out colorspace);

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

        private ColorPalette FixPaletteToGrayscale(ColorPalette palette)
        {
            for (var index = 0; index < palette.Entries.Length; ++index)
            {
                palette.Entries[index] = Color.FromArgb(index, index, index);
            }

            return palette;
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

        private void Dispose(bool callFromUserCode)
        {
            if (callFromUserCode)
            {
                this.isDisposed = true;
            }

            // If for whathever reason, the handle was not initialized correctly (e.g. an exception
            // in the constructor), we shouldn't free it either.
            if (this.decompressorHandle != IntPtr.Zero)
            {
                TurboJpegImport.TjDestroy(this.decompressorHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                this.decompressorHandle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// Finalizer
        /// </summary>
        ~TJDecompressor()
        {
            this.Dispose(false);
        }
    }
}
