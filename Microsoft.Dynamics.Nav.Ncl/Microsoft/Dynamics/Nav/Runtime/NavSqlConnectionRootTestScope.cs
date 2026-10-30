using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlConnectionRootTestScope : NavSqlConnectionTestScope
{
	private string activeSavepoint;

	public override string ActiveSavepoint
	{
		get
		{
			return activeSavepoint;
		}
		set
		{
			activeSavepoint = value;
		}
	}

	internal NavSqlConnectionRootTestScope(NavDatabase database, bool requireNewConnection, TransactionType transactionType)
		: base(database, requireNewConnection, transactionType)
	{
		activeSavepoint = Connection.SetTransactionSavepoint();
	}

	protected override void TryEndTransactionAndDispose(NavSqlEndTransactionType endType)
	{
		if (endType == NavSqlEndTransactionType.Commit)
		{
			ActiveSavepoint = Connection.SetTransactionSavepoint();
		}
		else
		{
			Connection.RollbackTransactionSavepoint(ActiveSavepoint);
		}
	}

	protected override void OnDisposing()
	{
		NavCurrentThread.Session.FlushAllDataAccessCaches();
		if (FindAncestorWithConnection(Connection) is NavSqlConnectionTestScope { IsTestScope: not false, ActiveSavepoint: not null } navSqlConnectionTestScope)
		{
			Connection.RollbackTransactionSavepoint(navSqlConnectionTestScope.ActiveSavepoint);
			return;
		}
		try
		{
			Connection.CheckEndTransaction(NavSqlEndTransactionType.Rollback);
		}
		catch (NavBaseException ex) when (((ex as NavSqlException) ?? ex.InnerException) is NavSqlException ex2 && ex2.ErrorNumber == -2)
		{
		}
	}
}
