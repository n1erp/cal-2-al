using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpSyntaxErrorException : NavHelpException
{
	public const int SyntaxErrorCode = 18415619;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpSyntaxErrorException()
	{
	}

	public NavHelpSyntaxErrorException(string message)
		: base(message)
	{
	}

	public NavHelpSyntaxErrorException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpSyntaxErrorException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpSyntaxErrorException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpSyntaxErrorException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
