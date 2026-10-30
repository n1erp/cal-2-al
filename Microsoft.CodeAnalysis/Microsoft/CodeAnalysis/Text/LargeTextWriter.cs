using System;
using System.Text;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class LargeTextWriter : SourceTextWriter
{
	private readonly Encoding _encoding;

	private readonly SourceHashAlgorithm _checksumAlgorithm;

	private readonly ArrayBuilder<char[]> _chunks;

	private int _bufferSize;

	private char[] _buffer;

	private int _currentUsed;

	public override Encoding Encoding => _encoding;

	public LargeTextWriter(Encoding encoding, SourceHashAlgorithm checksumAlgorithm, int length)
	{
		_encoding = encoding;
		_checksumAlgorithm = checksumAlgorithm;
		_chunks = ArrayBuilder<char[]>.GetInstance(1 + length / 40960);
		_bufferSize = Math.Min(40960, length);
	}

	public override SourceText ToSourceText()
	{
		Flush();
		return new LargeText(_chunks.ToImmutableAndFree(), _encoding, default, _checksumAlgorithm, default);
	}

	public bool CanFitInAllocatedBuffer(int chars)
	{
		if (_buffer != null)
		{
			return chars <= _buffer.Length - _currentUsed;
		}
		return false;
	}

	public override void Write(char value)
	{
		if (_buffer != null && _currentUsed < _buffer.Length)
		{
			_buffer[_currentUsed] = value;
			_currentUsed++;
		}
		else
		{
			Write(new char[1] { value }, 0, 1);
		}
	}

	public override void Write(string value)
	{
		if (value == null)
		{
			return;
		}
		int num = value.Length;
		int num2 = 0;
		while (num > 0)
		{
			EnsureBuffer();
			int num3 = Math.Min(_buffer.Length - _currentUsed, num);
			value.CopyTo(num2, _buffer, _currentUsed, num3);
			_currentUsed += num3;
			num2 += num3;
			num -= num3;
			if (_currentUsed == _buffer.Length)
			{
				Flush();
			}
		}
	}

	public override void Write(char[] chars, int index, int count)
	{
		if (index < 0 || index >= chars.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0 || count > chars.Length - index)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		while (count > 0)
		{
			EnsureBuffer();
			int num = Math.Min(_buffer.Length - _currentUsed, count);
			Array.Copy(chars, index, _buffer, _currentUsed, num);
			_currentUsed += num;
			index += num;
			count -= num;
			if (_currentUsed == _buffer.Length)
			{
				Flush();
			}
		}
	}

	internal void AppendChunk(char[] chunk)
	{
		if (CanFitInAllocatedBuffer(chunk.Length))
		{
			Write(chunk, 0, chunk.Length);
			return;
		}
		Flush();
		_chunks.Add(chunk);
	}

	public override void Flush()
	{
		if (_buffer != null && _currentUsed > 0)
		{
			if (_currentUsed < _buffer.Length)
			{
				Array.Resize(ref _buffer, _currentUsed);
			}
			_chunks.Add(_buffer);
			_buffer = null;
			_currentUsed = 0;
		}
	}

	private void EnsureBuffer()
	{
		if (_buffer == null)
		{
			_buffer = new char[_bufferSize];
		}
	}
}
