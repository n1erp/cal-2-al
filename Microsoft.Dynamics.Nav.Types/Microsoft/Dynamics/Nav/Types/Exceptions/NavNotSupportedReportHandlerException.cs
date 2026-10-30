using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedReportHandlerException : NavNotSupportedException
{
	public NavNotSupportedReportHandlerException()
	{
	}

	public NavNotSupportedReportHandlerException(string message)
		: base(message)
	{
	}

	public NavNotSupportedReportHandlerException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedReportHandlerException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
