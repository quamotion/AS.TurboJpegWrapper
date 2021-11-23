// <copyright file="TJDrawingUtils.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Drawing.Imaging;

namespace TurboJpegWrapper
{
    internal static class TJDrawingUtils
    {
        /// <summary>
        /// Converts pixel format from <see cref="PixelFormat"/> to <see cref="TJPixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat">Pixel format to convert.</param>
        /// <returns>Converted value of pixel format or exception if convertion is impossible.</returns>
        /// <exception cref="NotSupportedException">Convertion can not be performed.</exception>
        public static TJPixelFormat ConvertPixelFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return TJPixelFormat.BGRA;
                case PixelFormat.Format24bppRgb:
                    return TJPixelFormat.BGR;
                case PixelFormat.Format8bppIndexed:
                    return TJPixelFormat.Gray;
                default:
                    throw new NotSupportedException($"Provided pixel format \"{pixelFormat}\" is not supported");
            }
        }
    }
}
