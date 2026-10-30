using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavHelpFileListBlankNameException : NavHelpException
{
	public const int FileListBlankNameErrorCode = 18415617;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavHelpFileListBlankNameException()
	{
	}

	public NavHelpFileListBlankNameException(string message)
		: base(message)
	{
	}

	public NavHelpFileListBlankNameException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavHelpFileListBlankNameException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavHelpFileListBlankNameException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private NavHelpFileListBlankNameException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
