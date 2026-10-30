using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavFormCloseNotAllowedException : NavBaseException
{
	public NavFormCloseNotAllowedException()
	{
	}

	public NavFormCloseNotAllowedException(string message)
		: base(message)
	{
	}

	public NavFormCloseNotAllowedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavFormCloseNotAllowedException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
