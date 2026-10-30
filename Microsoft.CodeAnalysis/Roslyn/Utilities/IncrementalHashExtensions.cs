using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace Roslyn.Utilities;

internal static class IncrementalHashExtensions
{
	internal static void AppendData(this IncrementalHash hash, IEnumerable<Blob> blobs)
	{
		foreach (Blob blob in blobs)
		{
			AppendData(hash, blob.GetBytes());
		}
	}

	internal static void AppendData(this IncrementalHash hash, IEnumerable<ArraySegment<byte>> blobs)
	{
		foreach (ArraySegment<byte> blob in blobs)
		{
			AppendData(hash, blob);
		}
	}

	internal static void AppendData(this IncrementalHash hash, ArraySegment<byte> segment)
	{
		hash.AppendData(segment.Array, segment.Offset, segment.Count);
	}
}
