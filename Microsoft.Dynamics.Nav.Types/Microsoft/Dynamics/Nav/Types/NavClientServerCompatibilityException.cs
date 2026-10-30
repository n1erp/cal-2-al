using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[EventLog(WriteEntry = true, LogType = EventLogEntryType.Warning)]
public sealed class NavClientServerCompatibilityException : NavBaseException
{
	private NavClientServerCompatibilityException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientServerCompatibilityException()
	{
	}

	public NavClientServerCompatibilityException(string message)
		: base(message)
	{
	}

	public NavClientServerCompatibilityException(Version clientVersion, Version serverVersion)
		: base(CreateMessage(clientVersion, serverVersion))
	{
	}

	private static string CreateMessage(Version clientVersion, Version serverVersion)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.IncompatibleVersionErrorMessage, clientVersion, serverVersion);
	}

	public NavClientServerCompatibilityException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
