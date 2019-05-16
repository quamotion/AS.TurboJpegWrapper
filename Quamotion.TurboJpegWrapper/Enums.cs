// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

using System;

namespace TurboJpegWrapper
{
    /// <summary>
    /// JPEG colorspaces.
    /// </summary>
    public enum TJColorSpaces
    {
        /// <summary>
        /// RGB colorspace.  When compressing the JPEG image, the R, G, and B
        /// components in the source image are reordered into image planes, but no
        /// colorspace conversion or subsampling is performed.  RGB JPEG images can be
        /// decompressed to any of the extended RGB pixel formats or grayscale, but
        /// they cannot be decompressed to YUV images.
        /// </summary>
        TJCS_RGB = 0,

        /// <summary>
        /// YCbCr colorspace.  YCbCr is not an absolute colorspace but rather a
        /// mathematical transformation of RGB designed solely for storage and
        /// transmission.  YCbCr images must be converted to RGB before they can
        /// actually be displayed.  In the YCbCr colorspace, the Y (luminance)
        /// component represents the black-and-white portion of the original image, and
        /// the Cb and Cr (chrominance) components represent the color portion of the
        /// original image.  Originally, the analog equivalent of this transformation
        /// allowed the same signal to drive both black-and-white and color televisions,
        /// but JPEG images use YCbCr primarily because it allows the color data to be
        /// optionally subsampled for the purposes of reducing bandwidth or disk
        /// space.  YCbCr is the most common JPEG colorspace, and YCbCr JPEG images
        /// can be compressed from and decompressed to any of the extended RGB pixel
        /// formats or grayscale, or they can be decompressed to YUV planar images.
        /// </summary>
        TJCS_YCbCr,

        /// <summary>
        /// Grayscale colorspace.  The JPEG image retains only the luminance data (Y
        /// component), and any color data from the source image is discarded.
        /// Grayscale JPEG images can be compressed from and decompressed to any of
        /// the extended RGB pixel formats or grayscale, or they can be decompressed
        /// to YUV planar images.
        /// </summary>
        TJCS_GRAY,

        /// <summary>
        /// CMYK colorspace.  When compressing the JPEG image, the C, M, Y, and K
        /// components in the source image are reordered into image planes, but no
        /// colorspace conversion or subsampling is performed.  CMYK JPEG images can
        /// only be decompressed to CMYK pixels.
        /// </summary>
        TJCS_CMYK,

        /// <summary>
        /// YCCK colorspace.  YCCK (AKA "YCbCrK") is not an absolute colorspace but
        /// rather a mathematical transformation of CMYK designed solely for storage
        /// and transmission.  It is to CMYK as YCbCr is to RGB.  CMYK pixels can be
        /// reversibly transformed into YCCK, and as with YCbCr, the chrominance
        /// components in the YCCK pixels can be subsampled without incurring major
        /// perceptual loss.  YCCK JPEG images can only be compressed from and
        /// decompressed to CMYK pixels.
        /// </summary>
        TJCS_YCCK,
    }

    /// <summary>
    /// Chrominance subsampling options.
    /// <para>
    /// When pixels are converted from RGB to YCbCr (see #TJCS_YCbCr) or from CMYK
    /// to YCCK (see #TJCS_YCCK) as part of the JPEG compression process, some of
    /// the Cb and Cr (chrominance) components can be discarded or averaged together
    /// to produce a smaller image with little perceptible loss of image clarity
    /// (the human eye is more sensitive to small changes in brightness than to
    /// small changes in color.)  This is called "chrominance subsampling".
    /// </para>
    /// </summary>
    public enum TJSubsamplingOptions
    {
        /// <summary>
        /// 4:4:4 chrominance subsampling (no chrominance subsampling).  The JPEG or * YUV image will contain one chrominance component for every pixel in the source image.
        /// </summary>
        TJSAMP_444 = 0,

        /// <summary>
        /// 4:2:2 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 2x1 block of pixels in the source image.
        /// </summary>
        TJSAMP_422,

        /// <summary>
        /// 4:2:0 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 2x2 block of pixels in the source image.
        /// </summary>
        TJSAMP_420,

        /// <summary>
        /// Grayscale.  The JPEG or YUV image will contain no chrominance components.
        /// </summary>
        TJSAMP_GRAY,

        /// <summary>
        /// 4:4:0 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 1x2 block of pixels in the source image.
        /// </summary>
        /// <remarks>4:4:0 subsampling is not fully accelerated in libjpeg-turbo.</remarks>
        TJSAMP_440,

        /// <summary>
        /// 4:1:1 chrominance subsampling.  The JPEG or YUV image will contain one
        /// chrominance component for every 4x1 block of pixels in the source image.
        /// JPEG images compressed with 4:1:1 subsampling will be almost exactly the
        /// same size as those compressed with 4:2:0 subsampling, and in the
        /// aggregate, both subsampling methods produce approximately the same
        /// perceptual quality.  However, 4:1:1 is better able to reproduce sharp
        /// horizontal features.
        /// </summary>
        /// <remarks> 4:1:1 subsampling is not fully accelerated in libjpeg-turbo.</remarks>
        TJSAMP_411,
    }

