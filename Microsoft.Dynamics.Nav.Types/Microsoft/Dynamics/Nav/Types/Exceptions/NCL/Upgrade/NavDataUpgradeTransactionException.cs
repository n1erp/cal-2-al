using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types.Exceptions.NCL.Upgrade;

[Serializable]
internal class NavDataUpgradeTransactionException : NavNCLException
{
	public NavDataUpgradeTransactionException()
	{
	}

	public NavDataUpgradeTransactionException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public NavDataUpgradeTransactionException(string message)
		: base(message)
	{
	}

	protected NavDataUpgradeTransactionException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
