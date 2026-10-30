using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavReportLayoutValidationException : NavException
{
	public NavReportLayoutValidationException()
	{
	}

	public NavReportLayoutValidationException(string message)
		: base(message)
	{
	}

	public NavReportLayoutValidationException(string message, string name)
		: base(CreateMessage(message, name))
	{
	}

	public NavReportLayoutValidationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavReportLayoutValidationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	private static string CreateMessage(string message, string name)
	{
		return string.Format(CultureInfo.CurrentCulture, message, name);
	}
}
