using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLClrException : NavNCLException
{
	public NavNCLClrException()
	{
	}

	public NavNCLClrException(string message)
		: base(message)
	{
	}

	public NavNCLClrException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLClrException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
