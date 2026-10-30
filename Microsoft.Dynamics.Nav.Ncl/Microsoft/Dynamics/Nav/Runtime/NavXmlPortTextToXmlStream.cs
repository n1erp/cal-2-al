using System;
using System.IO;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortTextToXmlStream : Stream
{
	private NavXmlPortTextReader reader;

	private string curBuffer;

	private int curBufferIdx;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => -1L;

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public NavXmlPortTextToXmlStream(NavXmlPortTextReader reader)
	{
		this.reader = reader;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = 0;
		while (num < count)
		{
			if (curBuffer == null)
			{
				curBufferIdx = 0;
				curBuffer = reader.GetNextXmlBuffer();
				if (curBuffer == null)
				{
					return num;
				}
			}
			int num2 = Math.Min((count - offset) / 2, curBuffer.Length - curBufferIdx);
			int bytes = Encoding.Unicode.GetBytes(curBuffer, curBufferIdx, num2, buffer, offset);
			num += bytes;
			offset += bytes;
			curBufferIdx += num2;
			if (curBufferIdx >= curBuffer.Length)
			{
				curBuffer = null;
			}
		}
		return num;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
