using System;
using System.IO;
using System.IO.Compression;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class HybridMemoryCompressStream : HybridStream
{
	private bool isCompressed;

	private const int DefaultStreamCapacity = 4096;

	private int configCompressionThreshold;

	internal bool IsCompressed
	{
		get
		{
			return isCompressed;
		}
		private set
		{
			isCompressed = value;
		}
	}

	public HybridMemoryCompressStream()
		: base(CompressionMode.Compress)
	{
		OperationStream = (UnderlyingMemoryStream = new MemoryStream(4096));
	}

	public HybridMemoryCompressStream(int configCompressionThreshold)
		: base(CompressionMode.Compress)
	{
		OperationStream = (UnderlyingMemoryStream = new MemoryStream(4096));
		this.configCompressionThreshold = configCompressionThreshold;
	}

	public HybridMemoryCompressStream(byte[] data, int configCompressionThreshold)
		: base(CompressionMode.Compress)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		int capacity = ((data.Length > configCompressionThreshold) ? configCompressionThreshold : data.Length);
		this.configCompressionThreshold = configCompressionThreshold;
		OperationStream = (UnderlyingMemoryStream = new MemoryStream(capacity));
		Write(data, 0, data.Length);
	}

	public HybridMemoryCompressStream(Stream stream, int configCompressionThreshold)
		: base(CompressionMode.Compress)
	{
		if (stream == null)
		{
			throw new ArgumentNullException("stream");
		}
		long num = ((stream.Length > configCompressionThreshold) ? configCompressionThreshold : stream.Length);
		this.configCompressionThreshold = configCompressionThreshold;
		OperationStream = (UnderlyingMemoryStream = new MemoryStream((int)num));
		byte[] buffer = new byte[4096];
		int num2;
		do
		{
			num2 = stream.Read(buffer, 0, 4096);
			WriteInternal(buffer, 0, num2);
		}
		while (num2 > 0);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		WriteInternal(buffer, offset, count);
	}

	public HybridDataStore GetDataStore()
	{
		try
		{
			if (IsCompressed)
			{
				((DeflateStream)OperationStream).Close();
			}
			return new HybridDataStore(UnderlyingMemoryStream.ToArray(), IsCompressed);
		}
		finally
		{
			UnderlyingMemoryStream.Close();
		}
	}

	private void WriteInternal(byte[] buffer, int offset, int count)
	{
		if (!IsCompressed && Length + count > configCompressionThreshold)
		{
			UseCompressionStream();
		}
		OperationStream.Write(buffer, offset, count);
	}

	private void UseCompressionStream()
	{
		IsCompressed = true;
		MemoryStream stream = new MemoryStream((int)UnderlyingMemoryStream.Length / 8);
		DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
		byte[] array = UnderlyingMemoryStream.ToArray();
		deflateStream.Write(array, 0, array.Length);
		OperationStream = deflateStream;
		UnderlyingMemoryStream.Dispose();
		UnderlyingMemoryStream = stream;
	}
}
