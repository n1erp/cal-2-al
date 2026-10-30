using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavFormatCSideException : NavCSideException
{
	public NavFormatCSideException(string message)
		: base(message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavFormatCSideException(int errorCode, string message)
		: base(errorCode, message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavFormatCSideException(string message, Exception exception)
		: base(message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavFormatCSideException(string message, string diagnosticsMessage, Exception exception)
		: base(message, diagnosticsMessage, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavFormatCSideException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	protected NavFormatCSideException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavFormatCSideException()
	{
	}

	public static NavFormatCSideException Create(string message, Exception exception)
	{
		if (exception is NavBaseException ex)
		{
			return new NavFormatCSideException(message, ex.DiagnosticsMessage, exception);
		}
		return new NavFormatCSideException(message, exception);
	}
}
