using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps.TableData;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppTableDataImporter
{
	private readonly NavTenant tenant;

	public NavAppTableDataImporter(NavTenant tenant)
	{
		this.tenant = tenant;
	}

	public void Import(Stream source)
	{
		NavAppTableData val = NavAppTableData.CreateFromStream(source);
		try
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(tenant.Database, updateOrlockingActivity: true);
			SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(navSqlConnectionScope.Connection.Connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock, navSqlConnectionScope.Connection.CurrentTransaction);
			try
			{
				NavAppGroup navAppGroup = NavCurrentThread.TryResolveAppGroup() ?? tenant.NavAppGroup;
				NCLMetaTable table = NavGlobal.NCLMetadata.GetMetaTableById(val.TableId, navAppGroup.GroupId, requireCompiled: false);
				Action<BinaryReader> action = (BinaryReader reader) =>
				{
					DoBulkCopy(reader, sqlBulkCopy, table);
				};
				val.PerformImport(action);
				tenant.Database.CacheSynchronization.RegisterTableDataChanged(val.TableId, 0);
			}
			finally
			{
				if (sqlBulkCopy != null)
				{
					((IDisposable)sqlBulkCopy).Dispose();
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DoBulkCopy(BinaryReader reader, SqlBulkCopy sqlBulkCopy, NCLMetaTable table)
	{
		int num = 0;
		sqlBulkCopy.DestinationTableName = NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), table, tenant.Database, num, NavAppTableContext.Main).ToString();
		sqlBulkCopy.EnableStreaming = true;
		sqlBulkCopy.BulkCopyTimeout = 0;
		IEnumerable<NCLMetaField> enumerable = table.SqlReadableFields.Where((NCLMetaField field) => !field.FieldIsTimestamp);
		foreach (NCLMetaField item in enumerable)
		{
			sqlBulkCopy.ColumnMappings.Add(item.SqlColumnName, item.SqlColumnName);
		}
		CompanyId? companyId = tenant.Database.GetCompanyId(table, num);
		if (companyId.HasValue)
		{
			sqlBulkCopy.ColumnMappings.Add("$companyId", "$companyId");
		}
		NavSqlStreamReaderWriter.SetSqlBulkCopyBatchSize(table, sqlBulkCopy);
		IDataReader reader2 = new NavAppTableDataSqlDataReader(table, companyId, reader);
		sqlBulkCopy.WriteToServer(reader2);
	}
}
