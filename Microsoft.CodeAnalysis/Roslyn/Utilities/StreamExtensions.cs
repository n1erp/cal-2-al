using System;
using System.IO;

namespace Roslyn.Utilities;

internal static class StreamExtensions
{
	public static int TryReadAll(this Stream stream, byte[] buffer, int offset, int count)
	{
		int i;
		int num;
		for (i = 0; i < count; i += num)
		{
			num = stream.Read(buffer, offset + i, count - i);
			if (num == 0)
			{
				break;
			}
		}
		return i;
	}

	public static byte[] ReadAllBytes(this Stream stream)
	{
		if (stream.CanSeek)
		{
			long num = stream.Length - stream.Position;
			if (num == 0L)
			{
				return Array.Empty<byte>();
			}
			byte[] array = new byte[num];
			int newSize = stream.TryReadAll(array, 0, array.Length);
			Array.Resize(ref array, newSize);
			return array;
		}
		MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}
}
