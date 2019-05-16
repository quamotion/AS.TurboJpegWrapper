using System;
using System.IO;
using Xunit;

namespace TurboJpegWrapper.Tests
{
    public class TJTransformerTests : IDisposable
    {
        private TJTransformer _transformer;
        private string OutDirectory { get { return Path.Combine(TestUtils.BinPath, "transform_images_out"); } }

        public TJTransformerTests()
        {
            _transformer = new TJTransformer();
            if (Directory.Exists(OutDirectory))
            {
                Directory.Delete(OutDirectory, true);
            }
            Directory.CreateDirectory(OutDirectory);
        }

        public void Dispose()
        {
            _transformer.Dispose();
        }

        [Fact]
        public void TransformToGrayscaleFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
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
                var result = _transformer.Transform(data.Item2, transforms, TJFlags.NONE);

                Assert.NotNull(result);
                Assert.Single(result);

                var file = Path.Combine(OutDirectory, "gray_" + Path.GetFileName(data.Item1));
                File.WriteAllBytes(file, result[0]);
            }
        }

        [Fact]
        public void TransformToCroppedSingleImageFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var transforms = new[]
                {
                        new TJTransformDescription
                        {
                            Operation = TJTransformOperations.TJXOP_NONE,
                            Options = TJTransformOptions.CROP,
                            Region = new TJRegion
                            {
                                X = 0,
                                Y = 0,
                                W = 50,
                                H = 50
                            }
                        },
                        //new TJTransformDescription
                        //{
                        //    Operation = TJTransformOperations.TJXOP_NONE,
                        //    Options = TJTransformOptions.CROP,
                        //    Region = new TJRegion
                        //    {
                        //        X = 50,
                        //        Y = 50,
                        //        W = 50,
                        //        H = 50
                        //    }
                        //}
                    };

                var result = _transformer.Transform(data.Item2, transforms, TJFlags.NONE);
                Assert.NotNull(result);
                Assert.NotNull(result.Length == 1);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(OutDirectory, $"crop_s_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
                    File.WriteAllBytes(file, result[0]);
                }
            }
        }

        [Fact]
        public void TransformToCroppedMultiplyImagesFromArray()
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var transforms = new[]
                {
                        new TJTransformDescription
                        {
                            Operation = TJTransformOperations.TJXOP_NONE,
                            Options = TJTransformOptions.CROP,
                            Region = new TJRegion
                            {
                                X = 0,
                                Y = 0,
                                W = 100,
                                H = 100
                            },
                        },
                        new TJTransformDescription
                        {
                            Operation = TJTransformOperations.TJXOP_NONE,
                            Options = TJTransformOptions.CROP,
                            Region = new TJRegion
                            {
                                X = 50,
                                Y = 0,
                                W = 100,
                                H = 100
                            }
                        },
                    };

                var result = _transformer.Transform(data.Item2, transforms, TJFlags.NONE);
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(OutDirectory, $"crop_m_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
                    File.WriteAllBytes(file, result[idx]);
                }
            }
        }
    }
}
