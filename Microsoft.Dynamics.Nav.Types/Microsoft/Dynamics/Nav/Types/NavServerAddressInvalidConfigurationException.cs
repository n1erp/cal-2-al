using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[EventLog(WriteEntry = true, LogType = EventLogEntryType.Error)]
public class NavServerAddressInvalidConfigurationException : NavConfigurationException
{
	public NavServerAddressInvalidConfigurationException(string message)
		: base(message)
	{
	}

	public NavServerAddressInvalidConfigurationException(string message, params object[] args)
		: this(string.Format(CultureInfo.CurrentCulture, message, args))
	{
	}

	public NavServerAddressInvalidConfigurationException(string message, Exception exception)
		: base(message, exception)
	{
	}

	protected NavServerAddressInvalidConfigurationException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	public NavServerAddressInvalidConfigurationException()
	{
	}
}
