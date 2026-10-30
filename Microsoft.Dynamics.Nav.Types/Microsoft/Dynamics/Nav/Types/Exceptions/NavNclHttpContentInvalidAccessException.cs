using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclHttpContentInvalidAccessException : NavNCLException
{
	public NavNclHttpContentInvalidAccessException()
	{
	}

	public NavNclHttpContentInvalidAccessException(string message)
		: base(message)
	{
	}

	public NavNclHttpContentInvalidAccessException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclHttpContentInvalidAccessException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
