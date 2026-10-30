using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(WriteEntry = true, LogType = EventLogEntryType.Error)]
public abstract class NavServerSessionBaseException : Exception
{
	protected NavServerSessionBaseException()
	{
	}

	protected NavServerSessionBaseException(string message)
		: base(message)
	{
	}

	protected NavServerSessionBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavServerSessionBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
