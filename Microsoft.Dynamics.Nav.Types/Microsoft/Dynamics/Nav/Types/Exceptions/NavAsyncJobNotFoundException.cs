using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAsyncJobNotFoundException : NavBaseException
{
	public NavAsyncJobNotFoundException()
	{
	}

	public NavAsyncJobNotFoundException(string message)
		: base(message)
	{
	}

	public NavAsyncJobNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAsyncJobNotFoundException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
