using System;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

internal class BlockData
{
	private const int BlockSize = 40000;

	private byte[] data;

	internal int Size
	{
		get
		{
			int num = 0;
			BlockData blockData = this;
			do
			{
				num += blockData.data.Length;
				blockData = blockData.Next;
			}
			while (blockData != null);
			return num;
		}
	}

	internal byte[] Data => data;

	internal BlockData Next { get; private set; }

	internal BlockData()
	{
		data = new byte[40000];
	}

	internal BlockData(BlockData prevBlock)
		: this()
	{
		prevBlock.Next = this;
	}

	internal void Truncate(int thisBufferSize)
	{
		Next = null;
		if (thisBufferSize != data.Length)
		{
			Array.Resize(ref data, thisBufferSize % 40000);
		}
	}
}
