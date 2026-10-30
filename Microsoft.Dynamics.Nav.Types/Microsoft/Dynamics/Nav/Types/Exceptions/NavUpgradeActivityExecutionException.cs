using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUpgradeActivityExecutionException : NavNCLException
{
	public NavUpgradeActivityExecutionException()
	{
	}

	public NavUpgradeActivityExecutionException(string message)
		: base(message)
	{
	}

	public NavUpgradeActivityExecutionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUpgradeActivityExecutionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
