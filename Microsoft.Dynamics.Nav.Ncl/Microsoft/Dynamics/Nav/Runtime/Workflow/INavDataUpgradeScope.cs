namespace Microsoft.Dynamics.Nav.Runtime.Workflow;

public interface INavDataUpgradeScope
{
	bool IsTransactionStarted { get; }

	void StartTransaction(NavSession session, bool forceCreateNew = false);

	void CommitTransaction();
}
