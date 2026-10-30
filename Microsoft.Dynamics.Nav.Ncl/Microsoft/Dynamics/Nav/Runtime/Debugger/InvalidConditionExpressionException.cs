using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

[Serializable]
public class InvalidConditionExpressionException : NavBaseException
{
	public InvalidConditionExpressionException()
	{
	}

	public InvalidConditionExpressionException(string message)
		: base(AlterMessage(message))
	{
	}

	public InvalidConditionExpressionException(string message, Exception inner)
		: base(AlterMessage(message), inner)
	{
	}

	protected InvalidConditionExpressionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string AlterMessage(string originalMessage)
	{
		return string.Format(CultureInfo.CurrentCulture, "Invalid conditional breakpoint expression: {0}", originalMessage);
	}
}
