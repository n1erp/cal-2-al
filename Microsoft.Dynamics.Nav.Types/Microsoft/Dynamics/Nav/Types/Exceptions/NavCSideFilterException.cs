using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideFilterException : NavCSideException
{
	public const int NotIntervalFilterErrorCode = 18023434;

	public const int MinimumValueMissingErrorCode = 18023432;

	public const int MaximumValueMissingErrorCode = 18023433;

	public NavCSideFilterException()
	{
	}

	public NavCSideFilterException(string message)
		: base(message)
	{
	}

	public NavCSideFilterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCSideFilterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCSideFilterException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavCSideFilterException(int errorCode, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(errorCode, culture, message, args)
	{
	}

	public static NavCSideFilterException CreateForNoSingleRange(int errorCode, CultureInfo culture, string fieldName, string tableName, string filter)
	{
		return new NavCSideFilterException(errorCode, culture, Lang.GetRangeNoSingleRange, DiagnosticParameter.CustomerContent(filter), DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(tableName));
	}

	public static NavCSideFilterException CreateForNoPossibleValues(int errorCode, CultureInfo culture, string fieldName, string tableName, string filter)
	{
		return new NavCSideFilterException(errorCode, culture, Lang.GetRangeNoPossibleValues, DiagnosticParameter.CustomerContent(filter), DiagnosticParameter.SystemMetadata(fieldName), DiagnosticParameter.SystemMetadata(tableName));
	}
}
