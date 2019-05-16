// <copyright file="Delegates.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    /// <summary>
    /// A callback function that can be used to modify the DCT coefficients
    /// after they are losslessly transformed but before they are transcoded to a
    /// new JPEG image.  This allows for custom filters or other transformations
    /// to be applied in the frequency domain.
    /// </summary>
    /// <param name="coeffs">
    /// Pointer to an array of transformed DCT coefficients.  (NOTE
    /// this pointer is not guaranteed to be valid once the callback returns, so
    /// applications wishing to hand off the DCT coefficients to another function
    /// or library should make a copy of them within the body of the callback.)
    /// </param>
    /// <param name="arrayRegion">
    /// <see cref="TJRegion"/> structure containing the width and height of
    /// the array pointed to by <paramref name="coeffs"/> as well as its offset relative to
    /// the component plane.  TurboJPEG implementations may choose to split each
    /// component plane into multiple DCT coefficient arrays and call the callback
    /// function once for each array.
    /// </param>
    /// <param name="planeRegion">
    /// <see cref="TJRegion"/> structure containing the width and height of
    /// the component plane to which <paramref name="coeffs"/> belongs.
    /// </param>
    /// <param name="componentIndex">
    /// ID number of the component plane to which
    /// <paramref name="coeffs"/> belongs (Y, Cb, and Cr have, respectively, ID's of 0, 1,
    /// and 2 in typical JPEG images.)
    /// </param>
    /// <param name="transformIndex">
    /// ID number of the transformed image to which
    /// <paramref name="coeffs"/> belongs.  This is the same as the index of the transform
    /// in the "transforms" array that was passed to <see cref="TurboJpegImport.TjTransform"/>.
    /// </param>
    /// <param name="transform">
    /// A pointer to a <see cref="TjTransform"/> structure that specifies the
    /// parameters and/or cropping region for this transform.
    /// </param>
    /// <returns>0 if the callback was successful, or -1 if an error occurred.</returns>
    /// <remarks>
    /// Original signature is:
    /// <para><c>int customFilter(short *coeffs, tjregion arrayRegion, tjregion planeRegion, int componentIndex, int transformIndex, struct tjtransform * transform)</c>.</para>
    /// </remarks>
    public delegate int CustomFilter(IntPtr coeffs, IntPtr arrayRegion, IntPtr planeRegion, int componentIndex, int transformIndex, IntPtr transform);
}