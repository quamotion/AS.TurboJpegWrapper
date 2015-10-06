using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format
    /// </summary>
    public class TJDecompressor : IDisposable
    {
        private readonly IntPtr _decompressorHandle;
        private bool _isDisposed;
        private readonly object _lock = new object();

        static TJDecompressor()
        {
            TJUtils.SetUnmanagedDllPath();
        }
        /// <summary>
        /// Creates new instance of <see cref="TJDecompressor"/>
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created
        /// </exception>
        public TJDecompressor()
        {
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
        /// <returns>Raw pixel data of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="OutOfMemoryException">Unable to allocate buffer to store decompressed image</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public Bitmap Decompress(IntPtr jpegBuf, ulong jpegBufSize, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            int width;
            int height;
            int subsampl;
            int colorspace;
            var funcResult = TurboJpegImport.tjDecompressHeader(_decompressorHandle, jpegBuf, jpegBufSize,
                out width, out height, out subsampl, out colorspace);

            if (funcResult == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            var targetFormat = TJUtils.ConvertPixelFormat(destPixelFormat);

            var bufSize = width * height * TurboJpegImport.PixelSizes[targetFormat];
            var buf = Marshal.AllocHGlobal(bufSize);

            try
            {
                funcResult = TurboJpegImport.tjDecompress(
                    _decompressorHandle,
                    jpegBuf,
                    jpegBufSize,
                    buf,
                    width,
                    0,
                    height,
                    (int)targetFormat,
                    (int)flags);

                if (funcResult == -1)
                {
                    TJUtils.GetErrorAndThrow();
                }
                var result = new Bitmap(width, height, destPixelFormat);
                var data = result.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    destPixelFormat);
                data.Scan0 = buf;
                result.UnlockBits(data);
                return result;

            }
            catch (TJException)
            {
                Marshal.FreeHGlobal(buf);
                throw;
            }
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <returns>Raw pixel data of specified format</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="OutOfMemoryException">Unable to allocate buffer to store decompressed image</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed</exception>
        public unsafe Bitmap Decompress(byte[] jpegBuf, PixelFormat destPixelFormat, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");
            
            var jpegBufSize = (ulong)jpegBuf.Length;
            fixed (byte* jpegPtr = jpegBuf)
            {
                return Decompress((IntPtr) jpegPtr, jpegBufSize, destPixelFormat, flags);
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
            TurboJpegImport.tjDestroy(_decompressorHandle);
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
