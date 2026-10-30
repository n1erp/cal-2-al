using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavExternalDataProviderException : NavNCLException
{
	public NavExternalDataProviderException()
	{
	}

	public NavExternalDataProviderException(string message)
		: base(message)
	{
	}

	public NavExternalDataProviderException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavExternalDataProviderException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
