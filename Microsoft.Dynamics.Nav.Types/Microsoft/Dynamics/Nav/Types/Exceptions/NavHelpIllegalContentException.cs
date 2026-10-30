using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpIllegalContentException : NavHelpException
{
	public const int IllegalContentErrorCode = 18415623;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpIllegalContentException()
	{
	}

	public NavHelpIllegalContentException(string message)
		: base(message)
	{
	}

	public NavHelpIllegalContentException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpIllegalContentException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpIllegalContentException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpIllegalContentException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
