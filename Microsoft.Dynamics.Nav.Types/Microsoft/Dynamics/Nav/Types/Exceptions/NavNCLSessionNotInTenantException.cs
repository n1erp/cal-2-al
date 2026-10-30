using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavNCLSessionNotInTenantException : NavNCLException
{
	public NavNCLSessionNotInTenantException()
	{
	}

	public NavNCLSessionNotInTenantException(string message)
		: base(message)
	{
	}

	public NavNCLSessionNotInTenantException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavNCLSessionNotInTenantException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
