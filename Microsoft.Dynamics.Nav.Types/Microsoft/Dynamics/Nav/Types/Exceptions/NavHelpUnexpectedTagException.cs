using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpUnexpectedTagException : NavHelpException
{
	public const int UnexpectedTagErrorCode = 18415624;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpUnexpectedTagException()
	{
	}

	public NavHelpUnexpectedTagException(string message)
		: base(message)
	{
	}

	public NavHelpUnexpectedTagException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpUnexpectedTagException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpUnexpectedTagException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpUnexpectedTagException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
