using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class NavFilterErrorOnQueryCloseException : NavBaseException
{
	public NavFilterErrorOnQueryCloseException()
	{
	}

	public NavFilterErrorOnQueryCloseException(string message)
		: base(message)
	{
	}

	public NavFilterErrorOnQueryCloseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavFilterErrorOnQueryCloseException(string message, NavBaseException innerException)
		: base(string.Format(CultureInfo.CurrentCulture, message, innerException.Message), innerException)
	{
		if (!string.IsNullOrEmpty(innerException.DiagnosticsMessage))
		{
			DiagnosticsMessage = string.Format(CultureInfo.CurrentCulture, message, innerException.DiagnosticsMessage);
		}
	}

	protected NavFilterErrorOnQueryCloseException(SerializationInfo serializationInfo, StreamingContext streamingContext)
		: base(serializationInfo, streamingContext)
	{
	}
}
