using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Channels;

[Serializable]
public class CanceledChunkSequenceException : Exception
{
	public CanceledChunkSequenceException()
	{
	}

	public CanceledChunkSequenceException(string message)
		: base(message)
	{
	}

	public CanceledChunkSequenceException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected CanceledChunkSequenceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
