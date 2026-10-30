using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLUnexpectedClrException : NavNCLException
{
	public NavNCLUnexpectedClrException()
	{
	}

	public NavNCLUnexpectedClrException(string message)
		: base(message)
	{
	}

	public NavNCLUnexpectedClrException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLUnexpectedClrException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
