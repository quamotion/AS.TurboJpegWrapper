// <copyright file="TJTransformOptions.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Transformation options.
    /// </summary>
    [Flags]
    public enum TJTransformOptions
    {
        /// <summary>
        /// This option will cause <see cref="TurboJpegImport.TjTransform"/> to return an error if the transform is
        /// not perfect.  Lossless transforms operate on MCU blocks, whose size depends
        /// on the level of chrominance subsampling used
        /// If the image's width or height is not evenly divisible
        /// by the MCU block size, then there will be partial MCU blocks on the right
        /// and/or bottom edges.  It is not possible to move these partial MCU blocks to
        /// the top or left of the image, so any transform that would require that is
        /// "imperfect."  If this option is not specified, then any partial MCU blocks
        /// that cannot be transformed will be left in place, which will create
        /// odd-looking strips on the right or bottom edge of the image.
        /// </summary>
        Perfect = 1,

        /// <summary>
        /// This option will cause <see cref="TurboJpegImport.TjTransform"/> to discard any partial MCU blocks that cannot be transformed.
        /// </summary>
        Trim = 2,

        /// <summary>
        /// This option will enable lossless cropping.  See <see cref="TurboJpegImport.TjTransform"/> for more information.
        /// </summary>
        Crop = 4,

        /// <summary>
        /// This option will discard the color data in the input image and produce a grayscale output image.
        /// </summary>
        Gray = 8,

        /// <summary>
        /// This option will prevent <see cref="TurboJpegImport.TjTransform"/> from outputting a JPEG image for
        /// this particular transform (this can be used in conjunction with a custom
        /// filter to capture the transformed DCT coefficients without transcoding them.)
        /// </summary>
        NoOutput = 16,
    }
}
