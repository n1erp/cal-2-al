using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavGraphException : NavExternalDataProviderException
{
	public NavGraphException()
	{
	}

	public NavGraphException(string message)
		: base(message)
	{
	}

	public NavGraphException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavGraphException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static NavGraphException Create(string message)
	{
		return new NavGraphException(message);
	}
}
