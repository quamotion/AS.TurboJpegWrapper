using System;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// Structure describing transformation of source image.
    /// </summary>
    public struct TJTransformDescription
    {
        /// <summary>
        /// Transform operation.
        /// </summary>
        public TJTransformOperations Operation { get; set; }

        /// <summary>
        /// Transform options.
        /// </summary>
        public TJTransformOptions Options { get; set; }

        /// <summary>
        /// Transform region.
        /// </summary>
        public TJRegion Region { get; set; }

        /// <summary>
        /// Callback data.
        /// </summary>
        public IntPtr CallbackData { get; set; }

        /// <summary>
        /// Custom filter delegate.
        /// </summary>
        public CustomFilter CustomFilter { get; set; }
    }
}
