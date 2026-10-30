using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Roslyn.Utilities;

internal static class EncodingExtensions
{
	internal static int GetMaxCharCountOrThrowIfHuge(this Encoding encoding, Stream stream)
	{
		long length = stream.Length;
		if (encoding.TryGetMaxCharCount(length, out var maxCharCount))
		{
			return maxCharCount;
		}
		throw new IOException(CodeAnalysisResources.StreamIsTooLong);
	}

	internal static bool TryGetMaxCharCount(this Encoding encoding, long length, out int maxCharCount)
	{
		maxCharCount = 0;
		if (length <= int.MaxValue)
		{
			try
			{
				maxCharCount = encoding.GetMaxCharCount((int)length);
				return true;
			}
			catch (ArgumentOutOfRangeException)
			{
			}
		}
		return false;
	}
}
