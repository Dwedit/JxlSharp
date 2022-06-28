using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace JxlSharp
{
	internal static partial class MemoryStreamExtensions
	{
		static MethodInfo internalGetBufferMethodInfo = typeof(MemoryStream).GetMethod("InternalGetBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
		//static MethodInfo internalGetOriginAndLengthMethodInfo = typeof(MemoryStream).GetMethod("InternalGetOriginAndLength", BindingFlags.NonPublic | BindingFlags.Instance);
		static FieldInfo bufferFieldInfo = typeof(MemoryStream).GetField("_buffer", BindingFlags.NonPublic | BindingFlags.Instance);
		//static FieldInfo originFieldInfo = typeof(MemoryStream).GetField("_origin", BindingFlags.NonPublic | BindingFlags.Instance);

		public static ArraySegment<byte> UnsafeGetBuffer(this MemoryStream memoryStream)
		{
			{
				if (memoryStream.TryGetBuffer(out ArraySegment<byte> arraySegment))
				{
					return arraySegment;
				}
			}
			{
				int length = (int)memoryStream.Length;
				byte[] buffer;
				if (bufferFieldInfo != null)
				{
					buffer = (byte[])bufferFieldInfo.GetValue(memoryStream);
				}
				else if (internalGetBufferMethodInfo != null)
				{
					buffer = (byte[])internalGetBufferMethodInfo.Invoke(memoryStream, null);
				}
				else
				{
					return default;
				}
				return new ArraySegment<byte>(buffer, 0, length);
			}
		}
	}
}
