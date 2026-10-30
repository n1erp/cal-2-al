using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class HybridMemoryDecompressStream : HybridStream
{
	internal HybridMemoryDecompressStream(HybridDataStore hybridStorage)
		: base(CompressionMode.Decompress)
	{
		OperationStream = (UnderlyingMemoryStream = new MemoryStream(hybridStorage.Data, 0, hybridStorage.Data.Length, writable: false));
		if (hybridStorage.IsCompressed)
		{
			OperationStream = new DeflateStream(UnderlyingMemoryStream, CompressionMode.Decompress);
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return OperationStream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public byte[] ToArray()
	{
		if (OperationStream is MemoryStream)
		{
			return UnderlyingMemoryStream.ToArray();
		}
		List<byte[]> list = new List<byte[]>();
		int num = 0;
		while (true)
		{
			byte[] array = new byte[32768];
			int num2 = Read(array, 0, 32768);
			switch (num2)
			{
			default:
			{
				byte[] array4 = new byte[num2];
				Buffer.BlockCopy(array, 0, array4, 0, num2);
				array = array4;
				break;
			}
			case 32768:
				break;
			case 0:
			{
				byte[] array2 = new byte[num];
				for (int i = 0; i < list.Count; i++)
				{
					byte[] array3 = list[i];
					list[i] = null;
					Buffer.BlockCopy(array3, 0, array2, 32768 * i, array3.Length);
				}
				return array2;
			}
			}
			list.Add(array);
			num += num2;
		}
	}
}
