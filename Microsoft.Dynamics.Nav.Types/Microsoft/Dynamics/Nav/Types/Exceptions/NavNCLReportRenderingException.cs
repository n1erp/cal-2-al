using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportRenderingException : NavNCLException
{
	public NavNCLReportRenderingException()
	{
	}

	public NavNCLReportRenderingException(string message)
		: base(message)
	{
	}

	public NavNCLReportRenderingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportRenderingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportRenderingException Create(CultureInfo culture, string errorMessage)
	{
		return new NavNCLReportRenderingException(string.Format(culture, Lang.ReportRenderError, errorMessage));
	}
}
