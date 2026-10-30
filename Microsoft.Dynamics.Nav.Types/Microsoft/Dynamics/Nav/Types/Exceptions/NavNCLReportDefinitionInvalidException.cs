using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportDefinitionInvalidException : NavNCLException
{
	public NavNCLReportDefinitionInvalidException()
	{
	}

	public NavNCLReportDefinitionInvalidException(string message)
		: base(message)
	{
	}

	public NavNCLReportDefinitionInvalidException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportDefinitionInvalidException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportDefinitionInvalidException CreateNotTransformed(CultureInfo culture, int reportId)
	{
		return new NavNCLReportDefinitionInvalidException(string.Format(culture, Lang.ReportNotTransformedException, ProductNames.ProductNameClassicClient, reportId));
	}
}
