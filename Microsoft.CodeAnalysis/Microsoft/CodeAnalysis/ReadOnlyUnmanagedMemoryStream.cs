using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.CodeAnalysis;

internal sealed class ReadOnlyUnmanagedMemoryStream : Stream
{
	private readonly object _memoryOwner;

	private readonly IntPtr _data;

	private readonly int _length;

	private int _position;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => _length;

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			Seek(value, SeekOrigin.Begin);
		}
	}

	public ReadOnlyUnmanagedMemoryStream(object memoryOwner, IntPtr data, int length)
	{
		_memoryOwner = memoryOwner;
		_data = data;
		_length = length;
	}

	public unsafe override int ReadByte()
	{
		if (_position == _length)
		{
			return -1;
		}
		return ((byte*)(void*)_data)[_position++];
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = Math.Min(count, _length - _position);
		Marshal.Copy(_data + _position, buffer, offset, num);
		_position += num;
		return num;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		long num;
		try
		{
			num = checked(origin switch
			{
				SeekOrigin.Begin => offset, 
				SeekOrigin.Current => offset + _position, 
				SeekOrigin.End => offset + _length, 
				_ => throw new ArgumentOutOfRangeException("origin"), 
			});
		}
		catch (OverflowException)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (num < 0 || num >= _length)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		_position = (int)num;
		return num;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
