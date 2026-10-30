using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidOptionStringException : NavNCLEvaluateException
{
	public NavNCLInvalidOptionStringException()
	{
	}

	public NavNCLInvalidOptionStringException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidOptionStringException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidOptionStringException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidOptionStringException Create(CultureInfo culture, string option, string validOptions)
	{
		return new NavNCLInvalidOptionStringException(string.Format(culture, Lang._00095_50063, option, validOptions));
	}
}
