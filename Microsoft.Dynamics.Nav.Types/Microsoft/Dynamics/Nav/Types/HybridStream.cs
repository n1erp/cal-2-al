using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Dynamics.Nav.Types;

public abstract class HybridStream : Stream
{
	private CompressionMode compressionMode;

	private Stream operationStream;

	private MemoryStream underlyingMemoryStream;

	public override bool CanRead => compressionMode == CompressionMode.Decompress;

	public override bool CanSeek => false;

	public override bool CanWrite => compressionMode == CompressionMode.Compress;

	public override long Length => OperationStream.Length;

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	protected Stream OperationStream
	{
		get
		{
			return operationStream;
		}
		set
		{
			operationStream = value;
		}
	}

	protected MemoryStream UnderlyingMemoryStream
	{
		get
		{
			return underlyingMemoryStream;
		}
		set
		{
			underlyingMemoryStream = value;
		}
	}

	protected HybridStream(CompressionMode compressionMode)
	{
		this.compressionMode = compressionMode;
	}

	public override void Flush()
	{
		OperationStream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				if (operationStream != null)
				{
					operationStream.Dispose();
				}
				if (underlyingMemoryStream != null)
				{
					underlyingMemoryStream.Dispose();
				}
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
