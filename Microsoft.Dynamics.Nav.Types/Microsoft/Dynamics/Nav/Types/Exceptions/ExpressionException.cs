using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class ExpressionException : NavBaseException
{
	public ExpressionException()
	{
	}

	public ExpressionException(string message)
		: base(message)
	{
	}

	public ExpressionException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ExpressionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static ExpressionException Create(string expression, int index, string error, Exception innerException)
	{
		return new ExpressionException(string.Format(CultureInfo.InvariantCulture, "Error compiling dynamic control expression: '{0}' at index '{1}'. The error is: {2}.", expression, index, error), innerException);
	}
}
