using System;
using System.IO;
using System.Threading;

namespace Microsoft.Dynamics.Nav.Runtime;

public class ReaderBlocksWriterStream : Stream
{
	private class ReaderStream : Stream
	{
		private readonly byte[] readBuffer;

		private readonly AutoResetEvent bufferReadyForRead;

		private readonly AutoResetEvent bufferReadyForWrite;

		private readonly CancellationToken cancellationToken;

		private int readBufferPosition;

		private long position;

		private volatile int readBufferSize;

		private volatile bool isClosed;

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
				return position;
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public ReaderStream(CancellationToken cancellationToken, int bufferSize)
		{
			this.cancellationToken = cancellationToken;
			readBuffer = new byte[bufferSize];
			bufferReadyForRead = new AutoResetEvent(initialState: false);
			bufferReadyForWrite = new AutoResetEvent(initialState: true);
		}

		private void WaitForEventOrTimeout(AutoResetEvent autoResetEvent)
		{
			switch (WaitHandle.WaitAny(new WaitHandle[2] { autoResetEvent, cancellationToken.WaitHandle }, 30000))
			{
			case 1:
				if (cancellationToken.IsCancellationRequested)
				{
					throw new OperationCanceledException(cancellationToken);
				}
				break;
			case 258:
				throw new TimeoutException();
			}
			cancellationToken.ThrowIfCancellationRequested();
		}

		public void SetBufferReadyForRead(byte[] buffer, int bufferSize, bool closing)
		{
			WaitForEventOrTimeout(bufferReadyForWrite);
			readBufferPosition = 0;
			Buffer.BlockCopy(buffer, 0, readBuffer, 0, bufferSize);
			readBufferSize = bufferSize;
			isClosed = closing;
			bufferReadyForRead.Set();
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (!isClosed)
			{
				WaitForEventOrTimeout(bufferReadyForRead);
			}
			if (readBufferSize == 0)
			{
				bufferReadyForRead.Set();
				return 0;
			}
			int num = Math.Min(readBufferSize - readBufferPosition, count);
			if (num > 0)
			{
				Buffer.BlockCopy(readBuffer, readBufferPosition, buffer, offset, num);
				position += num;
				readBufferPosition += num;
			}
			if (readBufferSize == readBufferPosition)
			{
				readBufferSize = 0;
				bufferReadyForWrite.Set();
			}
			else
			{
				bufferReadyForRead.Set();
			}
			return num;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		public override void WriteByte(byte value)
		{
			throw new NotSupportedException();
		}
	}

	private const int DefaultBufferSize = 8192;

	private const int DefaultWaitTimeout = 30000;

	private readonly int bufferSize;

	private readonly ReaderStream reader;

	private readonly byte[] writeBuffer;

	private readonly CancellationToken cancellationToken;

	private int writeBufferPosition;

	private long position;

	private bool isClosed;

	public Stream Reader => reader;

	public override bool CanRead => false;

	public override bool CanSeek => false;

	public override bool CanWrite => true;

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
			return position;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public ReaderBlocksWriterStream(CancellationToken cancellationToken = default(CancellationToken), int bufferSize = 8192)
	{
		this.cancellationToken = cancellationToken;
		this.bufferSize = bufferSize;
		writeBuffer = new byte[this.bufferSize];
		reader = new ReaderStream(cancellationToken, this.bufferSize);
	}

	private void FlushWriteBuffer(bool closing)
	{
		reader.SetBufferReadyForRead(writeBuffer, writeBufferPosition, closing);
		writeBufferPosition = 0;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override int ReadByte()
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int num = Math.Min(bufferSize - writeBufferPosition, count);
			Buffer.BlockCopy(buffer, offset, writeBuffer, writeBufferPosition, num);
			writeBufferPosition += num;
			position += num;
			if (writeBufferPosition == bufferSize)
			{
				FlushWriteBuffer(closing: false);
				if (num < count)
				{
					offset += num;
					count -= num;
					continue;
				}
				break;
			}
			break;
		}
	}

	public override void Close()
	{
		if (!isClosed)
		{
			isClosed = true;
			if (!cancellationToken.IsCancellationRequested)
			{
				FlushWriteBuffer(closing: true);
			}
		}
	}
}
