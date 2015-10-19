using System.Drawing.Imaging;
using System.IO;
using NUnit.Framework;
using TS.NativeTools;

namespace TurboJpegWrapper.Tests
{
    [TestFixture]
    class TJDecompressorTests
    {
        private TJDecompressor _decompressor;
        private string OutDirectory { get { return Path.Combine(TestUtils.BinPath, "decompress_images_out"); } }

        [TestFixtureSetUp]
        public void SetUp()
        {
            _decompressor = new TJDecompressor();
            if (Directory.Exists(OutDirectory))
            {
                Directory.Delete(OutDirectory, true);
            }
            Directory.CreateDirectory(OutDirectory);
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            _decompressor.Dispose();
        }

        [Test, Combinatorial]
        public void DecompressByteArray(
            [Values(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                Assert.DoesNotThrow(() =>
                {
                    var result = _decompressor.Decompress(data.Item2, format, TJFlags.NONE);
                    Assert.NotNull(result);

                    var file = Path.Combine(OutDirectory, $"{Path.GetFileNameWithoutExtension(data.Item1)}_{format}.bmp");
                    result.Save(file);
                });
            }
        }

        [Test, Combinatorial]
        public void DecompressIntPtr(
           [Values(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var dataPtr = InteropUtils.CopyDataToPointer(data.Item2);
                Assert.DoesNotThrow(() =>
                {
                    var result = _decompressor.Decompress(dataPtr, (ulong)data.Item2.Length, format, TJFlags.NONE);
                    Assert.NotNull(result);
                });
                InteropUtils.FreePtr(dataPtr);
            }
        }
    }
}
