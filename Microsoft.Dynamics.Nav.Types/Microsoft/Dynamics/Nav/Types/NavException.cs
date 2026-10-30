using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.ServiceModel;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public abstract class NavException : NavBaseException
{
	protected NavException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	protected NavException()
	{
	}

	protected NavException(string message)
		: base(message)
	{
	}

	protected NavException(CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(culture, message, args)
	{
	}

	protected NavException(Exception inner, CultureInfo culture, string message, params DiagnosticParameter[] args)
		: base(inner, culture, message, args)
	{
	}

	protected NavException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected NavException(string message, string diagnosticsMessage, Exception inner)
		: base(message, diagnosticsMessage, inner)
	{
	}

	public static FaultException CreateFaultException(string faultCode, string reason)
	{
		FaultCode code = FaultCode.CreateSenderFaultCode(new FaultCode(faultCode));
		return new FaultException(reason, code);
	}
}
