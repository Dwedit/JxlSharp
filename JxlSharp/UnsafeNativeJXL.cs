using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JxlSharp
{
	internal static partial class UnsafeNativeJxl
	{
		internal struct OneHundredBytes
		{
			public int i0;

			public int i1;

			public int i2;

			public int i3;

			public int i4;

			public int i5;

			public int i6;

			public int i7;

			public int i8;

			public int i9;

			public int i10;

			public int i11;

			public int i12;

			public int i13;

			public int i14;

			public int i15;

			public int i16;

			public int i17;

			public int i18;

			public int i19;

			public int i20;

			public int i21;

			public int i22;

			public int i23;

			public int i24;
		}

		internal struct RGBAFloat
		{
			public float r;

			public float g;

			public float b;

			public float a;
		}

		internal struct XYValue
		{
			public double x;

			public double y;
		}

		/// <summary>
		/// Opaque structure for a memory manager, do not create this, pass in null instead.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
		internal struct JxlMemoryManager
		{
		}

		/// <summary>
		/// Data type for the sample values per channel per pixel.
		/// </summary>
		internal enum JxlDataType
		{
			/// <summary>
			/// Use 32-bit single-precision floating point values, with range 0.0-1.0
			/// (within gamut, may go outside this range for wide color gamut). Floating
			/// point output, either JXL_TYPE_FLOAT or JXL_TYPE_FLOAT16, is recommended
			/// for HDR and wide gamut images when color profile conversion is required. 
			/// </summary>
			JXL_TYPE_FLOAT = 0,
			/// <summary>
			/// Use type uint8_t. May clip wide color gamut data.
			/// </summary>
			JXL_TYPE_UINT8 = 2,
			/// <summary>
			/// Use type uint16_t. May clip wide color gamut data.
			/// </summary>
			JXL_TYPE_UINT16 = 3,
			/// <summary>
			/// Use 16-bit IEEE 754 half-precision floating point values 
			/// </summary>
			JXL_TYPE_FLOAT16 = 5
		}

		/// <summary>
		/// Ordering of multi-byte data.
		/// </summary>
		internal enum JxlEndianness
		{
			/// <summary>
			/// Use the endianness of the system, either little endian or big endian,
			/// without forcing either specific endianness. Do not use if pixel data
			/// should be exported to a well defined format.
			/// </summary>
			JXL_NATIVE_ENDIAN,
			/// <summary>
			/// Force little endian 
			/// </summary>
			JXL_LITTLE_ENDIAN,
			/// <summary>
			/// Force big endian 
			/// </summary>
			JXL_BIG_ENDIAN
		}

		/// <summary>
		/// Data type for the sample values per channel per pixel for the output buffer
		/// for pixels. This is not necessarily the same as the data type encoded in the
		/// codestream. The channels are interleaved per pixel. The pixels are
		/// organized row by row, left to right, top to bottom.
		/// TODO(lode): implement padding / alignment (row stride)
		/// TODO(lode): support different channel orders if needed (RGB, BGR, ...)
		/// </summary>
		internal struct JxlPixelFormat
		{
			/// <summary>
			/// Amount of channels available in a pixel buffer.
			/// 1: single-channel data, e.g. grayscale or a single extra channel
			/// 2: single-channel + alpha
			/// 3: trichromatic, e.g. RGB
			/// 4: trichromatic + alpha
			/// TODO(lode): this needs finetuning. It is not yet defined how the user
			/// chooses output color space. CMYK+alpha needs 5 channels.
			/// </summary>
			public uint num_channels;

			/// <summary>
			/// Data type of each channel.
			/// </summary>
			public JxlDataType data_type;

			/// <summary>
			/// Whether multi-byte data types are represented in big endian or little
			/// endian format. This applies to JXL_TYPE_UINT16, JXL_TYPE_UINT32
			/// and JXL_TYPE_FLOAT.
			/// </summary>
			public JxlEndianness endianness;

			/// <summary>
			/// Align scanlines to a multiple of align bytes, or 0 to require no
			/// alignment at all (which has the same effect as value 1)
			/// </summary>
			public UIntPtr align;
		}

		/// <summary>
		/// Types of progressive detail.
		/// Setting a progressive detail with value N implies all progressive details
		/// with smaller or equal value. Currently only the following level of
		/// progressive detail is implemented:
		/// <br /> - kDC (which implies kFrames)
		/// <br /> - kLastPasses (which implies kDC and kFrames)
		/// <br /> - kPasses (which implies kLastPasses, kDC and kFrames)
		/// </summary>
		internal enum JxlProgressiveDetail
		{
			/// <summary>
			/// after completed kRegularFrames
			/// </summary>
			kFrames,
			/// <summary>
			/// after completed DC (1:8)
			/// </summary>
			kDC,
			/// <summary>
			/// after completed AC passes that are the last pass for their resolution target.
			/// </summary>
			kLastPasses,
			/// <summary>
			/// after completed AC passes that are not the last pass for their resolution target.
			/// </summary>
			kPasses,
			/// <summary>
			/// during DC frame when lower resolution are completed (1:32, 1:16)
			/// </summary>
			kDCProgressive,
			/// <summary>
			/// after completed DC groups
			/// </summary>
			kDCGroups,
			/// <summary>
			/// after completed groups
			/// </summary>
			kGroups
		}

		/// <summary>
		/// Color space of the image data. 
		/// </summary>
		internal enum JxlColorSpace
		{
			/// <summary>
			/// Tristimulus RGB 
			/// </summary>
			JXL_COLOR_SPACE_RGB,
			/// <summary>
			/// Luminance based, the primaries in JxlColorEncoding must be ignored. This
			/// value implies that num_color_channels in JxlBasicInfo is 1, any other value
			/// implies num_color_channels is 3. 
			/// </summary>
			JXL_COLOR_SPACE_GRAY,
			/// <summary>
			/// XYB (opsin) color space 
			/// </summary>
			JXL_COLOR_SPACE_XYB,
			/// <summary>
			/// None of the other table entries describe the color space appropriately 
			/// </summary>
			JXL_COLOR_SPACE_UNKNOWN
		}

		/// <summary>
		/// Built-in whitepoints for color encoding. When decoding, the numerical xy
		/// whitepoint value can be read from the JxlColorEncoding white_point field
		/// regardless of the enum value. When encoding, enum values except
		/// JXL_WHITE_POINT_CUSTOM override the numerical fields. Some enum values match
		/// a subset of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)), however the
		/// white point and RGB primaries are separate enums here.
		/// </summary>
		internal enum JxlWhitePoint
		{
			/// <summary>
			/// CIE Standard Illuminant D65: 0.3127, 0.3290 
			/// </summary>
			JXL_WHITE_POINT_D65 = 1,
			/// <summary>
			/// White point must be read from the JxlColorEncoding white_point field, or
			/// as ICC profile. This enum value is not an exact match of the corresponding
			/// CICP value. 
			/// </summary>
			JXL_WHITE_POINT_CUSTOM = 2,
			/// <summary>
			/// CIE Standard Illuminant E (equal-energy): 1/3, 1/3 
			/// </summary>
			JXL_WHITE_POINT_E = 10,
			/// <summary>
			/// DCI-P3 from SMPTE RP 431-2: 0.314, 0.351 
			/// </summary>
			JXL_WHITE_POINT_DCI = 11
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
		internal enum JxlPrimaries
		{
			/// <summary>
			/// The CIE xy values of the red, green and blue primaries are: 0.639998686,
			/// 0.330010138; 0.300003784, 0.600003357; 0.150002046, 0.059997204 
			/// </summary>
			JXL_PRIMARIES_SRGB = 1,
			/// <summary>
			/// Primaries must be read from the JxlColorEncoding primaries_red_xy,
			/// primaries_green_xy and primaries_blue_xy fields, or as ICC profile. This
			/// enum value is not an exact match of the corresponding CICP value. 
			/// </summary>
			JXL_PRIMARIES_CUSTOM = 2,
			/// <summary>
			/// As specified in Rec. ITU-R BT.2100-1 
			/// </summary>
			JXL_PRIMARIES_2100 = 9,
			/// <summary>
			/// As specified in SMPTE RP 431-2 
			/// </summary>
			JXL_PRIMARIES_P3 = 11
		}

		/// <summary>
		/// Built-in transfer functions for color encoding. Enum values match a subset
		/// of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)) unless specified
		/// otherwise. 
		/// </summary>
		internal enum JxlTransferFunction
		{
			/// <summary>
			/// As specified in SMPTE RP 431-2 
			/// </summary>
			JXL_TRANSFER_FUNCTION_709 = 1,
			/// <summary>
			/// None of the other table entries describe the transfer function. 
			/// </summary>
			JXL_TRANSFER_FUNCTION_UNKNOWN = 2,
			/// <summary>
			/// The gamma exponent is 1 
			/// </summary>
			JXL_TRANSFER_FUNCTION_LINEAR = 8,
			/// <summary>
			/// As specified in IEC 61966-2-1 sRGB 
			/// </summary>
			JXL_TRANSFER_FUNCTION_SRGB = 13,
			/// <summary>
			/// As specified in SMPTE ST 2084 
			/// </summary>
			JXL_TRANSFER_FUNCTION_PQ = 16,
			/// <summary>
			/// As specified in SMPTE ST 428-1 
			/// </summary>
			JXL_TRANSFER_FUNCTION_DCI = 17,
			/// <summary>
			/// As specified in Rec. ITU-R BT.2100-1 (HLG) 
			/// </summary>
			JXL_TRANSFER_FUNCTION_HLG = 18,
			/// <summary>
			/// Transfer function follows power law given by the gamma value in
			/// JxlColorEncoding. Not a CICP value. 
			/// </summary>
			JXL_TRANSFER_FUNCTION_GAMMA = 65535
		}

		/// <summary>
		/// Renderig intent for color encoding, as specified in ISO 15076-1:2010 
		/// </summary>
		internal enum JxlRenderingIntent
		{
			/// <summary>
			/// vendor-specific 
			/// </summary>
			JXL_RENDERING_INTENT_PERCEPTUAL,
			/// <summary>
			/// media-relative 
			/// </summary>
			JXL_RENDERING_INTENT_RELATIVE,
			/// <summary>
			/// vendor-specific 
			/// </summary>
			JXL_RENDERING_INTENT_SATURATION,
			/// <summary>
			/// ICC-absolute 
			/// </summary>
			JXL_RENDERING_INTENT_ABSOLUTE
		}

		/// <summary>
		/// Color encoding of the image as structured information.
		/// </summary>
		internal struct JxlColorEncoding
		{
			/// <summary>
			/// Color space of the image data.
			/// </summary>
			public JxlColorSpace color_space;

			/// <summary>
			/// Built-in white point. If this value is JXL_WHITE_POINT_CUSTOM, must
			/// use the numerical whitepoint values from white_point_xy.
			/// </summary>
			public JxlWhitePoint white_point;

			/// <summary>
			/// Numerical whitepoint values in CIE xy space. 
			/// </summary>
			public XYValue white_point_xy;

			/// <summary>
			/// Built-in RGB primaries. If this value is JXL_PRIMARIES_CUSTOM, must
			/// use the numerical primaries values below. This field and the custom values
			/// below are unused and must be ignored if the color space is
			/// JXL_COLOR_SPACE_GRAY or JXL_COLOR_SPACE_XYB.
			/// </summary>
			public JxlPrimaries primaries;

			/// <summary>
			/// Numerical red primary values in CIE xy space. 
			/// </summary>
			public XYValue primaries_red_xy;

			/// <summary>
			/// Numerical green primary values in CIE xy space. 
			/// </summary>
			public XYValue primaries_green_xy;

			/// <summary>
			/// Numerical blue primary values in CIE xy space. 
			/// </summary>
			public XYValue primaries_blue_xy;

			/// <summary>
			/// Transfer function if have_gamma is 0 
			/// </summary>
			public JxlTransferFunction transfer_function;

			/// <summary>
			/// Gamma value used when transfer_function is JXL_TRANSFER_FUNCTION_GAMMA
			/// </summary>
			public double gamma;

			/// <summary>
			/// Rendering intent defined for the color profile. 
			/// </summary>
			public JxlRenderingIntent rendering_intent;
		}

		/// <summary>
		/// Represents an input or output colorspace to a color transform, as a
		/// serialized ICC profile. 
		/// </summary>
		internal struct JxlColorProfile
		{
			/// <summary>
			/// Structured representation of the colorspace, if applicable. If all fields
			/// are different from their "unknown" value, then this is equivalent to the
			/// ICC representation of the colorspace. If some are "unknown", those that are
			/// not are still valid and can still be used on their own if they are useful.
			/// </summary>
			public JxlColorEncoding color_encoding;

			/// <summary>
			/// Number of components per pixel. This can be deduced from the other
			/// representations of the colorspace but is provided for convenience and
			/// validation. 
			/// </summary>
			public UIntPtr num_channels;
		}

		/// <summary>
		/// Interface for performing colorspace transforms. The <tt>init</tt> function can be
		/// called several times to instantiate several transforms, including before
		/// other transforms have been destroyed.
		/// </summary>
		internal struct JxlCmsInterface
		{
			/// <summary>
			/// CMS-specific data that will be passed to <see cref="JxlCmsInterface.init" />. 
			/// </summary>
			public unsafe void* init_data;

			/// <summary>
			/// Prepares a colorspace transform as described in the documentation of 
			/// <see cref="D:jpegxl_cms_init_func" />. 
			/// </summary>
			public UIntPtr/*delegate*<void*, UIntPtr, UIntPtr, JxlColorProfile*, JxlColorProfile*, float, void*>*/ init;

			/// <summary>
			/// Returns a buffer that can be used as input to <tt>run</tt>. 
			/// </summary>
			public UIntPtr/*delegate*<void*, UIntPtr, float*>*/ get_src_buf;

			/// <summary>
			/// Returns a buffer that can be used as output from <tt>run</tt>. 
			/// </summary>
			public UIntPtr/*delegate*<void*, UIntPtr, float*>*/ get_dst_buf;

			/// <summary>
			/// Executes the transform on a batch of pixels, per <see cref="D:jpegxl_cms_run_func" />.
			/// </summary>
			public UIntPtr/*delegate*<void*, UIntPtr, float*, float*, UIntPtr, int>*/ run;

			/// <summary>
			/// Cleans up the transform. 
			/// </summary>
			public UIntPtr/*delegate*<void*, void>*/ destroy;
		}

		/// <summary>
		/// Image orientation metadata.
		/// Values 1..8 match the EXIF definitions.
		/// The name indicates the operation to perform to transform from the encoded
		/// image to the display image.
		/// </summary>
		internal enum JxlOrientation
		{
			JXL_ORIENT_IDENTITY = 1,
			JXL_ORIENT_FLIP_HORIZONTAL,
			JXL_ORIENT_ROTATE_180,
			JXL_ORIENT_FLIP_VERTICAL,
			JXL_ORIENT_TRANSPOSE,
			JXL_ORIENT_ROTATE_90_CW,
			JXL_ORIENT_ANTI_TRANSPOSE,
			JXL_ORIENT_ROTATE_90_CCW
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
		internal struct JxlPreviewHeader
		{
			/// <summary>
			/// Preview width in pixels 
			/// </summary>
			public uint xsize;

			/// <summary>
			/// Preview height in pixels 
			/// </summary>
			public uint ysize;
		}

		/// <summary>
		/// The intrinsic size header 
		/// </summary>
		internal struct JxlIntrinsicSizeHeader
		{
			/// <summary>
			/// Intrinsic width in pixels 
			/// </summary>
			public uint xsize;

			/// <summary>
			/// Intrinsic height in pixels 
			/// </summary>
			public uint ysize;
		}

		/// <summary>
		/// The codestream animation header, optionally present in the beginning of
		/// the codestream, and if it is it applies to all animation frames, unlike
		/// JxlFrameHeader which applies to an individual frame.
		/// </summary>
		internal struct JxlAnimationHeader
		{
			/// <summary>
			/// Numerator of ticks per second of a single animation frame time unit 
			/// </summary>
			public uint tps_numerator;

			/// <summary>
			/// Denominator of ticks per second of a single animation frame time unit 
			/// </summary>
			public uint tps_denominator;

			/// <summary>
			/// Amount of animation loops, or 0 to repeat infinitely 
			/// </summary>
			public uint num_loops;

			/// <summary>
			/// Whether animation time codes are present at animation frames in the
			/// codestream 
			/// </summary>
			public int have_timecodes;
		}

		/// <summary>
		/// Defines which color profile to get: the profile from the codestream
		/// metadata header, which represents the color profile of the original image,
		/// or the color profile from the pixel data produced by the decoder. Both are
		/// the same if the JxlBasicInfo has uses_original_profile set.
		/// </summary>
		internal enum JxlColorProfileTarget
		{
			/// <summary>
			/// Get the color profile of the original image from the metadata.
			/// </summary>
			JXL_COLOR_PROFILE_TARGET_ORIGINAL,
			/// <summary>
			/// Get the color profile of the pixel data the decoder outputs. 
			/// </summary>
			JXL_COLOR_PROFILE_TARGET_DATA
		}

		/// <summary>
		/// Basic image information. This information is available from the file
		/// signature and first part of the codestream header.
		/// </summary>
		internal struct JxlBasicInfo
		{
			/// <summary>
			/// Whether the codestream is embedded in the container format. If true,
			/// metadata information and extensions may be available in addition to the
			/// codestream.
			/// </summary>
			public int have_container;

			/// <summary>
			/// Width of the image in pixels, before applying orientation.
			/// </summary>
			public uint xsize;

			/// <summary>
			/// Height of the image in pixels, before applying orientation.
			/// </summary>
			public uint ysize;

			/// <summary>
			/// Original image color channel bit depth.
			/// </summary>
			public uint bits_per_sample;

			/// <summary>
			/// Original image color channel floating point exponent bits, or 0 if they
			/// are unsigned integer. For example, if the original data is half-precision
			/// (binary16) floating point, bits_per_sample is 16 and
			/// exponent_bits_per_sample is 5, and so on for other floating point
			/// precisions.
			/// </summary>
			public uint exponent_bits_per_sample;

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
			/// See the description of <see cref="JxlBasicInfo.linear_below" />.
			/// </summary>
			public int relative_to_max_display;

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
			/// functions needs to be called with <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" /> to get
			/// the color profile of the decoder output, and then an external CMS can be
			/// used for conversion.
			/// Note that for lossy compression, this should be set to false for most use
			/// cases, and if needed, the image should be converted to the original color
			/// profile after decoding, as described above.
			/// </summary>
			public int uses_original_profile;

			/// <summary>
			/// Indicates a preview image exists near the beginning of the codestream.
			/// The preview itself or its dimensions are not included in the basic info.
			/// </summary>
			public int have_preview;

			/// <summary>
			/// Indicates animation frames exist in the codestream. The animation
			/// information is not included in the basic info.
			/// </summary>
			public int have_animation;

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
			public uint num_color_channels;

			/// <summary>
			/// Number of additional image channels. This includes the main alpha channel,
			/// but can also include additional channels such as depth, additional alpha
			/// channels, spot colors, and so on. Information about the extra channels
			/// can be queried with JxlDecoderGetExtraChannelInfo. The main alpha channel,
			/// if it exists, also has its information available in the alpha_bits,
			/// alpha_exponent_bits and alpha_premultiplied fields in this JxlBasicInfo.
			/// </summary>
			public uint num_extra_channels;

			/// <summary>
			/// Bit depth of the encoded alpha channel, or 0 if there is no alpha channel.
			/// If present, matches the alpha_bits value of the JxlExtraChannelInfo
			/// associated with this alpha channel.
			/// </summary>
			public uint alpha_bits;

			/// <summary>
			/// Alpha channel floating point exponent bits, or 0 if they are unsigned. If
			/// present, matches the alpha_bits value of the JxlExtraChannelInfo associated
			/// with this alpha channel. integer.
			/// </summary>
			public uint alpha_exponent_bits;

			/// <summary>
			/// Whether the alpha channel is premultiplied. Only used if there is a main
			/// alpha channel. Matches the alpha_premultiplied value of the
			/// JxlExtraChannelInfo associated with this alpha channel.
			/// </summary>
			public int alpha_premultiplied;

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
			public uint intrinsic_xsize;

			/// <summary>
			/// Intrinsic heigth of the image.
			/// The intrinsic size can be different from the actual size in pixels
			/// (as given by xsize and ysize) and it denotes the recommended dimensions
			/// for displaying the image, i.e. applications are advised to resample the
			/// decoded image to the intrinsic dimensions.
			/// </summary>
			public uint intrinsic_ysize;

			/// <summary>
			/// Padding for forwards-compatibility, in case more fields are exposed
			/// in a future version of the library.
			/// </summary>
			public OneHundredBytes padding;
		}

		/// <summary>
		/// Information for a single extra channel.
		/// </summary>
		internal struct JxlExtraChannelInfo
		{
			/// <summary>
			/// Given type of an extra channel.
			/// </summary>
			public JxlExtraChannelType type;

			/// <summary>
			/// Total bits per sample for this channel.
			/// </summary>
			public uint bits_per_sample;

			/// <summary>
			/// Floating point exponent bits per channel, or 0 if they are unsigned
			/// integer.
			/// </summary>
			public uint exponent_bits_per_sample;

			/// <summary>
			/// The exponent the channel is downsampled by on each axis.
			/// TODO(lode): expand this comment to match the JPEG XL specification,
			/// specify how to upscale, how to round the size computation, and to which
			/// extra channels this field applies.
			/// </summary>
			public uint dim_shift;

			/// <summary>
			/// Length of the extra channel name in bytes, or 0 if no name.
			/// Excludes null termination character.
			/// </summary>
			public uint name_length;

			/// <summary>
			/// Whether alpha channel uses premultiplied alpha. Only applicable if
			/// type is JXL_CHANNEL_ALPHA.
			/// </summary>
			public int alpha_premultiplied;

			/// <summary>
			/// Spot color of the current spot channel in linear RGBA. Only applicable if
			/// type is JXL_CHANNEL_SPOT_COLOR.
			/// </summary>
			public RGBAFloat spot_color;

			/// <summary>
			/// Only applicable if type is JXL_CHANNEL_CFA.
			/// TODO(lode): add comment about the meaning of this field.
			/// </summary>
			public uint cfa_channel;
		}

		/// <summary>
		/// Extensions in the codestream header. 
		/// </summary>
		internal struct JxlHeaderExtensions
		{
			/// <summary>
			/// Extension bits. 
			/// </summary>
			public ulong extensions;
		}

		/// <summary>
		/// Frame blend modes.
		/// When decoding, if coalescing is enabled (default), this can be ignored.
		/// </summary>
		internal enum JxlBlendMode
		{
			JXL_BLEND_REPLACE,
			JXL_BLEND_ADD,
			JXL_BLEND_BLEND,
			JXL_BLEND_MULADD,
			JXL_BLEND_MUL
		}

		/// <summary>
		/// The information about blending the color channels or a single extra channel.
		/// When decoding, if coalescing is enabled (default), this can be ignored and
		/// the blend mode is considered to be JXL_BLEND_REPLACE.
		/// When encoding, these settings apply to the pixel data given to the encoder.
		/// </summary>
		internal struct JxlBlendInfo
		{
			/// <summary>
			/// Blend mode.
			/// </summary>
			public JxlBlendMode blendmode;

			/// <summary>
			/// Reference frame ID to use as the 'bottom' layer (0-3).
			/// </summary>
			public uint source;

			/// <summary>
			/// Which extra channel to use as the 'alpha' channel for blend modes
			/// JXL_BLEND_BLEND and JXL_BLEND_MULADD.
			/// </summary>
			public uint alpha;

			/// <summary>
			/// Clamp values to [0,1] for the purpose of blending.
			/// </summary>
			public int clamp;
		}

		/// <summary>
		/// The information about layers.
		/// When decoding, if coalescing is enabled (default), this can be ignored.
		/// When encoding, these settings apply to the pixel data given to the encoder,
		/// the encoder could choose an internal representation that differs.
		/// </summary>
		internal struct JxlLayerInfo
		{
			/// <summary>
			/// Whether cropping is applied for this frame. When decoding, if false,
			/// crop_x0 and crop_y0 are set to zero, and xsize and ysize to the main
			/// image dimensions. When encoding and this is false, those fields are
			/// ignored. When decoding, if coalescing is enabled (default), this is always
			/// false, regardless of the internal encoding in the JPEG XL codestream.
			/// </summary>
			public int have_crop;

			/// <summary>
			/// Horizontal offset of the frame (can be negative).
			/// </summary>
			public int crop_x0;

			/// <summary>
			/// Vertical offset of the frame (can be negative).
			/// </summary>
			public int crop_y0;

			/// <summary>
			/// Width of the frame (number of columns).
			/// </summary>
			public uint xsize;

			/// <summary>
			/// Height of the frame (number of rows).
			/// </summary>
			public uint ysize;

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
			public uint save_as_reference;
		}

		/// <summary>
		/// The header of one displayed frame or non-coalesced layer. 
		/// </summary>
		internal struct JxlFrameHeader
		{
			/// <summary>
			/// How long to wait after rendering in ticks. The duration in seconds of a
			/// tick is given by tps_numerator and tps_denominator in JxlAnimationHeader.
			/// </summary>
			public uint duration;

			/// <summary>
			/// SMPTE timecode of the current frame in form 0xHHMMSSFF, or 0. The bits are
			/// interpreted from most-significant to least-significant as hour, minute,
			/// second, and frame. If timecode is nonzero, it is strictly larger than that
			/// of a previous frame with nonzero duration. These values are only available
			/// if have_timecodes in JxlAnimationHeader is JXL_TRUE.
			/// This value is only used if have_timecodes in JxlAnimationHeader is
			/// JXL_TRUE.
			/// </summary>
			public uint timecode;

			/// <summary>
			/// Length of the frame name in bytes, or 0 if no name.
			/// Excludes null termination character. This value is set by the decoder.
			/// For the encoder, this value is ignored and <see cref="JxlEncoderSetFrameName(JxlEncoderFrameSettings*,byte*)" /> is
			/// used instead to set the name and the length.
			/// </summary>
			public uint name_length;

			/// <summary>
			/// Indicates this is the last animation frame. This value is set by the
			/// decoder to indicate no further frames follow. For the encoder, it is not
			/// required to set this value and it is ignored, <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> is
			/// used to indicate the last frame to the encoder instead.
			/// </summary>
			public int is_last;

			/// <summary>
			/// Information about the layer in case of no coalescing.
			/// </summary>
			public JxlLayerInfo layer_info;
		}

		/// <summary>
		/// The result of <see cref="JxlSignatureCheck(byte*,UIntPtr)" />.
		/// </summary>
		internal enum JxlSignature
		{
			/// <summary>
			/// Not enough bytes were passed to determine if a valid signature was found.
			/// </summary>
			JXL_SIG_NOT_ENOUGH_BYTES,
			/// <summary>
			/// No valid JPEG XL header was found. 
			/// </summary>
			JXL_SIG_INVALID,
			/// <summary>
			/// A valid JPEG XL codestream signature was found, that is a JPEG XL image
			/// without container.
			/// </summary>
			JXL_SIG_CODESTREAM,
			/// <summary>
			/// A valid container signature was found, that is a JPEG XL image embedded
			/// in a box format container.
			/// </summary>
			JXL_SIG_CONTAINER
		}

		/// <summary>
		/// Return value for <see cref="JxlDecoderProcessInput(JxlDecoder*)" />.
		/// The values from <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" /> onwards are optional informative
		/// events that can be subscribed to, they are never returned if they
		/// have not been registered with <see cref="JxlDecoderSubscribeEvents(JxlDecoder*,int)" />.
		/// </summary>
		internal enum JxlDecoderStatus
		{
			/// <summary>
			/// Function call finished successfully, or decoding is finished and there is
			/// nothing more to be done.
			/// </summary>
			JXL_DEC_SUCCESS = 0,
			/// <summary>
			/// An error occurred, for example invalid input file or out of memory.
			/// TODO(lode): add function to get error information from decoder.
			/// </summary>
			JXL_DEC_ERROR = 1,
			/// <summary>
			/// The decoder needs more input bytes to continue. Before the next 
			/// <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> call, more input data must be set, by calling 
			/// <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> (if input was set previously) and then calling 
			/// <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />. <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> returns how many bytes
			/// are not yet processed, before a next call to <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// all unprocessed bytes must be provided again (the address need not match,
			/// but the contents must), and more bytes must be concatenated after the
			/// unprocessed bytes.
			/// </summary>
			JXL_DEC_NEED_MORE_INPUT = 2,
			/// <summary>
			/// The decoder is able to decode a preview image and requests setting a
			/// preview output buffer using <see cref="JxlDecoderSetPreviewOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />. This occurs
			/// if <see cref="JxlDecoderStatus.JXL_DEC_PREVIEW_IMAGE" /> is requested and it is possible to decode a
			/// preview image from the codestream and the preview out buffer was not yet
			/// set. There is maximum one preview image in a codestream.
			/// </summary>
			JXL_DEC_NEED_PREVIEW_OUT_BUFFER = 3,
			/// <summary>
			/// The decoder is able to decode a DC image and requests setting a DC output
			/// buffer using <see cref="JxlDecoderSetDCOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />. This occurs if 
			/// <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE" /> is requested and it is possible to decode a DC image from
			/// the codestream and the DC out buffer was not yet set. This event re-occurs
			/// for new frames if there are multiple animation frames.
			/// @deprecated The DC feature in this form will be removed. For progressive
			/// rendering, <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> should be used.
			/// </summary>
			JXL_DEC_NEED_DC_OUT_BUFFER = 4,
			/// <summary>
			/// The decoder requests an output buffer to store the full resolution image,
			/// which can be set with <see cref="JxlDecoderSetImageOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" /> or with 
			/// <see cref="JxlDecoderSetImageOutCallback(JxlDecoder*,JxlPixelFormat*,UIntPtr,void*)" />. This event re-occurs for new frames if
			/// there are multiple animation frames and requires setting an output again.
			/// </summary>
			JXL_DEC_NEED_IMAGE_OUT_BUFFER = 5,
			/// <summary>
			/// The JPEG reconstruction buffer is too small for reconstructed JPEG
			/// codestream to fit. <see cref="JxlDecoderSetJPEGBuffer(JxlDecoder*,byte*,UIntPtr)" /> must be called again to
			/// make room for remaining bytes. This event may occur multiple times
			/// after <see cref="JxlDecoderStatus.JXL_DEC_JPEG_RECONSTRUCTION" />.
			/// </summary>
			JXL_DEC_JPEG_NEED_MORE_OUTPUT = 6,
			/// <summary>
			/// The box contents output buffer is too small. <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" />
			/// must be called again to make room for remaining bytes. This event may occur
			/// multiple times after <see cref="JxlDecoderStatus.JXL_DEC_BOX" />.
			/// </summary>
			JXL_DEC_BOX_NEED_MORE_OUTPUT = 7,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": Basic information such as image dimensions and
			/// extra channels. This event occurs max once per image.
			/// </summary>
			JXL_DEC_BASIC_INFO = 64,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": User extensions of the codestream header. This
			/// event occurs max once per image and always later than 
			/// <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" /> and earlier than any pixel data.
			/// <br /><br />
			/// @deprecated The decoder no longer returns this, the header extensions,
			/// if any, are available at the JXL_DEC_BASIC_INFO event.
			/// </summary>
			JXL_DEC_EXTENSIONS = 128,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": Color encoding or ICC profile from the
			/// codestream header. This event occurs max once per image and always later
			/// than <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" /> and earlier than any pixel data.
			/// </summary>
			JXL_DEC_COLOR_ENCODING = 256,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": Preview image, a small frame, decoded. This
			/// event can only happen if the image has a preview frame encoded. This event
			/// occurs max once for the codestream and always later than 
			/// <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING" /> and before <see cref="JxlDecoderStatus.JXL_DEC_FRAME" />.
			/// </summary>
			JXL_DEC_PREVIEW_IMAGE = 512,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": Beginning of a frame. 
			/// <see cref="JxlDecoderGetFrameHeader(JxlDecoder*,JxlFrameHeader*)" /> can be used at this point. A note on frames:
			/// a JPEG XL image can have internal frames that are not intended to be
			/// displayed (e.g. used for compositing a final frame), but this only returns
			/// displayed frames, unless <see cref="JxlDecoderSetCoalescing(JxlDecoder*,int)" /> was set to JXL_FALSE:
			/// in that case, the individual layers are returned, without blending. Note
			/// that even when coalescing is disabled, only frames of type kRegularFrame
			/// are returned; frames of type kReferenceOnly and kLfFrame are always for
			/// internal purposes only and cannot be accessed. A displayed frame either has
			/// an animation duration or is the only or last frame in the image. This event
			/// occurs max once per displayed frame, always later than 
			/// <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING" />, and always earlier than any pixel data. While
			/// JPEG XL supports encoding a single frame as the composition of multiple
			/// internal sub-frames also called frames, this event is not indicated for the
			/// internal frames.
			/// </summary>
			JXL_DEC_FRAME = 1024,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": DC image, 8x8 sub-sampled frame, decoded. It is
			/// not guaranteed that the decoder will always return DC separately, but when
			/// it does it will do so before outputting the full frame. 
			/// <see cref="JxlDecoderSetDCOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" /> must be used after getting the basic image
			/// information to be able to get the DC pixels, if not this return status only
			/// indicates we're past this point in the codestream. This event occurs max
			/// once per frame and always later than <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> and other header
			/// events and earlier than full resolution pixel data.
			/// <br /><br />
			/// @deprecated The DC feature in this form will be removed. For progressive
			/// rendering, <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> should be used.
			/// </summary>
			JXL_DEC_DC_IMAGE = 2048,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": full frame (or layer, in case coalescing is
			/// disabled) is decoded. <see cref="JxlDecoderSetImageOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" /> must be used after
			/// getting the basic image information to be able to get the image pixels, if
			/// not this return status only indicates we're past this point in the
			/// codestream. This event occurs max once per frame and always later than 
			/// <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE" />.
			/// </summary>
			JXL_DEC_FULL_IMAGE = 4096,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": JPEG reconstruction data decoded. 
			/// <see cref="JxlDecoderSetJPEGBuffer(JxlDecoder*,byte*,UIntPtr)" /> may be used to set a JPEG reconstruction buffer
			/// after getting the JPEG reconstruction data. If a JPEG reconstruction buffer
			/// is set a byte stream identical to the JPEG codestream used to encode the
			/// image will be written to the JPEG reconstruction buffer instead of pixels
			/// to the image out buffer. This event occurs max once per image and always
			/// before <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" />.
			/// </summary>
			JXL_DEC_JPEG_RECONSTRUCTION = 8192,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": The header of a box of the container format
			/// (BMFF) is decoded. The following API functions related to boxes can be used
			/// after this event:
			/// - <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" /> and <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" />
			/// "JxlDecoderReleaseBoxBuffer": set and release a buffer to get the box
			/// data.
			/// - <see cref="JxlDecoderGetBoxType(JxlDecoder*,byte*,int)" /> get the 4-character box typename.
			/// - <see cref="JxlDecoderGetBoxSizeRaw(JxlDecoder*,System.UInt64*)" /> get the size of the box as it appears in
			/// the container file, not decompressed.
			/// - <see cref="JxlDecoderSetDecompressBoxes(JxlDecoder*,int)" /> to configure whether to get the box
			/// data decompressed, or possibly compressed.
			/// <br /><br />
			/// Boxes can be compressed. This is so when their box type is
			/// "brob". In that case, they have an underlying decompressed box
			/// type and decompressed data. <see cref="JxlDecoderSetDecompressBoxes(JxlDecoder*,int)" /> allows
			/// configuring which data to get. Decompressing requires
			/// Brotli. <see cref="JxlDecoderGetBoxType(JxlDecoder*,byte*,int)" /> has a flag to get the compressed box
			/// type, which can be "brob", or the decompressed box type. If a box
			/// is not compressed (its compressed type is not "brob"), then
			/// the output decompressed box type and data is independent of what
			/// setting is configured.
			/// <br /><br />
			/// The buffer set with <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" /> must be set again for each
			/// next box to be obtained, or can be left unset to skip outputting this box.
			/// The output buffer contains the full box data when the next <see cref="JxlDecoderStatus.JXL_DEC_BOX" />
			/// event or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> occurs. <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> occurs for all
			/// boxes, including non-metadata boxes such as the signature box or codestream
			/// boxes. To check whether the box is a metadata type for respectively EXIF,
			/// XMP or JUMBF, use <see cref="JxlDecoderGetBoxType(JxlDecoder*,byte*,int)" /> and check for types "Exif",
			/// "xml " and "jumb" respectively.
			/// </summary>
			JXL_DEC_BOX = 16384,
			/// <summary>
			/// Informative event by <see cref="JxlDecoderProcessInput(JxlDecoder*)" />
			/// "JxlDecoderProcessInput": a progressive step in decoding the frame is
			/// reached. When calling <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> at this point, the flushed
			/// image will correspond exactly to this point in decoding, and not yet
			/// contain partial results (such as partially more fine detail) of a next
			/// step. By default, this event will trigger maximum once per frame, when a
			/// 8x8th resolution (DC) image is ready (the image data is still returned at
			/// full resolution, giving upscaled DC). Use 
			/// <see cref="JxlDecoderSetProgressiveDetail(JxlDecoder*,JxlProgressiveDetail)" /> to configure more fine-grainedness. The
			/// event is not guaranteed to trigger, not all images have progressive steps
			/// or DC encoded.
			/// </summary>
			JXL_DEC_FRAME_PROGRESSION = 32768
		}

		/// <summary>
		/// Return value for multiple encoder functions.
		/// </summary>
		internal enum JxlEncoderStatus
		{
			/// <summary>
			/// Function call finished successfully, or encoding is finished and there is
			/// nothing more to be done.
			/// </summary>
			JXL_ENC_SUCCESS,
			/// <summary>
			/// An error occurred, for example out of memory.
			/// </summary>
			JXL_ENC_ERROR,
			/// <summary>
			/// The encoder needs more output buffer to continue encoding.
			/// </summary>
			JXL_ENC_NEED_MORE_OUTPUT,
			/// <summary>
			/// DEPRECATED: the encoder does not return this status and there is no need
			/// to handle or expect it.
			/// Instead, JXL_ENC_ERROR is returned with error condition
			/// JXL_ENC_ERR_NOT_SUPPORTED.
			/// </summary>
			JXL_ENC_NOT_SUPPORTED
		}

		/// <summary>
		/// Error conditions:
		/// API usage errors have the 0x80 bit set to 1
		/// Other errors have the 0x80 bit set to 0
		/// </summary>
		internal enum JxlEncoderError
		{
			/// <summary>
			/// No error
			/// </summary>
			JXL_ENC_ERR_OK = 0,
			/// <summary>
			/// Generic encoder error due to unspecified cause
			/// </summary>
			JXL_ENC_ERR_GENERIC = 1,
			/// <summary>
			/// Out of memory
			/// TODO(jon): actually catch this and return this error
			/// </summary>
			JXL_ENC_ERR_OOM = 2,
			/// <summary>
			/// JPEG bitstream reconstruction data could not be
			/// represented (e.g. too much tail data)
			/// </summary>
			JXL_ENC_ERR_JBRD = 3,
			/// <summary>
			/// Input is invalid (e.g. corrupt JPEG file or ICC profile)
			/// </summary>
			JXL_ENC_ERR_BAD_INPUT = 4,
			/// <summary>
			/// The encoder doesn't (yet) support this. Either no version of libjxl
			/// supports this, and the API is used incorrectly, or the libjxl version
			/// should have been checked before trying to do this.
			/// </summary>
			JXL_ENC_ERR_NOT_SUPPORTED = 128,
			/// <summary>
			/// The encoder API is used in an incorrect way.
			/// In this case, a debug build of libjxl should output a specific error
			/// message. (if not, please open an issue about it)
			/// </summary>
			JXL_ENC_ERR_API_USAGE = 129
		}

		/// <summary>
		/// Id of encoder options for a frame. This includes options such as setting
		/// encoding effort/speed or overriding the use of certain coding tools, for this
		/// frame. This does not include non-frame related encoder options such as for
		/// boxes.
		/// </summary>
		internal enum JxlEncoderFrameSettingId
		{
			/// <summary>
			/// Sets encoder effort/speed level without affecting decoding speed. Valid
			/// values are, from faster to slower speed: 1:lightning 2:thunder 3:falcon
			/// 4:cheetah 5:hare 6:wombat 7:squirrel 8:kitten 9:tortoise.
			/// Default: squirrel (7).
			/// </summary>
			JXL_ENC_FRAME_SETTING_EFFORT = 0,
			/// <summary>
			/// Sets the decoding speed tier for the provided options. Minimum is 0
			/// (slowest to decode, best quality/density), and maximum is 4 (fastest to
			/// decode, at the cost of some quality/density). Default is 0.
			/// </summary>
			JXL_ENC_FRAME_SETTING_DECODING_SPEED = 1,
			/// <summary>
			/// Sets resampling option. If enabled, the image is downsampled before
			/// compression, and upsampled to original size in the decoder. Integer option,
			/// use -1 for the default behavior (resampling only applied for low quality),
			/// 1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for 4x4
			/// downsampling, 8 for 8x8 downsampling.
			/// </summary>
			JXL_ENC_FRAME_SETTING_RESAMPLING = 2,
			/// <summary>
			/// Similar to JXL_ENC_FRAME_SETTING_RESAMPLING, but for extra channels.
			/// Integer option, use -1 for the default behavior (depends on encoder
			/// implementation), 1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for
			/// 4x4 downsampling, 8 for 8x8 downsampling.
			/// </summary>
			JXL_ENC_FRAME_SETTING_EXTRA_CHANNEL_RESAMPLING = 3,
			/// <summary>
			/// Indicates the frame added with <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" /> is already
			/// downsampled by the downsampling factor set with 
			/// <see cref="JxlEncoderFrameSettingId.JXL_ENC_FRAME_SETTING_RESAMPLING" />. The input frame must then be given in the
			/// downsampled resolution, not the full image resolution. The downsampled
			/// resolution is given by ceil(xsize / resampling), ceil(ysize / resampling)
			/// with xsize and ysize the dimensions given in the basic info, and resampling
			/// the factor set with <see cref="JxlEncoderFrameSettingId.JXL_ENC_FRAME_SETTING_RESAMPLING" />.
			/// Use 0 to disable, 1 to enable. Default value is 0.
			/// </summary>
			JXL_ENC_FRAME_SETTING_ALREADY_DOWNSAMPLED = 4,
			/// <summary>
			/// Adds noise to the image emulating photographic film noise, the higher the
			/// given number, the grainier the image will be. As an example, a value of 100
			/// gives low noise whereas a value of 3200 gives a lot of noise. The default
			/// value is 0.
			/// </summary>
			JXL_ENC_FRAME_SETTING_PHOTON_NOISE = 5,
			/// <summary>
			/// Enables adaptive noise generation. This setting is not recommended for
			/// use, please use JXL_ENC_FRAME_SETTING_PHOTON_NOISE instead. Use -1 for the
			/// default (encoder chooses), 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_NOISE = 6,
			/// <summary>
			/// Enables or disables dots generation. Use -1 for the default (encoder
			/// chooses), 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_DOTS = 7,
			/// <summary>
			/// Enables or disables patches generation. Use -1 for the default (encoder
			/// chooses), 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_PATCHES = 8,
			/// <summary>
			/// Edge preserving filter level, -1 to 3. Use -1 for the default (encoder
			/// chooses), 0 to 3 to set a strength.
			/// </summary>
			JXL_ENC_FRAME_SETTING_EPF = 9,
			/// <summary>
			/// Enables or disables the gaborish filter. Use -1 for the default (encoder
			/// chooses), 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_GABORISH = 10,
			/// <summary>
			/// Enables modular encoding. Use -1 for default (encoder
			/// chooses), 0 to enforce VarDCT mode (e.g. for photographic images), 1 to
			/// enforce modular mode (e.g. for lossless images).
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR = 11,
			/// <summary>
			/// Enables or disables preserving color of invisible pixels. Use -1 for the
			/// default (1 if lossless, 0 if lossy), 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_KEEP_INVISIBLE = 12,
			/// <summary>
			/// Determines the order in which 256x256 regions are stored in the codestream
			/// for progressive rendering. Use -1 for the encoder
			/// default, 0 for scanline order, 1 for center-first order.
			/// </summary>
			JXL_ENC_FRAME_SETTING_GROUP_ORDER = 13,
			/// <summary>
			/// Determines the horizontal position of center for the center-first group
			/// order. Use -1 to automatically use the middle of the image, 0..xsize to
			/// specifically set it.
			/// </summary>
			JXL_ENC_FRAME_SETTING_GROUP_ORDER_CENTER_X = 14,
			/// <summary>
			/// Determines the center for the center-first group order. Use -1 to
			/// automatically use the middle of the image, 0..ysize to specifically set it.
			/// </summary>
			JXL_ENC_FRAME_SETTING_GROUP_ORDER_CENTER_Y = 15,
			/// <summary>
			/// Enables or disables progressive encoding for modular mode. Use -1 for the
			/// encoder default, 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_RESPONSIVE = 16,
			/// <summary>
			/// Set the progressive mode for the AC coefficients of VarDCT, using spectral
			/// progression from the DCT coefficients. Use -1 for the encoder default, 0 to
			/// disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_PROGRESSIVE_AC = 17,
			/// <summary>
			/// Set the progressive mode for the AC coefficients of VarDCT, using
			/// quantization of the least significant bits. Use -1 for the encoder default,
			/// 0 to disable, 1 to enable.
			/// </summary>
			JXL_ENC_FRAME_SETTING_QPROGRESSIVE_AC = 18,
			/// <summary>
			/// Set the progressive mode using lower-resolution DC images for VarDCT. Use
			/// -1 for the encoder default, 0 to disable, 1 to have an extra 64x64 lower
			/// resolution pass, 2 to have a 512x512 and 64x64 lower resolution pass.
			/// </summary>
			JXL_ENC_FRAME_SETTING_PROGRESSIVE_DC = 19,
			/// <summary>
			/// Use Global channel palette if the amount of colors is smaller than this
			/// percentage of range. Use 0-100 to set an explicit percentage, -1 to use the
			/// encoder default. Used for modular encoding.
			/// </summary>
			JXL_ENC_FRAME_SETTING_CHANNEL_COLORS_GLOBAL_PERCENT = 20,
			/// <summary>
			/// Use Local (per-group) channel palette if the amount of colors is smaller
			/// than this percentage of range. Use 0-100 to set an explicit percentage, -1
			/// to use the encoder default. Used for modular encoding.
			/// </summary>
			JXL_ENC_FRAME_SETTING_CHANNEL_COLORS_GROUP_PERCENT = 21,
			/// <summary>
			/// Use color palette if amount of colors is smaller than or equal to this
			/// amount, or -1 to use the encoder default. Used for modular encoding.
			/// </summary>
			JXL_ENC_FRAME_SETTING_PALETTE_COLORS = 22,
			/// <summary>
			/// Enables or disables delta palette. Use -1 for the default (encoder
			/// chooses), 0 to disable, 1 to enable. Used in modular mode.
			/// </summary>
			JXL_ENC_FRAME_SETTING_LOSSY_PALETTE = 23,
			/// <summary>
			/// Color transform for internal encoding: -1 = default, 0=XYB, 1=none (RGB),
			/// 2=YCbCr. The XYB setting performs the forward XYB transform. None and
			/// YCbCr both perform no transform, but YCbCr is used to indicate that the
			/// encoded data losslessly represents YCbCr values.
			/// </summary>
			JXL_ENC_FRAME_SETTING_COLOR_TRANSFORM = 24,
			/// <summary>
			/// Reversible color transform for modular encoding: -1=default, 0-41=RCT
			/// index, e.g. index 0 = none, index 6 = YCoCg.
			/// If this option is set to a non-default value, the RCT will be globally
			/// applied to the whole frame.
			/// The default behavior is to try several RCTs locally per modular group,
			/// depending on the speed and distance setting.
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR_COLOR_SPACE = 25,
			/// <summary>
			/// Group size for modular encoding: -1=default, 0=128, 1=256, 2=512, 3=1024.
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR_GROUP_SIZE = 26,
			/// <summary>
			/// Predictor for modular encoding. -1 = default, 0=zero, 1=left, 2=top,
			/// 3=avg0, 4=select, 5=gradient, 6=weighted, 7=topright, 8=topleft,
			/// 9=leftleft, 10=avg1, 11=avg2, 12=avg3, 13=toptop predictive average 14=mix
			/// 5 and 6, 15=mix everything.
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR_PREDICTOR = 27,
			/// <summary>
			/// Fraction of pixels used to learn MA trees as a percentage. -1 = default,
			/// 0 = no MA and fast decode, 50 = default value, 100 = all, values above
			/// 100 are also permitted. Higher values use more encoder memory.
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR_MA_TREE_LEARNING_PERCENT = 28,
			/// <summary>
			/// Number of extra (previous-channel) MA tree properties to use. -1 =
			/// default, 0-11 = valid values. Recommended values are in the range 0 to 3,
			/// or 0 to amount of channels minus 1 (including all extra channels, and
			/// excluding color channels when using VarDCT mode). Higher value gives slower
			/// encoding and slower decoding.
			/// </summary>
			JXL_ENC_FRAME_SETTING_MODULAR_NB_PREV_CHANNELS = 29,
			/// <summary>
			/// Enable or disable CFL (chroma-from-luma) for lossless JPEG recompression.
			/// -1 = default, 0 = disable CFL, 1 = enable CFL.
			/// </summary>
			JXL_ENC_FRAME_SETTING_JPEG_RECON_CFL = 30,
			/// <summary>
			/// Prepare the frame for indexing in the frame index box.
			/// 0 = ignore this frame (same as not setting a value),
			/// 1 = index this frame within the Frame Index Box.
			/// If any frames are indexed, the first frame needs to
			/// be indexed, too. If the first frame is not indexed, and
			/// a later frame is attempted to be indexed, JXL_ENC_ERROR will occur.
			/// If non-keyframes, i.e., frames with cropping, blending or patches are
			/// attempted to be indexed, JXL_ENC_ERROR will occur.
			/// </summary>
			JXL_ENC_FRAME_INDEX_BOX = 31,
			/// <summary>
			/// Sets brotli encode effort for use in JPEG recompression and compressed
			/// metadata boxes (brob). Can be -1 (default) or 0 (fastest) to 11 (slowest).
			/// Default is based on the general encode effort in case of JPEG
			/// recompression, and 4 for brob boxes.
			/// </summary>
			JXL_ENC_FRAME_SETTING_BROTLI_EFFORT = 32,
			/// <summary>
			/// Enum value not to be used as an option. This value is added to force the
			/// C compiler to have the enum to take a known size.
			/// </summary>
			JXL_ENC_FRAME_SETTING_FILL_ENUM = 65535
		}

		/// <summary>
		/// Opaque structure that holds the JPEG XL decoder.
		/// <br /><br />
		/// Allocated and initialized with <see cref="JxlDecoderCreate(JxlMemoryManager*)" />().
		/// Cleaned up and deallocated with <see cref="JxlDecoderDestroy(JxlDecoder*)" />().
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
		internal struct JxlDecoder
		{
		}

		/// <summary>
		/// Opaque structure that holds the JPEG XL encoder.
		/// <br /><br />
		/// Allocated and initialized with JxlEncoderCreate().
		/// Cleaned up and deallocated with JxlEncoderDestroy().
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
		internal struct JxlEncoder
		{
		}

		/// <summary>
		/// Settings and metadata for a single image frame. This includes encoder options
		/// for a frame such as compression quality and speed.
		/// <br /><br />
		/// Allocated and initialized with JxlEncoderFrameSettingsCreate().
		/// Cleaned up and deallocated when the encoder is destroyed with
		/// JxlEncoderDestroy().
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 1)]
		internal struct JxlEncoderFrameSettings
		{
		}

		internal static int JXL_PARALLEL_RET_RUNNER_ERROR = -1;

		/// <summary>
		/// Portable <tt>true</tt> replacement. 
		/// </summary>
		internal static int JXL_TRUE = 1;

		/// <summary>
		/// Portable <tt>false</tt> replacement. 
		/// </summary>
		internal static int JXL_FALSE = 0;

		/// <summary>
		/// Decoder library version.
		/// </summary>
		/// <returns> the decoder library version as an integer:
		/// MAJOR_VERSION * 1000000 + MINOR_VERSION * 1000 + PATCH_VERSION. For example,
		/// version 1.2.3 would return 1002003.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal static extern uint JxlDecoderVersion();

		/// <summary>
		/// JPEG XL signature identification.
		/// <br /><br />
		/// Checks if the passed buffer contains a valid JPEG XL signature. The passed 
		/// <tt>buf</tt> of size
		/// <tt>size</tt> doesn't need to be a full image, only the beginning of the file.
		/// </summary>
		/// <returns> a flag indicating if a JPEG XL signature was found and what type.
		/// - <see cref="JxlSignature.JXL_SIG_NOT_ENOUGH_BYTES" /> if not enough bytes were passed to
		/// determine if a valid signature is there.
		/// - <see cref="JxlSignature.JXL_SIG_INVALID" /> if no valid signature found for JPEG XL decoding.
		/// - <see cref="JxlSignature.JXL_SIG_CODESTREAM" /> if a valid JPEG XL codestream signature was
		/// found.
		/// - <see cref="JxlSignature.JXL_SIG_CONTAINER" /> if a valid JPEG XL container signature was found.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlSignature JxlSignatureCheck(byte* buf, UIntPtr len);

		/// <summary>
		/// Creates an instance of <see cref=JxlDecoder" /> and initializes it.
		/// <br /><br /><tt>memory_manager</tt> will be used for all the library dynamic allocations made
		/// from this instance. The parameter may be NULL, in which case the default
		/// allocator will be used. See jxl/memory_manager.h for details.
		/// </summary>
		/// <param name="memory_manager"> custom allocator function. It may be NULL. The memory
		/// manager will be copied internally.</param>
		/// <returns>
		///     <tt>NULL</tt> if the instance can not be allocated or initialized</returns>
		/// <returns> pointer to initialized <see cref=JxlDecoder" /> otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoder* JxlDecoderCreate(JxlMemoryManager* memory_manager);

		/// <summary>
		/// Re-initializes a <see cref=JxlDecoder" /> instance, so it can be re-used for decoding
		/// another image. All state and settings are reset as if the object was
		/// newly created with <see cref="JxlDecoderCreate(JxlMemoryManager*)" />, but the memory manager is kept.
		/// </summary>
		/// <param name="dec"> instance to be re-initialized.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlDecoderReset(JxlDecoder* dec);

		/// <summary>
		/// Deinitializes and frees <see cref=JxlDecoder" /> instance.
		/// </summary>
		/// <param name="dec"> instance to be cleaned up and deallocated.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlDecoderDestroy(JxlDecoder* dec);

		/// <summary>
		/// Rewinds decoder to the beginning. The same input must be given again from
		/// the beginning of the file and the decoder will emit events from the beginning
		/// again. When rewinding (as opposed to <see cref="JxlDecoderReset(JxlDecoder*)" />), the decoder can
		/// keep state about the image, which it can use to skip to a requested frame
		/// more efficiently with <see cref="JxlDecoderSkipFrames(JxlDecoder*,UIntPtr)" />. Settings such as parallel
		/// runner or subscribed events are kept. After rewind, 
		/// <see cref="JxlDecoderSubscribeEvents(JxlDecoder*,int)" /> can be used again, and it is feasible to leave out
		/// events that were already handled before, such as <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" />
		/// and <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING" />, since they will provide the same information
		/// as before.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlDecoderRewind(JxlDecoder* dec);

		/// <summary>
		/// Makes the decoder skip the next `amount` frames. It still needs to process
		/// the input, but will not output the frame events. It can be more efficient
		/// when skipping frames, and even more so when using this after 
		/// <see cref="JxlDecoderRewind(JxlDecoder*)" />. If the decoder is already processing a frame (could
		/// have emitted <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> but not yet <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" />), it
		/// starts skipping from the next frame. If the amount is larger than the amount
		/// of frames remaining in the image, all remaining frames are skipped. Calling
		/// this function multiple times adds the amount to skip to the already existing
		/// amount.
		/// <br /><br />
		/// A frame here is defined as a frame that without skipping emits events such
		/// as <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> and <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" />, frames that are internal
		/// to the file format but are not rendered as part of an animation, or are not
		/// the final still frame of a still image, are not counted.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="amount"> the amount of frames to skip</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlDecoderSkipFrames(JxlDecoder* dec, UIntPtr amount);

		/// <summary>
		/// Skips processing the current frame. Can be called after frame processing
		/// already started, signaled by a <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER" /> event,
		/// but before the corrsponding <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" /> event. The next signaled
		/// event will be another <see cref="JxlDecoderStatus.JXL_DEC_FRAME" />, or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if there
		/// are no more frames. If pixel data is required from the already processed part
		/// of the frame, <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> must be called before this.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if there is a frame to skip, and 
		/// <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if the function was not called during frame processing.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSkipCurrentFrame(JxlDecoder* dec);

		/// <summary>
		/// Get the default pixel format for this decoder.
		/// <br /><br />
		/// Requires that the decoder can produce JxlBasicInfo.
		/// </summary>
		/// <param name="dec">
		///     <see cref=JxlDecoder" /> to query when creating the recommended pixel
		/// format.</param>
		/// <param name="format"> JxlPixelFormat to populate with the recommended settings for
		/// the data loaded into this decoder.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if no error, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if the
		/// basic info isn't yet available, and <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderDefaultPixelFormat(JxlDecoder* dec, JxlPixelFormat* format);

		/// <summary>
		/// Set the parallel runner for multithreading. May only be set before starting
		/// decoding.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="parallel_runner"> function pointer to runner for multithreading. It may
		/// be NULL to use the default, single-threaded, runner. A multithreaded
		/// runner should be set to reach fast performance.</param>
		/// <param name="parallel_runner_opaque"> opaque pointer for parallel_runner.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the runner was set, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" />
		/// otherwise (the previous runner remains set).</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetParallelRunner(JxlDecoder* dec, IntPtr parallel_runner, void* parallel_runner_opaque);

		/// <summary>
		/// Returns a hint indicating how many more bytes the decoder is expected to
		/// need to make <see cref="JxlDecoderGetBasicInfo(JxlDecoder*,JxlBasicInfo*)" /> available after the next 
		/// <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> call. This is a suggested large enough value for
		/// the amount of bytes to provide in the next <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" /> call, but
		/// it is not guaranteed to be an upper bound nor a lower bound. This number does
		/// not include bytes that have already been released from the input. Can be used
		/// before the first <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> call, and is correct the first
		/// time in most cases. If not, <see cref="JxlDecoderSizeHintBasicInfo(JxlDecoder*)" /> can be called
		/// again to get an updated hint.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns> the size hint in bytes if the basic info is not yet fully decoded.</returns>
		/// <returns> 0 when the basic info is already available.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern UIntPtr JxlDecoderSizeHintBasicInfo(JxlDecoder* dec);

		/// <summary>
		/// Select for which informative events, i.e. <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" />, etc., the
		/// decoder should return with a status. It is not required to subscribe to any
		/// events, data can still be requested from the decoder as soon as it available.
		/// By default, the decoder is subscribed to no events (events_wanted == 0), and
		/// the decoder will then only return when it cannot continue because it needs
		/// more input data or more output buffer. This function may only be be called
		/// before using <see cref="JxlDecoderProcessInput(JxlDecoder*)" />.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="events_wanted"> bitfield of desired events.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSubscribeEvents(JxlDecoder* dec, int events_wanted);

		/// <summary>
		/// Enables or disables preserving of as-in-bitstream pixeldata
		/// orientation. Some images are encoded with an Orientation tag
		/// indicating that the decoder must perform a rotation and/or
		/// mirroring to the encoded image data.
		/// <br /><br />
		/// - If skip_reorientation is JXL_FALSE (the default): the decoder
		/// will apply the transformation from the orientation setting, hence
		/// rendering the image according to its specified intent. When
		/// producing a JxlBasicInfo, the decoder will always set the
		/// orientation field to JXL_ORIENT_IDENTITY (matching the returned
		/// pixel data) and also align xsize and ysize so that they correspond
		/// to the width and the height of the returned pixel data.
		/// - If skip_reorientation is JXL_TRUE: the decoder will skip
		/// applying the transformation from the orientation setting, returning
		/// the image in the as-in-bitstream pixeldata orientation.
		/// This may be faster to decode since the decoder doesn't have to apply the
		/// transformation, but can cause wrong display of the image if the
		/// orientation tag is not correctly taken into account by the user.
		/// <br /><br />
		/// By default, this option is disabled, and the returned pixel data is
		/// re-oriented according to the image's Orientation setting.
		/// <br /><br />
		/// This function must be called at the beginning, before decoding is performed.
		/// <br /><br /><see cref=JxlBasicInfo" /> for the orientation field, and <see cref=JxlOrientation" /> for the
		/// possible values.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="skip_reorientation"> JXL_TRUE to enable, JXL_FALSE to disable.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetKeepOrientation(JxlDecoder* dec, int skip_reorientation);

		/// <summary>
		/// Enables or disables rendering spot colors. By default, spot colors
		/// are rendered, which is OK for viewing the decoded image. If render_spotcolors
		/// is JXL_FALSE, then spot colors are not rendered, and have to be retrieved
		/// separately using <see cref="JxlDecoderSetExtraChannelBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr,System.UInt32)" />. This is useful for
		/// e.g. printing applications.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="render_spotcolors"> JXL_TRUE to enable (default), JXL_FALSE to disable.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetRenderSpotcolors(JxlDecoder* dec, int render_spotcolors);

		/// <summary>
		/// Enables or disables coalescing of zero-duration frames. By default, frames
		/// are returned with coalescing enabled, i.e. all frames have the image
		/// dimensions, and are blended if needed. When coalescing is disabled, frames
		/// can have arbitrary dimensions, a non-zero crop offset, and blending is not
		/// performed. For display, coalescing is recommended. For loading a multi-layer
		/// still image as separate layers (as opposed to the merged image), coalescing
		/// has to be disabled.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="coalescing"> JXL_TRUE to enable coalescing (default), JXL_FALSE to
		/// disable it.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetCoalescing(JxlDecoder* dec, int coalescing);

		/// <summary>
		/// Decodes JPEG XL file using the available bytes. Requires input has been
		/// set with <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />. After <see cref="JxlDecoderProcessInput(JxlDecoder*)" />, input
		/// can optionally be released with <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> and then set
		/// again to next bytes in the stream. <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> returns how
		/// many bytes are not yet processed, before a next call to 
		/// <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> all unprocessed bytes must be provided again (the
		/// address need not match, but the contents must), and more bytes may be
		/// concatenated after the unprocessed bytes.
		/// <br /><br />
		/// The returned status indicates whether the decoder needs more input bytes, or
		/// more output buffer for a certain type of output data. No matter what the
		/// returned status is (other than <see cref="JxlDecoderStatus.JXL_DEC_ERROR" />), new information, such
		/// as <see cref="JxlDecoderGetBasicInfo(JxlDecoder*,JxlBasicInfo*)" />, may have become available after this call.
		/// When the return value is not <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" />, the
		/// decoding requires more <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> calls to continue.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> when decoding finished and all events handled.
		/// If you still have more unprocessed input data anyway, then you can still
		/// continue by using <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" /> and calling 
		/// <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> again, similar to handling 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" />. <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> can occur instead of 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> when, for example, the input data ended right at
		/// the boundary of a box of the container format, all essential codestream
		/// boxes were already decoded, but extra metadata boxes are still present in
		/// the next data. <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> cannot return success if all
		/// codestream boxes have not been seen yet.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> when decoding failed, e.g. invalid codestream.
		/// TODO(lode): document the input data mechanism</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> when more input data is necessary.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO" /> when basic info such as image dimensions is
		/// available and this informative event is subscribed to.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING" /> when color profile information is
		/// available and this informative event is subscribed to.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_PREVIEW_IMAGE" /> when preview pixel information is
		/// available and output in the preview buffer.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE" /> when DC pixel information (8x8 downscaled
		/// version of the image) is available and output is in the DC buffer.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" /> when all pixel information at highest detail
		/// is available and has been output in the pixel buffer.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderProcessInput(JxlDecoder* dec);

		/// <summary>
		/// Sets input data for <see cref="JxlDecoderProcessInput(JxlDecoder*)" />. The data is owned by the
		/// caller and may be used by the decoder until <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> is
		/// called or the decoder is destroyed or reset so must be kept alive until then.
		/// Cannot be called if <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" /> was already called and 
		/// <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> was not yet called, and cannot be called after 
		/// <see cref="JxlDecoderCloseInput(JxlDecoder*)" /> indicating the end of input was called.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="data"> pointer to next bytes to read from</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if input was already set without releasing or 
		/// <see cref="JxlDecoderCloseInput(JxlDecoder*)" /> was already called, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetInput(JxlDecoder* dec, byte* data, UIntPtr size);

		/// <summary>
		/// Releases input which was provided with <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />. Between 
		/// <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> and <see cref="JxlDecoderReleaseInput(JxlDecoder*)" />, the user may not
		/// alter the data in the buffer. Calling <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> is required
		/// whenever any input is already set and new input needs to be added with 
		/// <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />, but is not required before <see cref="JxlDecoderDestroy(JxlDecoder*)" /> or 
		/// <see cref="JxlDecoderReset(JxlDecoder*)" />. Calling <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> when no input is set is
		/// not an error and returns 0.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns> The amount of bytes the decoder has not yet processed that are still
		/// remaining in the data set by <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />, or 0 if no input is
		/// set or <see cref="JxlDecoderReleaseInput(JxlDecoder*)" /> was already called. For a next call
		/// to <see cref="JxlDecoderProcessInput(JxlDecoder*)" />, the buffer must start with these
		/// unprocessed bytes. This value doesn't provide information about how many
		/// bytes the decoder truly processed internally or how large the original
		/// JPEG XL codestream or file are.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern UIntPtr JxlDecoderReleaseInput(JxlDecoder* dec);

		/// <summary>
		/// Marks the input as finished, indicates that no more <see cref="JxlDecoderSetInput(JxlDecoder*,byte*,UIntPtr)" />
		/// will be called. This function allows the decoder to determine correctly if it
		/// should return success, need more input or error in certain cases. For
		/// backwards compatibility with a previous version of the API, using this
		/// function is optional when not using the <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> event (the decoder
		/// is able to determine the end of the image frames without marking the end),
		/// but using this function is required when using <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> for getting
		/// metadata box contents. This function does not replace 
		/// <see cref="JxlDecoderReleaseInput(JxlDecoder*)" />, that function should still be called if its return
		/// value is needed.
		/// <br /><br /><see cref="JxlDecoderCloseInput(JxlDecoder*)" /> should be called as soon as all known input bytes
		/// are set (e.g. at the beginning when not streaming but setting all input
		/// at once), before the final <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> calls.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlDecoderCloseInput(JxlDecoder* dec);

		/// <summary>
		/// Outputs the basic image information, such as image dimensions, bit depth and
		/// all other JxlBasicInfo fields, if available.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="info"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" />
		/// in case of other error conditions.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetBasicInfo(JxlDecoder* dec, JxlBasicInfo* info);

		/// <summary>
		/// Outputs information for extra channel at the given index. The index must be
		/// smaller than num_extra_channels in the associated JxlBasicInfo.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="index"> index of the extra channel to query.</param>
		/// <param name="info"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" />
		/// in case of other error conditions.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetExtraChannelInfo(JxlDecoder* dec, UIntPtr index, JxlExtraChannelInfo* info);

		/// <summary>
		/// Outputs name for extra channel at the given index in UTF-8. The index must be
		/// smaller than num_extra_channels in the associated JxlBasicInfo. The buffer
		/// for name must have at least name_length + 1 bytes allocated, gotten from
		/// the associated JxlExtraChannelInfo.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="index"> index of the extra channel to query.</param>
		/// <param name="name"> buffer to copy the name into</param>
		/// <param name="size"> size of the name buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" />
		/// in case of other error conditions.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetExtraChannelName(JxlDecoder* dec, UIntPtr index, byte* name, UIntPtr size);

		/// <summary>
		/// Outputs the color profile as JPEG XL encoded structured data, if available.
		/// This is an alternative to an ICC Profile, which can represent a more limited
		/// amount of color spaces, but represents them exactly through enum values.
		/// <br /><br />
		/// It is often possible to use <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" /> as an
		/// alternative anyway. The following scenarios are possible:
		/// - The JPEG XL image has an attached ICC Profile, in that case, the encoded
		/// structured data is not available, this function will return an error
		/// status. <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" /> should be called instead.
		/// - The JPEG XL image has an encoded structured color profile, and it
		/// represents an RGB or grayscale color space. This function will return it.
		/// You can still use <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" /> as well as an
		/// alternative if desired, though depending on which RGB color space is
		/// represented, the ICC profile may be a close approximation. It is also not
		/// always feasible to deduce from an ICC profile which named color space it
		/// exactly represents, if any, as it can represent any arbitrary space.
		/// - The JPEG XL image has an encoded structured color profile, and it
		/// indicates an unknown or xyb color space. In that case, 
		/// <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" /> is not available.
		/// <br /><br />
		/// When rendering an image on a system that supports ICC profiles, 
		/// <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" /> should be used first. When rendering
		/// for a specific color space, possibly indicated in the JPEG XL
		/// image, <see cref="JxlDecoderGetColorAsEncodedProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,JxlColorEncoding*)" /> should be used first.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" />, may be nullptr otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="color_encoding"> struct to copy the information into, or NULL to only
		/// check whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the data is available and returned, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> in
		/// case the encoded structured color profile does not exist in the
		/// codestream.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetColorAsEncodedProfile(JxlDecoder* dec, JxlPixelFormat* format, JxlColorProfileTarget target, JxlColorEncoding* color_encoding);

		/// <summary>
		/// Outputs the size in bytes of the ICC profile returned by 
		/// <see cref="JxlDecoderGetColorAsICCProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,byte*,UIntPtr)" />, if available, or indicates there is none
		/// available. In most cases, the image will have an ICC profile available, but
		/// if it does not, <see cref="JxlDecoderGetColorAsEncodedProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,JxlColorEncoding*)" /> must be used instead.
		/// <br /><br /><see cref="JxlDecoderGetColorAsEncodedProfile(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,JxlColorEncoding*)" /> for more information. The ICC
		/// profile is either the exact ICC profile attached to the codestream metadata,
		/// or a close approximation generated from JPEG XL encoded structured data,
		/// depending of what is encoded in the codestream.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" />, may be NULL otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="size"> variable to output the size into, or NULL to only check the
		/// return status.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the ICC profile is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if the decoder has not yet received enough
		/// input data to determine whether an ICC profile is available or what its
		/// size is, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> in case the ICC profile is not available and
		/// cannot be generated.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetICCProfileSize(JxlDecoder* dec, JxlPixelFormat* format, JxlColorProfileTarget target, UIntPtr* size);

		/// <summary>
		/// Outputs ICC profile if available. The profile is only available if 
		/// <see cref="JxlDecoderGetICCProfileSize(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,UIntPtr*)" /> returns success. The output buffer must have
		/// at least as many bytes as given by <see cref="JxlDecoderGetICCProfileSize(JxlDecoder*,JxlPixelFormat*,JxlColorProfileTarget,UIntPtr*)" />.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" />, may be NULL otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="icc_profile"> buffer to copy the ICC profile into</param>
		/// <param name="size"> size of the icc_profile buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the profile was successfully returned is
		/// available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if the profile doesn't exist or the output size is not
		/// large enough.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetColorAsICCProfile(JxlDecoder* dec, JxlPixelFormat* format, JxlColorProfileTarget target, byte* icc_profile, UIntPtr size);

		/// <summary>
		/// Sets the color profile to use for <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" /> for the
		/// special case when the decoder has a choice. This only has effect for a JXL
		/// image where uses_original_profile is false. If uses_original_profile is true,
		/// this setting is ignored and the decoder uses a profile related to the image.
		/// No matter what, the <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" /> must still be queried
		/// to know the actual data format of the decoded pixels after decoding.
		/// <br /><br />
		/// The JXL decoder has no color management system built in, but can convert XYB
		/// color to any of the ones supported by JxlColorEncoding. Note that if the
		/// requested color encoding has a narrower gamut, or the white points differ,
		/// then the resulting image can have significant color distortion.
		/// <br /><br />
		/// Can only be set after the <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING" /> event occurred and
		/// before any other event occurred, and can affect the result of 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" /> (but not of 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_ORIGINAL" />), so should be used after getting 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_ORIGINAL" /> but before getting 
		/// <see cref="JxlColorProfileTarget.JXL_COLOR_PROFILE_TARGET_DATA" />. The color_encoding must be grayscale if
		/// num_color_channels from the basic info is 1, RGB if num_color_channels from
		/// the basic info is 3.
		/// <br /><br />
		/// If <see cref="JxlDecoderSetPreferredColorProfile(JxlDecoder*,JxlColorEncoding*)" /> is not used, then for images for
		/// which uses_original_profile is false and with ICC color profile, the decoder
		/// will choose linear sRGB for color images, linear grayscale for grayscale
		/// images. This function only sets a preference, since for other images the
		/// decoder has no choice what color profile to use, it is determined by the
		/// image.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="color_encoding"> the default color encoding to set</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the preference was set successfully, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetPreferredColorProfile(JxlDecoder* dec, JxlColorEncoding* color_encoding);

		/// <summary>
		/// Requests that the decoder perform tone mapping to the peak display luminance
		/// passed as <tt>desired_intensity_target</tt>, if appropriate.
		/// <br />Note:  This is provided for convenience and the exact tone mapping that is
		/// performed is not meant to be considered authoritative in any way. It may
		/// change from version to version.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="desired_intensity_target"> the intended target peak luminance</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the preference was set successfully, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetDesiredIntensityTarget(JxlDecoder* dec, float desired_intensity_target);

		/// <summary>
		/// Returns the minimum size in bytes of the preview image output pixel buffer
		/// for the given format. This is the buffer for 
		/// <see cref="JxlDecoderSetPreviewOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />. Requires the preview header information is
		/// available in the decoder.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// information not available yet.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderPreviewOutBufferSize(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr* size);

		/// <summary>
		/// Sets the buffer to write the small resolution preview image
		/// to. The size of the buffer must be at least as large as given by 
		/// <see cref="JxlDecoderPreviewOutBufferSize(JxlDecoder*,JxlPixelFormat*,UIntPtr*)" />. The buffer follows the format described
		/// by JxlPixelFormat. The preview image dimensions are given by the
		/// JxlPreviewHeader. The buffer is owned by the caller.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// size too small.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetPreviewOutBuffer(JxlDecoder* dec, JxlPixelFormat* format, void* buffer, UIntPtr size);

		/// <summary>
		/// Outputs the information from the frame, such as duration when have_animation.
		/// This function can be called when <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> occurred for the current
		/// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="header"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> in
		/// case of other error conditions.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetFrameHeader(JxlDecoder* dec, JxlFrameHeader* header);

		/// <summary>
		/// Outputs name for the current frame. The buffer for name must have at least
		/// name_length + 1 bytes allocated, gotten from the associated JxlFrameHeader.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="name"> buffer to copy the name into</param>
		/// <param name="size"> size of the name buffer in bytes, including zero termination
		/// character, so this must be at least JxlFrameHeader.name_length + 1.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" /> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> in
		/// case of other error conditions.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetFrameName(JxlDecoder* dec, byte* name, UIntPtr size);

		/// <summary>
		/// Outputs the blend information for the current frame for a specific extra
		/// channel. This function can be called when <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> occurred for the
		/// current frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
		/// This information is only useful if coalescing is disabled; otherwise the
		/// decoder will have performed blending already.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="index"> the index of the extra channel</param>
		/// <param name="blend_info"> struct to copy the information into</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetExtraChannelBlendInfo(JxlDecoder* dec, UIntPtr index, JxlBlendInfo* blend_info);

		/// <summary>
		/// Returns the minimum size in bytes of the DC image output buffer
		/// for the given format. This is the buffer for <see cref="JxlDecoderSetDCOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// information not available yet.
		/// <br /><br />
		/// @deprecated The DC feature in this form will be removed. Use 
		/// <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> for progressive rendering.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlDecoderStatus JxlDecoderDCOutBufferSize(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr* size);

		/// <summary>
		/// Sets the buffer to write the lower resolution (8x8 sub-sampled) DC image
		/// to. The size of the buffer must be at least as large as given by 
		/// <see cref="JxlDecoderDCOutBufferSize(JxlDecoder*,JxlPixelFormat*,UIntPtr*)" />. The buffer follows the format described by
		/// JxlPixelFormat. The DC image has dimensions ceil(xsize / 8) * ceil(ysize /
		/// 8). The buffer is owned by the caller.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// size too small.
		/// <br /><br />
		/// @deprecated The DC feature in this form will be removed. Use 
		/// <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> for progressive rendering.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetDCOutBuffer(JxlDecoder* dec, JxlPixelFormat* format, void* buffer, UIntPtr size);

		/// <summary>
		/// Returns the minimum size in bytes of the image output pixel buffer for the
		/// given format. This is the buffer for <see cref="JxlDecoderSetImageOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />.
		/// Requires that the basic image information is available in the decoder in the
		/// case of coalescing enabled (default). In case coalescing is disabled, this
		/// can only be called after the <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> event occurs. In that case,
		/// it will return the size required to store the possibly cropped frame (which
		/// can be larger or smaller than the image dimensions).
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of the pixels.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// information not available yet.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderImageOutBufferSize(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr* size);

		/// <summary>
		/// Sets the buffer to write the full resolution image to. This can be set when
		/// the <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> event occurs, must be set when the 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER" /> event occurs, and applies only for the
		/// current frame. The size of the buffer must be at least as large as given
		/// by <see cref="JxlDecoderImageOutBufferSize(JxlDecoder*,JxlPixelFormat*,UIntPtr*)" />. The buffer follows the format described
		/// by JxlPixelFormat. The buffer is owned by the caller.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// size too small.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetImageOutBuffer(JxlDecoder* dec, JxlPixelFormat* format, void* buffer, UIntPtr size);

		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetImageOutCallback(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr/*delegate*<void*, UIntPtr, UIntPtr, UIntPtr, void*, void>*/ callback, void* opaque);

		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetMultithreadedImageOutCallback(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr/*delegate*<void*, UIntPtr, UIntPtr, void*>*/ init_callback, UIntPtr/*delegate*<void*, UIntPtr, UIntPtr, UIntPtr, UIntPtr, void*, void>*/ run_callback, UIntPtr/*delegate*<void*, void>*/ destroy_callback, void* init_opaque);

		/// <summary>
		/// Returns the minimum size in bytes of an extra channel pixel buffer for the
		/// given format. This is the buffer for <see cref="JxlDecoderSetExtraChannelBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr,System.UInt32)" />.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of the pixels. The num_channels value is ignored and is
		/// always treated to be 1.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in 
		/// <see cref="JxlDecoderGetExtraChannelInfo(JxlDecoder*,UIntPtr,JxlExtraChannelInfo*)" />. Must be smaller than num_extra_channels in
		/// the associated JxlBasicInfo.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// information not available yet or invalid index.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderExtraChannelBufferSize(JxlDecoder* dec, JxlPixelFormat* format, UIntPtr* size, uint index);

		/// <summary>
		/// Sets the buffer to write an extra channel to. This can be set when
		/// the <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> or <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER" /> event occurs,
		/// and applies only for the current frame. The size of the buffer must be at
		/// least as large as given by <see cref="JxlDecoderExtraChannelBufferSize(JxlDecoder*,JxlPixelFormat*,UIntPtr*,System.UInt32)" />. The buffer
		/// follows the format described by JxlPixelFormat, but where num_channels is 1.
		/// The buffer is owned by the caller. The amount of extra channels is given by
		/// the num_extra_channels field in the associated JxlBasicInfo, and the
		/// information of individual extra channels can be queried with 
		/// <see cref="JxlDecoderGetExtraChannelInfo(JxlDecoder*,UIntPtr,JxlExtraChannelInfo*)" />. To get multiple extra channels, this function
		/// must be called multiple times, once for each wanted index. Not all images
		/// have extra channels. The alpha channel is an extra channel and can be gotten
		/// as part of the color channels when using an RGBA pixel buffer with 
		/// <see cref="JxlDecoderSetImageOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" />, but additionally also can be gotten
		/// separately as extra channel. The color channels themselves cannot be gotten
		/// this way.
		/// <br /><br /></summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally. The num_channels value is ignored and is always
		/// treated to be 1.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in 
		/// <see cref="JxlDecoderGetExtraChannelInfo(JxlDecoder*,UIntPtr,JxlExtraChannelInfo*)" />. Must be smaller than num_extra_channels in
		/// the associated JxlBasicInfo.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// size too small or invalid index.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetExtraChannelBuffer(JxlDecoder* dec, JxlPixelFormat* format, void* buffer, UIntPtr size, uint index);

		/// <summary>
		/// Sets output buffer for reconstructed JPEG codestream.
		/// <br /><br />
		/// The data is owned by the caller and may be used by the decoder until 
		/// <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" /> is called or the decoder is destroyed or
		/// reset so must be kept alive until then.
		/// <br /><br />
		/// If a JPEG buffer was set before and released with 
		/// <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" />, bytes that the decoder has already output
		/// should not be included, only the remaining bytes output must be set.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if output buffer was already set and 
		/// <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" /> was not called on it, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" />
		/// otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetJPEGBuffer(JxlDecoder* dec, byte* data, UIntPtr size);

		/// <summary>
		/// Releases buffer which was provided with <see cref="JxlDecoderSetJPEGBuffer(JxlDecoder*,byte*,UIntPtr)" />.
		/// <br /><br />
		/// Calling <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" /> is required whenever
		/// a buffer is already set and a new buffer needs to be added with 
		/// <see cref="JxlDecoderSetJPEGBuffer(JxlDecoder*,byte*,UIntPtr)" />, but is not required before 
		/// <see cref="JxlDecoderDestroy(JxlDecoder*)" /> or <see cref="JxlDecoderReset(JxlDecoder*)" />.
		/// <br /><br />
		/// Calling <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" /> when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by <see cref="JxlDecoderSetJPEGBuffer(JxlDecoder*,byte*,UIntPtr)" />, or 0 if no buffer is set or 
		/// <see cref="JxlDecoderReleaseJPEGBuffer(JxlDecoder*)" /> was already called.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern UIntPtr JxlDecoderReleaseJPEGBuffer(JxlDecoder* dec);

		/// <summary>
		/// Sets output buffer for box output codestream.
		/// <br /><br />
		/// The data is owned by the caller and may be used by the decoder until 
		/// <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> is called or the decoder is destroyed or
		/// reset so must be kept alive until then.
		/// <br /><br />
		/// If for the current box a box buffer was set before and released with 
		/// <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" />, bytes that the decoder has already output
		/// should not be included, only the remaining bytes output must be set.
		/// <br /><br />
		/// The <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> must be used at the next <see cref="JxlDecoderStatus.JXL_DEC_BOX" />
		/// event or final <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> event to compute the size of the output
		/// box bytes.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if output buffer was already set and 
		/// <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> was not called on it, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" />
		/// otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetBoxBuffer(JxlDecoder* dec, byte* data, UIntPtr size);

		/// <summary>
		/// Releases buffer which was provided with <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" />.
		/// <br /><br />
		/// Calling <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> is required whenever
		/// a buffer is already set and a new buffer needs to be added with 
		/// <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" />, but is not required before 
		/// <see cref="JxlDecoderDestroy(JxlDecoder*)" /> or <see cref="JxlDecoderReset(JxlDecoder*)" />.
		/// <br /><br />
		/// Calling <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by <see cref="JxlDecoderSetBoxBuffer(JxlDecoder*,byte*,UIntPtr)" />, or 0 if no buffer is set or 
		/// <see cref="JxlDecoderReleaseBoxBuffer(JxlDecoder*)" /> was already called.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern UIntPtr JxlDecoderReleaseBoxBuffer(JxlDecoder* dec);

		/// <summary>
		/// Configures whether to get boxes in raw mode or in decompressed mode. In raw
		/// mode, boxes are output as their bytes appear in the container file, which may
		/// be decompressed, or compressed if their type is "brob". In decompressed mode,
		/// "brob" boxes are decompressed with Brotli before outputting them. The size of
		/// the decompressed stream is not known before the decompression has already
		/// finished.
		/// <br /><br />
		/// The default mode is raw. This setting can only be changed before decoding, or
		/// directly after a <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> event, and is remembered until the decoder
		/// is reset or destroyed.
		/// <br /><br />
		/// Enabling decompressed mode requires Brotli support from the library.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="decompress"> JXL_TRUE to transparently decompress, JXL_FALSE to get
		/// boxes in raw mode.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if decompressed mode is set and Brotli is not
		/// available, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetDecompressBoxes(JxlDecoder* dec, int decompress);

		/// <summary>
		/// Outputs the type of the current box, after a <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> event occured,
		/// as 4 characters without null termination character. In case of a compressed
		/// "brob" box, this will return "brob" if the decompressed argument is
		/// JXL_FALSE, or the underlying box type if the decompressed argument is
		/// JXL_TRUE.
		/// <br /><br />
		/// The following box types are currently described in ISO/IEC 18181-2:
		/// - "Exif": a box with EXIF metadata.  Starts with a 4-byte tiff header offset
		/// (big-endian uint32) that indicates the start of the actual EXIF data
		/// (which starts with a tiff header). Usually the offset will be zero and the
		/// EXIF data starts immediately after the offset field. The Exif orientation
		/// should be ignored by applications; the JPEG XL codestream orientation
		/// takes precedence and libjxl will by default apply the correct orientation
		/// automatically (see <see cref="JxlDecoderSetKeepOrientation(JxlDecoder*,int)" />).
		/// - "xml ": a box with XML data, in particular XMP metadata.
		/// - "jumb": a JUMBF superbox (JPEG Universal Metadata Box Format, ISO/IEC
		/// 19566-5).
		/// - "JXL ": mandatory signature box, must come first, 12 bytes long including
		/// the box header
		/// - "ftyp": a second mandatory signature box, must come second, 20 bytes long
		/// including the box header
		/// - "jxll": a JXL level box. This indicates if the codestream is level 5 or
		/// level 10 compatible. If not present, it is level 5. Level 10 allows more
		/// features such as very high image resolution and bit-depths above 16 bits
		/// per channel. Added automatically by the encoder when
		/// JxlEncoderSetCodestreamLevel is used
		/// - "jxlc": a box with the image codestream, in case the codestream is not
		/// split across multiple boxes. The codestream contains the JPEG XL image
		/// itself, including the basic info such as image dimensions, ICC color
		/// profile, and all the pixel data of all the image frames.
		/// - "jxlp": a codestream box in case it is split across multiple boxes.
		/// The contents are the same as in case of a jxlc box, when concatenated.
		/// - "brob": a Brotli-compressed box, which otherwise represents an existing
		/// type of box such as Exif or "xml ". When <see cref="JxlDecoderSetDecompressBoxes(JxlDecoder*,int)" />
		/// is set to JXL_TRUE, these boxes will be transparently decompressed by the
		/// decoder.
		/// - "jxli": frame index box, can list the keyframes in case of a JPEG XL
		/// animation allowing the decoder to jump to individual frames more
		/// efficiently.
		/// - "jbrd": JPEG reconstruction box, contains the information required to
		/// byte-for-byte losslessly recontruct a JPEG-1 image. The JPEG DCT
		/// coefficients (pixel content) themselves as well as the ICC profile are
		/// encoded in the JXL codestream (jxlc or jxlp) itself. EXIF, XMP and JUMBF
		/// metadata is encoded in the corresponding boxes. The jbrd box itself
		/// contains information such as the remaining app markers of the JPEG-1 file
		/// and everything else required to fit the information together into the
		/// exact original JPEG file.
		/// <br /><br />
		/// Other application-specific boxes can exist. Their typename should not begin
		/// with "jxl" or "JXL" or conflict with other existing typenames.
		/// <br /><br />
		/// The signature, jxl* and jbrd boxes are processed by the decoder and would
		/// typically be ignored by applications. The typical way to use this function is
		/// to check if an encountered box contains metadata that the application is
		/// interested in (e.g. EXIF or XMP metadata), in order to conditionally set a
		/// box buffer.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="type"> buffer to copy the type into</param>
		/// <param name="decompressed"> which box type to get: JXL_FALSE to get the raw box type,
		/// which can be "brob", JXL_TRUE, get the underlying box type.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if
		/// not, for example the JXL file does not use the container format.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetBoxType(JxlDecoder* dec, byte* type, int decompressed);

		/// <summary>
		/// Returns the size of a box as it appears in the container file, after the 
		/// <see cref="JxlDecoderStatus.JXL_DEC_BOX" /> event. For a non-compressed box, this is the size of the
		/// contents, excluding the 4 bytes indicating the box type. For a compressed
		/// "brob" box, this is the size of the compressed box contents plus the
		/// additional 4 byte indicating the underlying box type, but excluding the 4
		/// bytes indicating "brob". This function gives the size of the data that will
		/// be written in the output buffer when getting boxes in the default raw
		/// compressed mode. When <see cref="JxlDecoderSetDecompressBoxes(JxlDecoder*,int)" /> is enabled, the
		/// return value of function does not change, and the decompressed size is not
		/// known before it has already been decompressed and output.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="size"> raw size of the box in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> if no box size is available, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" />
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderGetBoxSizeRaw(JxlDecoder* dec, ulong* size);

		/// <summary>
		/// Configures at which progressive steps in frame decoding these 
		/// <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION" /> event occurs. The default value for the level
		/// of detail if this function is never called is `kDC`.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <param name="detail"> at which level of detail to trigger 
		/// <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION" /></param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> on error, such as
		/// an invalid value for the progressive detail.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderSetProgressiveDetail(JxlDecoder* dec, JxlProgressiveDetail detail);

		/// <summary>
		/// Returns the intended downsampling ratio for the progressive frame produced
		/// by <see cref="JxlDecoderFlushImage(JxlDecoder*)" /> after the latest <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION" />
		/// event.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns> The intended downsampling ratio, can be 1, 2, 4 or 8.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern UIntPtr JxlDecoderGetIntendedDownsamplingRatio(JxlDecoder* dec);

		/// <summary>
		/// Outputs progressive step towards the decoded image so far when only partial
		/// input was received. If the flush was successful, the buffer set with 
		/// <see cref="JxlDecoderSetImageOutBuffer(JxlDecoder*,JxlPixelFormat*,void*,UIntPtr)" /> will contain partial image data.
		/// <br /><br />
		/// Can be called when <see cref="JxlDecoderProcessInput(JxlDecoder*)" /> returns 
		/// <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT" />, after the <see cref="JxlDecoderStatus.JXL_DEC_FRAME" /> event already occurred
		/// and before the <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE" /> event occurred for a frame.
		/// </summary>
		/// <param name="dec"> decoder object</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS" /> if image data was flushed to the output buffer,
		/// or <see cref="JxlDecoderStatus.JXL_DEC_ERROR" /> when no flush was done, e.g. if not enough image
		/// data was available yet even for flush, or no output buffer was set yet.
		/// This error is not fatal, it only indicates no flushed image is available
		/// right now. Regular decoding can still be performed.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlDecoderStatus JxlDecoderFlushImage(JxlDecoder* dec);

		/// <summary>
		/// Encoder library version.
		/// </summary>
		/// <returns> the encoder library version as an integer:
		/// MAJOR_VERSION * 1000000 + MINOR_VERSION * 1000 + PATCH_VERSION. For example,
		/// version 1.2.3 would return 1002003.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal static extern uint JxlEncoderVersion();

		/// <summary>
		/// Creates an instance of JxlEncoder and initializes it.
		/// <br /><br /><tt>memory_manager</tt> will be used for all the library dynamic allocations made
		/// from this instance. The parameter may be NULL, in which case the default
		/// allocator will be used. See jpegxl/memory_manager.h for details.
		/// </summary>
		/// <param name="memory_manager"> custom allocator function. It may be NULL. The memory
		/// manager will be copied internally.</param>
		/// <returns>
		///     <tt>NULL</tt> if the instance can not be allocated or initialized</returns>
		/// <returns> pointer to initialized JxlEncoder otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoder* JxlEncoderCreate(JxlMemoryManager* memory_manager);

		/// <summary>
		/// Re-initializes a JxlEncoder instance, so it can be re-used for encoding
		/// another image. All state and settings are reset as if the object was
		/// newly created with JxlEncoderCreate, but the memory manager is kept.
		/// </summary>
		/// <param name="enc"> instance to be re-initialized.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderReset(JxlEncoder* enc);

		/// <summary>
		/// Deinitializes and frees JxlEncoder instance.
		/// </summary>
		/// <param name="enc"> instance to be cleaned up and deallocated.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderDestroy(JxlEncoder* enc);

		/// <summary>
		/// Sets the color management system (CMS) that will be used for color conversion
		/// (if applicable) during encoding. May only be set before starting encoding. If
		/// left unset, the default CMS implementation will be used.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="cms"> structure representing a CMS implementation. See JxlCmsInterface
		/// for more details.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderSetCms(JxlEncoder* enc, JxlCmsInterface cms);

		/// <summary>
		/// Set the parallel runner for multithreading. May only be set before starting
		/// encoding.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="parallel_runner"> function pointer to runner for multithreading. It may
		/// be NULL to use the default, single-threaded, runner. A multithreaded
		/// runner should be set to reach fast performance.</param>
		/// <param name="parallel_runner_opaque"> opaque pointer for parallel_runner.</param>
		/// <returns> JXL_ENC_SUCCESS if the runner was set, JXL_ENC_ERROR
		/// otherwise (the previous runner remains set).</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetParallelRunner(JxlEncoder* enc, IntPtr parallel_runner, void* parallel_runner_opaque);

		/// <summary>
		/// Get the (last) error code in case JXL_ENC_ERROR was returned.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <returns> the JxlEncoderError that caused the (last) JXL_ENC_ERROR to be
		/// returned.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderError JxlEncoderGetError(JxlEncoder* enc);

		/// <summary>
		/// Encodes JPEG XL file using the available bytes. 
		/// <tt>many</tt> output bytes are available, and 
		/// <tt>*avail_out will be decremented by the amount of bytes that have been</tt>
		/// processed by the encoder and *next_out will be incremented by the same
		/// amount, so *next_out will now point at the amount of *avail_out unprocessed
		/// bytes.
		/// <br /><br />
		/// The returned status indicates whether the encoder needs more output bytes.
		/// When the return value is not JXL_ENC_ERROR or JXL_ENC_SUCCESS, the encoding
		/// requires more JxlEncoderProcessOutput calls to continue.
		/// <br /><br />
		/// This encodes the frames and/or boxes added so far. If the last frame or last
		/// box has been added, <see cref="JxlEncoderCloseInput(JxlEncoder*)" />, <see cref="JxlEncoderCloseFrames(JxlEncoder*)" />
		/// and/or <see cref="JxlEncoderCloseBoxes(JxlEncoder*)" /> must be called before the next
		/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call, or the codestream won't be encoded
		/// correctly.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="next_out"> pointer to next bytes to write to.</param>
		/// <param name="avail_out"> amount of bytes available starting from *next_out.</param>
		/// <returns> JXL_ENC_SUCCESS when encoding finished and all events handled.</returns>
		/// <returns> JXL_ENC_ERROR when encoding failed, e.g. invalid input.</returns>
		/// <returns> JXL_ENC_NEED_MORE_OUTPUT more output buffer is necessary.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderProcessOutput(JxlEncoder* enc, byte** next_out, UIntPtr* avail_out);

		/// <summary>
		/// Sets the frame information for this frame to the encoder. This includes
		/// animation information such as frame duration to store in the frame header.
		/// The frame header fields represent the frame as passed to the encoder, but not
		/// necessarily the exact values as they will be encoded file format: the encoder
		/// could change crop and blending options of a frame for more efficient encoding
		/// or introduce additional internal frames. Animation duration and time code
		/// information is not altered since those are immutable metadata of the frame.
		/// <br /><br />
		/// It is not required to use this function, however if have_animation is set
		/// to true in the basic info, then this function should be used to set the
		/// time duration of this individual frame. By default individual frames have a
		/// time duration of 0, making them form a composite still. See 
		/// <see cref=JxlFrameHeader" /> for more information.
		/// <br /><br />
		/// This information is stored in the JxlEncoderFrameSettings and so is used for
		/// any frame encoded with these JxlEncoderFrameSettings. It is ok to change
		/// between <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" /> calls, each added image frame will have
		/// the frame header that was set in the options at the time of calling
		/// JxlEncoderAddImageFrame.
		/// <br /><br />
		/// The is_last and name_length fields of the JxlFrameHeader are ignored, use
		/// <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> to indicate last frame, and 
		/// <see cref="JxlEncoderSetFrameName(JxlEncoderFrameSettings*,byte*)" /> to indicate the name and its length instead.
		/// Calling this function will clear any name that was previously set with 
		/// <see cref="JxlEncoderSetFrameName(JxlEncoderFrameSettings*,byte*)" />.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="frame_header"> frame header data to set. Object owned by the caller and
		/// does not need to be kept in memory, its information is copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetFrameHeader(JxlEncoderFrameSettings* frame_settings, JxlFrameHeader* frame_header);

		/// <summary>
		/// Sets blend info of an extra channel. The blend info of extra channels is set
		/// separately from that of the color channels, the color channels are set with
		/// <see cref="JxlEncoderSetFrameHeader(JxlEncoderFrameSettings*,JxlFrameHeader*)" />.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="index"> index of the extra channel to use.</param>
		/// <param name="blend_info"> blend info to set for the extra channel</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetExtraChannelBlendInfo(JxlEncoderFrameSettings* frame_settings, UIntPtr index, JxlBlendInfo* blend_info);

		/// <summary>
		/// Sets the name of the animation frame. This function is optional, frames are
		/// not required to have a name. This setting is a part of the frame header, and
		/// the same principles as for <see cref="JxlEncoderSetFrameHeader(JxlEncoderFrameSettings*,JxlFrameHeader*)" /> apply. The
		/// name_length field of JxlFrameHeader is ignored by the encoder, this function
		/// determines the name length instead as the length in bytes of the C string.
		/// <br /><br />
		/// The maximum possible name length is 1071 bytes (excluding terminating null
		/// character).
		/// <br /><br />
		/// Calling <see cref="JxlEncoderSetFrameHeader(JxlEncoderFrameSettings*,JxlFrameHeader*)" /> clears any name that was
		/// previously set.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
		/// string (zero terminated). Owned by the caller, and copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetFrameName(JxlEncoderFrameSettings* frame_settings, byte* frame_name);

		/// <summary>
		/// Sets the buffer to read JPEG encoded bytes from for the next frame to encode.
		/// <br /><br />
		/// If JxlEncoderSetBasicInfo has not yet been called, calling
		/// JxlEncoderAddJPEGFrame will implicitly call it with the parameters of the
		/// added JPEG frame.
		/// <br /><br />
		/// If JxlEncoderSetColorEncoding or JxlEncoderSetICCProfile has not yet been
		/// called, calling JxlEncoderAddJPEGFrame will implicitly call it with the
		/// parameters of the added JPEG frame.
		/// <br /><br />
		/// If the encoder is set to store JPEG reconstruction metadata using 
		/// <see cref="JxlEncoderStoreJPEGMetadata(JxlEncoder*,int)" /> and a single JPEG frame is added, it will be
		/// possible to losslessly reconstruct the JPEG codestream.
		/// <br /><br />
		/// If this is the last frame, <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> or 
		/// <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> must be called before the next
		/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
		/// are copied internally.</param>
		/// <param name="size"> size of buffer in bytes.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderAddJPEGFrame(JxlEncoderFrameSettings* frame_settings, byte* buffer, UIntPtr size);

		/// <summary>
		/// Sets the buffer to read pixels from for the next image to encode. Must call
		/// JxlEncoderSetBasicInfo before JxlEncoderAddImageFrame.
		/// <br /><br />
		/// Currently only some data types for pixel formats are supported:
		/// - JXL_TYPE_UINT8, with range 0..255
		/// - JXL_TYPE_UINT16, with range 0..65535
		/// - JXL_TYPE_FLOAT16, with nominal range 0..1
		/// - JXL_TYPE_FLOAT, with nominal range 0..1
		/// <br /><br />
		/// Note: the sample data type in pixel_format is allowed to be different from
		/// what is described in the JxlBasicInfo. The type in pixel_format describes the
		/// format of the uncompressed pixel buffer. The bits_per_sample and
		/// exponent_bits_per_sample in the JxlBasicInfo describes what will actually be
		/// encoded in the JPEG XL codestream. For example, to encode a 12-bit image, you
		/// would set bits_per_sample to 12, and you could use e.g. JXL_TYPE_UINT16
		/// (where the values are rescaled to 16-bit, i.e. multiplied by 65535/4095) or
		/// JXL_TYPE_FLOAT (where the values are rescaled to 0..1, i.e. multiplied
		/// by 1.f/4095.f). While it is allowed, it is obviously not recommended to use a
		/// pixel_format with lower precision than what is specified in the JxlBasicInfo.
		/// <br /><br />
		/// We support interleaved channels as described by the JxlPixelFormat:
		/// - single-channel data, e.g. grayscale
		/// - single-channel + alpha
		/// - trichromatic, e.g. RGB
		/// - trichromatic + alpha
		/// <br /><br />
		/// Extra channels not handled here need to be set by 
		/// <see cref="JxlEncoderSetExtraChannelBuffer(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr,System.UInt32)" />.
		/// If the image has alpha, and alpha is not passed here, it will implicitly be
		/// set to all-opaque (an alpha value of 1.0 everywhere).
		/// <br /><br />
		/// The pixels are assumed to be encoded in the original profile that is set with
		/// JxlEncoderSetColorEncoding or JxlEncoderSetICCProfile. If none of these
		/// functions were used, the pixels are assumed to be nonlinear sRGB for integer
		/// data types (JXL_TYPE_UINT8, JXL_TYPE_UINT16), and linear sRGB for floating
		/// point data types (JXL_TYPE_FLOAT16, JXL_TYPE_FLOAT).
		/// <br /><br />
		/// Sample values in floating-point pixel formats are allowed to be outside the
		/// nominal range, e.g. to represent out-of-sRGB-gamut colors in the
		/// uses_original_profile=false case. They are however not allowed to be NaN or
		/// +-infinity.
		/// <br /><br />
		/// If this is the last frame, <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> or 
		/// <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> must be called before the next
		/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
		/// and its contents are copied internally.</param>
		/// <param name="size"> size of buffer in bytes. This size should match what is implied
		/// by the frame dimensions and the pixel format.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderAddImageFrame(JxlEncoderFrameSettings* frame_settings, JxlPixelFormat* pixel_format, void* buffer, UIntPtr size);

		/// <summary>
		/// Sets the buffer to read pixels from for an extra channel at a given index.
		/// The index must be smaller than the num_extra_channels in the associated
		/// JxlBasicInfo. Must call <see cref="JxlEncoderSetExtraChannelInfo(JxlEncoder*,UIntPtr,JxlExtraChannelInfo*)" /> before
		/// JxlEncoderSetExtraChannelBuffer.
		/// <br /><br />
		/// TODO(firsching): mention what data types in pixel formats are supported.
		/// <br /><br />
		/// It is required to call this function for every extra channel, except for the
		/// alpha channel if that was already set through <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" />.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
		/// contents are copied internally. The num_channels value is ignored, since the
		/// number of channels for an extra channel is always assumed to be one.</param>
		/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
		/// and its contents are copied internally.</param>
		/// <param name="size"> size of buffer in bytes. This size should match what is implied
		/// by the frame dimensions and the pixel format.</param>
		/// <param name="index"> index of the extra channel to use.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetExtraChannelBuffer(JxlEncoderFrameSettings* frame_settings, JxlPixelFormat* pixel_format, void* buffer, UIntPtr size, uint index);

		/// <summary>
		/// Adds a metadata box to the file format. JxlEncoderProcessOutput must be used
		/// to effectively write the box to the output. <see cref="JxlEncoderUseBoxes(JxlEncoder*)" /> must
		/// be enabled before using this function.
		/// <br /><br />
		/// Boxes allow inserting application-specific data and metadata (Exif, XML/XMP,
		/// JUMBF and user defined boxes).
		/// <br /><br />
		/// The box format follows ISO BMFF and shares features and box types with other
		/// image and video formats, including the Exif, XML and JUMBF boxes. The box
		/// format for JPEG XL is specified in ISO/IEC 18181-2.
		/// <br /><br />
		/// Boxes in general don't contain other boxes inside, except a JUMBF superbox.
		/// Boxes follow each other sequentially and are byte-aligned. If the container
		/// format is used, the JXL stream consists of concatenated boxes.
		/// It is also possible to use a direct codestream without boxes, but in that
		/// case metadata cannot be added.
		/// <br /><br />
		/// Each box generally has the following byte structure in the file:
		/// - 4 bytes: box size including box header (Big endian. If set to 0, an
		/// 8-byte 64-bit size follows instead).
		/// - 4 bytes: type, e.g. "JXL " for the signature box, "jxlc" for a codestream
		/// box.
		/// - N bytes: box contents.
		/// <br /><br />
		/// Only the box contents are provided to the contents argument of this function,
		/// the encoder encodes the size header itself. Most boxes are written
		/// automatically by the encoder as needed ("JXL ", "ftyp", "jxll", "jxlc",
		/// "jxlp", "jxli", "jbrd"), and this function only needs to be called to add
		/// optional metadata when encoding from pixels (using JxlEncoderAddImageFrame).
		/// When recompressing JPEG files (using JxlEncoderAddJPEGFrame), if the input
		/// JPEG contains EXIF, XMP or JUMBF metadata, the corresponding boxes are
		/// already added automatically.
		/// <br /><br />
		/// Box types are given by 4 characters. The following boxes can be added with
		/// this function:
		/// - "Exif": a box with EXIF metadata, can be added by libjxl users, or is
		/// automatically added when needed for JPEG reconstruction. The contents of
		/// this box must be prepended by a 4-byte tiff header offset, which may
		/// be 4 zero bytes in case the tiff header follows immediately.
		/// The EXIF metadata must be in sync with what is encoded in the JPEG XL
		/// codestream, specifically the image orientation. While this is not
		/// recommended in practice, in case of conflicting metadata, the JPEG XL
		/// codestream takes precedence.
		/// - "xml ": a box with XML data, in particular XMP metadata, can be added by
		/// libjxl users, or is automatically added when needed for JPEG reconstruction
		/// - "jumb": a JUMBF superbox, which can contain boxes with different types of
		/// metadata inside. This box type can be added by the encoder transparently,
		/// and other libraries to create and handle JUMBF content exist.
		/// - Application-specific boxes. Their typename should not begin with "jxl" or
		/// "JXL" or conflict with other existing typenames, and they should be
		/// registered with MP4RA (mp4ra.org).
		/// <br /><br />
		/// These boxes can be stored uncompressed or Brotli-compressed (using a "brob"
		/// box), depending on the compress_box parameter.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="type"> the box type, e.g. "Exif" for EXIF metadata, "xml " for XMP or
		/// IPTC metadata, "jumb" for JUMBF metadata.</param>
		/// <param name="contents"> the full contents of the box, for example EXIF
		/// data. ISO BMFF box header must not be included, only the contents. Owned by
		/// the caller and its contents are copied internally.</param>
		/// <param name="size"> size of the box contents.</param>
		/// <param name="compress_box"> Whether to compress this box as a "brob" box. Requires
		/// Brotli support.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error, such as when
		/// using this function without JxlEncoderUseContainer, or adding a box type
		/// that would result in an invalid file format.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderAddBox(JxlEncoder* enc, byte* type, byte* contents, UIntPtr size, int compress_box);

		/// <summary>
		/// Indicates the intention to add metadata boxes. This allows 
		/// <see cref="JxlEncoderAddBox(JxlEncoder*,byte*,byte*,UIntPtr,int)" /> to be used. When using this function, then it is required
		/// to use <see cref="JxlEncoderCloseBoxes(JxlEncoder*)" /> at the end.
		/// <br /><br />
		/// By default the encoder assumes no metadata boxes will be added.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderUseBoxes(JxlEncoder* enc);

		/// <summary>
		/// Declares that no further boxes will be added with <see cref="JxlEncoderAddBox(JxlEncoder*,byte*,byte*,UIntPtr,int)" />.
		/// This function must be called after the last box is added so the encoder knows
		/// the stream will be finished. It is not necessary to use this function if
		/// <see cref="JxlEncoderUseBoxes(JxlEncoder*)" /> is not used. Further frames may still be added.
		/// <br /><br />
		/// Must be called between JxlEncoderAddBox of the last box
		/// and the next call to JxlEncoderProcessOutput, or <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" />
		/// won't output the last box correctly.
		/// <br /><br />
		/// NOTE: if you don't need to close frames and boxes at separate times, you can
		/// use <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> instead to close both at once.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderCloseBoxes(JxlEncoder* enc);

		/// <summary>
		/// Declares that no frames will be added and <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" /> and
		/// <see cref="JxlEncoderAddJPEGFrame(JxlEncoderFrameSettings*,byte*,UIntPtr)" /> won't be called anymore. Further metadata boxes
		/// may still be added. This function or <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> must be called
		/// after adding the last frame and the next call to
		/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" />, or the frame won't be properly marked as last.
		/// <br /><br />
		/// NOTE: if you don't need to close frames and boxes at separate times, you can
		/// use <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> instead to close both at once.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderCloseFrames(JxlEncoder* enc);

		/// <summary>
		/// Closes any input to the encoder, equivalent to calling JxlEncoderCloseFrames
		/// as well as calling JxlEncoderCloseBoxes if needed. No further input of any
		/// kind may be given to the encoder, but further <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" />
		/// calls should be done to create the final output.
		/// <br /><br />
		/// The requirements of both <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> and 
		/// <see cref="JxlEncoderCloseBoxes(JxlEncoder*)" /> apply to this function. Either this function or the
		/// other two must be called after the final frame and/or box, and the next
		/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call, or the codestream won't be encoded
		/// correctly.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderCloseInput(JxlEncoder* enc);

		/// <summary>
		/// Sets the original color encoding of the image encoded by this encoder. This
		/// is an alternative to JxlEncoderSetICCProfile and only one of these two must
		/// be used. This one sets the color encoding as a <see cref=JxlColorEncoding" />, while
		/// the other sets it as ICC binary data.
		/// Must be called after JxlEncoderSetBasicInfo.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="color"> color encoding. Object owned by the caller and its contents are
		/// copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
		/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetColorEncoding(JxlEncoder* enc, JxlColorEncoding* color);

		/// <summary>
		/// Sets the original color encoding of the image encoded by this encoder as an
		/// ICC color profile. This is an alternative to JxlEncoderSetColorEncoding and
		/// only one of these two must be used. This one sets the color encoding as ICC
		/// binary data, while the other defines it as a <see cref=JxlColorEncoding" />.
		/// Must be called after JxlEncoderSetBasicInfo.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="icc_profile"> bytes of the original ICC profile</param>
		/// <param name="size"> size of the icc_profile buffer in bytes</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
		/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetICCProfile(JxlEncoder* enc, byte* icc_profile, UIntPtr size);

		/// <summary>
		/// Initializes a JxlBasicInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to an 8-bit RGB image, no alpha or any
		/// other extra channels.
		/// </summary>
		/// <param name="info"> global image metadata. Object owned by the caller.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderInitBasicInfo(JxlBasicInfo* info);

		/// <summary>
		/// Initializes a JxlFrameHeader struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to a frame with no animation duration and the
		/// 'replace' blend mode. After using this function, For animation duration must
		/// be set, for composite still blend settings must be set.
		/// </summary>
		/// <param name="frame_header"> frame metadata. Object owned by the caller.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderInitFrameHeader(JxlFrameHeader* frame_header);

		/// <summary>
		/// Initializes a JxlBlendInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// </summary>
		/// <param name="blend_info"> blending info. Object owned by the caller.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderInitBlendInfo(JxlBlendInfo* blend_info);

		/// <summary>
		/// Sets the global metadata of the image encoded by this encoder.
		/// <br /><br />
		/// If the JxlBasicInfo contains information of extra channels beyond an alpha
		/// channel, then <see cref="JxlEncoderSetExtraChannelInfo(JxlEncoder*,UIntPtr,JxlExtraChannelInfo*)" /> must be called between
		/// JxlEncoderSetBasicInfo and <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" />. In order to indicate
		/// extra channels, the value of `info.num_extra_channels` should be set to the
		/// number of extra channels, also counting the alpha channel if present.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="info"> global image metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful,
		/// JXL_ENC_ERROR or JXL_ENC_NOT_SUPPORTED otherwise</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetBasicInfo(JxlEncoder* enc, JxlBasicInfo* info);

		/// <summary>
		/// Initializes a JxlExtraChannelInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to an 8-bit channel of the provided type.
		/// </summary>
		/// <param name="type"> type of the extra channel.</param>
		/// <param name="info"> global extra channel metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlEncoderInitExtraChannelInfo(JxlExtraChannelType type, JxlExtraChannelInfo* info);

		/// <summary>
		/// Sets information for the extra channel at the given index. The index
		/// must be smaller than num_extra_channels in the associated JxlBasicInfo.
		/// </summary>
		/// <param name="enc"> encoder object</param>
		/// <param name="index"> index of the extra channel to set.</param>
		/// <param name="info"> global extra channel metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetExtraChannelInfo(JxlEncoder* enc, UIntPtr index, JxlExtraChannelInfo* info);

		/// <summary>
		/// Sets the name for the extra channel at the given index in UTF-8. The index
		/// must be smaller than the num_extra_channels in the associated JxlBasicInfo.
		/// <br /><br />
		/// TODO(lode): remove size parameter for consistency with
		/// JxlEncoderSetFrameName
		/// </summary>
		/// <param name="enc"> encoder object</param>
		/// <param name="index"> index of the extra channel to set.</param>
		/// <param name="name"> buffer with the name of the extra channel.</param>
		/// <param name="size"> size of the name buffer in bytes, not counting the terminating
		/// character.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetExtraChannelName(JxlEncoder* enc, UIntPtr index, byte* name, UIntPtr size);

		/// <summary>
		/// Sets a frame-specific option of integer type to the encoder options.
		/// The JxlEncoderFrameSettingId argument determines which option is set.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="option"> ID of the option to set.</param>
		/// <param name="value"> Integer value to set for this option.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR in
		/// case of an error, such as invalid or unknown option id, or invalid integer
		/// value for the given option. If an error is returned, the state of the
		/// JxlEncoderFrameSettings object is still valid and is the same as before this
		/// function was called.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderFrameSettingsSetOption(JxlEncoderFrameSettings* frame_settings, JxlEncoderFrameSettingId option, int value);

		/// <summary>
		/// Forces the encoder to use the box-based container format (BMFF) even
		/// when not necessary.
		/// <br /><br />
		/// When using <see cref="JxlEncoderUseBoxes(JxlEncoder*)" />, <see cref="JxlEncoderStoreJPEGMetadata(JxlEncoder*,int)" /> or 
		/// <see cref="JxlEncoderSetCodestreamLevel(JxlEncoder*,int)" /> with level 10, the encoder will automatically
		/// also use the container format, it is not necessary to use
		/// JxlEncoderUseContainer for those use cases.
		/// <br /><br />
		/// By default this setting is disabled.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="use_container"> true if the encoder should always output the JPEG XL
		/// container format, false to only output it when necessary.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderUseContainer(JxlEncoder* enc, int use_container);

		/// <summary>
		/// Configure the encoder to store JPEG reconstruction metadata in the JPEG XL
		/// container.
		/// <br /><br />
		/// If this is set to true and a single JPEG frame is added, it will be
		/// possible to losslessly reconstruct the JPEG codestream.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="store_jpeg_metadata"> true if the encoder should store JPEG metadata.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderStoreJPEGMetadata(JxlEncoder* enc, int store_jpeg_metadata);

		/// <summary>
		/// Sets the feature level of the JPEG XL codestream. Valid values are 5 and
		/// 10, or -1 (to choose automatically). Using the minimum required level, or
		/// level 5 in most cases, is recommended for compatibility with all decoders.
		/// <br /><br />
		/// Level 5: for end-user image delivery, this level is the most widely
		/// supported level by image decoders and the recommended level to use unless a
		/// level 10 feature is absolutely necessary. Supports a maximum resolution
		/// 268435456 pixels total with a maximum width or height of 262144 pixels,
		/// maximum 16-bit color channel depth, maximum 120 frames per second for
		/// animation, maximum ICC color profile size of 4 MiB, it allows all color
		/// models and extra channel types except CMYK and the JXL_CHANNEL_BLACK extra
		/// channel, and a maximum of 4 extra channels in addition to the 3 color
		/// channels. It also sets boundaries to certain internally used coding tools.
		/// <br /><br />
		/// Level 10: this level removes or increases the bounds of most of the level
		/// 5 limitations, allows CMYK color and up to 32 bits per color channel, but
		/// may be less widely supported.
		/// <br /><br />
		/// The default value is -1. This means the encoder will automatically choose
		/// between level 5 and level 10 based on what information is inside the 
		/// <see cref=JxlBasicInfo" /> structure. Do note that some level 10 features, particularly
		/// those used by animated JPEG XL codestreams, might require level 10, even
		/// though the <see cref=JxlBasicInfo" /> only suggests level 5. In this case, the level
		/// must be explicitly set to 10, otherwise the encoder will return an error.
		/// The encoder will restrict internal encoding choices to those compatible with
		/// the level setting.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="level"> the level value to set, must be -1, 5, or 10.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetCodestreamLevel(JxlEncoder* enc, int level);

		/// <summary>
		/// Returns the codestream level required to support the currently configured
		/// settings and basic info. This function can only be used at the beginning,
		/// before encoding starts, but after setting basic info.
		/// <br /><br />
		/// This does not support per-frame settings, only global configuration, such as
		/// the image dimensions, that are known at the time of writing the header of
		/// the JPEG XL file.
		/// <br /><br />
		/// If this returns 5, nothing needs to be done and the codestream can be
		/// compatible with any decoder. If this returns 10, JxlEncoderSetCodestreamLevel
		/// has to be used to set the codestream level to 10, or the encoder can be
		/// configured differently to allow using the more compatible level 5.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <returns> -1 if no level can support the configuration (e.g. image dimensions
		/// larger than even level 10 supports), 5 if level 5 is supported, 10 if setting
		/// the codestream level to 10 is required.
		/// </returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern int JxlEncoderGetRequiredCodestreamLevel(JxlEncoder* enc);

		/// <summary>
		/// Enables lossless encoding.
		/// <br /><br />
		/// This is not an option like the others on itself, but rather while enabled it
		/// overrides a set of existing options (such as distance, modular mode and
		/// color transform) that enables bit-for-bit lossless encoding.
		/// <br /><br />
		/// When disabled, those options are not overridden, but since those options
		/// could still have been manually set to a combination that operates losslessly,
		/// using this function with lossless set to JXL_DEC_FALSE does not guarantee
		/// lossy encoding, though the default set of options is lossy.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="lossless"> whether to override options for lossless mode</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetFrameLossless(JxlEncoderFrameSettings* frame_settings, int lossless);

		/// <summary>
		/// DEPRECATED: use JxlEncoderSetFrameLossless instead.
		/// </summary>
		[Obsolete]
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderOptionsSetLossless(JxlEncoderFrameSettings* frame_settings, int lossless);

		/// <summary />
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="effort"> the effort value to set.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.
		/// <br /><br />
		/// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
		/// JXL_ENC_FRAME_SETTING_EFFORT, effort) instead.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlEncoderStatus JxlEncoderOptionsSetEffort(JxlEncoderFrameSettings* frame_settings, int effort);

		/// <summary />
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="tier"> the decoding speed tier to set.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.
		/// <br /><br />
		/// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
		/// JXL_ENC_FRAME_SETTING_DECODING_SPEED, tier) instead.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlEncoderStatus JxlEncoderOptionsSetDecodingSpeed(JxlEncoderFrameSettings* frame_settings, int tier);

		/// <summary>
		/// Sets the distance level for lossy compression: target max butteraugli
		/// distance, lower = higher quality. Range: 0 .. 15.
		/// 0.0 = mathematically lossless (however, use JxlEncoderSetFrameLossless
		/// instead to use true lossless, as setting distance to 0 alone is not the only
		/// requirement). 1.0 = visually lossless. Recommended range: 0.5 .. 3.0. Default
		/// value: 1.0.
		/// </summary>
		/// <param name="frame_settings"> set of options and metadata for this frame. Also
		/// includes reference to the encoder object.</param>
		/// <param name="distance"> the distance value to set.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderStatus JxlEncoderSetFrameDistance(JxlEncoderFrameSettings* frame_settings, float distance);

		/// <summary>
		/// DEPRECATED: use JxlEncoderSetFrameDistance instead.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlEncoderStatus JxlEncoderOptionsSetDistance(JxlEncoderFrameSettings* A_0, float A_1);

		/// <summary>
		/// Create a new set of encoder options, with all values initially copied from
		/// the <tt>source</tt> options, or set to default if <tt>source</tt> is NULL.
		/// <br /><br />
		/// The returned pointer is an opaque struct tied to the encoder and it will be
		/// deallocated by the encoder when JxlEncoderDestroy() is called. For functions
		/// taking both a <see cref=JxlEncoder" /> and a <see cref=JxlEncoderFrameSettings" />, only
		/// JxlEncoderFrameSettings created with this function for the same encoder
		/// instance can be used.
		/// </summary>
		/// <param name="enc"> encoder object.</param>
		/// <param name="source"> source options to copy initial values from, or NULL to get
		/// defaults initialized to defaults.</param>
		/// <returns> the opaque struct pointer identifying a new set of encoder options.</returns>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern JxlEncoderFrameSettings* JxlEncoderFrameSettingsCreate(JxlEncoder* enc, JxlEncoderFrameSettings* source);

		/// <summary>
		/// DEPRECATED: use JxlEncoderFrameSettingsCreate instead.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		[Obsolete]
		internal unsafe static extern JxlEncoderFrameSettings* JxlEncoderOptionsCreate(JxlEncoder* A_0, JxlEncoderFrameSettings* A_1);

		/// <summary>
		/// Sets a color encoding to be sRGB.
		/// </summary>
		/// <param name="color_encoding"> color encoding instance.</param>
		/// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlColorEncodingSetToSRGB(JxlColorEncoding* color_encoding, int is_gray);

		/// <summary>
		/// Sets a color encoding to be linear sRGB.
		/// </summary>
		/// <param name="color_encoding"> color encoding instance.</param>
		/// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlColorEncodingSetToLinearSRGB(JxlColorEncoding* color_encoding, int is_gray);

		/// <summary>
		/// Parallel runner internally using std::thread. Use as JxlParallelRunner.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern int JxlResizableParallelRunner(void* runner_opaque, void* jpegxl_opaque, IntPtr init, IntPtr func, uint start_range, uint end_range);

		/// <summary>
		/// Creates the runner for JxlResizableParallelRunner. Use as the opaque
		/// runner. The runner will execute tasks on the calling thread until
		/// <see cref="JxlResizableParallelRunnerSetThreads(void*,UIntPtr)" /> is called.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void* JxlResizableParallelRunnerCreate(JxlMemoryManager* memory_manager);

		/// <summary>
		/// Changes the number of threads for JxlResizableParallelRunner.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlResizableParallelRunnerSetThreads(void* runner_opaque, UIntPtr num_threads);

		/// <summary>
		/// Suggests a number of threads to use for an image of given size.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal static extern uint JxlResizableParallelRunnerSuggestThreads(ulong xsize, ulong ysize);

		/// <summary>
		/// Destroys the runner created by JxlResizableParallelRunnerCreate.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlResizableParallelRunnerDestroy(void* runner_opaque);

		///// <summary>
		///// Parallel runner internally using std::thread. Use as JxlParallelRunner.
		///// </summary>
		//[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		//[MethodImpl(MethodImplOptions.ForwardRef)]
		//internal unsafe static extern int JxlThreadParallelRunner(void* runner_opaque, void* jpegxl_opaque, IntPtr init, IntPtr func, uint start_range, uint end_range);

		/// <summary>
		/// Creates the runner for JxlThreadParallelRunner. Use as the opaque
		/// runner.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void* JxlThreadParallelRunnerCreate(JxlMemoryManager* memory_manager, UIntPtr num_worker_threads);

		/// <summary>
		/// Destroys the runner created by JxlThreadParallelRunnerCreate.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal unsafe static extern void JxlThreadParallelRunnerDestroy(void* runner_opaque);

		/// <summary>
		/// Returns a default num_worker_threads value for
		/// JxlThreadParallelRunnerCreate.
		/// </summary>
		[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		[MethodImpl(MethodImplOptions.ForwardRef)]
		internal static extern UIntPtr JxlThreadParallelRunnerDefaultNumWorkerThreads();
	}
}
