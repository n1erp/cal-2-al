using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientFileException : NavClientBaseException
{
	public NavClientFileException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavClientFileException()
	{
	}

	protected NavClientFileException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientFileException(string message)
		: base(message)
	{
	}
}
