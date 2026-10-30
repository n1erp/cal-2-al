using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportLanguageNotFoundException : NavNCLException
{
	public NavNCLReportLanguageNotFoundException()
	{
	}

	public NavNCLReportLanguageNotFoundException(string message)
		: base(message)
	{
	}

	public NavNCLReportLanguageNotFoundException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportLanguageNotFoundException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportLanguageNotFoundException Create(CultureInfo culture, string errorMessage)
	{
		return new NavNCLReportLanguageNotFoundException(string.Format(culture, Lang.ReportLanguageNotFoundErrorString, errorMessage));
	}
}
