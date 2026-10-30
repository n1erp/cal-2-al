using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLConversionCastException : NavNCLException
{
	public NavNCLConversionCastException()
	{
	}

	public NavNCLConversionCastException(string message)
		: base(message)
	{
	}

	public NavNCLConversionCastException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLConversionCastException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLConversionCastException Create(CultureInfo culture, Type fromType, Type toType)
	{
		return Create(culture, fromType, null, toType);
	}

	public static NavNCLConversionCastException Create(CultureInfo culture, Type fromType, object fromValue, Type toType)
	{
		string message = ((fromValue != null) ? string.Format(culture, Lang.ConversionCastException, fromType, fromValue, toType) : string.Format(culture, Lang._00095_50005, fromType, toType));
		return new NavNCLConversionCastException(message);
	}
}
