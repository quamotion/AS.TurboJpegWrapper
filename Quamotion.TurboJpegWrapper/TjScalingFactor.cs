// <copyright file="TjScalingFactor.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Scaling factor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct TjScalingFactor
    {
        /// <summary>
        /// Gets or sets the numerator.
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// Gets or sets the denominator.
        /// </summary>
        public int Denom { get; set; }
    }
}
