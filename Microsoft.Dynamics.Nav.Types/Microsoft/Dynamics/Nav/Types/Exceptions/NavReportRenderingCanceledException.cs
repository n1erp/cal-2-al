using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportRenderingCanceledException : NavBaseException
{
	public NavReportRenderingCanceledException()
	{
	}

	public NavReportRenderingCanceledException(string message)
		: base(message)
	{
	}

	public NavReportRenderingCanceledException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavReportRenderingCanceledException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
