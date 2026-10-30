using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TenantDatabaseTableMetadataRetriever
{
	private readonly NavDatabase tenantDatabase;

	public TenantDatabaseTableMetadataRetriever(NavDatabase tenantDatabase)
	{
		this.tenantDatabase = tenantDatabase;
	}

	public IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> GetSnapshotOfAllTenantTablesAndContexts(NavSqlConnectionScope tenantDatabaseScope)
	{
		List<NCLMetaTable> source = NavGlobal.NCLMetadata.GetSnapshotOfAllNonVirtualMetaTablesExceptApplicationTables(0).ToList();
		IAppSyncTrackingRepository appSyncTrackingRepository = new DatabaseAppSyncTrackingRepository(tenantDatabase, tenantDatabaseScope);
		IAppSyncSnapshotRepository snapshotRepository = new DatabaseAppSyncSnapshotRepository(tenantDatabase, tenantDatabaseScope);
		IEnumerable<AppId> source2 = (from r in appSyncTrackingRepository.GetAll()
			select r.AppId).Distinct();
		List<Tuple<NCLMetaTable, NavAppTableContext>> list = source.Select((NCLMetaTable t) => Tuple.Create(t, NavAppTableContext.Main)).ToList();
		List<Tuple<NCLMetaTable, NavAppTableContext>> list2 = new List<Tuple<NCLMetaTable, NavAppTableContext>>();
		IEnumerable<IGrouping<int, AppSyncSnapshotRecord>> enumerable = from s in source2.SelectMany((AppId r) => snapshotRepository.GetAllForApp(r))
			group s by s.TargetTableId;
		foreach (IGrouping<int, AppSyncSnapshotRecord> item in enumerable)
		{
			int key = item.Key;
			foreach (AppSyncSnapshotRecord item2 in item)
			{
				MetaTable tableSnapshot = item2.TableSnapshot;
				NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, key, tenantDatabase.Tenant.NavAppGroup);
				nCLMetaTable.AssignFromMetaTable(tableSnapshot, databaseConvertedFromNav2013: false);
				if (!item2.IsTableExtension)
				{
					list.Add(Tuple.Create(nCLMetaTable, NavAppTableContext.Main));
				}
				else
				{
					list2.Add(Tuple.Create(nCLMetaTable, NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)item2.AppId).Value))));
				}
			}
		}
		return list.Concat(list2);
	}
}
