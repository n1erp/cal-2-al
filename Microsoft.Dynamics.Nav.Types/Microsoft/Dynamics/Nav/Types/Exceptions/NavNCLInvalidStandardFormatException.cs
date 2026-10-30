using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidStandardFormatException : NavNCLException
{
	public NavNCLInvalidStandardFormatException()
	{
	}

	public NavNCLInvalidStandardFormatException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidStandardFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidStandardFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidStandardFormatException Create(CultureInfo culture, int formatType, NavType type)
	{
		return new NavNCLInvalidStandardFormatException(string.Format(culture, Lang.InvalidStandardFormat, formatType, type.ToString()));
	}
}
