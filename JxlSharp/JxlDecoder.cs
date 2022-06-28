using System;
using System.Collections.Generic;
using System.Text;

namespace JxlSharp
{
	/// <summary>
	/// Wrapper for JPEG XL library decoder functions.  Use SetInput, SubscribeEvents, and ProcessInput.
	/// </summary>
	/// <example>
	/// Example of usage:
	/// <code>
	///using (JxlDecoder jxlDecoder = new JxlDecoder())
	///{
	///	bool canTranscodeToJpeg = false;
	///	JxlBasicInfo basicInfo = null;
	///	jxlDecoder.SetInput(input);
	///	jxlDecoder.SubscribeEvents(JxlDecoderStatus.BasicInfo | JxlDecoderStatus.JpegReconstruction | JxlDecoderStatus.Frame | JxlDecoderStatus.FullImage);
	///	bool proceed = true;
	///	bool success = false;
	///	while (proceed)
	///	{
	///		JxlDecoderStatus status = jxlDecoder.ProcessInput();
	///		switch (status)
	///		{
	///			case JxlDecoderStatus.BasicInfo:
	///				status = jxlDecoder.GetBasicInfo(out basicInfo);
	///				break;
	///			case JxlDecoderStatus.JpegReconstruction:
	///				canTranscodeToJpeg = true;
	///				break;
	///			case JxlDecoderStatus.Frame:
	///				//call jxlDecoder.SetImageOutBuffer here
	///				break;
	///			case JxlDecoderStatus.FullImage:
	///				//post-process the image if necessary
	///				break;
	///			case JxlDecoderStatus.Success:
	///				//handle a succesful case here
	///				proceed = false;
	///				success = true;
	///				break;
	///			case JxlDecoderStatus.Error:
	///			default:
	///				//handle a failure here
	///				proceed = false;
	///				break;
	///		}
	///	}
	///}
	/// </code>
	/// </example>
	public class JxlDecoder : IDisposable
	{
		UnsafeNativeJXL.JxlDecoderWrapper decoderWrapper;
		/// <summary>
		/// Creates a new JxlDecoder
		/// </summary>
		public JxlDecoder()
		{
			decoderWrapper = new UnsafeNativeJXL.JxlDecoderWrapper();
		}

		/// <summary>
		/// Disposes the JxlDecoder object.  Do not call any other methods after disposing this object.
		/// </summary>
		public void Dispose()
		{
			decoderWrapper.Dispose();
		}

