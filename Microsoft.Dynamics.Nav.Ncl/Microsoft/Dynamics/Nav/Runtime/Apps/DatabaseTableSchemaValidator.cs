using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public static class DatabaseTableSchemaValidator
{
	private static readonly List<string> TablesToExclude = new List<string> { "Object Metadata Snapshot" };

	private const string NdoTablePrefix = "$ndo";

	private const string AppDataTablePrefix = "$AppData";

	internal static TestTenantDatabaseSchemaResponse ValidateSchema(string tenantId)
	{
		NavTenant mountedTenant = NavTenantManagementTasks.GetMountedTenant(tenantId);
		if (!mountedTenant.Database.IsCurrentVersion)
		{
			throw new NavTenantDatabaseException(string.Format(CultureInfo.InvariantCulture, "The {0} database has version {1} which is not supported by this version of {2}. Run Sync-NAVTenantDatabase to upgrade or initialize the tenant database.", mountedTenant.TenantSettings.TenantDatabaseId, mountedTenant.Database.TenantDatabaseProperties.VersionNo, ProductNames.ProductNameServer));
		}
		List<NavTenantDatabaseSchemaFault> list = new List<NavTenantDatabaseSchemaFault>();
		using (NavSqlConnectionScope tenantDatabaseScope = NavSqlConnectionScope.Create(mountedTenant.Database, TransactionType.UpdateNoLocks))
		{
			TenantDatabaseTableMetadataRetriever tenantDatabaseTableMetadataRetriever = new TenantDatabaseTableMetadataRetriever(mountedTenant.Database);
			List<Tuple<NCLMetaTable, NavAppTableContext>> list2 = (from pair in tenantDatabaseTableMetadataRetriever.GetSnapshotOfAllTenantTablesAndContexts(tenantDatabaseScope)
				where !pair.Item1.IsExternalDataSource
				select pair).ToList();
			List<DatabaseTable> allTablesInDatabase = new TenantDatabaseMetadataRetriever(mountedTenant.Database).AllSqlTables.ToList();
			allTablesInDatabase = FilterOutApplicationDatabaseTables(allTablesInDatabase);
			foreach (Tuple<NCLMetaTable, NavAppTableContext> item2 in list2)
			{
				if (item2.Item1.DataPerCompany && !mountedTenant.Database.IsSharedTenantSchema)
				{
					int[] companyTokens = mountedTenant.Database.GetCompanyTokens();
					foreach (int token in companyTokens)
					{
						string companyNameFromToken = mountedTenant.Database.GetCompanyNameFromToken(token);
						list.AddRange(CompareMetadataAndSqlTableDefinitions(mountedTenant.Database, item2.Item1, item2.Item2, allTablesInDatabase, companyNameFromToken));
					}
				}
				else
				{
					list.AddRange(CompareMetadataAndSqlTableDefinitions(mountedTenant.Database, item2.Item1, item2.Item2, allTablesInDatabase));
				}
			}
			foreach (DatabaseTable item3 in allTablesInDatabase)
			{
				NavTenantDatabaseSchemaFault item = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.AdditionalTable, item3.Name, -1);
				list.Add(item);
			}
		}
		return new TestTenantDatabaseSchemaResponse(list);
	}

	private static List<NavTenantDatabaseSchemaFault> CompareMetadataAndSqlTableDefinitions(NavDatabase database, NCLMetaTable metaTable, NavAppTableContext context, List<DatabaseTable> whatTheyActuallyLookLike, string companyName = null)
	{
		List<NavTenantDatabaseSchemaFault> list = new List<NavTenantDatabaseSchemaFault>();
		string databaseTableName = ((!string.IsNullOrEmpty(companyName)) ? (companyName + "$" + metaTable.TableName) : metaTable.TableName);
		if (context.TableType == NavAppTableContextType.Companion)
		{
			databaseTableName = databaseTableName + "$" + context.CompanionTableAppOrigin.AppId.ToString();
		}
		else if (context.TableType == NavAppTableContextType.Main && metaTable.SourceExtensionType == NavExtensionType.ModernDev)
		{
			databaseTableName = databaseTableName + "$" + metaTable.SourceAppId.Value.ToString();
		}
		databaseTableName = NavSqlStatementHelper.ConvertToSqlIdentifier(databaseTableName);
		DatabaseTable databaseTable = whatTheyActuallyLookLike.FirstOrDefault((DatabaseTable table) => table.Name == databaseTableName);
		if (databaseTable == null)
		{
			NavTenantDatabaseSchemaFault item = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.MissingTable, databaseTableName, metaTable.TableId);
			list.Add(item);
		}
		else
		{
			list.AddRange(CompareFields(database, metaTable, databaseTable, databaseTableName));
			list.AddRange(CompareKeys(metaTable, databaseTable, databaseTableName, database.IsSharedTenantSchema));
			whatTheyActuallyLookLike.Remove(databaseTable);
		}
		return list;
	}

	private static List<NavTenantDatabaseSchemaFault> CompareFields(NavDatabase database, NCLMetaTable metaTable, DatabaseTable databaseTable, string databaseTableName)
	{
		List<NavTenantDatabaseSchemaFault> list = new List<NavTenantDatabaseSchemaFault>();
		if (database.IsSharedTenantSchema)
		{
			databaseTable.RemoveColumns("$companyId");
		}
		foreach (NCLMetaField field in metaTable.Fields)
		{
			if (!field.FieldBelongsInPhysicalTable(field.OriginatingTableContext) || field.Field.FieldClass != FieldClass.Normal || field.FieldIsUserDefinedTimestamp)
			{
				continue;
			}
			string fieldName = NavSqlStatementHelper.ConvertToSqlIdentifier(field.FieldName);
			StringBuilder stringBuilder = new StringBuilder();
			NavSqlStatementHelper.AppendSqlColumnTypeAndLength(stringBuilder, field.FieldNclType, field.SqlDataType, field.FieldDefinedLength);
			string text = stringBuilder.ToString();
			if (text.Contains("("))
			{
				text = text.Substring(0, text.IndexOf('('));
			}
			int fieldDefinedLength = field.FieldDefinedLength;
			DatabaseColumn databaseColumn = databaseTable.ColumnNames.FirstOrDefault((DatabaseColumn column) => string.Equals(column.Name, fieldName, StringComparison.CurrentCultureIgnoreCase));
			if (databaseColumn == null)
			{
				NavTenantDatabaseSchemaFault item = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.MissingField, databaseTableName, metaTable.TableId, fieldName);
				list.Add(item);
				continue;
			}
			if (!string.Equals(databaseColumn.SqlType, text, StringComparison.CurrentCultureIgnoreCase))
			{
				NavTenantDatabaseSchemaFault item2 = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.IncorrectFieldType, databaseTableName, metaTable.TableId, fieldName);
				list.Add(item2);
			}
			else if ((databaseColumn.Length != -1 || field.FieldNclType == NavNclType.NavText) && databaseColumn.Length != fieldDefinedLength && field.FieldNclType != NavNclType.NavBlob)
			{
				NavTenantDatabaseSchemaFault item3 = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.IncorrectFieldLength, databaseTableName, metaTable.TableId, fieldName);
				list.Add(item3);
			}
			databaseTable.RemoveColumns(fieldName);
		}
		if (databaseTable.ColumnNames.Any())
		{
			foreach (string item5 in databaseTable.ColumnNames.Select((DatabaseColumn column) => column.Name))
			{
				NavTenantDatabaseSchemaFault item4 = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.AdditionalField, databaseTableName, metaTable.TableId, item5);
				list.Add(item4);
			}
		}
		return list;
	}

	private static List<NavTenantDatabaseSchemaFault> CompareKeys(NCLMetaTable metaTable, DatabaseTable databaseTable, string databaseTableName, bool isSharedTenantSchema)
	{
		List<NavTenantDatabaseSchemaFault> list = new List<NavTenantDatabaseSchemaFault>();
		foreach (DatabaseIndex index in databaseTable.Indexes)
		{
			if (isSharedTenantSchema)
			{
				index.RemoveColumns("$companyId");
			}
		}
		NCLMetaKey[] keys = metaTable.Keys;
		foreach (NCLMetaKey nCLMetaKey in keys)
		{
			if (nCLMetaKey.MaintainSqlIndex || nCLMetaKey.Clustered)
			{
				List<string> keyFields = (nCLMetaKey.Unique ? nCLMetaKey.KeyFieldsList.Select((NCLMetaField field) => field.FieldName).ToList() : nCLMetaKey.KeyFieldsWithPrimaryKeyFields.Select((INavFieldMetadata field) => ((NCLMetaField)field).FieldName).ToList());
				keyFields = (from field in keyFields.Select(NavSqlStatementHelper.ConvertToSqlIdentifier)
					orderby field
					select field).Distinct().ToList();
				DatabaseIndex databaseIndex = databaseTable.Indexes.FirstOrDefault((DatabaseIndex index) => index.ColumnNames.SequenceEqual(keyFields));
				if (databaseIndex == null)
				{
					NavTenantDatabaseSchemaFault item = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.MissingKey, databaseTableName, metaTable.TableId, nCLMetaKey.GetKeyName());
					list.Add(item);
				}
				databaseTable.RemoveIndex(databaseIndex);
			}
		}
		if (databaseTable.Indexes.Any())
		{
			foreach (string item3 in databaseTable.Indexes.Select((DatabaseIndex index) => index.Name))
			{
				NavTenantDatabaseSchemaFault item2 = new NavTenantDatabaseSchemaFault(NavTenantDatabaseSchemaErrorType.AdditionalKey, databaseTableName, metaTable.TableId, item3);
				list.Add(item2);
			}
		}
		return list;
	}

	private static List<DatabaseTable> FilterOutApplicationDatabaseTables(List<DatabaseTable> allTablesInDatabase)
	{
		List<int> list = SystemTables.ApplicationDatabaseTables.ToList();
		foreach (int item in list)
		{
			string tableName = NavSqlStatementHelper.ConvertToSqlIdentifier(NavGlobal.NCLMetadata.GetTableNameByTableId(item));
			allTablesInDatabase.RemoveAll((DatabaseTable table) => table.Name.Equals(tableName));
		}
		allTablesInDatabase.RemoveAll((DatabaseTable table) => table.Name.StartsWith("$ndo", StringComparison.OrdinalIgnoreCase));
		allTablesInDatabase.RemoveAll((DatabaseTable table) => table.Name.StartsWith("$AppData", StringComparison.OrdinalIgnoreCase));
		allTablesInDatabase.RemoveAll((DatabaseTable table) => TablesToExclude.Select(NavSqlStatementHelper.ConvertToSqlIdentifier).Contains(table.Name));
		return allTablesInDatabase;
	}
}
