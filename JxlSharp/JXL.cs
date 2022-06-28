using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
	/// <summary>
	/// A simple static class for working with JXL files.
	/// </summary>
	public static class JXL
	{
		/// <summary>
		/// Returns the number of bytes per pixel for the built-in PixelFormat type
		/// </summary>
		/// <param name="pixelFormat">The GDI+ pixel format</param>
		/// <returns>The number of bytes per pixel for that pixel format</returns>
		private static int GetBytesPerPixel(PixelFormat pixelFormat)
		{
			switch (pixelFormat)
			{
				case PixelFormat.Format16bppArgb1555:
				case PixelFormat.Format16bppGrayScale:
				case PixelFormat.Format16bppRgb555:
				case PixelFormat.Format16bppRgb565:
					return 2;
				case PixelFormat.Format64bppArgb:
				case PixelFormat.Format64bppPArgb:
					return 8;
				case PixelFormat.Format48bppRgb:
					return 6;
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppRgb:
					return 4;
				case PixelFormat.Format24bppRgb:
					return 3;
				case PixelFormat.Format8bppIndexed:
					return 1;
				case PixelFormat.Format1bppIndexed:
				case PixelFormat.Format4bppIndexed:
					throw new NotSupportedException();
				default:
					throw new NotSupportedException();
			}
		}

		//[ThreadStatic]
		//static JxlDecoder _threadJxlDecoder;
		//static JxlDecoder threadJxlDecoder
		//{
		//	get
		//	{
		//		if (_threadJxlDecoder == null)
		//		{
		//			_threadJxlDecoder = new JxlDecoder();
		//		}
		//		return _threadJxlDecoder;
		//	}
		//}

		/// <summary>
		/// Loads a JXL file and returns a Bitmap.
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns>Returns a bitmap, or returns null on failure.</returns>
		public static Bitmap LoadImage(string fileName)
		{
			return LoadImage(File.ReadAllBytes(fileName));
		}

		/// <summary>
		/// Returns the basic info for a JXL file
		/// </summary>
		/// <param name="data">The bytes of the JXL file (can be partial data)</param>
		/// <param name="canTranscodeToJpeg">Set to true if the image contains JPEG reconstruction data</param>
		/// <returns>A JxlBasicInfo object describing the image</returns>
		public static JxlBasicInfo GetBasicInfo(byte[] data, out bool canTranscodeToJpeg)
		{
			using (var jxlDecoder = new JxlDecoder())
			{
				JxlBasicInfo basicInfo = null;
				canTranscodeToJpeg = false;
				jxlDecoder.SetInput(data);
				jxlDecoder.SubscribeEvents(JxlDecoderStatus.BasicInfo | JxlDecoderStatus.JpegReconstruction | JxlDecoderStatus.Frame);

				while (true)
				{
					var status = jxlDecoder.ProcessInput();
					if (status == JxlDecoderStatus.BasicInfo)
					{
						status = jxlDecoder.GetBasicInfo(out basicInfo);
						if (status != JxlDecoderStatus.Success)
						{
							return null;
						}
					}
					else if (status == JxlDecoderStatus.JpegReconstruction)
					{
						canTranscodeToJpeg = true;
					}
					else if (status == JxlDecoderStatus.Frame)
					{
						return basicInfo;
					}
					else if (status == JxlDecoderStatus.Success)
					{
						return basicInfo;
					}
					else if (status >= JxlDecoderStatus.Error && status < JxlDecoderStatus.BasicInfo)
					{
						return null;
					}

					else if (status < JxlDecoderStatus.BasicInfo)
					{
						return basicInfo;
					}
				}
			}
		}

		private static void BgrSwap(int width, int height, int bytesPerPixel, IntPtr scan0, int stride)
		{
			unsafe
			{
				if (bytesPerPixel == 3)
				{
					for (int y = 0; y < height; y++)
					{
						byte* p = (byte*)scan0 + stride * y;
						for (int x = 0; x < width; x++)
						{
							byte r = p[2];
							byte b = p[0];
							p[0] = r;
							p[2] = b;
							p += 3;
						}
					}
				}
				else if (bytesPerPixel == 4)
				{
					for (int y = 0; y < height; y++)
					{
						byte* p = (byte*)scan0 + stride * y;
						for (int x = 0; x < width; x++)
						{
							byte r = p[2];
							byte b = p[0];
							p[0] = r;
							p[2] = b;
							p += 4;
						}
					}
				}
			}
		}

		private static void BgrSwap(BitmapData bitmapData)
		{
			int bytesPerPixel = 4;
			switch (bitmapData.PixelFormat)
			{
				case PixelFormat.Format32bppArgb:
				case PixelFormat.Format32bppPArgb:
				case PixelFormat.Format32bppRgb:
					bytesPerPixel = 4;
					break;
				case PixelFormat.Format24bppRgb:
					bytesPerPixel = 3;
					break;
				default:
					return;
			}
			BgrSwap(bitmapData.Width, bitmapData.Height, bytesPerPixel, bitmapData.Scan0, bitmapData.Stride);
		}
		
		private static void BgrSwap(Bitmap bitmap)
		{
			BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			try
			{
				BgrSwap(bitmapData);
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}
		
		private static void SetGrayscalePalette(Bitmap bitmap)
		{
			var palette = bitmap.Palette;
			for (int i = 0; i < 256; i++)
			{
				palette.Entries[i] = Color.FromArgb(i, i, i);
			}
			bitmap.Palette = palette;
		}
		
		/// <summary>
		/// Loads a JXL image into a BitmapData object (SRGB or grayscale only, Image dimensions must match the file)
		/// </summary>
		/// <param name="data">The byte data for the JXL file</param>
		/// <param name="bitmapData">A BitmapData object (from Bitmap.LockBits)</param>
		/// <returns></returns>
		public static bool LoadImageIntoBitmap(byte[] data, BitmapData bitmapData)
		{
			return LoadImageIntoMemory(data, bitmapData.Width, bitmapData.Height, GetBytesPerPixel(bitmapData.PixelFormat), bitmapData.Scan0, bitmapData.Stride, true);
		}

		/// <summary>
		/// Loads a JXL image into a Bitmap object (SRGB or grayscale only, Image dimensions must match the file)
		/// </summary>
		/// <param name="data">The byte data for the JXL file</param>
		/// <param name="bitmap">A Bitmap object</param>
		/// <returns></returns>
		public static bool LoadImageIntoBitmap(byte[] data, Bitmap bitmap)
		{
			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			if (bitmapData.Stride < 0)
			{
				throw new NotSupportedException("Stride can not be negative");
			}
			try
			{
				bool okay = LoadImageIntoBitmap(data, bitmapData);
				if (okay)
				{
					if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
					{
						SetGrayscalePalette(bitmap);
					}
				}
				return okay;
			}
			finally
			{
				bitmap.UnlockBits(bitmapData);
			}
		}

		/// <summary>
		/// Loads a JXL file into a locked image buffer  (SRGB or grayscale only, Image dimensions must match the file)
		/// </summary>
		/// <param name="data">The byte data for the JXL file</param>
		/// <param name="width">Width of the buffer (must match the file)</param>
		/// <param name="height">Height of the buffer (must match the file)</param>
		/// <param name="bytesPerPixel">Bytes per pixel (1 = grayscale, 3 = RGB, 4 = RGBA)</param>
		/// <param name="scan0">Pointer to a locked scanline buffer</param>
		/// <param name="stride">Distance between scanlines in the buffer (must be positive)</param>
		/// <param name="doBgrSwap">If true, swaps the red and blue channel.  Required for GDI/GDI+ bitmaps which use BGR byte order.</param>
		/// <returns>True if the image was successfully loaded, otherwise false</returns>
		public static bool LoadImageIntoMemory(byte[] data, int width, int height, int bytesPerPixel, IntPtr scan0, int stride, bool doBgrSwap)
		{
			if (stride < 0) throw new NotSupportedException("Stride can not be negative");
			if (bytesPerPixel < 0 || bytesPerPixel > 4) throw new NotSupportedException("bytesPerPixel must be between 1 and 4");

			JxlBasicInfo basicInfo;
			using (var jxlDecoder = new JxlDecoder())
			{
				jxlDecoder.SetInput(data);
				jxlDecoder.SubscribeEvents(JxlDecoderStatus.BasicInfo | JxlDecoderStatus.Frame | JxlDecoderStatus.FullImage);
				while (true)
				{
					var status = jxlDecoder.ProcessInput();
					if (status == JxlDecoderStatus.BasicInfo)
					{
						status = jxlDecoder.GetBasicInfo(out basicInfo);
						if (status == JxlDecoderStatus.Success)
						{
							if (width != basicInfo.Width || height != basicInfo.Height)
							{
								return false;
							}
						}
						else
						{
							return false;
						}

					}
					else if (status == JxlDecoderStatus.Frame)
					{
						//PixelFormat bitmapPixelFormat = PixelFormat.Format32bppArgb;
						JxlPixelFormat pixelFormat = new JxlPixelFormat();
						pixelFormat.DataType = JxlDataType.UInt8;
						pixelFormat.Endianness = JxlEndianness.NativeEndian;
						pixelFormat.NumChannels = bytesPerPixel;
						
						pixelFormat.Align = stride;
						status = jxlDecoder.SetImageOutBuffer(pixelFormat, scan0, stride * height);
						if (status != JxlDecoderStatus.Success)
						{
							return false;
						}
						status = jxlDecoder.ProcessInput();
						if (status > JxlDecoderStatus.Success && status < JxlDecoderStatus.BasicInfo)
						{
							return false;
						}
						if (doBgrSwap && bytesPerPixel >= 3)
						{
							BgrSwap(width, height, bytesPerPixel, scan0, stride);
						}
						return true;
					}
					else if (status == JxlDecoderStatus.FullImage)
					{
						0.GetHashCode();
					}
					else if (status == JxlDecoderStatus.Success)
					{
						return true;
					}
					else if (status > JxlDecoderStatus.Success && status < JxlDecoderStatus.BasicInfo)
					{
						return false;
					}
				}
			}



			//pixelFormat.Align = bitmapData.Stride;
			//var status = jxlDecoder.SetImageOutBuffer(pixelFormat, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
			//if (status != JxlDecoderStatus.Success)
			//{
			//	throw new Exception();
			//}
			//status = jxlDecoder.ProcessInput();
			//if (status > JxlDecoderStatus.Success && status < JxlDecoderStatus.BasicInfo)
			//{
			//	throw new Exception();
			//}

		}

		/// <summary>
		/// Suggests a pixel format based on the BasicInfo header
		/// </summary>
		/// <param name="basicInfo">A JxlBasicInfo object describing the image</param>
		/// <returns>Either PixelFormat.Format32bppArgb, PixelFormat.Format24bppRgb, or PixelFormat.Format8bppIndexed</returns>
		public static PixelFormat GetPixelFormat(JxlBasicInfo basicInfo)
		{
			bool isColor = basicInfo.NumColorChannels > 1;
			bool hasAlpha = basicInfo.AlphaBits > 0;
			PixelFormat bitmapPixelFormat = PixelFormat.Format32bppArgb;
			if (isColor)
			{
				if (hasAlpha)
				{
					bitmapPixelFormat = PixelFormat.Format32bppArgb;
				}
				else
				{
					bitmapPixelFormat = PixelFormat.Format24bppRgb;
				}
			}
			else
			{
				if (hasAlpha)
				{
					bitmapPixelFormat = PixelFormat.Format32bppArgb;
				}
				else
				{
					bitmapPixelFormat = PixelFormat.Format8bppIndexed;
				}
			}
			return bitmapPixelFormat;
		}
		private static Bitmap CreateBlankBitmap(JxlBasicInfo basicInfo)
		{
			PixelFormat bitmapPixelFormat = GetPixelFormat(basicInfo);
			Bitmap bitmap = new Bitmap(basicInfo.Width, basicInfo.Height, bitmapPixelFormat);
			return bitmap;
		}
		/// <summary>
		/// Loads a JXL image as a Bitmap
		/// </summary>
		/// <param name="data">The JXL bytes</param>
		/// <returns>Returns a bitmap on success, otherwise returns null</returns>
		public static Bitmap LoadImage(byte[] data)
		{
			Bitmap bitmap = null;
			JxlBasicInfo basicInfo = GetBasicInfo(data, out _);
			if (basicInfo == null)
			{
				return null;
			}
			bitmap = CreateBlankBitmap(basicInfo);
			if (!LoadImageIntoBitmap(data, bitmap))
			{
				if (bitmap != null)
				{
					bitmap.Dispose();
				}
				return null;
			}
			return bitmap;


			
			
			
			//Bitmap bitmap = null;
			//JxlBasicInfo basicInfo = null;
			//using (var jxlDecoder = new JxlDecoder())
			//{
			//	jxlDecoder.SetInput(data);
			//	jxlDecoder.SubscribeEvents(JxlDecoderStatus.BasicInfo | JxlDecoderStatus.Frame | JxlDecoderStatus.FullImage);
			//	while (true)
			//	{
			//		var status = jxlDecoder.ProcessInput();
			//		if (status == JxlDecoderStatus.BasicInfo)
			//		{
			//			status = jxlDecoder.GetBasicInfo(out basicInfo);
			//		}
			//		else if (status == JxlDecoderStatus.Frame)
			//		{
			//			PixelFormat bitmapPixelFormat = PixelFormat.Format32bppArgb;
			//			JxlPixelFormat pixelFormat = new JxlPixelFormat();
			//			pixelFormat.DataType = JxlDataType.UInt8;
			//			pixelFormat.Endianness = JxlEndianness.NativeEndian;
			//			bool isColor = basicInfo.NumColorChannels > 1;
			//			bool hasAlpha = basicInfo.AlphaBits > 0;
			//			if (isColor)
			//			{
			//				if (hasAlpha)
			//				{
			//					bitmapPixelFormat = PixelFormat.Format32bppArgb;
			//					pixelFormat.NumChannels = 4;
			//				}
			//				else
			//				{
			//					bitmapPixelFormat = PixelFormat.Format24bppRgb;
			//					pixelFormat.NumChannels = 3;
			//				}
			//			}
			//			else
			//			{
			//				if (hasAlpha)
			//				{
			//					bitmapPixelFormat = PixelFormat.Format32bppArgb;
			//					pixelFormat.NumChannels = 4;
			//				}
			//				else
			//				{
			//					bitmapPixelFormat = PixelFormat.Format8bppIndexed;
			//					pixelFormat.NumChannels = 1;
			//				}
			//			}

			//			bitmap = new Bitmap(basicInfo.Width, basicInfo.Height, bitmapPixelFormat);
			//			var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
			//			if (bitmapData.Stride < 0)
			//			{
			//				throw new NotSupportedException("Stride can not be negative");
			//			}
			//			try
			//			{
			//				pixelFormat.Align = bitmapData.Stride;
			//				status = jxlDecoder.SetImageOutBuffer(pixelFormat, bitmapData.Scan0, bitmapData.Stride * bitmapData.Height);
			//				if (status != JxlDecoderStatus.Success)
			//				{
			//					throw new Exception();
			//				}
			//				status = jxlDecoder.ProcessInput();
			//				if (status > JxlDecoderStatus.Success && status < JxlDecoderStatus.BasicInfo)
			//				{
			//					throw new Exception();
			//				}
			//			}
			//			finally
			//			{
			//				if (bitmap != null)
			//				{
			//					bitmap.UnlockBits(bitmapData);
			//				}
			//			}
			//		}
			//		else if (status == JxlDecoderStatus.FullImage)
			//		{
			//			0.GetHashCode();
			//		}
			//		else if (status == JxlDecoderStatus.Success)
			//		{
			//			if (bitmap != null)
			//			{
			//				if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed)
			//				{
			//					SetGrayscalePalette(bitmap);
			//				}
			//				else
			//				{
			//					BgrSwap(bitmap);
			//				}
			//			}
			//			return bitmap;
			//			//break;
			//		}
			//		else if (status > JxlDecoderStatus.Success && status < JxlDecoderStatus.BasicInfo)
			//		{
			//			if (bitmap != null)
			//			{
			//				bitmap.Dispose();
			//				bitmap = null;
			//			}
			//			return null;
			//			//break;
			//		}
			//	}
			//}
		}

		/// <summary>
		/// Transcodes a JXL file back to a JPEG file, only possible if the image was originally a JPEG file.
		/// </summary>
		/// <param name="jxlBytes">File bytes for the JXL file</param>
		/// <returns>The resulting JPEG bytes on success, otherwise returns null</returns>
		public static byte[] TranscodeToJpeg(byte[] jxlBytes)
		{
			byte[] buffer = new byte[0];
			int outputPosition = 0;
			//byte[] buffer = new byte[1024 * 1024];
			using (var jxlDecoder = new JxlDecoder())
			{
				jxlDecoder.SetInput(jxlBytes);
				JxlBasicInfo basicInfo = null;
				bool canTranscodeToJpeg = false;
				jxlDecoder.SubscribeEvents(JxlDecoderStatus.BasicInfo | JxlDecoderStatus.JpegReconstruction | JxlDecoderStatus.Frame | JxlDecoderStatus.FullImage);
				while (true)
				{
					var status = jxlDecoder.ProcessInput();
					if (status == JxlDecoderStatus.BasicInfo)
					{
						status = jxlDecoder.GetBasicInfo(out basicInfo);
					}
					else if (status == JxlDecoderStatus.JpegReconstruction)
					{
						canTranscodeToJpeg = true;
						buffer = new byte[1024 * 1024];
						jxlDecoder.SetJPEGBuffer(buffer, outputPosition);
					}
					else if (status == JxlDecoderStatus.JpegNeedMoreOutput)
					{
						outputPosition += buffer.Length - jxlDecoder.ReleaseJPEGBuffer();
						byte[] nextBuffer = new byte[buffer.Length * 4];
						if (outputPosition > 0)
						{
							Array.Copy(buffer, 0, nextBuffer, 0, outputPosition);
						}
						buffer = nextBuffer;
						jxlDecoder.SetJPEGBuffer(buffer, outputPosition);
					}
					else if (status == JxlDecoderStatus.Frame)
					{
						//if (!canTranscodeToJpeg)
						//{
						//	return null;
						//}
					}
					else if (status == JxlDecoderStatus.Success)
					{
						outputPosition += buffer.Length - jxlDecoder.ReleaseJPEGBuffer();
						byte[] jpegBytes;
						if (buffer.Length == outputPosition)
						{
							jpegBytes = buffer;
						}
						else
						{
							jpegBytes = new byte[outputPosition];
							Array.Copy(buffer, 0, jpegBytes, 0, outputPosition);
						}
						return jpegBytes;
					}
					else if (status == JxlDecoderStatus.NeedImageOutBuffer)
					{
						return null;
					}
					else if (status >= JxlDecoderStatus.Error && status < JxlDecoderStatus.BasicInfo)
					{
						return null;
					}
					else if (status < JxlDecoderStatus.BasicInfo)
					{
						return null;
					}
				}
			}
		}
	}
}
