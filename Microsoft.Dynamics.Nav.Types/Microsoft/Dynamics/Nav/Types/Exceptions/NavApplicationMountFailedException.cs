using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavApplicationMountFailedException : NavException
{
	public NavApplicationMountFailedException()
	{
	}

	public NavApplicationMountFailedException(string message)
		: base(message)
	{
	}

	public NavApplicationMountFailedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavApplicationMountFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
