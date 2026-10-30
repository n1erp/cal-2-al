using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavProcessException : NavBaseException
{
	public NavProcessException()
	{
	}

	public NavProcessException(string message)
		: base(message)
	{
	}

	public NavProcessException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavProcessException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
