// <copyright file="TjTransform.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Lossless transform.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TjTransform
    {
        /// <summary>
        /// Gets or sets the cropping region.
        /// </summary>
        public TJRegion R { get; set; }

        /// <summary>
        /// Gets or sets one of the <see cref="TJTransformOperations"/> "transform operations".
        /// </summary>
        public int Op { get; set; }

        /// <summary>
        /// Gets or sets the bitwise OR of one of more of the <see cref="TJTransformOptions"/> "transform options".
        /// </summary>
        public int Options { get; set; }

        /// <summary>
        /// Gets or sets arbitrary data that can be accessed within the body of the callback function.
        /// </summary>
        public IntPtr Data { get; set; }

        /// <summary>
        /// Gets or sets a callback function that can be used to modify the DCT coefficients
        /// after they are losslessly transformed but before they are transcoded to a
        /// new JPEG image.  This allows for custom filters or other transformations
        /// to be applied in the frequency domain.
        /// </summary>
        public CustomFilter CustomFilter { get; set; }
    }
}
