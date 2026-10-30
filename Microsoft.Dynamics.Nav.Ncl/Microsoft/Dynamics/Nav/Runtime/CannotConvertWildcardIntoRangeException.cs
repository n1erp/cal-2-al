using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

[Serializable]
public class CannotConvertWildcardIntoRangeException : NavBaseException
{
	public CannotConvertWildcardIntoRangeException()
		: base(suppressExceptionCreatedEvent: true)
	{
	}

	public CannotConvertWildcardIntoRangeException(string message)
		: base(message, suppressExceptionCreatedEvent: true)
	{
	}

	public CannotConvertWildcardIntoRangeException(string message, Exception inner)
		: base(message, inner, suppressExceptionCreatedEvent: true)
	{
	}

	protected CannotConvertWildcardIntoRangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
