using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportNoLayoutException : NavNCLException
{
	public NavNCLReportNoLayoutException()
	{
	}

	public NavNCLReportNoLayoutException(string message)
		: base(message)
	{
	}

	public NavNCLReportNoLayoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportNoLayoutException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavNCLReportNoLayoutException Create(CultureInfo culture, string layoutMessage, int reportId)
	{
		return new NavNCLReportNoLayoutException(string.Format(culture, layoutMessage, reportId));
	}
}
