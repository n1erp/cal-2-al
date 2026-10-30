using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class WatsonReportException : Exception
{
	public WatsonReportException()
	{
	}

	public WatsonReportException(string message)
		: base(message)
	{
	}

	public WatsonReportException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected WatsonReportException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
