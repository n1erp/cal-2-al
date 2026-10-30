using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUserNotFoundException : NavException
{
	public NavUserNotFoundException()
	{
	}

	public NavUserNotFoundException(string message)
		: base(message)
	{
	}

	public NavUserNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUserNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
