using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLEvaluateException : NavNCLException
{
	public NavNCLEvaluateException()
	{
	}

	public NavNCLEvaluateException(string message)
		: base(message)
	{
	}

	public NavNCLEvaluateException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLEvaluateException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavNCLEvaluateException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	private NavNCLEvaluateException(Exception innerException, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(innerException, culture, message, args)
	{
	}

	public static NavNCLEvaluateException Create(CultureInfo culture, NavType type, string source)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateError, DiagnosticParameter.SystemMetadata(type.ToString()), DiagnosticParameter.CustomerContent(source));
	}

	public static NavNCLEvaluateException Create(CultureInfo culture, NavType type, string source, Exception innerException)
	{
		throw new NavNCLEvaluateException(innerException, culture, Lang.EvaluateErrorWithMessage, DiagnosticParameter.SystemMetadata(type.ToString()), DiagnosticParameter.CustomerContent(source), DiagnosticParameter.CustomerContent(innerException.Message));
	}

	public static NavNCLEvaluateException CreateDateFormulaSignMissing(CultureInfo culture, string source)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateDateFormulaSignMissingError, DiagnosticParameter.CustomerContent(source));
	}

	public static NavNCLEvaluateException CreateDateFormulaShouldIncludeANumber(CultureInfo culture, string source)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateDateFormulaShouldIncludeANumberError, DiagnosticParameter.CustomerContent(source));
	}

	public static NavNCLEvaluateException CreateDateFormulaShouldIncludeQuantor(CultureInfo culture, string source)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateDateFormulaShouldIncludeAQuantorError, DiagnosticParameter.CustomerContent(source));
	}

	public static NavNCLEvaluateException CreateDateFormulaExceedMaxLength(CultureInfo culture, string source, int maxLength)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateDateFormulaExceedsMaxLengthError, DiagnosticParameter.CustomerContent(source), DiagnosticParameter.SystemMetadata(maxLength));
	}

	public static NavNCLEvaluateException CreateDateFormulaNumberOutOfRange(CultureInfo culture, string source, int lowerBound, int upperBound)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateDateFormulaNumberOutOfRangeError, DiagnosticParameter.CustomerContent(source), DiagnosticParameter.SystemMetadata(lowerBound), DiagnosticParameter.SystemMetadata(upperBound));
	}

	public static NavNCLEvaluateException CreateIntegerValueOutOfRange(CultureInfo culture, string source, int lowerBound, int upperBound)
	{
		throw new NavNCLEvaluateException(culture, Lang.EvaluateIntegerValueOutOfRangeError, DiagnosticParameter.CustomerContent(source), DiagnosticParameter.SystemMetadata(lowerBound), DiagnosticParameter.SystemMetadata(upperBound));
	}
}
