using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidStreamLengthException : NavNCLException
{
	public NavNCLInvalidStreamLengthException()
	{
	}

	public NavNCLInvalidStreamLengthException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidStreamLengthException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidStreamLengthException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidStreamLengthException Create(CultureInfo culture, int length, int expectedLength)
	{
		return new NavNCLInvalidStreamLengthException(string.Format(culture, Lang.InvalidLengthSpecifiedMustBe, length, expectedLength));
	}

	public static NavNCLInvalidStreamLengthException CreateInRange(CultureInfo culture, int length, int minLength, int maxLength)
	{
		return new NavNCLInvalidStreamLengthException(string.Format(culture, Lang.InvalidLengthSpecifiedMustBeInRange, length, minLength, maxLength));
	}
}
