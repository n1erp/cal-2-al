using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideDataException : NavCSideException
{
	public const int DBPrimRecordNotFound = 18023402;

	public const int DBRecordNotFound = 85132272;

	public const int DBObjectNotFound = 18023495;

	public const int DBTableNotFound = 85132265;

	public const int DBFlowErrorCode = 18023439;

	public NavCSideDataException()
	{
	}

	public NavCSideDataException(string message)
		: base(message)
	{
	}

	public NavCSideDataException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavCSideDataException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavCSideDataException(int errorCode, string message)
		: base(errorCode, message)
	{
	}

	public NavCSideDataException(int errorCode, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(errorCode, culture, message, args)
	{
	}

	public NavCSideDataException(int errorCode, string message, Exception innerException)
		: base(errorCode, message, innerException)
	{
	}

	public static bool IsApplicable(int errorCode)
	{
		switch (errorCode)
		{
		case 18023402:
		case 18023439:
		case 18023495:
		case 85132265:
		case 85132272:
			return true;
		default:
			return false;
		}
	}

	public static NavCSideDataException CreateForThisFilter(int errorCode, string tableCaption, string filterDescription)
	{
		return new NavCSideDataException(errorCode, CultureInfo.CurrentCulture, Lang.NothingFoundWithinFilter, DiagnosticParameter.SystemMetadata(tableCaption), DiagnosticParameter.CustomerContent(filterDescription));
	}
}
