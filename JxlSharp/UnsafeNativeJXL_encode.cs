using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    //typedefs for C types
    using int32_t = Int32;
    using uint32_t = UInt32;
    using uint8_t = Byte;
    using size_t = UIntPtr;
    using JXL_BOOL = Int32;
    //"JxlParallelRunner" is a function pointer type
    using JxlParallelRunner = IntPtr;

    internal unsafe partial class UnsafeNativeJXL
    {
        /// <summary>
        /// Encoder library version.
        /// </summary>
        /// <returns> the encoder library version as an integer:
        /// MAJOR_VERSION * 1000000 + MINOR_VERSION * 1000 + PATCH_VERSION. For example,
        /// version 1.2.3 would return 1002003.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern uint32_t JxlEncoderVersion();

        /// <summary>
        /// Opaque structure that holds the JPEG XL encoder.
        /// <br/><br/>
        /// Allocated and initialized with JxlEncoderCreate().
        /// Cleaned up and deallocated with JxlEncoderDestroy().
        /// </summary>
        internal struct JxlEncoder { }

        /// <summary>
        /// Settings and metadata for a single image frame. This includes encoder options
        /// for a frame such as compression quality and speed.
        /// <br/><br/>
        /// Allocated and initialized with JxlEncoderFrameSettingsCreate().
        /// Cleaned up and deallocated when the encoder is destroyed with
        /// JxlEncoderDestroy().
        /// </summary>
        internal struct JxlEncoderFrameSettings { }

        /// <summary>
        /// DEPRECATED: Use JxlEncoderFrameSettings instead.
        /// </summary>
        //using JxlEncoderOptions = JxlEncoderFrameSettings;

        /// <summary>
        /// Return value for multiple encoder functions.
        /// </summary>
        internal enum JxlEncoderStatus
        {
          /// <summary>
          /// Function call finished successfully, or encoding is finished and there is
          /// nothing more to be done.
          /// </summary>
          JXL_ENC_SUCCESS = 0,

          /// <summary>
          /// An error occurred, for example out of memory.
          /// </summary>
          JXL_ENC_ERROR = 1,

          /// <summary>
          /// The encoder needs more output buffer to continue encoding.
          /// </summary>
          JXL_ENC_NEED_MORE_OUTPUT = 2,

          /// <summary>
          /// DEPRECATED: the encoder does not return this status and there is no need
          /// to handle or expect it.
          /// Instead, JXL_ENC_ERROR is returned with error condition
          /// JXL_ENC_ERR_NOT_SUPPORTED.
          /// </summary>
          JXL_ENC_NOT_SUPPORTED = 3,

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
          JXL_ENC_ERR_NOT_SUPPORTED = 0x80,

          /// <summary>
          /// The encoder API is used in an incorrect way.
          /// In this case, a debug build of libjxl should output a specific error
          /// message. (if not, please open an issue about it)
          /// </summary>
          JXL_ENC_ERR_API_USAGE = 0x81,

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
          /// Indicates the frame added with <see cref="JxlEncoderAddImageFrame"/> is already
          /// downsampled by the downsampling factor set with <see cref="JXL_ENC_FRAME_SETTING_RESAMPLING"/>. The input frame must then be given in the
          /// downsampled resolution, not the full image resolution. The downsampled
          /// resolution is given by ceil(xsize / resampling), ceil(ysize / resampling)
          /// with xsize and ysize the dimensions given in the basic info, and resampling
          /// the factor set with <see cref="JXL_ENC_FRAME_SETTING_RESAMPLING"/>.
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
          JXL_ENC_FRAME_SETTING_FILL_ENUM = 65535,

        }

        /// <summary>
        /// Creates an instance of JxlEncoder and initializes it.
        /// <br/><br/>
        /// <tt>memory_manager</tt> will be used for all the library dynamic allocations made
        /// from this instance. The parameter may be NULL, in which case the default
        /// allocator will be used. See jpegxl/memory_manager.h for details.
        /// </summary>
        /// <param name="memory_manager"> custom allocator function. It may be NULL. The memory
        /// manager will be copied internally.</param>
        /// <returns> <tt>NULL</tt> if the instance can not be allocated or initialized</returns>
        /// <returns> pointer to initialized JxlEncoder otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoder* JxlEncoderCreate([In]JxlMemoryManager* memory_manager);

        /// <summary>
        /// Re-initializes a JxlEncoder instance, so it can be re-used for encoding
        /// another image. All state and settings are reset as if the object was
        /// newly created with JxlEncoderCreate, but the memory manager is kept.
        /// </summary>
        /// <param name="enc"> instance to be re-initialized.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderReset(JxlEncoder* enc);

        /// <summary>
        /// Deinitializes and frees JxlEncoder instance.
        /// </summary>
        /// <param name="enc"> instance to be cleaned up and deallocated.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderDestroy(JxlEncoder* enc);

        ///// <summary>
        ///// Sets the color management system (CMS) that will be used for color conversion
        ///// (if applicable) during encoding. May only be set before starting encoding. If
        ///// left unset, the default CMS implementation will be used.
        ///// </summary>
        ///// <param name="enc"> encoder object.</param>
        ///// <param name="cms"> structure representing a CMS implementation. See JxlCmsInterface
        ///// for more details.</param>
        //[DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        //internal static extern void JxlEncoderSetCms(JxlEncoder* enc, JxlCmsInterface cms);

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
        internal static extern JxlEncoderStatus
        JxlEncoderSetParallelRunner(JxlEncoder* enc, JxlParallelRunner parallel_runner,
                                    void* parallel_runner_opaque);

        /// <summary>
        /// Get the (last) error code in case JXL_ENC_ERROR was returned.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <returns> the JxlEncoderError that caused the (last) JXL_ENC_ERROR to be
        /// returned.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderError JxlEncoderGetError(JxlEncoder* enc);

        /// <summary>
        /// Encodes JPEG XL file using the available bytes. <tt>*avail_out indicates how</tt>
        /// many output bytes are available, and <tt>*next_out points to the input bytes.</tt>
        /// *avail_out will be decremented by the amount of bytes that have been
        /// processed by the encoder and *next_out will be incremented by the same
        /// amount, so *next_out will now point at the amount of *avail_out unprocessed
        /// bytes.
        /// <br/><br/>
        /// The returned status indicates whether the encoder needs more output bytes.
        /// When the return value is not JXL_ENC_ERROR or JXL_ENC_SUCCESS, the encoding
        /// requires more JxlEncoderProcessOutput calls to continue.
        /// <br/><br/>
        /// This encodes the frames and/or boxes added so far. If the last frame or last
        /// box has been added, <see cref="JxlEncoderCloseInput"/>, <see cref="JxlEncoderCloseFrames"/>
        /// and/or <see cref="JxlEncoderCloseBoxes"/> must be called before the next
        /// <see cref="JxlEncoderProcessOutput"/> call, or the codestream won't be encoded
        /// correctly.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="next_out"> pointer to next bytes to write to.</param>
        /// <param name="avail_out"> amount of bytes available starting from *next_out.</param>
        /// <returns> JXL_ENC_SUCCESS when encoding finished and all events handled.</returns>
        /// <returns> JXL_ENC_ERROR when encoding failed, e.g. invalid input.</returns>
        /// <returns> JXL_ENC_NEED_MORE_OUTPUT more output buffer is necessary.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderProcessOutput(JxlEncoder* enc,
                                                            uint8_t** next_out,
                                                            size_t* avail_out);

        /// <summary>
        /// Sets the frame information for this frame to the encoder. This includes
        /// animation information such as frame duration to store in the frame header.
        /// The frame header fields represent the frame as passed to the encoder, but not
        /// necessarily the exact values as they will be encoded file format: the encoder
        /// could change crop and blending options of a frame for more efficient encoding
        /// or introduce additional internal frames. Animation duration and time code
        /// information is not altered since those are immutable metadata of the frame.
        /// <br/><br/>
        /// It is not required to use this function, however if have_animation is set
        /// to true in the basic info, then this function should be used to set the
        /// time duration of this individual frame. By default individual frames have a
        /// time duration of 0, making them form a composite still. See <see cref="JxlFrameHeader"/> for more information.
        /// <br/><br/>
        /// This information is stored in the JxlEncoderFrameSettings and so is used for
        /// any frame encoded with these JxlEncoderFrameSettings. It is ok to change
        /// between <see cref="JxlEncoderAddImageFrame"/> calls, each added image frame will have
        /// the frame header that was set in the options at the time of calling
        /// JxlEncoderAddImageFrame.
        /// <br/><br/>
        /// The is_last and name_length fields of the JxlFrameHeader are ignored, use
        /// <see cref="JxlEncoderCloseFrames"/> to indicate last frame, and <see cref="JxlEncoderSetFrameName"/> to indicate the name and its length instead.
        /// Calling this function will clear any name that was previously set with <see cref="JxlEncoderSetFrameName"/>.
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="frame_header"> frame header data to set. Object owned by the caller and
        /// does not need to be kept in memory, its information is copied internally.</param>
        /// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderSetFrameHeader(JxlEncoderFrameSettings* frame_settings,
                                 [In]JxlFrameHeader* frame_header);

        /// <summary>
        /// Sets blend info of an extra channel. The blend info of extra channels is set
        /// separately from that of the color channels, the color channels are set with
        /// <see cref="JxlEncoderSetFrameHeader"/>.
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="index"> index of the extra channel to use.</param>
        /// <param name="blend_info"> blend info to set for the extra channel</param>
        /// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderSetExtraChannelBlendInfo(
            JxlEncoderFrameSettings* frame_settings, size_t index,
            [In]JxlBlendInfo* blend_info);

        /// <summary>
        /// Sets the name of the animation frame. This function is optional, frames are
        /// not required to have a name. This setting is a part of the frame header, and
        /// the same principles as for <see cref="JxlEncoderSetFrameHeader"/> apply. The
        /// name_length field of JxlFrameHeader is ignored by the encoder, this function
        /// determines the name length instead as the length in bytes of the C string.
        /// <br/><br/>
        /// The maximum possible name length is 1071 bytes (excluding terminating null
        /// character).
        /// <br/><br/>
        /// Calling <see cref="JxlEncoderSetFrameHeader"/> clears any name that was
        /// previously set.
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="frame_name"> name of the next frame to be encoded, as a UTF-8 encoded C
        /// string (zero terminated). Owned by the caller, and copied internally.</param>
        /// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderSetFrameName(
            JxlEncoderFrameSettings* frame_settings, [In]byte* frame_name);

        /// <summary>
        /// Sets the buffer to read JPEG encoded bytes from for the next frame to encode.
        /// <br/><br/>
        /// If JxlEncoderSetBasicInfo has not yet been called, calling
        /// JxlEncoderAddJPEGFrame will implicitly call it with the parameters of the
        /// added JPEG frame.
        /// <br/><br/>
        /// If JxlEncoderSetColorEncoding or JxlEncoderSetICCProfile has not yet been
        /// called, calling JxlEncoderAddJPEGFrame will implicitly call it with the
        /// parameters of the added JPEG frame.
        /// <br/><br/>
        /// If the encoder is set to store JPEG reconstruction metadata using <see cref="JxlEncoderStoreJPEGMetadata"/> and a single JPEG frame is added, it will be
        /// possible to losslessly reconstruct the JPEG codestream.
        /// <br/><br/>
        /// If this is the last frame, <see cref="JxlEncoderCloseInput"/> or <see cref="JxlEncoderCloseFrames"/> must be called before the next
        /// <see cref="JxlEncoderProcessOutput"/> call.
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="buffer"> bytes to read JPEG from. Owned by the caller and its contents
        /// are copied internally.</param>
        /// <param name="size"> size of buffer in bytes.</param>
        /// <returns> JXL_ENC_SUCCESS on success, JXL_ENC_ERROR on error</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderAddJPEGFrame([In]JxlEncoderFrameSettings* frame_settings,
                               [In]uint8_t* buffer, size_t size);

        /// <summary>
        /// Sets the buffer to read pixels from for the next image to encode. Must call
        /// JxlEncoderSetBasicInfo before JxlEncoderAddImageFrame.
        /// <br/><br/>
        /// Currently only some data types for pixel formats are supported:
        /// - JXL_TYPE_UINT8, with range 0..255
        /// - JXL_TYPE_UINT16, with range 0..65535
        /// - JXL_TYPE_FLOAT16, with nominal range 0..1
        /// - JXL_TYPE_FLOAT, with nominal range 0..1
        /// <br/><br/>
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
        /// <br/><br/>
        /// We support interleaved channels as described by the JxlPixelFormat:
        /// - single-channel data, e.g. grayscale
        /// - single-channel + alpha
        /// - trichromatic, e.g. RGB
        /// - trichromatic + alpha
        /// <br/><br/>
        /// Extra channels not handled here need to be set by <see cref="JxlEncoderSetExtraChannelBuffer"/>.
        /// If the image has alpha, and alpha is not passed here, it will implicitly be
        /// set to all-opaque (an alpha value of 1.0 everywhere).
        /// <br/><br/>
        /// The pixels are assumed to be encoded in the original profile that is set with
        /// JxlEncoderSetColorEncoding or JxlEncoderSetICCProfile. If none of these
        /// functions were used, the pixels are assumed to be nonlinear sRGB for integer
        /// data types (JXL_TYPE_UINT8, JXL_TYPE_UINT16), and linear sRGB for floating
        /// point data types (JXL_TYPE_FLOAT16, JXL_TYPE_FLOAT).
        /// <br/><br/>
        /// Sample values in floating-point pixel formats are allowed to be outside the
        /// nominal range, e.g. to represent out-of-sRGB-gamut colors in the
        /// uses_original_profile=false case. They are however not allowed to be NaN or
        /// +-infinity.
        /// <br/><br/>
        /// If this is the last frame, <see cref="JxlEncoderCloseInput"/> or <see cref="JxlEncoderCloseFrames"/> must be called before the next
        /// <see cref="JxlEncoderProcessOutput"/> call.
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
        internal static extern JxlEncoderStatus JxlEncoderAddImageFrame(
            [In]JxlEncoderFrameSettings* frame_settings,
            [In]JxlPixelFormat* pixel_format, [In]void* buffer, size_t size);

        /// <summary>
        /// Sets the buffer to read pixels from for an extra channel at a given index.
        /// The index must be smaller than the num_extra_channels in the associated
        /// JxlBasicInfo. Must call <see cref="JxlEncoderSetExtraChannelInfo"/> before
        /// JxlEncoderSetExtraChannelBuffer.
        /// <br/><br/>
        /// TODO(firsching): mention what data types in pixel formats are supported.
        /// <br/><br/>
        /// It is required to call this function for every extra channel, except for the
        /// alpha channel if that was already set through <see cref="JxlEncoderAddImageFrame"/>.
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
        internal static extern JxlEncoderStatus JxlEncoderSetExtraChannelBuffer(
            [In]JxlEncoderFrameSettings* frame_settings,
            [In]JxlPixelFormat* pixel_format, [In]void* buffer, size_t size,
            uint32_t index);

        /// <summary>
        /// Adds a metadata box to the file format. JxlEncoderProcessOutput must be used
        /// to effectively write the box to the output. <see cref="JxlEncoderUseBoxes"/> must
        /// be enabled before using this function.
        /// <br/><br/>
        /// Boxes allow inserting application-specific data and metadata (Exif, XML/XMP,
        /// JUMBF and user defined boxes).
        /// <br/><br/>
        /// The box format follows ISO BMFF and shares features and box types with other
        /// image and video formats, including the Exif, XML and JUMBF boxes. The box
        /// format for JPEG XL is specified in ISO/IEC 18181-2.
        /// <br/><br/>
        /// Boxes in general don't contain other boxes inside, except a JUMBF superbox.
        /// Boxes follow each other sequentially and are byte-aligned. If the container
        /// format is used, the JXL stream consists of concatenated boxes.
        /// It is also possible to use a direct codestream without boxes, but in that
        /// case metadata cannot be added.
        /// <br/><br/>
        /// Each box generally has the following byte structure in the file:
        /// - 4 bytes: box size including box header (Big endian. If set to 0, an
        /// 8-byte 64-bit size follows instead).
        /// - 4 bytes: type, e.g. "JXL " for the signature box, "jxlc" for a codestream
        /// box.
        /// - N bytes: box contents.
        /// <br/><br/>
        /// Only the box contents are provided to the contents argument of this function,
        /// the encoder encodes the size header itself. Most boxes are written
        /// automatically by the encoder as needed ("JXL ", "ftyp", "jxll", "jxlc",
        /// "jxlp", "jxli", "jbrd"), and this function only needs to be called to add
        /// optional metadata when encoding from pixels (using JxlEncoderAddImageFrame).
        /// When recompressing JPEG files (using JxlEncoderAddJPEGFrame), if the input
        /// JPEG contains EXIF, XMP or JUMBF metadata, the corresponding boxes are
        /// already added automatically.
        /// <br/><br/>
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
        /// <br/><br/>
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
        internal static extern JxlEncoderStatus JxlEncoderAddBox(JxlEncoder* enc,
                                                     [In]byte* type,
                                                     [In]uint8_t* contents,
                                                     size_t size,
                                                     JXL_BOOL compress_box);

        /// <summary>
        /// Indicates the intention to add metadata boxes. This allows <see cref="JxlEncoderAddBox"/> to be used. When using this function, then it is required
        /// to use <see cref="JxlEncoderCloseBoxes"/> at the end.
        /// <br/><br/>
        /// By default the encoder assumes no metadata boxes will be added.
        /// <br/><br/>
        /// This setting can only be set at the beginning, before encoding starts.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderUseBoxes(JxlEncoder* enc);

        /// <summary>
        /// Declares that no further boxes will be added with <see cref="JxlEncoderAddBox"/>.
        /// This function must be called after the last box is added so the encoder knows
        /// the stream will be finished. It is not necessary to use this function if
        /// <see cref="JxlEncoderUseBoxes"/> is not used. Further frames may still be added.
        /// <br/><br/>
        /// Must be called between JxlEncoderAddBox of the last box
        /// and the next call to JxlEncoderProcessOutput, or <see cref="JxlEncoderProcessOutput"/>
        /// won't output the last box correctly.
        /// <br/><br/>
        /// NOTE: if you don't need to close frames and boxes at separate times, you can
        /// use <see cref="JxlEncoderCloseInput"/> instead to close both at once.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderCloseBoxes(JxlEncoder* enc);

        /// <summary>
        /// Declares that no frames will be added and <see cref="JxlEncoderAddImageFrame"/> and
        /// <see cref="JxlEncoderAddJPEGFrame"/> won't be called anymore. Further metadata boxes
        /// may still be added. This function or <see cref="JxlEncoderCloseInput"/> must be called
        /// after adding the last frame and the next call to
        /// <see cref="JxlEncoderProcessOutput"/>, or the frame won't be properly marked as last.
        /// <br/><br/>
        /// NOTE: if you don't need to close frames and boxes at separate times, you can
        /// use <see cref="JxlEncoderCloseInput"/> instead to close both at once.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderCloseFrames(JxlEncoder* enc);

        /// <summary>
        /// Closes any input to the encoder, equivalent to calling JxlEncoderCloseFrames
        /// as well as calling JxlEncoderCloseBoxes if needed. No further input of any
        /// kind may be given to the encoder, but further <see cref="JxlEncoderProcessOutput"/>
        /// calls should be done to create the final output.
        /// <br/><br/>
        /// The requirements of both <see cref="JxlEncoderCloseFrames"/> and <see cref="JxlEncoderCloseBoxes"/> apply to this function. Either this function or the
        /// other two must be called after the final frame and/or box, and the next
        /// <see cref="JxlEncoderProcessOutput"/> call, or the codestream won't be encoded
        /// correctly.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderCloseInput(JxlEncoder* enc);

        /// <summary>
        /// Sets the original color encoding of the image encoded by this encoder. This
        /// is an alternative to JxlEncoderSetICCProfile and only one of these two must
        /// be used. This one sets the color encoding as a <see cref="JxlColorEncoding"/>, while
        /// the other sets it as ICC binary data.
        /// Must be called after JxlEncoderSetBasicInfo.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="color"> color encoding. Object owned by the caller and its contents are
        /// copied internally.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
        /// JXL_ENC_NOT_SUPPORTED otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderSetColorEncoding(JxlEncoder* enc, [In]JxlColorEncoding* color);

        /// <summary>
        /// Sets the original color encoding of the image encoded by this encoder as an
        /// ICC color profile. This is an alternative to JxlEncoderSetColorEncoding and
        /// only one of these two must be used. This one sets the color encoding as ICC
        /// binary data, while the other defines it as a <see cref="JxlColorEncoding"/>.
        /// Must be called after JxlEncoderSetBasicInfo.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="icc_profile"> bytes of the original ICC profile</param>
        /// <param name="size"> size of the icc_profile buffer in bytes</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR or
        /// JXL_ENC_NOT_SUPPORTED otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderSetICCProfile(JxlEncoder* enc,
                                                            [In]uint8_t* icc_profile,
                                                            size_t size);

        /// <summary>
        /// Initializes a JxlBasicInfo struct to default values.
        /// For forwards-compatibility, this function has to be called before values
        /// are assigned to the struct fields.
        /// The default values correspond to an 8-bit RGB image, no alpha or any
        /// other extra channels.
        /// </summary>
        /// <param name="info"> global image metadata. Object owned by the caller.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderInitBasicInfo(JxlBasicInfo* info);

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
        internal static extern void JxlEncoderInitFrameHeader(JxlFrameHeader* frame_header);

        /// <summary>
        /// Initializes a JxlBlendInfo struct to default values.
        /// For forwards-compatibility, this function has to be called before values
        /// are assigned to the struct fields.
        /// </summary>
        /// <param name="blend_info"> blending info. Object owned by the caller.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlEncoderInitBlendInfo(JxlBlendInfo* blend_info);

        /// <summary>
        /// Sets the global metadata of the image encoded by this encoder.
        /// <br/><br/>
        /// If the JxlBasicInfo contains information of extra channels beyond an alpha
        /// channel, then <see cref="JxlEncoderSetExtraChannelInfo"/> must be called between
        /// JxlEncoderSetBasicInfo and <see cref="JxlEncoderAddImageFrame"/>. In order to indicate
        /// extra channels, the value of `info.num_extra_channels` should be set to the
        /// number of extra channels, also counting the alpha channel if present.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="info"> global image metadata. Object owned by the caller and its
        /// contents are copied internally.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful,
        /// JXL_ENC_ERROR or JXL_ENC_NOT_SUPPORTED otherwise</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderSetBasicInfo(JxlEncoder* enc,
                                                           [In]JxlBasicInfo* info);

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
        internal static extern void JxlEncoderInitExtraChannelInfo(JxlExtraChannelType type,
                                                       JxlExtraChannelInfo* info);

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
        internal static extern JxlEncoderStatus JxlEncoderSetExtraChannelInfo(
            JxlEncoder* enc, size_t index, [In]JxlExtraChannelInfo* info);

        /// <summary>
        /// Sets the name for the extra channel at the given index in UTF-8. The index
        /// must be smaller than the num_extra_channels in the associated JxlBasicInfo.
        /// <br/><br/>
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
        internal static extern JxlEncoderStatus JxlEncoderSetExtraChannelName(JxlEncoder* enc,
                                                                  size_t index,
                                                                  [In]byte* name,
                                                                  size_t size);

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
        internal static extern JxlEncoderStatus JxlEncoderFrameSettingsSetOption(
            JxlEncoderFrameSettings* frame_settings, JxlEncoderFrameSettingId option,
            int32_t value);

        /// <summary>
        /// Forces the encoder to use the box-based container format (BMFF) even
        /// when not necessary.
        /// <br/><br/>
        /// When using <see cref="JxlEncoderUseBoxes"/>, <see cref="JxlEncoderStoreJPEGMetadata"/> or <see cref="JxlEncoderSetCodestreamLevel"/> with level 10, the encoder will automatically
        /// also use the container format, it is not necessary to use
        /// JxlEncoderUseContainer for those use cases.
        /// <br/><br/>
        /// By default this setting is disabled.
        /// <br/><br/>
        /// This setting can only be set at the beginning, before encoding starts.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="use_container"> true if the encoder should always output the JPEG XL
        /// container format, false to only output it when necessary.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
        /// otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderUseContainer(JxlEncoder* enc,
                                                           JXL_BOOL use_container);

        /// <summary>
        /// Configure the encoder to store JPEG reconstruction metadata in the JPEG XL
        /// container.
        /// <br/><br/>
        /// If this is set to true and a single JPEG frame is added, it will be
        /// possible to losslessly reconstruct the JPEG codestream.
        /// <br/><br/>
        /// This setting can only be set at the beginning, before encoding starts.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="store_jpeg_metadata"> true if the encoder should store JPEG metadata.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
        /// otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderStoreJPEGMetadata(JxlEncoder* enc, JXL_BOOL store_jpeg_metadata);

        /// <summary>
        /// Sets the feature level of the JPEG XL codestream. Valid values are 5 and
        /// 10. Keeping the default value of 5 is recommended for compatibility with all
        /// decoders.
        /// <br/><br/>
        /// Level 5: for end-user image delivery, this level is the most widely
        /// supported level by image decoders and the recommended level to use unless a
        /// level 10 feature is absolutely necessary. Supports a maximum resolution
        /// 268435456 pixels total with a maximum width or height of 262144 pixels,
        /// maximum 16-bit color channel depth, maximum 120 frames per second for
        /// animation, maximum ICC color profile size of 4 MiB, it allows all color
        /// models and extra channel types except CMYK and the JXL_CHANNEL_BLACK extra
        /// channel, and a maximum of 4 extra channels in addition to the 3 color
        /// channels. It also sets boundaries to certain internally used coding tools.
        /// <br/><br/>
        /// Level 10: this level removes or increases the bounds of most of the level
        /// 5 limitations, allows CMYK color and up to 32 bits per color channel, but
        /// may be less widely supported.
        /// <br/><br/>
        /// The default value is 5. To use level 10 features, the setting must be
        /// explicitly set to 10, the encoder will not automatically enable it. If
        /// incompatible parameters such as too high image resolution for the current
        /// level are set, the encoder will return an error. For internal coding tools,
        /// the encoder will only use those compatible with the level setting.
        /// <br/><br/>
        /// This setting can only be set at the beginning, before encoding starts.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="level"> the level value to set, must be 5 or 10.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
        /// otherwise.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderSetCodestreamLevel(JxlEncoder* enc,
                                                                 int level);

        /// <summary>
        /// Returns the codestream level required to support the currently configured
        /// settings and basic info. This function can only be used at the beginning,
        /// before encoding starts, but after setting basic info.
        /// <br/><br/>
        /// This does not support per-frame settings, only global configuration, such as
        /// the image dimensions, that are known at the time of writing the header of
        /// the JPEG XL file.
        /// <br/><br/>
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
        internal static extern int JxlEncoderGetRequiredCodestreamLevel([In]JxlEncoder* enc);

        /// <summary>
        /// Enables lossless encoding.
        /// <br/><br/>
        /// This is not an option like the others on itself, but rather while enabled it
        /// overrides a set of existing options (such as distance, modular mode and
        /// color transform) that enables bit-for-bit lossless encoding.
        /// <br/><br/>
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
        internal static extern JxlEncoderStatus JxlEncoderSetFrameLossless(
            JxlEncoderFrameSettings* frame_settings, JXL_BOOL lossless);

        /// <summary>
        /// DEPRECATED: use JxlEncoderSetFrameLossless instead.
        /// </summary>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderOptionsSetLossless(JxlEncoderFrameSettings*frame_settings, JXL_BOOL value);

        /// <summary>
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="effort"> the effort value to set.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
        /// otherwise.
        /// <br/><br/>
        /// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
        /// JXL_ENC_FRAME_SETTING_EFFORT, effort) instead.</returns>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderOptionsSetEffort(JxlEncoderFrameSettings* frame_settings, int effort);

        /// <summary>
        /// </summary>
        /// <param name="frame_settings"> set of options and metadata for this frame. Also
        /// includes reference to the encoder object.</param>
        /// <param name="tier"> the decoding speed tier to set.</param>
        /// <returns> JXL_ENC_SUCCESS if the operation was successful, JXL_ENC_ERROR
        /// otherwise.
        /// <br/><br/>
        /// DEPRECATED: use JxlEncoderFrameSettingsSetOption(frame_settings,
        /// JXL_ENC_FRAME_SETTING_DECODING_SPEED, tier) instead.</returns>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus JxlEncoderOptionsSetDecodingSpeed(
            JxlEncoderFrameSettings* frame_settings, int tier);

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
        internal static extern JxlEncoderStatus JxlEncoderSetFrameDistance(
            JxlEncoderFrameSettings* frame_settings, float distance);

        /// <summary>
        /// DEPRECATED: use JxlEncoderSetFrameDistance instead.
        /// </summary>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderStatus
        JxlEncoderOptionsSetDistance(JxlEncoderFrameSettings* frame_settings, float distance);

        /// <summary>
        /// Create a new set of encoder options, with all values initially copied from
        /// the <tt>source</tt> options, or set to default if <tt>source</tt> is NULL.
        /// <br/><br/>
        /// The returned pointer is an opaque struct tied to the encoder and it will be
        /// deallocated by the encoder when JxlEncoderDestroy() is called. For functions
        /// taking both a <see cref="JxlEncoder"/> and a <see cref="JxlEncoderFrameSettings"/>, only
        /// JxlEncoderFrameSettings created with this function for the same encoder
        /// instance can be used.
        /// </summary>
        /// <param name="enc"> encoder object.</param>
        /// <param name="source"> source options to copy initial values from, or NULL to get
        /// defaults initialized to defaults.</param>
        /// <returns> the opaque struct pointer identifying a new set of encoder options.</returns>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderFrameSettings* JxlEncoderFrameSettingsCreate(
            JxlEncoder* enc, [In]JxlEncoderFrameSettings* source);

        /// <summary>
        /// DEPRECATED: use JxlEncoderFrameSettingsCreate instead.
        /// </summary>
        [Obsolete]
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern JxlEncoderFrameSettings* JxlEncoderOptionsCreate(
            JxlEncoder* encoder, [In]JxlEncoderFrameSettings* frame_settings);

        /// <summary>
        /// Sets a color encoding to be sRGB.
        /// </summary>
        /// <param name="color_encoding"> color encoding instance.</param>
        /// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlColorEncodingSetToSRGB(JxlColorEncoding* color_encoding,
                                                  JXL_BOOL is_gray);

        /// <summary>
        /// Sets a color encoding to be linear sRGB.
        /// </summary>
        /// <param name="color_encoding"> color encoding instance.</param>
        /// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
        [DllImport("libjxl.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        internal static extern void JxlColorEncodingSetToLinearSRGB(
            JxlColorEncoding* color_encoding, JXL_BOOL is_gray);

    }
}
