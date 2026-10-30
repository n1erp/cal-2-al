using System;
using System.Data.SqlClient;
using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class BlobReaderStream : Stream
{
	private SqlDataReader reader;

	private int fieldIndex;

	private int length;

	private int position;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => length;

	public override long Position
	{
		get
		{
			return position;
		}
		set
		{
			position = (int)value;
		}
	}

	public BlobReaderStream(SqlDataReader reader, int fieldIndex)
		: this(reader, fieldIndex, (int)reader.GetBytes(fieldIndex, 0L, null, 0, 0))
	{
	}

	public BlobReaderStream(SqlDataReader reader, int fieldIndex, int length)
	{
		this.reader = reader;
		this.fieldIndex = fieldIndex;
		this.length = length;
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
		if (position < length)
		{
			int num = (int)reader.GetBytes(fieldIndex, position, buffer, offset, count);
			position += num;
			return num;
		}
		return 0;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
