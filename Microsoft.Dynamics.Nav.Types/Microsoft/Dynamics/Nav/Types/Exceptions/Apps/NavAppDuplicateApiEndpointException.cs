using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.Apps;

[Serializable]
public class NavAppDuplicateApiEndpointException : NavAppException
{
	public NavAppDuplicateApiEndpointException()
	{
	}

	public NavAppDuplicateApiEndpointException(string message)
		: base(message)
	{
	}

	public NavAppDuplicateApiEndpointException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavAppDuplicateApiEndpointException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
