using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCommandLineException : NavException
{
	public NavCommandLineException()
	{
	}

	public NavCommandLineException(string message)
		: base(message)
	{
	}

	public NavCommandLineException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCommandLineException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
