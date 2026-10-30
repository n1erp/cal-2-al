using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public abstract class NavNCLException : NavException
{
	protected NavNCLException()
	{
	}

	protected NavNCLException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLException(string message)
		: base(message)
	{
	}

	protected NavNCLException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	protected NavNCLException(Exception innerException, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(innerException, culture, message, args)
	{
	}

	protected NavNCLException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
