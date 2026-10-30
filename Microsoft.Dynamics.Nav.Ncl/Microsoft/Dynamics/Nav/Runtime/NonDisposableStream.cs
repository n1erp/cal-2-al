using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NonDisposableStream : Stream
{
	private Stream stream;

	public override bool CanRead => stream.CanRead;

	public override bool CanSeek => stream.CanSeek;

	public override bool CanWrite => stream.CanWrite;

	public override long Length => stream.Length;

	public override long Position
	{
		get
		{
			return stream.Position;
		}
		set
		{
			stream.Position = value;
		}
	}

	public NonDisposableStream(Stream stream)
	{
		this.stream = stream;
	}

	public override void Close()
	{
	}

	protected override void Dispose(bool disposing)
	{
	}

	public override void Flush()
	{
		stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return stream.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		stream.SetLength(value);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return stream.Read(buffer, offset, count);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		stream.Write(buffer, offset, count);
	}
}
