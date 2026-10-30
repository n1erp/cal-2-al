using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUpgradeScopeException : NavException
{
	public NavUpgradeScopeException()
	{
	}

	public NavUpgradeScopeException(string message)
		: base(message)
	{
	}

	public NavUpgradeScopeException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUpgradeScopeException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
