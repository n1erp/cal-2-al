using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging.Nea;

internal sealed class Rc4Stream : Stream
{
	private Stream subject;

	private readonly Rc4 rc4;

	private readonly bool leaveOpen;

	public override bool CanRead => subject.CanRead;

	public override bool CanSeek => subject.CanSeek;

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
			subject.Position = value;
		}
	}

	public Rc4Stream(Stream subject, byte[] key, bool leaveOpen = false)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		this.subject = subject ?? throw new ArgumentNullException("subject");
		this.leaveOpen = leaveOpen;
		rc4 = new Rc4(key);
	}

	public override void Flush()
	{
		subject.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int num = subject.Read(buffer, offset, count);
		rc4.Decode(buffer, offset, num);
		return num;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		return subject.Seek(offset, origin);
	}

	public override void SetLength(long value)
	{
		subject.SetLength(value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		subject.Write(rc4.Encode(buffer, offset, count), 0, count);
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!leaveOpen & disposing)
			{
				subject?.Close();
			}
		}
		finally
		{
			if (!leaveOpen && subject != null)
			{
				subject = null;
			}
			base.Dispose(disposing);
		}
	}
}
