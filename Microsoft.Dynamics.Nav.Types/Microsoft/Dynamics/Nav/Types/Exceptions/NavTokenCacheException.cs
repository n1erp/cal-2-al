using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTokenCacheException : NavException
{
	public NavTokenCacheException()
	{
	}

	public NavTokenCacheException(string message)
		: base(message)
	{
	}

	public NavTokenCacheException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavTokenCacheException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
