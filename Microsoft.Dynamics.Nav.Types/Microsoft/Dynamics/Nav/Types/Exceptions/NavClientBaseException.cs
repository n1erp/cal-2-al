using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavClientBaseException : NavBaseException
{
	protected NavClientBaseException()
	{
	}

	protected NavClientBaseException(string message)
		: base(message)
	{
	}

	protected NavClientBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavClientBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
