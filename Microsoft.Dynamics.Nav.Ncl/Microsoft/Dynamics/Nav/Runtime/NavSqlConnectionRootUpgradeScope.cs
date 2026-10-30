using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlConnectionRootUpgradeScope : NavSqlConnectionScope
{
	private bool committed;

	internal override bool IsUpgradeScope => true;

	internal NavSqlConnectionRootUpgradeScope(NavDatabase database)
		: base(database, requireNewConnection: false, TransactionType.UpdateNoLocks, tryReuseConnection: false)
	{
	}

	protected override void TryEndTransactionAndDispose(NavSqlEndTransactionType endType)
	{
		try
		{
			if (endType == NavSqlEndTransactionType.Commit)
			{
				Connection.CheckEndTransaction(endType);
				committed = true;
			}
			else
			{
				Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
			}
		}
		finally
		{
			Dispose();
		}
	}

	protected override void OnDisposing()
	{
		if (!committed)
		{
			Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
		}
	}
}
