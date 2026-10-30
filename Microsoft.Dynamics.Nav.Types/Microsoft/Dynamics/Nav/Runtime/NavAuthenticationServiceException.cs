using System;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

[Serializable]
public class NavAuthenticationServiceException : NavException
{
	public NavAuthenticationServiceException()
	{
	}

	public NavAuthenticationServiceException(string message)
		: base(message)
	{
	}

	protected NavAuthenticationServiceException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public NavAuthenticationServiceException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
