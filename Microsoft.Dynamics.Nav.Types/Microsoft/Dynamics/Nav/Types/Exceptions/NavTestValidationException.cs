using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestValidationException : NavTestBaseException
{
	public NavTestValidationException()
	{
	}

	public NavTestValidationException(string message)
		: base(message)
	{
	}

	public NavTestValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestValidationException Create(CultureInfo culture, string fieldName, string validationText)
	{
		return new NavTestValidationException(string.Format(culture, Lang.TestValidationException, fieldName, validationText));
	}
}