    /// <summary>
    /// Pixel formats.
    /// </summary>
    public enum TJPixelFormats
    {
        /// <summary>
        /// RGB pixel format.  The red, green, and blue components in the image are
        /// stored in 3-byte pixels in the order R, G, B from lowest to highest byte
        /// address within each pixel.
        /// </summary>
        TJPF_RGB = 0,

        /// <summary>
        /// BGR pixel format.  The red, green, and blue components in the image are
        /// stored in 3-byte pixels in the order B, G, R from lowest to highest byte
        /// address within each pixel.
        /// </summary>
        TJPF_BGR,

        /// <summary>
        /// RGBX pixel format.  The red, green, and blue components in the image are
        /// stored in 4-byte pixels in the order R, G, B from lowest to highest byte
        /// address within each pixel.  The X component is ignored when compressing
        /// and undefined when decompressing.
        /// </summary>
        TJPF_RGBX,

        /// <summary>
        /// BGRX pixel format.  The red, green, and blue components in the image are
        /// stored in 4-byte pixels in the order B, G, R from lowest to highest byte
        /// address within each pixel.  The X component is ignored when compressing
        /// and undefined when decompressing.
        ///  </summary>
        TJPF_BGRX,

        /// <summary>
        /// XBGR pixel format.  The red, green, and blue components in the image are
        /// stored in 4-byte pixels in the order R, G, B from highest to lowest byte
        /// address within each pixel.  The X component is ignored when compressing
        /// and undefined when decompressing.
        /// </summary>
        TJPF_XBGR,

        /// <summary>
        /// XRGB pixel format.  The red, green, and blue components in the image are
        /// stored in 4-byte pixels in the order B, G, R from highest to lowest byte
        /// address within each pixel.  The X component is ignored when compressing
        /// and undefined when decompressing.
        /// </summary>
        TJPF_XRGB,

        /// <summary>
        /// Grayscale pixel format.  Each 1-byte pixel represents a luminance
        /// (brightness) level from 0 to 255.
        /// </summary>
        TJPF_GRAY,

        /// <summary>
        /// RGBA pixel format.  This is the same as <see cref="TJPF_RGBX"/>, except that when
        /// decompressing, the X component is guaranteed to be 0xFF, which can be
        /// interpreted as an opaque alpha channel.
        /// </summary>
        TJPF_RGBA,

        /// <summary>
        /// BGRA pixel format.  This is the same as <see cref="TJPF_BGRX"/>, except that when
        /// decompressing, the X component is guaranteed to be 0xFF, which can be
        /// interpreted as an opaque alpha channel.
        /// </summary>
        TJPF_BGRA,

        /// <summary>
        /// ABGR pixel format.  This is the same as <see cref="TJPF_XBGR"/>, except that when
        /// decompressing, the X component is guaranteed to be 0xFF, which can be
        /// interpreted as an opaque alpha channel.
        /// </summary>
        TJPF_ABGR,

        /// <summary>
        /// ARGB pixel format.  This is the same as <see cref="TJPF_XRGB"/>, except that when
        /// decompressing, the X component is guaranteed to be 0xFF, which can be
        /// interpreted as an opaque alpha channel.
        /// </summary>
        TJPF_ARGB,

        /// <summary>
        /// CMYK pixel format.  Unlike RGB, which is an additive color model used
        /// primarily for display, CMYK (Cyan/Magenta/Yellow/Key) is a subtractive
        /// color model used primarily for printing.  In the CMYK color model, the
        /// value of each color component typically corresponds to an amount of cyan,
        /// magenta, yellow, or black ink that is applied to a white background.  In
        /// order to convert between CMYK and RGB, it is necessary to use a color
        /// management system (CMS.)  A CMS will attempt to map colors within the
        /// printer's gamut to perceptually similar colors in the display's gamut and
        /// vice versa, but the mapping is typically not 1:1 or reversible, nor can it
        /// be defined with a simple formula.  Thus, such a conversion is out of scope
        /// for a codec library.  However, the TurboJPEG API allows for compressing
        /// CMYK pixels into a YCCK JPEG image (see #TJCS_YCCK) and decompressing YCCK
        /// JPEG images into CMYK pixels.
        /// </summary>
        TJPF_CMYK,
    }

    /// <summary>
    /// Flags for turbo jpeg.
    /// </summary>
    [Flags]
    public enum TJFlags
    {
        /// <summary>
        /// Flags not set
        /// </summary>
        NONE = 0,

        /// <summary>
        /// The uncompressed source/destination image is stored in bottom-up (Windows, OpenGL) order,
        /// not top-down (X11) order.
        /// </summary>
        BOTTOMUP = 2,

        /// <summary>
        /// When decompressing an image that was compressed using chrominance subsampling,
        /// use the fastest chrominance upsampling algorithm available in the underlying codec.
        /// The default is to use smooth upsampling, which creates a smooth transition between
        /// neighboring chrominance components in order to reduce upsampling artifacts in the decompressed image.
        /// </summary>
        FASTUPSAMPLE = 256,

