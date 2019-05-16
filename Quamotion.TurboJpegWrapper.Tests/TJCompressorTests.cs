using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Xunit;

namespace TurboJpegWrapper.Tests
{
    public class TJCompressorTests : IDisposable
    {
        private TJCompressor _compressor;

        private string OutDirectory { get { return Path.Combine(TestUtils.BinPath, "compress_images_out"); } }

        public TJCompressorTests()
        {
            _compressor = new TJCompressor();
            if (Directory.Exists(OutDirectory))
            {
                Directory.Delete(OutDirectory, true);
            }
            Directory.CreateDirectory(OutDirectory);
        }

        public void Dispose()
        {
            _compressor.Dispose();
        }

        [Theory, CombinatorialData]
        public void CompressBitmap(
            [CombinatorialValues
            (TJSubsamplingOption.Gray,
            TJSubsamplingOption.Chrominance411,
            TJSubsamplingOption.Chrominance420,
            TJSubsamplingOption.Chrominance440,
            TJSubsamplingOption.Chrominance422,
            TJSubsamplingOption.Chrominance444)]TJSubsamplingOption options,
            [CombinatorialValues(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
        {
            var imageidx = 0;
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    Trace.WriteLine($"Options: {options}; Quality: {quality}");

                    var result = _compressor.Compress(bitmap, options, quality, TJFlags.None);

                    Assert.NotNull(result);

                    var file = Path.Combine(OutDirectory, $"{imageidx}_{quality}_{options}.jpg");
                    File.WriteAllBytes(file, result);
                }
                finally
                {
                    bitmap.Dispose();
                }
                imageidx++;
            }
        }

        [Theory, CombinatorialData]
        public void CompressIntPtr(
            [CombinatorialValues
            (TJSubsamplingOption.Gray,
            TJSubsamplingOption.Chrominance411,
            TJSubsamplingOption.Chrominance420,
            TJSubsamplingOption.Chrominance440,
            TJSubsamplingOption.Chrominance422,
            TJSubsamplingOption.Chrominance444)]TJSubsamplingOption options,
            [CombinatorialValues(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
        {
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                BitmapData data = null;
                try
                {
                    data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    var result = _compressor.Compress(data.Scan0, data.Stride, data.Width, data.Height, data.PixelFormat, options, quality, TJFlags.None);
                    Assert.NotNull(result);

                }
                finally
                {
                    if (data != null)
                    {
                        bitmap.UnlockBits(data);
                    }
                    bitmap.Dispose();
                }
            }
        }

        [Theory, CombinatorialData]
        public void CompressByteArray(
            [CombinatorialValues
            (TJSubsamplingOption.Gray,
            TJSubsamplingOption.Chrominance411,
            TJSubsamplingOption.Chrominance420,
            TJSubsamplingOption.Chrominance440,
            TJSubsamplingOption.Chrominance422,
            TJSubsamplingOption.Chrominance444)]TJSubsamplingOption options,
            [CombinatorialValues(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
        {
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    var stride = data.Stride;
                    var width = data.Width;
                    var height = data.Height;
                    var pixelFormat = data.PixelFormat;


                    var buf = new byte[stride * height];
                    Marshal.Copy(data.Scan0, buf, 0, buf.Length);
                    bitmap.UnlockBits(data);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    var result = _compressor.Compress(buf, stride, width, height, pixelFormat, options, quality, TJFlags.None);
                    Assert.NotNull(result);
                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }
    }
}
