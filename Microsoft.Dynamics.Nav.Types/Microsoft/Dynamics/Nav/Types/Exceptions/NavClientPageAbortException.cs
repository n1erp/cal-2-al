using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavClientPageAbortException : NavBaseException
{
	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientPageAbortException()
	{
	}

	[Obsolete("Do use a constructor with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavClientPageAbortException(string message)
		: base(message)
	{
	}

	public NavClientPageAbortException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavClientPageAbortException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavClientPageAbortException(Exception innerException)
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
