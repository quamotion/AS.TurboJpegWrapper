using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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

        [TestFixtureSetUp]
        public void SetUp()
        {
            _decompressor = new TJDecompressor();
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
                    var result = _decompressor.Decompress(data, format, TJFlags.BOTTOMUP);
                    Assert.NotNull(result);
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
                var dataPtr = TS.NativeTools.InteropUtils.CopyDataToPointer(data);
                Assert.DoesNotThrow(() =>
                {
                    var result = _decompressor.Decompress(dataPtr, (ulong)data.Length, format, TJFlags.BOTTOMUP);
                    Assert.NotNull(result);
                });
                TS.NativeTools.InteropUtils.FreePtr(dataPtr);
            }
        }
    }
}
