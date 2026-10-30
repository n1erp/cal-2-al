using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavHelpException : NavCSideException
{
	public NavHelpException(string message)
		: base(message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavHelpException(int errorCode, string message)
		: base(errorCode, message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavHelpException(string message, Exception exception)
		: base(message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavHelpException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	protected NavHelpException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpException()
	{
	}
}
