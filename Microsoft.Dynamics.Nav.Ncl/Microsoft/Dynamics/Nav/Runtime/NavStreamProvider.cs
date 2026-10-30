using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavStreamProvider : TreeObject, INavStreamProvider, IDisposable
{
	internal class ComStream : Stream
	{
		private const long StgmRead = 0L;

		private const long StgmWrite = 1L;

		private const long StgmReadWrite = 2L;

		private IStream stream;

		private bool? canRead;

		private bool? canWrite;

		private bool? canSeek;

		private bool isDisposed;

		public override bool CanRead
		{
			get
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException("stream");
				}
				if (canRead.HasValue)
				{
					return canRead.Value;
				}
				try
				{
					stream.Stat(out var pstatstg, 1);
					canRead = pstatstg.grfMode == 0L || ((ulong)pstatstg.grfMode & 2uL) != 0;
				}
				catch (NotImplementedException)
				{
					canRead = false;
				}
				return canRead.Value;
			}
		}

		public override bool CanSeek
		{
			get
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException("stream");
				}
				if (canSeek.HasValue)
				{
					return canSeek.Value;
				}
				try
				{
					long position = Position;
					Position = 0L;
					Position = position;
					canSeek = true;
				}
				catch (NotImplementedException)
				{
					canSeek = false;
				}
				return canSeek.Value;
			}
		}

		public override bool CanWrite
		{
			get
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException("stream");
				}
				if (canWrite.HasValue)
				{
					return canWrite.Value;
				}
				try
				{
					stream.Stat(out var pstatstg, 1);
					canWrite = ((ulong)pstatstg.grfMode & 1uL) != 0L || ((ulong)pstatstg.grfMode & 2uL) != 0;
				}
				catch (NotImplementedException)
				{
					canWrite = false;
				}
				return canWrite.Value;
			}
		}

		public override long Length
		{
			get
			{
				if (isDisposed)
				{
					throw new ObjectDisposedException("stream");
				}
				stream.Stat(out var pstatstg, 1);
				return pstatstg.cbSize;
			}
		}

		public override long Position
		{
			get
			{
				return Seek(0L, SeekOrigin.Current);
			}
			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}

		internal ComStream(IStream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException("stream");
			}
			this.stream = stream;
		}

		public override void Close()
		{
			isDisposed = true;
			if (!isDisposed && stream != null)
			{
				stream.Commit(0);
			}
			stream = null;
			base.Close();
		}

		public override void Flush()
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("stream");
			}
			stream.Commit(0);
		}

		public unsafe override int Read(byte[] buffer, int offset, int count)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("stream");
			}
			if (offset != 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			int result = default;
			IntPtr pcbRead = new IntPtr(&result);
			stream.Read(buffer, count, pcbRead);
			return result;
		}

		public unsafe override long Seek(long offset, SeekOrigin origin)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("stream");
			}
			long result = default;
			IntPtr plibNewPosition = new IntPtr(&result);
			stream.Seek(offset, (int)origin, plibNewPosition);
			return result;
		}

		public override void SetLength(long value)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("stream");
			}
			stream.SetSize(value);
		}

		public unsafe override void Write(byte[] buffer, int offset, int count)
		{
			if (isDisposed)
			{
				throw new ObjectDisposedException("stream");
			}
			if (offset != 0)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			int num = default;
			IntPtr pcbWritten = new IntPtr(&num);
			stream.Write(buffer, count, pcbWritten);
		}
	}

	private Stream stream;

	private bool disposeStream;

	public NavStreamProvider(Stream stream)
		: this(stream, NavCurrentThread.Session.Company.SharedObjects)
	{
	}

	public NavStreamProvider(Stream stream, ITreeObject parent, bool disposeStream = false)
		: base(parent)
	{
		this.stream = stream;
		this.disposeStream = disposeStream;
	}

	public NavStreamProvider(IStream stream)
		: base(NavCurrentThread.Session.Company.SharedObjects)
	{
		this.stream = new ComStream(stream);
	}

	public Stream GetStream()
	{
		return stream;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				Tree.DisposeAllChildren();
			}
		}
		finally
		{
			try
			{
				if (disposing && stream != null && (disposeStream || stream is ComStream || stream is FileBufferedStream))
				{
					stream.Dispose();
					stream = null;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
