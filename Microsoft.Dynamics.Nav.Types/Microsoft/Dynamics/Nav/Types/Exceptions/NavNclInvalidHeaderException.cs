using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNclInvalidHeaderException : NavNCLException
{
	public NavNclInvalidHeaderException()
	{
	}

	public NavNclInvalidHeaderException(string message)
		: base(message)
	{
	}

	public NavNclInvalidHeaderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNclInvalidHeaderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
