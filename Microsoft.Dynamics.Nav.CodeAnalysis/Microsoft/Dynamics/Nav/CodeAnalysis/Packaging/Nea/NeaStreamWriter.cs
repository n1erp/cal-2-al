using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging.Nea;

internal sealed class NeaStreamWriter : NeaStream
{
	private Stream subject;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => subject.CanWrite;

	public override long Length => subject.Length;

	public override long Position
	{
		get
		{
			return subject.Position;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public NeaStreamWriter(Stream subject, bool leaveOpen = false)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		subject.Write(NeaStream.Header, 0, NeaStream.Header.Length);
		this.subject = new Rc4Stream(subject, NeaStream.Key, leaveOpen);
	}

	public override void Flush()
	{
		subject.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
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
		subject.Write(buffer, offset, count);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && subject != null)
			{
				subject.Close();
			}
		}
		finally
		{
			subject = null;
			base.Dispose(disposing);
		}
	}
}
