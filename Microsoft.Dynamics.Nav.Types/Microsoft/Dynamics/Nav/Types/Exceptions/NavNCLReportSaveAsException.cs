using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLReportSaveAsException : NavNCLException
{
	public NavNCLReportSaveAsException()
	{
	}

	public NavNCLReportSaveAsException(string message)
		: base(message)
	{
	}

	public NavNCLReportSaveAsException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLReportSaveAsException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
