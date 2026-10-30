using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavExportException : NavException
{
	public NavExportException()
	{
	}

	public NavExportException(string message)
		: base(message)
	{
	}

	public NavExportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavExportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavExportException CreateCouldNotExportForm(CultureInfo culture, Exception innerException)
	{
		string arg = string.Empty;
		if (innerException != null)
		{
			arg = innerException.Message;
		}
		return new NavExportException(string.Format(culture, Lang.CouldNotExportForm, arg), innerException);
	}

	public static NavExportException CreateRecordsNumberExceeded(CultureInfo culture, int maxRecordsNumber)
	{
		return new NavExportException(string.Format(culture, Lang.OfficeExportToManyRecords, maxRecordsNumber));
	}

	public static NavExportException CreateNoDataInTheRepeater(CultureInfo culture)
	{
		return new NavExportException(string.Format(culture, Lang.OfficeExportNoDataInTheRepeater));
	}
}