		/// <summary>
		/// Re-initializes a JxlDecoder instance, so it can be re-used for decoding
		/// another image. All state and settings are reset as if the object was
		/// newly created with JxlDecoderCreate, but the memory manager is kept.
		/// </summary>
		public void Reset()
		{
			decoderWrapper.Reset();
		}
		/// <summary>
		/// Rewinds decoder to the beginning. The same input must be given again from
		/// the beginning of the file and the decoder will emit events from the beginning
		/// again. When rewinding (as opposed to JxlDecoderReset), the decoder can keep
		/// state about the image, which it can use to skip to a requested frame more
		/// efficiently with JxlDecoderSkipFrames. Settings such as parallel runner or
		/// subscribed events are kept. After rewind, JxlDecoderSubscribeEvents can be
		/// used again, and it is feasible to leave out events that were already handled
		/// before, such as JXL_DEC_BASIC_INFO and JXL_DEC_COLOR_ENCODING, since they
		/// will provide the same information as before.
		/// </summary>
		public void Rewind()
		{
			decoderWrapper.Rewind();
		}
		/// <summary>
		/// Makes the decoder skip the next `amount` frames. It still needs to process
		/// the input, but will not output the frame events. It can be more efficient
		/// when skipping frames, and even more so when using this after
		/// JxlDecoderRewind. If the decoder is already processing a frame (could
		/// have emitted JXL_DEC_FRAME but not yet JXL_DEC_FULL_IMAGE), it starts
		/// skipping from the next frame. If the amount is larger than the amount of
		/// frames remaining in the image, all remaining frames are skipped. Calling this
		/// function multiple times adds the amount to skip to the already existing
		/// amount.
		/// A frame here is defined as a frame that without skipping emits events such as
		/// JXL_DEC_FRAME and JXL_FULL_IMAGE, frames that are internal to the file format
		/// but are not rendered as part of an animation, or are not the final still
		/// frame of a still image, are not counted.
		/// </summary>
		/// <param name="amount"> the amount of frames to skip</param>
		public void SkipFrames(int amount)
		{
			decoderWrapper.SkipFrames(amount);
		}
		/// <summary>
		/// Get the default pixel format for this decoder.
		/// Requires that the decoder can produce JxlBasicInfo.
		/// </summary>
		/// <param name="format"> JxlPixelFormat to populate with the recommended settings for
		/// the data loaded into this decoder.</param>
		/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_NEED_MORE_INPUT if the
		/// basic info isn't yet available, and JXL_DEC_ERROR otherwise.</returns>
		public JxlDecoderStatus GetDefaultPixelFormat(out JxlPixelFormat format)
		{
			format = new JxlPixelFormat();
			var status = (JxlDecoderStatus)decoderWrapper.GetDefaultPixelFormat(out format.pixelFormat);
			return status;
		}
		/// <summary>
		/// Returns a hint indicating how many more bytes the decoder is expected to
		/// need to make JxlDecoderGetBasicInfo available after the next
		/// JxlDecoderProcessInput call. This is a suggested large enough value for
		/// the amount of bytes to provide in the next JxlDecoderSetInput call, but it is
		/// not guaranteed to be an upper bound nor a lower bound. This number does not
		/// include bytes that have already been released from the input.
		/// Can be used before the first JxlDecoderProcessInput call, and is correct
		/// the first time in most cases. If not, JxlDecoderSizeHintBasicInfo can be
		/// called again to get an updated hint.
		/// </summary>
		/// <returns> the size hint in bytes if the basic info is not yet fully decoded.</returns>
		/// <returns> 0 when the basic info is already available.</returns>
		public int GetSizeHintBasicInfo()
		{
			return decoderWrapper.GetSizeHintBasicInfo();
		}
		/// <summary>
		/// Select for which informative events (JXL_DEC_BASIC_INFO, etc...) the
		/// decoder should return with a status. It is not required to subscribe to any
		/// events, data can still be requested from the decoder as soon as it available.
		/// By default, the decoder is subscribed to no events (events_wanted == 0), and
		/// the decoder will then only return when it cannot continue because it needs
		/// more input data or more output buffer. This function may only be be called
		/// before using JxlDecoderProcessInput
		/// </summary>
		/// <param name="eventsWanted"> bitfield of desired events.</param>
		/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_ERROR otherwise.</returns>
		public JxlDecoderStatus SubscribeEvents(JxlDecoderStatus eventsWanted)
		{
			return (JxlDecoderStatus)decoderWrapper.SubscribeEvents((int)eventsWanted);
		}
		/// <summary>
		/// Enables or disables preserving of original orientation. Some images are
		/// encoded with an orientation tag indicating the image is rotated and/or
		/// mirrored (here called the original orientation).
		/// <br/><br/>
		/// *) If keep_orientation is JXL_FALSE (the default): the decoder will perform
		/// work to undo the transformation. This ensures the decoded pixels will not
		/// be rotated or mirrored. The decoder will always set the orientation field
		/// of the JxlBasicInfo to JXL_ORIENT_IDENTITY to match the returned pixel data.
		/// The decoder may also swap xsize and ysize in the JxlBasicInfo compared to the
		/// values inside of the codestream, to correctly match the decoded pixel data,
		/// e.g. when a 90 degree rotation was performed.
		/// <br/><br/>
		/// *) If this option is JXL_TRUE: then the image is returned as-is, which may be
		/// rotated or mirrored, and the user must check the orientation field in
		/// JxlBasicInfo after decoding to correctly interpret the decoded pixel data.
		/// This may be faster to decode since the decoder doesn't have to apply the
		/// transformation, but can cause wrong display of the image if the orientation
		/// tag is not correctly taken into account by the user.
		/// <br/><br/>
		/// By default, this option is disabled, and the decoder automatically corrects
		/// the orientation.
		/// <br/><br/>
		/// This function must be called at the beginning, before decoding is performed.
		/// <br/><br/>
		/// <see cref="JxlBasicInfo"/> for the orientation field, and <see cref="JxlOrientation"/> for the
		/// possible values.
		/// </summary>
		/// <param name="keepOrientation"> JXL_TRUE to enable, JXL_FALSE to disable.</param>
		/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_ERROR otherwise.</returns>
		public JxlDecoderStatus SetKeepOrientation(bool keepOrientation)
		{
			return (JxlDecoderStatus)decoderWrapper.SetKeepOrientation(keepOrientation);
		}
		/// <summary>
		/// Decodes JPEG XL file using the available bytes. Requires input has been
		/// set with JxlDecoderSetInput. After JxlDecoderProcessInput, input can
		/// optionally be released with JxlDecoderReleaseInput and then set again to
		/// next bytes in the stream. JxlDecoderReleaseInput returns how many bytes are
		/// not yet processed, before a next call to JxlDecoderProcessInput all
		/// unprocessed bytes must be provided again (the address need not match, but the
		/// contents must), and more bytes may be concatenated after the unprocessed
		/// bytes.
		/// <br/><br/>
		/// The returned status indicates whether the decoder needs more input bytes, or
		/// more output buffer for a certain type of output data. No matter what the
		/// returned status is (other than JXL_DEC_ERROR), new information, such as
		/// JxlDecoderGetBasicInfo, may have become available after this call. When
		/// the return value is not JXL_DEC_ERROR or JXL_DEC_SUCCESS, the decoding
		/// requires more JxlDecoderProcessInput calls to continue.
		/// </summary>
		/// <returns> JXL_DEC_SUCCESS when decoding finished and all events handled. If you
		/// still have more unprocessed input data anyway, then you can still continue
		/// by using JxlDecoderSetInput and calling JxlDecoderProcessInput again, similar
		/// to handling JXL_DEC_NEED_MORE_INPUT. JXL_DEC_SUCCESS can occur instead of
		/// JXL_DEC_NEED_MORE_INPUT when, for example, the input data ended right at
		/// the boundary of a box of the container format, all essential codestream boxes
		/// were already decoded, but extra metadata boxes are still present in the next
		/// data. JxlDecoderProcessInput cannot return success if all codestream boxes
		/// have not been seen yet.</returns>
		/// <returns> JXL_DEC_ERROR when decoding failed, e.g. invalid codestream.
		/// TODO(lode) document the input data mechanism</returns>
		/// <returns> JXL_DEC_NEED_MORE_INPUT more input data is necessary.</returns>
		/// <returns> JXL_DEC_BASIC_INFO when basic info such as image dimensions is
		/// available and this informative event is subscribed to.</returns>
		/// <returns> JXL_DEC_EXTENSIONS when JPEG XL codestream user extensions are
		/// available and this informative event is subscribed to.</returns>
		/// <returns> JXL_DEC_COLOR_ENCODING when color profile information is
		/// available and this informative event is subscribed to.</returns>
		/// <returns> JXL_DEC_PREVIEW_IMAGE when preview pixel information is available and
		/// output in the preview buffer.</returns>
		/// <returns> JXL_DEC_DC_IMAGE when DC pixel information (8x8 downscaled version
		/// of the image) is available and output in the DC buffer.</returns>
		/// <returns> JXL_DEC_FULL_IMAGE when all pixel information at highest detail is
		/// available and has been output in the pixel buffer.</returns>
		public JxlDecoderStatus ProcessInput()
		{
			return (JxlDecoderStatus)decoderWrapper.ProcessInput();
		}
		/// <summary>
		/// Warning: Pointer must remain valid between the call to SetInput and the call to ReleaseInput.
		/// Use the Array version of this function instead for better safety.
		/// <br/><br/>
		/// Sets input data for JxlDecoderProcessInput. The data is owned by the caller
		/// and may be used by the decoder until JxlDecoderReleaseInput is called or
		/// the decoder is destroyed or reset so must be kept alive until then.
		/// </summary>
		/// <param name="data"> pointer to next bytes to read from</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns> JXL_DEC_ERROR if input was already set without releasing,
		/// JXL_DEC_SUCCESS otherwise.</returns>
		public JxlDecoderStatus SetInput(IntPtr data, int size)
		{
			unsafe
			{
				return (JxlDecoderStatus)decoderWrapper.SetInput((byte*)data, size);
			}
		}
		/// <summary>
		/// Sets input data for JxlDecoderProcessInput.
		/// This acquires a pinned GC handle for the input array until the data is released,
		/// a different input array is provided, or the wrapper is disposed.
		/// </summary>
		/// <param name="data"> byte array to read from</param>
		/// <returns> JXL_DEC_SUCCESS </returns>
		public JxlDecoderStatus SetInput(byte[] data)
		{
			return (JxlDecoderStatus)decoderWrapper.SetInput(data);
		}
		/// <summary>
		/// Releases input which was provided with JxlDecoderSetInput. Between
		/// JxlDecoderProcessInput and JxlDecoderReleaseInput, the user may not alter
		/// the data in the buffer. Calling JxlDecoderReleaseInput is required whenever
		/// any input is already set and new input needs to be added with
		/// JxlDecoderSetInput, but is not required before JxlDecoderDestroy or
		/// JxlDecoderReset. Calling JxlDecoderReleaseInput when no input is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> the amount of bytes the decoder has not yet processed that are
		/// still remaining in the data set by JxlDecoderSetInput, or 0 if no input is
		/// set or JxlDecoderReleaseInput was already called. For a next call to
		/// JxlDecoderProcessInput, the buffer must start with these unprocessed bytes.
		/// This value doesn't provide information about how many bytes the decoder
		/// truly processed internally or how large the original JPEG XL codestream or
		/// file are.</returns>
		public int ReleaseInput()
		{
			return decoderWrapper.ReleaseInput();
		}
		/// <summary>
		/// Outputs the basic image information, such as image dimensions, bit depth and
		/// all other JxlBasicInfo fields, if available.
		/// </summary>
		/// <param name="info"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns> JXL_DEC_SUCCESS if the value is available,
		/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		/// of other error conditions.</returns>
		public JxlDecoderStatus GetBasicInfo(out JxlBasicInfo info)
		{
			info = new JxlBasicInfo();
			return (JxlDecoderStatus)decoderWrapper.GetBasicInfo(out info.basicInfo);
		}
		/// <summary>
		/// Outputs information for extra channel at the given index. The index must be
		/// smaller than num_extra_channels in the associated JxlBasicInfo.
		/// </summary>
		/// <param name="index"> index of the extra channel to query.</param>
		/// <param name="info"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns> JXL_DEC_SUCCESS if the value is available,
		/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		/// of other error conditions.</returns>
		public JxlDecoderStatus GetExtraChannelInfo(int index, out JxlExtraChannelInfo info)
		{
			UnsafeNativeJXL.JxlExtraChannelInfo info2;
			var status = (JxlDecoderStatus)decoderWrapper.GetExtraChannelInfo(index, out info2);
			info = new JxlExtraChannelInfo();
			info2.WriteToPublic(info);
			return status;
		}

