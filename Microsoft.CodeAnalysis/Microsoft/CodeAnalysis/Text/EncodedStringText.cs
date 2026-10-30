using System;
using System.IO;
using System.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Text;

internal static class EncodedStringText
{
	private const int LargeObjectHeapLimitInChars = 40960;

	private static readonly Encoding s_utf8Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

	private static readonly Lazy<Encoding> s_fallbackEncoding = new Lazy<Encoding>(GetFallbackEncoding);

	private static Encoding GetFallbackEncoding()
	{
		try
		{
			if (CodePagesEncodingProvider.Instance != null)
			{
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			}
			return Encoding.GetEncoding(0) ?? Encoding.GetEncoding(1252);
		}
		catch (NotSupportedException)
		{
			return Encoding.GetEncoding("Latin1");
		}
	}

	internal static SourceText Create(Stream stream, Encoding defaultEncoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool canBeEmbedded = false)
	{
		return Create(stream, s_fallbackEncoding, defaultEncoding, checksumAlgorithm, canBeEmbedded);
	}

	internal static SourceText Create(Stream stream, Lazy<Encoding> getEncoding, Encoding defaultEncoding = null, SourceHashAlgorithm checksumAlgorithm = SourceHashAlgorithm.Sha1, bool canBeEmbedded = false)
	{
		bool flag = defaultEncoding == null;
		if (flag)
		{
			try
			{
				return Decode(stream, s_utf8Encoding, checksumAlgorithm, throwIfBinaryDetected: false, canBeEmbedded);
			}
			catch (DecoderFallbackException)
			{
			}
		}
		try
		{
			return Decode(stream, defaultEncoding ?? getEncoding.Value, checksumAlgorithm, flag, canBeEmbedded);
		}
		catch (DecoderFallbackException ex2)
		{
			throw new InvalidDataException(ex2.Message);
		}
	}

	internal static SourceText Decode(Stream data, Encoding encoding, SourceHashAlgorithm checksumAlgorithm, bool throwIfBinaryDetected = false, bool canBeEmbedded = false)
	{
		data.Seek(0L, SeekOrigin.Begin);
		if (encoding.GetMaxCharCountOrThrowIfHuge(data) < 40960 && TryGetBytesFromStream(data, out var bytes) && bytes.Offset == 0)
		{
			return SourceText.From(bytes.Array, (int)data.Length, encoding, checksumAlgorithm, throwIfBinaryDetected, canBeEmbedded);
		}
		return SourceText.From(data, encoding, checksumAlgorithm, throwIfBinaryDetected, canBeEmbedded);
	}

	internal static bool TryGetBytesFromStream(Stream data, out ArraySegment<byte> bytes)
	{
		if (data is MemoryStream memoryStream)
		{
			return memoryStream.TryGetBuffer(out bytes);
		}
		if (data is FileStream stream)
		{
			return TryGetBytesFromFileStream(stream, out bytes);
		}
		bytes = new ArraySegment<byte>(Array.Empty<byte>());
		return false;
	}

	private static bool TryGetBytesFromFileStream(FileStream stream, out ArraySegment<byte> bytes)
	{
		int num = (int)stream.Length;
		if (num == 0)
		{
			bytes = new ArraySegment<byte>(Array.Empty<byte>());
			return true;
		}
		byte[] array = new byte[num];
		bool flag = stream.TryReadAll(array, 0, num) == num;
		bytes = (flag ? new ArraySegment<byte>(array) : new ArraySegment<byte>(Array.Empty<byte>()));
		return flag;
	}
}
