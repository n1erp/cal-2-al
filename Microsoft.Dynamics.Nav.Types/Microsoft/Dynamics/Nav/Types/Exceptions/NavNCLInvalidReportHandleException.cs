using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidReportHandleException : NavNCLException
{
	public NavNCLInvalidReportHandleException()
	{
	}

	public NavNCLInvalidReportHandleException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidReportHandleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidReportHandleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
