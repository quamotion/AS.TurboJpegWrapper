// <copyright file="TJSubsamplingOption.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

namespace TurboJpegWrapper
{
    /// <summary>
    /// Chrominance subsampling options.
    /// <para>
    /// When pixels are converted from RGB to YCbCr (see #TJCS_YCbCr) or from CMYK
    /// to YCCK (see #TJCS_YCCK) as part of the JPEG compression process, some of
    /// the Cb and Cr (chrominance) components can be discarded or averaged together
    /// to produce a smaller image with little perceptible loss of image clarity
    /// (the human eye is more sensitive to small changes in brightness than to
    /// small changes in color.)  This is called "chrominance subsampling".
    /// </para>
    /// </summary>
    public enum TJSubsamplingOption
    {
        /// <summary>
        /// 4:4:4 chrominance subsampling (no chrominance subsampling).  The JPEG or * YUV image will contain one chrominance component for every pixel in the source image.
        /// </summary>
        Chrominance444 = 0,

        /// <summary>
        /// 4:2:2 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 2x1 block of pixels in the source image.
        /// </summary>
        Chrominance422,

        /// <summary>
        /// 4:2:0 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 2x2 block of pixels in the source image.
        /// </summary>
        Chrominance420,

        /// <summary>
        /// Grayscale.  The JPEG or YUV image will contain no chrominance components.
        /// </summary>
        Gray,

        /// <summary>
        /// 4:4:0 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 1x2 block of pixels in the source image.
        /// </summary>
        /// <remarks>4:4:0 subsampling is not fully accelerated in libjpeg-turbo.</remarks>
        Chrominance440,

        /// <summary>
        /// 4:1:1 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 4x1 block of pixels in the source image.
        /// JPEG images compressed with 4:1:1 subsampling will be almost exactly the
        /// same size as those compressed with 4:2:0 subsampling, and in the
        /// aggregate, both subsampling methods produce approximately the same
        /// perceptual quality.  However, 4:1:1 is better able to reproduce sharp
        /// horizontal features.
        /// </summary>
        /// <remarks> 4:1:1 subsampling is not fully accelerated in libjpeg-turbo.</remarks>
        Chrominance411,
    }
}
