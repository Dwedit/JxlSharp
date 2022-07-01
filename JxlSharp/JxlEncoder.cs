using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
	/// <summary>
	/// Class to encode JPEG-XL images.  Use CreateFrameSettings to the rest of the encoding functions.
	/// </summary>
	/// <example>
	/// Usage example:
	/// <code>
	///			MemoryStream stream = new MemoryStream();
	///			using (var encoder = new JxlEncoder(stream))
	///			{
	///				JxlEncoderStatus status;
	///				JxlBasicInfo basicInfo = new JxlBasicInfo();
	///				//Set properties of basicInfo here
	///				//Set Width, Height, NumColorChannels, NumChannels
	///				//For Alpha, also set NumExtraChannels, AlphaBits
	///				status = encoder.SetBasicInfo(basicInfo);
	///				JxlPixelFormat pixelFormat = new JxlPixelFormat();
	///				//Set properties of pixelFormat here
	///				//Set NumChannels to 3 for color, 4 for color+alpha, 1 for grayscale
	///				//Usually set DataType to JxlDataType.UInt8
	///				//Set Align to the stride (or pitch) of the bitmap (must be positive)
	///				JxlColorEncoding colorEncoding = new JxlColorEncoding();
	///				//Usually, you will set color encoding to SRGB
	///				colorEncoding.SetToSRGB(false);
	///				status = encoder.SetColorEncoding(colorEncoding);
	///				//Most actions are done with a FrameSettings object
	///				JxlEncoderFrameSettings frameSettings = encoder.CreateFrameSettings();
	///				//call frameSettings.FrameSettingsSetOption for encoding settings
	///				//call frameSettings.JxlEncoderSetFrameDistance to select the lossy quality
	///				status = frameSettings.AddImageFrame(pixelFormat, bytes);
	///				//must call CloseFrames and CloseInput to finalize the image, otherwise a truncated (invalid) file will be generated
	///				encoder.CloseFrames();
	///				encoder.CloseInput();
	///				//Call ProcessOutput to write the output to the stream
	///				status = encoder.ProcessOutput();
	///				//return the contents of the stream
	///				return stream.ToArray();
	///			}
	/// </code>
	/// </example>
	public class JxlEncoder : IDisposable
	{
		UnsafeNativeJxl.JxlEncoderWrapper encoderWrapper;

		public JxlEncoder(Stream outputStream) : this(outputStream, 16 * 1024 * 1024)
		{

		}

		public JxlEncoder(Stream outputStream, int outputBufferSize)
		{
			this.encoderWrapper = new UnsafeNativeJxl.JxlEncoderWrapper(outputStream, outputBufferSize);
		}

		public bool IsDisposed
		{
			get
			{
				return encoderWrapper.IsDisposed;
			}
		}

		/// <summary>
		/// Disposes the Decoder Wrapper object
		/// </summary>
		public void Dispose()
		{
			encoderWrapper.Dispose();
		}

		/// <summary>
		/// Re-initializes a JxlEncoder instance, so it can be re-used for encoding
		/// another image. All state and settings are reset as if the object was
		/// newly created with JxlEncoderCreate, but the memory manager is kept.
		/// </summary>
		public void Reset()
		{
			encoderWrapper.Reset();
		}

		///// <summary>
		///// Sets the color management system (CMS) that will be used for color conversion
		///// (if applicable) during encoding. May only be set before starting encoding. If
		///// left unset, the default CMS implementation will be used.
		///// </summary>
		///// <param name="cms"> structure representing a CMS implementation. See JxlCmsInterface
		///// for more details.</param>
		//public void SetCms(JxlCmsInterface cms)
		//{
		//	//TODO
		//	encoderWrapper.SetCms(cms);
		//}

		/// <summary>
		/// Get the (last) error code in case JXL_ENC_ERROR was returned.
		/// </summary>
		/// <returns> the JxlEncoderError that caused the (last) JXL_ENC_ERROR to be
		/// returned.</returns>
		public JxlEncoderError GetError()
		{
			return (JxlEncoderError)encoderWrapper.GetError();
		}

		/// <summary>
		/// Encodes JPEG XL file
		/// <br /><br />
		/// The returned status indicates whether the encoder needs more output bytes.
		/// When the return value is not JXL_ENC_ERROR or JXL_ENC_SUCCESS, the encoding
		/// requires more JxlEncoderProcessOutput calls to continue.
		/// <br /><br />
		/// This encodes the frames and/or boxes added so far. If the last frame or last
		/// box has been added, <see cref="CloseInput" />, <see cref="CloseFrames" />
		/// and/or <see cref="CloseBoxes" /> must be called before the next
		/// <see cref="ProcessOutput" /> call, or the codestream won't be encoded
		/// correctly.
		/// </summary>
		/// <returns> JXL_ENC_SUCCESS when encoding finished and all events handled.</returns>
		/// <returns> JXL_ENC_ERROR when encoding failed, e.g. invalid input.</returns>
		public JxlEncoderStatus ProcessOutput()
		{
			return (JxlEncoderStatus)encoderWrapper.ProcessOutput();
		}

		/// <summary>
		/// Adds a metadata box to the file format. JxlEncoderProcessOutput must be used
		/// to effectively write the box to the output. <see cref="UseBoxes" /> must
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
		/// <br/> - 4 bytes: box size including box header (Big endian. If set to 0, an
		/// 8-byte 64-bit size follows instead).
		/// <br/> - 4 bytes: type, e.g. "JXL " for the signature box, "jxlc" for a codestream
		/// box.
		/// <br/> - N bytes: box contents.
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
		/// <br/> - "Exif": a box with EXIF metadata, can be added by libjxl users, or is
		/// automatically added when needed for JPEG reconstruction. The contents of
		/// this box must be prepended by a 4-byte tiff header offset, which may
		/// be 4 zero bytes in case the tiff header follows immediately.
		/// The EXIF metadata must be in sync with what is encoded in the JPEG XL
		/// codestream, specifically the image orientation. While this is not
		/// recommended in practice, in case of conflicting metadata, the JPEG XL
		/// codestream takes precedence.
		/// <br/> - "xml ": a box with XML data, in particular XMP metadata, can be added by
		/// libjxl users, or is automatically added when needed for JPEG reconstruction
		/// <br/> - "jumb": a JUMBF superbox, which can contain boxes with different types of
		/// metadata inside. This box type can be added by the encoder transparently,
		/// and other libraries to create and handle JUMBF content exist.
		/// <br/> - Application-specific boxes. Their typename should not begin with "jxl" or
		/// "JXL" or conflict with other existing typenames, and they should be
		/// registered with MP4RA (mp4ra.org).
		/// <br /><br />
		/// These boxes can be stored uncompressed or Brotli-compressed (using a "brob"
		/// box), depending on the compress_box parameter.
		/// </summary>
		/// <param name="boxType"> the box type, e.g. "Exif" for EXIF metadata, "xml " for XMP or
		/// IPTC metadata, "jumb" for JUMBF metadata.</param>
		/// <param name="contents"> the full contents of the box, for example EXIF
		/// data. ISO BMFF box header must not be included, only the contents. Owned by
		/// the caller and its contents are copied internally.</param>
		/// <param name="compressBox"> Whether to compress this box as a "brob" box. Requires
		/// Brotli support.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error, such as when
		/// using this function without JxlEncoderUseContainer, or adding a box type
		/// that would result in an invalid file format.</returns>
		public JxlEncoderStatus AddBox(string boxType, byte[] contents, bool compressBox)
		{
			return (JxlEncoderStatus)encoderWrapper.AddBox(boxType, contents, compressBox);
		}

		/// <summary>
		/// Indicates the intention to add metadata boxes. This allows 
		/// <see cref="AddBox" /> to be used. When using this function, then it is required
		/// to use <see cref="CloseBoxes" /> at the end.
		/// <br /><br />
		/// By default the encoder assumes no metadata boxes will be added.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		public JxlEncoderStatus UseBoxes()
		{
			return (JxlEncoderStatus)encoderWrapper.UseBoxes();
		}

		/// <summary>
		/// Declares that no further boxes will be added with <see cref="AddBox" />.
		/// This function must be called after the last box is added so the encoder knows
		/// the stream will be finished. It is not necessary to use this function if
		/// <see cref="UseBoxes" /> is not used. Further frames may still be added.
		/// <br /><br />
		/// Must be called between JxlEncoderAddBox of the last box
		/// and the next call to JxlEncoderProcessOutput, or <see cref="ProcessOutput" />
		/// won't output the last box correctly.
		/// <br /><br />
		/// NOTE: if you don't need to close frames and boxes at separate times, you can
		/// use <see cref="CloseInput" /> instead to close both at once.
		/// </summary>
		public void CloseBoxes()
		{
			encoderWrapper.CloseBoxes();
		}

		/// <summary>
		/// Declares that no frames will be added and <see cref="JxlEncoderFrameSettings.AddImageFrame" /> and
		/// <see cref="JxlEncoderFrameSettings.AddJPEGFrame" /> won't be called anymore. Further metadata boxes
		/// may still be added. This function or <see cref="CloseInput" /> must be called
		/// after adding the last frame and the next call to
		/// <see cref="ProcessOutput" />, or the frame won't be properly marked as last.
		/// <br /><br />
		/// NOTE: if you don't need to close frames and boxes at separate times, you can
		/// use <see cref="CloseInput" /> instead to close both at once.
		/// </summary>
		public void CloseFrames()
		{
			encoderWrapper.CloseFrames();
		}

		/// <summary>
		/// Closes any input to the encoder, equivalent to calling JxlEncoderCloseFrames
		/// as well as calling JxlEncoderCloseBoxes if needed. No further input of any
		/// kind may be given to the encoder, but further <see cref="ProcessOutput" />
		/// calls should be done to create the final output.
		/// <br /><br />
		/// The requirements of both <see cref="CloseFrames" /> and 
		/// <see cref="CloseBoxes" /> apply to this function. Either this function or the
		/// other two must be called after the final frame and/or box, and the next
		/// <see cref="ProcessOutput" /> call, or the codestream won't be encoded
		/// correctly.
		/// </summary>
		public void CloseInput()
		{
			encoderWrapper.CloseInput();
		}

		/// <summary>
		/// Sets the original color encoding of the image encoded by this encoder. This
		/// is an alternative to JxlEncoderSetICCProfile and only one of these two must
		/// be used. This one sets the color encoding as a <see cref="T:JxlSharp.UnsafeNativeJxl.JxlColorEncoding" />, while
		/// the other sets it as ICC binary data.
		/// Must be called after JxlEncoderSetBasicInfo.
		/// </summary>
		/// <param name="color"> color encoding. Object owned by the caller and its contents are
		/// copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
		/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
		public JxlEncoderStatus SetColorEncoding(JxlColorEncoding color)
		{
			return (JxlEncoderStatus)encoderWrapper.SetColorEncoding(ref color.colorEncoding);
		}

		/// <summary>
		/// Sets the original color encoding of the image encoded by this encoder as an
		/// ICC color profile. This is an alternative to JxlEncoderSetColorEncoding and
		/// only one of these two must be used. This one sets the color encoding as ICC
		/// binary data, while the other defines it as a <see cref="T:JxlSharp.UnsafeNativeJxl.JxlColorEncoding" />.
		/// Must be called after JxlEncoderSetBasicInfo.
		/// </summary>
		/// <param name="icc_profile"> bytes of the original ICC profile</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
		/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
		public JxlEncoderStatus SetICCProfile(byte[] icc_profile)
		{
			return (JxlEncoderStatus)encoderWrapper.SetICCProfile(icc_profile);
		}

		/// <summary>
		/// Sets the global metadata of the image encoded by this encoder.
		/// <br /><br />
		/// If the JxlBasicInfo contains information of extra channels beyond an alpha
		/// channel, then <see cref="SetExtraChannelInfo" /> must be called between
		/// JxlEncoderSetBasicInfo and <see cref="JxlEncoderFrameSettings.AddImageFrame" />. In order to indicate
		/// extra channels, the value of `info.num_extra_channels` should be set to the
		/// number of extra channels, also counting the alpha channel if present.
		/// </summary>
		/// <param name="info"> global image metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful,
		/// JXL_ENC_ERROR or JXL_ENC_NOT_SUPPORTED otherwise</returns>
		public JxlEncoderStatus SetBasicInfo(JxlBasicInfo info)
		{
			return (JxlEncoderStatus)encoderWrapper.SetBasicInfo(ref info.basicInfo);
		}

		/// <summary>
		/// Sets information for the extra channel at the given index. The index
		/// must be smaller than num_extra_channels in the associated JxlBasicInfo.
		/// </summary>
		/// <param name="index"> index of the extra channel to set.</param>
		/// <param name="info"> global extra channel metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetExtraChannelInfo(int index, JxlExtraChannelInfo info)
		{
			UnsafeNativeJxl.JxlExtraChannelInfo info2 = new UnsafeNativeJxl.JxlExtraChannelInfo();
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out info2, info);
			return (JxlEncoderStatus)encoderWrapper.SetExtraChannelInfo(index, ref info2);
		}

		/// <summary>
		/// Sets the name for the extra channel at the given index in UTF-8. The index
		/// must be smaller than the num_extra_channels in the associated JxlBasicInfo.
		/// </summary>
		/// <param name="index"> index of the extra channel to set.</param>
		/// <param name="name"> buffer with the name of the extra channel.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetExtraChannelName(int index, string name)
		{
			return (JxlEncoderStatus)encoderWrapper.SetExtraChannelName(index, name);
		}

		/// <summary>
		/// Forces the encoder to use the box-based container format (BMFF) even
		/// when not necessary.
		/// <br /><br />
		/// When using <see cref="UseBoxes" />, <see cref="StoreJPEGMetadata" /> or 
		/// <see cref="SetCodestreamLevel" /> with level 10, the encoder will automatically
		/// also use the container format, it is not necessary to use
		/// JxlEncoderUseContainer for those use cases.
		/// <br /><br />
		/// By default this setting is disabled.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="use_container"> true if the encoder should always output the JPEG XL
		/// container format, false to only output it when necessary.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus UseContainer(bool use_container)
		{
			return (JxlEncoderStatus)encoderWrapper.UseContainer(use_container);
		}

		/// <summary>
		/// Configure the encoder to store JPEG reconstruction metadata in the JPEG XL
		/// container.
		/// <br /><br />
		/// If this is set to true and a single JPEG frame is added, it will be
		/// possible to losslessly reconstruct the JPEG codestream.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="store_jpeg_metadata"> true if the encoder should store JPEG metadata.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus StoreJPEGMetadata(bool store_jpeg_metadata)
		{
			return (JxlEncoderStatus)encoderWrapper.StoreJPEGMetadata(store_jpeg_metadata);
		}

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
		/// <see cref="T:JxlSharp.UnsafeNativeJxl.JxlBasicInfo" /> structure. Do note that some level 10 features, particularly
		/// those used by animated JPEG XL codestreams, might require level 10, even
		/// though the <see cref="T:JxlSharp.UnsafeNativeJxl.JxlBasicInfo" /> only suggests level 5. In this case, the level
		/// must be explicitly set to 10, otherwise the encoder will return an error.
		/// The encoder will restrict public encoding choices to those compatible with
		/// the level setting.
		/// <br /><br />
		/// This setting can only be set at the beginning, before encoding starts.
		/// </summary>
		/// <param name="level"> the level value to set, must be -1, 5, or 10.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus SetCodestreamLevel(int level)
		{
			return (JxlEncoderStatus)encoderWrapper.SetCodestreamLevel(level);
		}

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
		/// <returns> -1 if no level can support the configuration (e.g. image dimensions
		/// larger than even level 10 supports), 5 if level 5 is supported, 10 if setting
		/// the codestream level to 10 is required.
		/// </returns>
		public int GetRequiredCodestreamLevel()
		{
			return encoderWrapper.GetRequiredCodestreamLevel();
		}

		/// <summary>
		/// Create a new set of encoder options <br/>
		/// The returned object is tied to the encoder and it will be
		/// deallocated by the encoder when JxlEncoderDestroy() is called. For functions
		/// taking both a <see cref="T:JxlSharp.UnsafeNativeJxl.JxlEncoder" /> and a <see cref="T:JxlSharp.UnsafeNativeJxl.JxlEncoderFrameSettings" />, only
		/// JxlEncoderFrameSettings created with this function for the same encoder
		/// instance can be used.
		/// </summary>
		/// <returns> A wrapper object for the Frame Settings object</returns>
		public JxlEncoderFrameSettings CreateFrameSettings()
		{
			return new JxlEncoderFrameSettings(this, encoderWrapper.FrameSettingsCreate());
		}
	}

	/// <summary>
	/// A FrameSettings object, tied to the Encoder object which created it.  Contains many of the Encoder functions.
	/// Does not need to be diposed.  The native object is disposed when the Encoder object is disposed.
	/// </summary>
	public class JxlEncoderFrameSettings
	{
		UnsafeNativeJxl.JxlEncoderFrameSettingsWrapper frameSettings;
		WeakReference<JxlEncoder> _parent;
		public JxlEncoder Parent
		{
			get
			{
				if (_parent.TryGetTarget(out JxlEncoder value) && value != null)
				{
					return value;
				}
				return null;
			}
		}

		internal JxlEncoderFrameSettings(JxlEncoder parent, UnsafeNativeJxl.JxlEncoderFrameSettingsWrapper frameSettings)
		{
			this._parent = new WeakReference<JxlEncoder>(parent);
			this.frameSettings = frameSettings;
		}

		//internal JxlEncoderFrameSettings(JxlEncoder parent) : this(parent, null)
		//{

		//}

		public JxlEncoderFrameSettings Clone()
		{
			var parent = this.Parent;
			if (parent == null) return null;
			return new JxlEncoderFrameSettings(parent, frameSettings.Clone());
		}

		/// <summary>
		/// Sets the frame information for this frame to the encoder. This includes
		/// animation information such as frame duration to store in the frame header.
		/// The frame header fields represent the frame as passed to the encoder, but not
		/// necessarily the exact values as they will be encoded file format: the encoder
		/// could change crop and blending options of a frame for more efficient encoding
		/// or introduce additional public frames. Animation duration and time code
		/// information is not altered since those are immutable metadata of the frame.
		/// <br /><br />
		/// It is not required to use this function, however if have_animation is set
		/// to true in the basic info, then this function should be used to set the
		/// time duration of this individual frame. By default individual frames have a
		/// time duration of 0, making them form a composite still. See 
		/// <see cref="T:JxlSharp.UnsafeNativeJxl.JxlFrameHeader" /> for more information.
		/// <br /><br />
		/// This information is stored in the JxlEncoderFrameSettings and so is used for
		/// any frame encoded with these JxlEncoderFrameSettings. It is ok to change
		/// between <see cref="JxlEncoderFrameSettings.AddImageFrame" /> calls, each added image frame will have
		/// the frame header that was set in the options at the time of calling
		/// JxlEncoderAddImageFrame.
		/// <br /><br />
		/// The is_last and name_length fields of the JxlFrameHeader are ignored, use
		/// <see cref="JxlEncoder.CloseFrames" /> to indicate last frame, and 
		/// <see cref="SetFrameName" /> to indicate the name and its length instead.
		/// Calling this function will clear any name that was previously set with 
		/// <see cref="SetFrameName" />.
		/// </summary>
		/// <param name="frame_header"> frame header data to set. Object owned by the caller and
		/// does not need to be kept in memory, its information is copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetFrameHeader(JxlFrameHeader frame_header)
		{
			UnsafeNativeJxl.JxlFrameHeader header2 = new UnsafeNativeJxl.JxlFrameHeader();
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out header2, frame_header);
			JxlEncoderStatus status;
			status = (JxlEncoderStatus)frameSettings.SetFrameName(frame_header.Name);
			if (status != JxlEncoderStatus.Success)
			{
				return status;
			}
			return (JxlEncoderStatus)frameSettings.SetFrameHeader(ref header2);
		}

		/// <summary>
		/// Sets blend info of an extra channel. The blend info of extra channels is set
		/// separately from that of the color channels, the color channels are set with
		/// <see cref="SetFrameHeader" />.
		/// </summary>
		/// <param name="index"> index of the extra channel to use.</param>
		/// <param name="blend_info"> blend info to set for the extra channel</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetExtraChannelBlendInfo(int index, JxlBlendInfo blend_info)
		{
			UnsafeNativeJxl.JxlBlendInfo blendInfo2 = new UnsafeNativeJxl.JxlBlendInfo();
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out blendInfo2, ref blend_info);
			return (JxlEncoderStatus)frameSettings.SetExtraChannelBlendInfo(index, ref blendInfo2);
		}

		///// <summary>
		///// Sets the name of the animation frame. This function is optional, frames are
		///// not required to have a name. This setting is a part of the frame header, and
		///// the same principles as for <see cref="SetFrameHeader" /> apply. The
		///// name_length field of JxlFrameHeader is ignored by the encoder, this function
		///// determines the name length instead as the length in bytes of the C string.
		///// <br /><br />
		///// The maximum possible name length is 1071 bytes (excluding terminating null
		///// character).
		///// <br /><br />
		///// Calling <see cref="SetFrameHeader" /> clears any name that was
		///// previously set.
		///// </summary>
		///// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
		///// string (zero terminated). Owned by the caller, and copied internally.</param>
		///// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		//public JxlEncoderStatus SetFrameName(byte* frame_name)
		//{
		//	return (JxlEncoderStatus)frameSettings.SetFrameName(frame_name);
		//}

		/// <summary>
		/// Sets the name of the animation frame. This function is optional, frames are
		/// not required to have a name. This setting is a part of the frame header, and
		/// the same principles as for <see cref="SetFrameHeader" /> apply. The
		/// name_length field of JxlFrameHeader is ignored by the encoder, this function
		/// determines the name length instead as the length in bytes of the C string.
		/// <br /><br />
		/// The maximum possible name length is 1071 bytes (excluding terminating null
		/// character).
		/// <br /><br />
		/// Calling <see cref="SetFrameHeader" /> clears any name that was
		/// previously set.
		/// </summary>
		/// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
		/// string (zero terminated). Owned by the caller, and copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetFrameName(string frame_name)
		{
			return (JxlEncoderStatus)frameSettings.SetFrameName(frame_name);
		}

		///// <summary>
		///// Sets the buffer to read JPEG encoded bytes from for the next frame to encode.
		///// <br /><br />
		///// If JxlEncoderSetBasicInfo has not yet been called, calling
		///// JxlEncoderAddJPEGFrame will implicitly call it with the parameters of the
		///// added JPEG frame.
		///// <br /><br />
		///// If JxlEncoderSetColorEncoding or JxlEncoderSetICCProfile has not yet been
		///// called, calling JxlEncoderAddJPEGFrame will implicitly call it with the
		///// parameters of the added JPEG frame.
		///// <br /><br />
		///// If the encoder is set to store JPEG reconstruction metadata using 
		///// <see cref="StoreJPEGMetadata" /> and a single JPEG frame is added, it will be
		///// possible to losslessly reconstruct the JPEG codestream.
		///// <br /><br />
		///// If this is the last frame, <see cref="CloseInput" /> or 
		///// <see cref="CloseFrames" /> must be called before the next
		///// <see cref="ProcessOutput" /> call.
		///// </summary>
		///// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
		///// are copied internally.</param>
		///// <param name="size"> size of buffer in bytes.</param>
		///// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		//public JxlEncoderStatus AddJPEGFrame(byte* buffer, int size)
		//{
		//	return (JxlEncoderStatus)frameSettings.AddJPEGFrame(buffer, size);
		//}

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
		/// <see cref="JxlEncoder.StoreJPEGMetadata" /> and a single JPEG frame is added, it will be
		/// possible to losslessly reconstruct the JPEG codestream.
		/// <br /><br />
		/// If this is the last frame, <see cref="JxlEncoder.CloseInput" /> or 
		/// <see cref="JxlEncoder.CloseFrames" /> must be called before the next
		/// <see cref="JxlEncoder.ProcessOutput" /> call.
		/// </summary>
		/// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
		/// are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus AddJPEGFrame(byte[] buffer)
		{
			return (JxlEncoderStatus)frameSettings.AddJPEGFrame(buffer);
		}


		/// <summary>
		/// Sets the buffer to read pixels from for the next image to encode. Must call
		/// JxlEncoderSetBasicInfo before JxlEncoderAddImageFrame.
		/// <br /><br />
		/// Currently only some data types for pixel formats are supported:
		/// <br/> - JXL_TYPE_UINT8, with range 0..255
		/// <br/> - JXL_TYPE_UINT16, with range 0..65535
		/// <br/> - JXL_TYPE_FLOAT16, with nominal range 0..1
		/// <br/> - JXL_TYPE_FLOAT, with nominal range 0..1
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
		/// <br/> - single-channel data, e.g. grayscale
		/// <br/> - single-channel + alpha
		/// <br/> - trichromatic, e.g. RGB
		/// <br/> - trichromatic + alpha
		/// <br /><br />
		/// Extra channels not handled here need to be set by 
		/// <see cref="SetExtraChannelBuffer" />.
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
		/// If this is the last frame, <see cref="JxlEncoder.CloseInput" /> or 
		/// <see cref="JxlEncoder.CloseFrames" /> must be called before the next
		/// <see cref="JxlEncoder.ProcessOutput" /> call.
		/// </summary>
		/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
		/// and its contents are copied internally.</param>
		/// <param name="size"> size of buffer in bytes. This size should match what is implied
		/// by the frame dimensions and the pixel format.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus AddImageFrame(JxlPixelFormat pixel_format, IntPtr buffer, int size)
		{
			unsafe
			{
				return (JxlEncoderStatus)frameSettings.AddImageFrame(ref pixel_format.pixelFormat, (void*)buffer, size);
			}
		}

		/// <summary>
		/// Sets the buffer to read pixels from for the next image to encode. Must call
		/// JxlEncoderSetBasicInfo before JxlEncoderAddImageFrame.
		/// <br /><br />
		/// Currently only some data types for pixel formats are supported:
		/// <br/> - JXL_TYPE_UINT8, with range 0..255
		/// <br/> - JXL_TYPE_UINT16, with range 0..65535
		/// <br/> - JXL_TYPE_FLOAT16, with nominal range 0..1
		/// <br/> - JXL_TYPE_FLOAT, with nominal range 0..1
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
		/// <br/> - single-channel data, e.g. grayscale
		/// <br/> - single-channel + alpha
		/// <br/> - trichromatic, e.g. RGB
		/// <br/> - trichromatic + alpha
		/// <br /><br />
		/// Extra channels not handled here need to be set by 
		/// <see cref="SetExtraChannelBuffer" />.
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
		/// If this is the last frame, <see cref="JxlEncoder.CloseInput" /> or 
		/// <see cref="JxlEncoder.CloseFrames" /> must be called before the next
		/// <see cref="JxlEncoder.ProcessOutput" /> call.
		/// </summary>
		/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
		/// contents are copied internally.</param>
		/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
		/// and its contents are copied internally.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus AddImageFrame(JxlPixelFormat pixel_format, byte[] buffer)
		{
			unsafe
			{
				fixed (byte* pBuffer = buffer)
				{
					return AddImageFrame(pixel_format, (IntPtr)pBuffer, buffer.Length);
				}
			}
		}

		/// <summary>
		/// Sets the buffer to read pixels from for an extra channel at a given index.
		/// The index must be smaller than the num_extra_channels in the associated
		/// JxlBasicInfo. Must call <see cref="SetExtraChannelInfo" /> before
		/// JxlEncoderSetExtraChannelBuffer.
		/// <br /><br />
		/// TODO(firsching): mention what data types in pixel formats are supported.
		/// <br /><br />
		/// It is required to call this function for every extra channel, except for the
		/// alpha channel if that was already set through <see cref="JxlEncoderFrameSettings.AddImageFrame" />.
		/// </summary>
		/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
		/// contents are copied internally. The num_channels value is ignored, since the
		/// number of channels for an extra channel is always assumed to be one.</param>
		/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
		/// and its contents are copied internally.</param>
		/// <param name="size"> size of buffer in bytes. This size should match what is implied
		/// by the frame dimensions and the pixel format.</param>
		/// <param name="index"> index of the extra channel to use.</param>
		/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
		public JxlEncoderStatus SetExtraChannelBuffer(JxlPixelFormat pixel_format, IntPtr buffer, int size, int index)
		{
			unsafe
			{
				return (JxlEncoderStatus)frameSettings.SetExtraChannelBuffer(ref pixel_format.pixelFormat, (void*)buffer, size, index);
			}
		}

		/// <summary>
		/// Sets a frame-specific option of integer type to the encoder options.
		/// The JxlEncoderFrameSettingId argument determines which option is set.
		/// </summary>
		/// <param name="option"> ID of the option to set.</param>
		/// <param name="value"> Integer value to set for this option.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR in
		/// case of an error, such as invalid or unknown option id, or invalid integer
		/// value for the given option. If an error is returned, the state of the
		/// JxlEncoderFrameSettings object is still valid and is the same as before this
		/// function was called.</returns>
		public JxlEncoderStatus FrameSettingsSetOption(JxlEncoderFrameSettingId option, int value)
		{
			return (JxlEncoderStatus)frameSettings.FrameSettingsSetOption((UnsafeNativeJxl.JxlEncoderFrameSettingId)option, value);
		}

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
		/// <param name="lossless"> whether to override options for lossless mode</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus SetFrameLossless(bool lossless)
		{
			return (JxlEncoderStatus)frameSettings.SetFrameLossless(lossless);
		}

		///// <summary>
		///// DEPRECATED: use JxlEncoderSetFrameLossless instead.
		///// </summary>
		//[Obsolete]
		//public JxlEncoderStatus OptionsSetLossless(bool lossless)
		//{
		//	return (JxlEncoderStatus)frameSettings.OptionsSetLossless(lossless);
		//}

		///// <summary />
		///// <param name="effort"> the effort value to set.</param>
		///// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		///// otherwise.
		///// <br /><br />
		///// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
		///// JXL_ENC_FRAME_SETTING_EFFORT, effort) instead.</returns>
		//[Obsolete]
		//public JxlEncoderStatus JxlEncoderOptionsSetEffort(int effort)
		//{
		//	return (JxlEncoderStatus)frameSettings.JxlEncoderOptionsSetEffort(effort);
		//}

		///// <summary />
		///// <param name="tier"> the decoding speed tier to set.</param>
		///// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		///// otherwise.
		///// <br /><br />
		///// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
		///// JXL_ENC_FRAME_SETTING_DECODING_SPEED, tier) instead.</returns>
		//[Obsolete]
		//public JxlEncoderStatus JxlEncoderOptionsSetDecodingSpeed(int tier)
		//{
		//	return (JxlEncoderStatus)frameSettings.JxlEncoderOptionsSetDecodingSpeed(tier);
		//}

		/// <summary>
		/// Sets the distance level for lossy compression: target max butteraugli
		/// distance, lower = higher quality. Range: 0 .. 15.
		/// 0.0 = mathematically lossless (however, use JxlEncoderSetFrameLossless
		/// instead to use true lossless, as setting distance to 0 alone is not the only
		/// requirement). 1.0 = visually lossless. Recommended range: 0.5 .. 3.0. Default
		/// value: 1.0.
		/// </summary>
		/// <param name="distance"> the distance value to set.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus JxlEncoderSetFrameDistance(float distance)
		{
			return (JxlEncoderStatus)frameSettings.SetFrameDistance(distance);
		}

		/// <summary>
		/// Sets the distance level for lossy compression: target max butteraugli
		/// distance, lower = higher quality. Range: 0 .. 15.
		/// 0.0 = mathematically lossless (however, use JxlEncoderSetFrameLossless
		/// instead to use true lossless, as setting distance to 0 alone is not the only
		/// requirement). 1.0 = visually lossless. Recommended range: 0.5 .. 3.0. Default
		/// value: 1.0.
		/// </summary>
		/// <param name="distance"> the distance value to set.</param>
		/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
		/// otherwise.</returns>
		public JxlEncoderStatus JxlEncoderSetFrameDistance(double distance)
		{
			return (JxlEncoderStatus)frameSettings.SetFrameDistance((float)distance);
		}

		///// <summary>
		///// DEPRECATED: use JxlEncoderSetFrameDistance instead.
		///// </summary>
		//[Obsolete]
		//public JxlEncoderStatus JxlEncoderOptionsSetDistance(float distance)
		//{
		//	return (JxlEncoderStatus)frameSettings.JxlEncoderOptionsSetDistance(distance);
		//}
	}
}
