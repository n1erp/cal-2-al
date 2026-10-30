using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlConnectionTestScope : NavSqlConnectionScope
{
	private bool originalUserIsSuper;

	private string activeSavepoint;

	internal override bool IsTestScope => true;

	public virtual string ActiveSavepoint
	{
		get
		{
			if (activeSavepoint == null)
			{
				activeSavepoint = (FindAncestorWithConnection(Connection) as NavSqlConnectionTestScope)?.ActiveSavepoint;
			}
			return activeSavepoint;
		}
		set
		{
			activeSavepoint = value;
			NavSqlConnectionTestScope navSqlConnectionTestScope = FindAncestorWithConnection(Connection) as NavSqlConnectionTestScope;
			navSqlConnectionTestScope.ActiveSavepoint = value;
		}
	}

	internal NavSqlConnectionTestScope(NavDatabase database, bool requireNewConnection, TransactionType transactionType)
		: base(database, requireNewConnection, transactionType, tryReuseConnection: false)
	{
		NavUser navUser = NavCurrentThread.Session?.Authenticator?.User;
		if (navUser != null)
		{
			originalUserIsSuper = navUser.IsDefaultSuper;
		}
	}

	protected void RestoreUsers()
	{
		NavUser navUser = NavCurrentThread.Session?.Authenticator?.User;
		if (navUser != null && !navUser.IsDefaultSuper && originalUserIsSuper)
		{
			NavUserCache navUsers = Connection.Database.NavUsers;
			navUsers.RefreshList();
			if (navUsers.IsEmpty)
			{
				navUser.RestoreDefaultSuperGuid();
			}
		}
	}

	protected override void TryEndTransactionAndDispose(NavSqlEndTransactionType endType)
	{
		try
		{
			if (Connection.CurrentTransaction == null || Connection.CurrentTransaction.Connection == null)
			{
				throw new NavCSideException(18022407, "Transaction was rolled back by the server.");
			}
			if (endType == NavSqlEndTransactionType.Commit)
			{
				ActiveSavepoint = Connection.SetTransactionSavepoint();
			}
			else
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
