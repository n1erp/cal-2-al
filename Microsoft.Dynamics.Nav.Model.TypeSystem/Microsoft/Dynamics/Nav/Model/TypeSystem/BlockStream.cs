using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

internal class BlockStream : Stream
{
	private readonly BlockStreamMode mode;

	private readonly BlockData firstBlock;

	private BlockData currentBlock;

	private int position;

	private int currentOffset;

	private int length;

	public override bool CanRead => mode == BlockStreamMode.Read;

	public override bool CanSeek => false;

	public override bool CanWrite => mode == BlockStreamMode.Write;

	public override long Length
	{
		get
		{
			if (mode == BlockStreamMode.Read)
			{
				if (length < 0)
				{
					length = firstBlock.Size;
				}
				return length;
			}
			return position;
		}
	}

	public override long Position
	{
		get
		{
			return position;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	internal BlockStream()
		: this(null, BlockStreamMode.Write)
	{
	}

	internal BlockStream(BlockData contents)
		: this(contents, BlockStreamMode.Read)
	{
	}

	internal BlockStream(BlockData contents, BlockStreamMode mode)
	{
		this.mode = mode;
		switch (mode)
		{
		case BlockStreamMode.Read:
			firstBlock = contents;
			currentBlock = contents;
			length = -1;
			break;
		case BlockStreamMode.Write:
			firstBlock = new BlockData();
			currentBlock = firstBlock;
			break;
		default:
			throw new ArgumentException();
		}
	}

	public override void Flush()
	{
	}

	internal BlockData CloseAndGetData()
	{
		if (mode == BlockStreamMode.Write)
		{
			currentBlock.Truncate(currentOffset);
		}
		return firstBlock;
	}

	public override int ReadByte()
	{
		if (mode != BlockStreamMode.Read)
		{
			throw new InvalidOperationException();
		}
		while (true)
		{
			byte[] data = currentBlock.Data;
			if (currentOffset < data.Length)
			{
				return data[currentOffset++];
			}
			if (currentBlock.Next == null)
			{
				break;
			}
			currentBlock = currentBlock.Next;
			currentOffset = 0;
		}
		return -1;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (mode != BlockStreamMode.Read)
		{
			throw new InvalidOperationException();
		}
		int num = 0;
		while (count > 0)
		{
			byte[] data = currentBlock.Data;
			int num2 = data.Length - currentOffset;
			if (num2 == 0)
			{
				if (currentBlock.Next == null)
				{
					break;
				}
				currentBlock = currentBlock.Next;
				currentOffset = 0;
				continue;
			}
			int num3 = ((count > num2) ? num2 : count);
			count -= num3;
			position += num3;
			num += num3;
			if (num3 < 128)
			{
				while (num3-- > 0)
				{
					buffer[offset++] = data[currentOffset++];
				}
			}
			else
			{
				Buffer.BlockCopy(data, currentOffset, buffer, offset, num3);
				offset += num3;
				currentOffset += num3;
			}
		}
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new InvalidOperationException();
	}

	public override void SetLength(long value)
	{
		throw new InvalidOperationException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (mode != BlockStreamMode.Write)
		{
			throw new InvalidOperationException();
		}
		while (count > 0)
		{
			byte[] data = currentBlock.Data;
			int num = data.Length - currentOffset;
			if (num == 0)
			{
				currentBlock = new BlockData(currentBlock);
				currentOffset = 0;
				continue;
			}
			int num2 = ((count > num) ? num : count);
			count -= num2;
			position += num2;
			if (num2 < 128)
			{
				while (num2-- > 0)
				{
					data[currentOffset++] = buffer[offset++];
				}
			}
			else
			{
				Buffer.BlockCopy(buffer, offset, data, currentOffset, num2);
				offset += num2;
				currentOffset += num2;
			}
		}
	}
}
