#define SQLSEQUENCE
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavSqlImportExportData
{
	private class BinaryReaderWithProgressReporting : BinaryReader
	{
		private const int ReportChunkSize = 10240;

		private readonly long totalBytesInStream;

		private long lastPosition;

		private readonly Action<int> reportProgress;

		public BinaryReaderWithProgressReporting(Stream stream, Encoding encoding, bool leaveStreamOpen, Action<int> reportProgress, long totalBytesInStream)
			: base(stream, encoding, leaveStreamOpen)
		{
			this.reportProgress = reportProgress;
			this.totalBytesInStream = totalBytesInStream;
			if (reportProgress != null && totalBytesInStream == 0L)
			{
				throw new NotSupportedException("Must specify totalBytesInStream when a progress reporting action is specified");
			}
		}

		public override bool ReadBoolean()
		{
			if (BaseStream.Position >= lastPosition + 10240)
			{
				lastPosition = BaseStream.Position;
				reportProgress((int)(lastPosition * 100 / totalBytesInStream));
			}
			return base.ReadBoolean();
		}
	}

	private static readonly int[] TablesToOmitOnImport = new int[2] { 2000000111, 2000000110 };

	private static readonly int[] ApplicationTablesToExcludeAndCreateIfEmpty = new int[1] { 2000000151 };

	private static IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> GlobalTablesAndContexts(IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> allTablesAndContexts)
	{
		return allTablesAndContexts.Where((Tuple<NCLMetaTable, NavAppTableContext> tableAndContext) => !tableAndContext.Item1.IsExternalDataSource && !tableAndContext.Item1.DataPerCompany && tableAndContext.Item1.TableId != 2000000006);
	}

	private static IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> CompanySpecificTablesAndContexts(IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> allTablesAndContexts)
	{
		return allTablesAndContexts.Where((Tuple<NCLMetaTable, NavAppTableContext> tableAndContext) => !tableAndContext.Item1.IsExternalDataSource && tableAndContext.Item1.DataPerCompany);
	}

	private static void WriteHeader(BinaryWriter writer, string description, bool includeApplication, bool includeAppDatabaseTables, NavTenant tenant, bool includeGlobalTables, IEnumerable<string> companyNames)
	{
		NavSqlDataExportHeader navSqlDataExportHeader = new NavSqlDataExportHeader(NavGlobal.AppDatabase.SqlDatabaseProperties.DatabaseVersionNo, description, includeApplication, includeAppDatabaseTables, tenant?.Database.TenantDatabaseProperties.VersionNo ?? 0, (tenant == null) ? string.Empty : tenant.Database.TenantProperties.TenantId, includeGlobalTables, companyNames, (tenant == null) ? string.Empty : tenant.Database.TenantDatabaseProperties.ApplicationVersion);
		navSqlDataExportHeader.Write(writer);
	}

	private static void WriteApplication(BinaryWriter writer)
	{
		NavGlobal.AppDatabase.SqlDatabaseProperties.WriteProperties(writer);
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000001, -1, requireCompiled: false);
		NavSqlStreamReaderWriter.WriteTableContentToStream(writer, NavGlobal.AppDatabase, metaTableById, NavAppTableContext.Main, 0);
		NCLMetaTable metaTableById2 = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		NavSqlStreamReaderWriter.WriteTableContentToStream(writer, NavGlobal.AppDatabase, metaTableById2, NavAppTableContext.Main, 0);
	}

	private static void WriteApplicationDatabaseTables(BinaryWriter writer)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(3);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]={1} AND (", fieldByNo.SqlColumnName, 1);
		foreach (int item in SystemTables.ApplicationDatabaseTables.Where((int id) => id != 2000000071 && id != 2000000001 && !ApplicationTablesToExcludeAndCreateIfEmpty.Contains(id)))
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}]={1} OR ", fieldByNo2.SqlColumnName, item);
		}
		stringBuilder.Length -= 4;
		stringBuilder.Append(')');
		NavSqlStreamReaderWriter.WriteTableContentToStream(writer, NavGlobal.AppDatabase, metaTableById, NavAppTableContext.Main, 0, stringBuilder.ToString());
		foreach (int item2 in SystemTables.ApplicationDatabaseTables.Where((int id) => id != 2000000071 && id != 2000000001 && !ApplicationTablesToExcludeAndCreateIfEmpty.Contains(id)))
		{
			NCLMetaTable metaTableById2 = NavGlobal.NCLMetadata.GetMetaTableById(item2, -1, requireCompiled: false);
			NavSqlStreamReaderWriter.WriteTableContentToStream(writer, NavGlobal.AppDatabase, metaTableById2, NavAppTableContext.Main, 0);
		}
	}

	private static void WriteApplicationDatabase(BinaryWriter writer, bool includeApplication, bool includeApplicationDatabaseTables)
	{
		if (includeApplication)
		{
			WriteApplication(writer);
		}
		if (includeApplicationDatabaseTables)
		{
			WriteApplicationDatabaseTables(writer);
		}
	}

	private static void WriteTenantDatabase(BinaryWriter writer, bool includeGlobalTables, NavTenant tenant, IEnumerable<string> includedCompanies, Action<int> reportProgress, NavAppTenantRepositories navAppTenantRepositories)
	{
		if (!includeGlobalTables && includedCompanies == null)
		{
			return;
		}
		tenant.Database.DatabaseSync.PerformSynchronizationAction(() =>
		{
			List<AppSyncTrackingRecord> list = navAppTenantRepositories.TrackingRepository.GetAll().ToList();
			List<AppSyncSnapshotRecord> snapshotRecords = list.SelectMany((AppSyncTrackingRecord tracking) => navAppTenantRepositories.SnapshotRepository.GetAllForApp(tracking.AppId)).ToList();
			List<UninstalledAppRecord> uninstalledRecords = navAppTenantRepositories.UninstallRepository.GetAllForTenant(tenant.Id).ToList();
			DatabaseAppSyncTrackingReaderWriter.WriteSyncedAppTrackingRecordsToStream(writer, list);
			DatabaseAppSyncSnapshotReaderWriter.WriteSyncedAppSnapshotRecordsToStream(writer, snapshotRecords);
			DatabaseUninstalledAppReaderWriter.WriteUninstalledAppRecordsToStream(writer, uninstalledRecords);
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
			NavSqlStreamReaderWriter.WriteTableContentToStream(writer, tenant.Database, metaTableById, NavAppTableContext.Main, 0);
			WriteCompanyFields(tenant, includedCompanies, writer);
			ReadOnlyCollection<NCLMetaTable> tablesFromTenant = GetTablesFromTenant(tenant.Database);
			List<Tuple<NCLMetaTable, NavAppTableContext>> allTablesAndContexts = TablesAndContextsHelper.GetTablesAndContexts(tenant.Database, tablesFromTenant, snapshotRecords).ToList();
			IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> enumerable = GlobalTablesAndContexts(allTablesAndContexts);
			IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> enumerable2 = CompanySpecificTablesAndContexts(allTablesAndContexts);
			int num = 0;
			if (reportProgress != null)
			{
				num += (includeGlobalTables ? enumerable.Count() : 0);
				num += ((includedCompanies != null && includedCompanies.Count() > 0) ? (enumerable2.Count() * includedCompanies.Count()) : 0);
			}
			int num2 = 0;
			if (includeGlobalTables)
			{
				foreach (Tuple<NCLMetaTable, NavAppTableContext> item in enumerable)
				{
					NavSqlStreamReaderWriter.WriteTableContentToStream(writer, tenant.Database, item.Item1, item.Item2, 0);
					num2++;
					if (reportProgress != null)
					{
						reportProgress(num2 * 100 / num);
					}
				}
			}
			if (includedCompanies != null && includedCompanies.Any())
			{
				foreach (Tuple<NCLMetaTable, NavAppTableContext> item2 in enumerable2)
				{
					foreach (string includedCompany in includedCompanies)
					{
						int companyNameToken = tenant.Database.GetCompanyNameToken(includedCompany);
						NavSqlStreamReaderWriter.WriteTableContentToStream(writer, tenant.Database, item2.Item1, item2.Item2, companyNameToken);
						num2++;
						if (reportProgress != null)
						{
							reportProgress(num2 * 100 / num);
						}
					}
				}
			}
		}, lockOnlyCurrentTenant: true);
	}

	private static ReadOnlyCollection<string> GetAndLockAllCompanies(NavTenant tenant)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000006);
		NCLMetaField companyNameField = metaTableById.GetFieldByNo(1);
		PositionedFindProviderRequest request = new PositionedFindProviderRequest(0, metaTableById, DataLockState.LockExclusive, FiltersAndMarks.Empty, null, SecurityFiltering.Ignored, null, metaTableById.PrimaryKey.SortingFieldsWithPrimaryKeyFields, FindType.All, null, includeCurrent: false);
		return new ReadOnlyCollection<string>((from record in NavSqlIsamQueryCommand.FindAll(tenant.Database, request, null)
			select ((NavText)record[companyNameField.FieldIndex]).Value).ToArray());
	}

	private static void CheckAndLockIncludedCompanies(NavTenant tenant, IEnumerable<string> includedCompanies)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000006);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(1);
		foreach (string includedCompany in includedCompanies)
		{
			FilterExpression item = FilterExpression.Equal(new NavText(fieldByNo.FieldDefinedLength, includedCompany), fieldByNo.ExpressionContext);
			if (!NavSqlExistsCommand.Exists(tenant.Database, metaTableById, 0, new FiltersAndMarks(new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(fieldByNo, item))), DataLockState.LockExclusive))
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.SpecifiedCompanyDoesNotExist, includedCompany));
			}
		}
	}

	public static void Export(Stream stream, string description, bool includeApplication, bool includeAppDatabaseTables, bool includeGlobalTables, bool includeAllCompanies, IEnumerable<string> includedCompanies, NavSession session, Action<int> reportProgress = null, bool leaveStreamOpenWhenDone = false)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		if (!session.HasExclusiveAccess)
		{
			throw new NavNCLExclusiveActionException("Data export can be run only by a session with exclusive tenant access.");
		}
		NavTenant tenant = session.Tenant;
		if (tenant.NavAppGroup != NavAppGroup.BaseGroup && NavEnvironment.Instance.StandaloneInstance && ((includeGlobalTables | includeAllCompanies) || (includedCompanies != null && includedCompanies.Any())))
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppPresenceMakesDirectDBAccessUnsupported));
		}
		ExportInternal(stream, description, includeApplication, includeAppDatabaseTables, tenant, includeGlobalTables, includeAllCompanies, includedCompanies, reportProgress, leaveStreamOpenWhenDone);
	}

	private static void ExportInternal(Stream stream, string description, bool includeApplication, bool includeAppDatabaseTables, NavTenant tenant, bool includeGlobalTables, bool includeAllCompanies, IEnumerable<string> includedCompanies, Action<int> reportProgress = null, bool leaveStreamOpenWhenDone = false)
	{
		string arg = ((tenant != null) ? tenant.Id : "NA");
		string arg2 = ((tenant != null) ? tenant.Database.DatabaseName : "NA");
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000086", DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "Export NavData, tenant {0}, database {1}", arg, arg2)), MonitorType.ALStatementsExecuted, Category.Sql))
		{
			using BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, leaveStreamOpenWhenDone);
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
			using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
			{
				NavSqlConnectionScope navSqlConnectionScope = null;
				NavSqlDatabaseLock navSqlDatabaseLock = null;
				try
				{
					NavSqlMetadata.LockAppDatabaseForChangesToMetadata(conScope);
					if ((includeAllCompanies || includedCompanies != null) | includeGlobalTables)
					{
						if (tenant == null)
						{
							throw new ArgumentNullException("tenant", "Tenant object must be set when tenant data is exported.");
						}
						navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenant.Database, TransactionType.UpdateNoLocks);
						NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope);
						navSqlDatabaseLock = tenant.Database.DatabaseSync.AcquireCrossServerSyncLock(navSqlConnectionScope, lockOnlyCurrentTenant: true);
					}
					if (includeAllCompanies)
					{
						tenant.Database.DatabaseSync.PerformSynchronizationAction(() =>
						{
							includedCompanies = GetAndLockAllCompanies(tenant);
						}, lockOnlyCurrentTenant: true);
					}
					else if (includedCompanies != null)
					{
						tenant.Database.DatabaseSync.PerformSynchronizationAction(() =>
						{
							CheckAndLockIncludedCompanies(tenant, includedCompanies);
						}, lockOnlyCurrentTenant: true);
					}
					WriteHeader(writer, description, includeApplication, includeAppDatabaseTables, tenant, includeGlobalTables, includedCompanies);
					WriteApplicationDatabase(writer, includeApplication, includeAppDatabaseTables);
					if (navSqlConnectionScope != null)
					{
						NavAppTenantRepositories navAppTenantRepositories = new NavAppTenantRepositories(tenant.Database, navSqlConnectionScope);
						WriteTenantDatabase(writer, includeGlobalTables, tenant, includedCompanies, reportProgress, navAppTenantRepositories);
					}
				}
				catch (IOException)
				{
					throw new NavNotSupportedException(Lang.IOErrorDuringExport);
				}
				finally
				{
					navSqlDatabaseLock?.Dispose();
					navSqlConnectionScope?.Dispose();
				}
			}
		}
	}

	private static void WriteCompanyFields(NavTenant tenant, IEnumerable<string> includedCompanies, BinaryWriter writer)
	{
		if (includedCompanies == null || !includedCompanies.Any())
		{
			return;
		}
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000006, -1, requireCompiled: false);
		List<SqlParameter> list = new List<SqlParameter>();
		StringBuilder stringBuilder = new StringBuilder();
		NCLMetaField nCLMetaField = metaTableById.AllFields[1];
		NavSqlStatementHelper.AppendSqlColumnName(stringBuilder, nCLMetaField, null);
		foreach (string includedCompany in includedCompanies)
		{
			string parameterName = NavSqlStatementHelper.ParameterName(list.Count);
			list.Add(NavSqlStatementHelper.CreateSqlParameter(parameterName, nCLMetaField.FieldNclType, nCLMetaField.SqlDataType, nCLMetaField.FieldDefinedLength, includedCompany, isWildcardValue: false, isFilterValue: false));
		}
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, " IN ({0})", string.Join(",", list.Select((SqlParameter param) => param.ParameterName)));
		NavSqlStreamReaderWriter.WriteTableContentToStream(writer, tenant.Database, metaTableById, NavAppTableContext.Main, 0, stringBuilder.ToString(), list);
	}

	private static IEnumerable<object[]> ReadTable(NavDatabase database, BinaryReader reader, NCLMetaTable table, NavAppTableContext tableContext, int exportVersion)
	{
		NavSqlStreamDataReader streamReader = new NavSqlStreamDataReader(table, null, reader, tableContext, database.IsSharedTenantSchema, exportVersion);
		while (streamReader.Read())
		{
			object[] array = new object[streamReader.FieldCount + 1];
			for (int i = 1; i < array.Length; i++)
			{
				array[i] = streamReader.GetValue(i - 1);
			}
			yield return array;
		}
	}

	private static void ReadPastTable(NavDatabase database, BinaryReader reader, NCLMetaTable table, NavAppTableContext tableContext, int exportVersion)
	{
		IEnumerator<object[]> enumerator = ReadTable(database, reader, table, tableContext, exportVersion).GetEnumerator();
		while (enumerator.MoveNext())
		{
		}
	}

	private static void CreateTableTriggers(NavDatabase destinationDatabase, NCLMetaTable table)
	{
		switch (table.TableId)
		{
		case 2000000069:
		case 2000000071:
		case 2000000100:
		case 2000000150:
		case 2000000151:
		case 2000000155:
		case 2000000160:
		{
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(destinationDatabase, updateOrlockingActivity: true);
			NavDatabaseChangeListener.EnableSqlChangeTrackingOnTable(conScope, table);
			break;
		}
		}
	}

	private static void CreateEmptyTable(NavDatabase destinationDatabase, NCLMetaTable table, int companyToken, NavAppTableContext tableContext)
	{
		NavSqlDdlCommands.CreateTableAndIndexes(destinationDatabase, table, companyToken, tableContext, null);
		CreateTableTriggers(destinationDatabase, table);
	}

	private static bool IsTenantDatabasePerCompanySchema(NavDatabase database)
	{
		if (database.DatabaseType == NavDatabaseType.Tenant)
		{
			return database.TenantDatabaseProperties.PerCompanySchema;
		}
		return true;
	}

	internal static void ImportTableFromReader(BinaryReader reader, NavDatabase destinationDatabase, SqlBulkCopy sqlBulkCopy, NCLMetaTable table, int companyToken, NavAppTableContext tableContext, int ExportVersion)
	{
		bool flag = IsTenantDatabasePerCompanySchema(destinationDatabase);
		bool flag2 = NavSqlMetadataCommands.TableExists(destinationDatabase, table, companyToken, tableContext);
		if (flag || !flag2)
		{
			NavSqlDdlCommands.DropTableAndIndexes(destinationDatabase, table, companyToken, tableContext);
			NavSqlDdlCommands.CreateTableNoIndexes(destinationDatabase, table, companyToken, tableContext, null);
		}
		else
		{
			NavSqlDmlCommand.DeleteMultiple(destinationDatabase, table, companyToken, FiltersAndMarks.Empty, tableContext);
		}
		NavSqlStreamReaderWriter.BulkCopyFromStreamIntoTable(destinationDatabase, table, tableContext, companyToken, reader, sqlBulkCopy, ExportVersion);
		if (flag || !flag2)
		{
			NavSqlDdlCommands.CreateAllIndexes(destinationDatabase, table, companyToken, tableContext, null);
			CreateTableTriggers(destinationDatabase, table);
		}
	}

	private static bool VerifyTableMetadataFromReader(byte[] metadataBytesFromReader, NCLMetaTable currentTableMetadata, int tableBlobVersion, NavAppGroup appGroup)
	{
		MemoryStream memoryStream = new MemoryStream(metadataBytesFromReader);
		memoryStream.Position = 4L;
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		using (DeflateStream documentStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
		{
			xmlDocument.Load(XmlHelper.SafeXmlReader(documentStream));
		}
		NCLMetaTable nCLMetaTable = NCLMetaTable.CreateEmptyNCLMetaTable(null, currentTableMetadata.TableId, appGroup);
		MetaTable metaTableFromMetadata = NavSqlObjectMetadataSnapshotHandler.GetMetaTableFromMetadata(xmlDocument.DocumentElement, tableBlobVersion);
		nCLMetaTable.AssignFromMetaTable(metaTableFromMetadata, databaseConvertedFromNav2013: false);
		ReadOnlyCollection<NavSqlFieldAndChanges> fieldsAndChanges;
		ReadOnlyCollection<NavSqlIndexAndChanges> indexesAndChanges;
		return !NavSqlTableAndChange.CheckChangeAffectSqlSchema(nCLMetaTable, currentTableMetadata, changedSortingProperties: false, databaseConvertedFromNav2013: false, out fieldsAndChanges, out indexesAndChanges);
	}

	private static void ReadApplication(BinaryReader reader, NavSqlConnection connection, SqlBulkCopy globalSqlBulkCopy, NavSqlDataExportHeader header, bool include, bool commitPerTable)
	{
		if (!header.HasApplication)
		{
			return;
		}
		NavDatabaseChangeListener.EnableSqlChangeTrackingOnDatabase();
		NCLMetaTable objectTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000001, -1, requireCompiled: false);
		NCLMetaTable objectMetadataTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		if (include)
		{
			RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
			{
				ImportTableFromReader(reader, NavGlobal.AppDatabase, sqlBulkCopy, objectTable, 0, NavAppTableContext.Main, header.ExportVersion);
			}, objectTable, commitPerTable, connection, globalSqlBulkCopy);
			RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
			{
				ImportTableFromReader(reader, NavGlobal.AppDatabase, sqlBulkCopy, objectMetadataTable, 0, NavAppTableContext.Main, header.ExportVersion);
			}, objectMetadataTable, commitPerTable, connection, globalSqlBulkCopy);
		}
		else
		{
			ReadPastTable(NavGlobal.AppDatabase, reader, objectTable, NavAppTableContext.Main, header.ExportVersion);
			ReadPastTable(NavGlobal.AppDatabase, reader, objectMetadataTable, NavAppTableContext.Main, header.ExportVersion);
		}
	}

	private static void ReadAndVerifyMetadataForApplicationDatabaseTables(NavDatabase database, BinaryReader reader, bool verify, int exportVersion)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(2000000071, -1, requireCompiled: false);
		NCLMetaField fieldByNo = metaTableById.GetFieldByNo(9);
		NCLMetaField fieldByNo2 = metaTableById.GetFieldByNo(6);
		NavAppGroup baseGroup = NavAppGroup.BaseGroup;
		foreach (object[] item in ReadTable(database, reader, metaTableById, NavAppTableContext.Main, exportVersion))
		{
			if (verify)
			{
				int num = (int)item[fieldByNo2.FieldIndex];
				NCLMetaTable metaTableById2 = NavGlobal.NCLMetadata.GetMetaTableById(num, baseGroup.GroupId, requireCompiled: false);
				if (!VerifyTableMetadataFromReader((byte[])item[fieldByNo.FieldIndex], metaTableById2, 1, baseGroup))
				{
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedTableNotIdentical, num));
				}
			}
		}
	}

	private static void ReadApplicationDatabaseTables(BinaryReader reader, NavSqlConnection connection, SqlBulkCopy globalSqlBulkCopy, NavSqlDataExportHeader header, bool include, bool createEmptyTablesWhenNotIncluded, bool commitPerTable)
	{
		if (header.HasAppDatabaseTables)
		{
			ReadAndVerifyMetadataForApplicationDatabaseTables(NavGlobal.AppDatabase, reader, include, header.ExportVersion);
		}
		NavAppGroup baseGroup = NavAppGroup.BaseGroup;
		foreach (int item in SystemTables.ApplicationDatabaseTables.Where((int id) => id != 2000000071 && id != 2000000001))
		{
			NCLMetaTable table = NavGlobal.NCLMetadata.GetMetaTableById(item, baseGroup.GroupId, requireCompiled: false);
			if (!ApplicationTablesToExcludeAndCreateIfEmpty.Contains(item))
			{
				if (include && header.HasAppDatabaseTables)
				{
					RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
					{
						ImportTableFromReader(reader, NavGlobal.AppDatabase, sqlBulkCopy, table, 0, NavAppTableContext.Main, header.ExportVersion);
					}, table, commitPerTable, connection, globalSqlBulkCopy);
					continue;
				}
				if (header.HasAppDatabaseTables)
				{
					ReadPastTable(NavGlobal.AppDatabase, reader, table, NavAppTableContext.Main, header.ExportVersion);
				}
			}
			if (createEmptyTablesWhenNotIncluded)
			{
				RunActionInDefaultOrNewTransaction(() =>
				{
					CreateEmptyTable(NavGlobal.AppDatabase, table, 0, NavAppTableContext.Main);
				}, table, commitPerTable, connection);
			}
		}
	}

	private static void ReadSqlDatabaseProperties(BinaryReader reader, NavSqlDataExportHeader header, bool includeApplication)
	{
		if (!header.HasApplication)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.CheckDatabaseVersion();
		}
		else if (includeApplication)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.ReadPropertiesAndCreateTable(reader);
			CacheSynchronizationStorage.CreateTable();
			NavTaskScheduler.SqlDml.CreateTaskScheduleTable();
		}
		else
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.CheckDatabaseVersion();
			NavSqlDatabaseProperties.ReadPastProperties(reader);
		}
	}

	private static void ReadApplicationDatabase(BinaryReader reader, NavSqlDataExportHeader header, bool includeApplication, bool includeApplicationDatabaseTables, bool commitPerTable, bool emptyAppDatabase)
	{
		if (!header.HasApplication && !header.HasAppDatabaseTables)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.CheckDatabaseVersion();
			return;
		}
		if (!includeApplication && !includeApplicationDatabaseTables)
		{
			ReadSqlDatabaseProperties(reader, header, includeApplication: false);
			ReadApplication(reader, null, null, header, include: false, commitPerTable: false);
			ReadApplicationDatabaseTables(reader, null, null, header, include: false, createEmptyTablesWhenNotIncluded: false, commitPerTable: false);
			return;
		}
		if (emptyAppDatabase)
		{
			NavGlobal.AppDatabase.SqlDatabaseProperties.ReadPropertiesAndCreateTable(reader);
			CacheSynchronizationStorage.CreateTable();
			NavTaskScheduler.SqlDml.CreateTaskScheduleTable();
		}
		else
		{
			ReadSqlDatabaseProperties(reader, header, includeApplication);
		}
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks))
		{
			try
			{
				if (!emptyAppDatabase)
				{
					NavSqlMetadata.LockAppDatabaseForChangesToMetadata(navSqlConnectionScope);
				}
				if (commitPerTable)
				{
					ReadApplication(reader, navSqlConnectionScope.Connection, null, header, includeApplication, commitPerTable: true);
					ReadApplicationDatabaseTables(reader, navSqlConnectionScope.Connection, null, header, includeApplicationDatabaseTables, emptyAppDatabase, commitPerTable: true);
				}
				else
				{
					using SqlBulkCopy globalSqlBulkCopy = new SqlBulkCopy(navSqlConnectionScope.Connection.Connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock, navSqlConnectionScope.Connection.CurrentTransaction);
					ReadApplication(reader, navSqlConnectionScope.Connection, globalSqlBulkCopy, header, includeApplication, commitPerTable: false);
					ReadApplicationDatabaseTables(reader, navSqlConnectionScope.Connection, globalSqlBulkCopy, header, includeApplicationDatabaseTables, emptyAppDatabase, commitPerTable: false);
				}
			}
			catch
			{
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				throw;
			}
		}
		NavGlobal.AppDatabase.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: false);
		NavGlobal.AppDatabase.DatabaseState.ClearGlobalCache();
	}

	private static void VerifyExportedTenantMetadataMatchCurrentVersion(BinaryReader reader, NavDatabase tenantDatabase, NCLMetaTable objectMetadataSnapshotTable, int exportVersion)
	{
		Dictionary<int, NavText> tableIdsAndMetadataHashesFromTenantDatabase = NavSqlMetadata.GetTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase);
		int sqlColumnIndex = objectMetadataSnapshotTable.GetFieldByNo(6).SqlColumnIndex;
		int sqlColumnIndex2 = objectMetadataSnapshotTable.GetFieldByNo(30).SqlColumnIndex;
		NCLMetaField fieldByNo = objectMetadataSnapshotTable.GetFieldByNo(9);
		NCLMetaField fieldByNo2 = objectMetadataSnapshotTable.GetFieldByNo(39);
		int sqlColumnIndex3 = fieldByNo.SqlColumnIndex;
		int sqlColumnIndex4 = fieldByNo2.SqlColumnIndex;
		foreach (object[] item in ReadTable(tenantDatabase, reader, objectMetadataSnapshotTable, NavAppTableContext.Main, exportVersion))
		{
			int num = (int)item[sqlColumnIndex];
			if (SystemTables.VirtualTables.Contains(num))
			{
				continue;
			}
			string text = (string)item[sqlColumnIndex2];
			if (!tableIdsAndMetadataHashesFromTenantDatabase.TryGetValue(num, out var value))
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedExcessiveTableInFile, num));
			}
			if (value != text)
			{
				NavAppGroup appGroup = NavCurrentThread.TryResolveAppGroup() ?? tenantDatabase.Tenant.NavAppGroup;
				if (!NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, num, out var table, databaseConvertedFromNav2013: false, lockExclusive: false) || !VerifyTableMetadataFromReader((byte[])item[sqlColumnIndex3], table, (int)item[sqlColumnIndex4], appGroup))
				{
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedTableNotIdentical, num));
				}
			}
			tableIdsAndMetadataHashesFromTenantDatabase.Remove(num);
		}
		if (tableIdsAndMetadataHashesFromTenantDatabase.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<int, NavText> item2 in tableIdsAndMetadataHashesFromTenantDatabase)
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(item2.Key, -1, requireCompiled: false);
			if (!metaTableById.IsExternalDataSource)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000087", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, "The navdata file does not contain data for table id {0}", item2.Key);
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedMissingTableInFile, item2.Key));
			}
		}
	}

	private static ReadOnlyCollection<NCLMetaTable> GetTablesFromTenant(NavDatabase tenantDatabase)
	{
		ICollection<int> keys = NavSqlMetadata.GetTableIdsAndMetadataHashesFromTenantDatabase(tenantDatabase).Keys;
		List<NCLMetaTable> list = new List<NCLMetaTable>();
		foreach (int item in keys)
		{
			NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, item, out var table, databaseConvertedFromNav2013: false, lockExclusive: false);
			list.Add(table);
		}
		return new ReadOnlyCollection<NCLMetaTable>(list);
	}

	private static List<Tuple<bool, int>> GetCompaniesAndInclusion(BinaryReader reader, NavSqlConnection connection, NavSqlDataExportHeader header, NavDatabase tenantDatabase, bool commitPerTable, NCLMetaTable companyTable, IReadOnlyDictionary<string, bool> companyImports, SqlBulkCopy globalSqlBulkCopy)
	{
		if (companyImports == null || !companyImports.Any())
		{
			if (header.CompanyNames != null)
			{
				ReadPastTable(tenantDatabase, reader, companyTable, NavAppTableContext.Main, header.ExportVersion);
			}
			return null;
		}
		List<Tuple<bool, int>> list = new List<Tuple<bool, int>>();
		HashSet<string> companiesToBulkImport = new HashSet<string>();
		NCLMetaField fieldByNo = companyTable.GetFieldByNo(1);
		foreach (string companyName in header.CompanyNames)
		{
			NavValue[] array = new NavValue[companyTable.FieldCount];
			array[fieldByNo.ColumnIndex] = new NavText(fieldByNo.FieldDefinedLength, companyName);
			MutableRecordBuffer inputRecord = new MutableRecordBuffer(new ReadOnlyRecordBuffer(companyTable, array));
			bool flag = companyImports.Keys.Contains(companyName);
			if (flag)
			{
				ReadOnlyRecordBuffer readOnlyRecordBuffer = NavSqlIsamQueryCommand.FindEqual(tenantDatabase, companyTable, 0, 61, locking: true, inputRecord, null, companyTable.PrimaryKey.SortingFieldsWithPrimaryKeyFields);
				if (readOnlyRecordBuffer == null)
				{
					companiesToBulkImport.Add(companyName);
					SystemTableTriggers.CleanupDeletedCompanyData(tenantDatabase.Tenant, companyName);
					tenantDatabase.TenantCompanies.Insert(companyName);
				}
				else if (!companyImports[companyName])
				{
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CannotImportIntoExistingCompany, companyName));
				}
			}
			list.Add(Tuple.Create(flag, tenantDatabase.GetCompanyNameToken(companyName)));
		}
		RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
		{
			NavSqlStreamReaderWriter.BulkCopyFromStreamIntoTable(tenantDatabase, companyTable, NavAppTableContext.Main, 0, reader, sqlBulkCopy, header.ExportVersion, (NavSqlStreamDataReader companyReader) => ShouldImportCompanyRecord(companyTable, companyReader, companiesToBulkImport));
		}, companyTable, commitPerTable, connection, globalSqlBulkCopy, exclusiveTableLock: false);
		tenantDatabase.CacheSynchronization.RegisterTableDataChanged(2000000006, 0);
		return list;
	}

	private static NCLMetaTable GetCompanyTable(BinaryReader reader, bool emptyTenantDatabase, NavSqlConnection connection, NavDatabase tenantDatabase, bool commitPerTable, int exportVersion, SqlBulkCopy globalSqlBulkCopy = null)
	{
		NCLMetaTable objectMetadataSnapshotTable = NavGlobal.NCLMetadata.GetMetaTableById(2000001071, -1, requireCompiled: false);
		NCLMetaTable companyTable;
		if (emptyTenantDatabase)
		{
			RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
			{
				ImportTableFromReader(reader, tenantDatabase, sqlBulkCopy, objectMetadataSnapshotTable, 0, NavAppTableContext.Main, exportVersion);
			}, objectMetadataSnapshotTable, commitPerTable, connection, globalSqlBulkCopy);
			NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, 2000000006, out companyTable, databaseConvertedFromNav2013: false, lockExclusive: false);
			RunActionInDefaultOrNewTransaction(() =>
			{
				CreateEmptyTable(tenantDatabase, companyTable, 0, NavAppTableContext.Main);
			}, companyTable, commitPerTable, connection);
		}
		else
		{
			VerifyExportedTenantMetadataMatchCurrentVersion(reader, tenantDatabase, objectMetadataSnapshotTable, exportVersion);
			NavSqlObjectMetadataSnapshotHandler.GetTableMetadataFromTenant(tenantDatabase, 2000000006, out companyTable, databaseConvertedFromNav2013: false, lockExclusive: false);
		}
		return companyTable;
	}

	private static bool ShouldImportCompanyRecord(NCLMetaTable table, NavSqlStreamDataReader reader, HashSet<string> companyImports)
	{
		int columnIndex = table.GetFieldByNo(1).ColumnIndex;
		string item = (string)reader.GetValue(columnIndex - 1);
		return companyImports.Contains(item);
	}

	private static List<AppSyncSnapshotRecord> ReadSyncedAppRecordsForTenantDatabase(bool emptyTenantDatabase, NavAppTenantRepositories navAppTenantRepositories, List<AppSyncTrackingRecord> exportedTrackingRecords, List<AppSyncSnapshotRecord> exportedSnapshotRecords, List<UninstalledAppRecord> exportedUninstalledRecords)
	{
		List<AppSyncSnapshotRecord> result;
		if (emptyTenantDatabase)
		{
			foreach (AppSyncTrackingRecord exportedTrackingRecord in exportedTrackingRecords)
			{
				navAppTenantRepositories.TrackingRepository.InsertOrUpdate(exportedTrackingRecord, forceUpdateBaseline: false);
			}
			foreach (AppSyncSnapshotRecord exportedSnapshotRecord in exportedSnapshotRecords)
			{
				navAppTenantRepositories.SnapshotRepository.InsertOrUpdate(exportedSnapshotRecord);
			}
			result = exportedSnapshotRecords;
		}
		else
		{
			List<AppSyncTrackingRecord> list = navAppTenantRepositories.TrackingRepository.GetAll().ToList();
			List<AppSyncSnapshotRecord> list2 = list.SelectMany((AppSyncTrackingRecord tracking) => navAppTenantRepositories.SnapshotRepository.GetAllForApp(tracking.AppId)).ToList();
			ExtensionsImportSyncStateValidator.EnsureSyncStateIsAcceptableForImport(exportedTrackingRecords, exportedSnapshotRecords, list, list2);
			navAppTenantRepositories.UninstallRepository.DeleteAllForTenant();
			result = list2;
		}
		foreach (UninstalledAppRecord exportedUninstalledRecord in exportedUninstalledRecords)
		{
			navAppTenantRepositories.UninstallRepository.InsertOrUpdate(exportedUninstalledRecord);
		}
		return result;
	}

	private static void ReadTenantTables(BinaryReader reader, bool emptyTenantDatabase, NavSqlDataExportHeader header, NavSqlConnection connection, NavDatabase tenantDatabase, bool includeGlobalTables, IReadOnlyDictionary<string, bool> companyImports, bool commitPerTable, NavAppTenantRepositories navAppTenantRepositories, SqlBulkCopy globalSqlBulkCopy = null)
	{
		List<AppSyncTrackingRecord> exportedTrackingRecords = DatabaseAppSyncTrackingReaderWriter.ReadSyncedAppTrackingRecordsFromBinary(reader);
		List<AppSyncSnapshotRecord> exportedSnapshotRecords = DatabaseAppSyncSnapshotReaderWriter.ReadSyncedAppSnapshotRecordsFromBinary(reader);
		List<UninstalledAppRecord> exportedUninstalledRecords = DatabaseUninstalledAppReaderWriter.ReadUninstalledAppRecordsFromBinary(reader);
		NCLMetaTable companyTable = GetCompanyTable(reader, emptyTenantDatabase, connection, tenantDatabase, commitPerTable, header.ExportVersion, globalSqlBulkCopy);
		List<Tuple<bool, int>> companiesAndInclusion = GetCompaniesAndInclusion(reader, connection, header, tenantDatabase, commitPerTable, companyTable, companyImports, globalSqlBulkCopy);
		IEnumerable<NCLMetaTable> tablesFromTenant = GetTablesFromTenant(tenantDatabase);
		List<AppSyncSnapshotRecord> snapshotRecords = ReadSyncedAppRecordsForTenantDatabase(emptyTenantDatabase, navAppTenantRepositories, exportedTrackingRecords, exportedSnapshotRecords, exportedUninstalledRecords);
		List<Tuple<NCLMetaTable, NavAppTableContext>> tablesAndContexts = TablesAndContextsHelper.GetTablesAndContexts(tenantDatabase, tablesFromTenant, snapshotRecords).ToList();
		ReadTenantGlobalTables(reader, emptyTenantDatabase, header, connection, tenantDatabase, includeGlobalTables, commitPerTable, globalSqlBulkCopy, tablesAndContexts);
		if (companiesAndInclusion != null && companiesAndInclusion.Any((Tuple<bool, int> companyAndInclusion) => companyAndInclusion.Item1))
		{
			NavSqlDdlCommands navSqlDdlCommands = ReadTenantCompanySpecificTables(reader, connection, tenantDatabase, commitPerTable, globalSqlBulkCopy, tablesAndContexts, companiesAndInclusion, header.ExportVersion);
			navSqlDdlCommands.ExecuteNonQuery();
		}
	}

	private static NavSqlDdlCommands ReadTenantCompanySpecificTables(BinaryReader reader, NavSqlConnection connection, NavDatabase tenantDatabase, bool commitPerTable, SqlBulkCopy globalSqlBulkCopy, IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> tablesAndContexts, List<Tuple<bool, int>> companiesAndInclusion, int exportVersion)
	{
		NavSqlDdlCommands sequenceCommands = new NavSqlDdlCommands(tenantDatabase);
		foreach (Tuple<NCLMetaTable, NavAppTableContext> tableAndContext in CompanySpecificTablesAndContexts(tablesAndContexts))
		{
			RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
			{
				foreach (Tuple<bool, int> item in companiesAndInclusion)
				{
					if (item.Item1)
					{
						ImportTableFromReader(reader, tenantDatabase, sqlBulkCopy, tableAndContext.Item1, item.Item2, tableAndContext.Item2, exportVersion);
						if (tenantDatabase.HasSequencedAutoIncrementField(tableAndContext.Item1, tableAndContext.Item2))
						{
							sequenceCommands.CreateOrAlterSequenceForAutoIncrementField(tableAndContext.Item1, tableAndContext.Item1.AutoIncrementField, tenantDatabase.GetCompanyId(tableAndContext.Item1, item.Item2).Value);
							sequenceCommands.RestartSequenceForAutoIncrementField(tableAndContext.Item1, tableAndContext.Item1.AutoIncrementField, tenantDatabase.GetCompanyId(tableAndContext.Item1, item.Item2).Value);
						}
					}
					else
					{
						ReadPastTable(tenantDatabase, reader, tableAndContext.Item1, tableAndContext.Item2, exportVersion);
					}
				}
			}, tableAndContext.Item1, commitPerTable, connection, globalSqlBulkCopy, exclusiveTableLock: false);
		}
		return sequenceCommands;
	}

	private static void ReadTenantGlobalTables(BinaryReader reader, bool emptyTenantDatabase, NavSqlDataExportHeader header, NavSqlConnection connection, NavDatabase tenantDatabase, bool includeGlobalTables, bool commitPerTable, SqlBulkCopy globalSqlBulkCopy, IEnumerable<Tuple<NCLMetaTable, NavAppTableContext>> tablesAndContexts)
	{
		foreach (Tuple<NCLMetaTable, NavAppTableContext> tableAndContext in GlobalTablesAndContexts(tablesAndContexts))
		{
			if (includeGlobalTables && !TablesToOmitOnImport.Contains(tableAndContext.Item1.TableId))
			{
				RunActionInDefaultOrNewTransaction((SqlBulkCopy sqlBulkCopy) =>
				{
					ImportTableFromReader(reader, tenantDatabase, sqlBulkCopy, tableAndContext.Item1, 0, tableAndContext.Item2, header.ExportVersion);
				}, tableAndContext.Item1, commitPerTable, connection, globalSqlBulkCopy, exclusiveTableLock: false);
				if (tableAndContext.Item2 == NavAppTableContext.Main)
				{
					tenantDatabase.CacheSynchronization.RegisterTableDataChanged(tableAndContext.Item1.TableId, 0);
				}
				continue;
			}
			if (emptyTenantDatabase)
			{
				RunActionInDefaultOrNewTransaction(() =>
				{
					CreateEmptyTable(tenantDatabase, tableAndContext.Item1, 0, tableAndContext.Item2);
				}, tableAndContext.Item1, commitPerTable, connection);
			}
			if (header.HasGlobalTables)
			{
				ReadPastTable(tenantDatabase, reader, tableAndContext.Item1, tableAndContext.Item2, header.ExportVersion);
			}
		}
	}

	private static void RunActionInDefaultOrNewTransaction(Action<SqlBulkCopy> action, NCLMetaTable table, bool commitPerTable, NavSqlConnection connection, SqlBulkCopy sqlBulkCopy, bool exclusiveTableLock = true)
	{
		if (commitPerTable)
		{
			connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
			connection.CheckBeginTransaction(TransactionType.UpdateNoLocks);
			try
			{
				SqlBulkCopyOptions sqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity;
				if (exclusiveTableLock)
				{
					sqlBulkCopyOptions |= SqlBulkCopyOptions.TableLock;
				}
				using SqlBulkCopy obj = new SqlBulkCopy(connection.Connection, sqlBulkCopyOptions, connection.CurrentTransaction);
				action(obj);
				return;
			}
			catch (Exception innerException)
			{
				connection.EndTransaction(NavSqlEndTransactionType.Rollback);
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.FailedToImportTable, table.TableName), innerException);
			}
		}
		action(sqlBulkCopy);
	}

	private static void RunActionInDefaultOrNewTransaction(Action action, NCLMetaTable table, bool commitPerTable, NavSqlConnection connection)
	{
		if (commitPerTable)
		{
			connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
			connection.CheckBeginTransaction(TransactionType.UpdateNoLocks);
			try
			{
				action();
				return;
			}
			catch (Exception)
			{
				connection.EndTransaction(NavSqlEndTransactionType.Rollback);
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.FailedToImportTable, table.TableName));
			}
		}
		action();
	}

	private static void ReadTenantDatabase(BinaryReader reader, NavSqlDataExportHeader header, NavDatabase tenantDatabase, bool includeGlobalTables, IReadOnlyDictionary<string, bool> companyImports, bool commitPerTable)
	{
		if ((!header.HasGlobalTables && (header.CompanyNames == null || !header.CompanyNames.Any())) || (!includeGlobalTables && (companyImports == null || !companyImports.Any())))
		{
			return;
		}
		if (tenantDatabase.Tenant.NavAppGroup != NavAppGroup.BaseGroup && NavEnvironment.Instance.StandaloneInstance)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppPresenceMakesDirectDBAccessUnsupported));
		}
		if (tenantDatabase.IsNewTenantDatabase)
		{
			tenantDatabase.DatabaseSync.AlterDatabaseCollation(NavGlobal.AppDatabase.SqlSortingProperties.Collation);
		}
		tenantDatabase.DatabaseSync.PerformSynchronizationAction(() =>
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.UpdateNoLocks);
			using NavSqlConnectionScope navSqlConnectionScope2 = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.UpdateNoLocks);
			NavSqlMetadata.LockAppDatabaseForChangesToMetadata(navSqlConnectionScope);
			bool flag = tenantDatabase.TenantDatabaseProperties.NumberOfTenantsInThisDatabase() == 0;
			if (!flag)
			{
				NavSqlMetadata.LockTenantDatabaseForChangesToMetadata(navSqlConnectionScope2);
			}
			NavSqlTenantPropertiesSync.Sync(tenantDatabase);
			tenantDatabase.TenantProperties.RegisterNewTenant();
			NavAppTenantRepositories navAppTenantRepositories = new NavAppTenantRepositories(tenantDatabase, navSqlConnectionScope2);
			try
			{
				if (commitPerTable)
				{
					ReadTenantTables(reader, flag, header, navSqlConnectionScope2.Connection, tenantDatabase, includeGlobalTables, companyImports, commitPerTable: true, navAppTenantRepositories);
				}
				else
				{
					using SqlBulkCopy globalSqlBulkCopy = new SqlBulkCopy(navSqlConnectionScope2.Connection.Connection, SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock, navSqlConnectionScope2.Connection.CurrentTransaction);
					ReadTenantTables(reader, flag, header, navSqlConnectionScope2.Connection, tenantDatabase, includeGlobalTables, companyImports, commitPerTable: false, navAppTenantRepositories, globalSqlBulkCopy);
				}
				IEnumerable<AppSyncTrackingRecord> all = new DatabaseAppSyncTrackingRepository(tenantDatabase, navSqlConnectionScope2).GetAll();
				List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(navSqlConnectionScope2);
				ExtensionsImportSyncStateValidator.EnsureInstallStateIsAcceptableForImport(all, appsInstalledForTenantInTenantDatabase);
				if (appsInstalledForTenantInTenantDatabase.Any() && NavEnvironment.Instance.StandaloneInstance)
				{
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppPresenceInFileMakesDirectDBAccessUnsupported));
				}
				if (!NavEnvironment.Instance.StandaloneInstance)
				{
					NavAppInstallationManagement.PurgeAndInsertNewTenantAppInstallationRecordsInternal(NavGlobal.AppDatabase, navSqlConnectionScope, appsInstalledForTenantInTenantDatabase.Select((INavAppMetadataReference a) => a.PackageId).ToList(), tenantDatabase.Tenant.Id);
					NavEnvironment.Instance.NavAppGroupUpdater.UpdateGroupForChangedTenant(tenantDatabase.Tenant);
				}
			}
			catch
			{
				navSqlConnectionScope2.TransactionEnd(NavSqlEndTransactionType.Rollback);
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				throw;
			}
		}, lockOnlyCurrentTenant: true);
		tenantDatabase.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: false);
		tenantDatabase.DatabaseState.ClearGlobalCache();
	}

	public static void Import(Stream stream, bool includeApplication, bool includeApplicationDatabaseTables, NavDatabase tenantDatabase, bool includeGlobalTables, IReadOnlyDictionary<string, bool> companyImports, Action<int> reportProgress = null, long totalBytesInStream = 0L, bool leaveStreamOpenWhenDone = false, bool commitPerTable = false, bool allCompanies = false)
	{
		string arg = ((tenantDatabase != null) ? tenantDatabase.DatabaseName : "NA");
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("0000088", DiagnosticParameter.Oii(string.Format(CultureInfo.InvariantCulture, "Import NavData, database {0}", arg)), MonitorType.ALStatementsExecuted, Category.Sql))
		{
			using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
			{
				try
				{
					using BinaryReader reader = ((reportProgress == null) ? new BinaryReader(stream, Encoding.UTF8, leaveStreamOpenWhenDone) : new BinaryReaderWithProgressReporting(stream, Encoding.UTF8, leaveStreamOpenWhenDone, reportProgress, totalBytesInStream));
					NavSqlDataExportHeader navSqlDataExportHeader = new NavSqlDataExportHeader(reader);
					bool emptyAppDatabase = !NavSqlMetadataCommands.TableExists(NavGlobal.AppDatabase, "$ndo$dbproperty");
					IReadOnlyDictionary<string, bool> readOnlyDictionary2;
					if (allCompanies && navSqlDataExportHeader.CompanyNames != null)
					{
						IReadOnlyDictionary<string, bool> readOnlyDictionary = navSqlDataExportHeader.CompanyNames.ToDictionary((string c) => c, (string c) => true);
						readOnlyDictionary2 = readOnlyDictionary;
					}
					else
					{
						readOnlyDictionary2 = companyImports;
					}
					companyImports = readOnlyDictionary2;
					UnsupportedImportGuard(navSqlDataExportHeader, tenantDatabase, includeApplication, includeApplicationDatabaseTables, includeGlobalTables, emptyAppDatabase, companyImports);
					ReadApplicationDatabase(reader, navSqlDataExportHeader, includeApplication, includeApplicationDatabaseTables, commitPerTable, emptyAppDatabase);
					ReadTenantDatabase(reader, navSqlDataExportHeader, tenantDatabase, includeGlobalTables, companyImports, commitPerTable);
					if (tenantDatabase.IsSharedTenantSchema)
					{
						NavSqlDdlCommands.RestartSequencesForAllCompanies(tenantDatabase);
					}
				}
				catch (EndOfStreamException)
				{
					throw new NavNotSupportedException(Lang.UnexpectedReadErrorDuringImport);
				}
				catch (IOException ex2)
				{
					throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.IOErrorDuringImport, ex2.Message), ex2);
				}
			}
		}
	}

	private static void UnsupportedImportGuard(NavSqlDataExportHeader header, NavDatabase tenantDatabase, bool includeApplication, bool includeApplicationDatabaseTables, bool includeGlobalTables, bool emptyAppDatabase, IReadOnlyDictionary<string, bool> companyImports)
	{
		if (header.ExportVersion != 3 && header.ExportVersion != 4)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedWrongVersion, ProductNames.ProductNameShort));
		}
		if (header.NavDatabaseVersion != 130453)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.DataCannotBeImportedWrongVersion, ProductNames.ProductNameShort));
		}
		if (!header.HasApplication & includeApplication)
		{
			throw new NavNotSupportedException(Lang.NoApplicationInNavDatafile);
		}
		if (!header.HasAppDatabaseTables & includeApplicationDatabaseTables)
		{
			throw new NavNotSupportedException(Lang.NoApplicationDataInNavDatafile);
		}
		if (!header.HasGlobalTables & includeGlobalTables)
		{
			throw new NavNotSupportedException(Lang.NoGlobalDataInNavDatafile);
		}
		if (!emptyAppDatabase && (includeApplication | includeApplicationDatabaseTables))
		{
			using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(NavGlobal.AppDatabase);
			if (TenantConfigurationStorage.CountTenantConfigurations(conScope) > 1)
			{
				throw new NavNotSupportedException(includeApplication ? Lang.ApplicationMetadataCannotBeImportedMultiTenant : Lang.ApplicationTableDataCannotBeImportedMultiTenant);
			}
		}
		if ((includeApplication | includeApplicationDatabaseTables) && tenantDatabase != null && !tenantDatabase.Tenant.TenantSettings.AllowAppDatabaseWrite)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CannotImportApplicationDataTableRecords));
		}
		if (emptyAppDatabase && !includeApplication)
		{
			throw new NavNotSupportedException(Lang.MustImportApplicationWhenImportingApplicationDataAndDatabaseIsEmpty);
		}
		if (!emptyAppDatabase & includeApplication)
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CannotImportApplicationIntoExistingNavDatabase, NavGlobal.AppDatabase.DatabaseName, NavGlobal.AppDatabase.DatabaseServer, ProductNames.ProductNameShort));
		}
		IEnumerable<string> enumerable = companyImports?.Keys.Except(header.CompanyNames ?? Enumerable.Empty<string>());
		if (enumerable != null && enumerable.Any())
		{
			throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.CompaniesDoNotExistInExport, string.Join(",", enumerable)));
		}
	}

	public static DataTable GetHeaderInformation(Stream stream, bool leaveStreamOpenWhenDone = false)
	{
		DataTable dataTable = new DataTable("Nav Export Information");
		dataTable.Locale = CultureInfo.CurrentCulture;
		dataTable.Columns.Add("ExportVersion", typeof(int));
		dataTable.Columns.Add("DatabaseVersion", typeof(int));
		dataTable.Columns.Add("Description", typeof(string));
		dataTable.Columns.Add("IncludeApplication", typeof(bool));
		dataTable.Columns.Add("IncludeApplicationData", typeof(bool));
		dataTable.Columns.Add("TenantId", typeof(string));
		dataTable.Columns.Add("IncludeGlobalData", typeof(bool));
		dataTable.Columns.Add("CompanyName", typeof(string[]));
		dataTable.Columns.Add("ExportDateTime", typeof(DateTime));
		using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveStreamOpenWhenDone);
		NavSqlDataExportHeader navSqlDataExportHeader = new NavSqlDataExportHeader(reader);
		dataTable.Rows.Add(navSqlDataExportHeader.ExportVersion, navSqlDataExportHeader.NavDatabaseVersion, navSqlDataExportHeader.Description, navSqlDataExportHeader.HasApplication, navSqlDataExportHeader.HasAppDatabaseTables, navSqlDataExportHeader.TenantId, navSqlDataExportHeader.HasGlobalTables, (navSqlDataExportHeader.CompanyNames == null) ? null : navSqlDataExportHeader.CompanyNames.ToArray(), navSqlDataExportHeader.ExportDateTime);
		return dataTable;
	}

	public static DataTable GetCompanyInformation(Stream stream)
	{
		DataTable dataTable = new DataTable("Nav Export Information");
		dataTable.Locale = CultureInfo.CurrentCulture;
		dataTable.Columns.Add("CompanyName");
		using BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
		NavSqlDataExportHeader navSqlDataExportHeader = new NavSqlDataExportHeader(reader);
		if (navSqlDataExportHeader.CompanyNames != null)
		{
			foreach (string companyName in navSqlDataExportHeader.CompanyNames)
			{
				dataTable.Rows.Add(companyName);
			}
		}
		return dataTable;
	}
}
