// <copyright file="TJRegion.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Cropping region.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TJRegion
    {
        /// <summary>
        /// Gets an empty region which is interpreted as full image region.
        /// </summary>
        public static TJRegion Empty
        {
            get
            {
                return default(TJRegion);
            }
        }

        /// <summary>
        /// Gets or sets the left boundary of the cropping region.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Gets or sets The upper boundary of the cropping region.
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the cropping region. Setting this to 0 is the equivalent of
        /// setting it to the width of the source JPEG image - x.
        /// </summary>
        public int W { get; set; }

        /// <summary>
        /// Gets or sets the height of the cropping region. Setting this to 0 is the equivalent of
        /// setting it to the height of the source JPEG image - y.
        /// </summary>
        public int H { get; set; }
    }
}