// <copyright file="TJUtils.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace TurboJpegWrapper
{
    internal static class TJUtils
    {
        ///<summary>
        /// Retrieves last error from underlying turbo-jpeg library and throws exception.</summary>
        /// <exception cref="TJException"> Throws if low level turbo jpeg function fails. </exception>
        public static void GetErrorAndThrow()
        {
            var error = Marshal.PtrToStringAnsi(TurboJpegImport.TjGetErrorStr());
            throw new TJException(error);
        }

        /// <summary>
        /// Converts pixel format from <see cref="PixelFormat"/> to <see cref="TJPixelFormat"/>.
        /// </summary>
        /// <param name="pixelFormat">Pixel format to convert.</param>
        /// <returns>Converted value of pixel format or exception if convertion is impossible.</returns>
        /// <exception cref="NotSupportedException">Convertion can not be performed.</exception>
        public static TJPixelFormat ConvertPixelFormat(PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format32bppArgb:
                    return TJPixelFormat.BGRA;
                case PixelFormat.Format24bppRgb:
                    return TJPixelFormat.BGR;
                case PixelFormat.Format8bppIndexed:
                    return TJPixelFormat.Gray;
                default:
                    throw new NotSupportedException($"Provided pixel format \"{pixelFormat}\" is not supported");
            }
        }

        /// <summary>
        /// Returns actual platform name depending on pointer size.
        /// </summary>
        /// <returns>"x86" for 32 bit processes and "x64" for 64 bit processes.</returns>
        public static string GetPlatformName()
        {
            return IntPtr.Size == sizeof(int) ? "x86" : "x64";
        }

        /// <summary>
        /// Converts array of managed structures to the unmanaged pointer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structArray"></param>
        /// <returns></returns>
        public static IntPtr StructArrayToIntPtr<T>(T[] structArray)
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var result = Marshal.AllocHGlobal(structArray.Length * structSize);
            var longPtr = result.ToInt64(); // Must work both on x86 and x64
            foreach (var s in structArray)
            {
                var structPtr = new IntPtr(longPtr);
                Marshal.StructureToPtr(s, structPtr, false); // You do not need to erase struct in this case
                longPtr += structSize;
            }

            return result;
        }

        /// <summary>
        /// Copies data from array to unmanaged pointer.
        /// </summary>
        /// <param name="data">Byte array for copy.</param>
        /// <param name="useComAllocation">If set to <c>true</c>, Com allocator will be used to allocate memory.</param>
        /// <returns></returns>
        public static IntPtr CopyDataToPointer(byte[] data, bool useComAllocation = false)
        {
            var res = useComAllocation ? Marshal.AllocCoTaskMem(data.Length) : Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, res, data.Length);
            return res;
        }

        /// <summary>
        /// Allocate an image buffer for use with TurboJPEG.
        /// </summary>
        /// <param name="bytes">The number of bytes to allocate.</param>
        /// <returns>A pointer to a newly-allocated buffer with the specified number of bytes.</returns>
        /// <seealso cref="Free"/>
        public static IntPtr Alloc(int bytes) => TurboJpegImport.TjAlloc(bytes);

        /// <summary>
        /// Free an image buffer previously allocated by TurboJPEG.
        /// </summary>
        /// <param name="buffer">Address of the buffer to free.</param>
        /// <seealso cref="Alloc"/>
        public static void Free(IntPtr buffer) => TurboJpegImport.TjFree(buffer);

        /// <summary>
        /// Frees unmanaged pointer using allocator.
        /// </summary>
        /// <param name="ptr"></param>
        /// <param name="isComAllocated">If set to <c>true</c>, Com allocator will be used to free memory.</param>
        public static void FreePtr(IntPtr ptr, bool isComAllocated = false)
        {
            if (ptr == IntPtr.Zero)
            {
                return;
            }

            if (isComAllocated)
            {
                Marshal.FreeCoTaskMem(ptr);
            }
            else
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
    }
}
