using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace TurboJpegWrapper.Tests
{
    // ReSharper disable once InconsistentNaming
    [TestFixture]
    public class TJTransformerTests
    {
        private TJTransformer _transformer;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _transformer = new TJTransformer();
        }

        [TestFixtureTearDown]
        public void Clean()
        {
            _transformer.Dispose();
        }

        [Test]
        public void TransformFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                Assert.DoesNotThrow(() =>
                {
                    var transforms = new[]
                    {
                        new TJTransformDescription
                        {
                            Operation = TJTransformOperations.TJXOP_NONE,
                            Options = TJTransformOptions.GRAY,
                            Region = TJRegion.Empty
                        }
                    };
                    var result = _transformer.Transform(data, transforms, TJFlags.BOTTOMUP);
                    Assert.NotNull(result);
                });
            }
        }
    }
}
