using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DatabaseNavAppDataArchiveRepository : INavAppDataArchiveRepository
{
	private class TableMetadata
	{
		public NCLMetaTable Table { get; private set; }

		public NCLMetaField AppId { get; private set; }

		public NCLMetaField TableId { get; private set; }

		public NCLMetaField CompanyName { get; private set; }

		public NCLMetaField VersionMajor { get; private set; }

		public NCLMetaField VersionMinor { get; private set; }

		public NCLMetaField VersionBuild { get; private set; }

		public NCLMetaField VersionRevision { get; private set; }

		public NCLMetaField ArchiveTable { get; private set; }

		public NCLMetaField Metadata { get; private set; }

		public NCLMetaField TableBlobVersion { get; private set; }

		public TableMetadata()
		{
			Table = NavGlobal.NCLMetadata.GetMetaTableById(2000000152, -1, requireCompiled: false);
			AppId = Table.GetFieldByNo(1);
			TableId = Table.GetFieldByNo(2);
			CompanyName = Table.GetFieldByNo(3);
			VersionMajor = Table.GetFieldByNo(4);
			VersionMinor = Table.GetFieldByNo(5);
			VersionBuild = Table.GetFieldByNo(6);
			VersionRevision = Table.GetFieldByNo(7);
			ArchiveTable = Table.GetFieldByNo(8);
			Metadata = Table.GetFieldByNo(9);
			TableBlobVersion = Table.GetFieldByNo(10);
		}
	}

	private readonly NavDatabase tenantDatabase;

	private readonly Lazy<TableMetadata> tableMetadata = new Lazy<TableMetadata>();

	public DatabaseNavAppDataArchiveRepository(NavDatabase tenantDatabase)
	{
		this.tenantDatabase = tenantDatabase;
	}

	public void InsertRecord(NavAppDataArchiveRecord record)
	{
		NavValue[] array = new NavValue[tableMetadata.Value.Table.FieldCount];
		array[tableMetadata.Value.AppId.FieldIndex] = NavGuid.CreateFromObject(record.AppId);
		array[tableMetadata.Value.TableId.FieldIndex] = NavInteger.Create(record.TableId);
		array[tableMetadata.Value.CompanyName.FieldIndex] = NavText.Create(record.CompanyName);
		array[tableMetadata.Value.VersionMajor.FieldIndex] = NavInteger.Create(record.AppVersion.Major);
		array[tableMetadata.Value.VersionMinor.FieldIndex] = NavInteger.Create(record.AppVersion.Minor);
		array[tableMetadata.Value.VersionBuild.FieldIndex] = NavInteger.Create(record.AppVersion.Build);
		array[tableMetadata.Value.VersionRevision.FieldIndex] = NavInteger.Create(record.AppVersion.Revision);
		array[tableMetadata.Value.ArchiveTable.FieldIndex] = NavText.Create(record.ArchiveTableName);
		array[tableMetadata.Value.Metadata.FieldIndex] = record.MetadataSnapshot;
		array[tableMetadata.Value.TableBlobVersion.FieldIndex] = NavInteger.Create(record.TableBlobVersion);
		MutableRecordBuffer recordBuffer = new MutableRecordBuffer(new ReadOnlyRecordBuffer(tableMetadata.Value.Table, array));
		NavValue identityValue = null;
		NavSqlDmlCommand.Insert(tenantDatabase, tableMetadata.Value.Table, 0, recordBuffer, ref identityValue, out var _);
	}

	public void DeleteRecord(NavAppDataArchiveRecord record)
	{
		ReadOnlyRecordBuffer raw = GetRaw(record.AppId, record.TableId, record.CompanyName);
		if (raw != null)
		{
			NavSqlDmlCommand.Delete(tenantDatabase, tableMetadata.Value.Table, 0, new MutableRecordBuffer(raw), recordConsistentWithCurrentTransaction: true);
		}
	}

	public NavAppDataArchiveRecord GetRecord(Guid appId, int tableId, string companyName)
	{
		ReadOnlyRecordBuffer raw = GetRaw(appId, tableId, companyName);
		if (raw != null)
		{
			return CreateFromRaw(raw);
		}
		return null;
	}

	public IEnumerable<NavAppDataArchiveRecord> GetRecordsForApp(Guid appId)
	{
		UnaryFilterExpression item = new UnaryFilterExpression(FilterExpressionType.Equal, NavGuid.CreateFromObject(appId), tableMetadata.Value.AppId.ExpressionContext);
		FiltersAndMarks filtersAndMarks = new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(tableMetadata.Value.AppId, item)));
		return GetRecords(filtersAndMarks);
	}

	public IEnumerable<NavAppDataArchiveRecord> GetRecordsForAppAndCompany(Guid appId, string companyName)
	{
		UnaryFilterExpression item = new UnaryFilterExpression(FilterExpressionType.Equal, NavGuid.CreateFromObject(appId), tableMetadata.Value.AppId.ExpressionContext);
		UnaryFilterExpression item2 = new UnaryFilterExpression(FilterExpressionType.Equal, NavText.CreateFromObject(companyName), tableMetadata.Value.CompanyName.ExpressionContext);
		FiltersAndMarks filtersAndMarks = new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(tableMetadata.Value.AppId, item), new Tuple<INavFieldMetadata, FilterExpression>(tableMetadata.Value.CompanyName, item2)));
		return GetRecords(filtersAndMarks);
	}

	public IEnumerable<Tuple<Guid, Version>> GetAppsForTenant()
	{
		IEnumerable<NavAppDataArchiveRecord> records = GetRecords(new FiltersAndMarks(new FilterFieldDictionary()));
		return records.Select((NavAppDataArchiveRecord rec) => new Tuple<Guid, Version>(rec.AppId, rec.AppVersion)).Distinct();
	}

	private IEnumerable<NavAppDataArchiveRecord> GetRecords(FiltersAndMarks filtersAndMarks)
	{
		List<NavAppDataArchiveRecord> list = new List<NavAppDataArchiveRecord>();
		PositionedFindProviderRequest request = new PositionedFindProviderRequest(0, tableMetadata.Value.Table, DataLockState.None, filtersAndMarks, null, SecurityFiltering.Ignored, FieldList.Empty, tableMetadata.Value.Table.PrimaryKey.DefinedSortingFields, FindType.All, null, includeCurrent: false);
		foreach (ReadOnlyRecordBuffer item in NavSqlIsamQueryCommand.FindAll(tenantDatabase, request, () => false))
		{
			list.Add(CreateFromRaw(item));
		}
		return list;
	}

	private ReadOnlyRecordBuffer GetRaw(Guid appId, int tableId, string companyName)
	{
		FilterExpression[] expressions = new FilterExpression[3]
		{
			new UnaryFilterExpression(FilterExpressionType.Equal, NavGuid.CreateFromObject(appId), tableMetadata.Value.AppId.ExpressionContext),
			new UnaryFilterExpression(FilterExpressionType.Equal, NavInteger.Create(tableId), tableMetadata.Value.TableId.ExpressionContext),
			new UnaryFilterExpression(FilterExpressionType.Equal, NavText.Create(companyName), tableMetadata.Value.CompanyName.ExpressionContext)
		};
		FilterExpression filter = FilterExpression.And(expressions);
		NCLMetaTable table = tableMetadata.Value.Table;
		return NavSqlIsamQueryCommand.FindTop1(tenantDatabase, table, 0, 91, locking: false, onlyCurrentKeyFieldsNeeded: false, tableMetadata.Value.Table.PrimaryKey.DefinedSortingFields, filter);
	}

	private NavAppDataArchiveRecord CreateFromRaw(ReadOnlyRecordBuffer raw)
	{
		Guid appId = raw[tableMetadata.Value.AppId.FieldIndex].ToGuid();
		int tableId = raw[tableMetadata.Value.TableId.FieldIndex].ToInt32();
		string companyName = raw[tableMetadata.Value.CompanyName.FieldIndex].ToString();
		int num = raw[tableMetadata.Value.VersionMajor.FieldIndex].ToInt32();
		int num2 = raw[tableMetadata.Value.VersionMinor.FieldIndex].ToInt32();
		int num3 = raw[tableMetadata.Value.VersionBuild.FieldIndex].ToInt32();
		int num4 = raw[tableMetadata.Value.VersionRevision.FieldIndex].ToInt32();
		string archiveTableName = raw[tableMetadata.Value.ArchiveTable.FieldIndex].ToString();
		int num5 = raw[tableMetadata.Value.TableBlobVersion.FieldIndex].ToInt32();
		if (num5 == 0)
		{
			num5 = 1;
		}
		MutableRecordBuffer mutableRecordBuffer = new MutableRecordBuffer(raw);
		if (NavSqlBlobCommand.GetBlob(tableMetadata.Value.Table, tenantDatabase, 0, new FieldList(tableMetadata.Value.Metadata), mutableRecordBuffer, locking: false) != 85132272)
		{
			NavBLOB metadataSnapshot = (NavBLOB)mutableRecordBuffer[tableMetadata.Value.Metadata.FieldIndex];
			return new NavAppDataArchiveRecord
			{
				AppId = appId,
				TableId = tableId,
				CompanyName = companyName,
				AppVersion = VersionHelper.Create(num, num2, num3, num4),
				ArchiveTableName = archiveTableName,
				MetadataSnapshot = metadataSnapshot,
				TableBlobVersion = num5
			};
		}
		return null;
	}
}
