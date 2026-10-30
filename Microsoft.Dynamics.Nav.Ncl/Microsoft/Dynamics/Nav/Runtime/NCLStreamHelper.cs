using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NCLStreamHelper
{
	public static int CopyStream(Stream source, Stream destination)
	{
		return CopyStream(source, destination, entireSource: false);
	}

	public static int CopyStream(Stream source, uint bytesToRead, Stream destination)
	{
		return CopyStream(source, bytesToRead, destination, entireSource: false);
	}

	public static int CopyStream(Stream source, Stream destination, bool entireSource)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return CopyStreamInternal(source, destination, entireSource);
	}

	public static int CopyStream(Stream source, uint bytesToRead, Stream destination, bool entireSource)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		return CopyStreamInternal(source, bytesToRead, destination, entireSource);
	}

	private static int CopyStreamInternal(Stream source, Stream destination, bool entireSource)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (entireSource && source.CanSeek)
		{
			source.Position = 0L;
		}
		MemoryStream memoryStream = source as MemoryStream;
		int num = 0;
		if (memoryStream != null && source.Position == 0L)
		{
			memoryStream.WriteTo(destination);
		}
		else
		{
			byte[] array = new byte[4096];
			while (true)
			{
				int num2 = source.Read(array, 0, array.Length);
				if (num2 == 0)
				{
					break;
				}
				destination.Write(array, 0, num2);
				num += num2;
				array = new byte[(array.Length < 16777216) ? (array.Length * 2) : array.Length];
			}
		}
		destination.SetLength(destination.Position);
		return num;
	}

	private static int CopyStreamInternal(Stream source, uint bytesToRead, Stream destination, bool entireSource)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (entireSource && source.CanSeek)
		{
			source.Position = 0L;
		}
		int num = 0;
		byte[] array = new byte[4096];
		int num2 = (int)bytesToRead;
		int count = Math.Min(num2, array.Length);
		while (true)
		{
			int num3 = source.Read(array, 0, count);
			if (num3 == 0)
			{
				break;
			}
			num2 -= num3;
			count = Math.Min(num2, array.Length);
			destination.Write(array, 0, num3);
			num += num3;
		}
		if (num2 != 0)
		{
			throw NavNCLInvalidStreamLengthException.Create(CultureInfo.InvariantCulture, (int)bytesToRead, (int)bytesToRead - num2);
		}
		return num;
	}

	public static void AppendStream(Stream source, Stream destination)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		destination.Seek(0L, SeekOrigin.End);
		CopyStreamInternal(source, destination, entireSource: true);
	}

	public static bool CompareStreams(Stream source, Stream destination)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (source.Length != destination.Length)
		{
			return false;
		}
		if (source.Length == 0L)
		{
			return true;
		}
		long position = source.Position;
		long position2 = destination.Position;
		try
		{
			source.Seek(0L, SeekOrigin.Begin);
			destination.Seek(0L, SeekOrigin.Begin);
			int num = (int)source.Length;
			byte[] array = new byte[16384];
			byte[] array2 = new byte[16384];
			while (num > 0)
			{
				int num2 = source.Read(array, 0, array.Length);
				int i;
				for (i = 0; i < num2; i += destination.Read(array2, i, num2 - i))
				{
				}
				for (int j = 0; j < i; j++)
				{
					if (array[j] != array2[j])
					{
						return false;
					}
				}
				num -= i;
			}
		}
		finally
		{
			source.Position = position;
			destination.Position = position2;
		}
		return true;
	}

	public static byte[] ReadToByteArray(Stream stream, int length)
	{
		byte[] array;
		if (length <= 64000)
		{
			array = new byte[length];
			int i;
			int num;
			for (i = 0; i < length; i += num)
			{
				num = stream.Read(array, i, length - i);
				if (num == 0)
				{
					break;
				}
			}
			if (i < length)
			{
				byte[] array2 = new byte[i];
				Array.Copy(array, array2, i);
				array = array2;
			}
		}
		else
		{
			List<byte[]> list = new List<byte[]>();
			int num2 = 0;
			while (num2 < length)
			{
				int num3 = length - num2;
				if (num3 > 64000)
				{
					num3 = 64000;
				}
				byte[] array3 = ReadToByteArray(stream, num3);
				list.Add(array3);
				num2 += array3.Length;
				if (array3.Length < num3)
				{
					break;
				}
			}
			if (list.Count == 1)
			{
				array = list[0];
			}
			else
			{
				array = new byte[num2];
				num2 = 0;
				for (int j = 0; j < list.Count; j++)
				{
					Array.Copy(list[j], 0, array, num2, list[j].Length);
					num2 += list[j].Length;
				}
			}
		}
		return array;
	}

	internal static long CalcPositionFromSeek(long offset, SeekOrigin origin, long position, long length)
	{
		if (offset > int.MaxValue)
		{
			throw new NavNCLOutsidePermittedRangeException("Seek", 0, offset, 0m, 2147483647m);
		}
		switch (origin)
		{
		case SeekOrigin.Begin:
			if (offset < 0)
			{
				throw new NavNCLOutsidePermittedRangeException("Seek/OffsetFromBeginning", 0, offset, 0m, 2147483647m);
			}
			return offset;
		case SeekOrigin.Current:
			if (position + offset < 0)
			{
				throw new NavNCLOutsidePermittedRangeException("Seek/OffsetFromPosition", 0, offset, -position, int.MaxValue - position);
			}
			return position + offset;
		case SeekOrigin.End:
			if (length + offset < 0)
			{
				throw new NavNCLOutsidePermittedRangeException("Seek/OffsetFromEnd", 0, offset, -length, int.MaxValue - length);
			}
			return length + offset;
		default:
			throw new NavNCLArgumentException(origin, "Seek", 1);
		}
	}
}
