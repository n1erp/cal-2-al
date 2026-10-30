using System;
using System.IO;
using System.Text;

namespace Microsoft.CodeAnalysis.Text;

internal sealed class SourceTextStream : Stream
{
	private readonly SourceText _source;

	private readonly Encoding _encoding;

	private readonly Encoder _encoder;

	private readonly int _minimumTargetBufferCount;

	private int _position;

	private int _sourceOffset;

	private readonly char[] _charBuffer;

	private int _bufferOffset;

	private int _bufferUnreadChars;

	private bool _preambleWritten;

	private static readonly Encoding s_utf8EncodingWithNoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false);

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			return _position;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public SourceTextStream(SourceText source, int bufferSize = 2048, bool useDefaultEncodingIfNull = false)
	{
		_source = source;
		_encoding = source.Encoding ?? s_utf8EncodingWithNoBOM;
		_encoder = _encoding.GetEncoder();
		_minimumTargetBufferCount = _encoding.GetMaxByteCount(1);
		_sourceOffset = 0;
		_position = 0;
		_charBuffer = new char[Math.Min(bufferSize, _source.Length)];
		_bufferOffset = 0;
		_bufferUnreadChars = 0;
		_preambleWritten = false;
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (count < _minimumTargetBufferCount)
		{
			throw new ArgumentException(string.Format("{0} must be greater than or equal to {1}", "count", _minimumTargetBufferCount), "count");
		}
		int num = count;
		if (!_preambleWritten)
		{
			int num2 = WritePreamble(buffer, offset, count);
			offset += num2;
			count -= num2;
		}
		while (count >= _minimumTargetBufferCount && _position < _source.Length)
		{
			if (_bufferUnreadChars == 0)
			{
				FillBuffer();
			}
			_encoder.Convert(_charBuffer, _bufferOffset, _bufferUnreadChars, buffer, offset, count, flush: false, out var charsUsed, out var bytesUsed, out var _);
			_position += charsUsed;
			_bufferOffset += charsUsed;
			_bufferUnreadChars -= charsUsed;
			offset += bytesUsed;
			count -= bytesUsed;
		}
		return num - count;
	}

	private int WritePreamble(byte[] buffer, int offset, int count)
	{
		_preambleWritten = true;
		byte[] preamble = _encoding.GetPreamble();
		if (preamble == null)
		{
			return 0;
		}
		int num = Math.Min(count, preamble.Length);
		Array.Copy(preamble, 0, buffer, offset, num);
		return num;
	}

	private void FillBuffer()
	{
		int num = Math.Min(_charBuffer.Length, _source.Length - _sourceOffset);
		_source.CopyTo(_sourceOffset, _charBuffer, 0, num);
		_sourceOffset += num;
		_bufferOffset = 0;
		_bufferUnreadChars = num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
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
