using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavSortingException : NavCSideException
{
	public NavSortingException()
	{
	}

	public NavSortingException(string message)
		: base(message)
	{
	}

	public NavSortingException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavSortingException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavSortingException(int errorCode, string message)
		: base(errorCode, message)
	{
	}
}
