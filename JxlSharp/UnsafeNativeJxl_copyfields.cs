using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
    using UIntPtr = UIntPtr;
    using size_t = UIntPtr;
	internal static partial class UnsafeNativeJxl
	{
        internal static class CopyFields
        {
            internal static void ReadFromPublic(out UnsafeNativeJxl.JxlColorEncoding obj, JxlSharp.JxlColorEncoding colorEncoding)
            {
                obj.color_space = (UnsafeNativeJxl.JxlColorSpace)colorEncoding.ColorSpace;
                obj.white_point = (UnsafeNativeJxl.JxlWhitePoint)colorEncoding.WhitePoint;
                obj.white_point_xy.x = colorEncoding.WhitePointXY.X;
                obj.white_point_xy.y = colorEncoding.WhitePointXY.Y;
                obj.primaries = (UnsafeNativeJxl.JxlPrimaries)colorEncoding.Primaries;
                obj.primaries_red_xy.x = colorEncoding.PrimariesRedXY.X;
                obj.primaries_red_xy.y = colorEncoding.PrimariesRedXY.Y;
                obj.primaries_green_xy.x = colorEncoding.PrimariesGreenXY.X;
                obj.primaries_green_xy.y = colorEncoding.PrimariesGreenXY.Y;
                obj.primaries_blue_xy.x = colorEncoding.PrimariesBlueXY.X;
                obj.primaries_blue_xy.y = colorEncoding.PrimariesBlueXY.Y;
                obj.transfer_function = (UnsafeNativeJxl.JxlTransferFunction)colorEncoding.TransferFunction;
                obj.gamma = colorEncoding.Gamma;
                obj.rendering_intent = (UnsafeNativeJxl.JxlRenderingIntent)colorEncoding.RenderingIntent;
            }

            internal static void WriteToPublic(ref UnsafeNativeJxl.JxlColorEncoding obj, JxlSharp.JxlColorEncoding colorEncoding)
            {
                colorEncoding.ColorSpace = (JxlSharp.JxlColorSpace)obj.color_space;
                colorEncoding.WhitePoint = (JxlSharp.JxlWhitePoint)obj.white_point;
                colorEncoding.WhitePointXY = new JxlSharp.XYValue() { X = obj.white_point_xy.x, Y = obj.white_point_xy.y };
                colorEncoding.Primaries = (JxlSharp.JxlPrimaries)obj.primaries;
                colorEncoding.PrimariesRedXY = new JxlSharp.XYValue(){ X = obj.primaries_red_xy.x, Y = obj.primaries_red_xy.y };
                colorEncoding.PrimariesGreenXY = new JxlSharp.XYValue() { X = obj.primaries_green_xy.x, Y = obj.primaries_green_xy.y };
                colorEncoding.PrimariesBlueXY = new JxlSharp.XYValue() { X = obj.primaries_blue_xy.x, Y = obj.primaries_blue_xy.y };
                colorEncoding.TransferFunction = (JxlSharp.JxlTransferFunction)obj.transfer_function;
                colorEncoding.Gamma = obj.gamma;
                colorEncoding.RenderingIntent = (JxlSharp.JxlRenderingIntent)obj.rendering_intent;
            }

            internal static void ReadFromPublic(ref UnsafeNativeJxl.JxlBasicInfo obj, JxlSharp.JxlBasicInfo basicInfo)
            {
                obj.have_container = Convert.ToInt32(basicInfo.HaveContainer);
                obj.xsize = (uint)basicInfo.Width;
                obj.ysize = (uint)basicInfo.Height;
                obj.bits_per_sample = (uint)basicInfo.BitsPerSample;
                obj.exponent_bits_per_sample = (uint)basicInfo.ExponentBitsPerSample;
                obj.intensity_target = basicInfo.IntensityTarget;
                obj.min_nits = basicInfo.MinNits;
                obj.relative_to_max_display = Convert.ToInt32(basicInfo.RelativeToMaxDisplay);
                obj.linear_below = basicInfo.LinearBelow;
                obj.uses_original_profile = Convert.ToInt32(basicInfo.UsesOriginalProfile);
                obj.have_preview = Convert.ToInt32(basicInfo.HavePreview);
                obj.have_animation = Convert.ToInt32(basicInfo.HaveAnimation);
                obj.orientation = (UnsafeNativeJxl.JxlOrientation)basicInfo.Orientation;
                obj.num_color_channels = (uint)basicInfo.NumColorChannels;
                obj.num_extra_channels = (uint)basicInfo.NumExtraChannels;
                obj.alpha_bits = (uint)basicInfo.AlphaBits;
                obj.alpha_exponent_bits = (uint)basicInfo.AlphaExponentBits;
                obj.alpha_premultiplied = Convert.ToInt32(basicInfo.AlphaPremultiplied);
                obj.preview.xsize = (uint)basicInfo.Preview.Width;
                obj.preview.ysize = (uint)basicInfo.Preview.Height;
                obj.animation.tps_numerator = basicInfo.Animation.TpsNumerator;
                obj.animation.tps_denominator = basicInfo.Animation.TpsDenominator;
                obj.animation.num_loops = (uint)basicInfo.Animation.NumLoops;
                obj.animation.have_timecodes = Convert.ToInt32(basicInfo.Animation.HaveTimecodes);
                obj.intrinsic_xsize = (uint)basicInfo.IntrinsicWidth;
                obj.intrinsic_ysize = (uint)basicInfo.IntrinsicHeight;
            }

            internal static void WriteToPublic(ref UnsafeNativeJxl.JxlBasicInfo obj, JxlSharp.JxlBasicInfo basicInfo)
            {
                basicInfo.HaveContainer = Convert.ToBoolean(obj.have_container);
                basicInfo.Width = (int)obj.xsize;
                basicInfo.Height = (int)obj.ysize;
                basicInfo.BitsPerSample = (int)obj.bits_per_sample;
                basicInfo.ExponentBitsPerSample = (int)obj.exponent_bits_per_sample;
                basicInfo.IntensityTarget = obj.intensity_target;
                basicInfo.MinNits = obj.min_nits;
                basicInfo.RelativeToMaxDisplay = Convert.ToBoolean(obj.relative_to_max_display);
                basicInfo.LinearBelow = obj.linear_below;
                basicInfo.UsesOriginalProfile = Convert.ToBoolean(obj.uses_original_profile);
                basicInfo.HavePreview = Convert.ToBoolean(obj.have_preview);
                basicInfo.HaveAnimation = Convert.ToBoolean(obj.have_animation);
                basicInfo.Orientation = (JxlSharp.JxlOrientation)obj.orientation;
                basicInfo.NumColorChannels = (int)obj.num_color_channels;
                basicInfo.NumExtraChannels = (int)obj.num_extra_channels;
                basicInfo.AlphaBits = (int)obj.alpha_bits;
                basicInfo.AlphaExponentBits = (int)obj.alpha_exponent_bits;
                basicInfo.AlphaPremultiplied = Convert.ToBoolean(obj.alpha_premultiplied);
                var preview = basicInfo.Preview;
                preview.Width = (int)obj.preview.xsize;
                preview.Height = (int)obj.preview.ysize;
                basicInfo.Preview = preview;
                var animation = basicInfo.Animation;
                animation.TpsNumerator = obj.animation.tps_numerator;
                animation.TpsDenominator = obj.animation.tps_denominator;
                animation.NumLoops = (int)obj.animation.num_loops;
                animation.HaveTimecodes = Convert.ToBoolean(obj.animation.have_timecodes);
                basicInfo.Animation = animation;
                basicInfo.IntrinsicWidth = (int)obj.intrinsic_xsize;
                basicInfo.IntrinsicHeight = (int)obj.intrinsic_ysize;
            }

            internal static void ReadFromPublic(out UnsafeNativeJxl.JxlPixelFormat obj, JxlSharp.JxlPixelFormat format)
            {
                obj.num_channels = (uint)format.NumChannels;
                obj.data_type = (JxlDataType)format.DataType;
                obj.endianness = (JxlEndianness)format.Endianness;
                obj.align = (size_t)format.Align;
            }

            internal static void WriteToPublic(ref UnsafeNativeJxl.JxlPixelFormat obj, JxlSharp.JxlPixelFormat format)
            {
                format.Align = (int)obj.align;
                format.DataType = (JxlSharp.JxlDataType)obj.data_type;
                format.Endianness = (JxlSharp.JxlEndianness)obj.endianness;
                format.NumChannels = (int)obj.num_channels;
            }

            internal static void ReadFromPublic(out UnsafeNativeJxl.JxlExtraChannelInfo obj, JxlSharp.JxlExtraChannelInfo info)
            {
                obj.alpha_premultiplied = Convert.ToInt32(info.AlphaPremultiplied);
                obj.bits_per_sample = (uint)info.BitsPerSample;
                obj.cfa_channel = (uint)info.CfaChannel;
                obj.dim_shift = (uint)info.DimShift;
                obj.exponent_bits_per_sample = (uint)info.ExponentBitsPerSample;
                obj.name_length = (uint)info.NameLength;
                obj.spot_color = new UnsafeNativeJxl.RGBAFloat() { r = info.SpotColor.R, g = info.SpotColor.G, b = info.SpotColor.B, a = info.SpotColor.A };
                obj.type = (UnsafeNativeJxl.JxlExtraChannelType)info.Type;
            }

            internal static void WriteToPublic(ref UnsafeNativeJxl.JxlExtraChannelInfo obj, JxlSharp.JxlExtraChannelInfo info)
            {
                info.AlphaPremultiplied = Convert.ToBoolean(obj.alpha_premultiplied);
                info.BitsPerSample = (int)obj.bits_per_sample;
                info.CfaChannel = (int)obj.cfa_channel;
                info.DimShift = (int)obj.dim_shift;
                info.ExponentBitsPerSample = (int)obj.exponent_bits_per_sample;
                info.NameLength = (int)obj.name_length;
                info.SpotColor = new JxlSharp.RGBAFloat() { R = obj.spot_color.r, G = obj.spot_color.g, B = obj.spot_color.b, A = obj.spot_color.a };
                info.Type = (JxlSharp.JxlExtraChannelType)obj.type;
            }

            internal static void WriteToPublic(ref UnsafeNativeJxl.JxlFrameHeader obj, JxlSharp.JxlFrameHeader header)
            {
                header.Duration = obj.duration;
                header.Timecode = obj.timecode;
                //header.NameLength = (int)obj.name_length;
                header.IsLast = Convert.ToBoolean(obj.is_last);
                header.LayerInfo.HaveCrop = Convert.ToBoolean(obj.layer_info.have_crop);
                header.LayerInfo.CropX0 = obj.layer_info.crop_x0;
                header.LayerInfo.CropY0 = obj.layer_info.crop_y0;
                header.LayerInfo.Width = (int)obj.layer_info.xsize;
                header.LayerInfo.Height = (int)obj.layer_info.ysize;
                header.LayerInfo.BlendInfo.BlendMode = (JxlSharp.JxlBlendMode)obj.layer_info.blend_info.blendmode;
                header.LayerInfo.BlendInfo.Source = (int)obj.layer_info.blend_info.source;
                header.LayerInfo.BlendInfo.Alpha = (int)obj.layer_info.blend_info.alpha;
                header.LayerInfo.BlendInfo.Clamp = Convert.ToBoolean(obj.layer_info.blend_info.clamp);
                header.LayerInfo.SaveAsReference = (int)obj.layer_info.save_as_reference;
            }

            internal static void ReadFromPublic(out UnsafeNativeJxl.JxlFrameHeader obj, JxlSharp.JxlFrameHeader header)
            {
                obj.duration = header.Duration;
                obj.timecode = header.Timecode;
                obj.name_length = (uint)header.Name.Length;
                obj.is_last = Convert.ToInt32(header.IsLast);
                obj.layer_info.have_crop = Convert.ToInt32(header.LayerInfo.HaveCrop);
                obj.layer_info.crop_x0 = header.LayerInfo.CropX0;
                obj.layer_info.crop_y0 = header.LayerInfo.CropY0;
                obj.layer_info.xsize = (uint)header.LayerInfo.Width;
                obj.layer_info.ysize = (uint)header.LayerInfo.Height;
                obj.layer_info.blend_info.blendmode = (UnsafeNativeJxl.JxlBlendMode)header.LayerInfo.BlendInfo.BlendMode;
                obj.layer_info.blend_info.source = (uint)header.LayerInfo.BlendInfo.Source;
                obj.layer_info.blend_info.alpha = (uint)header.LayerInfo.BlendInfo.Alpha;
                obj.layer_info.blend_info.clamp = Convert.ToInt32(header.LayerInfo.BlendInfo.Clamp);
                obj.layer_info.save_as_reference = (uint)header.LayerInfo.SaveAsReference;
            }

            internal static void ReadFromPublic(out JxlBlendInfo obj, ref JxlSharp.JxlBlendInfo blendInfo)
            {
                obj.blendmode = (UnsafeNativeJxl.JxlBlendMode)blendInfo.BlendMode;
                obj.source = (uint)blendInfo.Source;
                obj.alpha = (uint)blendInfo.Alpha;
                obj.clamp = Convert.ToInt32(blendInfo.Clamp);
            }

            internal static void WriteToPublic(ref JxlBlendInfo obj, out JxlSharp.JxlBlendInfo blendInfo)
            {
                blendInfo.BlendMode = (JxlSharp.JxlBlendMode)obj.blendmode;
                blendInfo.Source = (int)obj.source;
                blendInfo.Alpha = (int)obj.alpha;
                blendInfo.Clamp = Convert.ToBoolean(obj.clamp);
            }
		}

    }
}
