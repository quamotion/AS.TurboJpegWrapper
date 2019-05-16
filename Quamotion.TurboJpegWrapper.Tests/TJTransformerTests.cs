// <copyright file="TJTransformerTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.IO;
using Xunit;

namespace TurboJpegWrapper.Tests
{
    public class TJTransformerTests : IDisposable
    {
        private TJTransformer transformer;

        public TJTransformerTests()
        {
            this.transformer = new TJTransformer();
            if (Directory.Exists(this.OutDirectory))
            {
                Directory.Delete(this.OutDirectory, true);
            }

            Directory.CreateDirectory(this.OutDirectory);
        }

        private string OutDirectory
        {
            get { return Path.Combine(TestUtils.BinPath, "transform_images_out"); }
        }

        public void Dispose()
        {
            this.transformer.Dispose();
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
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Gray,
                        Region = TJRegion.Empty,
                    },
                };
                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);

                Assert.NotNull(result);
                Assert.Single(result);

                var file = Path.Combine(this.OutDirectory, "gray_" + Path.GetFileName(data.Item1));
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
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 0,
                            Y = 0,
                            W = 50,
                            H = 50,
                        },
                    },
                };

                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(this.OutDirectory, $"crop_s_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
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
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 0,
                            Y = 0,
                            W = 100,
                            H = 100,
                        },
                    },
                    new TJTransformDescription
                    {
                        Operation = TJTransformOperation.None,
                        Options = TJTransformOptions.Crop,
                        Region = new TJRegion
                        {
                            X = 50,
                            Y = 0,
                            W = 100,
                            H = 100,
                        },
                    },
                };

                var result = this.transformer.Transform(data.Item2, transforms, TJFlags.None);
                Assert.NotNull(result);
                Assert.NotEmpty(result);

                for (var idx = 0; idx < result.Length; idx++)
                {
                    var file = Path.Combine(this.OutDirectory, $"crop_m_{Path.GetFileNameWithoutExtension(data.Item1)}_{idx}.jpg");
                    File.WriteAllBytes(file, result[idx]);
                }
            }
        }
    }
}
