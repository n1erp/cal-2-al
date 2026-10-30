using System;
using Microsoft.Dynamics.Nav.Types.Exceptions.NCL.Upgrade;

namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

internal class NavDataUpgradeScope : INavDataUpgradeScope
{
	private NavSqlConnectionRootUpgradeScope rootUpgradeScope;

	public bool IsTransactionStarted => rootUpgradeScope != null;

	public void StartTransaction(NavSession session, bool forceCreateNew = false)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (forceCreateNew && IsTransactionStarted)
		{
			rootUpgradeScope.TransactionEnd(NavSqlEndTransactionType.Commit);
			rootUpgradeScope = null;
		}
		else if (IsTransactionStarted)
		{
			throw new NavDataUpgradeTransactionException("The data upgrade transaction has already started.");
		}
		rootUpgradeScope = NavSqlConnectionScope.CreateRootUpgradeScope(session.Database);
	}

	public void CommitTransaction()
	{
		if (!IsTransactionStarted)
		{
			throw new NavDataUpgradeTransactionException("The data upgrade transaction has not been started.");
		}
		rootUpgradeScope.TransactionEnd(NavSqlEndTransactionType.Commit);
		rootUpgradeScope = null;
	}
}
