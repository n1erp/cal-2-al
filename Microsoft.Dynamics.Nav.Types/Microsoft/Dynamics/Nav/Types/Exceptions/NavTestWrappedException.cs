using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTestWrappedException : NavTestBaseException
{
	public NavTestWrappedException()
	{
	}

	public NavTestWrappedException(string message)
		: base(message)
	{
	}

	public NavTestWrappedException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTestWrappedException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
