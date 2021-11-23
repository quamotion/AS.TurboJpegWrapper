// <copyright file="TJCompressorExtensions.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Drawing;
using System.Drawing.Imaging;
using Validation;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Provides extension methods which allow using <see cref="TJCompressor"/> with <see cref="Image"/> objects.
    /// </summary>
    public static class TJCompressorExtensions
    {
        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="compressor">
        /// The <see cref="TJCompressor"/> to use.
        /// </param>
        /// <param name="srcImage"> Source image to be converted. </param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// A <see cref="byte"/> array containing the compressed image.
        /// </returns>
        /// <remarks>Only <see cref="PixelFormat.Format24bppRgb"/>, <see cref="PixelFormat.Format32bppArgb"/>, <see cref="PixelFormat.Format8bppIndexed"/> pixel formats are supported.</remarks>
        /// <exception cref="TJException"> Throws if compress function failed. </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public static byte[] Compress(this TJCompressor compressor, Bitmap srcImage, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            Requires.NotNull(compressor, nameof(compressor));
            Verify.NotDisposed(compressor);

            var pixelFormat = srcImage.PixelFormat;

            var width = srcImage.Width;
            var height = srcImage.Height;
            var srcData = srcImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);

            var stride = srcData.Stride;
            var srcPtr = srcData.Scan0;

            try
            {
                return compressor.Compress(srcPtr, stride, width, height, pixelFormat, subSamp, quality, flags);
            }
            finally
            {
                srcImage.UnlockBits(srcData);
            }
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="compressor">
        /// The <see cref="TJCompressor"/> to use.
        /// </param>
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
        /// <param name="width">Width (in pixels) of the source image.</param>
        /// <param name="height">Height (in pixels) of the source image.</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats").</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// A <see cref="byte"/> array containing the compressed image.
        /// </returns>
        /// <exception cref="TJException"> Throws if compress function failed. </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public static unsafe byte[] Compress(this TJCompressor compressor, IntPtr srcPtr, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            Requires.NotNull(compressor, nameof(compressor));
            Verify.NotDisposed(compressor);

            return compressor.Compress(srcPtr, stride, width, height, TJDrawingUtils.ConvertPixelFormat(pixelFormat), subSamp, quality, flags);
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="compressor">
        /// The <see cref="TJCompressor"/> to use.
        /// </param>
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
        /// <param name="width">Width (in pixels) of the source image.</param>
        /// <param name="height">Height (in pixels) of the source image.</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats").</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// A <see cref="byte"/> array containing the compressed image.
        /// </returns>
        /// <exception cref="TJException">
        /// Throws if compress function failed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public static unsafe byte[] Compress(this TJCompressor compressor, byte[] srcBuf, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            fixed (byte* srcBufPtr = srcBuf)
            {
                return Compress(compressor, (IntPtr)srcBufPtr, stride, width, height, pixelFormat, subSamp, quality, flags);
            }
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="compressor">
        /// The <see cref="TJCompressor"/> to use.
        /// </param>
        /// <param name="srcBuf">
        /// Image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.
        /// </param>
        /// <param name="destBuf">
        /// A <see cref="byte"/> array containing the compressed image.
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
        /// <param name="width">Width (in pixels) of the source image.</param>
        /// <param name="height">Height (in pixels) of the source image.</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats").</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// A <see cref="Span{T}"/> which is a slice of <paramref name="destBuf"/> which holds the compressed image.
        /// </returns>
        /// <exception cref="TJException">
        /// Throws if compress function failed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public static unsafe Span<byte> Compress(this TJCompressor compressor, Span<byte> srcBuf, Span<byte> destBuf, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            Requires.NotNull(compressor, nameof(compressor));
            Verify.NotDisposed(compressor);

            var tjPixelFormat = TJDrawingUtils.ConvertPixelFormat(pixelFormat);

            return compressor.Compress(srcBuf, destBuf, stride, width, height, tjPixelFormat, subSamp, quality, flags);
        }

        /// <summary>
        /// Compresses input image to the jpeg format with specified quality.
        /// </summary>
        /// <param name="compressor">
        /// The <see cref="TJCompressor"/> to use.
        /// </param>
        /// <param name="srcBuf">
        /// Image buffer containing RGB, grayscale, or CMYK pixels to be compressed.
        /// This buffer is not modified.
        /// </param>
        /// <param name="destBuf">
        /// A <see cref="byte"/> array containing the compressed image.
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
        /// <param name="width">Width (in pixels) of the source image.</param>
        /// <param name="height">Height (in pixels) of the source image.</param>
        /// <param name="pixelFormat">Pixel format of the source image (see <see cref="PixelFormat"/> "Pixel formats").</param>
        /// <param name="subSamp">
        /// The level of chrominance subsampling to be used when
        /// generating the JPEG image (see <see cref="TJSubsamplingOption"/> "Chrominance subsampling options".)
        /// </param>
        /// <param name="quality">The image quality of the generated JPEG image (1 = worst, 100 = best).</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>
        /// The size of the JPEG image (in bytes).
        /// </returns>
        /// <exception cref="TJException">
        /// Throws if compress function failed.
        /// </exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">
        /// Some parameters' values are incompatible:
        /// <list type="bullet">
        /// <item><description>Subsampling not equals to <see cref="TJSubsamplingOption.Gray"/> and pixel format <see cref="TJPixelFormat.Gray"/></description></item>
        /// </list>
        /// </exception>
        public static unsafe int Compress(this TJCompressor compressor, byte[] srcBuf, byte[] destBuf, int stride, int width, int height, PixelFormat pixelFormat, TJSubsamplingOption subSamp, int quality, TJFlags flags)
        {
            return Compress(compressor, srcBuf.AsSpan(), destBuf.AsSpan(), stride, width, height, pixelFormat, subSamp, quality, flags).Length;
        }
    }
}
