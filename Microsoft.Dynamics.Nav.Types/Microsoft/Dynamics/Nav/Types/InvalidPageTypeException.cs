using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class InvalidPageTypeException : NavBaseException
{
	private InvalidPageTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public InvalidPageTypeException()
	{
	}

	public InvalidPageTypeException(string message)
		: base(message)
	{
	}

	public InvalidPageTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
