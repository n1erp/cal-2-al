using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavDebuggerDebuggingNotAllowedException : NavNCLException
{
	public NavDebuggerDebuggingNotAllowedException()
	{
	}

	public NavDebuggerDebuggingNotAllowedException(string message)
		: base(message)
	{
	}

	public NavDebuggerDebuggingNotAllowedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavDebuggerDebuggingNotAllowedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavDebuggerDebuggingNotAllowedException Create()
	{
		return new NavDebuggerDebuggingNotAllowedException(string.Format(CultureInfo.CurrentCulture, Lang.DebuggingNotAllowed, ProductNames.ServerServiceDisplayName));
	}
}
