using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Reflection.Internal;

internal static class EncodingHelper
{
	internal unsafe delegate string Encoding_GetString(Encoding encoding, byte* bytes, int byteCount);

	private unsafe delegate string String_CreateStringFromEncoding(byte* bytes, int byteCount, Encoding encoding);

	public const int PooledBufferSize = 200;

	private static readonly ObjectPool<byte[]> s_pool = new ObjectPool<byte[]>(() => new byte[200]);

	private static Encoding_GetString s_getStringPlatform = LoadGetStringPlatform();

	internal static bool TestOnly_LightUpEnabled
	{
		get
		{
			return s_getStringPlatform != null;
		}
		set
		{
			s_getStringPlatform = (value ? LoadGetStringPlatform() : null);
		}
	}

	public unsafe static string DecodeUtf8(byte* bytes, int byteCount, byte[] prefix, MetadataStringDecoder utf8Decoder)
	{
		if (prefix != null)
		{
			return DecodeUtf8Prefixed(bytes, byteCount, prefix, utf8Decoder);
		}
		if (byteCount == 0)
		{
			return string.Empty;
		}
		return utf8Decoder.GetString(bytes, byteCount);
	}

	private unsafe static string DecodeUtf8Prefixed(byte* bytes, int byteCount, byte[] prefix, MetadataStringDecoder utf8Decoder)
	{
		int num = byteCount + prefix.Length;
		if (num == 0)
		{
			return string.Empty;
		}
		byte[] array = AcquireBuffer(num);
		prefix.CopyTo(array, 0);
		Marshal.Copy((IntPtr)bytes, array, prefix.Length, byteCount);
		string result;
		fixed (byte* bytes2 = &array[0])
		{
			result = utf8Decoder.GetString(bytes2, num);
		}
		ReleaseBuffer(array);
		return result;
	}

	private static byte[] AcquireBuffer(int byteCount)
	{
		if (byteCount > 200)
		{
			return new byte[byteCount];
		}
		return s_pool.Allocate();
	}

	private static void ReleaseBuffer(byte[] buffer)
	{
		if (buffer.Length == 200)
		{
			s_pool.Free(buffer);
		}
	}

	public unsafe static string GetString(this Encoding encoding, byte* bytes, int byteCount)
	{
		if (s_getStringPlatform == null)
		{
			return GetStringPortable(encoding, bytes, byteCount);
		}
		return s_getStringPlatform(encoding, bytes, byteCount);
	}

	private unsafe static string GetStringPortable(Encoding encoding, byte* bytes, int byteCount)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount");
		}
		byte[] array = AcquireBuffer(byteCount);
		Marshal.Copy((IntPtr)bytes, array, 0, byteCount);
		string result = encoding.GetString(array, 0, byteCount);
		ReleaseBuffer(array);
		return result;
	}

	private unsafe static Encoding_GetString LoadGetStringPlatform()
	{
		MethodInfo method = LightUpHelper.GetMethod(typeof(Encoding), "GetString", typeof(byte*), typeof(int));
		if ((object)method != null && (object)method.ReturnType == typeof(string))
		{
			try
			{
				return (Encoding_GetString)method.CreateDelegate(typeof(Encoding_GetString), null);
			}
			catch (MemberAccessException)
			{
			}
			catch (InvalidOperationException)
			{
			}
		}
		IEnumerable<MethodInfo> declaredMethods = typeof(string).GetTypeInfo().GetDeclaredMethods("CreateStringFromEncoding");
		foreach (MethodInfo item in declaredMethods)
		{
			ParameterInfo[] parameters = item.GetParameters();
			if (parameters.Length != 3 || (object)parameters[0].ParameterType != typeof(byte*) || (object)parameters[1].ParameterType != typeof(int) || (object)parameters[2].ParameterType != typeof(Encoding) || (object)item.ReturnType != typeof(string))
			{
				continue;
			}
			try
			{
				String_CreateStringFromEncoding createStringFromEncoding = (String_CreateStringFromEncoding)item.CreateDelegate(typeof(String_CreateStringFromEncoding), null);
				return (Encoding encoding, byte* bytes, int byteCount) => GetStringUsingCreateStringFromEncoding(createStringFromEncoding, bytes, byteCount, encoding);
			}
			catch (MemberAccessException)
			{
			}
			catch (InvalidOperationException)
			{
			}
		}
		return null;
	}

	private unsafe static string GetStringUsingCreateStringFromEncoding(String_CreateStringFromEncoding createStringFromEncoding, byte* bytes, int byteCount, Encoding encoding)
	{
		if (bytes == null)
		{
			throw new ArgumentNullException("bytes");
		}
		if (byteCount < 0)
		{
			throw new ArgumentOutOfRangeException("byteCount");
		}
		return createStringFromEncoding(bytes, byteCount, encoding);
	}
}
