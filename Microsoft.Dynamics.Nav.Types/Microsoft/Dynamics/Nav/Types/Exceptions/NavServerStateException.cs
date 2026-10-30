using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = false)]
public class NavServerStateException : NavException
{
	public NavServerStateException()
	{
	}

	public NavServerStateException(string message)
		: base(message)
	{
	}

	public NavServerStateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavServerStateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
