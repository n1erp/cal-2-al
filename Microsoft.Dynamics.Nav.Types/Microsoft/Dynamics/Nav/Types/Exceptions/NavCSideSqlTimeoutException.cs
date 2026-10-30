using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavCSideSqlTimeoutException : NavCSideException
{
	private const int SqlErrorTimeoutErrorCode = -2;

	public NavCSideSqlTimeoutException()
	{
	}

	public NavCSideSqlTimeoutException(string message)
		: base(message)
	{
	}

	public NavCSideSqlTimeoutException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavCSideSqlTimeoutException(string message, Exception innerException, bool remappable)
		: base(-2, message, innerException, remappable)
	{
	}

	protected NavCSideSqlTimeoutException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
