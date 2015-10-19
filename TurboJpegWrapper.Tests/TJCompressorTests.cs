using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace TurboJpegWrapper.Tests
{
    // ReSharper disable once InconsistentNaming
    [TestFixture]
    public class TJCompressorTests
    {
        private TJCompressor _compressor;

        private string OutDirectory { get { return Path.Combine(TestUtils.BinPath, "compress_images_out"); } }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _compressor = new TJCompressor();
            if (Directory.Exists(OutDirectory))
            {
                Directory.Delete(OutDirectory, true);
            }
            Directory.CreateDirectory(OutDirectory);
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            _compressor.Dispose();
        }

        [Test, Combinatorial]
        public void CompressBitmap(
            [Values
            (TJSubsamplingOptions.TJSAMP_GRAY,
            TJSubsamplingOptions.TJSAMP_411,
            TJSubsamplingOptions.TJSAMP_420,
            TJSubsamplingOptions.TJSAMP_440,
            TJSubsamplingOptions.TJSAMP_422,
            TJSubsamplingOptions.TJSAMP_444)]TJSubsamplingOptions options,
            [Values(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
        {
            var imageidx = 0;
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(bitmap, options, quality, TJFlags.NONE);

                        Assert.NotNull(result);

                        var file = Path.Combine(OutDirectory, $"{imageidx}_{quality}_{options}.jpg");
                        File.WriteAllBytes(file, result);
                    });

                }
                finally
                {
                    bitmap.Dispose();
                }
                imageidx++;
            }
        }
        [Test, Combinatorial]
        public void CompressIntPtr(
            [Values
            (TJSubsamplingOptions.TJSAMP_GRAY,
            TJSubsamplingOptions.TJSAMP_411,
            TJSubsamplingOptions.TJSAMP_420,
            TJSubsamplingOptions.TJSAMP_440,
            TJSubsamplingOptions.TJSAMP_422,
            TJSubsamplingOptions.TJSAMP_444)]TJSubsamplingOptions options,
            [Values(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
        {
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                BitmapData data = null;
                try
                {
                    data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
                        bitmap.PixelFormat);

                    Trace.WriteLine($"Options: {options}; Quality: {quality}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(data.Scan0, data.Stride, data.Width, data.Height, data.PixelFormat, options, quality, TJFlags.NONE);
                        Assert.NotNull(result);
                    });

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
        [Test, Combinatorial]
        public void CompressByteArray(
            [Values
            (TJSubsamplingOptions.TJSAMP_GRAY,
            TJSubsamplingOptions.TJSAMP_411,
            TJSubsamplingOptions.TJSAMP_420,
            TJSubsamplingOptions.TJSAMP_440,
            TJSubsamplingOptions.TJSAMP_422,
            TJSubsamplingOptions.TJSAMP_444)]TJSubsamplingOptions options,
            [Values(1, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100)]int quality)
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
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(buf, stride, width, height, pixelFormat, options, quality, TJFlags.NONE);
                        Assert.NotNull(result);
                    });

                }
                finally
                {
                    bitmap.Dispose();
                }
            }
        }
        

        
    }
}
