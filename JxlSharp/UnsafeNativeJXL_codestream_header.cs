using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace JxlSharp
{
    //We declare a type to equal itself because of intellisense
    //otherwise intellisense would display the basic type under a different name
    //example: All "int" types would suddenly be displayed as JXL_BOOL if we didn't do this
    using UInt32 = UInt32;
    using Int32 = Int32;
    using IntPtr = IntPtr;
    using UIntPtr = UIntPtr;
    using Byte = Byte;
    using UInt64 = UInt64;

    //typedefs for C types
    using int32_t = Int32;
    using uint32_t = UInt32;
    using uint8_t = Byte;
    using size_t = UIntPtr;
    using JXL_BOOL = Int32;
    using uint64_t = UInt64;
    //"JxlParallelRunner" is a function pointer type
    using JxlParallelRunner = IntPtr;

    internal unsafe partial class UnsafeNativeJXL
    {
        /// <summary>
        /// Image orientation metadata.
        /// Values 1..8 match the EXIF definitions.
        /// The name indicates the operation to perform to transform from the encoded
        /// image to the display image.
        /// </summary>
        internal enum JxlOrientation
        {
            JXL_ORIENT_IDENTITY = 1,
            JXL_ORIENT_FLIP_HORIZONTAL = 2,
            JXL_ORIENT_ROTATE_180 = 3,
            JXL_ORIENT_FLIP_VERTICAL = 4,
            JXL_ORIENT_TRANSPOSE = 5,
            JXL_ORIENT_ROTATE_90_CW = 6,
            JXL_ORIENT_ANTI_TRANSPOSE = 7,
            JXL_ORIENT_ROTATE_90_CCW = 8,
        }

        /// <summary>
        /// Given type of an extra channel.
        /// </summary>
        internal enum JxlExtraChannelType
        {
            JXL_CHANNEL_ALPHA,
            JXL_CHANNEL_DEPTH,
            JXL_CHANNEL_SPOT_COLOR,
            JXL_CHANNEL_SELECTION_MASK,
            JXL_CHANNEL_BLACK,
            JXL_CHANNEL_CFA,
            JXL_CHANNEL_THERMAL,
            JXL_CHANNEL_RESERVED0,
            JXL_CHANNEL_RESERVED1,
            JXL_CHANNEL_RESERVED2,
            JXL_CHANNEL_RESERVED3,
            JXL_CHANNEL_RESERVED4,
            JXL_CHANNEL_RESERVED5,
            JXL_CHANNEL_RESERVED6,
            JXL_CHANNEL_RESERVED7,
            JXL_CHANNEL_UNKNOWN,
            JXL_CHANNEL_OPTIONAL
        }

        /// <summary>
        /// The codestream preview header 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlPreviewHeader
        {
            /// <summary>
            /// Preview width in pixels 
            /// </summary>
            public uint32_t xsize;

            /// <summary>
            /// Preview height in pixels 
            /// </summary>
            public uint32_t ysize;
        }

        /// <summary>
        /// The intrinsic size header 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlIntrinsicSizeHeader
        {
            /// <summary>
            /// Intrinsic width in pixels 
            /// </summary>
            public uint32_t xsize;

            /// <summary>
            /// Intrinsic height in pixels 
            /// </summary>
            public uint32_t ysize;
        }

        /// <summary>
        /// The codestream animation header, optionally present in the beginning of
        /// the codestream, and if it is it applies to all animation frames, unlike
        /// JxlFrameHeader which applies to an individual frame.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlAnimationHeader
        {
            /// <summary>
            /// Numerator of ticks per second of a single animation frame time unit 
            /// </summary>
            public uint32_t tps_numerator;

            /// <summary>
            /// Denominator of ticks per second of a single animation frame time unit 
            /// </summary>
            public uint32_t tps_denominator;

            /// <summary>
            /// Amount of animation loops, or 0 to repeat infinitely 
            /// </summary>
            public uint32_t num_loops;

            /// <summary>
            /// Whether animation time codes are present at animation frames in the
            /// codestream 
            /// </summary>
            public JXL_BOOL have_timecodes;
        }

        /// <summary>
        /// Basic image information. This information is available from the file
        /// signature and first part of the codestream header.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlBasicInfo
        {
            /* TODO(lode): need additional fields for (transcoded) JPEG? For reusable
             * fields orientation must be read from Exif APP1. For has_icc_profile: must
             * look up where ICC profile is guaranteed to be in a JPEG file to be able to
             * indicate this. */

            /* TODO(lode): make struct packed, and/or make this opaque struct with getter
             * functions (still separate struct from opaque decoder) */

            /// <summary>
            /// Whether the codestream is embedded in the container format. If true,
            /// metadata information and extensions may be available in addition to the
            /// codestream.
            /// </summary>
            public JXL_BOOL have_container;

            /// <summary>
            /// Width of the image in pixels, before applying orientation.
            /// </summary>
            public uint32_t xsize;

            /// <summary>
            /// Height of the image in pixels, before applying orientation.
            /// </summary>
            public uint32_t ysize;

            /// <summary>
            /// Original image color channel bit depth.
            /// </summary>
            public uint32_t bits_per_sample;

            /// <summary>
            /// Original image color channel floating point exponent bits, or 0 if they
            /// are unsigned integer. For example, if the original data is half-precision
            /// (binary16) floating point, bits_per_sample is 16 and
            /// exponent_bits_per_sample is 5, and so on for other floating point
            /// precisions.
            /// </summary>
            public uint32_t exponent_bits_per_sample;

            /// <summary>
            /// Upper bound on the intensity level present in the image in nits. For
            /// unsigned integer pixel encodings, this is the brightness of the largest
            /// representable value. The image does not necessarily contain a pixel
            /// actually this bright. An encoder is allowed to set 255 for SDR images
            /// without computing a histogram.
            /// Leaving this set to its default of 0 lets libjxl choose a sensible default
            /// value based on the color encoding.
            /// </summary>
            public float intensity_target;

            /// <summary>
            /// Lower bound on the intensity level present in the image. This may be
            /// loose, i.e. lower than the actual darkest pixel. When tone mapping, a
            /// decoder will map [min_nits, intensity_target] to the display range.
            /// </summary>
            public float min_nits;

            /// <summary>
            /// See the description of <see cref="linear_below"/>.
            /// </summary>
            public JXL_BOOL relative_to_max_display;

            /// <summary>
            /// The tone mapping will leave unchanged (linear mapping) any pixels whose
            /// brightness is strictly below this. The interpretation depends on
            /// relative_to_max_display. If true, this is a ratio [0, 1] of the maximum
            /// display brightness [nits], otherwise an absolute brightness [nits].
            /// </summary>
            public float linear_below;

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
            public JXL_BOOL uses_original_profile;

            /// <summary>
            /// Indicates a preview image exists near the beginning of the codestream.
            /// The preview itself or its dimensions are not included in the basic info.
            /// </summary>
            public JXL_BOOL have_preview;

            /// <summary>
            /// Indicates animation frames exist in the codestream. The animation
            /// information is not included in the basic info.
            /// </summary>
            public JXL_BOOL have_animation;

            /// <summary>
            /// Image orientation, value 1-8 matching the values used by JEITA CP-3451C
            /// (Exif version 2.3).
            /// </summary>
            public JxlOrientation orientation;

            /// <summary>
            /// Number of color channels encoded in the image, this is either 1 for
            /// grayscale data, or 3 for colored data. This count does not include
            /// the alpha channel or other extra channels. To check presence of an alpha
            /// channel, such as in the case of RGBA color, check alpha_bits != 0.
            /// If and only if this is 1, the JxlColorSpace in the JxlColorEncoding is
            /// JXL_COLOR_SPACE_GRAY.
            /// </summary>
            public uint32_t num_color_channels;

            /// <summary>
            /// Number of additional image channels. This includes the main alpha channel,
            /// but can also include additional channels such as depth, additional alpha
            /// channels, spot colors, and so on. Information about the extra channels
            /// can be queried with JxlDecoderGetExtraChannelInfo. The main alpha channel,
            /// if it exists, also has its information available in the alpha_bits,
            /// alpha_exponent_bits and alpha_premultiplied fields in this JxlBasicInfo.
            /// </summary>
            public uint32_t num_extra_channels;

            /// <summary>
            /// Bit depth of the encoded alpha channel, or 0 if there is no alpha channel.
            /// If present, matches the alpha_bits value of the JxlExtraChannelInfo
            /// associated with this alpha channel.
            /// </summary>
            public uint32_t alpha_bits;

            /// <summary>
            /// Alpha channel floating point exponent bits, or 0 if they are unsigned. If
            /// present, matches the alpha_bits value of the JxlExtraChannelInfo associated
            /// with this alpha channel. integer.
            /// </summary>
            public uint32_t alpha_exponent_bits;

            /// <summary>
            /// Whether the alpha channel is premultiplied. Only used if there is a main
            /// alpha channel. Matches the alpha_premultiplied value of the
            /// JxlExtraChannelInfo associated with this alpha channel.
            /// </summary>
            public JXL_BOOL alpha_premultiplied;

            /// <summary>
            /// Dimensions of encoded preview image, only used if have_preview is
            /// JXL_TRUE.
            /// </summary>
            public JxlPreviewHeader preview;

            /// <summary>
            /// Animation header with global animation properties for all frames, only
            /// used if have_animation is JXL_TRUE.
            /// </summary>
            public JxlAnimationHeader animation;

            /// <summary>
            /// Intrinsic width of the image.
            /// The intrinsic size can be different from the actual size in pixels
            /// (as given by xsize and ysize) and it denotes the recommended dimensions
            /// for displaying the image, i.e. applications are advised to resample the
            /// decoded image to the intrinsic dimensions.
            /// </summary>
            public uint32_t intrinsic_xsize;

            /// <summary>
            /// Intrinsic height of the image.
            /// The intrinsic size can be different from the actual size in pixels
            /// (as given by xsize and ysize) and it denotes the recommended dimensions
            /// for displaying the image, i.e. applications are advised to resample the
            /// decoded image to the intrinsic dimensions.
            /// </summary>
            public uint32_t intrinsic_ysize;

            /// <summary>
            /// Padding for forwards-compatibility, in case more fields are exposed
            /// in a future version of the library.
            /// </summary>
            private fixed uint8_t padding[100];

            internal void ReadFromPublic(JxlSharp.JxlBasicInfo basicInfo)
            {
                this.have_container = Convert.ToInt32(basicInfo.HaveContainer);
                this.xsize = (uint)basicInfo.Width;
                this.ysize = (uint)basicInfo.Height;
                this.bits_per_sample = (uint)basicInfo.BitsPerSample;
                this.exponent_bits_per_sample = (uint)basicInfo.ExponentBitsPerSample;
                this.intensity_target = basicInfo.IntensityTarget;
                this.min_nits = basicInfo.MinNits;
                this.relative_to_max_display = Convert.ToInt32(basicInfo.RelativeToMaxDisplay);
                this.linear_below = basicInfo.LinearBelow;
                this.uses_original_profile = Convert.ToInt32(basicInfo.UsesOriginalProfile);
                this.have_preview = Convert.ToInt32(basicInfo.HavePreview);
                this.have_animation = Convert.ToInt32(basicInfo.HaveAnimation);
                this.orientation = (UnsafeNativeJXL.JxlOrientation)basicInfo.Orientation;
                this.num_color_channels = (uint)basicInfo.NumColorChannels;
                this.num_extra_channels = (uint)basicInfo.NumExtraChannels;
                this.alpha_bits = (uint)basicInfo.AlphaBits;
                this.alpha_exponent_bits = (uint)basicInfo.AlphaExponentBits;
                this.alpha_premultiplied = Convert.ToInt32(basicInfo.AlphaPremultiplied);
                this.preview.xsize = (uint)basicInfo.Preview.Width;
                this.preview.ysize = (uint)basicInfo.Preview.Height;
                this.animation.tps_numerator = basicInfo.Animation.TpsNumerator;
                this.animation.tps_denominator = basicInfo.Animation.TpsDenominator;
                this.animation.num_loops = (uint)basicInfo.Animation.NumLoops;
                this.animation.have_timecodes = Convert.ToInt32(basicInfo.Animation.HaveTimecodes);
                this.intrinsic_xsize = (uint)basicInfo.IntrinsicWidth;
                this.intrinsic_ysize = (uint)basicInfo.IntrinsicHeight;
            }

            internal void WriteToPublic(JxlSharp.JxlBasicInfo basicInfo)
            {
                basicInfo.HaveContainer = Convert.ToBoolean(this.have_container);
                basicInfo.Width = (int)this.xsize;
                basicInfo.Height = (int)this.ysize;
                basicInfo.BitsPerSample = (int)this.bits_per_sample;
                basicInfo.ExponentBitsPerSample = (int)this.exponent_bits_per_sample;
                basicInfo.IntensityTarget = this.intensity_target;
                basicInfo.MinNits = this.min_nits;
                basicInfo.RelativeToMaxDisplay = Convert.ToBoolean(this.relative_to_max_display);
                basicInfo.LinearBelow = this.linear_below;
                basicInfo.UsesOriginalProfile = Convert.ToBoolean(this.uses_original_profile);
                basicInfo.HavePreview = Convert.ToBoolean(this.have_preview);
                basicInfo.HaveAnimation = Convert.ToBoolean(this.have_animation);
                basicInfo.Orientation = (JxlSharp.JxlOrientation)this.orientation;
                basicInfo.NumColorChannels = (int)this.num_color_channels;
                basicInfo.NumExtraChannels = (int)this.num_extra_channels;
                basicInfo.AlphaBits = (int)this.alpha_bits;
                basicInfo.AlphaExponentBits = (int)this.alpha_exponent_bits;
                basicInfo.AlphaPremultiplied = Convert.ToBoolean(this.alpha_premultiplied);
                var preview = basicInfo.Preview;
                preview.Width = (int)this.preview.xsize;
                preview.Height = (int)this.preview.ysize;
                basicInfo.Preview = preview;
                var animation = basicInfo.Animation;
                animation.TpsNumerator = this.animation.tps_numerator;
                animation.TpsDenominator = this.animation.tps_denominator;
                animation.NumLoops = (int)this.animation.num_loops;
                animation.HaveTimecodes = Convert.ToBoolean(this.animation.have_timecodes);
                basicInfo.Animation = animation;
                basicInfo.IntrinsicWidth = (int)this.intrinsic_xsize;
                basicInfo.IntrinsicHeight = (int)this.intrinsic_ysize;
            }
        };

        /// <summary>
        /// Information for a single extra channel.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlExtraChannelInfo
        {
            /// <summary>
            /// Given type of an extra channel.
            /// </summary>
            public JxlExtraChannelType type;

            /// <summary>
            /// Total bits per sample for this channel.
            /// </summary>
            public uint32_t bits_per_sample;

            /// <summary>
            /// Floating point exponent bits per channel, or 0 if they are unsigned
            /// integer.
            /// </summary>
            public uint32_t exponent_bits_per_sample;

            /// <summary>
            /// The exponent the channel is downsampled by on each axis.
            /// TODO(lode): expand this comment to match the JPEG XL specification,
            /// specify how to upscale, how to round the size computation, and to which
            /// extra channels this field applies.
            /// </summary>
            public uint32_t dim_shift;

            /// <summary>
            /// Length of the extra channel name in bytes, or 0 if no name.
            /// Excludes null termination character.
            /// </summary>
            public uint32_t name_length;

            /// <summary>
            /// Whether alpha channel uses premultiplied alpha. Only applicable if
            /// type is JXL_CHANNEL_ALPHA.
            /// </summary>
            public JXL_BOOL alpha_premultiplied;

            /// <summary>
            /// Spot color of the current spot channel in linear RGBA. Only applicable if
            /// type is JXL_CHANNEL_SPOT_COLOR.
            /// </summary>
            public fixed float spot_color[4];

            /// <summary>
            /// Only applicable if type is JXL_CHANNEL_CFA.
            /// TODO(lode): add comment about the meaning of this field.
            /// </summary>
            public uint32_t cfa_channel;

			public void WriteToPublic(JxlSharp.JxlExtraChannelInfo info)
			{
                info.AlphaPremultiplied = Convert.ToBoolean(this.alpha_premultiplied);
                info.BitsPerSample = (int)this.bits_per_sample;
                info.CfaChannel = (int)this.cfa_channel;
                info.DimShift = (int)this.dim_shift;
                info.ExponentBitsPerSample = (int)this.exponent_bits_per_sample;
                info.NameLength = (int)this.name_length;
                unsafe
                {
                    info.SpotColor[0] = this.spot_color[0];
                    info.SpotColor[1] = this.spot_color[1];
                    info.SpotColor[2] = this.spot_color[2];
                    info.SpotColor[3] = this.spot_color[3];
                }
                info.Type = (JxlSharp.JxlExtraChannelType)this.type;
            }
		}

        /* TODO(lode): add API to get the codestream header extensions. */
        /// <summary>
        /// Extensions in the codestream header. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlHeaderExtensions
        {
            /// <summary>
            /// Extension bits. 
            /// </summary>
            public uint64_t extensions;
        }

        /// <summary>
        /// Frame blend modes.
        /// When decoding, if coalescing is enabled (default), this can be ignored.
        /// </summary>
        internal enum JxlBlendMode
        {
            JXL_BLEND_REPLACE = 0,
            JXL_BLEND_ADD = 1,
            JXL_BLEND_BLEND = 2,
            JXL_BLEND_MULADD = 3,
            JXL_BLEND_MUL = 4,
        }

        /// <summary>
        /// The information about blending the color channels or a single extra channel.
        /// When decoding, if coalescing is enabled (default), this can be ignored and
        /// the blend mode is considered to be JXL_BLEND_REPLACE.
        /// When encoding, these settings apply to the pixel data given to the encoder.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlBlendInfo
        {
            /// <summary>
            /// Blend mode.
            /// </summary>
            public JxlBlendMode blendmode;
            /// <summary>
            /// Reference frame ID to use as the 'bottom' layer (0-3).
            /// </summary>
            public uint32_t source;
            /// <summary>
            /// Which extra channel to use as the 'alpha' channel for blend modes
            /// JXL_BLEND_BLEND and JXL_BLEND_MULADD.
            /// </summary>
            public uint32_t alpha;
            /// <summary>
            /// Clamp values to [0,1] for the purpose of blending.
            /// </summary>
            public JXL_BOOL clamp;
        }

        /// <summary>
        /// The information about layers.
        /// When decoding, if coalescing is enabled (default), this can be ignored.
        /// When encoding, these settings apply to the pixel data given to the encoder,
        /// the encoder could choose an internal representation that differs.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlLayerInfo
        {
            /// <summary>
            /// Whether cropping is applied for this frame. When decoding, if false,
            /// crop_x0 and crop_y0 are set to zero, and xsize and ysize to the main
            /// image dimensions. When encoding and this is false, those fields are
            /// ignored. When decoding, if coalescing is enabled (default), this is always
            /// false, regardless of the internal encoding in the JPEG XL codestream.
            /// </summary>
            public JXL_BOOL have_crop;

            /// <summary>
            /// Horizontal offset of the frame (can be negative).
            /// </summary>
            public int32_t crop_x0;

            /// <summary>
            /// Vertical offset of the frame (can be negative).
            /// </summary>
            public int32_t crop_y0;

            /// <summary>
            /// Width of the frame (number of columns).
            /// </summary>
            public uint32_t xsize;

            /// <summary>
            /// Height of the frame (number of rows).
            /// </summary>
            public uint32_t ysize;

            /// <summary>
            /// The blending info for the color channels. Blending info for extra channels
            /// has to be retrieved separately using JxlDecoderGetExtraChannelBlendInfo.
            /// </summary>
            public JxlBlendInfo blend_info;

            /// <summary>
            /// After blending, save the frame as reference frame with this ID (0-3).
            /// Special case: if the frame duration is nonzero, ID 0 means "will not be
            /// referenced in the future". This value is not used for the last frame.
            /// </summary>
            public uint32_t save_as_reference;
        }

        /// <summary>
        /// The header of one displayed frame or non-coalesced layer. 
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlFrameHeader
        {
            /// <summary>
            /// How long to wait after rendering in ticks. The duration in seconds of a
            /// tick is given by tps_numerator and tps_denominator in JxlAnimationHeader.
            /// </summary>
            public uint32_t duration;

            /// <summary>
            /// SMPTE timecode of the current frame in form 0xHHMMSSFF, or 0. The bits are
            /// interpreted from most-significant to least-significant as hour, minute,
            /// second, and frame. If timecode is nonzero, it is strictly larger than that
            /// of a previous frame with nonzero duration. These values are only available
            /// if have_timecodes in JxlAnimationHeader is JXL_TRUE.
            /// This value is only used if have_timecodes in JxlAnimationHeader is
            /// JXL_TRUE.
            /// </summary>
            public uint32_t timecode;

            /// <summary>
            /// Length of the frame name in bytes, or 0 if no name.
            /// Excludes null termination character. This value is set by the decoder.
            /// For the encoder, this value is ignored and <see cref="JxlEncoderSetFrameName"/> is
            /// used instead to set the name and the length.
            /// </summary>
            public uint32_t name_length;

            /// <summary>
            /// Indicates this is the last animation frame. This value is set by the
            /// decoder to indicate no further frames follow. For the encoder, it is not
            /// required to set this value and it is ignored, <see cref="JxlEncoderCloseFrames"/> is
            /// used to indicate the last frame to the encoder instead.
            /// </summary>
            public JXL_BOOL is_last;

            /// <summary>
            /// Information about the layer in case of no coalescing.
            /// </summary>
            public JxlLayerInfo layer_info;

			public void WriteToPublic(JxlSharp.JxlFrameHeader header)
			{
                header.Duration = this.duration;
                header.Timecode = this.timecode;
                //header.NameLength = (int)this.name_length;
                header.IsLast = Convert.ToBoolean(this.is_last);
                header.LayerInfo.HaveCrop = Convert.ToBoolean(this.layer_info.have_crop);
                header.LayerInfo.CropX0 = this.layer_info.crop_x0;
                header.LayerInfo.CropY0 = this.layer_info.crop_y0;
                header.LayerInfo.Width = (int)this.layer_info.xsize;
                header.LayerInfo.Height = (int)this.layer_info.ysize;
                header.LayerInfo.BlendInfo.BlendMode = (JxlSharp.JxlBlendMode)this.layer_info.blend_info.blendmode;
                header.LayerInfo.BlendInfo.Source = (int)this.layer_info.blend_info.source;
                header.LayerInfo.BlendInfo.Alpha = (int)this.layer_info.blend_info.alpha;
                header.LayerInfo.BlendInfo.Clamp= Convert.ToBoolean(this.layer_info.blend_info.clamp);
                header.LayerInfo.SaveAsReference = (int)this.layer_info.save_as_reference;
            }
        }
    }
}
