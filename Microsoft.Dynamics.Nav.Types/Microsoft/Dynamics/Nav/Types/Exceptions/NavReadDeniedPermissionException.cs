using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavReadDeniedPermissionException : NavPermissionException
{
	public const int ClientReadDeniedErrorCode = 18023807;

	public const int ServerReadDeniedErrorCode = 18023523;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReadDeniedPermissionException()
	{
	}

	public NavReadDeniedPermissionException(string message)
		: base(message)
	{
	}

	public NavReadDeniedPermissionException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavReadDeniedPermissionException(string message, Exception exception)
		: base(message, exception)
	{
	}

	public NavReadDeniedPermissionException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
	}

	private NavReadDeniedPermissionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
