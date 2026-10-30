using System;
using System.IO;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Text;

internal static class EncodedStringText
{
	private const int LargeObjectHeapLimitInChars = 40960;

	private static readonly Encoding Utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private static readonly Encoding FallbackEncoding = GetFallbackEncoding();

	private static Encoding GetFallbackEncoding()
	{
		try
		{
			return Encoding.GetEncoding(0) ?? Encoding.GetEncoding(1252);
		}
		catch (NotSupportedException)
		{
			return Encoding.GetEncoding("Latin1");
		}
	}

	internal static SourceText Create(Stream stream, Encoding defaultEncoding = null)
	{
		DebugAssertHelper.Assert(stream != null);
		DebugAssertHelper.Assert(stream.CanRead && stream.CanSeek);
		bool flag = defaultEncoding == null;
		if (flag)
		{
			try
			{
				return Decode(stream, Utf8Encoding);
			}
			catch (DecoderFallbackException)
			{
			}
		}
		try
		{
			return Decode(stream, defaultEncoding ?? FallbackEncoding, flag);
		}
		catch (DecoderFallbackException ex2)
		{
			throw new InvalidDataException(ex2.Message);
		}
	}

	internal static SourceText Decode(Stream data, Encoding encoding, bool throwIfBinaryDetected = false)
	{
		DebugAssertHelper.Assert(data != null);
		DebugAssertHelper.Assert(encoding != null);
		data.Seek(0L, SeekOrigin.Begin);
		if (encoding.GetMaxCharCount((int)data.Length) < 40960)
		{
			byte[] array = TryGetByteArrayFromStream(data);
			if (array != null)
			{
				return SourceText.From(array, (int)data.Length, encoding, throwIfBinaryDetected);
			}
		}
		return SourceText.From(data, encoding, throwIfBinaryDetected);
	}

	private static byte[] TryGetByteArrayFromStream(Stream data)
	{
		if (data is MemoryStream data2 && TryGetByteArrayFromMemoryStream(data2, out var buffer))
		{
			return buffer;
		}
		if (data.GetType() == typeof(FileStream) && TryGetByteArrayFromFileStream(data, out buffer))
		{
			return buffer;
		}
		return null;
	}

	private static bool TryGetByteArrayFromMemoryStream(MemoryStream data, out byte[] buffer)
	{
		DebugAssertHelper.Assert(data.Position == 0);
		try
		{
			buffer = data.GetBuffer();
			return true;
		}
		catch (Exception)
		{
			buffer = null;
			return false;
		}
	}

	private static bool TryGetByteArrayFromFileStream(Stream stream, out byte[] buffer)
	{
		DebugAssertHelper.Assert(stream != null);
		DebugAssertHelper.Assert(stream.Position == 0);
		int num = (int)stream.Length;
		if (num == 0)
		{
			buffer = SpecializedCollections.EmptyBytes;
			return true;
		}
		buffer = new byte[num];
		return stream.TryReadAll(buffer, 0, num) == num;
	}
}
