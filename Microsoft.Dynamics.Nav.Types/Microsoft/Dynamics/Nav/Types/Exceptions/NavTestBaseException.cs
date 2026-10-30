using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestBaseException : NavNCLException
{
	public NavTestBaseException()
	{
	}

	public NavTestBaseException(string message)
		: base(message)
	{
	}

	public NavTestBaseException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestBaseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
