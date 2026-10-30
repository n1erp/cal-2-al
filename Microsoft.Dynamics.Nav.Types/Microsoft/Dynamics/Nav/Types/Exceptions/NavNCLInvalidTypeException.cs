using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidTypeException : NavNCLException
{
	public NavNCLInvalidTypeException()
	{
	}

	public NavNCLInvalidTypeException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidTypeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidTypeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidTypeException CreateExpectedType(CultureInfo culture, Type expectedType)
	{
		return new NavNCLInvalidTypeException(string.Format(culture, Lang._00095_50001, expectedType));
	}
}
