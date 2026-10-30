using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions;

[Serializable]
public class NavUpgradeCodeunitOnRunCallException : NavNCLException
{
	public NavUpgradeCodeunitOnRunCallException()
	{
	}

	public NavUpgradeCodeunitOnRunCallException(string message)
		: base(message)
	{
	}

	public NavUpgradeCodeunitOnRunCallException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	protected NavUpgradeCodeunitOnRunCallException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
