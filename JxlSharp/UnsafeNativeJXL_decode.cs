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
        /// Decoder library version.
        /// </summary>
        /// <returns> the decoder library version as an integer:
        /// MAJOR_VERSION * 1000000 + MINOR_VERSION * 1000 + PATCH_VERSION. For example,
        /// version 1.2.3 would return 1002003.</returns>
        [DllImport("libjxl.dll", CallingConvention= CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern uint32_t JxlDecoderVersion();

        /// <summary>
        /// The result of <see cref="JxlSignatureCheck"/>.
        /// </summary>
        internal enum JxlSignature
        {
            /// <summary>
            /// Not enough bytes were passed to determine if a valid signature was found.
            /// </summary>
            JXL_SIG_NOT_ENOUGH_BYTES = 0,

            /// <summary>
            /// No valid JPEG XL header was found. 
            /// </summary>
            JXL_SIG_INVALID = 1,

            /// <summary>
            /// A valid JPEG XL codestream signature was found, that is a JPEG XL image
            /// without container.
            /// </summary>
            JXL_SIG_CODESTREAM = 2,

            /// <summary>
            /// A valid container signature was found, that is a JPEG XL image embedded
            /// in a box format container.
            /// </summary>
            JXL_SIG_CONTAINER = 3,
        }

        /// <summary>
        /// JPEG XL signature identification.
        /// <br/><br/>
        /// Checks if the passed buffer contains a valid JPEG XL signature. The passed @p
        /// buf of size
        /// <tt>size</tt> doesn't need to be a full image, only the beginning of the file.
        /// </summary>
        /// <returns> a flag indicating if a JPEG XL signature was found and what type.
        /// - <see cref="JxlSignature.JXL_SIG_NOT_ENOUGH_BYTES"/> if not enough bytes were passed to
        /// determine if a valid signature is there.
        /// - <see cref="JxlSignature.JXL_SIG_INVALID"/> if no valid signature found for JPEG XL decoding.
        /// - <see cref="JxlSignature.JXL_SIG_CODESTREAM"/> if a valid JPEG XL codestream signature was
        /// found.
        /// - <see cref="JxlSignature.JXL_SIG_CONTAINER"/> if a valid JPEG XL container signature was found.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlSignature JxlSignatureCheck([In] uint8_t* buf, size_t len);

        /// <summary>
        /// Opaque structure that holds the JPEG XL decoder.
        /// <br/><br/>
        /// Allocated and initialized with <see cref="JxlDecoderCreate"/>().
        /// Cleaned up and deallocated with <see cref="JxlDecoderDestroy"/>().
        /// </summary>
        internal struct JxlDecoder
        {

        }

        /// <summary>
        /// Creates an instance of <see cref="JxlDecoder"/> and initializes it.
        /// <br/><br/>
        /// <tt>memory_manager</tt> will be used for all the library dynamic allocations made
        /// from this instance. The parameter may be NULL, in which case the default
        /// allocator will be used. See jxl/memory_manager.h for details.
        /// </summary>
        /// <param name="memory_manager"> custom allocator function. It may be NULL. The memory
        /// manager will be copied internally.</param>
        /// <returns> <tt>NULL</tt> if the instance can not be allocated or initialized</returns>
        /// <returns> pointer to initialized <see cref="JxlDecoder"/> otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoder* JxlDecoderCreate([In] JxlMemoryManager* memory_manager);

        /// <summary>
        /// Re-initializes a <see cref="JxlDecoder"/> instance, so it can be re-used for decoding
        /// another image. All state and settings are reset as if the object was
        /// newly created with <see cref="JxlDecoderCreate"/>, but the memory manager is kept.
        /// </summary>
        /// <param name="dec"> instance to be re-initialized.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlDecoderReset(JxlDecoder* dec);

        /// <summary>
        /// Deinitializes and frees <see cref="JxlDecoder"/> instance.
        /// </summary>
        /// <param name="dec"> instance to be cleaned up and deallocated.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlDecoderDestroy(JxlDecoder* dec);

        /// <summary>
        /// Return value for <see cref="JxlDecoderProcessInput"/>.
        /// The values from <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/> onwards are optional informative
        /// events that can be subscribed to, they are never returned if they
        /// have not been registered with <see cref="JxlDecoderSubscribeEvents"/>.
        /// </summary>
        [Flags]
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
            /// The decoder needs more input bytes to continue. Before the next <see cref="JxlDecoderProcessInput"/> call, more input data must be set, by calling <see cref="JxlDecoderReleaseInput"/> (if input was set previously) and then calling <see cref="JxlDecoderSetInput"/>. <see cref="JxlDecoderReleaseInput"/> returns how many bytes
            /// are not yet processed, before a next call to <see cref="JxlDecoderProcessInput"/>
            /// all unprocessed bytes must be provided again (the address need not match,
            /// but the contents must), and more bytes must be concatenated after the
            /// unprocessed bytes.
            /// </summary>
            JXL_DEC_NEED_MORE_INPUT = 2,

            /// <summary>
            /// The decoder is able to decode a preview image and requests setting a
            /// preview output buffer using <see cref="JxlDecoderSetPreviewOutBuffer"/>. This occurs
            /// if <see cref="JxlDecoderStatus.JXL_DEC_PREVIEW_IMAGE"/> is requested and it is possible to decode a
            /// preview image from the codestream and the preview out buffer was not yet
            /// set. There is maximum one preview image in a codestream.
            /// </summary>
            JXL_DEC_NEED_PREVIEW_OUT_BUFFER = 3,

            /// <summary>
            /// The decoder is able to decode a DC image and requests setting a DC output
            /// buffer using <see cref="JxlDecoderSetDCOutBuffer"/>. This occurs if <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE"/> is requested and it is possible to decode a DC image from
            /// the codestream and the DC out buffer was not yet set. This event re-occurs
            /// for new frames if there are multiple animation frames.
            /// @deprecated The DC feature in this form will be removed. For progressive
            /// rendering, <see cref="JxlDecoderFlushImage"/> should be used.
            /// </summary>
            JXL_DEC_NEED_DC_OUT_BUFFER = 4,

            /// <summary>
            /// The decoder requests an output buffer to store the full resolution image,
            /// which can be set with <see cref="JxlDecoderSetImageOutBuffer"/> or with <see cref="JxlDecoderSetImageOutCallback"/>. This event re-occurs for new frames if
            /// there are multiple animation frames and requires setting an output again.
            /// </summary>
            JXL_DEC_NEED_IMAGE_OUT_BUFFER = 5,

            /// <summary>
            /// The JPEG reconstruction buffer is too small for reconstructed JPEG
            /// codestream to fit. <see cref="JxlDecoderSetJPEGBuffer"/> must be called again to
            /// make room for remaining bytes. This event may occur multiple times
            /// after <see cref="JxlDecoderStatus.JXL_DEC_JPEG_RECONSTRUCTION"/>.
            /// </summary>
            JXL_DEC_JPEG_NEED_MORE_OUTPUT = 6,

            /// <summary>
            /// The box contents output buffer is too small. <see cref="JxlDecoderSetBoxBuffer"/>
            /// must be called again to make room for remaining bytes. This event may occur
            /// multiple times after <see cref="JxlDecoderStatus.JXL_DEC_BOX"/>.
            /// </summary>
            JXL_DEC_BOX_NEED_MORE_OUTPUT = 7,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": Basic information such as image dimensions and
            /// extra channels. This event occurs max once per image.
            /// </summary>
            JXL_DEC_BASIC_INFO = 0x40,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": User extensions of the codestream header. This
            /// event occurs max once per image and always later than <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/> and earlier than any pixel data.
            /// <br/><br/>
            /// @deprecated The decoder no longer returns this, the header extensions,
            /// if any, are available at the JXL_DEC_BASIC_INFO event.
            /// </summary>
            JXL_DEC_EXTENSIONS = 0x80,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": Color encoding or ICC profile from the
            /// codestream header. This event occurs max once per image and always later
            /// than <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/> and earlier than any pixel data.
            /// </summary>
            JXL_DEC_COLOR_ENCODING = 0x100,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": Preview image, a small frame, decoded. This
            /// event can only happen if the image has a preview frame encoded. This event
            /// occurs max once for the codestream and always later than <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING"/> and before <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/>.
            /// </summary>
            JXL_DEC_PREVIEW_IMAGE = 0x200,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": Beginning of a frame. <see cref="JxlDecoderGetFrameHeader"/> can be used at this point. A note on frames:
            /// a JPEG XL image can have internal frames that are not intended to be
            /// displayed (e.g. used for compositing a final frame), but this only returns
            /// displayed frames, unless <see cref="JxlDecoderSetCoalescing"/> was set to JXL_FALSE:
            /// in that case, the individual layers are returned, without blending. Note
            /// that even when coalescing is disabled, only frames of type kRegularFrame
            /// are returned; frames of type kReferenceOnly and kLfFrame are always for
            /// internal purposes only and cannot be accessed. A displayed frame either has
            /// an animation duration or is the only or last frame in the image. This event
            /// occurs max once per displayed frame, always later than <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING"/>, and always earlier than any pixel data. While
            /// JPEG XL supports encoding a single frame as the composition of multiple
            /// internal sub-frames also called frames, this event is not indicated for the
            /// internal frames.
            /// </summary>
            JXL_DEC_FRAME = 0x400,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": DC image, 8x8 sub-sampled frame, decoded. It is
            /// not guaranteed that the decoder will always return DC separately, but when
            /// it does it will do so before outputting the full frame. <see cref="JxlDecoderSetDCOutBuffer"/> must be used after getting the basic image
            /// information to be able to get the DC pixels, if not this return status only
            /// indicates we're past this point in the codestream. This event occurs max
            /// once per frame and always later than <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> and other header
            /// events and earlier than full resolution pixel data.
            /// <br/><br/>
            /// @deprecated The DC feature in this form will be removed. For progressive
            /// rendering, <see cref="JxlDecoderFlushImage"/> should be used.
            /// </summary>
            JXL_DEC_DC_IMAGE = 0x800,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": full frame (or layer, in case coalescing is
            /// disabled) is decoded. <see cref="JxlDecoderSetImageOutBuffer"/> must be used after
            /// getting the basic image information to be able to get the image pixels, if
            /// not this return status only indicates we're past this point in the
            /// codestream. This event occurs max once per frame and always later than <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE"/>.
            /// </summary>
            JXL_DEC_FULL_IMAGE = 0x1000,

            /// <summary>
            /// Informative event by <see cref="JxlDecoderProcessInput"/>
            /// "JxlDecoderProcessInput": JPEG reconstruction data decoded. <see cref="JxlDecoderSetJPEGBuffer"/> may be used to set a JPEG reconstruction buffer
            /// after getting the JPEG reconstruction data. If a JPEG reconstruction buffer
            /// is set a byte stream identical to the JPEG codestream used to encode the
            /// image will be written to the JPEG reconstruction buffer instead of pixels
            /// to the image out buffer. This event occurs max once per image and always
            /// before <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/>.
            /// </summary>
            JXL_DEC_JPEG_RECONSTRUCTION = 0x2000,

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
            /// The output buffer contains the full box data when the next <see cref="JxlDecoderStatus.JXL_DEC_BOX"/>
            /// event or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> occurs. <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> occurs for all
            /// boxes, including non-metadata boxes such as the signature box or codestream
            /// boxes. To check whether the box is a metadata type for respectively EXIF,
            /// XMP or JUMBF, use <see cref="JxlDecoderGetBoxType"/> and check for types "Exif",
            /// "xml " and "jumb" respectively.
            /// </summary>
            JXL_DEC_BOX = 0x4000,

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
            JXL_DEC_FRAME_PROGRESSION = 0x8000,
        }

        /// <summary>
        /// Rewinds decoder to the beginning. The same input must be given again from
        /// the beginning of the file and the decoder will emit events from the beginning
        /// again. When rewinding (as opposed to <see cref="JxlDecoderReset"/>), the decoder can
        /// keep state about the image, which it can use to skip to a requested frame
        /// more efficiently with <see cref="JxlDecoderSkipFrames"/>. Settings such as parallel
        /// runner or subscribed events are kept. After rewind, <see cref="JxlDecoderSubscribeEvents"/> can be used again, and it is feasible to leave out
        /// events that were already handled before, such as <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/>
        /// and <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING"/>, since they will provide the same information
        /// as before.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlDecoderRewind(JxlDecoder* dec);

        /// <summary>
        /// Makes the decoder skip the next `amount` frames. It still needs to process
        /// the input, but will not output the frame events. It can be more efficient
        /// when skipping frames, and even more so when using this after <see cref="JxlDecoderRewind"/>. If the decoder is already processing a frame (could
        /// have emitted <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> but not yet <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/>), it
        /// starts skipping from the next frame. If the amount is larger than the amount
        /// of frames remaining in the image, all remaining frames are skipped. Calling
        /// this function multiple times adds the amount to skip to the already existing
        /// amount.
        /// <br/><br/>
        /// A frame here is defined as a frame that without skipping emits events such
        /// as <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> and <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/>, frames that are internal
        /// to the file format but are not rendered as part of an animation, or are not
        /// the final still frame of a still image, are not counted.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="amount"> the amount of frames to skip</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlDecoderSkipFrames(JxlDecoder* dec, size_t amount);

        /// <summary>
        /// Skips processing the current frame. Can be called after frame processing
        /// already started, signaled by a <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER"/> event,
        /// but before the corrsponding <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/> event. The next signaled
        /// event will be another <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/>, or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if there
        /// are no more frames. If pixel data is required from the already processed part
        /// of the frame, <see cref="JxlDecoderFlushImage"/> must be called before this.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if there is a frame to skip, and <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if the function was not called during frame processing.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSkipCurrentFrame(JxlDecoder* dec);

        /// <summary>
        /// Get the default pixel format for this decoder.
        /// <br/><br/>
        /// Requires that the decoder can produce JxlBasicInfo.
        /// </summary>
        /// <param name="dec"> <see cref="JxlDecoder"/> to query when creating the recommended pixel
        /// format.</param>
        /// <param name="format"> JxlPixelFormat to populate with the recommended settings for
        /// the data loaded into this decoder.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if no error, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if the
        /// basic info isn't yet available, and <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderDefaultPixelFormat([In] JxlDecoder* dec, out JxlPixelFormat format);

        /// <summary>
        /// Set the parallel runner for multithreading. May only be set before starting
        /// decoding.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="parallel_runner"> function pointer to runner for multithreading. It may
        /// be NULL to use the default, single-threaded, runner. A multithreaded
        /// runner should be set to reach fast performance.</param>
        /// <param name="parallel_runner_opaque"> opaque pointer for parallel_runner.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the runner was set, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/>
        /// otherwise (the previous runner remains set).</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetParallelRunner(JxlDecoder* dec, JxlParallelRunner parallel_runner,
                                    void* parallel_runner_opaque);

        /// <summary>
        /// Returns a hint indicating how many more bytes the decoder is expected to
        /// need to make <see cref="JxlDecoderGetBasicInfo"/> available after the next <see cref="JxlDecoderProcessInput"/> call. This is a suggested large enough value for
        /// the amount of bytes to provide in the next <see cref="JxlDecoderSetInput"/> call, but
        /// it is not guaranteed to be an upper bound nor a lower bound. This number does
        /// not include bytes that have already been released from the input. Can be used
        /// before the first <see cref="JxlDecoderProcessInput"/> call, and is correct the first
        /// time in most cases. If not, <see cref="JxlDecoderSizeHintBasicInfo"/> can be called
        /// again to get an updated hint.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> the size hint in bytes if the basic info is not yet fully decoded.</returns>
        /// <returns> 0 when the basic info is already available.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlDecoderSizeHintBasicInfo([In] JxlDecoder* dec);

        /// <summary>
        /// Select for which informative events, i.e. <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/>, etc., the
        /// decoder should return with a status. It is not required to subscribe to any
        /// events, data can still be requested from the decoder as soon as it available.
        /// By default, the decoder is subscribed to no events (events_wanted == 0), and
        /// the decoder will then only return when it cannot continue because it needs
        /// more input data or more output buffer. This function may only be be called
        /// before using <see cref="JxlDecoderProcessInput"/>.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="events_wanted"> bitfield of desired events.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSubscribeEvents(JxlDecoder* dec,
                                                              int events_wanted);

        /// <summary>
        /// Enables or disables preserving of as-in-bitstream pixeldata
        /// orientation. Some images are encoded with an Orientation tag
        /// indicating that the decoder must perform a rotation and/or
        /// mirroring to the encoded image data.
        /// <br/><br/>
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
        /// <br/><br/>
        /// By default, this option is disabled, and the returned pixel data is
        /// re-oriented according to the image's Orientation setting.
        /// <br/><br/>
        /// This function must be called at the beginning, before decoding is performed.
        /// <br/><br/>
        /// <see cref="JxlBasicInfo"/> for the orientation field, and <see cref="JxlOrientation"/> for the
        /// possible values.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="skip_reorientation"> JXL_TRUE to enable, JXL_FALSE to disable.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetKeepOrientation(JxlDecoder* dec, JXL_BOOL skip_reorientation);

        /// <summary>
        /// Enables or disables rendering spot colors. By default, spot colors
        /// are rendered, which is OK for viewing the decoded image. If render_spotcolors
        /// is JXL_FALSE, then spot colors are not rendered, and have to be retrieved
        /// separately using <see cref="JxlDecoderSetExtraChannelBuffer"/>. This is useful for
        /// e.g. printing applications.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="render_spotcolors"> JXL_TRUE to enable (default), JXL_FALSE to disable.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetRenderSpotcolors(JxlDecoder* dec, JXL_BOOL render_spotcolors);

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
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if no error, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetCoalescing(JxlDecoder* dec,
                                                            JXL_BOOL coalescing);

        /// <summary>
        /// Decodes JPEG XL file using the available bytes. Requires input has been
        /// set with <see cref="JxlDecoderSetInput"/>. After <see cref="JxlDecoderProcessInput"/>, input
        /// can optionally be released with <see cref="JxlDecoderReleaseInput"/> and then set
        /// again to next bytes in the stream. <see cref="JxlDecoderReleaseInput"/> returns how
        /// many bytes are not yet processed, before a next call to <see cref="JxlDecoderProcessInput"/> all unprocessed bytes must be provided again (the
        /// address need not match, but the contents must), and more bytes may be
        /// concatenated after the unprocessed bytes.
        /// <br/><br/>
        /// The returned status indicates whether the decoder needs more input bytes, or
        /// more output buffer for a certain type of output data. No matter what the
        /// returned status is (other than <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/>), new information, such
        /// as <see cref="JxlDecoderGetBasicInfo"/>, may have become available after this call.
        /// When the return value is not <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> or <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/>, the
        /// decoding requires more <see cref="JxlDecoderProcessInput"/> calls to continue.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> when decoding finished and all events handled.
        /// If you still have more unprocessed input data anyway, then you can still
        /// continue by using <see cref="JxlDecoderSetInput"/> and calling <see cref="JxlDecoderProcessInput"/> again, similar to handling <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/>. <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> can occur instead of <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> when, for example, the input data ended right at
        /// the boundary of a box of the container format, all essential codestream
        /// boxes were already decoded, but extra metadata boxes are still present in
        /// the next data. <see cref="JxlDecoderProcessInput"/> cannot return success if all
        /// codestream boxes have not been seen yet.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> when decoding failed, e.g. invalid codestream.
        /// TODO(lode): document the input data mechanism</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> when more input data is necessary.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_BASIC_INFO"/> when basic info such as image dimensions is
        /// available and this informative event is subscribed to.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING"/> when color profile information is
        /// available and this informative event is subscribed to.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_PREVIEW_IMAGE"/> when preview pixel information is
        /// available and output in the preview buffer.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_DC_IMAGE"/> when DC pixel information (8x8 downscaled
        /// version of the image) is available and output is in the DC buffer.</returns>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/> when all pixel information at highest detail
        /// is available and has been output in the pixel buffer.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderProcessInput(JxlDecoder* dec);

        /// <summary>
        /// Sets input data for <see cref="JxlDecoderProcessInput"/>. The data is owned by the
        /// caller and may be used by the decoder until <see cref="JxlDecoderReleaseInput"/> is
        /// called or the decoder is destroyed or reset so must be kept alive until then.
        /// Cannot be called if <see cref="JxlDecoderSetInput"/> was already called and <see cref="JxlDecoderReleaseInput"/> was not yet called, and cannot be called after <see cref="JxlDecoderCloseInput"/> indicating the end of input was called.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="data"> pointer to next bytes to read from</param>
        /// <param name="size"> amount of bytes available starting from data</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if input was already set without releasing or <see cref="JxlDecoderCloseInput"/> was already called, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetInput(JxlDecoder* dec,
                                                       [In] uint8_t* data,
                                                       size_t size);

        /// <summary>
        /// Releases input which was provided with <see cref="JxlDecoderSetInput"/>. Between <see cref="JxlDecoderProcessInput"/> and <see cref="JxlDecoderReleaseInput"/>, the user may not
        /// alter the data in the buffer. Calling <see cref="JxlDecoderReleaseInput"/> is required
        /// whenever any input is already set and new input needs to be added with <see cref="JxlDecoderSetInput"/>, but is not required before <see cref="JxlDecoderDestroy"/> or <see cref="JxlDecoderReset"/>. Calling <see cref="JxlDecoderReleaseInput"/> when no input is set is
        /// not an error and returns 0.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> The amount of bytes the decoder has not yet processed that are still
        /// remaining in the data set by <see cref="JxlDecoderSetInput"/>, or 0 if no input is
        /// set or <see cref="JxlDecoderReleaseInput"/> was already called. For a next call
        /// to <see cref="JxlDecoderProcessInput"/>, the buffer must start with these
        /// unprocessed bytes. This value doesn't provide information about how many
        /// bytes the decoder truly processed internally or how large the original
        /// JPEG XL codestream or file are.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlDecoderReleaseInput(JxlDecoder* dec);

        /// <summary>
        /// Marks the input as finished, indicates that no more <see cref="JxlDecoderSetInput"/>
        /// will be called. This function allows the decoder to determine correctly if it
        /// should return success, need more input or error in certain cases. For
        /// backwards compatibility with a previous version of the API, using this
        /// function is optional when not using the <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> event (the decoder
        /// is able to determine the end of the image frames without marking the end),
        /// but using this function is required when using <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> for getting
        /// metadata box contents. This function does not replace <see cref="JxlDecoderReleaseInput"/>, that function should still be called if its return
        /// value is needed.
        /// <br/><br/>
        /// <see cref="JxlDecoderCloseInput"/> should be called as soon as all known input bytes
        /// are set (e.g. at the beginning when not streaming but setting all input
        /// at once), before the final <see cref="JxlDecoderProcessInput"/> calls.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlDecoderCloseInput(JxlDecoder* dec);

        /// <summary>
        /// Outputs the basic image information, such as image dimensions, bit depth and
        /// all other JxlBasicInfo fields, if available.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="info"> struct to copy the information into, or NULL to only check
        /// whether the information is available through the return value.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/>
        /// in case of other error conditions.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetBasicInfo([In] JxlDecoder* dec,
                                                           out JxlBasicInfo info);

        /// <summary>
        /// Outputs information for extra channel at the given index. The index must be
        /// smaller than num_extra_channels in the associated JxlBasicInfo.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="index"> index of the extra channel to query.</param>
        /// <param name="info"> struct to copy the information into, or NULL to only check
        /// whether the information is available through the return value.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/>
        /// in case of other error conditions.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetExtraChannelInfo(
            [In] JxlDecoder* dec, size_t index, out JxlExtraChannelInfo info);

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
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/>
        /// in case of other error conditions.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetExtraChannelName([In] JxlDecoder* dec,
                                                                  size_t index,
                                                                  byte* name,
                                                                  size_t size);

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
            JXL_COLOR_PROFILE_TARGET_ORIGINAL = 0,

            /// <summary>
            /// Get the color profile of the pixel data the decoder outputs. 
            /// </summary>
            JXL_COLOR_PROFILE_TARGET_DATA = 1,
        }

        /// <summary>
        /// Outputs the color profile as JPEG XL encoded structured data, if available.
        /// This is an alternative to an ICC Profile, which can represent a more limited
        /// amount of color spaces, but represents them exactly through enum values.
        /// <br/><br/>
        /// It is often possible to use <see cref="JxlDecoderGetColorAsICCProfile"/> as an
        /// alternative anyway. The following scenarios are possible:
        /// - The JPEG XL image has an attached ICC Profile, in that case, the encoded
        /// structured data is not available, this function will return an error
        /// status. <see cref="JxlDecoderGetColorAsICCProfile"/> should be called instead.
        /// - The JPEG XL image has an encoded structured color profile, and it
        /// represents an RGB or grayscale color space. This function will return it.
        /// You can still use <see cref="JxlDecoderGetColorAsICCProfile"/> as well as an
        /// alternative if desired, though depending on which RGB color space is
        /// represented, the ICC profile may be a close approximation. It is also not
        /// always feasible to deduce from an ICC profile which named color space it
        /// exactly represents, if any, as it can represent any arbitrary space.
        /// - The JPEG XL image has an encoded structured color profile, and it
        /// indicates an unknown or xyb color space. In that case, <see cref="JxlDecoderGetColorAsICCProfile"/> is not available.
        /// <br/><br/>
        /// When rendering an image on a system that supports ICC profiles, <see cref="JxlDecoderGetColorAsICCProfile"/> should be used first. When rendering
        /// for a specific color space, possibly indicated in the JPEG XL
        /// image, <see cref="JxlDecoderGetColorAsEncodedProfile"/> should be used first.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> pixel format to output the data to. Only used for <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/>, may be nullptr otherwise.</param>
        /// <param name="target"> whether to get the original color profile from the metadata
        /// or the color profile of the decoded pixels.</param>
        /// <param name="color_encoding"> struct to copy the information into, or NULL to only
        /// check whether the information is available through the return value.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the data is available and returned, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> in
        /// case the encoded structured color profile does not exist in the
        /// codestream.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetColorAsEncodedProfile(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format,
            JxlColorProfileTarget target, out JxlColorEncoding color_encoding);

        /// <summary>
        /// Outputs the size in bytes of the ICC profile returned by <see cref="JxlDecoderGetColorAsICCProfile"/>, if available, or indicates there is none
        /// available. In most cases, the image will have an ICC profile available, but
        /// if it does not, <see cref="JxlDecoderGetColorAsEncodedProfile"/> must be used instead.
        /// <br/><br/>
        /// <see cref="JxlDecoderGetColorAsEncodedProfile"/> for more information. The ICC
        /// profile is either the exact ICC profile attached to the codestream metadata,
        /// or a close approximation generated from JPEG XL encoded structured data,
        /// depending of what is encoded in the codestream.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> pixel format to output the data to. Only used for <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/>, may be NULL otherwise.</param>
        /// <param name="target"> whether to get the original color profile from the metadata
        /// or the color profile of the decoded pixels.</param>
        /// <param name="size"> variable to output the size into, or NULL to only check the
        /// return status.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the ICC profile is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if the decoder has not yet received enough
        /// input data to determine whether an ICC profile is available or what its
        /// size is, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> in case the ICC profile is not available and
        /// cannot be generated.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderGetICCProfileSize([In] JxlDecoder* dec, [In] JxlPixelFormat* format,
                                    JxlColorProfileTarget target, size_t* size);

        /// <summary>
        /// Outputs ICC profile if available. The profile is only available if <see cref="JxlDecoderGetICCProfileSize"/> returns success. The output buffer must have
        /// at least as many bytes as given by <see cref="JxlDecoderGetICCProfileSize"/>.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> pixel format to output the data to. Only used for <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/>, may be NULL otherwise.</param>
        /// <param name="target"> whether to get the original color profile from the metadata
        /// or the color profile of the decoded pixels.</param>
        /// <param name="icc_profile"> buffer to copy the ICC profile into</param>
        /// <param name="size"> size of the icc_profile buffer in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the profile was successfully returned is
        /// available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if the profile doesn't exist or the output size is not
        /// large enough.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetColorAsICCProfile(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format,
            JxlColorProfileTarget target, uint8_t* icc_profile, size_t size);

        /// <summary>
        /// Sets the color profile to use for <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/> for the
        /// special case when the decoder has a choice. This only has effect for a JXL
        /// image where uses_original_profile is false, and the original color profile is
        /// encoded as an ICC color profile rather than a JxlColorEncoding with known
        /// enum values. In most other cases (uses uses_original_profile is true, or the
        /// color profile is already given as a JxlColorEncoding), this setting is
        /// ignored and the decoder uses a profile related to the image.
        /// No matter what, the <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/> must still be queried
        /// to know the actual data format of the decoded pixels after decoding.
        /// <br/><br/>
        /// The intended use case of this function is for cases where you are using
        /// a color management system to parse the original ICC color profile
        /// (<see cref="JXL_COLOR_PROFILE_TARGET_ORIGINAL"/>), from this you know that the ICC
        /// profile represents one of the color profiles supported by JxlColorEncoding
        /// (such as sRGB, PQ or HLG): in that case it is beneficial (but not necessary)
        /// to use <see cref="JxlDecoderSetPreferredColorProfile"/> to match the parsed profile.
        /// The JXL decoder has no color management system built in, but can convert XYB
        /// color to any of the ones supported by JxlColorEncoding.
        /// <br/><br/>
        /// Can only be set after the <see cref="JxlDecoderStatus.JXL_DEC_COLOR_ENCODING"/> event occurred and
        /// before any other event occurred, and can affect the result of <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/> (but not of <see cref="JXL_COLOR_PROFILE_TARGET_ORIGINAL"/>), so should be used after getting <see cref="JXL_COLOR_PROFILE_TARGET_ORIGINAL"/> but before getting <see cref="JXL_COLOR_PROFILE_TARGET_DATA"/>. The color_encoding must be grayscale if
        /// num_color_channels from the basic info is 1, RGB if num_color_channels from
        /// the basic info is 3.
        /// <br/><br/>
        /// If <see cref="JxlDecoderSetPreferredColorProfile"/> is not used, then for images for
        /// which uses_original_profile is false and with ICC color profile, the decoder
        /// will choose linear sRGB for color images, linear grayscale for grayscale
        /// images. This function only sets a preference, since for other images the
        /// decoder has no choice what color profile to use, it is determined by the
        /// image.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="color_encoding"> the default color encoding to set</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the preference was set successfully, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetPreferredColorProfile(
            JxlDecoder* dec, [In] JxlColorEncoding* color_encoding);

        /// <summary>
        /// Requests that the decoder perform tone mapping to the peak display luminance
        /// passed as <tt>desired_intensity_target</tt>, if appropriate.
        /// <br/>Note:  This is provided for convenience and the exact tone mapping that is
        /// performed is not meant to be considered authoritative in any way. It may
        /// change from version to version.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="desired_intensity_target"> the intended target peak luminance</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the preference was set successfully, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetDesiredIntensityTarget(
            JxlDecoder* dec, float desired_intensity_target);

        /// <summary>
        /// Returns the minimum size in bytes of the preview image output pixel buffer
        /// for the given format. This is the buffer for <see cref="JxlDecoderSetPreviewOutBuffer"/>. Requires the preview header information is
        /// available in the decoder.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of pixels</param>
        /// <param name="size"> output value, buffer size in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// information not available yet.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderPreviewOutBufferSize(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format, size_t* size);

        /// <summary>
        /// Sets the buffer to write the small resolution preview image
        /// to. The size of the buffer must be at least as large as given by <see cref="JxlDecoderPreviewOutBufferSize"/>. The buffer follows the format described
        /// by JxlPixelFormat. The preview image dimensions are given by the
        /// JxlPreviewHeader. The buffer is owned by the caller.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of pixels. Object owned by user and its contents are
        /// copied internally.</param>
        /// <param name="buffer"> buffer type to output the pixel data to</param>
        /// <param name="size"> size of buffer in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// size too small.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetPreviewOutBuffer(
            JxlDecoder* dec, [In] JxlPixelFormat* format, void* buffer, size_t size);

        /// <summary>
        /// Outputs the information from the frame, such as duration when have_animation.
        /// This function can be called when <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> occurred for the current
        /// frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="header"> struct to copy the information into, or NULL to only check
        /// whether the information is available through the return value.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> in
        /// case of other error conditions.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetFrameHeader([In] JxlDecoder* dec,
                                                             out JxlFrameHeader header);

        /// <summary>
        /// Outputs name for the current frame. The buffer for name must have at least
        /// name_length + 1 bytes allocated, gotten from the associated JxlFrameHeader.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="name"> buffer to copy the name into</param>
        /// <param name="size"> size of the name buffer in bytes, including zero termination
        /// character, so this must be at least JxlFrameHeader.name_length + 1.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/> if not yet available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> in
        /// case of other error conditions.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetFrameName([In] JxlDecoder* dec,
                                                           byte* name, size_t size);

        /// <summary>
        /// Outputs the blend information for the current frame for a specific extra
        /// channel. This function can be called when <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> occurred for the
        /// current frame, even when have_animation in the JxlBasicInfo is JXL_FALSE.
        /// This information is only useful if coalescing is disabled; otherwise the
        /// decoder will have performed blending already.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="index"> the index of the extra channel</param>
        /// <param name="blend_info"> struct to copy the information into</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetExtraChannelBlendInfo(
            [In] JxlDecoder* dec, size_t index, JxlBlendInfo* blend_info);

        /// <summary>
        /// Returns the minimum size in bytes of the DC image output buffer
        /// for the given format. This is the buffer for <see cref="JxlDecoderSetDCOutBuffer"/>.
        /// Requires the basic image information is available in the decoder.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of pixels</param>
        /// <param name="size"> output value, buffer size in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// information not available yet.
        /// <br/><br/>
        /// @deprecated The DC feature in this form will be removed. Use <see cref="JxlDecoderFlushImage"/> for progressive rendering.</returns>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderDCOutBufferSize(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format, size_t* size);

        /// <summary>
        /// Sets the buffer to write the lower resolution (8x8 sub-sampled) DC image
        /// to. The size of the buffer must be at least as large as given by <see cref="JxlDecoderDCOutBufferSize"/>. The buffer follows the format described by
        /// JxlPixelFormat. The DC image has dimensions ceil(xsize / 8) * ceil(ysize /
        /// 8). The buffer is owned by the caller.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of pixels. Object owned by user and its contents are
        /// copied internally.</param>
        /// <param name="buffer"> buffer type to output the pixel data to</param>
        /// <param name="size"> size of buffer in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// size too small.
        /// <br/><br/>
        /// @deprecated The DC feature in this form will be removed. Use <see cref="JxlDecoderFlushImage"/> for progressive rendering.</returns>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetDCOutBuffer(
            JxlDecoder* dec, [In] JxlPixelFormat* format, void* buffer, size_t size);

        /// <summary>
        /// Returns the minimum size in bytes of the image output pixel buffer for the
        /// given format. This is the buffer for <see cref="JxlDecoderSetImageOutBuffer"/>.
        /// Requires that the basic image information is available in the decoder in the
        /// case of coalescing enabled (default). In case coalescing is disabled, this
        /// can only be called after the <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> event occurs. In that case,
        /// it will return the size required to store the possibly cropped frame (which
        /// can be larger or smaller than the image dimensions).
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels.</param>
        /// <param name="size"> output value, buffer size in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// information not available yet.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderImageOutBufferSize(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format, size_t* size);

        /// <summary>
        /// Sets the buffer to write the full resolution image to. This can be set when
        /// the <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> event occurs, must be set when the <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER"/> event occurs, and applies only for the
        /// current frame. The size of the buffer must be at least as large as given
        /// by <see cref="JxlDecoderImageOutBufferSize"/>. The buffer follows the format described
        /// by JxlPixelFormat. The buffer is owned by the caller.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels. Object owned by user and its contents
        /// are copied internally.</param>
        /// <param name="buffer"> buffer type to output the pixel data to</param>
        /// <param name="size"> size of buffer in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// size too small.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetImageOutBuffer(
            JxlDecoder* dec, [In] JxlPixelFormat* format, void* buffer, size_t size);

        /// <summary>
        /// Function type for <see cref="JxlDecoderSetImageOutCallback"/>.
        /// <br/><br/>
        /// The callback may be called simultaneously by different threads when using a
        /// threaded parallel runner, on different pixels.
        /// </summary>
        /// <param name="opaque"> optional user data, as given to <see cref="JxlDecoderSetImageOutCallback"/>.</param>
        /// <param name="x"> horizontal position of leftmost pixel of the pixel data.</param>
        /// <param name="y"> vertical position of the pixel data.</param>
        /// <param name="num_pixels"> amount of pixels included in the pixel data, horizontally.
        /// This is not the same as xsize of the full image, it may be smaller.</param>
        /// <param name="pixels"> pixel data as a horizontal stripe, in the format passed to <see cref="JxlDecoderSetImageOutCallback"/>. The memory is not owned by the user, and
        /// is only valid during the time the callback is running.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void JxlImageOutCallback(void* opaque, size_t x, size_t y,
                                            size_t num_pixels, [In] void* pixels);




        /// <summary>
        /// Initialization callback for <see cref="JxlDecoderSetMultithreadedImageOutCallback"/>.
        /// </summary>
        /// <param name="init_opaque"> optional user data, as given to <see cref="JxlDecoderSetMultithreadedImageOutCallback"/>.</param>
        /// <param name="num_threads"> maximum number of threads that will call the <tt>run</tt>
        /// callback concurrently.</param>
        /// <param name="num_pixels_per_thread"> maximum number of pixels that will be passed in
        /// one call to <tt>run</tt>.</param>
        /// <returns> a pointer to data that will be passed to the <tt>run</tt> callback, or
        /// <tt>NULL</tt> if initialization failed.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void* JxlImageOutInitCallback(void* init_opaque, size_t num_threads,
                                                 size_t num_pixels_per_thread);

        /// <summary>
        /// Worker callback for <see cref="JxlDecoderSetMultithreadedImageOutCallback"/>.
        /// </summary>
        /// <param name="run_opaque"> user data returned by the <tt>init</tt> callback.</param>
        /// <param name="thread_id"> number in `[0, num_threads)` identifying the thread of the
        /// current invocation of the callback.</param>
        /// <param name="x"> horizontal position of the first (leftmost) pixel of the pixel data.</param>
        /// <param name="y"> vertical position of the pixel data.</param>
        /// <param name="num_pixels"> number of pixels in the pixel data. May be less than the
        /// full <tt>xsize</tt> of the image, and will be at most equal to the @c
        /// num_pixels_per_thread that was passed to <tt>init</tt>.</param>
        /// <param name="pixels"> pixel data as a horizontal stripe, in the format passed to <see cref="JxlDecoderSetMultithreadedImageOutCallback"/>. The data pointed to
        /// remains owned by the caller and is only guaranteed to outlive the current
        /// callback invocation.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void JxlImageOutRunCallback(void* run_opaque, size_t thread_id,
                                               size_t x, size_t y, size_t num_pixels,
                                               [In] void* pixels);

        /// <summary>
        /// Destruction callback for <see cref="JxlDecoderSetMultithreadedImageOutCallback"/>,
        /// called after all invocations of the <tt>run</tt> callback to perform any
        /// appropriate clean-up of the <tt>run_opaque</tt> data returned by <tt>init</tt>.
        /// </summary>
        /// <param name="run_opaque"> user data returned by the <tt>init</tt> callback.</param>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal delegate void JxlImageOutDestroyCallback(void* run_opaque);

        /// <summary>
        /// Sets pixel output callback. This is an alternative to <see cref="JxlDecoderSetImageOutBuffer"/>. This can be set when the <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/>
        /// event occurs, must be set when the <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER"/> event
        /// occurs, and applies only for the current frame. Only one of <see cref="JxlDecoderSetImageOutBuffer"/> or <see cref="JxlDecoderSetImageOutCallback"/> may be used
        /// for the same frame, not both at the same time.
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
        /// If <see cref="JxlDecoderFlushImage"/> is not used, then each pixel will be visited
        /// exactly once by the different callback calls, during processing with one or
        /// more <see cref="JxlDecoderProcessInput"/> calls. These pixels are decoded to full
        /// detail, they are not part of a lower resolution or lower quality progressive
        /// pass, but the final pass.
        /// <br/><br/>
        /// If <see cref="JxlDecoderFlushImage"/> is used, then in addition each pixel will be
        /// visited zero or one times during the blocking <see cref="JxlDecoderFlushImage"/> call.
        /// Pixels visited as a result of <see cref="JxlDecoderFlushImage"/> may represent a lower
        /// resolution or lower quality intermediate progressive pass of the image. Any
        /// visited pixel will be of a quality at least as good or better than previous
        /// visits of this pixel. A pixel may be visited zero times if it cannot be
        /// decoded yet or if it was already decoded to full precision (this behavior is
        /// not guaranteed).
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels. Object owned by user; its contents are
        /// copied internally.</param>
        /// <param name="callback"> the callback function receiving partial scanlines of pixel
        /// data.</param>
        /// <param name="opaque"> optional user data, which will be passed on to the callback,
        /// may be NULL.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such
        /// as <see cref="JxlDecoderSetImageOutBuffer"/> already set.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetImageOutCallback(JxlDecoder* dec, [In] JxlPixelFormat* format,
                                      JxlImageOutCallback callback, void* opaque);

        /// <summary>
        /// Similar to <see cref="JxlDecoderSetImageOutCallback"/> except that the callback is
        /// allowed an initialization phase during which it is informed of how many
        /// threads will call it concurrently, and those calls are further informed of
        /// which thread they are occurring in.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels. Object owned by user; its contents are
        /// copied internally.</param>
        /// <param name="init_callback"> initialization callback.</param>
        /// <param name="run_callback"> the callback function receiving partial scanlines of
        /// pixel data.</param>
        /// <param name="destroy_callback"> clean-up callback invoked after all calls to @c
        /// run_callback. May be NULL if no clean-up is necessary.</param>
        /// <param name="init_opaque"> optional user data passed to <tt>init_callback</tt>, may be NULL
        /// (unlike the return value from <tt>init_callback</tt> which may only be NULL if
        /// initialization failed).</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such
        /// as <see cref="JxlDecoderSetImageOutBuffer"/> having already been called.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetMultithreadedImageOutCallback(
            JxlDecoder* dec, [In] JxlPixelFormat* format,
            JxlImageOutInitCallback init_callback, JxlImageOutRunCallback run_callback,
            JxlImageOutDestroyCallback destroy_callback, void* init_opaque);

        /// <summary>
        /// Returns the minimum size in bytes of an extra channel pixel buffer for the
        /// given format. This is the buffer for <see cref="JxlDecoderSetExtraChannelBuffer"/>.
        /// Requires the basic image information is available in the decoder.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels. The num_channels value is ignored and is
        /// always treated to be 1.</param>
        /// <param name="size"> output value, buffer size in bytes</param>
        /// <param name="index"> which extra channel to get, matching the index used in <see cref="JxlDecoderGetExtraChannelInfo"/>. Must be smaller than num_extra_channels in
        /// the associated JxlBasicInfo.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// information not available yet or invalid index.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderExtraChannelBufferSize(
            [In] JxlDecoder* dec, [In] JxlPixelFormat* format, size_t* size,
            uint32_t index);

        /// <summary>
        /// Sets the buffer to write an extra channel to. This can be set when
        /// the <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> or <see cref="JxlDecoderStatus.JXL_DEC_NEED_IMAGE_OUT_BUFFER"/> event occurs,
        /// and applies only for the current frame. The size of the buffer must be at
        /// least as large as given by <see cref="JxlDecoderExtraChannelBufferSize"/>. The buffer
        /// follows the format described by JxlPixelFormat, but where num_channels is 1.
        /// The buffer is owned by the caller. The amount of extra channels is given by
        /// the num_extra_channels field in the associated JxlBasicInfo, and the
        /// information of individual extra channels can be queried with <see cref="JxlDecoderGetExtraChannelInfo"/>. To get multiple extra channels, this function
        /// must be called multiple times, once for each wanted index. Not all images
        /// have extra channels. The alpha channel is an extra channel and can be gotten
        /// as part of the color channels when using an RGBA pixel buffer with <see cref="JxlDecoderSetImageOutBuffer"/>, but additionally also can be gotten
        /// separately as extra channel. The color channels themselves cannot be gotten
        /// this way.
        /// <br/><br/>
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="format"> format of the pixels. Object owned by user and its contents
        /// are copied internally. The num_channels value is ignored and is always
        /// treated to be 1.</param>
        /// <param name="buffer"> buffer type to output the pixel data to</param>
        /// <param name="size"> size of buffer in bytes</param>
        /// <param name="index"> which extra channel to get, matching the index used in <see cref="JxlDecoderGetExtraChannelInfo"/>. Must be smaller than num_extra_channels in
        /// the associated JxlBasicInfo.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// size too small or invalid index.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetExtraChannelBuffer(JxlDecoder* dec, [In] JxlPixelFormat* format,
                                        void* buffer, size_t size, uint32_t index);

        /// <summary>
        /// Sets output buffer for reconstructed JPEG codestream.
        /// <br/><br/>
        /// The data is owned by the caller and may be used by the decoder until <see cref="JxlDecoderReleaseJPEGBuffer"/> is called or the decoder is destroyed or
        /// reset so must be kept alive until then.
        /// <br/><br/>
        /// If a JPEG buffer was set before and released with <see cref="JxlDecoderReleaseJPEGBuffer"/>, bytes that the decoder has already output
        /// should not be included, only the remaining bytes output must be set.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="data"> pointer to next bytes to write to</param>
        /// <param name="size"> amount of bytes available starting from data</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if output buffer was already set and <see cref="JxlDecoderReleaseJPEGBuffer"/> was not called on it, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/>
        /// otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetJPEGBuffer(JxlDecoder* dec,
                                                            uint8_t* data, size_t size);

        /// <summary>
        /// Releases buffer which was provided with <see cref="JxlDecoderSetJPEGBuffer"/>.
        /// <br/><br/>
        /// Calling <see cref="JxlDecoderReleaseJPEGBuffer"/> is required whenever
        /// a buffer is already set and a new buffer needs to be added with <see cref="JxlDecoderSetJPEGBuffer"/>, but is not required before <see cref="JxlDecoderDestroy"/> or <see cref="JxlDecoderReset"/>.
        /// <br/><br/>
        /// Calling <see cref="JxlDecoderReleaseJPEGBuffer"/> when no buffer is set is
        /// not an error and returns 0.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> the amount of bytes the decoder has not yet written to of the data
        /// set by <see cref="JxlDecoderSetJPEGBuffer"/>, or 0 if no buffer is set or <see cref="JxlDecoderReleaseJPEGBuffer"/> was already called.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlDecoderReleaseJPEGBuffer(JxlDecoder* dec);

        /// <summary>
        /// Sets output buffer for box output codestream.
        /// <br/><br/>
        /// The data is owned by the caller and may be used by the decoder until <see cref="JxlDecoderReleaseBoxBuffer"/> is called or the decoder is destroyed or
        /// reset so must be kept alive until then.
        /// <br/><br/>
        /// If for the current box a box buffer was set before and released with <see cref="JxlDecoderReleaseBoxBuffer"/>, bytes that the decoder has already output
        /// should not be included, only the remaining bytes output must be set.
        /// <br/><br/>
        /// The <see cref="JxlDecoderReleaseBoxBuffer"/> must be used at the next <see cref="JxlDecoderStatus.JXL_DEC_BOX"/>
        /// event or final <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> event to compute the size of the output
        /// box bytes.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="data"> pointer to next bytes to write to</param>
        /// <param name="size"> amount of bytes available starting from data</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if output buffer was already set and <see cref="JxlDecoderReleaseBoxBuffer"/> was not called on it, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/>
        /// otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetBoxBuffer(JxlDecoder* dec,
                                                           uint8_t* data, size_t size);

        /// <summary>
        /// Releases buffer which was provided with <see cref="JxlDecoderSetBoxBuffer"/>.
        /// <br/><br/>
        /// Calling <see cref="JxlDecoderReleaseBoxBuffer"/> is required whenever
        /// a buffer is already set and a new buffer needs to be added with <see cref="JxlDecoderSetBoxBuffer"/>, but is not required before <see cref="JxlDecoderDestroy"/> or <see cref="JxlDecoderReset"/>.
        /// <br/><br/>
        /// Calling <see cref="JxlDecoderReleaseBoxBuffer"/> when no buffer is set is
        /// not an error and returns 0.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> the amount of bytes the decoder has not yet written to of the data
        /// set by <see cref="JxlDecoderSetBoxBuffer"/>, or 0 if no buffer is set or <see cref="JxlDecoderReleaseBoxBuffer"/> was already called.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlDecoderReleaseBoxBuffer(JxlDecoder* dec);

        /// <summary>
        /// Configures whether to get boxes in raw mode or in decompressed mode. In raw
        /// mode, boxes are output as their bytes appear in the container file, which may
        /// be decompressed, or compressed if their type is "brob". In decompressed mode,
        /// "brob" boxes are decompressed with Brotli before outputting them. The size of
        /// the decompressed stream is not known before the decompression has already
        /// finished.
        /// <br/><br/>
        /// The default mode is raw. This setting can only be changed before decoding, or
        /// directly after a <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> event, and is remembered until the decoder
        /// is reset or destroyed.
        /// <br/><br/>
        /// Enabling decompressed mode requires Brotli support from the library.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="decompress"> JXL_TRUE to transparently decompress, JXL_FALSE to get
        /// boxes in raw mode.</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if decompressed mode is set and Brotli is not
        /// available, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderSetDecompressBoxes(JxlDecoder* dec,
                                                                 JXL_BOOL decompress);

        /// <summary>
        /// Outputs the type of the current box, after a <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> event occured,
        /// as 4 characters without null termination character. In case of a compressed
        /// "brob" box, this will return "brob" if the decompressed argument is
        /// JXL_FALSE, or the underlying box type if the decompressed argument is
        /// JXL_TRUE.
        /// <br/><br/>
        /// The following box types are currently described in ISO/IEC 18181-2:
        /// - "Exif": a box with EXIF metadata.  Starts with a 4-byte tiff header offset
        /// (big-endian uint32) that indicates the start of the actual EXIF data
        /// (which starts with a tiff header). Usually the offset will be zero and the
        /// EXIF data starts immediately after the offset field. The Exif orientation
        /// should be ignored by applications; the JPEG XL codestream orientation
        /// takes precedence and libjxl will by default apply the correct orientation
        /// automatically (see <see cref="JxlDecoderSetKeepOrientation"/>).
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
        /// type of box such as Exif or "xml ". When <see cref="JxlDecoderSetDecompressBoxes"/>
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
        /// <br/><br/>
        /// Other application-specific boxes can exist. Their typename should not begin
        /// with "jxl" or "JXL" or conflict with other existing typenames.
        /// <br/><br/>
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
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if the value is available, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if
        /// not, for example the JXL file does not use the container format.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetBoxType(JxlDecoder* dec,
                                                         byte *type,
                                                         JXL_BOOL decompressed);

        /// <summary>
        /// Returns the size of a box as it appears in the container file, after the <see cref="JxlDecoderStatus.JXL_DEC_BOX"/> event. For a non-compressed box, this is the size of the
        /// contents, excluding the 4 bytes indicating the box type. For a compressed
        /// "brob" box, this is the size of the compressed box contents plus the
        /// additional 4 byte indicating the underlying box type, but excluding the 4
        /// bytes indicating "brob". This function gives the size of the data that will
        /// be written in the output buffer when getting boxes in the default raw
        /// compressed mode. When <see cref="JxlDecoderSetDecompressBoxes"/> is enabled, the
        /// return value of function does not change, and the decompressed size is not
        /// known before it has already been decompressed and output.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="size"> raw size of the box in bytes</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> if no box size is available, <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/>
        /// otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderGetBoxSizeRaw([In] JxlDecoder* dec,
                                                            out uint64_t size);

        /// <summary>
        /// Configures at which progressive steps in frame decoding these <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION"/> event occurs. The default value for the level
        /// of detail if this function is never called is `kDC`.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <param name="detail"> at which level of detail to trigger <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION"/></param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> on success, <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> on error, such as
        /// an invalid value for the progressive detail.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus
        JxlDecoderSetProgressiveDetail(JxlDecoder* dec, JxlProgressiveDetail detail);

        /// <summary>
        /// Returns the intended downsampling ratio for the progressive frame produced
        /// by <see cref="JxlDecoderFlushImage"/> after the latest <see cref="JxlDecoderStatus.JXL_DEC_FRAME_PROGRESSION"/>
        /// event.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> The intended downsampling ratio, can be 1, 2, 4 or 8.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern size_t JxlDecoderGetIntendedDownsamplingRatio(JxlDecoder* dec);

        /// <summary>
        /// Outputs progressive step towards the decoded image so far when only partial
        /// input was received. If the flush was successful, the buffer set with <see cref="JxlDecoderSetImageOutBuffer"/> will contain partial image data.
        /// <br/><br/>
        /// Can be called when <see cref="JxlDecoderProcessInput"/> returns <see cref="JxlDecoderStatus.JXL_DEC_NEED_MORE_INPUT"/>, after the <see cref="JxlDecoderStatus.JXL_DEC_FRAME"/> event already occurred
        /// and before the <see cref="JxlDecoderStatus.JXL_DEC_FULL_IMAGE"/> event occurred for a frame.
        /// </summary>
        /// <param name="dec"> decoder object</param>
        /// <returns> <see cref="JxlDecoderStatus.JXL_DEC_SUCCESS"/> if image data was flushed to the output buffer,
        /// or <see cref="JxlDecoderStatus.JXL_DEC_ERROR"/> when no flush was done, e.g. if not enough image
        /// data was available yet even for flush, or no output buffer was set yet.
        /// This error is not fatal, it only indicates no flushed image is available
        /// right now. Regular decoding can still be performed.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlDecoderStatus JxlDecoderFlushImage(JxlDecoder* dec);
    }
}
