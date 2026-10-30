using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavDeleteDeniedPermissionException : NavPermissionException
{
	public const int DeleteDeniedErrorCode = 18023809;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavDeleteDeniedPermissionException()
	{
	}

	public NavDeleteDeniedPermissionException(string message)
		: this(18023809, message)
	{
	}

	public NavDeleteDeniedPermissionException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavDeleteDeniedPermissionException(string message, Exception exception)
		: this(18023809, message, exception)
	{
	}

	public NavDeleteDeniedPermissionException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
	}

	private NavDeleteDeniedPermissionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
