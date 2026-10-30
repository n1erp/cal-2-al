using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavGlobalTriggers
{
	public static void Insert(NavRecord record, bool runGlobalTrigger)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.IsTemporary)
		{
			return;
		}
		runGlobalTrigger &= record.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalInsert);
		bool flag = record.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseInsert);
		if (!(runGlobalTrigger | flag))
		{
			return;
		}
		using NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session.Company, record.SecurityFiltering);
		navRecordRef.Record = record;
		if (runGlobalTrigger)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(2, new object[1] { navRecordRef });
		}
		if (flag)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(7, new object[1] { navRecordRef });
		}
	}

	public static void Modify(NavRecord newRecord, NavRecord oldRecord, bool runGlobalTrigger)
	{
		if (newRecord == null)
		{
			throw new ArgumentNullException("newRecord");
		}
		if (oldRecord == null && runGlobalTrigger)
		{
			throw new ArgumentNullException("oldRecord");
		}
		if (newRecord.IsTemporary)
		{
			return;
		}
		runGlobalTrigger &= newRecord.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalModify);
		bool flag = newRecord.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseModify);
		if (!(runGlobalTrigger | flag))
		{
			return;
		}
		using NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session.Company, newRecord.SecurityFiltering);
		navRecordRef.Record = newRecord;
		using NavRecordRef navRecordRef2 = new NavRecordRef(NavCurrentThread.Session.Company, oldRecord.SecurityFiltering);
		navRecordRef2.Record = oldRecord;
		if (runGlobalTrigger)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(3, new object[2] { navRecordRef, navRecordRef2 });
		}
		if (flag)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.InvokeOnDatabaseModify(navRecordRef, navRecordRef2);
		}
	}

	public static void Delete(NavRecord record, bool runGlobalTrigger)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.IsTemporary)
		{
			return;
		}
		runGlobalTrigger &= record.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalDelete);
		bool flag = record.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseDelete);
		if (!(runGlobalTrigger | flag))
		{
			return;
		}
		using NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session.Company, record.SecurityFiltering);
		navRecordRef.Record = record;
		if (runGlobalTrigger)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(4, new object[1] { navRecordRef });
		}
		if (flag)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(9, new object[1] { navRecordRef });
		}
	}

	public static void Rename(NavRecord newRecord, NavRecord oldRecord, bool runGlobalTrigger)
	{
		if (newRecord == null)
		{
			throw new ArgumentNullException("newRecord");
		}
		if (oldRecord == null)
		{
			throw new ArgumentNullException("oldRecord");
		}
		if (newRecord.IsTemporary)
		{
			return;
		}
		runGlobalTrigger &= newRecord.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalRename);
		bool flag = newRecord.MetaTable.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseRename);
		if (!(runGlobalTrigger | flag))
		{
			return;
		}
		using NavRecordRef navRecordRef = new NavRecordRef(NavCurrentThread.Session.Company, newRecord.SecurityFiltering);
		navRecordRef.Record = newRecord;
		using NavRecordRef navRecordRef2 = new NavRecordRef(NavCurrentThread.Session.Company, oldRecord.SecurityFiltering);
		navRecordRef2.Record = oldRecord;
		if (runGlobalTrigger)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(5, new object[2] { navRecordRef, navRecordRef2 });
		}
		if (flag)
		{
			NavCurrentThread.Session.SystemCodeunitFactory.GlobalTriggers.Invoke(10, new object[2] { navRecordRef, navRecordRef2 });
		}
	}
}
