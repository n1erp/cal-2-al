using System;

namespace Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;

internal class DataModificationHandler : IDataModificationListener
{
	private readonly Action<NavRecord, DataModificationType> handler;

	public int Priority { get; }

	public DataModificationHandler(Action<NavRecord, DataModificationType> handler, int priority = 0)
	{
		if (priority >= DataModificationListener.HighestPriority)
		{
			throw new ArgumentException("DataModificationListener priority is too high.", "priority");
		}
		if (handler == null)
		{
			throw new ArgumentNullException("handler");
		}
		Priority = priority;
		this.handler = handler;
	}

	public bool CanCoexistWith(IDataModificationListener other)
	{
		return true;
	}

	public void OnBeforeInsert(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.Insert);
	}

	public void OnBeforeModify(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.Modify);
	}

	public void OnBeforeDelete(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.Delete);
	}

	public void OnBeforeRename(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.Rename);
	}

	public void OnBeforeModifyAll(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.ModifyAll);
	}

	public void OnBeforeDeleteAll(NavRecord record)
	{
		handler?.Invoke(record, DataModificationType.DeleteAll);
	}
}
