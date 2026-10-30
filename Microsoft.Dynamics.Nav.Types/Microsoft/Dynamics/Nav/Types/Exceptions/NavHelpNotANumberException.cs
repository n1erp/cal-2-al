using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpNotANumberException : NavHelpException
{
	public const int NotANumberErrorCode = 18415622;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpNotANumberException()
	{
	}

	public NavHelpNotANumberException(string message)
		: base(message)
	{
	}

	public NavHelpNotANumberException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpNotANumberException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpNotANumberException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpNotANumberException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
