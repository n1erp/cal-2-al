using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavDatabaseSharedSchemaExtensionMethods
{
	public static bool HasSequencedAutoIncrementField(this NavDatabase database, NCLMetaTable table, NavAppTableContext tableContext)
	{
		if (tableContext == NavAppTableContext.Main && database.IsSharedTenantSchema && table.IsTenantTable && !table.IsCrossTenantTable)
		{
			return table.AutoIncrementField != null;
		}
		return false;
	}

	public static bool HasCompanyIdColumn(this NavDatabase database, NCLMetaTable table)
	{
		if (database.IsSharedTenantSchema && !table.IsCrossTenantTable)
		{
			return table.IsTenantTable;
		}
		return false;
	}

	public static CompanyId? GetCompanyIdForSystemTables(this NavDatabase database, int systemTableId)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(systemTableId, -1, requireCompiled: false);
		return database.GetCompanyId(metaTableById, 0);
	}

	public static CompanyId? GetCompanyId(this NavDatabase database, NCLMetaTable table, int companyToken)
	{
		if (database.DatabaseType == NavDatabaseType.Tenant && database.HasCompanyIdColumn(table) && database.IsSharedTenantSchema)
		{
			if (table.IsAppArchiveTable)
			{
				return null;
			}
			if (database.Tenant.IsSystemTenant)
			{
				return null;
			}
			if (database.TenantProperties.IsNewTenant)
			{
				throw new InvalidOperationException("The tenant is not created yet, and so cannot have companies.");
			}
			if (!table.DataPerCompany)
			{
				companyToken = 0;
			}
			else if (companyToken == 0)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.GetCompanyIdInvalidCompanyTokenForPerCompanyTable, companyToken, table.TableName));
			}
			return database.GetCompanyIdFromToken(companyToken);
		}
		return null;
	}

	private static CompanyId? GetCompanyIdFromToken(this NavDatabase database, int companyToken)
	{
		return database.SharedSchemaExtension.GetCompanyIdFromToken(companyToken);
	}

	public static void ResetCompanyIdCache(this NavDatabase database)
	{
		database.SharedSchemaExtension.ResetCompanyIdCache();
	}

	public static TablePartitionMetadata DefaultPartitionMetadata(this NavDatabase database, NCLMetaTable table)
	{
		if (ServerUserSettings.Instance.SqlPartitionsCount.Value == 1 || !database.HasCompanyIdColumn(table))
		{
			return null;
		}
		return TablePartitionMetadata.CompanyIdPartitionMetadata;
	}
}
