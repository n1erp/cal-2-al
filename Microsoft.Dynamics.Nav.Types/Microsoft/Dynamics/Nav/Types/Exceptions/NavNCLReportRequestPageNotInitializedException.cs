using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportRequestPageNotInitializedException : NavNCLException
{
	public NavNCLReportRequestPageNotInitializedException()
	{
	}

	public NavNCLReportRequestPageNotInitializedException(string message)
		: base(message)
	{
	}

	public NavNCLReportRequestPageNotInitializedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportRequestPageNotInitializedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportRequestPageNotInitializedException Create(CultureInfo culture, int reportId)
	{
		return new NavNCLReportRequestPageNotInitializedException(string.Format(culture, Lang.ReportRequestpageNotInitialized, reportId));
	}
}
