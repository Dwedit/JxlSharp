using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

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
	/// The codestream animation header, optionally present in the beginning of
	/// the codestream, and if it is it applies to all animation frames, unlike
	/// JxlFrameHeader which applies to an individual frame.
	/// </summary>
	public struct JxlAnimationHeader
	{
		/// <summary>
		/// Construct a new JxlAnimationHeader object
		/// </summary>
		/// <param name="tpsNumerator">ticks per second numerator</param>
		/// <param name="tpsDenominator">ticks per second denominator</param>
		/// <param name="numLoops">number of loops</param>
		/// <param name="haveTimecodes">whether or not there are timecodes</param>
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
	/// Return value for <see cref="JxlDecoder.ProcessInput"/>.
	/// The values from <see cref="JxlBasicInfo"/> onwards are optional informative
	/// events that can be subscribed to, they are never returned if they
	/// have not been registered with <see cref="JxlDecoder.SubscribeEvents"/>.
	/// </summary>
	[Flags]
	public enum JxlDecoderStatus
	{
		/// <summary>
		/// Function call finished successfully, or decoding is finished and there is
		/// nothing more to be done.
		/// <br /><br />
		/// Note that <see cref="JxlDecoder.ProcessInput" /> will return <see cref="JxlDecoderStatus.Success"/> if all
		/// events that were registered with <see cref="JxlDecoder.SubscribeEvents" /> were
		/// processed, even before the end of the JPEG XL codestream.
		/// <br /><br />
		/// In this case, the return value <see cref="JxlDecoder.ReleaseInput" /> will be the same
		/// as it was at the last signaled event. E.g. if <see cref="JxlDecoderStatus.FullImage"/> was
		/// subscribed to, then all bytes from the end of the JPEG XL codestream
		/// (including possible boxes needed for jpeg reconstruction) will be returned
		/// as unprocessed.
		/// </summary>
		Success = 0,

		/// <summary>
		/// An error occurred, for example invalid input file or out of memory.
		/// <br/> TODO(lode): add function to get error information from decoder.
		/// </summary>
		Error = 1,

		/// <summary>
		/// The decoder needs more input bytes to continue. Before the next
		/// <see cref="JxlDecoder.ProcessInput"/> call, more input data must be set, by calling 
		/// <see cref="JxlDecoder.ReleaseInput"/> (if input was set previously) and then calling
		/// <see cref="JxlDecoder.SetInput"/>. <see cref="JxlDecoder.ReleaseInput"/> returns how many bytes
		/// are not yet processed, before a next call to <see cref="JxlDecoder.ProcessInput"/>
		/// all unprocessed bytes must be provided again (the address need not match,
		/// but the contents must), and more bytes must be concatenated after the
		/// unprocessed bytes.
		/// <br/>In most cases, <see cref="JxlDecoder.ReleaseInput" /> will return no unprocessed bytes
		/// at this event, the only exceptions are if the previously set input ended
		/// within (a) the raw codestream signature, (b) the signature box, (c) a box
		/// header, or (d) the first 4 bytes of a brob, ftyp, or jxlp box. In any of
		/// these cases the number of unprocessed bytes is less than 20.
		/// </summary>
		NeedMoreInput = 2,

		/// <summary>
		/// The decoder is able to decode a preview image and requests setting a
		/// preview output buffer using <see cref="JxlDecoder.SetPreviewOutBuffer"/>. This occurs
		/// if <see cref="PreviewImage"/> is requested and it is possible to decode a
		/// preview image from the codestream and the preview out buffer was not yet
		/// set. There is maximum one preview image in a codestream.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the frame header (including ToC) of the preview frame as
		/// unprocessed.
		/// </summary>
		NeedPreviewOutBuffer = 3,

		/// <summary>
		/// The decoder is able to decode a DC image and requests setting a DC output
		/// buffer using <see cref="JxlDecoder.SetDCOutBuffer"/>. This occurs if <see cref="DcImage"/> is requested and it is possible to decode a DC image from
		/// the codestream and the DC out buffer was not yet set. This event re-occurs
		/// for new frames if there are multiple animation frames.
		/// @deprecated The DC feature in this form will be removed. For progressive
		/// rendering, <see cref="JxlDecoder.FlushImage"/> should be used.
		/// </summary>
		NeedDcOutBuffer = 4,

		/// <summary>
		/// The decoder requests an output buffer to store the full resolution image,
		/// which can be set with <see cref="JxlDecoder.SetImageOutBuffer"/> or with <see cref="JxlDecoder.SetImageOutCallback"/>. This event re-occurs for new frames if
		/// there are multiple animation frames and requires setting an output again.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the frame header (including ToC) as unprocessed.
		/// </summary>
		NeedImageOutBuffer = 5,

		/// <summary>
		/// The JPEG reconstruction buffer is too small for reconstructed JPEG
		/// codestream to fit. <see cref="JxlDecoder.SetJPEGBuffer"/> must be called again to
		/// make room for remaining bytes. This event may occur multiple times
		/// after <see cref="JpegReconstruction"/>.
		/// </summary>
		JpegNeedMoreOutput = 6,

		/// <summary>
		/// The box contents output buffer is too small. <see cref="JxlDecoder.SetBoxBuffer"/>
		/// must be called again to make room for remaining bytes. This event may occur
		/// multiple times after <see cref="Box"/>.
		/// </summary>
		BoxNeedMoreOutput = 7,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": Basic information such as image dimensions and
		/// extra channels. This event occurs max once per image.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the basic info as unprocessed (including the last byte of basic info
		/// if it did not end on a byte boundary).
		/// </summary>
		BasicInfo = 0x40,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": User extensions of the codestream header. This
		/// event occurs max once per image and always later than <see cref="BasicInfo"/> and earlier than any pixel data.
		/// <br/><br/>
		/// @deprecated The decoder no longer returns this, the header extensions,
		/// if any, are available at the BasicInfo event.
		/// </summary>
		Extensions = 0x80,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": Color encoding or ICC profile from the
		/// codestream header. This event occurs max once per image and always later
		/// than <see cref="BasicInfo"/> and earlier than any pixel data.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the image header (which is the start of the first frame) as
		/// unprocessed.
		/// </summary>
		ColorEncoding = 0x100,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": Preview image, a small frame, decoded. This
		/// event can only happen if the image has a preview frame encoded. This event
		/// occurs max once for the codestream and always later than <see cref="ColorEncoding"/> and before <see cref="Frame"/>.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the preview frame as unprocessed.
		/// </summary>
		PreviewImage = 0x200,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": Beginning of a frame. <see cref="JxlDecoder.GetFrameHeader"/> can be used at this point. A note on frames:
		/// a JPEG XL image can have internal frames that are not intended to be
		/// displayed (e.g. used for compositing a final frame), but this only returns
		/// displayed frames, unless <see cref="JxlDecoder.SetCoalescing"/> was set to false:
		/// in that case, the individual layers are returned, without blending. Note
		/// that even when coalescing is disabled, only frames of type kRegularFrame
		/// are returned; frames of type kReferenceOnly and kLfFrame are always for
		/// internal purposes only and cannot be accessed. A displayed frame either has
		/// an animation duration or is the only or last frame in the image. This event
		/// occurs max once per displayed frame, always later than <see cref="ColorEncoding"/>, and always earlier than any pixel data. While
		/// JPEG XL supports encoding a single frame as the composition of multiple
		/// internal sub-frames also called frames, this event is not indicated for the
		/// internal frames.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the frame header (including ToC) as unprocessed.
		/// </summary>
		Frame = 0x400,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": DC image, 8x8 sub-sampled frame, decoded. It is
		/// not guaranteed that the decoder will always return DC separately, but when
		/// it does it will do so before outputting the full frame. <see cref="JxlDecoder.SetDCOutBuffer"/> must be used after getting the basic image
		/// information to be able to get the DC pixels, if not this return status only
		/// indicates we're past this point in the codestream. This event occurs max
		/// once per frame and always later than <see cref="Frame"/> and other header
		/// events and earlier than full resolution pixel data.
		/// <br/><br/>
		/// @deprecated The DC feature in this form will be removed. For progressive
		/// rendering, <see cref="JxlDecoder.FlushImage"/> should be used.
		/// </summary>
		DcImage = 0x800,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": full frame (or layer, in case coalescing is
		/// disabled) is decoded. <see cref="JxlDecoder.SetImageOutBuffer"/> must be used after
		/// getting the basic image information to be able to get the image pixels, if
		/// not this return status only indicates we're past this point in the
		/// codestream. This event occurs max once per frame and always later than <see cref="DcImage"/>.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the frame (or if <see cref="JpegReconstruction" /> is subscribed to,
		/// from the end of the last box that is needed for jpeg reconstruction) as
		/// unprocessed.
		/// </summary>
		FullImage = 0x1000,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": JPEG reconstruction data decoded. <see cref="JxlDecoder.SetJPEGBuffer(byte[])"/> may be used to set a JPEG reconstruction buffer
		/// after getting the JPEG reconstruction data. If a JPEG reconstruction buffer
		/// is set a byte stream identical to the JPEG codestream used to encode the
		/// image will be written to the JPEG reconstruction buffer instead of pixels
		/// to the image out buffer. This event occurs max once per image and always
		/// before <see cref="FullImage"/>.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the 'jbrd' box as unprocessed.
		/// </summary>
		JpegReconstruction = 0x2000,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": The header of a box of the container format
		/// (BMFF) is decoded. The following API functions related to boxes can be used
		/// after this event:
		/// - <see cref="JxlDecoder.SetBoxBuffer"/> and <see cref="JxlDecoder.ReleaseBoxBuffer"/>
		/// "JxlDecoderReleaseBoxBuffer": set and release a buffer to get the box
		/// data.
		/// - <see cref="JxlDecoder.GetBoxType"/> get the 4-character box typename.
		/// - <see cref="JxlDecoder.GetBoxSizeRaw"/> get the size of the box as it appears in
		/// the container file, not decompressed.
		/// - <see cref="JxlDecoder.SetDecompressBoxes"/> to configure whether to get the box
		/// data decompressed, or possibly compressed.
		/// <br/><br/>
		/// Boxes can be compressed. This is so when their box type is
		/// "brob". In that case, they have an underlying decompressed box
		/// type and decompressed data. <see cref="JxlDecoder.SetDecompressBoxes"/> allows
		/// configuring which data to get. Decompressing requires
		/// Brotli. <see cref="JxlDecoder.GetBoxType"/> has a flag to get the compressed box
		/// type, which can be "brob", or the decompressed box type. If a box
		/// is not compressed (its compressed type is not "brob"), then
		/// the output decompressed box type and data is independent of what
		/// setting is configured.
		/// <br/><br/>
		/// The buffer set with <see cref="JxlDecoder.SetBoxBuffer"/> must be set again for each
		/// next box to be obtained, or can be left unset to skip outputting this box.
		/// The output buffer contains the full box data when the next <see cref="Box"/>
		/// event or <see cref="Success"/> occurs. <see cref="Box"/> occurs for all
		/// boxes, including non-metadata boxes such as the signature box or codestream
		/// boxes. To check whether the box is a metadata type for respectively EXIF,
		/// XMP or JUMBF, use <see cref="JxlDecoder.GetBoxType"/> and check for types "Exif",
		/// "xml " and "jumb" respectively.
		/// <br /><br />
		/// In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// start of the box header as unprocessed.
		/// </summary>
		Box = 0x4000,

		/// <summary>
		/// Informative event by <see cref="JxlDecoder.ProcessInput"/>
		/// "JxlDecoderProcessInput": a progressive step in decoding the frame is
		/// reached. When calling <see cref="JxlDecoder.FlushImage"/> at this point, the flushed
		/// image will correspond exactly to this point in decoding, and not yet
		/// contain partial results (such as partially more fine detail) of a next
		/// step. By default, this event will trigger maximum once per frame, when a
		/// 8x8th resolution (DC) image is ready (the image data is still returned at
		/// full resolution, giving upscaled DC). Use <see cref="JxlDecoder.SetProgressiveDetail"/> to configure more fine-grainedness. The
		/// event is not guaranteed to trigger, not all images have progressive steps
		/// or DC encoded.
		/// <br/>In this case, <see cref="JxlDecoder.ReleaseInput" /> will return all bytes from the
		/// end of the section that was needed to produce this progressive event as
		/// unprocessed.
		/// </summary>
		FrameProgression = 0x8000,
	}

	/// <summary>
	/// Data type for the sample values per channel per pixel for the output buffer
	/// for pixels. This is not necessarily the same as the data type encoded in the
	/// codestream. The channels are interleaved per pixel. The pixels are
	/// organized row by row, left to right, top to bottom.
	/// <br/><br/>
	/// Set the Align property to the stride of the Bitmap.
	/// <br/><br/>
	/// TODO(lode): implement padding / alignment (row stride)
	/// <br/>TODO(lode): support different channel orders if needed (RGB, BGR, ...)
	/// </summary>
	public class JxlPixelFormat
	{
		/// <summary>
		/// Create a JxlPixelFormat given a <see cref="System.Drawing.Imaging.PixelFormat"/>
		/// </summary>
		/// <param name="pixelFormat"></param>
		public JxlPixelFormat(System.Drawing.Imaging.PixelFormat pixelFormat)
		{
			this.NumChannels = JXL.GetBytesPerPixel(pixelFormat);
			this.DataType = JxlDataType.UInt8;
		}
		/// <summary>
		/// Create a default pixel format for 24-bit RGB
		/// </summary>
		public JxlPixelFormat()
		{
			this.NumChannels = 3;
			this.DataType = JxlDataType.UInt8;
		}
		internal UnsafeNativeJxl.JxlPixelFormat pixelFormat;

		/// <summary>
		/// Amount of channels available in a pixel buffer.
		/// <br/> 1: single-channel data, e.g. grayscale or a single extra channel
		/// <br/> 2: single-channel + alpha
		/// <br/> 3: trichromatic, e.g. RGB
		/// <br/> 4: trichromatic + alpha
		/// <br/> TODO(lode): this needs finetuning. It is not yet defined how the user
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
				pixelFormat.data_type = (UnsafeNativeJxl.JxlDataType)value;
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
				pixelFormat.endianness = (UnsafeNativeJxl.JxlEndianness)value;
			}
		}

		/// <summary>
		/// Align scanlines to a multiple of align bytes, or 0 to require no
		/// alignment at all (which has the same effect as value 1)
		/// <br/><br/>
		/// You can set this value to the Stride of the bitmap.
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
		/// <summary>
		/// Create a default BasicInfo initialized to the defaults for encoding images
		/// </summary>
		public JxlBasicInfo()
		{
			UnsafeNativeJxl.InitBasicInfo(out basicInfo);
			this.UsesOriginalProfile = true;
		}

		internal JxlBasicInfo(ref UnsafeNativeJxl.JxlBasicInfo basicInfo)
		{
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref basicInfo, this);
		}

		internal UnsafeNativeJxl.JxlBasicInfo basicInfo;

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
		/// functions needs to be called with <see cref="JxlColorProfileTarget.Data"/> to get
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
				basicInfo.orientation = (UnsafeNativeJxl.JxlOrientation)value;
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
	/// CIE 1931 XY value (two doubles)
	/// </summary>
	public struct XYValue
	{
		/// <summary>
		/// Constructs a new XYPair object
		/// </summary>
		/// <param name="x">X value</param>
		/// <param name="y">Y value</param>
		public XYValue(double x, double y)
		{
			this.X = x;
			this.Y = y;
		}
		/// <summary>
		/// X value
		/// </summary>
		public double X { get; set; }
		/// <summary>
		/// Y value
		/// </summary>
		public double Y { get; set; }
	}

	/// <summary>
	/// Color encoding of the image as structured information.
	/// </summary>
	public class JxlColorEncoding
	{
		/// <summary>
		/// Sets this Color Encoding to SRGB
		/// </summary>
		/// <param name="isGray">If true, makes it grayscale</param>
		public void SetToSRGB(bool isGray)
		{
			UnsafeNativeJxl.JxlColorEncodingSetToSRGB(out this.colorEncoding, isGray);
		}
		/// <summary>
		/// Sets this Color Encoding to SRGB
		/// </summary>
		public void SetToSRGB()
		{
			UnsafeNativeJxl.JxlColorEncodingSetToSRGB(out this.colorEncoding, false);
		}
		/// <summary>
		/// Sets this Color Encoding to Linear RGB
		/// </summary>
		/// <param name="isGray">If true, makes it grayscale</param>
		public void SetToLinearSRGB(bool isGray)
		{
			UnsafeNativeJxl.JxlColorEncodingSetToLinearSRGB(out this.colorEncoding, isGray);
		}
		/// <summary>
		/// Sets this Color Encoding to Linear RGB
		/// </summary>
		public void SetToLinearSRGB()
		{
			UnsafeNativeJxl.JxlColorEncodingSetToLinearSRGB(out this.colorEncoding, false);
		}

		internal UnsafeNativeJxl.JxlColorEncoding colorEncoding;

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
				colorEncoding.color_space = (UnsafeNativeJxl.JxlColorSpace)value;
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
				colorEncoding.white_point = (UnsafeNativeJxl.JxlWhitePoint)value;
			}
		}
		/// <summary>
		/// Numerical whitepoint values in CIE xy space.
		/// </summary>
		public XYValue WhitePointXY
		{
			get
			{
				unsafe
				{
					return new XYValue(colorEncoding.white_point_xy.x, colorEncoding.white_point_xy.y);
				}
			}
			set
			{
				unsafe
				{
					colorEncoding.white_point_xy.x = value.X;
					colorEncoding.white_point_xy.y = value.Y;
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
				colorEncoding.primaries = (UnsafeNativeJxl.JxlPrimaries)value;
			}
		}

		/// <summary>
		/// Numerical red primary values in CIE xy space.
		/// </summary>
		public XYValue PrimariesRedXY
		{
			get
			{
				unsafe
				{
					return new XYValue(colorEncoding.primaries_red_xy.x, colorEncoding.primaries_red_xy.y);
				}
			}
			set
			{
				unsafe
				{
					colorEncoding.primaries_red_xy.x = value.X;
					colorEncoding.primaries_red_xy.y = value.Y;
				}
			}
		}

		/// <summary>
		/// Numerical green primary values in CIE xy space.
		/// </summary>
		public XYValue PrimariesGreenXY
		{
			get
			{
				unsafe
				{
					return new XYValue(colorEncoding.primaries_green_xy.x, colorEncoding.primaries_green_xy.y);
				}
			}
			set
			{
				unsafe
				{
					colorEncoding.primaries_green_xy.x = value.X;
					colorEncoding.primaries_green_xy.y = value.Y;
				}
			}
		}

		/// <summary>
		/// Numerical blue primary values in CIE xy space.
		/// </summary>
		public XYValue PrimariesBlueXY
		{
			get
			{
				unsafe
				{
					return new XYValue(colorEncoding.primaries_blue_xy.x, colorEncoding.primaries_blue_xy.y);
				}
			}
			set
			{
				unsafe
				{
					colorEncoding.primaries_blue_xy.x = value.X;
					colorEncoding.primaries_blue_xy.y = value.Y;
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
				colorEncoding.transfer_function = (UnsafeNativeJxl.JxlTransferFunction)value;
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
				colorEncoding.rendering_intent = (UnsafeNativeJxl.JxlRenderingIntent)value;
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
		static JxlBlendInfo _defaultValues;
		static JxlBlendInfo()
		{
			UnsafeNativeJxl.JxlBlendInfo blendInfo;
			UnsafeNativeJxl.InitBlendInfo(out blendInfo);
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref blendInfo, out _defaultValues);
		}

		/// <summary>
		/// Initializes the object to the default values
		/// </summary>
		public void Initialize()
		{
			this.BlendMode = _defaultValues.BlendMode;
			this.Source = _defaultValues.Source;
			this.Alpha = _defaultValues.Alpha;
			this.Clamp = _defaultValues.Clamp;
		}

		/// <summary>
		/// Creates a JxlBlendInfo object using the default values
		/// </summary>
		/// <param name="unused">Not used</param>
		public JxlBlendInfo(bool unused)
		{
			this.BlendMode = _defaultValues.BlendMode;
			this.Source = _defaultValues.Source;
			this.Alpha = _defaultValues.Alpha;
			this.Clamp = _defaultValues.Clamp;
		}


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
		private static UnsafeNativeJxl.JxlFrameHeader _defaultFrameHeader;
		static JxlFrameHeader()
		{
			UnsafeNativeJxl.InitFrameHeader(out _defaultFrameHeader);
		}

		/// <summary>
		/// Creates a new JxlFrameHeader
		/// </summary>
		public JxlFrameHeader()
		{
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref _defaultFrameHeader, this);
		}

		internal JxlFrameHeader(ref UnsafeNativeJxl.JxlFrameHeader header2)
		{
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref header2, this);
		}

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
		/// required to set this value and it is ignored, <see cref="JxlEncoder.CloseFrames"/> is
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
	/// Floating point RGBA color (Linear)
	/// </summary>
	public struct RGBAFloat
	{
		/// <summary>
		/// Red component
		/// </summary>
		public float R;
		/// <summary>
		/// Green component
		/// </summary>
		public float G;
		/// <summary>
		/// Blue component
		/// </summary>
		public float B;
		/// <summary>
		/// Alpha component
		/// </summary>
		public float A;
	}

	/// <summary>
	/// Information for a single extra channel.
	/// </summary>
	public class JxlExtraChannelInfo
	{
		/// <summary>
		/// Creates a new JxlExtraChannelInfo object given a type
		/// </summary>
		/// <param name="type">The type of extra channel to create</param>
		public JxlExtraChannelInfo(JxlExtraChannelType type)
		{
			UnsafeNativeJxl.JxlExtraChannelInfo extraChannelInfo;
			UnsafeNativeJxl.InitExtraChannelInfo((UnsafeNativeJxl.JxlExtraChannelType)type, out extraChannelInfo);
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref extraChannelInfo, this);
		}

		internal JxlExtraChannelInfo(ref UnsafeNativeJxl.JxlExtraChannelInfo extraChannelInfo)
		{
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref extraChannelInfo, this);
		}

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
		/// <br/> TODO(lode): expand this comment to match the JPEG XL specification,
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
		public RGBAFloat SpotColor;

		/// <summary>
		/// Only applicable if type is JXL_CHANNEL_CFA.
		/// <br/> TODO(lode): add comment about the meaning of this field.
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
		Data = 1,
	}

	/// <summary>
	/// Error conditions:
	/// API usage errors have the 0x80 bit set to 1
	/// Other errors have the 0x80 bit set to 0
	/// </summary>
	public enum JxlEncoderError
	{
		/// <summary>
		/// No error
		/// </summary>
		Ok = 0,
		/// <summary>
		/// Generic encoder error due to unspecified cause
		/// </summary>
		GenericError = 1,
		/// <summary>
		/// Out of memory
		/// <br/> TODO(jon): actually catch this and return this error
		/// </summary>
		OutOfMemory = 2,
		/// <summary>
		/// JPEG bitstream reconstruction data could not be
		/// represented (e.g. too much tail data)
		/// </summary>
		JpegBitstreamReconstructionData = 3,
		/// <summary>
		/// Input is invalid (e.g. corrupt JPEG file or ICC profile)
		/// </summary>
		BadInput = 4,
		/// <summary>
		/// The encoder doesn't (yet) support this. Either no version of libjxl
		/// supports this, and the API is used incorrectly, or the libjxl version
		/// should have been checked before trying to do this.
		/// </summary>
		NotSupported = 128,
		/// <summary>
		/// The encoder API is used in an incorrect way.
		/// In this case, a debug build of libjxl should output a specific error
		/// message. (if not, please open an issue about it)
		/// </summary>
		ApiUsageError = 129
	}

	/// <summary>
	/// Return value for multiple encoder functions.
	/// </summary>
	public enum JxlEncoderStatus
	{
		/// <summary>
		/// Function call finished successfully, or encoding is finished and there is
		/// nothing more to be done.
		/// </summary>
		Success,
		/// <summary>
		/// An error occurred, for example out of memory.
		/// </summary>
		Error,
		/// <summary>
		/// The encoder needs more output buffer to continue encoding.
		/// </summary>
		NeedMoreOutput,
		/// <summary>
		/// DEPRECATED: the encoder does not return this status and there is no need
		/// to handle or expect it.
		/// Instead, JXL_ENC_ERROR is returned with error condition
		/// JXL_ENC_ERR_NOT_SUPPORTED.
		/// </summary>
		NotSupported
	}

	public class RangeAttribute : Attribute
	{
		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		public RangeAttribute(int minValue, int maxValue)
		{
			this.MinValue = minValue;
			this.MaxValue = maxValue;
		}
		public RangeAttribute() { }
	}

	/// <summary>
	/// Id of encoder options for a frame. This includes options such as setting
	/// encoding effort/speed or overriding the use of certain coding tools, for this
	/// frame. This does not include non-frame related encoder options such as for
	/// boxes.
	/// </summary>
	public enum JxlEncoderFrameSettingId
	{
		/// <summary>
		/// Sets encoder effort/speed level without affecting decoding speed. Valid
		/// values are, from faster to slower speed: 1:lightning 2:thunder 3:falcon
		/// 4:cheetah 5:hare 6:wombat 7:squirrel 8:kitten 9:tortoise.
		/// Default: squirrel (7).
		/// </summary>
		[Description("Sets encoder effort/speed level without affecting decoding speed.\r\n" +
			"Valid values are, from faster to slower speed:\r\n" +
			"1:lightning, 2:thunder, 3:falcon, 4:cheetah, 5:hare, 6:wombat, 7:squirrel, 8:kitten, 9:tortoise\r\n" +
			"Default: squirrel (7)")]
		[DefaultValue(7)]
		[Range(1, 9)]
		Effort = 0,
		/// <summary>
		/// Sets the decoding speed tier for the provided options. Minimum is 0
		/// (slowest to decode, best quality/density), and maximum is 4 (fastest to
		/// decode, at the cost of some quality/density). Default is 0.
		/// </summary>
		[Description("Sets the decoding speed tier for the provided options.\r\n" +
			" Minimum is 0  (slowest to decode, best quality/density), and maximum is 4 (fastest to " +
			"decode, at the cost of some quality/density).\r\n" +
			"Default is 0.")]
		[DefaultValue(0)]
		[Range(0, 4)]
		DecodingSpeed = 1,
		/// <summary>
		/// Sets resampling option. If enabled, the image is downsampled before
		/// compression, and upsampled to original size in the decoder. Integer option,
		/// use -1 for the default behavior (resampling only applied for low quality),
		/// 1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for 4x4
		/// downsampling, 8 for 8x8 downsampling.
		/// </summary>
		[Description("Sets resampling option.\r\n" +
			"If enabled, the image is downsampled before compression, and upsampled to original size in the decoder.\r\n" +
			"Integer option, use -1 for the default behavior (resampling only applied for low quality), " +
			"1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for 4x4 " +
			"downsampling, 8 for 8x8 downsampling. ")]
		[DefaultValue(-1)]
		[Range(1, 4)]
		Resampling = 2,
		/// <summary>
		/// Similar to RESAMPLING, but for extra channels.
		/// Integer option, use -1 for the default behavior (depends on encoder
		/// implementation), 1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for
		/// 4x4 downsampling, 8 for 8x8 downsampling.
		/// </summary>
		[Description("Similar to RESAMPLING, but for extra channels.\r\n" +
			"Integer option, use -1 for the default behavior (depends on encoder implementation), " +
			"1 for no downsampling (1x1), 2 for 2x2 downsampling, 4 for 4x4 downsampling, " +
			"8 for 8x8 downsampling. ")]
		[DefaultValue(-1)]
		[Range(1, 8)]
		ExtraChannelResampling = 3,
		/// <summary>
		/// Indicates the frame added with <see cref="M:JxlSharp.UnsafeNativeJxl.JxlEncoderAddImageFrame(JxlSharp.UnsafeNativeJxl.JxlEncoderFrameSettings*,JxlSharp.UnsafeNativeJxl.JxlPixelFormat*,System.Void*,System.UIntPtr)" /> is already
		/// downsampled by the downsampling factor set with 
		/// <see cref="F:JxlSharp.UnsafeNativeJxl.JxlEncoderFrameSettingId.RESAMPLING" />. The input frame must then be given in the
		/// downsampled resolution, not the full image resolution. The downsampled
		/// resolution is given by ceil(xsize / resampling), ceil(ysize / resampling)
		/// with xsize and ysize the dimensions given in the basic info, and resampling
		/// the factor set with <see cref="F:JxlSharp.UnsafeNativeJxl.JxlEncoderFrameSettingId.RESAMPLING" />.
		/// Use 0 to disable, 1 to enable. Default value is 0.
		/// </summary>
		[Description("Indicates the frame added with JxlEncoderAddImageFrame is already " +
		"downsampled by the downsampling factor set with  " +
		"Resampling.\r\n" + "The input frame must then be given in the " +
		"downsampled resolution, not the full image resolution. The downsampled " +
		"resolution is given by ceil(xsize / resampling), ceil(ysize / resampling) " +
		"with xsize and ysize the dimensions given in the basic info, and resampling " +
		"the factor set with Resampling\r\n" +
		"Use 0 to disable, 1 to enable. Default value is 0. ")]
		[Browsable(false)]
		[DefaultValue(0)]
		[Range(0, 1)]
		AlreadyDownsampled = 4,
		/// <summary>
		/// Adds noise to the image emulating photographic film noise, the higher the
		/// given number, the grainier the image will be. As an example, a value of 100
		/// gives low noise whereas a value of 3200 gives a lot of noise. The default
		/// value is 0.
		/// </summary>
		[Description("Adds noise to the image emulating photographic film noise, the higher the given number, " +
			"the grainier the image will be.\r\n" +
			"As an example, a value of 100 gives low noise whereas a value of 3200 gives a lot of noise.\r\n" +
			"The default value is 0.")]
		[DefaultValue(0)]
		[Range(0, 3200)]
		PhotonNoise = 5,
		/// <summary>
		/// Enables adaptive noise generation. This setting is not recommended for
		/// use, please use PHOTON_NOISE instead. Use -1 for the
		/// default (encoder chooses), 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables adaptive noise generation. This setting is not recommended for use, " +
			"please use PHOTON_NOISE instead.\r\n" +
			"Use -1 for the default (encoder chooses), 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Noise = 6,
		/// <summary>
		/// Enables or disables dots generation. Use -1 for the default (encoder
		/// chooses), 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables or disables dots generation.\r\n" +
			"Use -1 for the default (encoder chooses), 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Dots = 7,
		/// <summary>
		/// Enables or disables patches generation. Use -1 for the default (encoder
		/// chooses), 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables or disables patches generation.\r\n" +
			"Use -1 for the default (encoder chooses), 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Patches = 8,
		/// <summary>
		/// Edge preserving filter level, -1 to 3. Use -1 for the default (encoder
		/// chooses), 0 to 3 to set a strength.
		/// </summary>
		[Description("Edge preserving filter level, -1 to 3.\r\n" +
			"Use -1 for the default (encoder  chooses), 0 to 3 to set a strength. ")]
		[DefaultValue(-1)]
		[Range(0, 3)]
		EPF = 9,
		/// <summary>
		/// Enables or disables the gaborish filter. Use -1 for the default (encoder
		/// chooses), 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables or disables the gaborish filter\r\n" +
			"Use -1 for the default (encoder chooses), 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Gaborish = 10,
		/// <summary>
		/// Enables modular encoding. Use -1 for default (encoder
		/// chooses), 0 to enforce VarDCT mode (e.g. for photographic images), 1 to
		/// enforce modular mode (e.g. for lossless images).
		/// </summary>
		[Browsable(false)]
		[Description("Enables modular encoding rather than VarDCT mode.\r\n" +
			"Use -1 for default (encoder chooses), " +
			"0 to enforce VarDCT mode (e.g. for photographic images), " +
			"1 to enforce modular mode (e.g. for lossless images). ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Modular = 11,
		/// <summary>
		/// Enables or disables preserving color of invisible pixels. Use -1 for the
		/// default (1 if lossless, 0 if lossy), 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables or disables preserving color of invisible pixels.\r\n" +
			"Use -1 for the default (1 if lossless, 0 if lossy), 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		KeepInvisible = 12,
		/// <summary>
		/// Determines the order in which 256x256 regions are stored in the codestream
		/// for progressive rendering. Use -1 for the encoder
		/// default, 0 for scanline order, 1 for center-first order.
		/// </summary>
		[Description("Determines the order in which 256x256 regions are stored in the codestream " +
			"for progressive rendering.\r\n" +
			"Use -1 for the encoder default, 0 for scanline order, 1 for center-first order.")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		GroupOrder = 13,
		/// <summary>
		/// Determines the horizontal position of center for the center-first group
		/// order. Use -1 to automatically use the middle of the image, 0..xsize to
		/// specifically set it.
		/// </summary>
		[Description("Determines the horizontal position of center for the center-first group order.\r\n" +
			"Use -1 to automatically use the middle of the image, 0..xsize to specifically set it.")]
		[DefaultValue(-1)]
		GroupOrderCenterX = 14,
		/// <summary>
		/// Determines the center for the center-first group order. Use -1 to
		/// automatically use the middle of the image, 0..ysize to specifically set it.
		/// </summary>
		[Description("Determines the center for the center-first group order.\r\n" +
			"Use -1 to automatically use the middle of the image, 0..ysize to specifically set it.")]
		[DefaultValue(-1)]
		GroupOrderCenterY = 15,
		/// <summary>
		/// Enables or disables progressive encoding for modular mode. Use -1 for the
		/// encoder default, 0 to disable, 1 to enable.
		/// </summary>
		[Description("Enables or disables progressive encoding for modular mode.\r\n" +
			"Use -1 for the encoder default, 0 to disable, 1 to enable.")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		Responsive = 16,
		/// <summary>
		/// Set the progressive mode for the AC coefficients of VarDCT, using spectral
		/// progression from the DCT coefficients. Use -1 for the encoder default, 0 to
		/// disable, 1 to enable.
		/// </summary>
		[Description("Set the progressive mode for the AC coefficients of VarDCT, " +
			"using spectral progression from the DCT coefficients.\r\n" +
			"Use -1 for the encoder default, 0 to disable, 1 to enable. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		ProgressiveAC = 17,
		/// <summary>
		/// Set the progressive mode for the AC coefficients of VarDCT, using
		/// quantization of the least significant bits. Use -1 for the encoder default,
		/// 0 to disable, 1 to enable.
		/// </summary>
		[Description("Set the progressive mode for the AC coefficients of VarDCT, " +
			"using quantization of the least significant bits.\r\n" +
			"Use -1 for the encoder default, 0 to disable, 1 to enable.")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		QProgressiveAC = 18,
		/// <summary>
		/// Set the progressive mode using lower-resolution DC images for VarDCT. Use
		/// -1 for the encoder default, 0 to disable, 1 to have an extra 64x64 lower
		/// resolution pass, 2 to have a 512x512 and 64x64 lower resolution pass.
		/// </summary>
		[Description("Set the progressive mode using lower-resolution DC images for VarDCT.\r\n" +
			"Use -1 for the encoder default, 0 to disable, 1 to have an extra 64x64 lower resolution pass, " +
			"2 to have a 512x512 and 64x64 lower resolution pass. ")]
		[DefaultValue(-1)]
		[Range(0, 2)]
		ProgressiveDC = 19,
		/// <summary>
		/// Use Global channel palette if the amount of colors is smaller than this
		/// percentage of range. Use 0-100 to set an explicit percentage, -1 to use the
		/// encoder default. Used for modular encoding.
		/// </summary>
		[Description("Use Global channel palette if the amount of colors is smaller than this percentage of range.\r\n" +
			"Use 0-100 to set an explicit percentage, -1 to use the encoder default. Used for modular encoding.")]
		[DefaultValue(-1)]
		[Range(0, 100)]
		ChannelColorsGlobalPercent = 20,
		/// <summary>
		/// Use Local (per-group) channel palette if the amount of colors is smaller
		/// than this percentage of range. Use 0-100 to set an explicit percentage, -1
		/// to use the encoder default. Used for modular encoding.
		/// </summary>
		[Description("Use Local (per-group) channel palette if the amount of colors is smaller " +
			"than this percentage of range.\r\n" +
			"Use 0-100 to set an explicit percentage, -1 to use the encoder default. Used for modular encoding.")]
		[DefaultValue(-1)]
		[Range(0, 100)]
		ChannelColorsGroupPercent = 21,
		/// <summary>
		/// Use color palette if amount of colors is smaller than or equal to this
		/// amount, or -1 to use the encoder default. Used for modular encoding.
		/// </summary>
		[Description("Use color palette if amount of colors is smaller than or equal to this amount, " +
			"or -1 to use the encoder default. Used for modular encoding. ")]
		[DefaultValue(-1)]
		[Range(1, 256)]
		PaletteColors = 22,
		/// <summary>
		/// Enables or disables delta palette. Use -1 for the default (encoder
		/// chooses), 0 to disable, 1 to enable. Used in modular mode.
		/// </summary>
		[Description("Enables or disables delta palette.\r\n" +
			"Use -1 for the default (encoder chooses), 0 to disable, 1 to enable. Used in modular mode. ")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		LossyPalette = 23,
		/// <summary>
		/// Color transform for internal encoding: -1 = default, 0=XYB, 1=none (RGB),
		/// 2=YCbCr. The XYB setting performs the forward XYB transform. None and
		/// YCbCr both perform no transform, but YCbCr is used to indicate that the
		/// encoded data losslessly represents YCbCr values.
		/// </summary>
		[Description("Color transform for internal encoding:\r\n" +
			"-1 = default, 0=XYB, 1=none (RGB), 2=YCbCr.\r\n" +
			"The XYB setting performs the forward XYB transform. None and " +
			"YCbCr both perform no transform, but YCbCr is used to indicate that the " +
			"encoded data losslessly represents YCbCr values. ")]
		[DefaultValue(-1)]
		[Range(0, 2)]
		ColorTransform = 24,
		/// <summary>
		/// Reversible color transform for modular encoding: -1=default, 0-41=RCT
		/// index, e.g. index 0 = none, index 6 = YCoCg.
		/// If this option is set to a non-default value, the RCT will be globally
		/// applied to the whole frame.
		/// The default behavior is to try several RCTs locally per modular group,
		/// depending on the speed and distance setting.
		/// </summary>
		[Description("Reversible color transform for modular encoding:\r\n" +
			"-1=default, 0-41=RCT index, e.g. index 0 = none, index 6 = YCoCg.\r\n" +
			"If this option is set to a non-default value, the RCT will be globally " +
			"applied to the whole frame.\r\n" +
			"The default behavior is to try several RCTs locally per modular group, " +
			"depending on the speed and distance setting.")]
		[DefaultValue(-1)]
		[Range(0, 41)]
		ModularColorSpace = 25,
		/// <summary>
		/// Group size for modular encoding: -1=default, 0=128, 1=256, 2=512, 3=1024.
		/// </summary>
		[Description("Group size for modular encoding:\r\n-1=default, 0=128, 1=256, 2=512, 3=1024.")]
		[DefaultValue(-1)]
		[Range(0, 3)]
		ModularGroupSize = 26,
		/// <summary>
		/// Predictor for modular encoding. -1 = default, 0=zero, 1=left, 2=top,
		/// 3=avg0, 4=select, 5=gradient, 6=weighted, 7=topright, 8=topleft,
		/// 9=leftleft, 10=avg1, 11=avg2, 12=avg3, 13=toptop predictive average 14=mix
		/// 5 and 6, 15=mix everything.
		/// </summary>
		[Description("Predictor for modular encoding.\r\n" +
			"-1 = default, 0=zero, 1=left, 2=top, 3=avg0, 4=select, 5=gradient, 6=weighted, " +
			"7=topright, 8=topleft,  9=leftleft, 10=avg1, 11=avg2, 12=avg3, 13=toptop predictive average, " +
			"14=mix 5 and 6, 15=mix everything. ")]
		[DefaultValue(-1)]
		[Range(0, 15)]
		ModularPredictor = 27,
		/// <summary>
		/// Fraction of pixels used to learn MA trees as a percentage. -1 = default,
		/// 0 = no MA and fast decode, 50 = default value, 100 = all, values above
		/// 100 are also permitted. Higher values use more encoder memory.
		/// </summary>
		[Description("Fraction of pixels used to learn MA trees as a percentage.\r\n" +
			"-1 = default, 0 = no MA and fast decode, 50 = default value, 100 = all, values above " +
		"100 are also permitted.\r\n" +
			"Higher values use more encoder memory.")]
		[DefaultValue(-1)]
		[Range(0, 200)]
		ModularMaTreeLearningPercent = 28,
		/// <summary>
		/// Number of extra (previous-channel) MA tree properties to use. -1 =
		/// default, 0-11 = valid values. Recommended values are in the range 0 to 3,
		/// or 0 to amount of channels minus 1 (including all extra channels, and
		/// excluding color channels when using VarDCT mode). Higher value gives slower
		/// encoding and slower decoding.
		/// </summary>
		[Description("Number of extra (previous-channel) MA tree properties to use.\r\n" +
			"-1 = default, 0-11 = valid values. Recommended values are in the range 0 to 3, " +
		"or 0 to amount of channels minus 1 (including all extra channels, and " +
		"excluding color channels when using VarDCT mode).\r\nHigher value gives slower " +
		"encoding and slower decoding.")]
		[DefaultValue(-1)]
		[Range(0, 11)]
		ModularNbPrevChannels = 29,
		/// <summary>
		/// Enable or disable CFL (chroma-from-luma) for lossless JPEG recompression.
		/// -1 = default, 0 = disable CFL, 1 = enable CFL.
		/// </summary>
		[Description("Enable or disable CFL (chroma-from-luma) for lossless JPEG recompression.\r\n" +
		"-1 = default, 0 = disable CFL, 1 = enable CFL.")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		JpegReconChromaFromLuma = 30,
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
		[Description("Prepare the frame for indexing in the frame index box.\r\n" +
		" 0 = ignore this frame (same as not setting a value)," +
		" 1 = index this frame within the Frame Index Box.\r\n" +
		" If any frames are indexed, the first frame needs to" +
		" be indexed, too. If the first frame is not indexed, and" +
		" a later frame is attempted to be indexed, JXL_ENC_ERROR will occur." +
		" If non-keyframes, i.e., frames with cropping, blending or patches are" +
		" attempted to be indexed, JXL_ENC_ERROR will occur.")]
		[DefaultValue(-1)]
		[Range(0, 1)]
		FrameIndexBox = 31,
		/// <summary>
		/// Sets brotli encode effort for use in JPEG recompression and compressed
		/// metadata boxes (brob). Can be -1 (default) or 0 (fastest) to 11 (slowest).
		/// Default is based on the general encode effort in case of JPEG
		/// recompression, and 4 for brob boxes.
		/// </summary>
		[Description("Sets brotli encode effort for use in JPEG recompression and compressed metadata boxes (brob).\r\n" +
			"Can be -1 (default) or 0 (fastest) to 11 (slowest).\r\n" +
		"Default is based on the general encode effort in case of JPEG recompression, and 4 for brob boxes.")]
		[DefaultValue(-1)]
		[Range(0, 11)]
		BrotliEffort = 32,
	}

	//Remove this class if building for .NET Framework 2.0:
	/// <summary>
	/// Extension method for getting the description of the enum value
	/// </summary>
	public static class JxlEncoderFrameSettingIdExtensions
	{
		/// <summary>
		/// Returns the description of the enum value
		/// </summary>
		/// <param name="settingId">The setting to return the description of</param>
		/// <returns>The description if one exists, otherwise ""</returns>
		public static string GetDescription(this JxlEncoderFrameSettingId settingId)
		{
			var members = typeof(JxlEncoderFrameSettingId).GetMember(settingId.ToString());
			if (members == null || members.Length == 0) return "";
			var member = members[0];
			var attributes = member.GetCustomAttributes(typeof(DescriptionAttribute), false);
			if (attributes == null || attributes.Length == 0) return "";
			var descriptionAttribute = (DescriptionAttribute)attributes[0];
			return descriptionAttribute.Description;
		}
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
	public enum JxlProgressiveDetail
	{
		/// <summary>
		/// after completed kRegularFrames
		/// </summary>
		Frames,
		/// <summary>
		/// after completed DC (1:8)
		/// </summary>
		DC,
		/// <summary>
		/// after completed AC passes that are the last pass for their resolution target.
		/// </summary>
		LastPasses,
		/// <summary>
		/// after completed AC passes that are not the last pass for their resolution target.
		/// </summary>
		Passes,
		/// <summary>
		/// during DC frame when lower resolution are completed (1:32, 1:16)
		/// </summary>
		DCProgressive,
		/// <summary>
		/// after completed DC groups
		/// </summary>
		DCGroups,
		/// <summary>
		/// after completed groups
		/// </summary>
		Groups
	}
}
