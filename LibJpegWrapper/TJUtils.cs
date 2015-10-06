using System;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming
    static class TJUtils
    {
        ///<summary>
        /// Retrieves last error from underlying turbo-jpeg library and throws exception</summary>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails </exception>
        public static void GetErrorAndThrow()
        {
            var error = TurboJpegImport.tjGetErrorStr();
            throw new TJException(error);
        }
        /// <summary>
        /// Converts pixel format from <see cref="PixelFormat"/> to <see cref="TJPixelFormats"/>
        /// </summary>
        /// <param name="pixelFormat">Pixel format to convert</param>
        /// <returns>Converted value of pixel format or exception if convertion is impossible</returns>
        /// <exception cref="NotSupportedException">Convertion can not be performed</exception>
        public static TJPixelFormats ConvertPixelFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return TJPixelFormats.TJPF_ARGB;
                case PixelFormat.Format24bppRgb:
                    return TJPixelFormats.TJPF_RGB;
                case PixelFormat.Format8bppIndexed:
                    return TJPixelFormats.TJPF_GRAY;
                default:
                    throw new NotSupportedException($"Provided pixel format \"{pixelFormat}\" is not supported");
            }
        }

        public static void SetUnmanagedDllPath()
        {
            var rootPath = Path.GetDirectoryName(typeof(TJUtils).Assembly.Location);
            var platform = TS.NativeTools.SystemManager.GetPlatformName();
            var dllPath = Path.Combine(rootPath, platform);

            System.Diagnostics.Trace.WriteLine($"Set libjpeg-turbo path to {dllPath}");

            TS.NativeTools.LibraryLoader.Instance.AddDllPath(dllPath);
        }
    }
}
