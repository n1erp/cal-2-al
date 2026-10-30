using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUpgradeProcessExecutionException : NavNCLException
{
	public NavUpgradeProcessExecutionException()
	{
	}

	public NavUpgradeProcessExecutionException(string message)
		: base(message)
	{
	}

	public NavUpgradeProcessExecutionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUpgradeProcessExecutionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
