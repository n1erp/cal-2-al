using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclClientStopException : NavNCLException
{
	public NavNclClientStopException()
		: base("The supplied regional settings are not valid.")
	{
	}

	public NavNclClientStopException(string message)
		: base(message)
	{
	}

	public NavNclClientStopException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclClientStopException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
