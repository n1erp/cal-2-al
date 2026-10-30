using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNSValidationFailedException : NavNCLMetadataException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidationFailedException()
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidationFailedException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNSValidationFailedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavNSValidationFailedException(NavBaseException originalException)
		: base(NavBaseException.ExtractExceptionMessage(originalException), originalException)
	{
		FatalityScope = NavExceptionFatalityScope.Page;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected NavNSValidationFailedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
