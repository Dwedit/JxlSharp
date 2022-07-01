using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
	using UIntPtr = UIntPtr;
	using size_t = UIntPtr;

	internal static unsafe partial class UnsafeNativeJxl
	{
		/// <summary>
		/// JPEG XL signature identification.
		/// <br /><br />
		/// Checks if the passed buffer contains a valid JPEG XL signature. The passed 
		/// <tt>buf</tt> of size
		/// <tt>size</tt> doesn't need to be a full image, only the beginning of the file.
		/// </summary>
		/// <returns> a flag indicating if a JPEG XL signature was found and what type.
		/// - <see cref="F:JxlSharp.UnsafeNativeJxl.JxlSignature.JXL_SIG_NOT_ENOUGH_BYTES" /> if not enough bytes were passed to
		/// determine if a valid signature is there.
		/// - <see cref="F:JxlSharp.UnsafeNativeJxl.JxlSignature.JXL_SIG_INVALID" /> if no valid signature found for JPEG XL decoding.
		/// - <see cref="F:JxlSharp.UnsafeNativeJxl.JxlSignature.JXL_SIG_CODESTREAM" /> if a valid JPEG XL codestream signature was
		/// found.
		/// - <see cref="F:JxlSharp.UnsafeNativeJxl.JxlSignature.JXL_SIG_CONTAINER" /> if a valid JPEG XL container signature was found.</returns>
		internal static JxlSignature JxlSignatureCheck(byte[] buf)
		{
			fixed (byte* pBuf = buf)
			{
				return JxlSignatureCheck(pBuf, (size_t)buf.Length);
			}
		}

		/// <summary>
		/// Sets a color encoding to be sRGB.
		/// </summary>
		/// <param name="color_encoding"> color encoding instance.</param>
		/// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
		internal static unsafe void JxlColorEncodingSetToSRGB(out JxlColorEncoding color_encoding, bool is_gray)
		{
			fixed (JxlColorEncoding* pColorEncoding = &color_encoding)
			{
				UnsafeNativeJxl.JxlColorEncodingSetToSRGB(pColorEncoding, Convert.ToInt32(is_gray));
			}
		}

		/// <summary>
		/// Sets a color encoding to be linear sRGB.
		/// </summary>
		/// <param name="color_encoding"> color encoding instance.</param>
		/// <param name="is_gray"> whether the color encoding should be gray scale or color.</param>
		internal static unsafe void JxlColorEncodingSetToLinearSRGB(out JxlColorEncoding color_encoding, bool is_gray)
		{
			fixed (JxlColorEncoding* pColorEncoding = &color_encoding)
			{
				UnsafeNativeJxl.JxlColorEncodingSetToLinearSRGB(pColorEncoding, Convert.ToInt32(is_gray));
			}
		}

		/// <summary>
		/// Initializes a JxlBasicInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to an 8-bit RGB image, no alpha or any
		/// other extra channels.
		/// </summary>
		/// <param name="info"> global image metadata. Object owned by the caller.</param>
		internal static void InitBasicInfo(out JxlBasicInfo info)
		{
			fixed (JxlBasicInfo* pInfo = &info)
			{
				UnsafeNativeJxl.JxlEncoderInitBasicInfo(pInfo);
			}
		}

		/// <summary>
		/// Initializes a JxlFrameHeader struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to a frame with no animation duration and the
		/// 'replace' blend mode. After using this function, For animation duration must
		/// be set, for composite still blend settings must be set.
		/// </summary>
		/// <param name="frame_header"> frame metadata. Object owned by the caller.</param>
		internal static void InitFrameHeader(out JxlFrameHeader frame_header)
		{
			fixed (JxlFrameHeader* pFrameHeader = &frame_header)
			{
				UnsafeNativeJxl.JxlEncoderInitFrameHeader(pFrameHeader);
			}
		}

		/// <summary>
		/// Initializes a JxlBlendInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// </summary>
		/// <param name="blend_info"> blending info. Object owned by the caller.</param>
		internal static void InitBlendInfo(out JxlBlendInfo blend_info)
		{
			fixed (JxlBlendInfo* pBlendInfo = &blend_info)
			{
				UnsafeNativeJxl.JxlEncoderInitBlendInfo(pBlendInfo);
			}
		}

		/// <summary>
		/// Initializes a JxlExtraChannelInfo struct to default values.
		/// For forwards-compatibility, this function has to be called before values
		/// are assigned to the struct fields.
		/// The default values correspond to an 8-bit channel of the provided type.
		/// </summary>
		/// <param name="type"> type of the extra channel.</param>
		/// <param name="info"> global extra channel metadata. Object owned by the caller and its
		/// contents are copied internally.</param>
		internal static void InitExtraChannelInfo(JxlExtraChannelType type, out JxlExtraChannelInfo info)
		{
			fixed (JxlExtraChannelInfo* pInfo = &info)
			{
				UnsafeNativeJxl.JxlEncoderInitExtraChannelInfo(type, pInfo);
			}
		}


	}
}
