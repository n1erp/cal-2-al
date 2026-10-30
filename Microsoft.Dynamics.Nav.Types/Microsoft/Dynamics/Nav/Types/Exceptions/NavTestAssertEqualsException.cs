using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestAssertEqualsException : NavTestBaseException
{
	public NavTestAssertEqualsException()
	{
	}

	public NavTestAssertEqualsException(string message)
		: base(message)
	{
	}

	public NavTestAssertEqualsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestAssertEqualsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavTestAssertEqualsException Create(CultureInfo culture, string fieldName, string expectedValue, string actualValue)
	{
		return new NavTestAssertEqualsException(string.Format(culture, Lang.TestAssertEqualsExpectedActual, fieldName, expectedValue, actualValue));
	}
}
