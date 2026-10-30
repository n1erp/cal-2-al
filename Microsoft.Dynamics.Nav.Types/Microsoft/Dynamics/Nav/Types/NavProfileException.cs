using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavProfileException : NavBaseException
{
	private NavProfileException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavProfileException()
	{
	}

	public NavProfileException(string message)
		: base(message)
	{
	}

	public NavProfileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
