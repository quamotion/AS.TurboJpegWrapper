// <copyright file="TJException.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;

namespace TurboJpegWrapper
{
    /// <summary>
    /// Exception thrown then internal error in the underlying turbo jpeg library is occured.
    /// </summary>
    public class TJException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TJException"/> class.
        /// </summary>
        /// <param name="error">Error message from underlying turbo jpeg library.</param>
        public TJException(string error)
            : base(error)
        {
        }
    }
}