using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSaveAbortException : NavBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavSaveAbortException()
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavSaveAbortException(string message)
		: base(message)
	{
	}

	public NavSaveAbortException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSaveAbortException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavSaveAbortException(Exception innerException)
		: base(CreateMessage(innerException), innerException)
	{
		MarkFatalityScope(NavExceptionFatalityScope.Page);
	}

	private static string CreateMessage(Exception innerException)
	{
		if (innerException == null)
		{
			return string.Empty;
		}
		return innerException.Message;
	}
}
