using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class TablesAndContextsHelper
{
	public static IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> GetTablesAndContexts(NavDatabase tenantDatabase, IEnumerable<NCLMetaTable> baseTables, IEnumerable<AppSyncSnapshotRecord> snapshotRecords)
	{
		if (baseTables == null)
		{
			throw new ArgumentNullException("baseTables");
		}
		if (snapshotRecords == null)
		{
			throw new ArgumentNullException("snapshotRecords");
		}
		List<Tuple<NCLMetaTable, NavAppTableContext>> list = baseTables.Select((NCLMetaTable t) => Tuple.Create(t, NavAppTableContext.Main)).ToList();
		List<AppSyncSnapshotRecord> list2 = snapshotRecords.ToList();
		list2.Sort((AppSyncSnapshotRecord x, AppSyncSnapshotRecord y) =>
		{
			int num = ((WrappedComparable<Guid>)(object)x.AppId).CompareTo((object)y.AppId);
			if (num == 0)
			{
				num = x.TargetTableId.CompareTo(y.TargetTableId);
			}
			return num;
		});
		foreach (AppSyncSnapshotRecord item in list2)
		{
			NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, item.TargetTableId, tenantDatabase.Tenant.NavAppGroup);
			nCLMetaTable.AssignFromMetaTable(item.TableSnapshot, databaseConvertedFromNav2013: false);
			list.Add(Tuple.Create(nCLMetaTable, item.IsTableExtension ? NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)item.AppId).Value)) : NavAppTableContext.Main));
		}
		return list;
	}
}
