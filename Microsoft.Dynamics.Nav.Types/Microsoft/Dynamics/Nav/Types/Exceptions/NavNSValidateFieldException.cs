using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSValidateFieldException : NavException
{
	public NavNSValidateFieldException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidateFieldException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidateFieldException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNSValidateFieldException(NavBaseException originalException)
		: base(NavBaseException.ExtractExceptionMessage(originalException), NavBaseException.ExtractDiagnosticsMessage(originalException), originalException)
	{
		if (originalException == null)
		{
			throw new ArgumentNullException("originalException");
		}
		FatalityScope = NavExceptionFatalityScope.Call;
		SuppressMessage = originalException.SuppressMessage;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNSValidateFieldException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
