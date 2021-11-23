// <copyright file="TJDecompressorExtensions.cs" company="Autonomic Systems, Quamotion">
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
    public static class TJDecompressorExtensions
    {
        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="decompressor">
        /// The decompressor to use.
        /// </param>
        /// <param name="jpegBuf">Pointer to a buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="jpegBufSize">Size of the JPEG image (in bytes).</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Decompressed image of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed.</exception>
        public static unsafe Bitmap Decompress(this TJDecompressor decompressor, IntPtr jpegBuf, ulong jpegBufSize, PixelFormat destPixelFormat, TJFlags flags)
        {
            Requires.NotNull(decompressor, nameof(decompressor));
            Verify.NotDisposed(decompressor);

            var targetFormat = TJDrawingUtils.ConvertPixelFormat(destPixelFormat);
            int width;
            int height;
            int stride;
            var buffer = decompressor.Decompress(jpegBuf, jpegBufSize, targetFormat, flags, out width, out height, out stride);
            Bitmap result;
            fixed (byte* bufferPtr = buffer)
            {
                result = new Bitmap(width, height, stride, destPixelFormat, (IntPtr)bufferPtr);
                if (destPixelFormat == PixelFormat.Format8bppIndexed)
                {
                    result.Palette = FixPaletteToGrayscale(result.Palette);
                }
            }

            return result;
        }

        /// <summary>
        /// Decompress a JPEG image to an RGB, grayscale, or CMYK image.
        /// </summary>
        /// <param name="decompressor">
        /// The decompressor to use.
        /// </param>
        /// <param name="jpegBuf">A buffer containing the JPEG image to decompress. This buffer is not modified.</param>
        /// <param name="destPixelFormat">Pixel format of the destination image (see <see cref="PixelFormat"/> "Pixel formats".)</param>
        /// <param name="flags">The bitwise OR of one or more of the <see cref="TJFlags"/> "flags".</param>
        /// <returns>Decompressed image of specified format.</returns>
        /// <exception cref="TJException">Throws if underlying decompress function failed.</exception>
        /// <exception cref="ObjectDisposedException">Object is disposed and can not be used anymore.</exception>
        /// <exception cref="NotSupportedException">Convertion to the requested pixel format can not be performed.</exception>
        public static unsafe Bitmap Decompress(this TJDecompressor decompressor, byte[] jpegBuf, PixelFormat destPixelFormat, TJFlags flags)
        {
            Requires.NotNull(decompressor, nameof(decompressor));
            Verify.NotDisposed(decompressor);

            var jpegBufSize = (ulong)jpegBuf.Length;
            fixed (byte* jpegPtr = jpegBuf)
            {
                return decompressor.Decompress((IntPtr)jpegPtr, jpegBufSize, destPixelFormat, flags);
            }
        }

        private static ColorPalette FixPaletteToGrayscale(ColorPalette palette)
        {
            for (var index = 0; index < palette.Entries.Length; ++index)
            {
                palette.Entries[index] = Color.FromArgb(index, index, index);
            }

            return palette;
        }
    }
}
