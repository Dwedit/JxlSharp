using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
		bool decompressBoxes;
		JxlDecoderStatus lastStatus;

		UnsafeNativeJxl.JxlDecoderWrapper decoderWrapper;
		/// <summary>
		/// Creates a new JxlDecoder
		/// </summary>
		public JxlDecoder()
		{
			decoderWrapper = new UnsafeNativeJxl.JxlDecoderWrapper();
			decompressBoxes = false;
		}

		/// <summary>
		/// Disposes the JxlDecoder object.  Do not call any other methods after disposing this object.
		/// </summary>
		public void Dispose()
		{
			decoderWrapper.Dispose();
		}

		/// <summary>
		/// Re-initializes a <see cref="JxlDecoder" /> instance, so it can be re-used for decoding
		/// another image. All state and settings are reset as if the object was
		/// newly created with JxlDecoderCreate, but the memory manager is kept.
		/// </summary>
		public void Reset()
		{
			decoderWrapper.Reset();
			decompressBoxes = false;
		}
		/// <summary>
		/// Rewinds decoder to the beginning. The same input must be given again from
		/// the beginning of the file and the decoder will emit events from the beginning
		/// again. When rewinding (as opposed to <see cref="Reset" />), the decoder can
		/// keep state about the image, which it can use to skip to a requested frame
		/// more efficiently with <see cref="SkipFrames" />. Settings such as parallel
		/// runner or subscribed events are kept. After rewind, 
		/// <see cref="SubscribeEvents" /> can be used again, and it is feasible to leave out
		/// events that were already handled before, such as <see cref="JxlDecoderStatus.BasicInfo" />
		/// and <see cref="JxlDecoderStatus.ColorEncoding" />, since they will provide the same information
		/// as before.
		/// </summary>
		public void Rewind()
		{
			decoderWrapper.Rewind();
		}
		/// <summary>
		/// Makes the decoder skip the next `amount` frames. It still needs to process
		/// the input, but will not output the frame events. It can be more efficient
		/// when skipping frames, and even more so when using this after 
		/// <see cref="Rewind" />. If the decoder is already processing a frame (could
		/// have emitted <see cref="JxlDecoderStatus.Frame" /> but not yet <see cref="JxlDecoderStatus.FullImage" />), it
		/// starts skipping from the next frame. If the amount is larger than the amount
		/// of frames remaining in the image, all remaining frames are skipped. Calling
		/// this function multiple times adds the amount to skip to the already existing
		/// amount.
		/// <br /><br />
		/// A frame here is defined as a frame that without skipping emits events such
		/// as <see cref="JxlDecoderStatus.Frame" /> and <see cref="JxlDecoderStatus.FullImage" />, frames that are internal
		/// to the file format but are not rendered as part of an animation, or are not
		/// the final still frame of a still image, are not counted.
		/// </summary>
		/// <param name="amount"> the amount of frames to skip</param>
		public void SkipFrames(int amount)
		{
			decoderWrapper.SkipFrames(amount);
		}
		/// <summary>
		/// Skips processing the current frame. Can be called after frame processing
		/// already started, signaled by a <see cref="JxlDecoderStatus.NeedImageOutBuffer" /> event,
		/// but before the corrsponding <see cref="JxlDecoderStatus.FullImage" /> event. The next signaled
		/// event will be another <see cref="JxlDecoderStatus.Frame" />, or <see cref="JxlDecoderStatus.Success" /> if there
		/// are no more frames. If pixel data is required from the already processed part
		/// of the frame, <see cref="FlushImage" /> must be called before this.
		/// </summary>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if there is a frame to skip, and 
		/// <see cref="JxlDecoderStatus.Error" /> if the function was not called during frame processing.</returns>
		public JxlDecoderStatus SkipCurrentFrame()
		{
			return (JxlDecoderStatus)decoderWrapper.SkipCurrentFrame();
		}

		/// <summary>
		/// Get the default pixel format for this decoder.
		/// <br /><br />
		/// Requires that the decoder can produce JxlBasicInfo.
		/// </summary>
		/// <param name="format"> JxlPixelFormat to populate with the recommended settings for
		/// the data loaded into this decoder.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if no error, <see cref="JxlDecoderStatus.NeedMoreInput" /> if the
		/// basic info isn't yet available, and <see cref="JxlDecoderStatus.Error" /> otherwise.</returns>
		public JxlDecoderStatus GetDefaultPixelFormat(out JxlPixelFormat format)
		{
			format = new JxlPixelFormat();
			var status = (JxlDecoderStatus)decoderWrapper.GetDefaultPixelFormat(out format.pixelFormat);
			return status;
		}
		
		/// <summary>
		/// Returns a hint indicating how many more bytes the decoder is expected to
		/// need to make <see cref="GetBasicInfo" /> available after the next 
		/// <see cref="ProcessInput" /> call. This is a suggested large enough value for
		/// the amount of bytes to provide in the next <see cref="SetInput(byte[])" /> call, but
		/// it is not guaranteed to be an upper bound nor a lower bound. This number does
		/// not include bytes that have already been released from the input. Can be used
		/// before the first <see cref="ProcessInput" /> call, and is correct the first
		/// time in most cases. If not, <see cref="GetSizeHintBasicInfo" /> can be called
		/// again to get an updated hint.
		/// </summary>
		/// <returns> the size hint in bytes if the basic info is not yet fully decoded.</returns>
		/// <returns> 0 when the basic info is already available.</returns>
		public int GetSizeHintBasicInfo()
		{
			return decoderWrapper.GetSizeHintBasicInfo();
		}

		/// <summary>
		/// Select for which informative events, i.e. <see cref="JxlDecoderStatus.BasicInfo" />, etc., the
		/// decoder should return with a status. It is not required to subscribe to any
		/// events, data can still be requested from the decoder as soon as it available.
		/// By default, the decoder is subscribed to no events (events_wanted == 0), and
		/// the decoder will then only return when it cannot continue because it needs
		/// more input data or more output buffer. This function may only be be called
		/// before using <see cref="ProcessInput" />.
		/// </summary>
		/// <param name="eventsWanted"> bitfield of desired events.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if no error, <see cref="JxlDecoderStatus.Error" /> otherwise.</returns>
		public JxlDecoderStatus SubscribeEvents(JxlDecoderStatus eventsWanted)
		{
			return (JxlDecoderStatus)decoderWrapper.SubscribeEvents((int)eventsWanted);
		}

		/// <summary>
		/// Enables or disables preserving of as-in-bitstream pixeldata
		/// orientation. Some images are encoded with an Orientation tag
		/// indicating that the decoder must perform a rotation and/or
		/// mirroring to the encoded image data.
		/// <br />
		/// <br /> - If skip_reorientation is JXL_FALSE (the default): the decoder
		/// will apply the transformation from the orientation setting, hence
		/// rendering the image according to its specified intent. When
		/// producing a JxlBasicInfo, the decoder will always set the
		/// orientation field to JXL_ORIENT_IDENTITY (matching the returned
		/// pixel data) and also align xsize and ysize so that they correspond
		/// to the width and the height of the returned pixel data.
		/// <br /> - If skip_reorientation is JXL_TRUE: the decoder will skip
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
		/// <br /><br /><see cref="T:JxlSharp.UnsafeNativeJxl.JxlBasicInfo" /> for the orientation field, and <see cref="T:JxlSharp.UnsafeNativeJxl.JxlOrientation" /> for the
		/// possible values.
		/// </summary>
		/// <param name="keepOrientation"> JXL_TRUE to enable, JXL_FALSE to disable.</param>
		/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_ERROR otherwise.</returns>
		public JxlDecoderStatus SetKeepOrientation(bool keepOrientation)
		{
			return (JxlDecoderStatus)decoderWrapper.SetKeepOrientation(keepOrientation);
		}

		/// <summary>
		/// Enables or disables rendering spot colors. By default, spot colors
		/// are rendered, which is OK for viewing the decoded image. If render_spotcolors
		/// is JXL_FALSE, then spot colors are not rendered, and have to be retrieved
		/// separately using <see cref="SetExtraChannelBuffer" />. This is useful for
		/// e.g. printing applications.
		/// </summary>
		/// <param name="renderSpotcolors"> JXL_TRUE to enable (default), JXL_FALSE to disable.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if no error, <see cref="JxlDecoderStatus.Error" /> otherwise.</returns>
		public JxlDecoderStatus SetRenderSpotcolors(bool renderSpotcolors)
		{
			return (JxlDecoderStatus)decoderWrapper.SetRenderSpotcolors(renderSpotcolors);
		}

		/// <summary>
		/// Enables or disables coalescing of zero-duration frames. By default, frames
		/// are returned with coalescing enabled, i.e. all frames have the image
		/// dimensions, and are blended if needed. When coalescing is disabled, frames
		/// can have arbitrary dimensions, a non-zero crop offset, and blending is not
		/// performed. For display, coalescing is recommended. For loading a multi-layer
		/// still image as separate layers (as opposed to the merged image), coalescing
		/// has to be disabled.
		/// </summary>
		/// <param name="coalescing"> JXL_TRUE to enable coalescing (default), JXL_FALSE to
		/// disable it.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if no error, <see cref="JxlDecoderStatus.Error" /> otherwise.</returns>
		public JxlDecoderStatus SetCoalescing(bool coalescing)
		{
			return (JxlDecoderStatus)decoderWrapper.SetCoalescing(coalescing);
		}

		/// <summary>
		/// Decodes JPEG XL file using the available bytes. Requires input has been
		/// set with <see cref="SetInput(byte[])" />. After <see cref="ProcessInput" />, input
		/// can optionally be released with <see cref="ReleaseInput" /> and then set
		/// again to next bytes in the stream. <see cref="ReleaseInput" /> returns how
		/// many bytes are not yet processed, before a next call to 
		/// <see cref="ProcessInput" /> all unprocessed bytes must be provided again (the
		/// address need not match, but the contents must), and more bytes may be
		/// concatenated after the unprocessed bytes.
		/// <br /><br />
		/// The returned status indicates whether the decoder needs more input bytes, or
		/// more output buffer for a certain type of output data. No matter what the
		/// returned status is (other than <see cref="JxlDecoderStatus.Error" />), new information, such
		/// as <see cref="GetBasicInfo" />, may have become available after this call.
		/// When the return value is not <see cref="JxlDecoderStatus.Error" /> or <see cref="JxlDecoderStatus.Success" />, the
		/// decoding requires more <see cref="ProcessInput" /> calls to continue.
		/// </summary>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> when decoding finished and all events handled.
		/// If you still have more unprocessed input data anyway, then you can still
		/// continue by using <see cref="SetInput(byte[])" /> and calling 
		/// <see cref="ProcessInput" /> again, similar to handling 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" />. <see cref="JxlDecoderStatus.Success" /> can occur instead of 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> when, for example, the input data ended right at
		/// the boundary of a box of the container format, all essential codestream
		/// boxes were already decoded, but extra metadata boxes are still present in
		/// the next data. <see cref="ProcessInput" /> cannot return success if all
		/// codestream boxes have not been seen yet.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> when decoding failed, e.g. invalid codestream.
		/// TODO(lode): document the input data mechanism</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.NeedMoreInput" /> when more input data is necessary.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.BasicInfo" /> when basic info such as image dimensions is
		/// available and this informative event is subscribed to.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.ColorEncoding" /> when color profile information is
		/// available and this informative event is subscribed to.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.PreviewImage" /> when preview pixel information is
		/// available and output in the preview buffer.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.DcImage" /> when DC pixel information (8x8 downscaled
		/// version of the image) is available and output is in the DC buffer.</returns>
		/// <returns>
		///     <see cref="JxlDecoderStatus.FullImage" /> when all pixel information at highest detail
		/// is available and has been output in the pixel buffer.</returns>
		public JxlDecoderStatus ProcessInput()
		{
			lastStatus = (JxlDecoderStatus)decoderWrapper.ProcessInput();
			return lastStatus;
		}

		/// <summary>
		/// Sets input data for <see cref="ProcessInput" />. The data is owned by the
		/// caller and may be used by the decoder until <see cref="ReleaseInput" /> is
		/// called or the decoder is destroyed or reset so must be kept alive until then.
		/// Cannot be called if <see cref="SetInput(byte[])" /> was already called and 
		/// <see cref="ReleaseInput" /> was not yet called, and cannot be called after 
		/// <see cref="CloseInput" /> indicating the end of input was called.
		/// </summary>
		/// <param name="data"> pointer to next bytes to read from</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if input was already set without releasing or 
		/// <see cref="CloseInput" /> was already called, <see cref="JxlDecoderStatus.Success" /> otherwise.</returns>
		public JxlDecoderStatus SetInput([In] IntPtr data, int size)
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
		/// Releases input which was provided with <see cref="SetInput(byte[])" />. Between 
		/// <see cref="ProcessInput" /> and <see cref="ReleaseInput" />, the user may not
		/// alter the data in the buffer. Calling <see cref="ReleaseInput" /> is required
		/// whenever any input is already set and new input needs to be added with 
		/// <see cref="SetInput(byte[])" />, but is not required before <see cref="Dispose" /> or 
		/// <see cref="Reset" />. Calling <see cref="ReleaseInput" /> when no input is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> The amount of bytes the decoder has not yet processed that are still
		/// remaining in the data set by <see cref="SetInput(byte[])" />, or 0 if no input is
		/// set or <see cref="ReleaseInput" /> was already called. For a next call
		/// to <see cref="ProcessInput" />, the buffer must start with these
		/// unprocessed bytes. This value doesn't provide information about how many
		/// bytes the decoder truly processed internally or how large the original
		/// JPEG XL codestream or file are.</returns>
		public int ReleaseInput()
		{
			return decoderWrapper.ReleaseInput();
		}

		/// <summary>
		/// Marks the input as finished, indicates that no more <see cref="SetInput(byte[])" />
		/// will be called. This function allows the decoder to determine correctly if it
		/// should return success, need more input or error in certain cases. For
		/// backwards compatibility with a previous version of the API, using this
		/// function is optional when not using the <see cref="JxlDecoderStatus.Box" /> event (the decoder
		/// is able to determine the end of the image frames without marking the end),
		/// but using this function is required when using <see cref="JxlDecoderStatus.Box" /> for getting
		/// metadata box contents. This function does not replace 
		/// <see cref="ReleaseInput" />, that function should still be called if its return
		/// value is needed.
		/// <br /><br /><see cref="CloseInput" /> should be called as soon as all known input bytes
		/// are set (e.g. at the beginning when not streaming but setting all input
		/// at once), before the final <see cref="ProcessInput" /> calls.
		/// </summary>
		public void CloseInput()
		{
			decoderWrapper.CloseInput();
		}

		/// <summary>
		/// Outputs the basic image information, such as image dimensions, bit depth and
		/// all other JxlBasicInfo fields, if available.
		/// </summary>
		/// <param name="info"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" />
		/// in case of other error conditions.</returns>
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
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" />
		/// in case of other error conditions.</returns>
		public JxlDecoderStatus GetExtraChannelInfo(int index, out JxlExtraChannelInfo info)
		{
			UnsafeNativeJxl.JxlExtraChannelInfo info2;
			var status = (JxlDecoderStatus)decoderWrapper.GetExtraChannelInfo(index, out info2);
			info = new JxlExtraChannelInfo(ref info2);
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
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" />
		/// in case of other error conditions.</returns>
		public JxlDecoderStatus GetExtraChannelName(int index, out string name)
		{
			return (JxlDecoderStatus)decoderWrapper.GetExtraChannelName(index, out name);
		}

		/// <summary>
		/// Outputs the color profile as JPEG XL encoded structured data, if available.
		/// This is an alternative to an ICC Profile, which can represent a more limited
		/// amount of color spaces, but represents them exactly through enum values.
		/// <br /><br />
		/// It is often possible to use <see cref="GetColorAsICCProfile" /> as an
		/// alternative anyway. The following scenarios are possible:
		/// <br /> - The JPEG XL image has an attached ICC Profile, in that case, the encoded
		/// structured data is not available, this function will return an error
		/// status. <see cref="GetColorAsICCProfile" /> should be called instead.
		/// <br /> - The JPEG XL image has an encoded structured color profile, and it
		/// represents an RGB or grayscale color space. This function will return it.
		/// You can still use <see cref="GetColorAsICCProfile" /> as well as an
		/// alternative if desired, though depending on which RGB color space is
		/// represented, the ICC profile may be a close approximation. It is also not
		/// always feasible to deduce from an ICC profile which named color space it
		/// exactly represents, if any, as it can represent any arbitrary space.
		/// <br /> - The JPEG XL image has an encoded structured color profile, and it
		/// indicates an unknown or xyb color space. In that case, 
		/// <see cref="GetColorAsICCProfile" /> is not available.
		/// <br /><br />
		/// When rendering an image on a system that supports ICC profiles, 
		/// <see cref="GetColorAsICCProfile" /> should be used first. When rendering
		/// for a specific color space, possibly indicated in the JPEG XL
		/// image, <see cref="GetColorAsEncodedProfile" /> should be used first.
		/// </summary>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.Data" />, may be nullptr otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="colorEncoding"> struct to copy the information into, or NULL to only
		/// check whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the data is available and returned, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" /> in
		/// case the encoded structured color profile does not exist in the
		/// codestream.</returns>
		public JxlDecoderStatus GetColorAsEncodedProfile(JxlPixelFormat format, JxlColorProfileTarget target, out JxlColorEncoding colorEncoding)
		{
			colorEncoding = new JxlColorEncoding();
			unsafe
			{
				if (format != null)
				{
					fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
					{
						var status = (JxlDecoderStatus)decoderWrapper.GetColorAsEncodedProfile(pFormat, (UnsafeNativeJxl.JxlColorProfileTarget)target, out colorEncoding.colorEncoding);
						return status;
					}
				}
				else
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetColorAsEncodedProfile(null, (UnsafeNativeJxl.JxlColorProfileTarget)target, out colorEncoding.colorEncoding);
					return status;
				}
			}
		}

		/// <summary>
		/// Outputs the size in bytes of the ICC profile returned by 
		/// <see cref="GetColorAsICCProfile" />, if available, or indicates there is none
		/// available. In most cases, the image will have an ICC profile available, but
		/// if it does not, <see cref="GetColorAsEncodedProfile" /> must be used instead.
		/// <br /><br /><see cref="GetColorAsEncodedProfile" /> for more information. The ICC
		/// profile is either the exact ICC profile attached to the codestream metadata,
		/// or a close approximation generated from JPEG XL encoded structured data,
		/// depending of what is encoded in the codestream.
		/// </summary>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.Data" />, may be NULL otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="size"> variable to output the size into, or NULL to only check the
		/// return status.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the ICC profile is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if the decoder has not yet received enough
		/// input data to determine whether an ICC profile is available or what its
		/// size is, <see cref="JxlDecoderStatus.Error" /> in case the ICC profile is not available and
		/// cannot be generated.</returns>
		public JxlDecoderStatus GetICCProfileSize(JxlPixelFormat format, JxlColorProfileTarget target, out int size)
		{
			JxlDecoderStatus status;
			unsafe
			{
				if (format != null)
				{
					fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
					{
						status = (JxlDecoderStatus)decoderWrapper.GetICCProfileSize(pFormat, (UnsafeNativeJxl.JxlColorProfileTarget)target, out size);
						return status;
					}
				}
				else
				{
					status = (JxlDecoderStatus)decoderWrapper.GetICCProfileSize(null, (UnsafeNativeJxl.JxlColorProfileTarget)target, out size);
					return status;
				}
			}
		}

		/// <summary>
		/// Outputs ICC profile if available. The profile is only available if 
		/// <see cref="GetICCProfileSize" /> returns success. The output buffer must have
		/// at least as many bytes as given by <see cref="GetICCProfileSize" />.
		/// </summary>
		/// <param name="format"> pixel format to output the data to. Only used for 
		/// <see cref="JxlColorProfileTarget.Data" />, may be NULL otherwise.</param>
		/// <param name="target"> whether to get the original color profile from the metadata
		/// or the color profile of the decoded pixels.</param>
		/// <param name="iccProfile"> buffer to copy the ICC profile into</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the profile was successfully returned is
		/// available, <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, 
		/// <see cref="JxlDecoderStatus.Error" /> if the profile doesn't exist or the output size is not
		/// large enough.</returns>
		public JxlDecoderStatus GetColorAsICCProfile(JxlPixelFormat format, JxlColorProfileTarget target, out byte[] iccProfile)
		{
			unsafe
			{
				if (format != null)
				{
					fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
					{
						var status = (JxlDecoderStatus)decoderWrapper.GetColorAsICCProfile(pFormat, (UnsafeNativeJxl.JxlColorProfileTarget)target, out iccProfile);
						return status;
					}
				}
				else
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetColorAsICCProfile(null, (UnsafeNativeJxl.JxlColorProfileTarget)target, out iccProfile);
					return status;
				}
			}
		}

		/// <summary>
		/// Sets the color profile to use for <see cref="JxlColorProfileTarget.Data" /> for the
		/// special case when the decoder has a choice. This only has effect for a JXL
		/// image where uses_original_profile is false. If uses_original_profile is true,
		/// this setting is ignored and the decoder uses a profile related to the image.
		/// No matter what, the <see cref="JxlColorProfileTarget.Data" /> must still be queried
		/// to know the actual data format of the decoded pixels after decoding.
		/// <br /><br />
		/// The JXL decoder has no color management system built in, but can convert XYB
		/// color to any of the ones supported by JxlColorEncoding. Note that if the
		/// requested color encoding has a narrower gamut, or the white points differ,
		/// then the resulting image can have significant color distortion.
		/// <br /><br />
		/// Can only be set after the <see cref="JxlDecoderStatus.ColorEncoding" /> event occurred and
		/// before any other event occurred, and can affect the result of 
		/// <see cref="JxlColorProfileTarget.Data" /> (but not of 
		/// <see cref="JxlColorProfileTarget.Original" />), so should be used after getting 
		/// <see cref="JxlColorProfileTarget.Original" /> but before getting 
		/// <see cref="JxlColorProfileTarget.Data" />. The color_encoding must be grayscale if
		/// num_color_channels from the basic info is 1, RGB if num_color_channels from
		/// the basic info is 3.
		/// <br /><br />
		/// If <see cref="SetPreferredColorProfile" /> is not used, then for images for
		/// which uses_original_profile is false and with ICC color profile, the decoder
		/// will choose linear sRGB for color images, linear grayscale for grayscale
		/// images. This function only sets a preference, since for other images the
		/// decoder has no choice what color profile to use, it is determined by the
		/// image.
		/// </summary>
		/// <param name="colorEncoding"> the default color encoding to set</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the preference was set successfully, 
		/// <see cref="JxlDecoderStatus.Error" /> otherwise.</returns>
		public JxlDecoderStatus SetPreferredColorProfile(JxlColorEncoding colorEncoding)
		{
			UnsafeNativeJxl.JxlColorEncoding colorEncoding2;
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out colorEncoding2, colorEncoding);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetPreferredColorProfile(&colorEncoding2);
				return status;
			}
		}

		/// <summary>
		/// Returns the minimum size in bytes of the preview image output pixel buffer
		/// for the given format. This is the buffer for 
		/// <see cref="SetPreviewOutBuffer" />. Requires the preview header information is
		/// available in the decoder.
		/// </summary>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// information not available yet.</returns>
		public JxlDecoderStatus GetPreviewOutBufferSize(JxlPixelFormat format, out int size)
		{
			UnsafeNativeJxl.JxlPixelFormat pixelFormat2;
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out pixelFormat2, format);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.GetPreviewOutBufferSize(&pixelFormat2, out size);
				return status;
			}
		}

		/// <summary>
		/// Sets the buffer to write the small resolution preview image
		/// to. The size of the buffer must be at least as large as given by 
		/// <see cref="GetPreviewOutBufferSize" />. The buffer follows the format described
		/// by JxlPixelFormat. The preview image dimensions are given by the
		/// JxlPreviewHeader. The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// size too small.</returns>
		public JxlDecoderStatus SetPreviewOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			//TODO remove the dangling pointer
			UnsafeNativeJxl.JxlPixelFormat pixelFormat2;
			UnsafeNativeJxl.CopyFields.ReadFromPublic(out pixelFormat2, format);
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetPreviewOutBuffer(&pixelFormat2, (void*)buffer, size);
				return status;
			}
		}

		/// <summary>
		/// Outputs the information from the frame, such as duration when have_animation.
		/// This function can be called when <see cref="JxlDecoderStatus.Frame" /> occurred for the current
		/// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
		/// </summary>
		/// <param name="header"> struct to copy the information into, or NULL to only check
		/// whether the information is available through the return value.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" /> in
		/// case of other error conditions.</returns>
		public JxlDecoderStatus GetFrameHeader(out JxlFrameHeader header)
		{
			UnsafeNativeJxl.JxlFrameHeader header2;
			string name;
			var status = (JxlDecoderStatus)decoderWrapper.GetFrameHeaderAndName(out header2, out name);
			header = new JxlFrameHeader(ref header2);
			//UnsafeNativeJxl.CopyFields.WriteToPublic(ref header2, header);
			header.Name = name;
			return status;
		}

		/// <summary>
		/// Outputs name for the current frame. The buffer for name must have at least
		/// name_length + 1 bytes allocated, gotten from the associated JxlFrameHeader.
		/// </summary>
		/// <param name="name"> buffer to copy the name into</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" /> if not yet available, <see cref="JxlDecoderStatus.Error" /> in
		/// case of other error conditions.</returns>
		public JxlDecoderStatus GetFrameName(out string name)
		{
			var status = (JxlDecoderStatus)decoderWrapper.GetFrameName(out name);
			return status;
		}

		/// <summary>
		/// Outputs the blend information for the current frame for a specific extra
		/// channel. This function can be called when <see cref="JxlDecoderStatus.Frame" /> occurred for the
		/// current frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
		/// This information is only useful if coalescing is disabled; otherwise the
		/// decoder will have performed blending already.
		/// </summary>
		/// <param name="index"> the index of the extra channel</param>
		/// <param name="blend_info"> struct to copy the information into</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error</returns>
		public JxlDecoderStatus GetExtraChannelBlendInfo(int index, out JxlBlendInfo blend_info)
		{
			UnsafeNativeJxl.JxlBlendInfo blendInfo2;
			var status = (JxlDecoderStatus)decoderWrapper.GetExtraChannelBlendInfo(index, out blendInfo2);
			UnsafeNativeJxl.CopyFields.WriteToPublic(ref blendInfo2, out blend_info);
			return status;
		}

		/// <summary>
		/// Returns the minimum size in bytes of the DC image output buffer
		/// for the given format. This is the buffer for <see cref="SetDCOutBuffer" />.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of pixels</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// information not available yet.
		/// <br /><br />
		/// @deprecated The DC feature in this form will be removed. Use 
		/// <see cref="FlushImage" /> for progressive rendering.</returns>
		[Obsolete]
		public JxlDecoderStatus GetDCOutBufferSize(JxlPixelFormat format, out int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.GetDCOutBufferSize(pFormat, out size);
					return status;
				}
			}
		}
		/// <summary>
		/// Sets the buffer to write the lower resolution (8x8 sub-sampled) DC image
		/// to. The size of the buffer must be at least as large as given by 
		/// <see cref="GetDCOutBufferSize" />. The buffer follows the format described by
		/// JxlPixelFormat. The DC image has dimensions ceil(xsize / 8) * ceil(ysize /
		/// 8). The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of pixels. Object owned by user and its contents are
		/// copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// size too small.
		/// <br /><br />
		/// @deprecated The DC feature in this form will be removed. Use 
		/// <see cref="FlushImage" /> for progressive rendering.</returns>
		[Obsolete]
		public JxlDecoderStatus SetDCOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			//TODO: prevent dangling pointer
			unsafe
			{
				fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.SetDCOutBuffer(pFormat, (void*)buffer, size);
					return status;
				}
			}
		}

		/// <summary>
		/// Returns the minimum size in bytes of the image output pixel buffer for the
		/// given format. This is the buffer for <see cref="SetImageOutBuffer" />.
		/// Requires that the basic image information is available in the decoder in the
		/// case of coalescing enabled (default). In case coalescing is disabled, this
		/// can only be called after the <see cref="JxlDecoderStatus.Frame" /> event occurs. In that case,
		/// it will return the size required to store the possibly cropped frame (which
		/// can be larger or smaller than the image dimensions).
		/// </summary>
		/// <param name="format"> format of the pixels.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// information not available yet.</returns>
		public JxlDecoderStatus GetImageOutBufferSize(JxlPixelFormat format, out int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
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
		/// the <see cref="JxlDecoderStatus.Frame" /> event occurs, must be set when the 
		/// <see cref="JxlDecoderStatus.NeedImageOutBuffer" /> event occurs, and applies only for the
		/// current frame. The size of the buffer must be at least as large as given
		/// by <see cref="GetImageOutBufferSize" />. The buffer follows the format described
		/// by JxlPixelFormat. The buffer is owned by the caller.
		/// </summary>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// size too small.</returns>
		public JxlDecoderStatus SetImageOutBuffer(JxlPixelFormat format, IntPtr buffer, int size)
		{
			unsafe
			{
				fixed (UnsafeNativeJxl.JxlPixelFormat* pFormat = &format.pixelFormat)
				{
					var status = (JxlDecoderStatus)decoderWrapper.SetImageOutBuffer(pFormat, (void*)buffer, size);
					return status;
				}
			}
		}

		/// <summary>
		/// Sets pixel output callback. This is an alternative to
		/// JxlDecoderSetImageOutBuffer. This can be set when the JXL_DEC_FRAME event
		/// occurs, must be set when the JXL_DEC_NEED_IMAGE_OUT_BUFFER event occurs, and
		/// applies only for the current frame. Only one of JxlDecoderSetImageOutBuffer
		/// or JxlDecoderSetImageOutCallback may be used for the same frame, not both at
		/// the same time.
		/// <br/><br/>
		/// The callback will be called multiple times, to receive the image
		/// data in small chunks. The callback receives a horizontal stripe of pixel
		/// data, 1 pixel high, xsize pixels wide, called a scanline. The xsize here is
		/// not the same as the full image width, the scanline may be a partial section,
		/// and xsize may differ between calls. The user can then process and/or copy the
		/// partial scanline to an image buffer. The callback may be called
		/// simultaneously by different threads when using a threaded parallel runner, on
		/// different pixels.
		/// <br/><br/>
		/// If JxlDecoderFlushImage is not used, then each pixel will be visited exactly
		/// once by the different callback calls, during processing with one or more
		/// JxlDecoderProcessInput calls. These pixels are decoded to full detail, they
		/// are not part of a lower resolution or lower quality progressive pass, but the
		/// final pass.
		/// <br/><br/>
		/// If JxlDecoderFlushImage is used, then in addition each pixel will be visited
		/// zero or one times during the blocking JxlDecoderFlushImage call. Pixels
		/// visited as a result of JxlDecoderFlushImage may represent a lower resolution
		/// or lower quality intermediate progressive pass of the image. Any visited
		/// pixel will be of a quality at least as good or better than previous visits of
		/// this pixel. A pixel may be visited zero times if it cannot be decoded yet
		/// or if it was already decoded to full precision (this behavior is not
		/// guaranteed).
		/// </summary>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally.</param>
		/// <param name="callback"> the callback function receiving partial scanlines of pixel
		/// data.</param>
		/// <param name="opaque"> optional user data, which will be passed on to the callback,
		/// may be NULL.</param>
		/// <returns> JXL_DEC_SUCCESS on success, JXL_DEC_ERROR on error, such as
		/// JxlDecoderSetImageOutBuffer already set.</returns>
		public JxlDecoderStatus SetImageOutCallback(JxlPixelFormat format, IntPtr callback, IntPtr opaque)
		{
			JxlDecoderStatus status;
			unsafe
			{
				status = (JxlDecoderStatus)decoderWrapper.SetImageOutCallback(ref format.pixelFormat, (UIntPtr)(void*)callback, (void*)opaque);
			}
			return status;
		}

		/// <summary>
		/// Returns the minimum size in bytes of an extra channel pixel buffer for the
		/// given format. This is the buffer for <see cref="SetExtraChannelBuffer" />.
		/// Requires the basic image information is available in the decoder.
		/// </summary>
		/// <param name="format"> format of the pixels. The num_channels value is ignored and is
		/// always treated to be 1.</param>
		/// <param name="size"> output value, buffer size in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in 
		/// <see cref="GetExtraChannelInfo" />. Must be smaller than num_extra_channels in
		/// the associated JxlBasicInfo.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// information not available yet or invalid index.</returns>
		public JxlDecoderStatus GetExtraChannelBufferSize(JxlPixelFormat format, out int size, int index)
		{
			var status = (JxlDecoderStatus)decoderWrapper.GetExtraChannelBufferSize(ref format.pixelFormat, out size, index);
			return status;
		}
		/// <summary>
		/// Sets the buffer to write an extra channel to. This can be set when
		/// the <see cref="JxlDecoderStatus.Frame" /> or <see cref="JxlDecoderStatus.NeedImageOutBuffer" /> event occurs,
		/// and applies only for the current frame. The size of the buffer must be at
		/// least as large as given by <see cref="GetExtraChannelBufferSize" />. The buffer
		/// follows the format described by JxlPixelFormat, but where num_channels is 1.
		/// The buffer is owned by the caller. The amount of extra channels is given by
		/// the num_extra_channels field in the associated JxlBasicInfo, and the
		/// information of individual extra channels can be queried with 
		/// <see cref="GetExtraChannelInfo" />. To get multiple extra channels, this function
		/// must be called multiple times, once for each wanted index. Not all images
		/// have extra channels. The alpha channel is an extra channel and can be gotten
		/// as part of the color channels when using an RGBA pixel buffer with 
		/// <see cref="SetImageOutBuffer" />, but additionally also can be gotten
		/// separately as extra channel. The color channels themselves cannot be gotten
		/// this way.
		/// <br /><br /></summary>
		/// <param name="format"> format of the pixels. Object owned by user and its contents
		/// are copied internally. The num_channels value is ignored and is always
		/// treated to be 1.</param>
		/// <param name="buffer"> buffer type to output the pixel data to</param>
		/// <param name="size"> size of buffer in bytes</param>
		/// <param name="index"> which extra channel to get, matching the index used in 
		/// <see cref="GetExtraChannelInfo" />. Must be smaller than num_extra_channels in
		/// the associated JxlBasicInfo.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// size too small or invalid index.</returns>
		public JxlDecoderStatus SetExtraChannelBuffer(JxlPixelFormat format, IntPtr buffer, int size, int index)
		{
			unsafe
			{
				var status = (JxlDecoderStatus)decoderWrapper.SetExtraChannelBuffer(ref format.pixelFormat, (void*)buffer, size, (uint)index);
				return status;
			}
		}

		///// <summary>
		///// Warning: Pointer must remain valid between the call to SetJPEGBuffer and the call to ReleaseJPEGBuffer.
		///// Use the Array version of this function instead for better safety.
		///// <br/><br/>
		///// Sets output buffer for reconstructed JPEG codestream.
		///// <br /><br />
		///// The data is owned by the caller and may be used by the decoder until 
		///// <see cref="ReleaseJPEGBuffer" /> is called or the decoder is destroyed or
		///// reset so must be kept alive until then.
		///// <br /><br />
		///// If a JPEG buffer was set before and released with 
		///// <see cref="ReleaseJPEGBuffer" />, bytes that the decoder has already output
		///// should not be included, only the remaining bytes output must be set.
		///// </summary>
		///// <param name="data"> pointer to next bytes to write to</param>
		///// <param name="size"> amount of bytes available starting from data</param>
		///// <returns>
		/////     <see cref="JxlDecoderStatus.Error" /> if output buffer was already set and 
		///// <see cref="ReleaseJPEGBuffer" /> was not called on it, <see cref="JxlDecoderStatus.Success" />
		///// otherwise</returns>
		//public JxlDecoderStatus SetJPEGBuffer(IntPtr data, int size)
		//{
		//	unsafe
		//	{
		//		var status = (JxlDecoderStatus)decoderWrapper.SetJPEGBuffer((byte*)data, size);
		//		return status;
		//	}
		//}
		
		/// <summary>
		/// Sets output buffer for reconstructed JPEG codestream.
		/// <br /><br />
		/// The data is owned by the caller and may be used by the decoder until 
		/// <see cref="ReleaseJPEGBuffer" /> is called or the decoder is destroyed or
		/// reset so must be kept alive until then.
		/// <br /><br />
		/// If a JPEG buffer was set before and released with 
		/// <see cref="ReleaseJPEGBuffer" />, bytes that the decoder has already output
		/// should not be included, only the remaining bytes output must be set.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="outputPosition">output position within the array to start at</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if output buffer was already set and 
		/// <see cref="ReleaseJPEGBuffer" /> was not called on it, <see cref="JxlDecoderStatus.Success" />
		/// otherwise</returns>
		public JxlDecoderStatus SetJPEGBuffer(byte[] data, int outputPosition = 0)
		{
			var status = (JxlDecoderStatus)decoderWrapper.SetJPEGBuffer(data, outputPosition);
			return status;
		}

		/// <summary>
		/// Releases buffer which was provided with <see cref="SetJPEGBuffer" />.
		/// <br /><br />
		/// Calling <see cref="ReleaseJPEGBuffer" /> is required whenever
		/// a buffer is already set and a new buffer needs to be added with 
		/// <see cref="SetJPEGBuffer" />, but is not required before 
		/// <see cref="Dispose" /> or <see cref="Reset" />.
		/// <br /><br />
		/// Calling <see cref="ReleaseJPEGBuffer" /> when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by <see cref="SetJPEGBuffer" />, or 0 if no buffer is set or 
		/// <see cref="ReleaseJPEGBuffer" /> was already called.</returns>
		public int ReleaseJPEGBuffer()
		{
			return decoderWrapper.ReleaseJPEGBuffer();
		}

		/// <summary>
		/// Warning: Pointer must remain valid between the call to SetBoxBuffer and the call to ReleaseBoxBufffer.
		/// Use the Array version of this function instead for better safety.
		/// <br/><br/>
		/// Sets output buffer for box output codestream.
		/// <br /><br />
		/// The data is owned by the caller and may be used by the decoder until 
		/// <see cref="ReleaseBoxBuffer" /> is called or the decoder is destroyed or
		/// reset so must be kept alive until then.
		/// <br /><br />
		/// If for the current box a box buffer was set before and released with 
		/// <see cref="ReleaseBoxBuffer" />, bytes that the decoder has already output
		/// should not be included, only the remaining bytes output must be set.
		/// <br /><br />
		/// The <see cref="ReleaseBoxBuffer" /> must be used at the next <see cref="JxlDecoderStatus.Box" />
		/// event or final <see cref="JxlDecoderStatus.Success" /> event to compute the size of the output
		/// box bytes.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <param name="size"> amount of bytes available starting from data</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if output buffer was already set and 
		/// <see cref="ReleaseBoxBuffer" /> was not called on it, <see cref="JxlDecoderStatus.Success" />
		/// otherwise</returns>
		public JxlDecoderStatus SetBoxBuffer(IntPtr data, int size)
		{
			unsafe
			{
				return (JxlDecoderStatus)decoderWrapper.SetBoxBuffer((byte*)data, size);
			}
		}

		/// <summary>
		/// Sets output buffer for box output codestream.
		/// <br /><br />
		/// The data is owned by the caller and may be used by the decoder until 
		/// <see cref="ReleaseBoxBuffer" /> is called or the decoder is destroyed or
		/// reset so must be kept alive until then.
		/// <br /><br />
		/// If for the current box a box buffer was set before and released with 
		/// <see cref="ReleaseBoxBuffer" />, bytes that the decoder has already output
		/// should not be included, only the remaining bytes output must be set.
		/// <br /><br />
		/// The <see cref="ReleaseBoxBuffer" /> must be used at the next <see cref="JxlDecoderStatus.Box" />
		/// event or final <see cref="JxlDecoderStatus.Success" /> event to compute the size of the output
		/// box bytes.
		/// </summary>
		/// <param name="data"> pointer to next bytes to write to</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if output buffer was already set and 
		/// <see cref="ReleaseBoxBuffer" /> was not called on it, <see cref="JxlDecoderStatus.Success" />
		/// otherwise</returns>
		public JxlDecoderStatus SetBoxBuffer(byte[] data)
		{
			return (JxlDecoderStatus)decoderWrapper.SetBoxBuffer(data);
		}

		/// <summary>
		/// Releases buffer which was provided with <see cref="SetBoxBuffer" />.
		/// <br /><br />
		/// Calling <see cref="ReleaseBoxBuffer" /> is required whenever
		/// a buffer is already set and a new buffer needs to be added with 
		/// <see cref="SetBoxBuffer" />, but is not required before 
		/// <see cref="Dispose" /> or <see cref="Reset" />.
		/// <br /><br />
		/// Calling <see cref="ReleaseBoxBuffer" /> when no buffer is set is
		/// not an error and returns 0.
		/// </summary>
		/// <returns> the amount of bytes the decoder has not yet written to of the data
		/// set by <see cref="SetBoxBuffer" />, or 0 if no buffer is set or 
		/// <see cref="ReleaseBoxBuffer" /> was already called.</returns>
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
		/// <br /><br />
		/// The default mode is raw. This setting can only be changed before decoding, or
		/// directly after a <see cref="JxlDecoderStatus.Box" /> event, and is remembered until the decoder
		/// is reset or destroyed.
		/// <br /><br />
		/// Enabling decompressed mode requires Brotli support from the library.
		/// </summary>
		/// <param name="decompress"> JXL_TRUE to transparently decompress, JXL_FALSE to get
		/// boxes in raw mode.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if decompressed mode is set and Brotli is not
		/// available, <see cref="JxlDecoderStatus.Success" /> otherwise.</returns>
		public JxlDecoderStatus SetDecompressBoxes(bool decompress)
		{
			this.decompressBoxes = decompress;
			return (JxlDecoderStatus)decoderWrapper.SetDecompressBoxes(decompress);
		}

		/// <summary>
		/// Outputs the type of the current box, after a <see cref="JxlDecoderStatus.Box" /> event occured,
		/// as 4 characters without null termination character. In case of a compressed
		/// "brob" box, this will return "brob" if the decompressed argument is
		/// JXL_FALSE, or the underlying box type if the decompressed argument is
		/// JXL_TRUE.
		/// <br /><br />
		/// The following box types are currently described in ISO/IEC 18181-2:
		/// <br /> - "Exif": a box with EXIF metadata.  Starts with a 4-byte tiff header offset
		/// (big-endian uint32) that indicates the start of the actual EXIF data
		/// (which starts with a tiff header). Usually the offset will be zero and the
		/// EXIF data starts immediately after the offset field. The Exif orientation
		/// should be ignored by applications; the JPEG XL codestream orientation
		/// takes precedence and libjxl will by default apply the correct orientation
		/// automatically (see <see cref="SetKeepOrientation" />).
		/// <br /> - "xml ": a box with XML data, in particular XMP metadata.
		/// <br /> - "jumb": a JUMBF superbox (JPEG Universal Metadata Box Format, ISO/IEC
		/// 19566-5).
		/// <br /> - "JXL ": mandatory signature box, must come first, 12 bytes long including
		/// the box header
		/// <br /> - "ftyp": a second mandatory signature box, must come second, 20 bytes long
		/// including the box header
		/// <br /> - "jxll": a JXL level box. This indicates if the codestream is level 5 or
		/// level 10 compatible. If not present, it is level 5. Level 10 allows more
		/// features such as very high image resolution and bit-depths above 16 bits
		/// per channel. Added automatically by the encoder when
		/// JxlEncoderSetCodestreamLevel is used
		/// <br /> - "jxlc": a box with the image codestream, in case the codestream is not
		/// split across multiple boxes. The codestream contains the JPEG XL image
		/// itself, including the basic info such as image dimensions, ICC color
		/// profile, and all the pixel data of all the image frames.
		/// <br /> - "jxlp": a codestream box in case it is split across multiple boxes.
		/// The contents are the same as in case of a jxlc box, when concatenated.
		/// <br /> - "brob": a Brotli-compressed box, which otherwise represents an existing
		/// type of box such as Exif or "xml ". When <see cref="SetDecompressBoxes" />
		/// is set to JXL_TRUE, these boxes will be transparently decompressed by the
		/// decoder.
		/// <br /> - "jxli": frame index box, can list the keyframes in case of a JPEG XL
		/// animation allowing the decoder to jump to individual frames more
		/// efficiently.
		/// <br /> - "jbrd": JPEG reconstruction box, contains the information required to
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
		/// <param name="boxType"> buffer to copy the type into</param>
		/// <param name="decompressed"> which box type to get: JXL_FALSE to get the raw box type,
		/// which can be "brob", JXL_TRUE, get the underlying box type.</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if the value is available, <see cref="JxlDecoderStatus.Error" /> if
		/// not, for example the JXL file does not use the container format.</returns>
		public JxlDecoderStatus GetBoxType(out string boxType, bool decompressed)
		{
			return (JxlDecoderStatus)decoderWrapper.GetBoxType(out boxType, decompressed);
		}

		/// <summary>
		/// Returns the size of a box as it appears in the container file, after the 
		/// <see cref="JxlDecoderStatus.Box" /> event. For a non-compressed box, this is the size of the
		/// contents, excluding the 4 bytes indicating the box type. For a compressed
		/// "brob" box, this is the size of the compressed box contents plus the
		/// additional 4 byte indicating the underlying box type, but excluding the 4
		/// bytes indicating "brob". This function gives the size of the data that will
		/// be written in the output buffer when getting boxes in the default raw
		/// compressed mode. When <see cref="SetDecompressBoxes" /> is enabled, the
		/// return value of function does not change, and the decompressed size is not
		/// known before it has already been decompressed and output.
		/// </summary>
		/// <param name="size"> raw size of the box in bytes</param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Error" /> if no box size is available, <see cref="JxlDecoderStatus.Success" />
		/// otherwise.</returns>
		public JxlDecoderStatus GetBoxSizeRaw(out ulong size)
		{
			return (JxlDecoderStatus)decoderWrapper.GetBoxSizeRaw(out size);
		}

		public JxlDecoderStatus GetBox(out byte[] box, out string boxType)
		{
			throw new NotImplementedException();
			//box = null;
			//boxType = "";
			//ulong boxSize;
			//JxlDecoderStatus status;
			//status = (JxlDecoderStatus)decoderWrapper.SetDecompressBoxes(this.decompressBoxes);
			//if (status != JxlDecoderStatus.Success)
			//{
			//	return status;
			//}
			//status = (JxlDecoderStatus)decoderWrapper.GetBoxSizeRaw(out boxSize);
			//if (status != JxlDecoderStatus.Success)
			//{
			//	return status;
			//}
			//status = (JxlDecoderStatus)decoderWrapper.GetBoxType(out boxType, this.decompressBoxes);
			//if (status != JxlDecoderStatus.Success)
			//{
			//	return status;
			//}
			//status = (JxlDecoderStatus)decoderWrapper.SetBoxBuffer

		}

		/// <summary>
		/// Configures at which progressive steps in frame decoding these 
		/// <see cref="JxlDecoderStatus.FrameProgression" /> event occurs. The default value for the level
		/// of detail if this function is never called is `kDC`.
		/// </summary>
		/// <param name="detail"> at which level of detail to trigger 
		/// <see cref="JxlDecoderStatus.FrameProgression" /></param>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> on success, <see cref="JxlDecoderStatus.Error" /> on error, such as
		/// an invalid value for the progressive detail.</returns>
		public JxlDecoderStatus SetProgressiveDetail(JxlProgressiveDetail detail)
		{
			return (JxlDecoderStatus)decoderWrapper.SetProgressiveDetail((UnsafeNativeJxl.JxlProgressiveDetail)detail);
		}

		/// <summary>
		/// Returns the intended downsampling ratio for the progressive frame produced
		/// by <see cref="FlushImage" /> after the latest <see cref="JxlDecoderStatus.FrameProgression" />
		/// event.
		/// </summary>
		/// <returns> The intended downsampling ratio, can be 1, 2, 4 or 8.</returns>
		public int GetIntendedDownsamplingRatio()
		{
			return (int)decoderWrapper.GetIntendedDownsamplingRatio();
		}

		/// <summary>
		/// Outputs progressive step towards the decoded image so far when only partial
		/// input was received. If the flush was successful, the buffer set with 
		/// <see cref="SetImageOutBuffer" /> will contain partial image data.
		/// <br /><br />
		/// Can be called when <see cref="ProcessInput" /> returns 
		/// <see cref="JxlDecoderStatus.NeedMoreInput" />, after the <see cref="JxlDecoderStatus.Frame" /> event already occurred
		/// and before the <see cref="JxlDecoderStatus.FullImage" /> event occurred for a frame.
		/// </summary>
		/// <returns>
		///     <see cref="JxlDecoderStatus.Success" /> if image data was flushed to the output buffer,
		/// or <see cref="JxlDecoderStatus.Error" /> when no flush was done, e.g. if not enough image
		/// data was available yet even for flush, or no output buffer was set yet.
		/// This error is not fatal, it only indicates no flushed image is available
		/// right now. Regular decoding can still be performed.</returns>
		public JxlDecoderStatus FlushImage()
		{
			return (JxlDecoderStatus)decoderWrapper.FlushImage();
		}

	}

}
