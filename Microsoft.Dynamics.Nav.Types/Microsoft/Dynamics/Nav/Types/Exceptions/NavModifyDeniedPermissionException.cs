using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavModifyDeniedPermissionException : NavPermissionException
{
	public const int ModifyDeniedErrorCode = 18023810;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavModifyDeniedPermissionException()
	{
	}

	public NavModifyDeniedPermissionException(string message)
		: this(18023810, message)
	{
	}

	public NavModifyDeniedPermissionException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavModifyDeniedPermissionException(string message, Exception exception)
		: this(18023810, message, exception)
	{
	}

	public NavModifyDeniedPermissionException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Call);
	}

	private NavModifyDeniedPermissionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
