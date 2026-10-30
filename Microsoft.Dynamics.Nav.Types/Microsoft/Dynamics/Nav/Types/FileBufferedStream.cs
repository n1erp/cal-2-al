using System;
using System.IO;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class FileBufferedStream : Stream
{
	private FileStream fileBufferStream;

	private Stream stream;

	private const int BufferReadSize = 4096;

	private bool deleteFileBuffer;

	private bool isDisposed;

	public bool ShouldDeleteFileOnDispose
	{
		get
		{
			return deleteFileBuffer;
		}
		set
		{
			deleteFileBuffer = value;
		}
	}

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public bool IsEos => stream == null;

	public override long Length
	{
		get
		{
			ReadToEos();
			return fileBufferStream.Length;
		}
	}

	public override long Position
	{
		get
		{
			return fileBufferStream.Position;
		}
		set
		{
			ReadDataIntoFileStream(value);
			if (value > fileBufferStream.Length)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			fileBufferStream.Position = value;
		}
	}

	public string PersistedFilePath => fileBufferStream.Name;

	public FileBufferedStream(string fileBufferPath, Stream stream)
	{
		if (string.IsNullOrEmpty(fileBufferPath))
		{
			throw new ArgumentNullException("fileBufferPath");
		}
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		fileBufferStream = new FileStream(fileBufferPath, FileMode.CreateNew, FileAccess.ReadWrite);
		this.stream = stream;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (!disposing || isDisposed)
			{
				return;
			}
			if (fileBufferStream != null)
			{
				if (!deleteFileBuffer)
				{
					try
					{
						if (fileBufferStream.CanWrite)
						{
							fileBufferStream.Flush();
						}
					}
					catch (ObjectDisposedException)
					{
					}
				}
				fileBufferStream.Dispose();
				if (deleteFileBuffer && File.Exists(fileBufferStream.Name))
				{
					try
					{
						File.Delete(fileBufferStream.Name);
					}
					catch (IOException)
					{
					}
					catch (UnauthorizedAccessException)
					{
					}
				}
				fileBufferStream = null;
			}
			if (stream != null)
			{
				stream.Dispose();
				stream = null;
			}
		}
		finally
		{
			isDisposed = true;
			base.Dispose(disposing);
		}
	}

	private void ReadDataIntoFileStream(long position)
	{
		if (stream == null || fileBufferStream.Length >= position)
		{
			return;
		}
		long position2 = fileBufferStream.Position;
		try
		{
			fileBufferStream.Position = fileBufferStream.Length;
			byte[] buffer = new byte[4096];
			int num;
			do
			{
				num = stream.Read(buffer, 0, 4096);
				fileBufferStream.Write(buffer, 0, num);
			}
			while (num > 0 && position > fileBufferStream.Length);
			if (num < 4096)
			{
				num = stream.Read(buffer, 0, 1);
				fileBufferStream.Write(buffer, 0, num);
			}
			if (num == 0)
			{
				stream.Dispose();
				stream = null;
			}
		}
		finally
		{
			fileBufferStream.Position = position2;
		}
	}

	public void ReadToEos()
	{
		if (stream == null)
		{
			return;
		}
		long position = Position;
		try
		{
			do
			{
				ReadDataIntoFileStream(fileBufferStream.Length + 4096);
			}
			while (stream != null);
			Position = fileBufferStream.Length;
		}
		finally
		{
			Position = position;
		}
	}

	public override void Flush()
	{
		fileBufferStream.Flush();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		ReadDataIntoFileStream(fileBufferStream.Position + count);
		return fileBufferStream.Read(buffer, offset, count);
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			Position = offset;
			break;
		case SeekOrigin.Current:
			Position += offset;
			break;
		case SeekOrigin.End:
			ReadToEos();
			Position = fileBufferStream.Length - offset;
			break;
		default:
			throw new ArgumentException("Unknown SeekOrigin", "origin");
		}
		return fileBufferStream.Position;
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
