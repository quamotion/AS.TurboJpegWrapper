using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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
            var imageidx = 0;
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                Assert.DoesNotThrow(() =>
                {
                    var result = _decompressor.Decompress(data, format, TJFlags.NONE);
                    Assert.NotNull(result);
                    
                    var file = Path.Combine(OutDirectory, $"{imageidx}_{format}.bmp");
                    result.Save(file);
                });
                imageidx++;
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
                var dataPtr = TS.NativeTools.InteropUtils.CopyDataToPointer(data);
                Assert.DoesNotThrow(() =>
                {
                    var result = _decompressor.Decompress(dataPtr, (ulong)data.Length, format, TJFlags.NONE);
                    Assert.NotNull(result);
                });
                TS.NativeTools.InteropUtils.FreePtr(dataPtr);
            }
        }
    }
}
