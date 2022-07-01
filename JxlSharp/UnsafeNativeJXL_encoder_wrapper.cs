using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

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

	internal unsafe partial class UnsafeNativeJxl
	{
		/// <summary>
		/// Wrapper class for JxlEncoder struct
		/// </summary>
		internal class JxlEncoderWrapper : IDisposable
		{
			JxlEncoder* enc;
			void* parallelRunner;

			//byte* pOutputBuffer;
			//GCHandle outputBufferGcHandle;
			
			byte[] outputBuffer;
			int outputBufferInitialSize = 1024 * 1024;
			Stream outputStream;

			public JxlEncoderWrapper(Stream outputStream) : this (outputStream, 16 * 1024 * 1024)
			{

			}

			//public JxlEncoderWrapper() : this(new MemoryStream(), 1024 *1024)
			//{

			//}

			//byte[] input;
			//byte* pInput;
			//GCHandle inputGcHandle;

			//byte[] jpegOutput;
			//byte* pJpegOutput;
			//GCHandle jpegOutputGcHandle;

			//byte[] boxBuffer;
			//byte* pBoxBuffer;
			//GCHandle boxBufferGcHandle;

			public bool IsDisposed
			{
				get
				{
					return enc == null;
				}
			}

			//public byte[] OutputToArray()
			//{
			//	byte[] returnValue = new byte[outputPosition];
			//	Array.Copy(returnValue, output, outputPosition);
			//	return returnValue;
			//}

			//public void OutputToStream(Stream stream)
			//{
			//	stream.Write(output, 0, outputPosition);
			//}

			//public int Length
			//{
			//	get
			//	{
			//		return outputPosition;
			//	}
			//}

			//public int Capacity
			//{
			//	get
			//	{
			//		return output.Length;
			//	}
			//}

			public JxlEncoderWrapper(Stream outputStream, int outputBufferSize)
			{
				this.outputStream = outputStream;
				enc = UnsafeNativeJxl.JxlEncoderCreate(null);
				parallelRunner = UnsafeNativeJxl.JxlThreadParallelRunnerCreate(null, (size_t)Environment.ProcessorCount);
				UnsafeNativeJxl.JxlEncoderSetParallelRunner(enc, JxlThreadParallelRunner, parallelRunner);

			}

			public JxlEncoderWrapper(int initialCapacity)
			{
			}
			~JxlEncoderWrapper()
			{
				Dispose();
			}

			/// <summary>
			/// Disposes the Encoder Wrapper object
			/// </summary>
			public void Dispose()
			{
				if (enc != null)
				{
					//ReleaseInput();
					//ReleaseJPEGBuffer();
					//ReleaseBoxBuffer();
					UnsafeNativeJxl.JxlEncoderDestroy(enc);
					enc = null;
					GC.SuppressFinalize(this);
				}
				if (parallelRunner != null)
				{
					UnsafeNativeJxl.JxlThreadParallelRunnerDestroy(parallelRunner);
					parallelRunner = null;
				}
			}
			[DebuggerStepThrough()]
			private void CheckIfDisposed()
			{
				if (IsDisposed) throw new ObjectDisposedException(nameof(enc));
			}

			/// <summary>
			/// Re-initializes a JxlEncoder instance, so it can be re-used for encoding
			/// another image. All state and settings are reset as if the object was
			/// newly created with JxlEncoderCreate, but the memory manager is kept.
			/// </summary>
			public void Reset()
			{
				CheckIfDisposed();
				//ReleaseInput();
				//ReleaseJPEGBuffer();
				//ReleaseBoxBuffer();
				UnsafeNativeJxl.JxlEncoderReset(enc);
				UnsafeNativeJxl.JxlEncoderSetParallelRunner(enc, JxlThreadParallelRunner, parallelRunner);
			}

			/// <summary>
			/// Sets the color management system (CMS) that will be used for color conversion
			/// (if applicable) during encoding. May only be set before starting encoding. If
			/// left unset, the default CMS implementation will be used.
			/// </summary>
			/// <param name="cms"> structure representing a CMS implementation. See JxlCmsInterface
			/// for more details.</param>
			public void SetCms(JxlCmsInterface cms)
			{
				CheckIfDisposed();
				UnsafeNativeJxl.JxlEncoderSetCms(enc, cms);
			}

			///// <summary>
			///// Set the parallel runner for multithreading. May only be set before starting
			///// encoding.
			///// </summary>
			///// <param name="parallel_runner"> function pointer to runner for multithreading. It may
			///// be NULL to use the default, single-threaded, runner. A multithreaded
			///// runner should be set to reach fast performance.</param>
			///// <param name="parallel_runner_opaque"> opaque pointer for parallel_runner.</param>
			///// <returns> JXL_ENC_SUCCESS if the runner was set, JXL_ENC_ERROR
			///// otherwise (the previous runner remains set).</returns>
			//public JxlEncoderStatus SetParallelRunner(IntPtr parallel_runner, void* parallel_runner_opaque)
			//{ }

			/// <summary>
			/// Get the (last) error code in case JXL_ENC_ERROR was returned.
			/// </summary>
			/// <returns> the JxlEncoderError that caused the (last) JXL_ENC_ERROR to be
			/// returned.</returns>
			public JxlEncoderError GetError()
			{
				return UnsafeNativeJxl.JxlEncoderGetError(enc);
			}

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
			/// <param name="next_out"> pointer to next bytes to write to.</param>
			/// <param name="avail_out"> amount of bytes available starting from *next_out.</param>
			/// <returns> JXL_ENC_SUCCESS when encoding finished and all events handled.</returns>
			/// <returns> JXL_ENC_ERROR when encoding failed, e.g. invalid input.</returns>
			/// <returns> JXL_ENC_NEED_MORE_OUTPUT more output buffer is necessary.</returns>
			public JxlEncoderStatus ProcessOutput(byte** next_out, size_t* avail_out)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderProcessOutput(enc, next_out, avail_out);
			}

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
			/// <returns> JXL_ENC_SUCCESS when encoding finished and all events handled.</returns>
			/// <returns> JXL_ENC_ERROR when encoding failed, e.g. invalid input.</returns>
			/// <returns> JXL_ENC_NEED_MORE_OUTPUT more output buffer is necessary.</returns>
			public JxlEncoderStatus ProcessOutput()
			{
				CheckIfDisposed();
				if (this.outputBuffer == null)
				{
					this.outputBuffer = new byte[this.outputBufferInitialSize];
				}
				fixed (byte* pOutput = this.outputBuffer)
				{
					int outputPosition = 0;
				repeat:
					byte* currentOutput = pOutput + outputPosition;
					byte* currentOutputInitial = currentOutput;
					size_t bytesRemaining = (size_t)(outputBuffer.Length - outputPosition);
					var status = ProcessOutput(&currentOutput, &bytesRemaining);
					outputPosition = (int)(currentOutput - currentOutputInitial);
					if (status == JxlEncoderStatus.JXL_ENC_NEED_MORE_OUTPUT)
					{
						byte[] nextBuffer = new byte[this.outputBuffer.Length * 4];
						Array.Copy(this.outputBuffer, nextBuffer, outputPosition);
						this.outputBuffer = nextBuffer;
						goto repeat;
					}
					this.outputStream.Write(this.outputBuffer, 0, outputPosition);
					return status;
				}
			}

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
				CheckIfDisposed();
				byte[] boxTypeBytes = Encoding.UTF8.GetBytes(boxType);
				if (boxTypeBytes.Length != 4)
				{
					byte[] bytes2 = new byte[4];
					int i;
					for (i = 0; i < boxTypeBytes.Length && i < bytes2.Length; i++)
					{
						bytes2[i] = boxTypeBytes[i];
					}
					for (; i < bytes2.Length; i++)
					{
						bytes2[i] = (byte)' ';
					}
					boxTypeBytes = bytes2;
				}
				fixed (byte* pBoxType = boxTypeBytes)
				{
					fixed (byte* pContents = contents)
					{
						return UnsafeNativeJxl.JxlEncoderAddBox(enc, pBoxType, pContents, (size_t)contents.Length, Convert.ToInt32(compressBox));
					}
				}
			}

			/// <summary>
			/// Indicates the intention to add metadata boxes. This allows 
			/// <see cref="JxlEncoderAddBox(JxlEncoder*,byte*,byte*,UIntPtr,System.Int32)" /> to be used. When using this function, then it is required
			/// to use <see cref="JxlEncoderCloseBoxes(JxlEncoder*)" /> at the end.
			/// <br /><br />
			/// By default the encoder assumes no metadata boxes will be added.
			/// <br /><br />
			/// This setting can only be set at the beginning, before encoding starts.
			/// </summary>
			public JxlEncoderStatus UseBoxes()
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderUseBoxes(enc);
			}

			/// <summary>
			/// Declares that no further boxes will be added with <see cref="JxlEncoderAddBox(JxlEncoder*,byte*,byte*,UIntPtr,System.Int32)" />.
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
			public void CloseBoxes()
			{
				CheckIfDisposed();
				UnsafeNativeJxl.JxlEncoderCloseBoxes(enc);
			}

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
			public void CloseFrames()
			{
				CheckIfDisposed();
				UnsafeNativeJxl.JxlEncoderCloseFrames(enc);
			}

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
			public void CloseInput()
			{
				CheckIfDisposed();
				UnsafeNativeJxl.JxlEncoderCloseInput(enc);
			}

			/// <summary>
			/// Sets the original color encoding of the image encoded by this encoder. This
			/// is an alternative to JxlEncoderSetICCProfile and only one of these two must
			/// be used. This one sets the color encoding as a <see cref="JxlColorEncoding" />, while
			/// the other sets it as ICC binary data.
			/// Must be called after JxlEncoderSetBasicInfo.
			/// </summary>
			/// <param name="color"> color encoding. Object owned by the caller and its contents are
			/// copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
			/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
			public JxlEncoderStatus SetColorEncoding(ref JxlColorEncoding color)
			{
				CheckIfDisposed();
				fixed (JxlColorEncoding* pColor = &color)
				{
					return UnsafeNativeJxl.JxlEncoderSetColorEncoding(enc, pColor);
				}
			}

			/// <summary>
			/// Sets the original color encoding of the image encoded by this encoder as an
			/// ICC color profile. This is an alternative to JxlEncoderSetColorEncoding and
			/// only one of these two must be used. This one sets the color encoding as ICC
			/// binary data, while the other defines it as a <see cref="JxlColorEncoding" />.
			/// Must be called after JxlEncoderSetBasicInfo.
			/// </summary>
			/// <param name="icc_profile"> bytes of the original ICC profile</param>
			/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
			/// JXL_ENC_NOT_SUPPORTED otherwise</returns>
			public JxlEncoderStatus SetICCProfile(byte[] icc_profile)
			{
				CheckIfDisposed();
				fixed (byte* pProfile = icc_profile)
				{
					return UnsafeNativeJxl.JxlEncoderSetICCProfile(enc, pProfile, (size_t)icc_profile.Length);
				}
			}

			/// <summary>
			/// Sets the global metadata of the image encoded by this encoder.
			/// <br /><br />
			/// If the JxlBasicInfo contains information of extra channels beyond an alpha
			/// channel, then <see cref="JxlEncoderSetExtraChannelInfo(JxlEncoder*,UIntPtr,JxlExtraChannelInfo*)" /> must be called between
			/// JxlEncoderSetBasicInfo and <see cref="JxlEncoderAddImageFrame(JxlEncoderFrameSettings*,JxlPixelFormat*,void*,UIntPtr)" />. In order to indicate
			/// extra channels, the value of `info.num_extra_channels` should be set to the
			/// number of extra channels, also counting the alpha channel if present.
			/// </summary>
			/// <param name="info"> global image metadata. Object owned by the caller and its
			/// contents are copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS if the operation was successful,
			/// JXL_ENC_ERROR or JXL_ENC_NOT_SUPPORTED otherwise</returns>
			public JxlEncoderStatus SetBasicInfo(ref JxlBasicInfo info)
			{
				CheckIfDisposed();
				fixed (JxlBasicInfo* pInfo = &info)
				{
					return UnsafeNativeJxl.JxlEncoderSetBasicInfo(enc, pInfo);
				}
			}

			/// <summary>
			/// Sets information for the extra channel at the given index. The index
			/// must be smaller than num_extra_channels in the associated JxlBasicInfo.
			/// </summary>
			/// <param name="index"> index of the extra channel to set.</param>
			/// <param name="info"> global extra channel metadata. Object owned by the caller and its
			/// contents are copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetExtraChannelInfo(int index, ref JxlExtraChannelInfo info)
			{
				CheckIfDisposed();
				fixed (JxlExtraChannelInfo* pInfo = &info)
				{
					return UnsafeNativeJxl.JxlEncoderSetExtraChannelInfo(enc, (size_t)index, pInfo);
				}
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
				CheckIfDisposed();
				int byteCount = Encoding.UTF8.GetByteCount(name);
				byte[] bytes = new byte[byteCount + 1];
				Encoding.UTF8.GetBytes(name, 0, name.Length, bytes, 0);
				fixed (byte* pBytes = bytes)
				{
					return UnsafeNativeJxl.JxlEncoderSetExtraChannelName(enc, (size_t)index, pBytes, (size_t)bytes.Length);
				}
			}

			/// <summary>
			/// Forces the encoder to use the box-based container format (BMFF) even
			/// when not necessary.
			/// <br /><br />
			/// When using <see cref="JxlEncoderUseBoxes(JxlEncoder*)" />, <see cref="JxlEncoderStoreJPEGMetadata(JxlEncoder*,System.Int32)" /> or 
			/// <see cref="JxlEncoderSetCodestreamLevel(JxlEncoder*,System.Int32)" /> with level 10, the encoder will automatically
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderUseContainer(enc, Convert.ToInt32(use_container));
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderStoreJPEGMetadata(enc, Convert.ToInt32(store_jpeg_metadata));
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
			/// <see cref="JxlBasicInfo" /> structure. Do note that some level 10 features, particularly
			/// those used by animated JPEG XL codestreams, might require level 10, even
			/// though the <see cref="JxlBasicInfo" /> only suggests level 5. In this case, the level
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderSetCodestreamLevel(enc, level);
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderGetRequiredCodestreamLevel(enc);
			}

			/// <summary>
			/// Create a new set of encoder options <br/>
			/// The returned object is tied to the encoder and it will be
			/// deallocated by the encoder when JxlEncoderDestroy() is called. For functions
			/// taking both a <see cref="JxlEncoder" /> and a <see cref="JxlEncoderFrameSettings" />, only
			/// JxlEncoderFrameSettings created with this function for the same encoder
			/// instance can be used.
			/// </summary>
			/// <returns> A wrapper object for the Frame Settings object</returns>
			public JxlEncoderFrameSettingsWrapper FrameSettingsCreate()
			{
				return new JxlEncoderFrameSettingsWrapper(this, this.FrameSettingsCreate(null));
			}

			/// <summary>
			/// Create a new set of encoder options, with all values initially copied from
			/// the <tt>source</tt> options, or set to default if <tt>source</tt> is NULL.
			/// <br /><br />
			/// The returned pointer is an opaque struct tied to the encoder and it will be
			/// deallocated by the encoder when JxlEncoderDestroy() is called. For functions
			/// taking both a <see cref="JxlEncoder" /> and a <see cref="JxlEncoderFrameSettings" />, only
			/// JxlEncoderFrameSettings created with this function for the same encoder
			/// instance can be used.
			/// </summary>
			/// <param name="source"> source options to copy initial values from, or NULL to get
			/// defaults initialized to defaults.</param>
			/// <returns> the opaque struct pointer identifying a new set of encoder options.</returns>
			public JxlEncoderFrameSettings* FrameSettingsCreate(JxlEncoderFrameSettings* source)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderFrameSettingsCreate(enc, source);
			}

			/// <summary>
			/// DEPRECATED: use JxlEncoderFrameSettingsCreate instead.
			/// </summary>
			[Obsolete]
			public JxlEncoderFrameSettings* JxlEncoderOptionsCreate(JxlEncoderFrameSettings* A_1)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderOptionsCreate(this.enc, A_1);
			}
		}

		/// <summary>
		/// Wrapper class for JxlEncoderFrameSettings struct
		/// </summary>
		internal class JxlEncoderFrameSettingsWrapper
		{
			WeakReference<JxlEncoderWrapper> parent;
			JxlEncoderFrameSettings* frame_settings;

			/// <summary>
			/// Gets the parent JxlEncoderWrapper object (or null if the object has been garbage collected)
			/// </summary>
			public JxlEncoderWrapper Parent
			{
				get
				{
					if (parent.TryGetTarget(out JxlEncoderWrapper value) && value != null)
					{
						return value;
					}
					return null;
				}
			}

			/// <summary>
			/// Creates a new Wrapper object  (called by JxlEncoderWrapper.CreateFrameSettings)
			/// </summary>
			/// <param name="parent">The JxlEncoderWrapper object</param>
			/// <param name="frameSettings">The pointer to the JxlEncoderFrameSettings pointer to wrap</param>
			internal JxlEncoderFrameSettingsWrapper(JxlEncoderWrapper parent, JxlEncoderFrameSettings* frameSettings)
			{
				this.parent = new WeakReference<JxlEncoderWrapper>(parent);
				this.frame_settings = frameSettings;
			}
			
			/// <summary>
			/// Copies this Frame Settings wrapper, so the same settings can be re-used to add another image
			/// </summary>
			/// <returns>A copy of the FrameSettings object</returns>
			public JxlEncoderFrameSettingsWrapper Clone()
			{
				var parent = this.Parent;
				if (parent == null) return null;
				return new JxlEncoderFrameSettingsWrapper(parent, parent.FrameSettingsCreate(this.frame_settings));
			}

			/// <summary>
			/// Throws an exception if the parent object is disposed
			/// </summary>
			[DebuggerStepThrough()]
			void CheckIfDisposed()
			{
				var parent = this.Parent;
				if (parent == null) throw new ObjectDisposedException(nameof(parent));
				if (Parent.IsDisposed)
				{
					throw new ObjectDisposedException(nameof(parent));
				}
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
			/// <see cref="JxlFrameHeader" /> for more information.
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
			/// <param name="frame_header"> frame header data to set. Object owned by the caller and
			/// does not need to be kept in memory, its information is copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetFrameHeader(ref JxlFrameHeader frame_header)
			{
				CheckIfDisposed();
				fixed (JxlFrameHeader* pFrameHeader = &frame_header)
				{
					return UnsafeNativeJxl.JxlEncoderSetFrameHeader(frame_settings, pFrameHeader);
				}
			}

			/// <summary>
			/// Sets blend info of an extra channel. The blend info of extra channels is set
			/// separately from that of the color channels, the color channels are set with
			/// <see cref="JxlEncoderSetFrameHeader(JxlEncoderFrameSettings*,JxlFrameHeader*)" />.
			/// </summary>
			/// <param name="index"> index of the extra channel to use.</param>
			/// <param name="blend_info"> blend info to set for the extra channel</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetExtraChannelBlendInfo(int index, ref JxlBlendInfo blend_info)
			{
				CheckIfDisposed();
				fixed (JxlBlendInfo* pBlendInfo = &blend_info)
				{
					return UnsafeNativeJxl.JxlEncoderSetExtraChannelBlendInfo(frame_settings, (size_t)index, pBlendInfo);
				}
			}

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
			/// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
			/// string (zero terminated). Owned by the caller, and copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetFrameName(byte* frame_name)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderSetFrameName(frame_settings, frame_name);
			}

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
			/// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
			/// string (zero terminated). Owned by the caller, and copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetFrameName(string frame_name)
			{
				int byteCount = Encoding.UTF8.GetByteCount(frame_name);
				byte[] bytes = new byte[byteCount + 1];
				Encoding.UTF8.GetBytes(frame_name, 0, frame_name.Length, bytes, 0);
				fixed (byte* pBytes = bytes)
				{
					return UnsafeNativeJxl.JxlEncoderSetFrameName(frame_settings, pBytes);
				}
			}

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
			/// <see cref="JxlEncoderStoreJPEGMetadata(JxlEncoder*,System.Int32)" /> and a single JPEG frame is added, it will be
			/// possible to losslessly reconstruct the JPEG codestream.
			/// <br /><br />
			/// If this is the last frame, <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> or 
			/// <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> must be called before the next
			/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call.
			/// </summary>
			/// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
			/// are copied internally.</param>
			/// <param name="size"> size of buffer in bytes.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus AddJPEGFrame(byte* buffer, int size)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderAddJPEGFrame(frame_settings, buffer, (size_t)size);
			}

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
			/// <see cref="JxlEncoderStoreJPEGMetadata(JxlEncoder*,System.Int32)" /> and a single JPEG frame is added, it will be
			/// possible to losslessly reconstruct the JPEG codestream.
			/// <br /><br />
			/// If this is the last frame, <see cref="JxlEncoderCloseInput(JxlEncoder*)" /> or 
			/// <see cref="JxlEncoderCloseFrames(JxlEncoder*)" /> must be called before the next
			/// <see cref="JxlEncoderProcessOutput(JxlEncoder*,byte**,UIntPtr*)" /> call.
			/// </summary>
			/// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
			/// are copied internally.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus AddJPEGFrame(byte[] buffer)
			{
				CheckIfDisposed();
				fixed (byte* pBuffer = buffer)
				{
					return UnsafeNativeJxl.JxlEncoderAddJPEGFrame(frame_settings, pBuffer, (size_t)buffer.Length);
				}
			}


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
			/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
			/// contents are copied internally.</param>
			/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
			/// and its contents are copied internally.</param>
			/// <param name="size"> size of buffer in bytes. This size should match what is implied
			/// by the frame dimensions and the pixel format.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus AddImageFrame(ref JxlPixelFormat pixel_format, void* buffer, int size)
			{
				CheckIfDisposed();
				fixed (JxlPixelFormat* pPixelFormat = &pixel_format)
				{
					return UnsafeNativeJxl.JxlEncoderAddImageFrame(frame_settings, pPixelFormat, buffer, (size_t)size);
				}
			}

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
			/// <param name="pixel_format"> format for pixels. Object owned by the caller and its
			/// contents are copied internally. The num_channels value is ignored, since the
			/// number of channels for an extra channel is always assumed to be one.</param>
			/// <param name="buffer"> buffer type to input the pixel data from. Owned by the caller
			/// and its contents are copied internally.</param>
			/// <param name="size"> size of buffer in bytes. This size should match what is implied
			/// by the frame dimensions and the pixel format.</param>
			/// <param name="index"> index of the extra channel to use.</param>
			/// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
			public JxlEncoderStatus SetExtraChannelBuffer(ref JxlPixelFormat pixel_format, void* buffer, int size, int index)
			{
				CheckIfDisposed();
				fixed (JxlPixelFormat* pPixelFormat = &pixel_format)
				{
					return UnsafeNativeJxl.JxlEncoderSetExtraChannelBuffer(frame_settings, pPixelFormat, buffer, (size_t)size, (uint)index);
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderFrameSettingsSetOption(frame_settings, option, value);
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
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderSetFrameLossless(frame_settings, Convert.ToInt32(lossless));
			}

			/// <summary>
			/// DEPRECATED: use JxlEncoderSetFrameLossless instead.
			/// </summary>
			[Obsolete]
			public JxlEncoderStatus OptionsSetLossless(bool lossless)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderOptionsSetLossless(frame_settings, Convert.ToInt32(lossless));
			}

			/// <summary />
			/// <param name="effort"> the effort value to set.</param>
			/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
			/// otherwise.
			/// <br /><br />
			/// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
			/// JXL_ENC_FRAME_SETTING_EFFORT, effort) instead.</returns>
			[Obsolete]
			public JxlEncoderStatus JxlEncoderOptionsSetEffort(int effort)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderOptionsSetEffort(frame_settings, effort);
			}

			/// <summary />
			/// <param name="tier"> the decoding speed tier to set.</param>
			/// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
			/// otherwise.
			/// <br /><br />
			/// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
			/// JXL_ENC_FRAME_SETTING_DECODING_SPEED, tier) instead.</returns>
			[Obsolete]
			public JxlEncoderStatus JxlEncoderOptionsSetDecodingSpeed(int tier)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderOptionsSetDecodingSpeed(frame_settings, tier);
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
			public JxlEncoderStatus SetFrameDistance(float distance)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderSetFrameDistance(frame_settings, distance);
			}

			/// <summary>
			/// DEPRECATED: use JxlEncoderSetFrameDistance instead.
			/// </summary>
			[Obsolete]
			public JxlEncoderStatus JxlEncoderOptionsSetDistance(float distance)
			{
				CheckIfDisposed();
				return UnsafeNativeJxl.JxlEncoderOptionsSetDistance(frame_settings, distance);
			}
		}
	}
}
