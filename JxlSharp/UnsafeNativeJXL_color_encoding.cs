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
        /// Color space of the image data. 
        /// </summary>
        internal enum JxlColorSpace
        {
            /// <summary>
            /// Tristimulus RGB 
            /// </summary>
            JXL_COLOR_SPACE_RGB,
            /// <summary>
            /// Luminance based, the primaries in JxlColorEncoding must be ignored. This
            /// value implies that num_color_channels in JxlBasicInfo is 1, any other value
            /// implies num_color_channels is 3. 
            /// </summary>
            JXL_COLOR_SPACE_GRAY,
            /// <summary>
            /// XYB (opsin) color space 
            /// </summary>
            JXL_COLOR_SPACE_XYB,
            /// <summary>
            /// None of the other table entries describe the color space appropriately 
            /// </summary>
            JXL_COLOR_SPACE_UNKNOWN,
        }

        /// <summary>
        /// Built-in whitepoints for color encoding. When decoding, the numerical xy
        /// whitepoint value can be read from the JxlColorEncoding white_point field
        /// regardless of the enum value. When encoding, enum values except
        /// JXL_WHITE_POINT_CUSTOM override the numerical fields. Some enum values match
        /// a subset of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)), however the
        /// white point and RGB primaries are separate enums here.
        /// </summary>
        internal enum JxlWhitePoint
        {
            /// <summary>
            /// CIE Standard Illuminant D65: 0.3127, 0.3290 
            /// </summary>
            JXL_WHITE_POINT_D65 = 1,
            /// <summary>
            /// White point must be read from the JxlColorEncoding white_point field, or
            /// as ICC profile. This enum value is not an exact match of the corresponding
            /// CICP value. 
            /// </summary>
            JXL_WHITE_POINT_CUSTOM = 2,
            /// <summary>
            /// CIE Standard Illuminant E (equal-energy): 1/3, 1/3 
            /// </summary>
            JXL_WHITE_POINT_E = 10,
            /// <summary>
            /// DCI-P3 from SMPTE RP 431-2: 0.314, 0.351 
            /// </summary>
            JXL_WHITE_POINT_DCI = 11,
        }

        /// <summary>
        /// Built-in primaries for color encoding. When decoding, the primaries can be
        /// read from the JxlColorEncoding primaries_red_xy, primaries_green_xy and
        /// primaries_blue_xy fields regardless of the enum value. When encoding, the
        /// enum values except JXL_PRIMARIES_CUSTOM override the numerical fields. Some
        /// enum values match a subset of CICP (Rec. ITU-T H.273 | ISO/IEC
        /// 23091-2:2019(E)), however the white point and RGB primaries are separate
        /// enums here.
        /// </summary>
        internal enum JxlPrimaries
        {
            /// <summary>
            /// The CIE xy values of the red, green and blue primaries are: 0.639998686,
            /// 0.330010138; 0.300003784, 0.600003357; 0.150002046, 0.059997204 
            /// </summary>
            JXL_PRIMARIES_SRGB = 1,
            /// <summary>
            /// Primaries must be read from the JxlColorEncoding primaries_red_xy,
            /// primaries_green_xy and primaries_blue_xy fields, or as ICC profile. This
            /// enum value is not an exact match of the corresponding CICP value. 
            /// </summary>
            JXL_PRIMARIES_CUSTOM = 2,
            /// <summary>
            /// As specified in Rec. ITU-R BT.2100-1 
            /// </summary>
            JXL_PRIMARIES_2100 = 9,
            /// <summary>
            /// As specified in SMPTE RP 431-2 
            /// </summary>
            JXL_PRIMARIES_P3 = 11,
        }

        /// <summary>
        /// Built-in transfer functions for color encoding. Enum values match a subset
        /// of CICP (Rec. ITU-T H.273 | ISO/IEC 23091-2:2019(E)) unless specified
        /// otherwise. 
        /// </summary>
        internal enum JxlTransferFunction
        {
            /// <summary>
            /// As specified in SMPTE RP 431-2 
            /// </summary>
            JXL_TRANSFER_FUNCTION_709 = 1,
            /// <summary>
            /// None of the other table entries describe the transfer function. 
            /// </summary>
            JXL_TRANSFER_FUNCTION_UNKNOWN = 2,
            /// <summary>
            /// The gamma exponent is 1 
            /// </summary>
            JXL_TRANSFER_FUNCTION_LINEAR = 8,
            /// <summary>
            /// As specified in IEC 61966-2-1 sRGB 
            /// </summary>
            JXL_TRANSFER_FUNCTION_SRGB = 13,
            /// <summary>
            /// As specified in SMPTE ST 2084 
            /// </summary>
            JXL_TRANSFER_FUNCTION_PQ = 16,
            /// <summary>
            /// As specified in SMPTE ST 428-1 
            /// </summary>
            JXL_TRANSFER_FUNCTION_DCI = 17,
            /// <summary>
            /// As specified in Rec. ITU-R BT.2100-1 (HLG) 
            /// </summary>
            JXL_TRANSFER_FUNCTION_HLG = 18,
            /// <summary>
            /// Transfer function follows power law given by the gamma value in
            /// JxlColorEncoding. Not a CICP value. 
            /// </summary>
            JXL_TRANSFER_FUNCTION_GAMMA = 65535,
        }

        /// <summary>
        /// Renderig intent for color encoding, as specified in ISO 15076-1:2010 
        /// </summary>
        internal enum JxlRenderingIntent
        {
            /// <summary>
            /// vendor-specific 
            /// </summary>
            JXL_RENDERING_INTENT_PERCEPTUAL = 0,
            /// <summary>
            /// media-relative 
            /// </summary>
            JXL_RENDERING_INTENT_RELATIVE,
            /// <summary>
            /// vendor-specific 
            /// </summary>
            JXL_RENDERING_INTENT_SATURATION,
            /// <summary>
            /// ICC-absolute 
            /// </summary>
            JXL_RENDERING_INTENT_ABSOLUTE,
        }

        /// <summary>
        /// Color encoding of the image as structured information.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct JxlColorEncoding
        {
            /// <summary>
            /// Color space of the image data.
            /// </summary>
            public JxlColorSpace color_space;

            /// <summary>
            /// Built-in white point. If this value is JXL_WHITE_POINT_CUSTOM, must
            /// use the numerical whitepoint values from white_point_xy.
            /// </summary>
            public JxlWhitePoint white_point;

            /// <summary>
            /// Numerical whitepoint values in CIE xy space. 
            /// </summary>
            public fixed double white_point_xy[2];

            /// <summary>
            /// Built-in RGB primaries. If this value is JXL_PRIMARIES_CUSTOM, must
            /// use the numerical primaries values below. This field and the custom values
            /// below are unused and must be ignored if the color space is
            /// JXL_COLOR_SPACE_GRAY or JXL_COLOR_SPACE_XYB.
            /// </summary>
            public JxlPrimaries primaries;

            /// <summary>
            /// Numerical red primary values in CIE xy space. 
            /// </summary>
            public fixed double primaries_red_xy[2];

            /// <summary>
            /// Numerical green primary values in CIE xy space. 
            /// </summary>
            public fixed double primaries_green_xy[2];

            /// <summary>
            /// Numerical blue primary values in CIE xy space. 
            /// </summary>
            public fixed double primaries_blue_xy[2];

            /// <summary>
            /// Transfer function if have_gamma is 0 
            /// </summary>
            public JxlTransferFunction transfer_function;

            /// <summary>
            /// Gamma value used when transfer_function is JXL_TRANSFER_FUNCTION_GAMMA
            /// </summary>
            public double gamma;

            /// <summary>
            /// Rendering intent defined for the color profile. 
            /// </summary>
            public JxlRenderingIntent rendering_intent;

			public JxlColorEncoding(JxlSharp.JxlColorEncoding colorEncoding) : this()
			{
                this.color_space = (JxlColorSpace)colorEncoding.ColorSpace;
                this.white_point = (JxlWhitePoint)colorEncoding.WhitePoint;
                this.white_point_xy[0] = colorEncoding.WhitePointXY.X;
                this.white_point_xy[1] = colorEncoding.WhitePointXY.Y;
                this.primaries = (JxlPrimaries)colorEncoding.Primaries;
                this.primaries_red_xy[0] = colorEncoding.PrimariesRedXY.X;
                this.primaries_red_xy[1] = colorEncoding.PrimariesRedXY.Y;
                this.primaries_green_xy[0] = colorEncoding.PrimariesGreenXY.X;
                this.primaries_green_xy[1] = colorEncoding.PrimariesGreenXY.Y;
                this.primaries_blue_xy[0] = colorEncoding.PrimariesBlueXY.X;
                this.primaries_blue_xy[1] = colorEncoding.PrimariesBlueXY.Y;
                this.transfer_function = (JxlTransferFunction)colorEncoding.TransferFunction;
                this.gamma = colorEncoding.Gamma;
                this.rendering_intent = (JxlRenderingIntent)colorEncoding.RenderingIntent;
            }

            public void WriteToPublic(JxlSharp.JxlColorEncoding colorEncoding)
			{
                colorEncoding.ColorSpace = (JxlSharp.JxlColorSpace)this.color_space;
                colorEncoding.WhitePoint = (JxlSharp.JxlWhitePoint)this.white_point;
                colorEncoding.WhitePointXY = new XYValue(this.white_point_xy[0], this.white_point_xy[1]);
                colorEncoding.Primaries = (JxlSharp.JxlPrimaries)this.primaries;
                colorEncoding.PrimariesRedXY = new XYValue(this.primaries_red_xy[0], this.primaries_red_xy[1]);
                colorEncoding.PrimariesGreenXY = new XYValue(this.primaries_green_xy[0], this.primaries_green_xy[1]);
                colorEncoding.PrimariesBlueXY = new XYValue(this.primaries_blue_xy[0], this.primaries_blue_xy[1]);
                colorEncoding.TransferFunction = (JxlSharp.JxlTransferFunction)this.transfer_function;
                colorEncoding.Gamma = this.gamma;
                colorEncoding.RenderingIntent = (JxlSharp.JxlRenderingIntent)this.rendering_intent;
            }
        }
    }
}
