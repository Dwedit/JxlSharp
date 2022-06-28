using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

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
		/// Wrapper class for JxlDecoder struct
		/// </summary>
		internal class JxlDecoderWrapper : IDisposable
		{
			JxlDecoder* dec;
			void* parallelRunner;

			byte[] input;
			byte* pInput;
			GCHandle inputGcHandle;

			byte[] jpegOutput;
			byte* pJpegOutput;
			GCHandle jpegOutputGcHandle;

			byte[] boxBuffer;
			byte* pBoxBuffer;
			GCHandle boxBufferGcHandle;

			public JxlDecoderWrapper()
			{
				dec = JxlDecoderCreate(null);
				parallelRunner = JxlThreadParallelRunnerCreate(null, (size_t)Environment.ProcessorCount);
				JxlDecoderSetParallelRunner(dec, JxlThreadParallelRunner, parallelRunner);
			}
			~JxlDecoderWrapper()
			{
				Dispose();
			}
			/// <summary>
			/// Disposes the Decoder Wrapper object
			/// </summary>
			public void Dispose()
			{
				if (dec != null)
				{
					ReleaseInput();
					ReleaseJPEGBuffer();
					ReleaseBoxBuffer();
					JxlDecoderDestroy(dec);
					dec = null;
					GC.SuppressFinalize(this);
				}
				if (parallelRunner != null)
				{
					JxlThreadParallelRunnerDestroy(parallelRunner);
					parallelRunner = null;
				}
			}
			[DebuggerStepThrough()]
			private void CheckIfDisposed()
			{
				if (dec == null) throw new ObjectDisposedException(nameof(dec));
			}

			/// <summary>
			/// Re-initializes a JxlDecoder instance, so it can be re-used for decoding
			/// another image. All state and settings are reset as if the object was
			/// newly created with JxlDecoderCreate, but the memory manager is kept.
			/// </summary>
			public void Reset()
			{
				CheckIfDisposed();
				ReleaseInput();
				ReleaseJPEGBuffer();
				ReleaseBoxBuffer();
				JxlDecoderReset(dec);
				JxlDecoderSetParallelRunner(dec, JxlThreadParallelRunner, parallelRunner);
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
				CheckIfDisposed();
				JxlDecoderRewind(dec);
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
				CheckIfDisposed();
				JxlDecoderSkipFrames(dec, (size_t)amount);
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
				CheckIfDisposed();
				return JxlDecoderDefaultPixelFormat(dec, out format);
			}
			/// <summary>
			/// Set the parallel runner for multithreading. May only be set before starting
			/// decoding.
			/// </summary>
			/// <param name="parallel_runner"> function pointer to runner for multithreading. It may
			/// be NULL to use the default, single-threaded, runner. A multithreaded
			/// runner should be set to reach fast performance.</param>
			/// <param name="parallel_runner_opaque"> opaque pointer for parallel_runner.</param>
			/// <returns> JXL_DEC_SUCCESS if the runner was set, JXL_DEC_ERROR
			/// otherwise (the previous runner remains set).</returns>
			private JxlDecoderStatus SetParallelRunner(JxlParallelRunner parallel_runner, void* parallel_runner_opaque)
			{
				CheckIfDisposed();
				return JxlDecoderSetParallelRunner(dec, parallel_runner, parallel_runner_opaque);
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
				CheckIfDisposed();
				return (int)JxlDecoderSizeHintBasicInfo(dec);
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
			/// <param name="events_wanted"> bitfield of desired events.</param>
			/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_ERROR otherwise.</returns>
			public JxlDecoderStatus SubscribeEvents(int events_wanted)
			{
				CheckIfDisposed();
				return JxlDecoderSubscribeEvents(dec, events_wanted);
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
			/// <param name="keep_orientation"> JXL_TRUE to enable, JXL_FALSE to disable.</param>
			/// <returns> JXL_DEC_SUCCESS if no error, JXL_DEC_ERROR otherwise.</returns>
			public JxlDecoderStatus SetKeepOrientation(bool keep_orientation)
			{
				CheckIfDisposed();
				return JxlDecoderSetKeepOrientation(dec, Convert.ToInt32(keep_orientation));
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
				CheckIfDisposed();
				return JxlDecoderProcessInput(dec);
			}
			/// <summary>
			/// Sets input data for JxlDecoderProcessInput. The data is owned by the caller
			/// and may be used by the decoder until JxlDecoderReleaseInput is called or
			/// the decoder is destroyed or reset so must be kept alive until then.
			/// </summary>
			/// <param name="data"> pointer to next bytes to read from</param>
			/// <param name="size"> amount of bytes available starting from data</param>
			/// <returns> JXL_DEC_ERROR if input was already set without releasing,
			/// JXL_DEC_SUCCESS otherwise.</returns>
			public JxlDecoderStatus SetInput([In] uint8_t* data, int size)
			{
				CheckIfDisposed();
				if (this.pInput != null)
				{
					return JxlDecoderStatus.JXL_DEC_ERROR;
				}
				this.pInput = data;
				return JxlDecoderSetInput(dec, data, (size_t)size);
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
				CheckIfDisposed();
				if (this.input == data) return JxlDecoderStatus.JXL_DEC_SUCCESS;
				
				ReleaseInput();
				this.input = data;
				this.inputGcHandle = GCHandle.Alloc(this.input, GCHandleType.Pinned);
				this.pInput = (byte*)this.inputGcHandle.AddrOfPinnedObject();
				return JxlDecoderSetInput(dec, this.pInput, (size_t)this.input.Length);
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
				CheckIfDisposed();
				if (this.pInput == null)
				{
					return 0;
				}
				if (this.input != null)
				{
					this.input = null;
				}
				this.pInput = null;
				if (this.inputGcHandle.IsAllocated)
				{
					this.inputGcHandle.Free();
				}
				int result = (int)JxlDecoderReleaseInput(dec);
				return result;
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
				CheckIfDisposed();
				return JxlDecoderGetBasicInfo(dec, out info);
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
				CheckIfDisposed();
				return JxlDecoderGetExtraChannelInfo(dec, (size_t)index, out info);
			}

			//private static int strlen_s(byte* pBytes, int bufferSize)
			//{
			//	int i;
			//	for (i = 0; i < bufferSize; i++)
			//	{
			//		if (pBytes[i] == 0) break;
			//	}
			//	return i;
			//}

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
				CheckIfDisposed();
				JxlDecoderStatus status = JxlDecoderStatus.JXL_DEC_ERROR;
				name = "";
				JxlExtraChannelInfo info;
				status = GetExtraChannelInfo(index, out info);
				if (status == JxlDecoderStatus.JXL_DEC_SUCCESS)
				{
					int bufferSize = (int)info.name_length + 1;
					byte[] buffer = new byte[bufferSize];
					fixed (byte* pBuffer = buffer)
					{
						status = JxlDecoderGetExtraChannelName(dec, (size_t)index, pBuffer, (size_t)bufferSize);
						if (status == JxlDecoderStatus.JXL_DEC_SUCCESS)
						{
							name = Encoding.UTF8.GetString(buffer, 0, bufferSize - 1);
						}
					}
				}
				return status;
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
			/// <param name="color_encoding"> struct to copy the information into, or NULL to only
			/// check whether the information is available through the return value.</param>
			/// <returns> JXL_DEC_SUCCESS if the data is available and returned,
			/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
			/// the encoded structured color profile does not exist in the codestream.</returns>
			public JxlDecoderStatus GetColorAsEncodedProfile([In] JxlPixelFormat* format, JxlColorProfileTarget target, out JxlColorEncoding color_encoding)
			{
				CheckIfDisposed();
				return JxlDecoderGetColorAsEncodedProfile(dec, format, target, out color_encoding);
			}
			/// <summary>
			/// Outputs the size in bytes of the ICC profile returned by
			/// JxlDecoderGetColorAsICCProfile, if available, or indicates there is none
			/// available. In most cases, the image will have an ICC profile available, but
			/// if it does not, JxlDecoderGetColorAsEncodedProfile must be used instead.
			/// <see cref="JxlDecoderGetColorAsEncodedProfile"/> for more information. The ICC
			/// profile is either the exact ICC profile attached to the codestream metadata,
			/// or a close approximation generated from JPEG XL encoded structured data,
			/// depending of what is encoded in the codestream.
			/// </summary>
			/// <param name="format"> pixel format to output the data to. Only used for
			/// JXL_COLOR_PROFILE_TARGET_DATA, may be nullptr otherwise.</param>
			/// <param name="target"> whether to get the original color profile from the metadata
			/// or the color profile of the decoded pixels.</param>
			/// <param name="size"> variable to output the size into, or NULL to only check the
			/// return status.</param>
			/// <returns> JXL_DEC_SUCCESS if the ICC profile is available,
			/// JXL_DEC_NEED_MORE_INPUT if the decoder has not yet received enough
			/// input data to determine whether an ICC profile is available or what its
			/// size is, JXL_DEC_ERROR in case the ICC profile is not available and
			/// cannot be generated.</returns>
			public JxlDecoderStatus GetICCProfileSize([In] JxlPixelFormat* format, JxlColorProfileTarget target, out int size)
			{
				CheckIfDisposed();
				size_t _size;
				var result = JxlDecoderGetICCProfileSize(dec, format, target, &_size);
				size = (int)_size;
				return result;
			}
			/// <summary>
			/// Outputs ICC profile if available. The profile is only available if
			/// JxlDecoderGetICCProfileSize returns success. The output buffer must have
			/// at least as many bytes as given by JxlDecoderGetICCProfileSize.
			/// </summary>
			/// <param name="format"> pixel format to output the data to. Only used for
			/// JXL_COLOR_PROFILE_TARGET_DATA, may be nullptr otherwise.</param>
			/// <param name="target"> whether to get the original color profile from the metadata
			/// or the color profile of the decoded pixels.</param>
			/// <param name="icc_profile"> buffer to copy the ICC profile into</param>
			/// <returns> JXL_DEC_SUCCESS if the profile was successfully returned is
			/// available, JXL_DEC_NEED_MORE_INPUT if not yet available,
			/// JXL_DEC_ERROR if the profile doesn't exist or the output size is not
			/// large enough.</returns>
			public JxlDecoderStatus GetColorAsICCProfile([In] JxlPixelFormat* format, JxlColorProfileTarget target, out byte[] icc_profile)
			{
				CheckIfDisposed();
				var status = GetICCProfileSize(format, target, out int size);
				if (status != JxlDecoderStatus.JXL_DEC_SUCCESS)
				{
					icc_profile = null;
					return status;
				}
				icc_profile = new byte[size];
				fixed (byte* pIccProfile = icc_profile)
				{
					return JxlDecoderGetColorAsICCProfile(dec, format, target, pIccProfile, (size_t)size);
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
			/// <param name="color_encoding"> the default color encoding to set</param>
			/// <returns> JXL_DEC_SUCCESS if the preference was set successfully, JXL_DEC_ERROR
			/// otherwise.</returns>
			public JxlDecoderStatus SetPreferredColorProfile([In] JxlColorEncoding* color_encoding)
			{
				CheckIfDisposed();
				return JxlDecoderSetPreferredColorProfile(dec, color_encoding);
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
			public JxlDecoderStatus GetPreviewOutBufferSize([In] JxlPixelFormat* format, out int size)
			{
				CheckIfDisposed();
				size_t _size;
				var result = JxlDecoderPreviewOutBufferSize(dec, format, &_size);
				size = (int)_size;
				return result;
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
			public JxlDecoderStatus SetPreviewOutBuffer([In] JxlPixelFormat* format, void* buffer, int size)
			{
				CheckIfDisposed();
				return JxlDecoderSetPreviewOutBuffer(dec, format, buffer, (size_t)size);
			}
			///// <summary>
			///// Outputs the information from the frame, such as duration when have_animation.
			///// This function can be called when JXL_DEC_FRAME occurred for the current
			///// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
			///// </summary>
			///// <param name="header"> struct to copy the information into, or NULL to only check
			///// whether the information is available through the return value.</param>
			///// <returns> JXL_DEC_SUCCESS if the value is available,
			///// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
			///// of other error conditions.</returns>
			//public JxlDecoderStatus GetFrameHeader(out JxlFrameHeader header)
			//{
			//	CheckIfDisposed();
			//	return JxlDecoderGetFrameHeader(dec, out header);
			//}

			/// <summary>
			/// Outputs the information from the frame, such as duration when have_animation.
			/// This function can be called when JXL_DEC_FRAME occurred for the current
			/// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.  This also includes the
			/// name of the frame.
			/// </summary>
			/// <param name="header"> struct to copy the information into, or NULL to only check
			/// whether the information is available through the return value.</param>
			/// <param name="name">string to copy the frame name into</param>
			/// <returns> JXL_DEC_SUCCESS if the value is available,
			/// JXL_DEC_NEED_MORE_INPUT if not yet available, JXL_DEC_ERROR in case
			/// of other error conditions.</returns>
			public JxlDecoderStatus GetFrameHeaderAndName(out JxlFrameHeader header, out string name)
			{
				CheckIfDisposed();
				name = "";
				var status = JxlDecoderGetFrameHeader(dec, out header);
				if (status == JxlDecoderStatus.JXL_DEC_SUCCESS && header.name_length > 0)
				{
					int bufferSize = (int)header.name_length + 1;
					byte[] buffer = new byte[bufferSize];
					fixed (byte* pBuffer = buffer)
					{
						status = JxlDecoderGetFrameName(dec, pBuffer, (size_t)bufferSize);
						if (status == JxlDecoderStatus.JXL_DEC_SUCCESS)
						{
							name = Encoding.UTF8.GetString(buffer, 0, bufferSize - 1);
						}
					}
				}
				return status;
			}
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
			//	CheckIfDisposed();
			//	name = "";
			//	JxlDecoderStatus status = JxlDecoderStatus.JXL_DEC_ERROR;
			//	JxlFrameHeader frameHeader;
			//	status = GetFrameHeader(out frameHeader);
			//	if (status == JxlDecoderStatus.JXL_DEC_SUCCESS)
			//	{
			//		int bufferSize = (int)frameHeader.name_length + 1;
			//		byte[] buffer = new byte[bufferSize];
			//		fixed (byte* pBuffer = buffer)
			//		{
			//			status = JxlDecoderGetFrameName(dec, pBuffer, (size_t)bufferSize);
			//			if (status == JxlDecoderStatus.JXL_DEC_SUCCESS)
			//			{
			//				name = Encoding.UTF8.GetString(pBuffer, bufferSize - 1);
			//			}
			//		}
			//	}
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
			public JxlDecoderStatus GetDCOutBufferSize([In] JxlPixelFormat* format, out int size)
			{
				CheckIfDisposed();
				size_t _size;
				var result = JxlDecoderDCOutBufferSize(dec, format, &_size);
				size = (int)_size;
				return result;
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
			public JxlDecoderStatus SetDCOutBuffer([In] JxlPixelFormat* format, void* buffer, int size)
			{
				CheckIfDisposed();
				return JxlDecoderSetDCOutBuffer(dec, format, buffer, (size_t)size);
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
			public JxlDecoderStatus GetImageOutBufferSize([In] JxlPixelFormat* format, out int size)
			{
				CheckIfDisposed();
				size_t _size;
				var result = JxlDecoderImageOutBufferSize(dec, format, &_size);
				size = (int)_size;
				return result;
			}
			/// <summary>
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
			public JxlDecoderStatus SetImageOutBuffer([In] JxlPixelFormat* format, void* buffer, int size)
			{
				CheckIfDisposed();
				return JxlDecoderSetImageOutBuffer(dec, format, buffer, (size_t)size);
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
			public JxlDecoderStatus SetImageOutCallback([In] JxlPixelFormat* format, JxlImageOutCallback callback, void* opaque)
			{
				CheckIfDisposed();
				return JxlDecoderSetImageOutCallback(dec, format, callback, opaque);
			}
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
			public JxlDecoderStatus GetExtraChannelBufferSize([In] JxlPixelFormat* format, out int size, int index)
			{
				CheckIfDisposed();
				size_t _size;
				var result = JxlDecoderExtraChannelBufferSize(dec, format, &_size, (uint32_t)index);
				size = (int)_size;
				return result;
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
			public JxlDecoderStatus SetExtraChannelBuffer([In] JxlPixelFormat* format, void* buffer, int size, uint32_t index)
			{
				CheckIfDisposed();
				return JxlDecoderSetExtraChannelBuffer(dec, format, buffer, (size_t)size, index);
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
			/// <param name="size"> amount of bytes available starting from data</param>
			/// <returns> JXL_DEC_ERROR if output buffer was already set and
			/// JxlDecoderReleaseJPEGBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
			public JxlDecoderStatus SetJPEGBuffer(uint8_t* data, int size)
			{
				CheckIfDisposed();
				if (this.pJpegOutput != null)
				{
					return JxlDecoderStatus.JXL_DEC_ERROR;
				}
				this.pJpegOutput = data;
				return JxlDecoderSetJPEGBuffer(dec, data, (size_t)size);
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
			/// <param name="outputPosition">output position within the array to start at</param>
			/// <returns> JXL_DEC_ERROR if output buffer was already set and
			/// JxlDecoderReleaseJPEGBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
			public JxlDecoderStatus SetJPEGBuffer(byte[] data, int outputPosition = 0)
			{
				CheckIfDisposed();
				ReleaseJPEGBuffer();
				if (outputPosition < 0 || outputPosition >= data.Length)
				{
					return JxlDecoderStatus.JXL_DEC_ERROR;
				}

				this.jpegOutput = data;
				this.jpegOutputGcHandle = GCHandle.Alloc(this.jpegOutput, GCHandleType.Pinned);
				this.pJpegOutput = (byte*)this.jpegOutputGcHandle.AddrOfPinnedObject();
				return JxlDecoderSetJPEGBuffer(dec, this.pJpegOutput + outputPosition, (size_t)(this.jpegOutput.Length - outputPosition));
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
				CheckIfDisposed();
				if (this.pJpegOutput == null)
				{
					return 0;
				}
				if (this.jpegOutput != null)
				{
					this.jpegOutput = null;
				}
				this.pJpegOutput = null;
				if (this.jpegOutputGcHandle.IsAllocated)
				{
					this.jpegOutputGcHandle.Free();
				}
				int result = (int)JxlDecoderReleaseJPEGBuffer(dec);
				return result;
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
			/// <param name="size"> amount of bytes available starting from data</param>
			/// <returns> JXL_DEC_ERROR if output buffer was already set and
			/// JxlDecoderReleaseBoxBuffer was not called on it, JXL_DEC_SUCCESS otherwise</returns>
			public JxlDecoderStatus SetBoxBuffer(uint8_t* data, int size)
			{
				CheckIfDisposed();
				return JxlDecoderSetBoxBuffer(dec, data, (size_t)size);
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
				CheckIfDisposed();
				ReleaseBoxBuffer();
				this.boxBuffer = data;
				this.boxBufferGcHandle = GCHandle.Alloc(this.boxBuffer, GCHandleType.Pinned);
				this.pBoxBuffer = (byte*)this.boxBufferGcHandle.AddrOfPinnedObject();
				return JxlDecoderSetBoxBuffer(dec, pBoxBuffer, (size_t)this.boxBuffer.Length);
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
				CheckIfDisposed();
				if (this.pBoxBuffer == null)
				{
					return 0;
				}
				if (this.boxBuffer != null)
				{
					this.boxBuffer = null;
				}
				this.pBoxBuffer = null;
				if (this.boxBufferGcHandle.IsAllocated)
				{
					this.boxBufferGcHandle.Free();
				}
				int result = (int)JxlDecoderReleaseBoxBuffer(dec);
				return result;
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
				CheckIfDisposed();
				return JxlDecoderSetDecompressBoxes(dec, (JXL_BOOL)Convert.ToInt32(decompress));
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
				CheckIfDisposed();
				byte[] buffer = new byte[4];
				fixed (byte* pBuffer = buffer)
				{
					var status = JxlDecoderGetBoxType(dec, pBuffer, (JXL_BOOL)Convert.ToInt32(decompressed));
					int len = 0;
					for (len = 0; len < buffer.Length; len++)
					{
						if (buffer[len] == 0) break;
					}
					boxType = Encoding.UTF8.GetString(buffer, 0, len);
					return status;
				}
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
			public JxlDecoderStatus GetBoxSizeRaw(out uint64_t size)
			{
				CheckIfDisposed();
				return JxlDecoderGetBoxSizeRaw(dec, out size);
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
				CheckIfDisposed();
				return JxlDecoderFlushImage(dec);
			}

			//public JxlDecoderStatus GetJpegBytes(out byte[] jpegData);
			//public JxlDecoderStatus ReadHeaders([In] byte[] inputData, JxlBasicInfo* basicInfo, bool* canTranscodeToJpeg);
			//public JxlDecoderStatus ConvertToJpeg([In] byte[] inputData, out byte[] jpegData);
			///// <summary>
			///// outputBytesPerPixel: 1 = Grayscale, 2= Grayscale + Alpha, 3 = RGB, 4 = RGBA
			///// </summary>
			//public JxlDecoderStatus DecodeToPixels([In] byte[] inputData, int outputWidth, int outputHeight, void* outputScan0, ptrdiff_t outputStride, int outputBytesPerPixel, bool outputIsBGR = false);

		}
    }
}
