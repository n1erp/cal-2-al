using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavConversionCastOverflowException : NavNCLException
{
	public NavConversionCastOverflowException()
	{
	}

	public NavConversionCastOverflowException(string message)
		: base(message)
	{
	}

	public NavConversionCastOverflowException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavConversionCastOverflowException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavConversionCastOverflowException Create(IFormatProvider formatProvider, NavType fromType, NavType toType)
	{
		return new NavConversionCastOverflowException(string.Format(formatProvider, Lang._00095_50005, fromType, toType));
	}
}
