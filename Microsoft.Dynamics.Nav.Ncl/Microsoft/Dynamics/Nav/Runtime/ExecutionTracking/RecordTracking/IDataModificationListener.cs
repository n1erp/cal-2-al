namespace Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;

internal interface IDataModificationListener
{
	int Priority { get; }

	void OnBeforeInsert(NavRecord record);

	void OnBeforeModify(NavRecord record);

	void OnBeforeDelete(NavRecord record);

	void OnBeforeRename(NavRecord record);

	void OnBeforeModifyAll(NavRecord record);

	void OnBeforeDeleteAll(NavRecord record);

	bool CanCoexistWith(IDataModificationListener other);
}
