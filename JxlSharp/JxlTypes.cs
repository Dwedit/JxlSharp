using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
    /// <summary>
    /// Image orientation metadata.
    /// Values 1..8 match the EXIF definitions.
    /// The name indicates the operation to perform to transform from the encoded
    /// image to the display image.
    /// </summary>
    public enum JxlOrientation
    {
        Identity = 1,
        FlipHorizontal = 2,
        Rotate180 = 3,
        FlipVertical = 4,
        Transpose = 5,
        Rotate90CW = 6,
        AntiTranspose = 7,
        Rotate90CCW = 8,
    }

    /// <summary>
    /// Ordering of multi-byte data.
    /// </summary>
    public enum JxlEndianness
    {
        /// <summary>
        /// Use the endianness of the system, either little endian or big endian,
        /// without forcing either specific endianness. Do not use if pixel data
        /// should be exported to a well defined format.
        /// </summary>
        NativeEndian = 0,
        /// <summary>
        /// Force little endian 
        /// </summary>
        LittleEndian = 1,
        /// <summary>
        /// Force big endian 
        /// </summary>
        BigEndian = 2,
    }

    /// <summary>
    /// Data type for the sample values per channel per pixel.
    /// </summary>
    public enum JxlDataType
    {
        /// <summary>
        /// Use 32-bit single-precision floating point values, with range 0.0-1.0
        /// (within gamut, may go outside this range for wide color gamut). Floating
        /// point output, either Float or Float16, is recommended
        /// for HDR and wide gamut images when color profile conversion is required. 
        /// </summary>
        Float = 0,

        /// <summary>
        /// Use type uint8_t. May clip wide color gamut data.
        /// </summary>
        UInt8 = 2,

        /// <summary>
        /// Use type uint16_t. May clip wide color gamut data.
        /// </summary>
        UInt16 = 3,

        /// <summary>
        /// Use 16-bit IEEE 754 half-precision floating point values 
        /// </summary>
        Float16 = 5,
    }

    /// <summary>
    /// The header of one displayed frame or non-coalesced layer. 
    /// </summary>
    public struct JxlAnimationHeader
    {
        public JxlAnimationHeader(uint tpsNumerator, uint tpsDenominator, int numLoops, bool haveTimecodes)
        {
            this.TpsNumerator = tpsNumerator;
            this.TpsDenominator = tpsDenominator;
            this.NumLoops = numLoops;
            this.HaveTimecodes = haveTimecodes;
        }

        /// <summary>
        /// Numerator of ticks per second of a single animation frame time unit 
        /// </summary>
        public uint TpsNumerator { get; set; }

        /// <summary>
        /// Denominator of ticks per second of a single animation frame time unit 
        /// </summary>
        public uint TpsDenominator { get; set; }

        /// <summary>
        /// Amount of animation loops, or 0 to repeat infinitely 
        /// </summary>
        public int NumLoops { get; set; }

        /// <summary>
        /// Whether animation time codes are present at animation frames in the
        /// codestream 
        /// </summary>
        public bool HaveTimecodes { get; set; }
    }

    /// <summary>
    /// Return value for <see cref="JxlDecoderProcessInput"/>.
    /// The values from <see cref="BasicInfo"/> onwards are optional informative
    /// events that can be subscribed to, they are never returned if they
    /// have not been registered with <see cref="JxlDecoderSubscribeEvents"/>.
    /// </summary>
    [Flags]
    public enum JxlDecoderStatus
    {
        /// <summary>
        /// Function call finished successfully, or decoding is finished and there is
        /// nothing more to be done.
        /// </summary>
        Success = 0,

        /// <summary>
        /// An error occurred, for example invalid input file or out of memory.
        /// TODO(lode): add function to get error information from decoder.
        /// </summary>
        Error = 1,

        /// <summary>
        /// The decoder needs more input bytes to continue. Before the next <see cref="JxlDecoderProcessInput"/> call, more input data must be set, by calling <see cref="JxlDecoderReleaseInput"/> (if input was set previously) and then calling <see cref="JxlDecoderSetInput"/>. <see cref="JxlDecoderReleaseInput"/> returns how many bytes
        /// are not yet processed, before a next call to <see cref="JxlDecoderProcessInput"/>
        /// all unprocessed bytes must be provided again (the address need not match,
        /// but the contents must), and more bytes must be concatenated after the
        /// unprocessed bytes.
        /// </summary>
        NeedMoreInput = 2,

        /// <summary>
        /// The decoder is able to decode a preview image and requests setting a
        /// preview output buffer using <see cref="JxlDecoderSetPreviewOutBuffer"/>. This occurs
        /// if <see cref="PreviewImage"/> is requested and it is possible to decode a
        /// preview image from the codestream and the preview out buffer was not yet
        /// set. There is maximum one preview image in a codestream.
        /// </summary>
        NeedPreviewOutBuffer = 3,

        /// <summary>
        /// The decoder is able to decode a DC image and requests setting a DC output
        /// buffer using <see cref="JxlDecoderSetDCOutBuffer"/>. This occurs if <see cref="DcImage"/> is requested and it is possible to decode a DC image from
        /// the codestream and the DC out buffer was not yet set. This event re-occurs
        /// for new frames if there are multiple animation frames.
        /// @deprecated The DC feature in this form will be removed. For progressive
        /// rendering, <see cref="JxlDecoderFlushImage"/> should be used.
        /// </summary>
        NeedDcOutBuffer = 4,

        /// <summary>
        /// The decoder requests an output buffer to store the full resolution image,
        /// which can be set with <see cref="JxlDecoderSetImageOutBuffer"/> or with <see cref="JxlDecoderSetImageOutCallback"/>. This event re-occurs for new frames if
        /// there are multiple animation frames and requires setting an output again.
        /// </summary>
        NeedImageOutBuffer = 5,

        /// <summary>
        /// The JPEG reconstruction buffer is too small for reconstructed JPEG
        /// codestream to fit. <see cref="JxlDecoderSetJPEGBuffer"/> must be called again to
        /// make room for remaining bytes. This event may occur multiple times
        /// after <see cref="JpegReconstruction"/>.
        /// </summary>
        JpegNeedMoreOutput = 6,

        /// <summary>
        /// The box contents output buffer is too small. <see cref="JxlDecoderSetBoxBuffer"/>
        /// must be called again to make room for remaining bytes. This event may occur
        /// multiple times after <see cref="Box"/>.
        /// </summary>
        BoxNeedMoreOutput = 7,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": Basic information such as image dimensions and
        /// extra channels. This event occurs max once per image.
        /// </summary>
        BasicInfo = 0x40,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": User extensions of the codestream header. This
        /// event occurs max once per image and always later than <see cref="BasicInfo"/> and earlier than any pixel data.
        /// <br/><br/>
        /// @deprecated The decoder no longer returns this, the header extensions,
        /// if any, are available at the BasicInfo event.
        /// </summary>
        Extensions = 0x80,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": Color encoding or ICC profile from the
        /// codestream header. This event occurs max once per image and always later
        /// than <see cref="BasicInfo"/> and earlier than any pixel data.
        /// </summary>
        ColorEncoding = 0x100,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": Preview image, a small frame, decoded. This
        /// event can only happen if the image has a preview frame encoded. This event
        /// occurs max once for the codestream and always later than <see cref="ColorEncoding"/> and before <see cref="Frame"/>.
        /// </summary>
        PreviewImage = 0x200,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": Beginning of a frame. <see cref="JxlDecoderGetFrameHeader"/> can be used at this point. A note on frames:
        /// a JPEG XL image can have internal frames that are not intended to be
        /// displayed (e.g. used for compositing a final frame), but this only returns
        /// displayed frames, unless <see cref="JxlDecoderSetCoalescing"/> was set to false:
        /// in that case, the individual layers are returned, without blending. Note
        /// that even when coalescing is disabled, only frames of type kRegularFrame
        /// are returned; frames of type kReferenceOnly and kLfFrame are always for
        /// internal purposes only and cannot be accessed. A displayed frame either has
        /// an animation duration or is the only or last frame in the image. This event
        /// occurs max once per displayed frame, always later than <see cref="ColorEncoding"/>, and always earlier than any pixel data. While
        /// JPEG XL supports encoding a single frame as the composition of multiple
        /// internal sub-frames also called frames, this event is not indicated for the
        /// internal frames.
        /// </summary>
        Frame = 0x400,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": DC image, 8x8 sub-sampled frame, decoded. It is
        /// not guaranteed that the decoder will always return DC separately, but when
        /// it does it will do so before outputting the full frame. <see cref="JxlDecoderSetDCOutBuffer"/> must be used after getting the basic image
        /// information to be able to get the DC pixels, if not this return status only
        /// indicates we're past this point in the codestream. This event occurs max
        /// once per frame and always later than <see cref="Frame"/> and other header
        /// events and earlier than full resolution pixel data.
        /// <br/><br/>
        /// @deprecated The DC feature in this form will be removed. For progressive
        /// rendering, <see cref="JxlDecoderFlushImage"/> should be used.
        /// </summary>
        DcImage = 0x800,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": full frame (or layer, in case coalescing is
        /// disabled) is decoded. <see cref="JxlDecoderSetImageOutBuffer"/> must be used after
        /// getting the basic image information to be able to get the image pixels, if
        /// not this return status only indicates we're past this point in the
        /// codestream. This event occurs max once per frame and always later than <see cref="DcImage"/>.
        /// </summary>
        FullImage = 0x1000,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": JPEG reconstruction data decoded. <see cref="JxlDecoderSetJPEGBuffer"/> may be used to set a JPEG reconstruction buffer
        /// after getting the JPEG reconstruction data. If a JPEG reconstruction buffer
        /// is set a byte stream identical to the JPEG codestream used to encode the
        /// image will be written to the JPEG reconstruction buffer instead of pixels
        /// to the image out buffer. This event occurs max once per image and always
        /// before <see cref="FullImage"/>.
        /// </summary>
        JpegReconstruction = 0x2000,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": The header of a box of the container format
        /// (BMFF) is decoded. The following API functions related to boxes can be used
        /// after this event:
        /// - <see cref="JxlDecoderSetBoxBuffer"/> and <see cref="JxlDecoderReleaseBoxBuffer"/>
        /// "JxlDecoderReleaseBoxBuffer": set and release a buffer to get the box
        /// data.
        /// - <see cref="JxlDecoderGetBoxType"/> get the 4-character box typename.
        /// - <see cref="JxlDecoderGetBoxSizeRaw"/> get the size of the box as it appears in
        /// the container file, not decompressed.
        /// - <see cref="JxlDecoderSetDecompressBoxes"/> to configure whether to get the box
        /// data decompressed, or possibly compressed.
        /// <br/><br/>
        /// Boxes can be compressed. This is so when their box type is
        /// "brob". In that case, they have an underlying decompressed box
        /// type and decompressed data. <see cref="JxlDecoderSetDecompressBoxes"/> allows
        /// configuring which data to get. Decompressing requires
        /// Brotli. <see cref="JxlDecoderGetBoxType"/> has a flag to get the compressed box
        /// type, which can be "brob", or the decompressed box type. If a box
        /// is not compressed (its compressed type is not "brob"), then
        /// the output decompressed box type and data is independent of what
        /// setting is configured.
        /// <br/><br/>
        /// The buffer set with <see cref="JxlDecoderSetBoxBuffer"/> must be set again for each
        /// next box to be obtained, or can be left unset to skip outputting this box.
        /// The output buffer contains the full box data when the next <see cref="Box"/>
        /// event or <see cref="Success"/> occurs. <see cref="Box"/> occurs for all
        /// boxes, including non-metadata boxes such as the signature box or codestream
        /// boxes. To check whether the box is a metadata type for respectively EXIF,
        /// XMP or JUMBF, use <see cref="JxlDecoderGetBoxType"/> and check for types "Exif",
        /// "xml " and "jumb" respectively.
        /// </summary>
        Box = 0x4000,

        /// <summary>
        /// Informative event by <see cref="JxlDecoderProcessInput"/>
        /// "JxlDecoderProcessInput": a progressive step in decoding the frame is
        /// reached. When calling <see cref="JxlDecoderFlushImage"/> at this point, the flushed
        /// image will correspond exactly to this point in decoding, and not yet
        /// contain partial results (such as partially more fine detail) of a next
        /// step. By default, this event will trigger maximum once per frame, when a
        /// 8x8th resolution (DC) image is ready (the image data is still returned at
        /// full resolution, giving upscaled DC). Use <see cref="JxlDecoderSetProgressiveDetail"/> to configure more fine-grainedness. The
        /// event is not guaranteed to trigger, not all images have progressive steps
        /// or DC encoded.
        /// </summary>
        FrameProgression = 0x8000,
    }

    /// <summary>
    /// Data type for the sample values per channel per pixel for the output buffer
    /// for pixels. This is not necessarily the same as the data type encoded in the
    /// codestream. The channels are interleaved per pixel. The pixels are
    /// organized row by row, left to right, top to bottom.
    /// TODO(lode): implement padding / alignment (row stride)
    /// TODO(lode): support different channel orders if needed (RGB, BGR, ...)
    /// </summary>
    public class JxlPixelFormat
    {
        internal UnsafeNativeJXL.JxlPixelFormat pixelFormat;

        /// <summary>
        /// Amount of channels available in a pixel buffer.
        /// 1: single-channel data, e.g. grayscale or a single extra channel
        /// 2: single-channel + alpha
        /// 3: trichromatic, e.g. RGB
        /// 4: trichromatic + alpha
        /// TODO(lode): this needs finetuning. It is not yet defined how the user
        /// chooses output color space. CMYK+alpha needs 5 channels.
        /// </summary>
        public int NumChannels
        {
            get
            {
                return (int)pixelFormat.num_channels;
            }
            set
            {
                pixelFormat.num_channels = (uint)value;
            }
        }

        /// <summary>
        /// Data type of each channel.
        /// </summary>
        public JxlDataType DataType
        {
            get
            {
                return (JxlDataType)pixelFormat.data_type;
            }
            set
            {
                pixelFormat.data_type = (UnsafeNativeJXL.JxlDataType)value;
            }
        }

        /// <summary>
        /// Whether multi-byte data types are represented in big endian or little
        /// endian format. This applies to JXL_TYPE_UINT16, JXL_TYPE_UINT32
        /// and JXL_TYPE_FLOAT.
        /// </summary>
        public JxlEndianness Endianness
        {
            get
            {
                return (JxlEndianness)pixelFormat.endianness;
            }
            set
            {
                pixelFormat.endianness = (UnsafeNativeJXL.JxlEndianness)value;
            }
        }

        /// <summary>
        /// Align scanlines to a multiple of align bytes, or 0 to require no
        /// alignment at all (which has the same effect as value 1)
        /// </summary>
        public int Align
        {
            get
            {
                return (int)pixelFormat.align;
            }
            set
            {
                pixelFormat.align = (UIntPtr)value;
            }
        }
    }


    /// <summary>
    /// Basic image information. This information is available from the file
    /// signature and first part of the codestream header.
    /// </summary>
    public class JxlBasicInfo
    {
        internal UnsafeNativeJXL.JxlBasicInfo basicInfo;

        /// <summary>
        /// Whether the codestream is embedded in the container format. If true,
        /// metadata information and extensions may be available in addition to the
        /// codestream.
        /// </summary>
        public bool HaveContainer
        {
            get
            {
                return Convert.ToBoolean(basicInfo.have_container);
            }
            set
            {
                basicInfo.have_container = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Width of the image in pixels, before applying orientation.
        /// </summary>
        public int Width
        {
            get
            {
                return (int)basicInfo.xsize;
            }
            set
            {
                basicInfo.xsize = (uint)value;
            }
        }

        /// <summary>
        /// Height of the image in pixels, before applying orientation.
        /// </summary>
        public int Height
        {
            get
            {
                return (int)basicInfo.ysize;
            }
            set
            {
                basicInfo.ysize = (uint)value;
            }
        }

        /// <summary>
        /// Original image color channel bit depth.
        /// </summary>
        public int BitsPerSample
        {
            get
            {
                return (int)basicInfo.bits_per_sample;
            }
            set
            {
                basicInfo.bits_per_sample = (uint)value;
            }
        }

        /// <summary>
        /// Original image color channel floating point exponent bits, or 0 if they
        /// are unsigned integer. For example, if the original data is half-precision
        /// (binary16) floating point, bits_per_sample is 16 and
        /// exponent_bits_per_sample is 5, and so on for other floating point
        /// precisions.
        /// </summary>
        public int ExponentBitsPerSample
        {
            get
            {
                return (int)basicInfo.exponent_bits_per_sample;
            }
            set
            {
                basicInfo.exponent_bits_per_sample = (uint)value;
            }
        }

        /// <summary>
        /// Upper bound on the intensity level present in the image in nits. For
        /// unsigned integer pixel encodings, this is the brightness of the largest
        /// representable value. The image does not necessarily contain a pixel
        /// actually this bright. An encoder is allowed to set 255 for SDR images
        /// without computing a histogram.
        /// Leaving this set to its default of 0 lets libjxl choose a sensible default
        /// value based on the color encoding.
        /// </summary>
        public float IntensityTarget
        {
            get
            {
                return basicInfo.intensity_target;
            }
            set
            {
                basicInfo.intensity_target = value;
            }
        }

        /// <summary>
        /// Lower bound on the intensity level present in the image. This may be
        /// loose, i.e. lower than the actual darkest pixel. When tone mapping, a
        /// decoder will map [min_nits, intensity_target] to the display range.
        /// </summary>
        public float MinNits
        {
            get
            {
                return basicInfo.min_nits;
            }
            set
            {
                basicInfo.min_nits = value;
            }
        }

        /// <summary>
        /// See the description of <see cref="LinearBelow"/>.
        /// </summary>
        public bool RelativeToMaxDisplay
        {
            get
            {
                return Convert.ToBoolean(basicInfo.relative_to_max_display);
            }
            set
            {
                basicInfo.relative_to_max_display = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// The tone mapping will leave unchanged (linear mapping) any pixels whose
        /// brightness is strictly below this. The interpretation depends on
        /// relative_to_max_display. If true, this is a ratio [0, 1] of the maximum
        /// display brightness [nits], otherwise an absolute brightness [nits].
        /// </summary>
        public float LinearBelow
        {
            get
            {
                return basicInfo.linear_below;
            }
            set
            {
                basicInfo.linear_below = value;
            }
        }

        /// <summary>
        /// Whether the data in the codestream is encoded in the original color
        /// profile that is attached to the codestream metadata header, or is
        /// encoded in an internally supported absolute color space (which the decoder
        /// can always convert to linear or non-linear sRGB or to XYB). If the original
        /// profile is used, the decoder outputs pixel data in the color space matching
        /// that profile, but doesn't convert it to any other color space. If the
        /// original profile is not used, the decoder only outputs the data as sRGB
        /// (linear if outputting to floating point, nonlinear with standard sRGB
        /// transfer function if outputting to unsigned integers) but will not convert
        /// it to to the original color profile. The decoder also does not convert to
        /// the target display color profile. To convert the pixel data produced by
        /// the decoder to the original color profile, one of the JxlDecoderGetColor*
        /// functions needs to be called with <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/> to get
        /// the color profile of the decoder output, and then an external CMS can be
        /// used for conversion.
        /// Note that for lossy compression, this should be set to false for most use
        /// cases, and if needed, the image should be converted to the original color
        /// profile after decoding, as described above.
        /// </summary>
        public bool UsesOriginalProfile
        {
            get
            {
                return Convert.ToBoolean(basicInfo.uses_original_profile);
            }
            set
            {
                basicInfo.uses_original_profile = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Indicates a preview image exists near the beginning of the codestream.
        /// The preview itself or its dimensions are not included in the basic info.
        /// </summary>
        public bool HavePreview
        {
            get
            {
                return Convert.ToBoolean(basicInfo.have_preview);
            }
            set
            {
                basicInfo.have_preview = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Indicates animation frames exist in the codestream. The animation
        /// information is not included in the basic info.
        /// </summary>
        public bool HaveAnimation
        {
            get
            {
                return Convert.ToBoolean(basicInfo.have_animation);
            }
            set
            {
                basicInfo.have_animation = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Image orientation, value 1-8 matching the values used by JEITA CP-3451C
        /// (Exif version 2.3).
        /// </summary>
        public JxlOrientation Orientation
        {
            get
            {
                return (JxlOrientation)basicInfo.orientation;
            }
            set
            {
                basicInfo.orientation = (UnsafeNativeJXL.JxlOrientation)value;
            }
        }

        /// <summary>
        /// Number of color channels encoded in the image, this is either 1 for
        /// grayscale data, or 3 for colored data. This count does not include
        /// the alpha channel or other extra channels. To check presence of an alpha
        /// channel, such as in the case of RGBA color, check alpha_bits != 0.
        /// If and only if this is 1, the JxlColorSpace in the JxlColorEncoding is
        /// JXL_COLOR_SPACE_GRAY.
        /// </summary>
        public int NumColorChannels
        {
            get
            {
                return (int)basicInfo.num_color_channels;
            }
            set
            {
                basicInfo.num_color_channels = (uint)value;
            }
        }

        /// <summary>
        /// Number of additional image channels. This includes the main alpha channel,
        /// but can also include additional channels such as depth, additional alpha
        /// channels, spot colors, and so on. Information about the extra channels
        /// can be queried with JxlDecoderGetExtraChannelInfo. The main alpha channel,
        /// if it exists, also has its information available in the alpha_bits,
        /// alpha_exponent_bits and alpha_premultiplied fields in this JxlBasicInfo.
        /// </summary>
        public int NumExtraChannels
        {
            get
            {
                return (int)basicInfo.num_extra_channels;
            }
            set
            {
                basicInfo.num_extra_channels = (uint)value;
            }
        }

        /// <summary>
        /// Bit depth of the encoded alpha channel, or 0 if there is no alpha channel.
        /// If present, matches the alpha_bits value of the JxlExtraChannelInfo
        /// associated with this alpha channel.
        /// </summary>
        public int AlphaBits
        {
            get
            {
                return (int)basicInfo.alpha_bits;
            }
            set
            {
                basicInfo.alpha_bits = (uint)value;
            }
        }

        /// <summary>
        /// Alpha channel floating point exponent bits, or 0 if they are unsigned. If
        /// present, matches the alpha_bits value of the JxlExtraChannelInfo associated
        /// with this alpha channel. integer.
        /// </summary>
        public int AlphaExponentBits
        {
            get
            {
                return (int)basicInfo.alpha_exponent_bits;
            }
            set
            {
                basicInfo.alpha_exponent_bits = (uint)value;
            }
        }

        /// <summary>
        /// Whether the alpha channel is premultiplied. Only used if there is a main
        /// alpha channel. Matches the alpha_premultiplied value of the
        /// JxlExtraChannelInfo associated with this alpha channel.
        /// </summary>
        public bool AlphaPremultiplied
        {
            get
            {
                return Convert.ToBoolean(basicInfo.alpha_premultiplied);
            }
            set
            {
                basicInfo.alpha_premultiplied = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Dimensions of encoded preview image, only used if have_preview is
        /// JXL_TRUE.
        /// </summary>
        public Size Preview
        {
            get
            {
                return new Size((int)basicInfo.preview.xsize, (int)basicInfo.preview.ysize);
            }
            set
            {
                basicInfo.preview.xsize = (uint)value.Width;
                basicInfo.preview.ysize = (uint)value.Height;
            }
        }


        /// <summary>
        /// Animation header with global animation properties for all frames, only
        /// used if have_animation is JXL_TRUE.
        /// </summary>
        public JxlAnimationHeader Animation
        {
            get
            {
                return new JxlAnimationHeader()
                {
                    TpsNumerator = basicInfo.animation.tps_numerator,
                    TpsDenominator = basicInfo.animation.tps_denominator,
                    HaveTimecodes = Convert.ToBoolean(basicInfo.animation.have_timecodes),
                    NumLoops = (int)basicInfo.animation.num_loops
                };

            }
            set
            {
                basicInfo.animation.tps_numerator = value.TpsNumerator;
                basicInfo.animation.tps_denominator = value.TpsDenominator;
                basicInfo.animation.have_timecodes = Convert.ToInt32(value.HaveTimecodes);
                basicInfo.animation.num_loops = (uint)value.NumLoops;
            }
        }

        /// <summary>
        /// Intrinsic width of the image.
        /// The intrinsic size can be different from the actual size in pixels
        /// (as given by xsize and ysize) and it denotes the recommended dimensions
        /// for displaying the image, i.e. applications are advised to resample the
        /// decoded image to the intrinsic dimensions.
        /// </summary>
        public int IntrinsicWidth
        {
            get
            {
                return (int)basicInfo.intrinsic_xsize;
            }
            set
            {
                basicInfo.intrinsic_xsize = (uint)value;
            }
        }

        /// <summary>
        /// Intrinsic height of the image.
        /// The intrinsic size can be different from the actual size in pixels
        /// (as given by xsize and ysize) and it denotes the recommended dimensions
        /// for displaying the image, i.e. applications are advised to resample the
        /// decoded image to the intrinsic dimensions.
        /// </summary>
        public int IntrinsicHeight
        {
            get
            {
                return (int)basicInfo.intrinsic_ysize;
            }
            set
            {
                basicInfo.intrinsic_ysize = (uint)value;
            }
        }
    }

    /// <summary>
    /// Color space of the image data. 
    /// </summary>
    public enum JxlColorSpace
    {
        /// <summary>
        /// Tristimulus RGB 
        /// </summary>
        RGB,
        /// <summary>
        /// Luminance based, the primaries in JxlColorEncoding must be ignored. This
        /// value implies that num_color_channels in JxlBasicInfo is 1, any other value
        /// implies num_color_channels is 3. 
        /// </summary>
        Gray,
        /// <summary>
        /// XYB (opsin) color space 
        /// </summary>
        XYB,
        /// <summary>
        /// None of the other table entries describe the color space appropriately 
        /// </summary>
        Unknown,
    }

    /// <summary>
    /// Built-in whitepoints for color encoding. When decoding, the numerical xy
    /// whitepoint value can be read from the JxlColorEncoding white_point field
    /// regardless of the enum value. When encoding, enum values except
    /// JXL_WHITE_POINT_CUSTOM override the numerical fields. Some enum values match
    /// a subset of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)), however the
    /// white point and RGB primaries are separate enums here.
    /// </summary>
    public enum JxlWhitePoint
    {
        /// <summary>
        /// CIE Standard Illuminant D65: 0.3127, 0.3290 
        /// </summary>
        D65 = 1,
        /// <summary>
        /// White point must be read from the JxlColorEncoding white_point field, or
        /// as ICC profile. This enum value is not an exact match of the corresponding
        /// CICP value. 
        /// </summary>
        Custom = 2,
        /// <summary>
        /// CIE Standard Illuminant E (equal-energy): 1/3, 1/3 
        /// </summary>
        E = 10,
        /// <summary>
        /// DCI-P3 from SMPTE RP 431-2: 0.314, 0.351 
        /// </summary>
        DCI = 11,
    }

    /// <summary>
    /// Built-in primaries for color encoding. When decoding, the primaries can be
    /// read from the JxlColorEncoding primaries_red_xy, primaries_green_xy and
    /// primaries_blue_xy fields regardless of the enum value. When encoding, the
    /// enum values except JXL_PRIMARIES_CUSTOM override the numerical fields. Some
    /// enum values match a subset of CICP (Rec. ITU-T H.273 | ISO/IEC
    /// 23091-2:2019(E)), however the white point and RGB primaries are separate
    /// enums here.
    /// </summary>
    public enum JxlPrimaries
    {
        /// <summary>
        /// The CIE xy values of the red, green and blue primaries are: 0.639998686,
        /// 0.330010138; 0.300003784, 0.600003357; 0.150002046, 0.059997204 
        /// </summary>
        SRGB = 1,
        /// <summary>
        /// Primaries must be read from the JxlColorEncoding primaries_red_xy,
        /// primaries_green_xy and primaries_blue_xy fields, or as ICC profile. This
        /// enum value is not an exact match of the corresponding CICP value. 
        /// </summary>
        Custom = 2,
        /// <summary>
        /// As specified in Rec. ITU-R BT.2100-1 
        /// </summary>
        _2100 = 9,
        /// <summary>
        /// As specified in SMPTE RP 431-2 
        /// </summary>
        P3 = 11,
    }

    /// <summary>
    /// Built-in transfer functions for color encoding. Enum values match a subset
    /// of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)) unless specified
    /// otherwise. 
    /// </summary>
    public enum JxlTransferFunction
    {
        /// <summary>
        /// As specified in SMPTE RP 431-2 
        /// </summary>
        _709 = 1,
        /// <summary>
        /// None of the other table entries describe the transfer function. 
        /// </summary>
        Unknown = 2,
        /// <summary>
        /// The gamma exponent is 1 
        /// </summary>
        Linear = 8,
        /// <summary>
        /// As specified in IEC 61966-2-1 sRGB 
        /// </summary>
        SRGB = 13,
        /// <summary>
        /// As specified in SMPTE ST 2084 
        /// </summary>
        PQ = 16,
        /// <summary>
        /// As specified in SMPTE ST 428-1 
        /// </summary>
        DCI = 17,
        /// <summary>
        /// As specified in Rec. ITU-R BT.2100-1 (HLG) 
        /// </summary>
        HLG = 18,
        /// <summary>
        /// Transfer function follows power law given by the gamma value in
        /// JxlColorEncoding. Not a CICP value. 
        /// </summary>
        Gamma = 65535,
    }

    /// <summary>
    /// Renderig intent for color encoding, as specified in ISO 15076-1:2010 
    /// </summary>
    public enum JxlRenderingIntent
    {
        /// <summary>
        /// vendor-specific 
        /// </summary>
        Perceptual = 0,
        /// <summary>
        /// media-relative 
        /// </summary>
        Relative,
        /// <summary>
        /// vendor-specific 
        /// </summary>
        Saturation,
        /// <summary>
        /// ICC-absolute 
        /// </summary>
        Absolute,
    }

    /// <summary>
    /// A point using double coordinates
    /// </summary>
    public struct XYPair
    {
        public XYPair(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public double X { get; set; }
        public double Y { get; set; }
    }

    /// <summary>
    /// Color encoding of the image as structured information.
    /// </summary>
    public class JxlColorEncoding
    {
        internal UnsafeNativeJXL.JxlColorEncoding colorEncoding;

        /// <summary>
        /// Color space of the image data.
        /// </summary>
        public JxlColorSpace ColorSpace
        {
            get
            {
                return (JxlColorSpace)colorEncoding.color_space;
            }
            set
            {
                colorEncoding.color_space = (UnsafeNativeJXL.JxlColorSpace)value;
            }
        }

        /// <summary>
        /// Built-in white point. If this value is JXL_WHITE_POINT_CUSTOM, must
        /// use the numerical whitepoint values from white_point_xy.
        /// </summary>
        public JxlWhitePoint WhitePoint
        {
            get
            {
                return (JxlWhitePoint)colorEncoding.white_point;
            }
            set
            {
                colorEncoding.white_point = (UnsafeNativeJXL.JxlWhitePoint)value;
            }
        }
        /// <summary>
        /// Numerical whitepoint values in CIE xy space.  Array size is 2.
        /// </summary>
        public XYPair WhitePointXY
        {
            get
            {
                unsafe
                {
                    return new XYPair(colorEncoding.white_point_xy[0], colorEncoding.white_point_xy[1]);
                }
            }
            set
            {
                unsafe
                {
                    colorEncoding.white_point_xy[0] = value.X;
                    colorEncoding.white_point_xy[1] = value.Y;
                }
            }
        }

        /// <summary>
        /// Built-in RGB primaries. If this value is JXL_PRIMARIES_CUSTOM, must
        /// use the numerical primaries values below. This field and the custom values
        /// below are unused and must be ignored if the color space is
        /// JXL_COLOR_SPACE_GRAY or JXL_COLOR_SPACE_XYB.
        /// </summary>
        public JxlPrimaries Primaries
        {
            get
            {
                return (JxlPrimaries)colorEncoding.primaries;
            }
            set
            {
                colorEncoding.primaries = (UnsafeNativeJXL.JxlPrimaries)value;
            }
        }

        /// <summary>
        /// Numerical red primary values in CIE xy space.  Array size is 2.
        /// </summary>
        public XYPair PrimariesRedXY
        {
            get
            {
                unsafe
                {
                    return new XYPair(colorEncoding.primaries_red_xy[0], colorEncoding.primaries_red_xy[1]);
                }
            }
            set
            {
                unsafe
                {
                    colorEncoding.primaries_red_xy[0] = value.X;
                    colorEncoding.primaries_red_xy[1] = value.Y;
                }
            }
        }

        /// <summary>
        /// Numerical green primary values in CIE xy space.  Array size is 2.
        /// </summary>
        public XYPair PrimariesGreenXY
        {
            get
            {
                unsafe
                {
                    return new XYPair(colorEncoding.primaries_green_xy[0], colorEncoding.primaries_green_xy[1]);
                }
            }
            set
            {
                unsafe
                {
                    colorEncoding.primaries_green_xy[0] = value.X;
                    colorEncoding.primaries_green_xy[1] = value.Y;
                }
            }
        }

        /// <summary>
        /// Numerical blue primary values in CIE xy space.  Array size is 2.
        /// </summary>
        public XYPair PrimariesBlueXY
        {
            get
            {
                unsafe
                {
                    return new XYPair(colorEncoding.primaries_blue_xy[0], colorEncoding.primaries_blue_xy[1]);
                }
            }
            set
            {
                unsafe
                {
                    colorEncoding.primaries_blue_xy[0] = value.X;
                    colorEncoding.primaries_blue_xy[1] = value.Y;
                }
            }
        }

        /// <summary>
        /// Transfer function if have_gamma is 0 
        /// </summary>
        public JxlTransferFunction TransferFunction
        {
            get
            {
                return (JxlTransferFunction)colorEncoding.transfer_function;
            }
            set
            {
                colorEncoding.transfer_function = (UnsafeNativeJXL.JxlTransferFunction)value;
            }
        }

        /// <summary>
        /// Gamma value used when transfer_function is JXL_TRANSFER_FUNCTION_GAMMA
        /// </summary>
        public double Gamma
        {
            get
            {
                return colorEncoding.gamma;
            }
            set
            {
                colorEncoding.gamma = value;
            }
        }

        /// <summary>
        /// Rendering intent defined for the color profile. 
        /// </summary>
        public JxlRenderingIntent RenderingIntent
        {
            get
            {
                return (JxlRenderingIntent)colorEncoding.rendering_intent;
            }
            set
            {
                colorEncoding.rendering_intent = (UnsafeNativeJXL.JxlRenderingIntent)value;
            }
        }
    }

    /// <summary>
    /// The information about layers.
    /// When decoding, if coalescing is enabled (default), this can be ignored.
    /// When encoding, these settings apply to the pixel data given to the encoder,
    /// the encoder could choose an internal representation that differs.
    /// </summary>
    public struct JxlLayerInfo
    {
        /// <summary>
        /// Whether cropping is applied for this frame. When decoding, if false,
        /// crop_x0 and crop_y0 are set to zero, and xsize and ysize to the main
        /// image dimensions. When encoding and this is false, those fields are
        /// ignored. When decoding, if coalescing is enabled (default), this is always
        /// false, regardless of the internal encoding in the JPEG XL codestream.
        /// </summary>
        public bool HaveCrop;

        /// <summary>
        /// Horizontal offset of the frame (can be negative).
        /// </summary>
        public int CropX0;

        /// <summary>
        /// Vertical offset of the frame (can be negative).
        /// </summary>
        public int CropY0;

        /// <summary>
        /// Width of the frame (number of columns).
        /// </summary>
        public int Width;

        /// <summary>
        /// Height of the frame (number of rows).
        /// </summary>
        public int Height;

        /// <summary>
        /// The blending info for the color channels. Blending info for extra channels
        /// has to be retrieved separately using JxlDecoderGetExtraChannelBlendInfo.
        /// </summary>
        public JxlBlendInfo BlendInfo;

        /// <summary>
        /// After blending, save the frame as reference frame with this ID (0-3).
        /// Special case: if the frame duration is nonzero, ID 0 means "will not be
        /// referenced in the future". This value is not used for the last frame.
        /// </summary>
        public int SaveAsReference;
    }

    /// <summary>
    /// Frame blend modes.
    /// When decoding, if coalescing is enabled (default), this can be ignored.
    /// </summary>
    public enum JxlBlendMode
    {
        Replace = 0,
        Add = 1,
        Blend = 2,
        MulAdd = 3,
        Mul = 4,
    }

    /// <summary>
    /// The information about blending the color channels or a single extra channel.
    /// When decoding, if coalescing is enabled (default), this can be ignored and
    /// the blend mode is considered to be JXL_BLEND_REPLACE.
    /// When encoding, these settings apply to the pixel data given to the encoder.
    /// </summary>
    public struct JxlBlendInfo
    {
        /// <summary>
        /// Blend mode.
        /// </summary>
        public JxlBlendMode BlendMode;
        /// <summary>
        /// Reference frame ID to use as the 'bottom' layer (0-3).
        /// </summary>
        public int Source;
        /// <summary>
        /// Which extra channel to use as the 'alpha' channel for blend modes
        /// JXL_BLEND_BLEND and JXL_BLEND_MULADD.
        /// </summary>
        public int Alpha;
        /// <summary>
        /// Clamp values to [0,1] for the purpose of blending.
        /// </summary>
        public bool Clamp;
    }

    /// <summary>
    /// The header of one displayed frame or non-coalesced layer. 
    /// </summary>
    public class JxlFrameHeader
    {
        /// <summary>
        /// How long to wait after rendering in ticks. The duration in seconds of a
        /// tick is given by tps_numerator and tps_denominator in JxlAnimationHeader.
        /// </summary>
        public uint Duration;

        /// <summary>
        /// SMPTE timecode of the current frame in form 0xHHMMSSFF, or 0. The bits are
        /// interpreted from most-significant to least-significant as hour, minute,
        /// second, and frame. If timecode is nonzero, it is strictly larger than that
        /// of a previous frame with nonzero duration. These values are only available
        /// if have_timecodes in JxlAnimationHeader is JXL_TRUE.
        /// This value is only used if have_timecodes in JxlAnimationHeader is
        /// JXL_TRUE.
        /// </summary>
        public uint Timecode;

        /// <summary>
        /// The frame name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Indicates this is the last animation frame. This value is set by the
        /// decoder to indicate no further frames follow. For the encoder, it is not
        /// required to set this value and it is ignored, <see cref="JxlEncoderCloseFrames"/> is
        /// used to indicate the last frame to the encoder instead.
        /// </summary>
        public bool IsLast;

        /// <summary>
        /// Information about the layer in case of no coalescing.
        /// </summary>
        public JxlLayerInfo LayerInfo;
    }

    /// <summary>
    /// Given type of an extra channel.
    /// </summary>
    public enum JxlExtraChannelType
    {
        Alpha,
        Depth,
        SpotColor,
        SelectionMask,
        Black,
        Cfa,
        Thermal,
        Reserved0,
        Reserved1,
        Reserved2,
        Reserved3,
        Reserved4,
        Reserved5,
        Reserved6,
        Reserved7,
        Unknown,
        Optional
    }

    /// <summary>
    /// Information for a single extra channel.
    /// </summary>
    public class JxlExtraChannelInfo
    {
        /// <summary>
        /// Given type of an extra channel.
        /// </summary>
        public JxlExtraChannelType Type;

        /// <summary>
        /// Total bits per sample for this channel.
        /// </summary>
        public int BitsPerSample;

        /// <summary>
        /// Floating point exponent bits per channel, or 0 if they are unsigned
        /// integer.
        /// </summary>
        public int ExponentBitsPerSample;

        /// <summary>
        /// The exponent the channel is downsampled by on each axis.
        /// TODO(lode): expand this comment to match the JPEG XL specification,
        /// specify how to upscale, how to round the size computation, and to which
        /// extra channels this field applies.
        /// </summary>
        public int DimShift;

        /// <summary>
        /// Length of the extra channel name in bytes, or 0 if no name.
        /// Excludes null termination character.
        /// </summary>
        public int NameLength;

        /// <summary>
        /// Whether alpha channel uses premultiplied alpha. Only applicable if
        /// type is JXL_CHANNEL_ALPHA.
        /// </summary>
        public bool AlphaPremultiplied;

        /// <summary>
        /// Spot color of the current spot channel in linear RGBA. Only applicable if
        /// type is JXL_CHANNEL_SPOT_COLOR.
        /// Array size is 4.
        /// </summary>
        public float[] SpotColor = new float[4];

        /// <summary>
        /// Only applicable if type is JXL_CHANNEL_CFA.
        /// TODO(lode): add comment about the meaning of this field.
        /// </summary>
        public int CfaChannel;
    }

    /// <summary>
    /// Defines which color profile to get: the profile from the codestream
    /// metadata header, which represents the color profile of the original image,
    /// or the color profile from the pixel data produced by the decoder. Both are
    /// the same if the JxlBasicInfo has uses_original_profile set.
    /// </summary>
    public enum JxlColorProfileTarget
    {
        /// <summary>
        /// Get the color profile of the original image from the metadata.
        /// </summary>
        Original = 0,

        /// <summary>
        /// Get the color profile of the pixel data the decoder outputs. 
        /// </summary>
        TargetData = 1,
    }
}
