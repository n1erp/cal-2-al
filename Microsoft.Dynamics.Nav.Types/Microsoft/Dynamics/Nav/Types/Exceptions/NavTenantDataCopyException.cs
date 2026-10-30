using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavTenantDataCopyException : NavNCLException
{
	public NavTenantDataCopyException()
	{
	}

	public NavTenantDataCopyException(string message)
		: base(message)
	{
	}

	protected NavTenantDataCopyException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavTenantDataCopyException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
