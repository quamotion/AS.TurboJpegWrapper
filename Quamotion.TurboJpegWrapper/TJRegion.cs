using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// Cropping region.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TJRegion
    {
        /// <summary>
        /// The left boundary of the cropping region.
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// The upper boundary of the cropping region.</summary>
        public int Y { get; set; }

        /// <summary>
        /// The width of the cropping region. Setting this to 0 is the equivalent of
        /// setting it to the width of the source JPEG image - x.
        /// </summary>
        public int W { get; set; }

        /// <summary>
        /// The height of the cropping region. Setting this to 0 is the equivalent of
        /// setting it to the height of the source JPEG image - y.
        /// </summary>
        public int H { get; set; }

        /// <summary>
        /// Returns empty region which is interpreted as full image region.
        /// </summary>
        public static TJRegion Empty { get { return new TJRegion();} }
    }
}