using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportUnsupportedFormatException : NavException
{
	public NavReportUnsupportedFormatException()
	{
	}

	public NavReportUnsupportedFormatException(string message)
		: base(message)
	{
	}

	public NavReportUnsupportedFormatException(string message, string format)
		: base(CreateMessage(message, format))
	{
	}

	public NavReportUnsupportedFormatException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavReportUnsupportedFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string message, string format)
	{
		return string.Format(CultureInfo.CurrentCulture, message, format);
	}
}
