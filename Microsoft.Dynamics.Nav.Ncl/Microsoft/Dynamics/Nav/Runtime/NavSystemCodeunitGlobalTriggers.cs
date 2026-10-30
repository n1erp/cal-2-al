using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavSystemCodeunitGlobalTriggers(TreeObject parent) : NavSystemCodeunit(parent, 2000000002)
{
	[Flags]
	public enum Triggers
	{
		None = 0,
		GlobalInsert = 1,
		GlobalModify = 2,
		GlobalDelete = 4,
		GlobalRename = 8,
		DatabaseInsert = 0x100,
		DatabaseModify = 0x200,
		DatabaseDelete = 0x400,
		DatabaseRename = 0x800
	}

	public enum CodeunitMethod
	{
		GetGlobalTableTriggerMask = 1,
		OnGlobalInsert,
		OnGlobalModify,
		OnGlobalDelete,
		OnGlobalRename,
		GetDatabaseTableTriggerSetup,
		OnDatabaseInsert,
		OnDatabaseModify,
		OnDatabaseDelete,
		OnDatabaseRename
	}

	private Dictionary<int, Triggers> triggersOnTables = new Dictionary<int, Triggers>();

	private bool? onDatabaseModifyHasXRec;

	protected override string GetMethodName(int methodId)
	{
		CodeunitMethod codeunitMethod = (CodeunitMethod)methodId;
		return codeunitMethod.ToString();
	}

	public object InvokeOnDatabaseModify(NavRecordRef rec, NavRecordRef oldRec)
	{
		if (!onDatabaseModifyHasXRec.HasValue)
		{
			onDatabaseModifyHasXRec = false;
		}
		if (onDatabaseModifyHasXRec.Value)
		{
			return Invoke(8, new object[2] { rec, oldRec });
		}
		return Invoke(8, new object[1] { rec });
	}

	public void FlushTriggerCache()
	{
		lock (triggersOnTables)
		{
			triggersOnTables.Clear();
		}
	}

	internal Triggers GetTriggersOnTable(int tableId)
	{
		if (!NavCurrentThread.Session.IsCompanyOpen)
		{
			return Triggers.None;
		}
		lock (triggersOnTables)
		{
			if (triggersOnTables.TryGetValue(tableId, out var value))
			{
				return value;
			}
			triggersOnTables.Add(tableId, Triggers.None);
			int intMask = 0;
			Invoke(1, new object[2]
			{
				tableId,
				new ByRef<int>(() => intMask, (int num) =>
				{
					intMask = num;
				})
			});
			value = (Triggers)(intMask & 0xF);
			bool databaseInsertEnabled = false;
			bool databaseModifyEnabled = false;
			bool databaseDeleteEnabled = false;
			bool databaseRenameEnabled = false;
			object[] arguments = new object[5]
			{
				tableId,
				new ByRef<bool>(() => databaseInsertEnabled, (bool flag) =>
				{
					databaseInsertEnabled = flag;
				}),
				new ByRef<bool>(() => databaseModifyEnabled, (bool flag) =>
				{
					databaseModifyEnabled = flag;
				}),
				new ByRef<bool>(() => databaseDeleteEnabled, (bool flag) =>
				{
					databaseDeleteEnabled = flag;
				}),
				new ByRef<bool>(() => databaseRenameEnabled, (bool flag) =>
				{
					databaseRenameEnabled = flag;
				})
			};
			Invoke(6, arguments);
			if (databaseInsertEnabled)
			{
				value |= Triggers.DatabaseInsert;
			}
			if (databaseModifyEnabled)
			{
				value |= Triggers.DatabaseModify;
			}
			if (databaseDeleteEnabled)
			{
				value |= Triggers.DatabaseDelete;
			}
			if (databaseRenameEnabled)
			{
				value |= Triggers.DatabaseRename;
			}
			triggersOnTables.Remove(tableId);
			triggersOnTables.Add(tableId, value);
			return value;
		}
	}
}
