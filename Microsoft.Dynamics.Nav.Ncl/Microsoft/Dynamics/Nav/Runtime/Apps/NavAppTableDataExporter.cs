using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.TableData;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppTableDataExporter
{
	private readonly NavTenant tenant;

	public NavAppTableDataExporter(NavTenant tenant)
	{
		this.tenant = tenant;
	}

	public int Export(Stream destination, int tableId)
	{
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
		NavSqlConnectionScope tenantConScope = null;
		try
		{
			NavSqlMetadata.LockAppDatabaseForChangesToMetadata(conScope);
			tenantConScope = NavSqlConnectionScope.CreateForceNewConnection(tenant.Database, TransactionType.UpdateNoLocks);
			NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(tenantConScope);
			int rowCount = 0;
			tenant.Database.DatabaseSync.PerformSynchronizationAction(() =>
			{
				//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b8: Expected Obj, but got Unknown
				NCLMetaTable table = NavGlobal.NCLMetadata.GetMetaTableById(tableId, tenant.NavAppGroup.GroupId, requireCompiled: false);
				if (table.DataPerCompany)
				{
					string message = string.Format(CultureInfo.InvariantCulture, "The table with ID '{0}' cannot be exported because it stores data per company.", table.TableId);
					throw new NavNCLArgumentException(message);
				}
				if (table.IsSystemOrVirtualTable)
				{
					string message2 = string.Format(CultureInfo.InvariantCulture, "The table with ID '{0}' cannot be exported because it stores system information.", table.TableId);
					throw new NavNCLArgumentException(message2);
				}
				Func<BinaryWriter, int> func = (BinaryWriter bw) => NavAppTableDataSqlExporter.Export(bw, tenant.Database, tenantConScope, table);
				NavAppTableDataWriter val = new NavAppTableDataWriter(func);
				rowCount = val.Write(destination, tableId, (Func<IEnumerable<string>>)(() => from f in table.SqlReadableFields
					where !f.FieldIsTimestamp
					select f.FieldName));
				if (rowCount == 0)
				{
					string message3 = string.Format(CultureInfo.InvariantCulture, "The table with ID '{0}' cannot be exported because it is empty.", tableId);
					throw new NavNCLArgumentException(message3);
				}
			});
			return rowCount;
		}
		finally
		{
			if (tenantConScope != null)
			{
				tenantConScope.Dispose();
			}
		}
	}
}
