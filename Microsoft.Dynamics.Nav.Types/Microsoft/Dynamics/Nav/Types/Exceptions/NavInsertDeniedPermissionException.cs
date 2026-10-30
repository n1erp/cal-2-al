using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavInsertDeniedPermissionException : NavPermissionException
{
	public const int InsertDeniedErrorCode = 18023808;

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavInsertDeniedPermissionException()
	{
	}

	public NavInsertDeniedPermissionException(string message)
		: this(18023808, message)
	{
	}

	public NavInsertDeniedPermissionException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavInsertDeniedPermissionException(string message, Exception exception)
		: this(18023808, message, exception)
	{
	}

	public NavInsertDeniedPermissionException(int errorCode, string message, Exception exception)
		: base(errorCode, message, exception)
	{
	}

	private NavInsertDeniedPermissionException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
