using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavMediaNotFoundException : NavMediaBaseException
{
	public NavMediaNotFoundException()
	{
	}

	public NavMediaNotFoundException(string message)
		: base(message)
	{
	}

	public NavMediaNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavMediaNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