        /// <summary>
        /// Disable buffer (re)allocation.  If passed to <see cref="TurboJpegImport.tjCompress2"/> or #tjTransform(),
        /// this flag will cause those functions to generate an error
        /// if the JPEG image buffer is invalid or too small rather than attempting to allocate or reallocate that buffer.
        /// This reproduces the behavior of earlier versions of TurboJPEG.
        /// </summary>
        NOREALLOC = 1024,

        /// <summary>
        /// Use the fastest DCT/IDCT algorithm available in the underlying codec.  The
        /// default if this flag is not specified is implementation-specific.  For
        /// example, the implementation of TurboJPEG for libjpeg[-turbo] uses the fast
        /// algorithm by default when compressing, because this has been shown to have
        /// only a very slight effect on accuracy, but it uses the accurate algorithm
        /// when decompressing, because this has been shown to have a larger effect.
        /// </summary>
        FASTDCT = 2048,

        /// <summary>
        /// Use the most accurate DCT/IDCT algorithm available in the underlying codec.
        /// The default if this flag is not specified is implementation-specific.  For
        /// example, the implementation of TurboJPEG for libjpeg[-turbo] uses the fast
        /// algorithm by default when compressing, because this has been shown to have
        /// only a very slight effect on accuracy, but it uses the accurate algorithm
        /// when decompressing, because this has been shown to have a larger effect.
        /// </summary>
        ACCURATEDCT = 4096,
    }

    /// <summary>
    /// Transform operations for <see cref="TurboJpegImport.tjTransform"/>.
    /// </summary>
    public enum TJTransformOperations
    {
        /// <summary>
        ///  Do not transform the position of the image pixels
        /// </summary>
        TJXOP_NONE = 0,

        /// <summary>
        /// Flip (mirror) image horizontally.  This transform is imperfect if there
        /// are any partial MCU blocks on the right edge (see <see cref="TJTransformOptions.PERFECT"/>.)</summary>
        TJXOP_HFLIP,

        /// <summary>
        /// Flip (mirror) image vertically.  This transform is imperfect if there are
        /// any partial MCU blocks on the bottom edge (see <see cref="TJTransformOptions.PERFECT"/>.)
        /// </summary>
        TJXOP_VFLIP,

        /// <summary>
        /// Transpose image (flip/mirror along upper left to lower right axis.)  This
        /// transform is always perfect.
        /// </summary>
        TJXOP_TRANSPOSE,

        /// <summary>
        /// Transverse transpose image (flip/mirror along upper right to lower left
        /// axis.)  This transform is imperfect if there are any partial MCU blocks in
        /// the image (see <see cref="TJTransformOptions.PERFECT"/>.)
        /// </summary>
        TJXOP_TRANSVERSE,

        /// <summary>
        /// Rotate image clockwise by 90 degrees.  This transform is imperfect if
        /// there are any partial MCU blocks on the bottom edge (<see cref="TJTransformOptions.PERFECT"/>.)
        /// </summary>
        TJXOP_ROT90,

        /// <summary>
        /// Rotate image 180 degrees.  This transform is imperfect if there are any
        /// partial MCU blocks in the image (see <see cref="TJTransformOptions.PERFECT"/>.)
        /// </summary>
        TJXOP_ROT180,

        /// <summary>
        /// Rotate image counter-clockwise by 90 degrees.  This transform is imperfect
        /// if there are any partial MCU blocks on the right edge (see <see cref="TJTransformOptions.PERFECT"/>.)
        /// </summary>
        TJXOP_ROT270,
    }

    /// <summary>
    /// Transformation options.
    /// </summary>
    [Flags]
    public enum TJTransformOptions
    {
        /// <summary>
        /// This option will cause <see cref="TurboJpegImport.tjTransform"/> to return an error if the transform is
        /// not perfect.  Lossless transforms operate on MCU blocks, whose size depends
        /// on the level of chrominance subsampling used
        /// If the image's width or height is not evenly divisible
        /// by the MCU block size, then there will be partial MCU blocks on the right
        /// and/or bottom edges.  It is not possible to move these partial MCU blocks to
        /// the top or left of the image, so any transform that would require that is
        /// "imperfect."  If this option is not specified, then any partial MCU blocks
        /// that cannot be transformed will be left in place, which will create
        /// odd-looking strips on the right or bottom edge of the image.
        /// </summary>
        PERFECT = 1,

        /// <summary>
        /// This option will cause <see cref="TurboJpegImport.tjTransform"/> to discard any partial MCU blocks that cannot be transformed.
        /// </summary>
        TRIM = 2,

        /// <summary>
        /// This option will enable lossless cropping.  See <see cref="TurboJpegImport.tjTransform"/> for more information.
        /// </summary>
        CROP = 4,

        /// <summary>
        /// This option will discard the color data in the input image and produce a grayscale output image.
        /// </summary>
        GRAY = 8,

        /// <summary>
        /// This option will prevent <see cref="TurboJpegImport.tjTransform"/> from outputting a JPEG image for
        /// this particular transform (this can be used in conjunction with a custom
        /// filter to capture the transformed DCT coefficients without transcoding them.)
        /// </summary>
        NOOUTPUT = 16,
    }
}