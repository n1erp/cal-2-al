using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpLineTooLongException : NavHelpException
{
	public const int LineTooLongErrorCode = 18415618;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpLineTooLongException()
	{
	}

	public NavHelpLineTooLongException(string message)
		: base(message)
	{
	}

	public NavHelpLineTooLongException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpLineTooLongException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpLineTooLongException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpLineTooLongException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
