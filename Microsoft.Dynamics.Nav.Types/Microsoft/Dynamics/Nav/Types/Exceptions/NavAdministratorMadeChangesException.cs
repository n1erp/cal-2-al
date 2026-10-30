using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavAdministratorMadeChangesException : NavException
{
	public NavAdministratorMadeChangesException()
		: base(CreateMessage())
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	public NavAdministratorMadeChangesException(string message)
		: base(message)
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	public NavAdministratorMadeChangesException(string message, Exception innerException)
		: base(message, innerException)
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	protected NavAdministratorMadeChangesException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		FatalityScope = NavExceptionFatalityScope.Call;
	}

	private static string CreateMessage()
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.MetadataChangeRestartActivity);
	}
}
