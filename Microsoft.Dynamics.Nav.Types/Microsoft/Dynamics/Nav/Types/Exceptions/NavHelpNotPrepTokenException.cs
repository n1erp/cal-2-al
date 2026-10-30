using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpNotPrepTokenException : NavHelpException
{
	public const int NotPrepTokenErrorCode = 18415620;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpNotPrepTokenException()
	{
	}

	public NavHelpNotPrepTokenException(string message)
		: base(message)
	{
	}

	public NavHelpNotPrepTokenException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpNotPrepTokenException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpNotPrepTokenException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpNotPrepTokenException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
