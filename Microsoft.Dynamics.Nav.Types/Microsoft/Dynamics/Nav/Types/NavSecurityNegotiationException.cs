using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public sealed class NavSecurityNegotiationException : NavException
{
	private NavSecurityNegotiationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavSecurityNegotiationException()
	{
	}

	public NavSecurityNegotiationException(string message)
		: base(message)
	{
	}

	public NavSecurityNegotiationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
