using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLInvalidFormHandleException : NavNCLException
{
	public NavNCLInvalidFormHandleException()
	{
	}

	public NavNCLInvalidFormHandleException(string message)
		: base(message)
	{
	}

	public NavNCLInvalidFormHandleException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLInvalidFormHandleException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
