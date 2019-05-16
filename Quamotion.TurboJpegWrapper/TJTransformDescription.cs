// <copyright file="TJTransformDescription.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Structure describing transformation of source image.
    /// </summary>
    public struct TJTransformDescription
    {
        /// <summary>
        /// Gets or sets the transform operation.
        /// </summary>
        public TJTransformOperation Operation { get; set; }

        /// <summary>
        /// Gets or sets the transform options.
        /// </summary>
        public TJTransformOptions Options { get; set; }

        /// <summary>
        /// Gets or sets the region to transform.
        /// </summary>
        public TJRegion Region { get; set; }

        /// <summary>
        /// Gets or sets callback data.
        /// </summary>
        public IntPtr CallbackData { get; set; }

        /// <summary>
        /// Gets or sets the custom filter delegate.
        /// </summary>
        public CustomFilter CustomFilter { get; set; }
    }
}
