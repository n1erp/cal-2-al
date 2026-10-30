using System;
using System.IO;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging.Nea;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal sealed class NavAppPackageContentStream : Stream
{
	private readonly Stream subject;

	private readonly long subjectOffset;

	private readonly long length;

	private readonly Stream outputStream;

	private readonly bool neaStream;

	public override bool CanRead => subject.CanRead;

	public override bool CanSeek => subject.CanSeek;

	public override bool CanWrite
	{
		get
		{
			if (0 <= length)
			{
				return false;
			}
			return subject.CanWrite;
		}
	}

	public override long Length
	{
		get
		{
			if (0 > length)
			{
				return subject.Length - subjectOffset;
			}
			return length;
		}
	}

	public override long Position
	{
		get
		{
			return subject.Position - subjectOffset;
		}
		set
		{
			subject.Position = value + subjectOffset;
		}
	}

	private NavAppPackageContentStream(Stream subject, long offset, long length, Stream outputStream, bool neaStream)
	{
		this.subject = subject;
		subjectOffset = offset;
		this.length = length;
		this.outputStream = outputStream;
		this.neaStream = neaStream;
	}

	public static Stream Create(Stream subject, long offset, long length, bool writeAsNeaStream)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (0 > offset)
		{
			throw new ArgumentException(CodeAnalysisResources.ValueMayNotBeLessThanZero, "offset");
		}
		if (subject.Length < offset)
		{
			throw new ArgumentException(CodeAnalysisResources.ValueMayNotBeLargerThanStream, "offset");
		}
		if (0 < length && 0 > subject.Length - offset - length)
		{
			throw new ArgumentException(CodeAnalysisResources.ValueMustBeWithinStreamLengthOrMinusOne, "length");
		}
		Stream stream = subject;
		subject = NeaStreamSupport(subject, offset, length, writeAsNeaStream);
		NavAppPackageContentStream navAppPackageContentStream = new NavAppPackageContentStream(subject, offset, length, stream, writeAsNeaStream);
		navAppPackageContentStream.Seek(0L, SeekOrigin.Begin);
		return navAppPackageContentStream;
	}

	private static Stream NeaStreamSupport(Stream subject, long offset, long length, bool writeAsNeaStream)
	{
		if (length > 0)
		{
			subject.Seek(offset, SeekOrigin.Begin);
			if (NeaStreamReader.IsSupported(subject))
			{
				MemoryStream memoryStream = new MemoryStream();
				using NeaStreamReader neaStreamReader = new NeaStreamReader(subject, leaveOpen: true);
				memoryStream.Seek(offset, SeekOrigin.Begin);
				neaStreamReader.CopyTo(memoryStream);
				subject = memoryStream;
			}
		}
		else if (writeAsNeaStream)
		{
			MemoryStream memoryStream2 = new MemoryStream();
			memoryStream2.Seek(offset, SeekOrigin.Begin);
			subject = memoryStream2;
		}
		return subject;
	}

	public override void Flush()
	{
		if (!CanWrite)
		{
			throw new IOException(CodeAnalysisResources.StreamNotWriteable);
		}
		subject.Flush();
	}

	public override void Close()
	{
		if (neaStream)
		{
			WriteAsNeaStream();
		}
	}

	private void WriteAsNeaStream()
	{
		outputStream.Seek(subjectOffset, SeekOrigin.Begin);
		subject.Seek(subjectOffset, SeekOrigin.Begin);
		using NeaStreamWriter destination = new NeaStreamWriter(outputStream, leaveOpen: true);
		subject.CopyTo(destination);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return subject.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			return subject.Seek(subjectOffset + offset, origin) - subjectOffset;
		case SeekOrigin.Current:
			return subject.Seek(offset, origin) - subjectOffset;
		case SeekOrigin.End:
			if (0 > length)
			{
				return subject.Seek(offset, origin) - subjectOffset;
			}
			return subject.Seek(subjectOffset + length + offset, SeekOrigin.Begin) - subjectOffset;
		default:
			throw new ArgumentException(CodeAnalysisResources.UnsupportedSeekOrigin, "origin");
		}
	}

	public override void SetLength(long value)
	{
		if (!CanWrite)
		{
			throw new IOException(CodeAnalysisResources.StreamNotWriteable);
		}
		subject.SetLength(subjectOffset + value);
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (!CanWrite)
		{
			throw new IOException(CodeAnalysisResources.StreamNotWriteable);
		}
		subject.Write(buffer, offset, count);
	}
}
