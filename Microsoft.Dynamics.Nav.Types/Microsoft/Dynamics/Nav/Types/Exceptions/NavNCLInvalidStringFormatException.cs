using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidStringFormatException : NavNCLException
{
	public NavNCLInvalidStringFormatException()
	{
	}

	public NavNCLInvalidStringFormatException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidStringFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidStringFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLInvalidStringFormatException CreateNoValueForIndex(CultureInfo culture, string source, int index)
	{
		return new NavNCLInvalidStringFormatException(string.Format(culture, Lang.ALSelectStringCommaStringIndexNotValue, source, index));
	}

	public static NavNCLInvalidStringFormatException CreateNotEqualLengthForConvert(CultureInfo culture, string fromCharacters, string toCharacters)
	{
		return new NavNCLInvalidStringFormatException(string.Format(culture, Lang.ALConvertStringExpectsEqualLength, fromCharacters, toCharacters));
	}
}
