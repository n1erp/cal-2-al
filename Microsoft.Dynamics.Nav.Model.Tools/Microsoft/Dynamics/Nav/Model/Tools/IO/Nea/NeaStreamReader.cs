using System;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;

internal sealed class NeaStreamReader : NeaStream
{
	private Stream subject;

	public override bool CanRead => subject.CanRead;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

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

	public NeaStreamReader(Stream subject, bool leaveOpen = false)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (IsSupported(subject, resetPosition: false, out var key))
		{
			this.subject = new Rc4Stream(subject, key, leaveOpen);
			return;
		}
		throw new ArgumentException("Invalid argument.", "subject");
	}

	public static bool IsSupported(Stream subject)
	{
		byte[] key;
		return IsSupported(subject, resetPosition: true, out key);
	}

	public override void Flush()
	{
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return subject.Read(buffer, offset, count);
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

	private static bool IsSupported(Stream subject, bool resetPosition, out byte[] key)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		bool result = false;
		key = null;
		if (subject.CanRead && (!resetPosition || subject.CanSeek) && subject.Length >= NeaStream.Header.Length)
		{
			long position = subject.Position;
			try
			{
				byte[] array = new byte[NeaStream.Header.Length];
				int num = subject.Read(array, 0, array.Length);
				if (array.Length == num && array.SequenceEqual(NeaStream.Header))
				{
					result = true;
					key = NeaStream.Key;
				}
			}
			finally
			{
				if (resetPosition)
				{
					subject.Seek(position, SeekOrigin.Begin);
				}
			}
		}
		return result;
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
			if (subject != null)
			{
				subject = null;
			}
			base.Dispose(disposing);
		}
	}
}
