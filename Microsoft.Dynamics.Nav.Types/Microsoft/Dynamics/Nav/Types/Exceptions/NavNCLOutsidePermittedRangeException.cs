using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLOutsidePermittedRangeException : NavNCLException
{
	private const string DecimalFormatString = "#,0.##################";

	private const string IntegerFormatStringWithGrouping = "N0";

	private const string IntegerFormatStringWithoutGrouping = null;

	private const int NoFormattingCutoffMin = 0;

	private const int NoFormattingCutoffMax = 9999;

	public NavNCLOutsidePermittedRangeException(string parameterName, int parameterNumber, int currentValue, int lowerBound, int upperBound)
		: base(CreateMessage(parameterName, parameterNumber, currentValue, lowerBound, upperBound))
	{
	}

	public NavNCLOutsidePermittedRangeException(string parameterName, int parameterNumber, decimal currentValue, decimal lowerBound, decimal upperBound)
		: base(CreateMessage(parameterName, parameterNumber, currentValue, lowerBound, upperBound, "#,0.##################"))
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	public NavNCLOutsidePermittedRangeException()
		: base(Lang._00095_50105)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLOutsidePermittedRangeException(string message)
		: base(message)
	{
	}

	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavNCLOutsidePermittedRangeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLOutsidePermittedRangeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string parameterName, int parameterNumber, int currentValue, int lowerBound, int upperBound)
	{
		string formatString = ((IsWithinYearRange(lowerBound) && IsWithinYearRange(upperBound)) ? null : "N0");
		return CreateMessage(parameterName, parameterNumber, currentValue, lowerBound, upperBound, formatString);
	}

	private static bool IsWithinYearRange(int value)
	{
		if (value <= 9999)
		{
			return value >= 0;
		}
		return false;
	}

	private static string CreateMessage(string parameterName, int parameterNumber, IFormattable currentValue, IFormattable lowerBound, IFormattable upperBound, string formatString)
	{
		string text = ((parameterNumber == 0) ? string.Empty : parameterNumber.ToString(CultureInfo.CurrentCulture));
		string text2 = currentValue.ToString(formatString, CultureInfo.CurrentCulture);
		string text3 = lowerBound.ToString(formatString, CultureInfo.CurrentCulture);
		string text4 = upperBound.ToString(formatString, CultureInfo.CurrentCulture);
		return string.Format(CultureInfo.CurrentCulture, Lang._00095_50010, parameterName, text, text2, text3, text4);
	}
}
