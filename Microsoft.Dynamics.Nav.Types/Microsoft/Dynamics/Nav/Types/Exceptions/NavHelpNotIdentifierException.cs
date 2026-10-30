using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpNotIdentifierException : NavHelpException
{
	public const int NoIdentifierErrorCode = 18415621;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpNotIdentifierException()
	{
	}

	public NavHelpNotIdentifierException(string message)
		: base(message)
	{
	}

	public NavHelpNotIdentifierException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpNotIdentifierException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpNotIdentifierException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpNotIdentifierException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
