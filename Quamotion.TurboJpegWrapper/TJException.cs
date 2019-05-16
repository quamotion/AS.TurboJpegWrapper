using System;

namespace TurboJpegWrapper
{
    // ReSharper disable once InconsistentNaming

    /// <summary>
    /// Exception thrown then internal error in the underlying turbo jpeg library is occured.
    /// </summary>
    public class TJException : Exception
    {
        /// <summary>
        /// Creates new instance of <see cref="TJException"/>.
        /// </summary>
        /// <param name="error">Error message from underlying turbo jpeg library.</param>
        public TJException(string error) : base(error)
        {
        }
    }
}