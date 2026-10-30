using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLPrinterNotInstalledOnServerException : NavNCLException
{
	public NavNCLPrinterNotInstalledOnServerException()
	{
	}

	public NavNCLPrinterNotInstalledOnServerException(string message)
		: base(message)
	{
	}

	public NavNCLPrinterNotInstalledOnServerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLPrinterNotInstalledOnServerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLPrinterNotInstalledOnServerException Create(CultureInfo culture, string printerName, int reportId, string reportName)
	{
		return new NavNCLPrinterNotInstalledOnServerException(string.Format(culture, Lang.PrinterNotInstalledOnServer, printerName, reportId, reportName));
	}
}
