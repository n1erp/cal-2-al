using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNotSupportedException : NavBaseException
{
	public NavNotSupportedException()
	{
	}

	public NavNotSupportedException(string message)
		: base(message)
	{
	}

	public NavNotSupportedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNotSupportedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
