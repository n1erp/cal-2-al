using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUpgradeCodeunitExecutionException : NavNCLException
{
	public NavUpgradeCodeunitExecutionException()
	{
	}

	public NavUpgradeCodeunitExecutionException(string message)
		: base(message)
	{
	}

	public NavUpgradeCodeunitExecutionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUpgradeCodeunitExecutionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
