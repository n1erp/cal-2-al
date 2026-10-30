using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportNoDefaultPrinterException : NavNCLException
{
	public NavNCLReportNoDefaultPrinterException()
		: base(Lang.ReportNoDefaultPrinterAvailable)
	{
	}

	public NavNCLReportNoDefaultPrinterException(string message)
		: base(message)
	{
	}

	public NavNCLReportNoDefaultPrinterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportNoDefaultPrinterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportNoDefaultPrinterException Create()
	{
		return new NavNCLReportNoDefaultPrinterException(Lang.ReportNoDefaultPrinterAvailable);
	}
}
