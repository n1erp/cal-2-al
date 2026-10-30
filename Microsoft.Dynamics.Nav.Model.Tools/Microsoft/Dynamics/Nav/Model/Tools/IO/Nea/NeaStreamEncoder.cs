using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Model.Tools.IO.Nea;

internal sealed class NeaStreamEncoder : NeaStream
{
	private sealed class StreamsStream : Stream
	{
		private long position;

		private List<Stream> streamList;

		public override bool CanRead
		{
			get
			{
				if (streamList.FirstOrDefault((Stream stream) => !stream.CanRead) != null)
				{
					return false;
				}
				if (streamList.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (streamList.FirstOrDefault((Stream stream) => !stream.CanSeek) != null)
				{
					return false;
				}
				if (streamList.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		public override bool CanWrite => false;

		public override long Length => streamList.Sum((Stream s) => s.Length);

		public override long Position
		{
			get
			{
				return position;
			}
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}

		internal bool Closable
		{
			get
			{
				if (streamList == null || streamList.Count <= 0)
				{
					return false;
				}
				return true;
			}
		}

		public StreamsStream()
		{
			streamList = new List<Stream>();
		}

		public void Add(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			if (Length > long.MaxValue - stream.Length)
			{
				throw new ArgumentException("Unable to add the specified stream as doing so would result in overflow.", "stream");
			}
			streamList.Add(stream);
		}

		public override void Close()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			long num = 0L;
			int num2 = 0;
			int num3 = offset;
			foreach (Stream stream in streamList)
			{
				if (position < num + stream.Length)
				{
					stream.Position = position - num;
					int num4 = stream.Read(buffer, num3, count);
					num2 += num4;
					num3 += num4;
					position += num4;
					if (num4 >= count)
					{
						break;
					}
					count -= num4;
				}
				num += stream.Length;
			}
			return num2;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long length = Length;
			switch (origin)
			{
			case SeekOrigin.Begin:
				position = offset;
				break;
			case SeekOrigin.Current:
				position += offset;
				break;
			case SeekOrigin.End:
				position = length - offset;
				break;
			}
			if (position > length)
			{
				position = length;
			}
			else if (position < 0)
			{
				position = 0L;
			}
			return position;
		}

		public override void Flush()
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

		protected override void Dispose(bool disposing)
		{
			if (!Closable)
			{
				return;
			}
			try
			{
				if (!disposing)
				{
					return;
				}
				foreach (Stream stream in streamList)
				{
					stream.Close();
				}
			}
			finally
			{
				streamList.Clear();
				base.Dispose(disposing);
			}
		}
	}

	private Stream subject;

	private bool leaveOpen;

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

	private NeaStreamEncoder(Stream subject, bool leaveOpen)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		this.subject = subject;
		this.leaveOpen = leaveOpen;
	}

	public static NeaStreamEncoder Create(Stream subject, bool leaveOpen = false)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		if (IsAlreadyEncoded(subject, resetPosition: true))
		{
			return new NeaStreamEncoder(subject, leaveOpen);
		}
		StreamsStream streamsStream = new StreamsStream();
		streamsStream.Add(new MemoryStream(NeaStream.Header));
		streamsStream.Add(new Rc4Stream(subject, NeaStream.Key, leaveOpen));
		return new NeaStreamEncoder(streamsStream, leaveOpen: false);
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

	private static bool IsAlreadyEncoded(Stream subject, bool resetPosition)
	{
		if (subject == null)
		{
			throw new ArgumentNullException("subject");
		}
		bool result = false;
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
				}
			}
			finally
			{
				if (resetPosition)
				{
					subject.Position = position;
				}
			}
		}
		return result;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if ((!leaveOpen & disposing) && subject != null)
			{
				subject.Close();
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
