using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlMetadata
{
	private static readonly Lazy<NCLMetaQueryDefinition> hashesFromTenantQuery = new Lazy<NCLMetaQueryDefinition>(GetHashesFromTenantQuery);

	private static readonly Lazy<NCLMetaQueryDefinition> baseHashesFromTenantQuery = new Lazy<NCLMetaQueryDefinition>(GetBaseHashesFromTenantQuery);

	private static readonly Lazy<NCLMetaQueryDefinition> hashesFromAppQuery = new Lazy<NCLMetaQueryDefinition>(GetHashesFromAppQuery);

	private static readonly ReadOnlyCollection<NCLMetaField> EmptyFieldCollection = new ReadOnlyCollection<NCLMetaField>(new List<NCLMetaField>());

	private static bool ArekeyFieldListModified(ReadOnlyCollection<NCLMetaField> oldKeyFieldsList, ReadOnlyCollection<NCLMetaField> newKeyFieldsList, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges)
	{
		if (oldKeyFieldsList.Count == newKeyFieldsList.Count)
		{
			return oldKeyFieldsList.Where((NCLMetaField oldIndexfield, int fieldIndex) => oldIndexfield.FieldNo != newKeyFieldsList[fieldIndex].FieldNo || !fieldsAndChanges.First((NavSqlFieldAndChanges fieldAndChange) => fieldAndChange.OldField != null && fieldAndChange.OldField.FieldNo == oldIndexfield.FieldNo).NoChange).Any();
		}
		return true;
	}

	private static IndexChangeTypes CompareSqlIndex(NCLMetaKey oldKey, NCLMetaKey newKey, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges)
	{
		if (newKey != null && newKey.MaintainSqlIndex)
		{
			if (oldKey == null || !oldKey.MaintainSqlIndex)
			{
				return IndexChangeTypes.Missing;
			}
			IndexChangeTypes indexChangeTypes = IndexChangeTypes.None;
			if (ArekeyFieldListModified(oldKey.KeyFieldsList, newKey.KeyFieldsList, fieldsAndChanges))
			{
				indexChangeTypes |= IndexChangeTypes.IndexFields;
			}
			if (ArekeyFieldListModified(oldKey.SqlIndexFields ?? EmptyFieldCollection, newKey.SqlIndexFields ?? EmptyFieldCollection, fieldsAndChanges))
			{
				indexChangeTypes |= IndexChangeTypes.IndexFields;
			}
			if (oldKey.Clustered != newKey.Clustered)
			{
				indexChangeTypes |= IndexChangeTypes.ClusterIndex;
			}
			if (oldKey.IsPrimaryKey != newKey.IsPrimaryKey)
			{
				indexChangeTypes |= IndexChangeTypes.PrimaryKey;
			}
			if (oldKey.Unique != newKey.Unique)
			{
				indexChangeTypes |= IndexChangeTypes.Unique;
			}
			return indexChangeTypes;
		}
		if (oldKey != null && oldKey.MaintainSqlIndex)
		{
			return IndexChangeTypes.Excessive;
		}
		return IndexChangeTypes.None;
	}

	private static IndexChangeTypes CompareSiftIndex(NCLMetaKey oldKey, NCLMetaKey newKey, bool tableRenamed, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges)
	{
		if (newKey != null && newKey.MaintainSumIndex)
		{
			if (oldKey == null || !oldKey.MaintainSumIndex)
			{
				return IndexChangeTypes.SiftMissing;
			}
			IndexChangeTypes indexChangeTypes = IndexChangeTypes.None;
			if (ArekeyFieldListModified(new ReadOnlyCollection<NCLMetaField>(oldKey.SumFieldsList.AsNclMetaFields().ToArray()), new ReadOnlyCollection<NCLMetaField>(newKey.SumFieldsList.AsNclMetaFields().ToArray()), fieldsAndChanges))
			{
				indexChangeTypes |= IndexChangeTypes.SumFields;
			}
			if (ArekeyFieldListModified(oldKey.KeyFieldsList, newKey.KeyFieldsList, fieldsAndChanges))
			{
				indexChangeTypes |= IndexChangeTypes.SumFields;
			}
			if (tableRenamed)
			{
				indexChangeTypes |= IndexChangeTypes.SiftExcessive | IndexChangeTypes.SiftMissing;
			}
			return indexChangeTypes;
		}
		if (oldKey != null && oldKey.MaintainSumIndex)
		{
			return IndexChangeTypes.SiftExcessive;
		}
		return IndexChangeTypes.None;
	}

	public static ReadOnlyCollection<NavSqlIndexAndChanges> GetIndexesAndChanges(NCLMetaTable oldTable, NCLMetaTable table, IEnumerable<NavSqlFieldAndChanges> fieldsAndChanges, bool databaseConvertedFromNav2013)
	{
		List<NavSqlIndexAndChanges> list = new List<NavSqlIndexAndChanges>();
		bool tableRenamed = string.CompareOrdinal(oldTable.TableName, table.TableName) != 0 || oldTable.DataPerCompany != table.DataPerCompany;
		NCLMetaKey[] array = (databaseConvertedFromNav2013 ? oldTable.Keys.Where((NCLMetaKey key) => key.KeyIsActive).ToArray() : oldTable.Keys);
		for (int num = 0; num < Math.Max(table.Keys.Length, array.Length); num++)
		{
			NCLMetaKey nCLMetaKey = ((num >= array.Length) ? null : array[num]);
			NCLMetaKey nCLMetaKey2 = ((num >= oldTable.Keys.Length) ? null : oldTable.Keys[num]);
			NCLMetaKey nCLMetaKey3 = ((num >= table.Keys.Length) ? null : table.Keys[num]);
			if (nCLMetaKey3 != null)
			{
				IEnumerable<NCLMetaField> source = nCLMetaKey3.KeyFieldsList.Where((NCLMetaField keyField) => keyField.FieldActive && !fieldsAndChanges.Any((NavSqlFieldAndChanges field) => field.NewField != null && field.NewField.FieldNo == keyField.FieldNo));
				if (source.Any())
				{
					NCLMetaField nCLMetaField = source.First();
					throw new NavSqlDatabaseSyncException(string.Format(CultureInfo.CurrentCulture, Lang.SchemaSynchronizationKeyContainsAFieldNotAllowdAsAKey, table.TableName, nCLMetaKey3.GetKeyName(), nCLMetaField.FieldName));
				}
			}
			IndexChangeTypes indexChangeTypes = CompareSqlIndex(nCLMetaKey, nCLMetaKey3, fieldsAndChanges) | CompareSiftIndex(nCLMetaKey2, nCLMetaKey3, tableRenamed, fieldsAndChanges);
			if (indexChangeTypes != IndexChangeTypes.None)
			{
				list.Add(new NavSqlIndexAndChanges(nCLMetaKey, nCLMetaKey2, indexChangeTypes, nCLMetaKey3));
			}
		}
		return new ReadOnlyCollection<NavSqlIndexAndChanges>(list);
	}

	public static ReadOnlyCollection<NavSqlFieldAndChanges> GetFieldsAndChanges(NCLMetaTable oldTable, NCLMetaTable newTable, bool sortingPropertiesChanged)
	{
		List<NavSqlFieldAndChanges> list = new List<NavSqlFieldAndChanges>();
		Func<NCLMetaField, bool> predicate = (NCLMetaField field) => !field.FieldIsTimestamp && !field.FieldIsUserDefinedTimestamp && field.FieldActive;
		Dictionary<int, NCLMetaField> dictionary = oldTable.NormalFields.Where(predicate).ToDictionary((NCLMetaField field) => field.FieldNo);
		foreach (NCLMetaField item in newTable.NormalFields.Where(predicate))
		{
			if (!dictionary.TryGetValue(item.FieldNo, out var value))
			{
				list.Add(new NavSqlFieldAndChanges(value, FieldChangeTypes.Missing, item));
				continue;
			}
			dictionary.Remove(value.FieldNo);
			FieldChangeTypes fieldChangeTypes = FieldChangeTypes.None;
			if (value.FieldNclType != item.FieldNclType)
			{
				fieldChangeTypes |= FieldChangeTypes.DataType;
			}
			else
			{
				if (value.FieldNclType == NavNclType.NavCode && value.SqlDataTypeWithDefaultMapping != item.SqlDataTypeWithDefaultMapping)
				{
					fieldChangeTypes |= FieldChangeTypes.SqlDataType;
				}
				if (value.IsStringType && item.IsStringType)
				{
					if (item.SqlDataTypeWithDefaultMapping != SqlDataType.Variant && value.FieldDefinedLength != item.FieldDefinedLength)
					{
						fieldChangeTypes |= FieldChangeTypes.Length;
					}
					if (sortingPropertiesChanged)
					{
						fieldChangeTypes |= FieldChangeTypes.Sorting;
					}
				}
				if (value.FieldIsAutoIncrement != item.FieldIsAutoIncrement)
				{
					fieldChangeTypes |= FieldChangeTypes.AutoIncrement;
				}
				if (value.FieldNclType == NavNclType.NavBlob && value.FieldIsCompressed != item.FieldIsCompressed)
				{
					fieldChangeTypes |= FieldChangeTypes.Compression;
				}
			}
			if (string.CompareOrdinal(value.FieldName, item.FieldName) != 0)
			{
				fieldChangeTypes |= FieldChangeTypes.Name;
			}
			list.Add(new NavSqlFieldAndChanges(value, fieldChangeTypes, item));
		}
		list.AddRange(dictionary.Select((KeyValuePair<int, NCLMetaField> item) => new NavSqlFieldAndChanges(item.Value, FieldChangeTypes.Excessive, null)));
		return new ReadOnlyCollection<NavSqlFieldAndChanges>(list.ToArray());
	}

	public static IReadOnlyCollection<string> CheckUpgradeTableDesign(NavSqlTableAndChange oldTableChange)
	{
		IList<string> list = new List<string>();
		bool flag = oldTableChange.OldTable.NormalFields.SequenceEqual(oldTableChange.DataUpgradeTable.NormalFields, new UpgradeFieldDesignEqualityComparer());
		NCLMetaKey primaryKey = oldTableChange.OldTable.PrimaryKey;
		NCLMetaKey primaryKey2 = oldTableChange.DataUpgradeTable.PrimaryKey;
		if (!(from field in primaryKey.DefinedKeyFields.AsNclMetaFields()
			select field.FieldNo).SequenceEqual(from field in primaryKey2.DefinedKeyFields.AsNclMetaFields()
			select field.FieldNo))
		{
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeTableAndOriginalTablePrimaryKeysNotMatching, oldTableChange.DataUpgradeTable.TableName, oldTableChange.OldTable.TableName));
		}
		if (flag)
		{
			return new ReadOnlyCollection<string>(list);
		}
		if (oldTableChange.DataUpgradeSetup == DataUpgradeSetup.OldPopulatedNewEmpty)
		{
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeTableDefinitionDifferFromOldTable, oldTableChange.DataUpgradeTable.TableName, oldTableChange.OldTable.TableName));
		}
		IEnumerable<string> first = from field in oldTableChange.OldTable.NormalFields.Except(oldTableChange.DataUpgradeTable.NormalFields, new UpgradeFieldDesignEqualityComparer())
			select field.FieldName;
		IEnumerable<string> enumerable = from field in oldTableChange.DataUpgradeTable.NormalFields.Except(oldTableChange.OldTable.NormalFields, new UpgradeFieldDesignEqualityComparer())
			select field.FieldName;
		IEnumerable<string> enumerable2 = first.Intersect(enumerable);
		if (enumerable2.Any())
		{
			string arg = string.Join(", ", enumerable2);
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.FieldsOfUpgradeTableDifferFromOriginalTable, oldTableChange.DataUpgradeTable.TableName, oldTableChange.OldTable.TableName, arg));
		}
		IEnumerable<string> first2 = (oldTableChange.Excessive ? oldTableChange.OldTable.NormalFields.Select((NCLMetaField field) => field.FieldName) : (from found in oldTableChange.FieldsAndChanges
			where found.IsUnsafeChange
			select found.OldField.FieldName));
		IEnumerable<string> enumerable3 = first2.Except(oldTableChange.DataUpgradeTable.NormalFields.Select((NCLMetaField field) => field.FieldName));
		if (enumerable3.Any())
		{
			string arg2 = string.Join(", ", enumerable3);
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.FieldsAffectedByDestructiveChangeInOldTableAndMissingInUpgradeTable, arg2, oldTableChange.OldTable.TableName, oldTableChange.DataUpgradeTable.TableName));
		}
		if (oldTableChange.DataUpgradeSetup != DataUpgradeSetup.OldPopulatedNewEmpty)
		{
			return new ReadOnlyCollection<string>(list);
		}
		first = first.Except(enumerable2);
		if (first.Any())
		{
			string arg3 = string.Join(", ", first);
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.OriginalTableWithFieldsNonExistingInUpgradeTable, oldTableChange.OldTable.TableName, oldTableChange.DataUpgradeTable.TableName, arg3));
		}
		enumerable = enumerable.Except(enumerable2);
		if (enumerable.Any())
		{
			string arg4 = string.Join(", ", enumerable);
			list.Add(string.Format(CultureInfo.CurrentCulture, Lang.UpgradeTableWithFieldsNonExistingInOldTable, oldTableChange.DataUpgradeTable.TableName, oldTableChange.OldTable.TableName, arg4));
		}
		return new ReadOnlyCollection<string>(list);
	}

	public static void PopulateBaseTableMetadataInTenantDatabaseForAllRecordsWhereMissing(NavDatabase tenantDatabase)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(33);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(36);
		NavOption navOption = NavOption.Create(fieldByNo2.FieldOptionMetadata, 1);
		Dictionary<int, NavText> tableIdsAndMetadataHashesFromAppDatabase = GetTableIdsAndMetadataHashesFromAppDatabase();
		Dictionary<int, NavText> tableIdsAndMetadataHashesFromTenantDatabase = GetTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
		Dictionary<int, NavText> baseTableIdsAndMetadataHashesFromTenantDatabase = GetBaseTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
		foreach (KeyValuePair<int, NavText> item in tableIdsAndMetadataHashesFromAppDatabase)
		{
			int key = item.Key;
			if (tableIdsAndMetadataHashesFromTenantDatabase.TryGetValue(key, out var _))
			{
				baseTableIdsAndMetadataHashesFromTenantDatabase.TryGetValue(key, out var value2);
				if (value2 == null || value2 == NavText.Empty)
				{
					ApplicationObjectId objectId = new ApplicationObjectId(ObjectType.Table, key);
					NCLObjectXmlMetadata unmodifiedBaseObjectXml = NavGlobal.NCLObjectXmlMetadataLoader.GetUnmodifiedBaseObjectXml(objectId);
					XmlDocument document = unmodifiedBaseObjectXml.Document;
					NavBLOB value3 = new NavBLOB(Encoding.UTF8.GetBytes(document.OuterXml))
					{
						IsDirty = true
					};
					NavText metadataHash = unmodifiedBaseObjectXml.MetadataHash;
					NavValue[] array = new NavValue[metaTableById.FieldCount];
					array[fieldByNo2.FieldIndex] = navOption;
					array[fieldByNo3.FieldIndex] = NavInteger.Create(item.Key);
					MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(metaTableById, array));
					mutableRecordBuffer[fieldByNo.FieldIndex] = value3;
					mutableRecordBuffer[fieldByNo4.FieldIndex] = metadataHash;
					NavSqlDmlCommand.Modify(tenantDatabase, 0, mutableRecordBuffer, recordHasBeenReadWithLockInsideThisTransaction: true, out var _);
				}
			}
		}
	}

	public static Dictionary<int, MetadataHashTuple> GetTableIdsAndMetadataHashTuplesFromAppDatabase()
	{
		return GetTableIdsAndMetadataHashTuplesFromDatabase(NavGlobal.AppDatabase, hashesFromAppQuery.Value);
	}

	public static Dictionary<int, MetadataHashTuple> GetTableIdsAndMetadataHashTuplesFromTenantDatabase(NavDatabase tenantDatabase)
	{
		return GetTableIdsAndMetadataHashTuplesFromDatabase(tenantDatabase, hashesFromTenantQuery.Value);
	}

	public static Dictionary<int, NavText> GetTableIdsAndMetadataHashesFromTenantDatabase(NavDatabase tenantDatabase)
	{
		return GetTableIdsAndMetadataHashTuplesFromTenantDatabase(tenantDatabase).ToDictionary((KeyValuePair<int, MetadataHashTuple> d) => d.Key, (KeyValuePair<int, MetadataHashTuple> d) => d.Value.MetadataHash);
	}

	public static Dictionary<int, MetadataHashTuple> GetBaseTableIdsAndMetadataHashTuplesFromTenantDatabase(NavDatabase tenantDatabase)
	{
		return (from e in GetTableIdsAndMetadataHashTuplesFromDatabase(tenantDatabase, baseHashesFromTenantQuery.Value)
			where e.Value != null && e.Value.MetadataHash != NavText.Empty
			select e).ToDictionary((KeyValuePair<int, MetadataHashTuple> e) => e.Key, (KeyValuePair<int, MetadataHashTuple> e) => e.Value);
	}

	public static Dictionary<int, NavText> GetBaseTableIdsAndMetadataHashesFromTenantDatabase(NavDatabase tenantDatabase)
	{
		return GetBaseTableIdsAndMetadataHashTuplesFromTenantDatabase(tenantDatabase).ToDictionary((KeyValuePair<int, MetadataHashTuple> d) => d.Key, (KeyValuePair<int, MetadataHashTuple> d) => d.Value.MetadataHash);
	}

	public static Dictionary<int, NavText> GetTableIdsAndMetadataHashesFromAppDatabase()
	{
		return GetTableIdsAndMetadataHashTuplesFromAppDatabase().ToDictionary((KeyValuePair<int, MetadataHashTuple> d) => d.Key, (KeyValuePair<int, MetadataHashTuple> d) => d.Value.MetadataHash);
	}

	private static Dictionary<int, MetadataHashTuple> GetTableIdsAndMetadataHashTuplesFromDatabase(NavDatabase database, NCLMetaQueryDefinition query)
	{
		IEnumerable<ReadOnlyRecordBuffer> enumerable = NavSqlQueryCommand.QueryWithSingleScope(query, database, 0, 0, 0, null, null, includeCurrent: false, FindType.All, null, SecurityFiltering.Ignored, addExplicitJoinHints: false);
		Dictionary<int, MetadataHashTuple> dictionary = new Dictionary<int, MetadataHashTuple>();
		foreach (ReadOnlyRecordBuffer item in enumerable)
		{
			int value = ((NavInteger)item[0]).Value;
			if (!dictionary.ContainsKey(value))
			{
				MetadataHashTuple value2 = MetadataHashTuple.Create((NavText)item[1], (NavInteger)item[2]);
				dictionary.Add(value, value2);
			}
		}
		foreach (int item2 in SystemTables.VirtualTables.Concat(SystemTables.ApplicationDatabaseTables).Concat(new int[1] { 2000001071 }))
		{
			dictionary.Remove(item2);
		}
		return dictionary;
	}

	private static NCLMetaQueryDefinition GetHashesFromAppQuery()
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(30);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(36);
		return CreateGetHashesQuery(metaTableById, fieldByNo, fieldByNo2, fieldByNo3, fieldByNo4);
	}

	private static NCLMetaQueryDefinition GetHashesFromTenantQuery()
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(30);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(40);
		return CreateGetHashesQuery(metaTableById, fieldByNo, fieldByNo2, fieldByNo3, fieldByNo4);
	}

	private static NCLMetaQueryDefinition GetBaseHashesFromTenantQuery()
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NCLMetaField fieldByNo3 = metaTableById.GetFieldByNo(36);
		NCLMetaField fieldByNo4 = metaTableById.GetFieldByNo(40);
		return CreateGetHashesQuery(metaTableById, fieldByNo, fieldByNo2, fieldByNo3, fieldByNo4);
	}

	private static NCLMetaQueryDefinition CreateGetHashesQuery(NCLMetaTable table, NCLMetaField objectTypeField, NCLMetaField objectIdField, NCLMetaField hashField, NCLMetaField schemaField)
	{
		NavValue value = NavOption.Create(objectTypeField.FieldOptionMetadata, 1);
		FilterExpression item = new UnaryFilterExpression(FilterExpressionType.Equal, value, objectTypeField.ExpressionContext);
		FilterFieldDictionary filters = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(objectTypeField, item));
		return NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition definition) => NCLMetaQueryDataItemCollection.Create(definition, table, DataLockState.None, table.TableName, (NCLMetaQueryDataItem dataItem) => new FieldList(NCLMetaQueryColumn.Create(dataItem, objectIdField, filterOnly: false), NCLMetaQueryColumn.Create(dataItem, hashField, filterOnly: false), NCLMetaQueryColumn.Create(dataItem, schemaField, filterOnly: false)), (NCLMetaQueryDataItem nCLMetaQueryDataItem) => new FiltersAndMarks(filters)), null);
	}

	private static void CheckWriteTransactionStartedOnConnection(NavSqlConnectionScope conScope, bool mustBeAppDatabaseConnection)
	{
		if (!conScope.Connection.CurrentTransactionIsWriteTransaction)
		{
			throw new NotSupportedException("A transaction on the database should already have been started");
		}
		if (conScope.Connection.Database.DatabaseType == NavDatabaseType.Application != mustBeAppDatabaseConnection)
		{
			throw new NotSupportedException("Database on connection does not have the correct type");
		}
	}

	internal static void LockAppDatabaseForChangesToMetadata(NavSqlConnectionScope conScope)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001LX", Category.Sql, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Acquiring App Database lock for changes to metadata");
		CheckWriteTransactionStartedOnConnection(conScope, mustBeAppDatabaseConnection: true);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection))
		{
			navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 NULL FROM {0} WITH(TABLOCK)", NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), metaTableById, conScope.Connection.Database, 0, NavAppTableContext.Main)));
		}
		if (conScope.Connection.Database.ChangeListener != null)
		{
			conScope.Connection.Database.ChangeListener.ForceHandleTrackingEvent();
		}
	}

	internal static void LockTenantDatabaseForChangesToMetadata(NavSqlConnectionScope conScope)
	{
		CheckWriteTransactionStartedOnConnection(conScope, mustBeAppDatabaseConnection: false);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		navSqlCommand.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "SELECT TOP 1 NULL FROM {0} WITH(TABLOCK)", NavSqlStatementHelper.AppendQuotedTableNameCompanyQualified(new StringBuilder(), metaTableById, conScope.Connection.Database, 0, NavAppTableContext.Main)));
	}
}
