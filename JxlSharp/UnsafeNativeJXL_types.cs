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
            JXL_TYPE_FLOAT16 = 5,
        }

        /* DEPRECATED: bit-packed 1-bit data type. Use JXL_TYPE_UINT8 instead.
         */
        [Obsolete]
        const int JXL_TYPE_BOOLEAN = 1;

        /* DEPRECATED: uint32_t data type. Use JXL_TYPE_FLOAT instead.
         */
        [Obsolete]
        const int JXL_TYPE_UINT32 = 4;

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
            JXL_NATIVE_ENDIAN = 0,
            /// <summary>
            /// Force little endian 
            /// </summary>
            JXL_LITTLE_ENDIAN = 1,
            /// <summary>
            /// Force big endian 
            /// </summary>
            JXL_BIG_ENDIAN = 2,
        }

        /// <summary>
        /// Data type for the sample values per channel per pixel for the output buffer
        /// for pixels. This is not necessarily the same as the data type encoded in the
        /// codestream. The channels are interleaved per pixel. The pixels are
        /// organized row by row, left to right, top to bottom.
        /// TODO(lode): implement padding / alignment (row stride)
        /// TODO(lode): support different channel orders if needed (RGB, BGR, ...)
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
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
            public uint32_t num_channels;

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
            public size_t align;

			public JxlPixelFormat(JxlSharp.JxlPixelFormat format) : this()
			{
                this.num_channels = (uint32_t)format.NumChannels;
                this.data_type = (JxlDataType)format.DataType;
                this.endianness = (JxlEndianness)format.Endianness;
                this.align = (size_t)format.Align;
			}

			public void WriteToPublic(JxlSharp.JxlPixelFormat format)
			{
                format.Align = (int)this.align;
                format.DataType = (JxlSharp.JxlDataType)this.data_type;
                format.Endianness = (JxlSharp.JxlEndianness)this.endianness;
                format.NumChannels = (int)this.num_channels;
            }
        }

  //      /// <summary>
  //      /// Data type holding the 4-character type name of an ISOBMFF box.
  //      /// </summary>
  //      [StructLayout(LayoutKind.Sequential)]
  //      internal struct JxlBoxType
  //      {
  //          public fixed byte Contents[4];
  //          public JxlBoxType(string name) : this()
  //          {
  //              var bytes = Encoding.UTF8.GetBytes(name);
  //              for (int i = 0; i < bytes.Length && i < 4; i++)
  //              {
  //                  Contents[i] = bytes[i];
  //              }
  //          }
		//	public override string ToString()
		//	{
  //              int len;
  //              for (len = 0; len < 4; len++)
  //              {
  //                  if (Contents[len] == 0) break;
  //              }
  //              byte[] bytes = new byte[len];
  //              for (int i = 0; i < len; i++)
  //              {
  //                  bytes[i] = Contents[i];
  //              }
  //              //return Encoding.UTF8.GetString(bytes);
  //              return Encoding.UTF8.GetString(bytes, 0, len);
		//	}
		//}

        /// <summary>
        /// Types of progressive detail.
        /// Setting a progressive detail with value N implies all progressive details
        /// with smaller or equal value. Currently only the following level of
        /// progressive detail is implemented:
        /// - kDC (which implies kFrames)
        /// - kLastPasses (which implies kDC and kFrames)
        /// - kPasses (which implies kLastPasses, kDC and kFrames)
        /// </summary>
        internal enum JxlProgressiveDetail
        {
            // after completed kRegularFrames
            kFrames = 0,
            // after completed DC (1:8)
            kDC = 1,
            // after completed AC passes that are the last pass for their resolution
            // target.
            kLastPasses = 2,
            // after completed AC passes that are not the last pass for their resolution
            // target.
            kPasses = 3,
            // during DC frame when lower resolution are completed (1:32, 1:16)
            kDCProgressive = 4,
            // after completed groups
            kDCGroups = 5,
            // after completed groups
            kGroups = 6,
        }
    }
}
