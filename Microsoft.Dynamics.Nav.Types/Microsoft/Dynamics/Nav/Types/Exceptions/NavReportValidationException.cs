using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public sealed class NavReportValidationException : NavException
{
	[Obsolete("Do not use this constructor. Use the one with specific parameters instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public NavReportValidationException()
	{
	}

	public NavReportValidationException(string message)
		: base(message)
	{
	}

	public NavReportValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	private NavReportValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
