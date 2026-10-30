using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportRenderingException : NavBaseException
{
	public NavReportRenderingException()
	{
	}

	public NavReportRenderingException(string message)
		: base(message)
	{
	}

	public NavReportRenderingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavReportRenderingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavReportRenderingException Create(CultureInfo culture, string errorMessage)
	{
		return new NavReportRenderingException(string.Format(culture, Lang.ReportRenderError, errorMessage));
	}
}
