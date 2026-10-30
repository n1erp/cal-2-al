using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[EventLog(WriteEntry = true, LogType = EventLogEntryType.Error)]
public class NavConfigurationException : NavBaseException
{
	public NavConfigurationException(string message)
		: base(message)
	{
	}

	public NavConfigurationException(string message, params object[] args)
		: this(string.Format(CultureInfo.CurrentCulture, message, args))
	{
	}

	public NavConfigurationException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavConfigurationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public NavConfigurationException()
	{
	}
}
