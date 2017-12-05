using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// Implements compression of RGB, CMYK, grayscale images to the jpeg format
    /// </summary>
    public class TJCompressor : IDisposable
    {
        private IntPtr _compressorHandle;
        private bool _isDisposed;
        private readonly object _lock;

        /// <summary>
        /// Creates new instance of <see cref="TJCompressor"/>
        /// </summary>
        /// <exception cref="TJException">
        /// Throws if internal compressor instance can not be created
        /// </exception>
        public TJCompressor()
        {
            _compressorHandle = TurboJpegImport.tjInitCompress();
            _lock = new object();

            if (_compressorHandle == IntPtr.Zero)
            {
                TJUtils.GetErrorAndThrow();
            }
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality
        /// </summary>
        /// <param name="srcImage"> Source image to be converted </param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOptions"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <remarks>Only <see cref="PixelFormat.Format24bppRgb"/>, <see cref="PixelFormat.Format32bppArgb"/>, <see cref="PixelFormat.Format8bppIndexed"/> pixel formats are supported</remarks>
        /// <exception cref="TJException"> Throws if compress function failed </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOptions.TJSAMP_GRAY"/> and pixel format <see cref="TJPixelFormats.TJPF_GRAY"/></description></item>
        /// </list>
        /// </exception>
        public byte[] Compress(Bitmap srcImage, TJSubsamplingOptions subSamp, int quality, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var pixelFormat = srcImage.PixelFormat;

            var width = srcImage.Width;
            var height = srcImage.Height;
            // ReSharper disable once ExceptionNotDocumented
            var srcData = srcImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
            // BitmapData.Stride may be negative if the bitmap is bottom up. In this case, BitmapData.Scan0 is the last scan line -> recalculate start of unmanaged memory
            var stride = Math.Abs(srcData.Stride);
            var srcPtr = srcData.Stride > 0 ? srcData.Scan0 : srcData.Scan0 - ((srcData.Height - 1) * stride);

            try
            {
                return Compress(srcPtr, stride, width, height, pixelFormat, subSamp, quality, flags);
            }
            finally
            {
                // ReSharper disable once ExceptionNotDocumented
                srcImage.UnlockBits(srcData);
            }
        }


        /// <summary>
        /// Compresses input image to the jpeg format with specified quality
        /// </summary>
        /// <param name="srcPtr">Pointer to an image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.</param>
        /// <param name="stride">Bytes per line in the source image.
        /// Normally, this should be <c>width * BytesPerPixel</c> if the image is unpadded,
        /// or <c>TJPAD(width * BytesPerPixel</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * BytesPerPixel</c>.</param>
        /// <param name="width">Width (in pixels) of the source image</param>
        /// <param name="height">Height (in pixels) of the source image</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat" /> "Pixel formats")</param>
        /// <param name="subSamp">The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOptions" /> "Chrominance subsampling options".)</param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags" /> "flags"</param>
        /// <param name="bufSize">Size of the returned unmanaged buffer (in bytes).</param>
        /// <returns></returns>
        /// <exception cref="System.ObjectDisposedException">this</exception>
        /// <exception cref="TJException">Throws if compress function failed</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">Some parameters' values are incompatible:
        /// <list type="bullet"><item><description>Subsampling not equals to <see cref="TJSubsamplingOptions.TJSAMP_GRAY" /> and pixel format <see cref="TJPixelFormats.TJPF_GRAY" /></description></item></list></exception>
        public TJUnmanagedMemory Compress(IntPtr srcPtr, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOptions subSamp, int quality, TJFlags flags, out ulong bufSize)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            var tjPixelFormat = TJUtils.ConvertPixelFormat(pixelFormat);
            CheckOptionsCompatibilityAndThrow(subSamp, tjPixelFormat);

            var buf = IntPtr.Zero;
            bufSize = 0;
            var result = TurboJpegImport.tjCompress2(
                _compressorHandle,
                srcPtr,
                width,
                stride,
                height,
                (int)tjPixelFormat,
                ref buf,
                ref bufSize,
                (int)subSamp,
                quality,
                (int)flags);

            if (result == -1)
            {
                TJUtils.GetErrorAndThrow();
            }

            return new TJUnmanagedMemory(buf, bufSize);
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality
        /// </summary>
        /// <param name="srcPtr">
        /// Pointer to an image buffer containing RGB, grayscale, or CMYK pixels to be compressed.  
        /// This buffer is not modified.
        /// </param>
        /// <param name="stride">
        /// Bytes per line in the source image.  
        /// Normally, this should be <c>width * BytesPerPixel</c> if the image is unpadded, 
        /// or <c>TJPAD(width * BytesPerPixel</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.  
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * BytesPerPixel</c>.
        /// </param>
        /// <param name="width">Width (in pixels) of the source image</param>
        /// <param name="height">Height (in pixels) of the source image</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats")</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOptions"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <exception cref="TJException"> Throws if compress function failed </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOptions.TJSAMP_GRAY"/> and pixel format <see cref="TJPixelFormats.TJPF_GRAY"/></description></item>
        /// </list>
        /// </exception>
        public byte[] Compress(IntPtr srcPtr, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOptions subSamp, int quality, TJFlags flags)
        {
            ulong bufSize;
            return Compress(srcPtr, stride, width, height, pixelFormat, subSamp, quality, flags, out bufSize);
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality
        /// </summary>
        /// <param name="srcBuf">
        /// Image buffer containing RGB, grayscale, or CMYK pixels to be compressed.  
        /// This buffer is not modified.
        /// </param>
        /// <param name="stride">
        /// Bytes per line in the source image.  
        /// Normally, this should be <c>width * BytesPerPixel</c> if the image is unpadded, 
        /// or <c>TJPAD(width * BytesPerPixel</c> if each line of the image
        /// is padded to the nearest 32-bit boundary, as is the case for Windows bitmaps.  
        /// You can also be clever and use this parameter to skip lines, etc.
        /// Setting this parameter to 0 is the equivalent of setting it to
        /// <c>width * BytesPerPixel</c>.
        /// </param>
        /// <param name="width">Width (in pixels) of the source image</param>
        /// <param name="height">Height (in pixels) of the source image</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats")</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOptions"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags"</param>
        /// <exception cref="TJException">
        /// Throws if compress function failed
        /// </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore</exception>
        /// <exception cref="NotSupportedException"> 
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOptions.TJSAMP_GRAY"/> and pixel format <see cref="TJPixelFormats.TJPF_GRAY"/></description></item>
        /// </list>
        /// </exception>
        public byte[] Compress(byte[] srcBuf, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOptions subSamp, int quality, TJFlags flags)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("this");

            using (var srcPtr = new TJUnmanagedMemory(srcBuf))
                return Compress((IntPtr)srcPtr, stride, width, height, pixelFormat, subSamp, quality, flags);
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
            if (_compressorHandle != IntPtr.Zero)
            {
                TurboJpegImport.tjDestroy(_compressorHandle);

                // Set the handle to IntPtr.Zero, to prevent double execution of this method
                // (i.e. make calling Dispose twice a safe thing to do).
                _compressorHandle = IntPtr.Zero;
            }
        }


        /// <summary>
        /// Finalizer
        /// </summary>
        ~TJCompressor()
        {
            Dispose(false);
        }

        /// <exception cref="NotSupportedException"> 
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOptions.TJSAMP_GRAY"/> and pixel format <see cref="TJPixelFormats.TJPF_GRAY"/></description></item>
        /// </list>
        /// </exception>
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        private static void CheckOptionsCompatibilityAndThrow(TJSubsamplingOptions subSamp, TJPixelFormats srcFormat)
        {
            if (srcFormat == TJPixelFormats.TJPF_GRAY && subSamp != TJSubsamplingOptions.TJSAMP_GRAY)
                throw new NotSupportedException(
                    $"Subsampling differ from {TJSubsamplingOptions.TJSAMP_GRAY} for pixel format {TJPixelFormats.TJPF_GRAY} is not supported");
        }
    }
}
