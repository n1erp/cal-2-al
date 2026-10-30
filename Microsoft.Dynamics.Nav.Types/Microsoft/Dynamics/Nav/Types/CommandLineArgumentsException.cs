using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class CommandLineArgumentsException : NavBaseException
{
	protected CommandLineArgumentsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public CommandLineArgumentsException()
	{
	}

	public CommandLineArgumentsException(string message)
		: base(message)
	{
	}

	public CommandLineArgumentsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