		/// <summary>
		/// Outputs name for extra channel at the given index in UTF-8. The index must be
		/// smaller than num_extra_channels in the associated JxlBasicInfo. The buffer
		/// for name must have at least name_length + 1 bytes allocated, gotten from
		/// the associated JxlExtraChannelInfo.
		/// </summary>
		/// <param name="index"> index of the extra channel to query.</param>
		/// <param name="name"> buffer to copy the name into</param>
		/// <returns> JXL_DEC_SUCCESS if the value is available,
		/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		/// of other error conditions.</returns>
		public JxlDecoderStatus GetExtraChannelName(int index, out string name)
		{
			return (JxlDecoderStatus)decoderWrapper.GetExtraChannelName(index, out name);
		}
		/// <summary>
		/// Outputs the color profile as JPEG XL encoded structured data, if available.
		/// This is an alternative to an ICC Profile, which can represent a more limited
		/// amount of color spaces, but represents them exactly through enum values.
		/// <br/><br/>
		/// It is often possible to use JxlDecoderGetColorAsICCProfile as an
		/// alternative anyway. The following scenarios are possible:
		/// - The JPEG XL image has an attached ICC Profile, in that case, the encoded
		/// structured data is not available, this function will return an error status
		/// and you must use JxlDecoderGetColorAsICCProfile instead.
		/// - The JPEG XL image has an encoded structured color profile, and it
		/// represents an RGB or grayscale color space. This function will return it.
		/// You can still use JxlDecoderGetColorAsICCProfile as well as an
		/// alternative if desired, though depending on which RGB color space is
		/// represented, the ICC profile may be a close approximation. It is also not
		/// always feasible to deduce from an ICC profile which named color space it
		/// exactly represents, if any, as it can represent any arbitrary space.
		/// - The JPEG XL image has an encoded structured color profile, and it indicates
		/// an unknown or xyb color space. In that case,
		/// JxlDecoderGetColorAsICCProfile is not available.
		/// <br/><br/>
		/// If you wish to render the image using a system that supports ICC profiles,
		/// use JxlDecoderGetColorAsICCProfile first. If you're looking for a specific
		/// color space possibly indicated in the JPEG XL image, use
		/// JxlDecoderGetColorAsEncodedProfile first.
		/// </summary>
		/// <param name="format"> pixel format to output the data to. Only used for
		/// JXL_COLOR_PROFILE_TARGET_DATA, may be nullptr otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="colorEncoding"> struct to copy the information into, or NULL to only
		/// check whether the information is available through the return value.</param>
		/// <returns> JXL_DEC_SUCCESS if the data is available and returned,
		/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		/// the encoded structured color profile does not exist in the codestream.</returns>
		public JxlDecoderStatus GetColorAsEncodedProfile(JxlPixelFormat format, JxlColorProfileTarget target, out JxlColorEncoding colorEncoding)
		{
			colorEncoding = new JxlColorEncoding();
			unsafe
			{
				if (format != null)
				{
					fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
					{
						var status = (JxlDecoderStatus)decoderWrapper.GetColorAsEncodedProfile(pFormat, (UnsafeNativeJXL.JxlColorProfileTarget)target, out colorEncoding.colorEncoding);
						return status;
					}
				}
				else
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetColorAsEncodedProfile(null, (UnsafeNativeJXL.JxlColorProfileTarget)target, out colorEncoding.colorEncoding);
					return status;
				}
			}
		}
		///// <summary>
		///// Outputs the size in bytes of the ICC profile returned by
		///// JxlDecoderGetColorAsICCProfile, if available, or indicates there is none
		///// available. In most cases, the image will have an ICC profile available, but
		///// if it does not, JxlDecoderGetColorAsEncodedProfile must be used instead.
		///// <see cref="JxlDecoderGetColorAsEncodedProfile"/> for more information. The ICC
		///// profile is either the exact ICC profile attached to the codestream metadata,
		///// or a close approximation generated from JPEG XL encoded structured data,
		///// depending of what is encoded in the codestream.
		///// </summary>
		///// <param name="format"> pixel format to output the data to. Only used for
		///// JXL_COLOR_PROFILE_TARGET_DATA, may be nullptr otherwise.</param>
		///// <param name="target"> whether to get the original color profile from the metadata
		///// or the color profile of the decoded pixels.</param>
		///// <param name="size"> variable to output the size into, or NULL to only check the
		///// return status.</param>
		///// <returns> JXL_DEC_SUCCESS if the ICC profile is available,
		///// JXL_DEC_NEED_MORE_INPUT if the decoder has not yet received enough
		///// input data to determine whether an ICC profile is available or what its
		///// size is, JXL_DEC_ERROR in case the ICC profile is not available and
		///// cannot be generated.</returns>
		//commented out because GetICCProfile already calls this to determine what size array to return
		//public JxlDecoderStatus GetICCProfileSize(JxlPixelFormat format, JxlColorProfileTarget target, out int size)
		//{
		//	UnsafeNativeJXL.JxlPixelFormat format2 = new UnsafeNativeJXL.JxlPixelFormat(format);
		//	unsafe
		//	{
		//		var status = (JxlDecoderStatus)decoderWrapper.GetICCProfileSize(&format2, (UnsafeNativeJXL.JxlColorProfileTarget)target, out size);
		//		return status;
		//	}
		//}
		/// <summary>
		/// Outputs ICC profile if available. The profile is only available if
		/// JxlDecoderGetICCProfileSize returns success. The output buffer must have
		/// at least as many bytes as given by JxlDecoderGetICCProfileSize.
		/// </summary>
		/// <param name="format"> pixel format to output the data to. Only used for
		/// JXL_COLOR_PROFILE_TARGET_DATA, may be nullptr otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="iccProfile"> buffer to copy the ICC profile into</param>
		/// <returns> JXL_DEC_SUCCESS if the profile was successfully returned is
		/// available, JXL_DEC_NEED_MORE_INPUT if not yet available,
		/// JXL_DEC_ERROR if the profile doesn't exist or the output size is not
		/// large enough.</returns>
		public JxlDecoderStatus GetColorAsICCProfile(JxlPixelFormat format, JxlColorProfileTarget target, out byte[] iccProfile)
		{
			unsafe
			{
				if (format != null)
				{
					fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
					{
						var status = (JxlDecoderStatus)decoderWrapper.GetColorAsICCProfile(pFormat, (UnsafeNativeJXL.JxlColorProfileTarget)target, out iccProfile);
						return status;
					}
				}
				else
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetColorAsICCProfile(null, (UnsafeNativeJXL.JxlColorProfileTarget)target, out iccProfile);
					return status;
				}
			}
		}
		/// <summary>
		/// Sets the color profile to use for JXL_COLOR_PROFILE_TARGET_DATA for the
		/// special case when the decoder has a choice. This only has effect for a JXL
		/// image where uses_original_profile is false, and the original color profile is
		/// encoded as an ICC color profile rather than a JxlColorEncoding with known
		/// enum values. In most other cases (uses uses_original_profile is true, or the
		/// color profile is already given as a JxlColorEncoding), this setting is
		/// ignored and the decoder uses a profile related to the image.
		/// No matter what, the JXL_COLOR_PROFILE_TARGET_DATA must still be queried to
		/// know the actual data format of the decoded pixels after decoding.
		/// <br/><br/>
		/// The intended use case of this function is for cases where you are using
		/// a color management system to parse the original ICC color profile
		/// (JXL_COLOR_PROFILE_TARGET_ORIGINAL), from this you know that the ICC
		/// profile represents one of the color profiles supported by JxlColorEncoding
		/// (such as sRGB, PQ or HLG): in that case it is beneficial (but not necessary)
		/// to use JxlDecoderSetPreferredColorProfile to match the parsed profile. The
		/// JXL decoder has no color management system built in, but can convert XYB
		/// color to any of the ones supported by JxlColorEncoding.
		/// <br/><br/>
		/// Can only be set after the JXL_DEC_COLOR_ENCODING event occurred and before
		/// any other event occurred, and can affect the result of
		/// JXL_COLOR_PROFILE_TARGET_DATA (but not of JXL_COLOR_PROFILE_TARGET_ORIGINAL),
		/// so should be used after getting JXL_COLOR_PROFILE_TARGET_ORIGINAL but before
		/// getting JXL_COLOR_PROFILE_TARGET_DATA. The color_encoding must be grayscale
		/// if num_color_channels from the basic info is 1, RGB if num_color_channels
		/// from the basic info is 3.
		/// <br/><br/>
		/// If JxlDecoderSetPreferredColorProfile is not used, then for images for which
		/// uses_original_profile is false and with ICC color profile, the decoder will
		/// choose linear sRGB for color images, linear grayscale for grayscale images.
		/// This function only sets a preference, since for other images the decoder has
		/// no choice what color profile to use, it is determined by the image.
		/// </summary>
		/// <param name="colorEncoding"> the default color encoding to set</param>
		/// <returns> JXL_DEC_SUCCESS if the preference was set successfully, JXL_DEC_ERROR
		/// otherwise.</returns>
		public JxlDecoderStatus SetPreferredColorProfile(JxlColorEncoding colorEncoding)
		{
			UnsafeNativeJXL.JxlColorEncoding colorEncoding2 = new UnsafeNativeJXL.JxlColorEncoding(colorEncoding);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetPreferredColorProfile(&colorEncoding2);
				return status;
			}
		}
		/// <summary>
		/// Returns the minimum size in bytes of the preview image output pixel buffer
		/// for the given format. This is the buffer for JxlDecoderSetPreviewOutBuffer.
		/// Requires the preview header information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// information not available yet.</returns>
		public JxlDecoderStatus GetPreviewOutBufferSize(JxlPixelFormat format, out int size)
		{
			UnsafeNativeJXL.JxlPixelFormat pixelFormat2 = new UnsafeNativeJXL.JxlPixelFormat(format);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.GetPreviewOutBufferSize(&pixelFormat2, out size);
				return status;
			}
		}
		/// <summary>
		/// Sets the buffer to write the small resolution preview image
		/// to. The size of the buffer must be at least as large as given by
		/// JxlDecoderPreviewOutBufferSize. The buffer follows the format described by
		/// JxlPixelFormat. The preview image dimensions are given by the
		/// JxlPreviewHeader. The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// size too small.</returns>
		public JxlDecoderStatus SetPreviewOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			//TODO remove the dangling pointer
			UnsafeNativeJXL.JxlPixelFormat pixelFormat2 = new UnsafeNativeJXL.JxlPixelFormat(format);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetPreviewOutBuffer(&pixelFormat2, (void*)buffer, size);
				return status;
			}
		}
		/// <summary>
		/// Outputs the information from the frame, such as duration when have_animation.
		/// This function can be called when JXL_DEC_FRAME occurred for the current
		/// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
		/// </summary>
		/// <param name="header"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns> JXL_DEC_SUCCESS if the value is available,
		/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		/// of other error conditions.</returns>
		public JxlDecoderStatus GetFrameHeader(out JxlFrameHeader header)
		{
			UnsafeNativeJXL.JxlFrameHeader header2;
			string name;
			var status = (JxlDecoderStatus)decoderWrapper.GetFrameHeaderAndName(out header2, out name);
			header = new JxlFrameHeader();
			header2.WriteToPublic(header);
			header.Name = name;
			return status;
		}

		//commented out because Frame Name was added to JxlFrameHeader type
		///// <summary>
		///// Outputs name for the current frame. The buffer for name must have at least
		///// name_length + 1 bytes allocated, gotten from the associated JxlFrameHeader.
		///// </summary>
		///// <param name="name"> buffer to copy the name into</param>
		///// <param name="size"> size of the name buffer in bytes, including zero termination
		///// character, so this must be at least JxlFrameHeader.name_length + 1.</param>
		///// <returns> JXL_DEC_SUCCESS if the value is available,
		///// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
		///// of other error conditions.</returns>
		//public JxlDecoderStatus GetFrameName(out string name)
		//{
		//	var status = (JxlDecoderStatus)decoderWrapper.GetFrameName(out name);
		//	return status;
		//}

		/// <summary>
		/// Returns the minimum size in bytes of the DC image output buffer
		/// for the given format. This is the buffer for JxlDecoderSetDCOutBuffer.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// information not available yet.
		/// <br/><br/>
		/// DEPRECATED: the DC feature in this form will be removed. You can use
		/// JxlDecoderFlushImage for progressive rendering.</returns>
		[Obsolete]
		public JxlDecoderStatus GetDCOutBufferSize(JxlPixelFormat format, out int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetDCOutBufferSize(pFormat, out size);
					return status;
				}
			}
		}
		/// <summary>
		/// Sets the buffer to write the lower resolution (8x8 sub-sampled) DC image
		/// to. The size of the buffer must be at least as large as given by
		/// JxlDecoderDCOutBufferSize. The buffer follows the format described by
		/// JxlPixelFormat. The DC image has dimensions ceil(xsize / 8) * ceil(ysize /
		/// 8). The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// size too small.
		/// <br/><br/>
		/// DEPRECATED: the DC feature in this form will be removed. You can use
		/// JxlDecoderFlushImage for progressive rendering.</returns>
		[Obsolete]
		public JxlDecoderStatus SetDCOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			//TODO: prevent dangling pointer
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.SetDCOutBuffer(pFormat, (void*)buffer, size);
					return status;
				}
			}
		}

		/// <summary>
		/// Returns the minimum size in bytes of the image output pixel buffer for the
		/// given format. This is the buffer for JxlDecoderSetImageOutBuffer. Requires
		/// the basic image information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of the pixels.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// information not available yet.</returns>
		public JxlDecoderStatus GetImageOutBufferSize(JxlPixelFormat format, out int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetImageOutBufferSize(pFormat, out size);
					return status;
				}
			}
		}
		/// <summary>
		/// Warning: Pointer must remain valid between the call to SetImageOutBuffer
		/// and the call to ProcessInput which returns "Frame" as the status.
		/// <br/><br/>
		/// Sets the buffer to write the full resolution image to. This can be set when
		/// the JXL_DEC_FRAME event occurs, must be set when the
		/// JXL_DEC_NEED_IMAGE_OUT_BUFFER event occurs, and applies only for the current
		/// frame. The size of the buffer must be at least as large as given by
		/// JxlDecoderImageOutBufferSize. The buffer follows the format described by
		/// JxlPixelFormat. The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// size too small.</returns>
		public JxlDecoderStatus SetImageOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.SetImageOutBuffer(pFormat, (void*)buffer, size);
					return status;
				}
			}
		}

		///// <summary>
		///// Sets pixel output callback. This is an alternative to
		///// JxlDecoderSetImageOutBuffer. This can be set when the JXL_DEC_FRAME event
		///// occurs, must be set when the JXL_DEC_NEED_IMAGE_OUT_BUFFER event occurs, and
		///// applies only for the current frame. Only one of JxlDecoderSetImageOutBuffer
		///// or JxlDecoderSetImageOutCallback may be used for the same frame, not both at
		///// the same time.
		///// <br/><br/>
		///// The callback will be called multiple times, to receive the image
		///// data in small chunks. The callback receives a horizontal stripe of pixel
		///// data, 1 pixel high, xsize pixels wide, called a scanline. The xsize here is
		///// not the same as the full image width, the scanline may be a partial section,
		///// and xsize may differ between calls. The user can then process and/or copy the
		///// partial scanline to an image buffer. The callback may be called
		///// simultaneously by different threads when using a threaded parallel runner, on
		///// different pixels.
		///// <br/><br/>
		///// If JxlDecoderFlushImage is not used, then each pixel will be visited exactly
		///// once by the different callback calls, during processing with one or more
		///// JxlDecoderProcessInput calls. These pixels are decoded to full detail, they
		///// are not part of a lower resolution or lower quality progressive pass, but the
		///// final pass.
		///// <br/><br/>
		///// If JxlDecoderFlushImage is used, then in addition each pixel will be visited
		///// zero or one times during the blocking JxlDecoderFlushImage call. Pixels
		///// visited as a result of JxlDecoderFlushImage may represent a lower resolution
		///// or lower quality intermediate progressive pass of the image. Any visited
		///// pixel will be of a quality at least as good or better than previous visits of
		///// this pixel. A pixel may be visited zero times if it cannot be decoded yet
		///// or if it was already decoded to full precision (this behavior is not
		///// guaranteed).
		///// </summary>
		///// <param name="format"> format of the pixels. Object owned by user and its contents
		///// are copied internally.</param>
		///// <param name="callback"> the callback function receiving partial scanlines of pixel
		///// data.</param>
		///// <param name="opaque"> optional user data, which will be passed on to the callback,
		///// may be NULL.</param>
		///// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		///// JxlDecoderSetImageOutBuffer already set.</returns>
		////commented out for now because callbacks from C++ to C# are complicated
		//public JxlDecoderStatus SetImageOutCallback(JxlPixelFormat format, JxlImageOutCallback callback, void* opaque)
		//{
		//	//CheckIfDisposed();
		//	return JxlDecoderSetImageOutCallback(dec, format, callback, opaque);
		//}

		/// <summary>
		/// Returns the minimum size in bytes of an extra channel pixel buffer for the
		/// given format. This is the buffer for JxlDecoderSetExtraChannelBuffer.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of the pixels. The num_channels value is ignored and is
		/// always treated to be 1.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in @see
		/// JxlDecoderGetExtraChannelInfo. Must be smaller than num_extra_channels in the
		/// associated JxlBasicInfo.</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// information not available yet or invalid index.</returns>
		public JxlDecoderStatus GetExtraChannelBufferSize(JxlPixelFormat format, out int size, int index)
		{
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetExtraChannelBufferSize(pFormat, out size, index);
					return status;
				}
			}
		}
		/// <summary>
		/// Sets the buffer to write an extra channel to. This can be set when
		/// the JXL_DEC_FRAME or JXL_DEC_NEED_IMAGE_OUT_BUFFER event occurs, and applies
		/// only for the current frame. The size of the buffer must be at least as large
		/// as given by JxlDecoderExtraChannelBufferSize. The buffer follows the format
		/// described by JxlPixelFormat, but where num_channels is 1. The buffer is owned
		/// by the caller. The amount of extra channels is given by the
		/// num_extra_channels field in the associated JxlBasicInfo, and the information
		/// of individual extra channels can be queried with @see
		/// JxlDecoderGetExtraChannelInfo. To get multiple extra channels, this function
		/// must be called multiple times, once for each wanted index. Not all images
		/// have extra channels. The alpha channel is an extra channel and can be gotten
		/// as part of the color channels when using an RGBA pixel buffer with
		/// JxlDecoderSetImageOutBuffer, but additionally also can be gotten separately
		/// as extra channel. The color channels themselves cannot be gotten this way.
		/// <br/><br/>
		/// </summary>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally. The num_channels value is ignored and is always
		/// treated to be 1.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in @see
		/// JxlDecoderGetExtraChannelInfo. Must be smaller than num_extra_channels in the
		/// associated JxlBasicInfo.</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// size too small or invalid index.</returns>
		public JxlDecoderStatus SetExtraChannelBuffer(JxlPixelFormat format, IntPtr buffer, int size, int index)
		{
			unsafe
			{
				fixed (UnsafeNativeJXL.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.SetExtraChannelBuffer(pFormat, (void*)buffer, size, (uint)index);
					return status;
				}
			}
		}
		/// <summary>
		/// Warning: Pointer must remain valid between the call to SetJPEGBuffer and the call to ReleaseJPEGBuffer.
		/// Use the Array version of this function instead for better safety.
		/// <br/><br/>
		/// Sets output buffer for reconstructed JPEG codestream.
		/// <br/><br/>
		/// The data is owned by the caller and may be used by the decoder until
		/// JxlDecoderReleaseJPEGBuffer is called or the decoder is destroyed or reset so
		/// must be kept alive until then.
		/// <br/><br/>
		/// If a JPEG buffer was set before and released with
		/// JxlDecoderReleaseJPEGBuffer, bytes that the decoder has already output should
		/// not be included, only the remaining bytes output must be set.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns> JXL_DEC_ERROR if output buffer was already set and
		/// JxlDecoderReleaseJPEGBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
		public JxlDecoderStatus SetJPEGBuffer(IntPtr data, int size)
		{
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetJPEGBuffer((byte*)data, size);
				return status;
			}
		}
		/// <summary>
		/// Sets output buffer for reconstructed JPEG codestream.
		/// <br/><br/>
		/// The data is owned by the caller and may be used by the decoder until
		/// JxlDecoderReleaseJPEGBuffer is called or the decoder is destroyed or reset so
		/// must be kept alive until then.
		/// <br/><br/>
		/// If a JPEG buffer was set before and released with
		/// JxlDecoderReleaseJPEGBuffer, bytes that the decoder has already output should
		/// not be included, only the remaining bytes output must be set.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="outputPosition"> position within the output buffer</param>
		/// <returns> JXL_DEC_ERROR if output buffer was already set and
		/// JxlDecoderReleaseJPEGBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
		public JxlDecoderStatus SetJPEGBuffer(byte[] data, int outputPosition = 0)
		{
			var status = (JxlDecoderStatus)decoderWrapper.SetJPEGBuffer(data, outputPosition);
			return status;
		}
		/// <summary>
		/// Releases buffer which was provided with JxlDecoderSetJPEGBuffer.
		/// <br/><br/>
		/// Calling JxlDecoderReleaseJPEGBuffer is required whenever
		/// a buffer is already set and a new buffer needs to be added with
		/// JxlDecoderSetJPEGBuffer, but is not required before JxlDecoderDestroy or
		/// JxlDecoderReset.
		/// <br/><br/>
		/// Calling JxlDecoderReleaseJPEGBuffer when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by JxlDecoderSetJPEGBuffer, or 0 if no buffer is set or
		/// JxlDecoderReleaseJPEGBuffer was already called.</returns>
		public int ReleaseJPEGBuffer()
		{
			return decoderWrapper.ReleaseJPEGBuffer();
		}
		/// <summary>
		/// Warning: Pointer must remain valid between the call to SetBoxBuffer and the call to ReleaseBoxBufffer.
		/// Use the Array version of this function instead for better safety.
		/// <br/><br/>
		/// Sets output buffer for box output codestream.
		/// <br/><br/>
		/// The data is owned by the caller and may be used by the decoder until
		/// JxlDecoderReleaseBoxBuffer is called or the decoder is destroyed or reset so
		/// must be kept alive until then.
		/// <br/><br/>
		/// If for the current box a box buffer was set before and released with
		/// JxlDecoderReleaseBoxBuffer, bytes that the decoder has already output should
		/// not be included, only the remaining bytes output must be set.
		/// <br/><br/>
		/// The JxlDecoderReleaseBoxBuffer must be used at the next JXL_DEC_BOX event
		/// or final JXL_DEC_SUCCESS event to compute the size of the output box bytes.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns> JXL_DEC_ERROR if output buffer was already set and
		/// JxlDecoderReleaseBoxBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
		public JxlDecoderStatus SetBoxBuffer(IntPtr data, int size)
		{
			unsafe
			{
				return (JxlDecoderStatus)decoderWrapper.SetBoxBuffer((byte*)data, size);
			}
		}
		/// <summary>
		/// Sets output buffer for box output codestream.
		/// <br/><br/>
		/// The data is owned by the caller and may be used by the decoder until
		/// JxlDecoderReleaseBoxBuffer is called or the decoder is destroyed or reset so
		/// must be kept alive until then.
		/// <br/><br/>
		/// If for the current box a box buffer was set before and released with
		/// JxlDecoderReleaseBoxBuffer, bytes that the decoder has already output should
		/// not be included, only the remaining bytes output must be set.
		/// <br/><br/>
		/// The JxlDecoderReleaseBoxBuffer must be used at the next JXL_DEC_BOX event
		/// or final JXL_DEC_SUCCESS event to compute the size of the output box bytes.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <returns> JXL_DEC_ERROR if output buffer was already set and
		/// JxlDecoderReleaseBoxBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
		public JxlDecoderStatus SetBoxBuffer(byte[] data)
		{
			return (JxlDecoderStatus)decoderWrapper.SetBoxBuffer(data);
		}
		/// <summary>
		/// Releases buffer which was provided with JxlDecoderSetBoxBuffer.
		/// <br/><br/>
		/// Calling JxlDecoderReleaseBoxBuffer is required whenever
		/// a buffer is already set and a new buffer needs to be added with
		/// JxlDecoderSetBoxBuffer, but is not required before JxlDecoderDestroy or
		/// JxlDecoderReset.
		/// <br/><br/>
		/// Calling JxlDecoderReleaseBoxBuffer when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by JxlDecoderSetBoxBuffer, or 0 if no buffer is set or
		/// JxlDecoderReleaseBoxBuffer was already called.</returns>
		public int ReleaseBoxBuffer()
		{
			return decoderWrapper.ReleaseBoxBuffer();
		}
		/// <summary>
		/// Configures whether to get boxes in raw mode or in decompressed mode. In raw
		/// mode, boxes are output as their bytes appear in the container file, which may
		/// be decompressed, or compressed if their type is "brob". In decompressed mode,
		/// "brob" boxes are decompressed with Brotli before outputting them. The size of
		/// the decompressed stream is not known before the decompression has already
		/// finished.
		/// <br/><br/>
		/// The default mode is raw. This setting can only be changed before decoding, or
		/// directly after a JXL_DEC_BOX event, and is remembered until the decoder is
		/// reset or destroyed.
		/// <br/><br/>
		/// Enabling decompressed mode requires Brotli support from the library.
		/// </summary>
		/// <param name="decompress"> JXL_TRUE to transparently decompress, JXL_FALSE to get
		/// boxes in raw mode.</param>
		/// <returns> JXL_DEC_ERROR if decompressed mode is set and Brotli is not
		/// available, JXL_DEC_SUCCESS otherwise.</returns>
		public JxlDecoderStatus SetDecompressBoxes(bool decompress)
		{
			return (JxlDecoderStatus)decoderWrapper.SetDecompressBoxes(decompress);
		}
		/// <summary>
		/// Outputs the type of the current box, after a JXL_DEC_BOX event occured, as 4
		/// characters without null termination character. In case of a compressed "brob"
		/// box, this will return "brob" if the decompressed argument is JXL_FALSE, or
		/// the underlying box type if the decompressed argument is JXL_TRUE.
		/// </summary>
		/// <param name="boxType"> buffer to copy the type into</param>
		/// <param name="decompressed"> which box type to get: JXL_TRUE to get the raw box type,
		/// which can be "brob", JXL_FALSE, get the underlying box type.</param>
		/// <returns> JXL_DEC_SUCCESS if the value is available, JXL_DEC_ERROR if not, for
		/// example the JXL file does not use the container format.</returns>
		public JxlDecoderStatus GetBoxType(out string boxType, bool decompressed)
		{
			return (JxlDecoderStatus)decoderWrapper.GetBoxType(out boxType, decompressed);
		}
		/// <summary>
		/// Returns the size of a box as it appears in the container file, after the
		/// JXL_DEC_BOX event. For a non-compressed box, this is the size of the
		/// contents, excluding the 4 bytes indicating the box type. For a compressed
		/// "brob" box, this is the size of the compressed box contents plus the
		/// additional 4 byte indicating the underlying box type, but excluding the 4
		/// bytes indicating "brob". This function gives the size of the data that will
		/// be written in the output buffer when getting boxes in the default raw
		/// compressed mode. When JxlDecoderSetDecompressBoxes is enabled, the return
		/// value of function does not change, and the decompressed size is not known
		/// before it has already been decompressed and output.
		/// </summary>
		/// <param name="size"> raw size of the box in bytes</param>
		/// <returns> JXL_DEC_ERROR if no box size is available, JXL_DEC_SUCCESS otherwise.</returns>
		public JxlDecoderStatus GetBoxSizeRaw(out ulong size)
		{
			return (JxlDecoderStatus)decoderWrapper.GetBoxSizeRaw(out size);
		}
		/// <summary>
		/// Outputs progressive step towards the decoded image so far when only partial
		/// input was received. If the flush was successful, the buffer set with
		/// JxlDecoderSetImageOutBuffer will contain partial image data.
		/// <br/><br/>
		/// Can be called when JxlDecoderProcessInput returns JXL_DEC_NEED_MORE_INPUT,
		/// after the JXL_DEC_FRAME event already occurred and before the
		/// JXL_DEC_FULL_IMAGE event occurred for a frame.
		/// </summary>
		/// <returns> JXL_DEC_SUCCESS if image data was flushed to the output buffer, or
		/// JXL_DEC_ERROR when no flush was done, e.g. if not enough image data was
		/// available yet even for flush, or no output buffer was set yet. An error is
		/// not fatal, it only indicates no flushed image is available now, regular,
		/// decoding can still be performed.</returns>
		public JxlDecoderStatus FlushImage()
		{
			return (JxlDecoderStatus)decoderWrapper.FlushImage();
		}

	}

}
