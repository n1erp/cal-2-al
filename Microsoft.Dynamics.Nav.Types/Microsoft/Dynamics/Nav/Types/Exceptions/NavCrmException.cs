using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCrmException : NavNCLException
{
	public NavCrmException()
	{
	}

	public NavCrmException(string message)
		: base(message)
	{
	}

	public NavCrmException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavCrmException(Exception innerException, CultureInfo culture, string message, DiagnosticParameter parameter)
		: base(innerException, culture, message, parameter)
	{
	}

	protected NavCrmException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavCrmException Create(string message)
	{
		return new NavCrmException(message);
	}

	public static NavCrmException Create(string message, Exception innerException)
	{
		return new NavCrmException(message, innerException);
	}

	public static NavCrmException CreateForRequestFailure(string detailedDescription, Exception innerException)
	{
		return new NavCrmException(innerException, CultureInfo.CurrentCulture, Lang.CrmRequestFailure, DiagnosticParameter.CustomerContent(detailedDescription));
	}
}
