using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
[EventLog(LogType = EventLogEntryType.Error)]
public class NavMetadataObsoleteException : NavNCLMetadataException
{
	public NavMetadataObsoleteException()
	{
		FatalityScope = NavExceptionFatalityScope.Page;
	}

	public NavMetadataObsoleteException(string message)
		: base(message)
	{
		FatalityScope = NavExceptionFatalityScope.Page;
	}

	public NavMetadataObsoleteException(string message, Exception innerException)
		: base(message, innerException)
	{
		FatalityScope = NavExceptionFatalityScope.Page;
	}

	protected NavMetadataObsoleteException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		FatalityScope = NavExceptionFatalityScope.Page;
	}
}
