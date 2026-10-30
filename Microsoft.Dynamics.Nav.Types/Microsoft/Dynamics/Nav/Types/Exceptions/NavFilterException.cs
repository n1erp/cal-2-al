using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavFilterException : NavException
{
	public NavFilterException()
	{
	}

	public NavFilterException(string message)
		: base(message)
	{
	}

	public NavFilterException(CultureInfo culture, NavFilter filter)
		: base(culture, Lang.FilterOnInvalidField, DiagnosticParameter.SystemMetadata(filter.FilterField), DiagnosticParameter.CustomerContent(filter.FilterValue))
	{
	}

	public NavFilterException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavFilterException(string exceptionMessage, string diagnosticsMessage, Exception inner)
		: base(exceptionMessage, diagnosticsMessage, inner)
	{
	}

	protected NavFilterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavFilterException(Exception innerException, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(innerException, culture, message, args)
	{
	}

	public static NavFilterException CreateFromInner(NavBaseException inner)
	{
		return new NavFilterException(inner.Message, inner.DiagnosticsMessage, inner);
	}
}
