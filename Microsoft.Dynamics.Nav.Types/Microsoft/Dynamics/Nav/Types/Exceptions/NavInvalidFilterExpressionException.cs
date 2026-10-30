using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavInvalidFilterExpressionException : NavBaseException
{
	public NavInvalidFilterExpressionException()
	{
	}

	public NavInvalidFilterExpressionException(string message)
		: base(message)
	{
	}

	public NavInvalidFilterExpressionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavInvalidFilterExpressionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
