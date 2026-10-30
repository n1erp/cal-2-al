using System;
using System.IO;
using System.ServiceModel;

namespace Microsoft.Dynamics.Nav.Types;

[MessageContract(WrapperNamespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public sealed class StreamTransfer : IDisposable
{
	[MessageHeader(MustUnderstand = true)]
	private string fileName;

	[MessageBodyMember(Order = 1)]
	private Stream stream;

	public string FileName
	{
		get
		{
			return fileName;
		}
		set
		{
			fileName = value;
		}
	}

	public Stream Stream
	{
		get
		{
			return stream;
		}
		set
		{
			stream = value;
		}
	}

	public StreamTransfer()
		: this(Stream.Null)
	{
	}

	public StreamTransfer(Stream stream)
		: this(string.Empty, stream)
	{
	}

	public StreamTransfer(string fileName, Stream stream)
	{
		this.fileName = fileName;
		this.stream = stream;
	}

	public Stream TakeStreamOwnership()
	{
		Stream result = Stream;
		Stream = null;
		return result;
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (stream != null)
		{
			stream.Dispose();
			stream = null;
		}
	}
}
