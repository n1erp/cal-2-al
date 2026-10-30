using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlConnectionUpgradeScope : NavSqlConnectionScope
{
	internal override bool IsUpgradeScope => true;

	private string ActiveSavepoint { get; set; }

	internal NavSqlConnectionUpgradeScope(NavDatabase database, bool requireNewConnection, TransactionType transactionType)
		: base(database, requireNewConnection, transactionType, tryReuseConnection: false)
	{
		if (!ParentScope.IsUpgradeScope)
		{
			throw new NavUpgradeScopeException("An NavSqlConnectionUpgradeScope that is not a root scope must have an NavSqlConnectionUpgradeScope parent.");
		}
		ActiveSavepoint = Connection.SetTransactionSavepoint();
	}

	protected override void TryEndTransactionAndDispose(NavSqlEndTransactionType endType)
	{
		try
		{
			if (endType != NavSqlEndTransactionType.Commit)
			{
				Connection.RollbackTransactionSavepoint(ActiveSavepoint);
			}
		}
		finally
		{
			Dispose();
		}
	}
}
