using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidFormatStringException : NavNCLException
{
	public NavNCLInvalidFormatStringException()
	{
	}

	public NavNCLInvalidFormatStringException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidFormatStringException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidFormatStringException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidFormatStringException Create(CultureInfo culture, string format)
	{
		return new NavNCLInvalidFormatStringException(string.Format(culture, Lang.InvalidFormatString, format));
	}
}
