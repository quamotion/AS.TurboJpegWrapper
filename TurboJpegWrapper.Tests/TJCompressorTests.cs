using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace TurboJpegWrapper.Tests
{
    // ReSharper disable once InconsistentNaming
    [TestFixture]
    public class TJCompressorTests
    {
        private TJCompressor _compressor;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _compressor = new TJCompressor();
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
            foreach (var bitmap in TestUtils.GetTestImages("*.bmp"))
            {
                try
                {
                    Debug.WriteLine($"Options: {options}; Quality: {quality}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(bitmap, options, quality, TJFlags.BOTTOMUP);
                        Assert.NotNull(result);
                    });

                }
                finally
                {
                    bitmap.Dispose();
                }
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

                    Debug.WriteLine($"Options: {options}; Quality: {quality}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(data.Scan0, data.Stride, data.Width, data.Height, data.PixelFormat, options, quality, TJFlags.BOTTOMUP);
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

                    Debug.WriteLine($"Options: {options}; Quality: {quality}");
                    Assert.DoesNotThrow(() =>
                    {
                        var result = _compressor.Compress(buf, stride, width, height, pixelFormat, options, quality, TJFlags.BOTTOMUP);
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
