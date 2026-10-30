using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportProcessingOnlyException : NavNCLException
{
	public NavNCLReportProcessingOnlyException()
	{
	}

	public NavNCLReportProcessingOnlyException(string message)
		: base(message)
	{
	}

	public NavNCLReportProcessingOnlyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportProcessingOnlyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportProcessingOnlyException Create(CultureInfo culture, int reportId)
	{
		return new NavNCLReportProcessingOnlyException(string.Format(culture, Lang.ProcessingOnlyReport, reportId));
	}
}
