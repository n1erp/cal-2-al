using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavPermissionException : NavCSideException
{
	public NavPermissionException(string message)
		: base(message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavPermissionException(int errorCode, string message)
		: base(errorCode, message)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavPermissionException(string message, Exception exception)
		: base(message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	public NavPermissionException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	protected NavPermissionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavPermissionException()
	{
	}
}
