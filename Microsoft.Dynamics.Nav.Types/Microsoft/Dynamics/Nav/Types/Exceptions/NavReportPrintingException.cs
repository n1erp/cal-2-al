using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportPrintingException : NavBaseException
{
	public NavReportPrintingException()
	{
	}

	public NavReportPrintingException(string message)
		: base(message)
	{
	}

	public NavReportPrintingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavReportPrintingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
