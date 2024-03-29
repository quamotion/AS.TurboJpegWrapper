﻿// <copyright file="TJDecompressorTests.cs" company="Autonomic Systems, Quamotion">
// Copyright (c) Autonomic Systems. All rights reserved.
// Copyright (c) Quamotion. All rights reserved.
// </copyright>

using System;
using System.Buffers;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Xunit;

namespace TurboJpegWrapper.Tests
{
    public class TJDecompressorTests : IDisposable
    {
        private TJDecompressor decompressor;

        public TJDecompressorTests()
        {
            this.decompressor = new TJDecompressor();
            if (Directory.Exists(this.OutDirectory))
            {
                Directory.Delete(this.OutDirectory, true);
            }

            Directory.CreateDirectory(this.OutDirectory);
        }

        private string OutDirectory
        {
            get { return Path.Combine(TestUtils.BinPath, "decompress_images_out"); }
        }

        public void Dispose()
        {
            this.decompressor.Dispose();
        }

        [Theory]
        [CombinatorialData]
        public void DecompressByteArray(
            [CombinatorialValues(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]
            PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var result = this.decompressor.Decompress(data.Item2, format, TJFlags.None);
                Assert.NotNull(result);

                var file = Path.Combine(this.OutDirectory, $"{Path.GetFileNameWithoutExtension(data.Item1)}_{format}.bmp");
                result.Save(file);
            }
        }

        [Theory]
        [CombinatorialData]
        public void DecompressIntPtr(
            [CombinatorialValues(
            PixelFormat.Format32bppArgb,
            PixelFormat.Format24bppRgb,
            PixelFormat.Format8bppIndexed)]
            PixelFormat format)
        {
            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var dataPtr = TJUtils.CopyDataToPointer(data.Item2);
                var result = this.decompressor.Decompress(dataPtr, (ulong)data.Item2.Length, format, TJFlags.None);
                Assert.NotNull(result);
                TJUtils.FreePtr(dataPtr);
            }
        }

        [Theory]
        [CombinatorialData]
        public void DecompressSpan(
            [CombinatorialValues(
            TJPixelFormat.ABGR,
            TJPixelFormat.RGB,
            TJPixelFormat.Gray)]
            TJPixelFormat format)
        {
            byte[] outBuf = ArrayPool<byte>.Shared.Rent(250 * 250 * 4);

            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                var dataSpan = data.Item2.AsSpan();
                this.decompressor.Decompress(dataSpan, outBuf.AsSpan(), format, TJFlags.None, out int width, out int height, out int stride);
            }

            ArrayPool<byte>.Shared.Return(outBuf);
        }

        [Theory]
        [CombinatorialData]
        public void DecompressArray(
            [CombinatorialValues(
            TJPixelFormat.ABGR,
            TJPixelFormat.RGB,
            TJPixelFormat.Gray)]
            TJPixelFormat format)
        {
            byte[] outBuf = ArrayPool<byte>.Shared.Rent(250 * 250 * 4);

            foreach (var data in TestUtils.GetTestImagesData("*.jpg"))
            {
                this.decompressor.Decompress(data.Item2, outBuf, format, TJFlags.None, out int width, out int height, out int stride);
            }

            ArrayPool<byte>.Shared.Return(outBuf);
        }

        [Fact]
        public void GetImageInfoPointer()
        {
            var data = TestUtils.GetTestImagesData("testorig.jpg").Single();

            var handle = GCHandle.Alloc(data.Item2, GCHandleType.Pinned);
            this.decompressor.GetImageInfo(handle.AddrOfPinnedObject(), (ulong)data.Item2.Length, TJPixelFormat.RGB, out int width, out int height, out int stride, out int bufSize);
            handle.Free();

            Assert.Equal(227, width);
            Assert.Equal(149, height);
            Assert.Equal(684, stride);
            Assert.Equal(101916, bufSize);
        }

        [Fact]
        public void GetImageInfoSpan()
        {
            var data = TestUtils.GetTestImagesData("testorig.jpg").Single();
            this.decompressor.GetImageInfo(data.Item2.AsSpan(), TJPixelFormat.RGB, out int width, out int height, out int stride, out int bufSize);

            Assert.Equal(227, width);
            Assert.Equal(149, height);
            Assert.Equal(684, stride);
            Assert.Equal(101916, bufSize);
        }
    }
}
