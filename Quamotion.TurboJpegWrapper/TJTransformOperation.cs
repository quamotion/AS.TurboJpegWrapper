// <copyright file="TJTransformOperation.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

namespace TurboJpegWrapper
{
    /// <summary>
    /// Transform operations for <see cref="TurboJpegImport.TjTransform"/>.
    /// </summary>
    public enum TJTransformOperation
    {
        /// <summary>
        ///  Do not transform the position of the image pixels
        /// </summary>
        None = 0,

        /// <summary>
        /// Flip (mirror) image horizontally.  This transform is imperfect if there
        /// are any partial MCU blocks on the right edge (see <see cref="TJTransformOptions.Perfect"/>.)</summary>
        HFlip,

        /// <summary>
        /// Flip (mirror) image vertically.  This transform is imperfect if there are
        /// any partial MCU blocks on the bottom edge (see <see cref="TJTransformOptions.Perfect"/>.)
        /// </summary>
        VFlip,

        /// <summary>
        /// Transpose image (flip/mirror along upper left to lower right axis.)  This
        /// transform is always perfect.
        /// </summary>
        Transpose,

        /// <summary>
        /// Transverse transpose image (flip/mirror along upper right to lower left
        /// axis.)  This transform is imperfect if there are any partial MCU blocks in
        /// the image (see <see cref="TJTransformOptions.Perfect"/>.)
        /// </summary>
        Transverse,

        /// <summary>
        /// Rotate image clockwise by 90 degrees.  This transform is imperfect if
        /// there are any partial MCU blocks on the bottom edge (<see cref="TJTransformOptions.Perfect"/>.)
        /// </summary>
        Rot90,

        /// <summary>
        /// Rotate image 180 degrees.  This transform is imperfect if there are any
        /// partial MCU blocks in the image (see <see cref="TJTransformOptions.Perfect"/>.)
        /// </summary>
        Rot180,

        /// <summary>
        /// Rotate image counter-clockwise by 90 degrees.  This transform is imperfect
        /// if there are any partial MCU blocks on the right edge (see <see cref="TJTransformOptions.Perfect"/>.)
        /// </summary>
        Rot270,
    }
}
