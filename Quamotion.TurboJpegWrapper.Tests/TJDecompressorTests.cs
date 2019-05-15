using System;
using System.Drawing.Imaging;
using System.IO;
using Xunit;

namespace TurboJpegWrapper.Tests
{
    public class TJDecompressorTests : IDisposable
    {
        private TJDecompressor _decompressor;
        private string OutDirectory { get { return Path.Combine(TestUtils.BinPath, "decompress_images_out"); } }

        public TJDecompressorTests()
        {
            _decompressor = new TJDecompressor();
            if (Directory.Exists(OutDirectory))
            {
                Directory.Delete(OutDirectory, true);
            }
            Directory.CreateDirectory(OutDirectory);
        }

        public void Dispose()
        {
            _decompressor.Dispose();
        }

        [Theory, CombinatorialData]
        public void DecompressByteArray(
            [CombinatorialValues(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var result = _decompressor.Decompress(data.Item2, format, TJFlags.NONE);
                Assert.NotNull(result);

                var file = Path.Combine(OutDirectory, $"{Path.GetFileNameWithoutExtension(data.Item1)}_{format}.bmp");
                result.Save(file);
            }
        }

        [Theory, CombinatorialData]
        public void DecompressIntPtr(
           [CombinatorialValues(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var dataPtr = TJUtils.CopyDataToPointer(data.Item2);
                var result = _decompressor.Decompress(dataPtr, (ulong)data.Item2.Length, format, TJFlags.NONE);
                Assert.NotNull(result);
                TJUtils.FreePtr(dataPtr);
            }
        }
    }
}
