using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavClientValidateFieldException : NavClientBaseException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientValidateFieldException()
	{
	}

	public NavClientValidateFieldException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private NavClientValidateFieldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientValidateFieldException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavClientValidateFieldException(NavBaseException innerException)
		: base(NavBaseException.ExtractExceptionMessage(innerException), innerException)
	{
		if (innerException == null)
		{
			throw new ArgumentNullException("innerException");
		}
		FatalityScope = NavExceptionFatalityScope.Call;
		SuppressMessage = innerException.SuppressMessage;
	}
}
