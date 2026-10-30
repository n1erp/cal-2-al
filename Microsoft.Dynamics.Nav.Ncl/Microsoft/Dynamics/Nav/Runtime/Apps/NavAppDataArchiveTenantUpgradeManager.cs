using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppDataArchiveTenantUpgradeManager
{
	private readonly HashSet<Tuple<string, int>> handledTablesByCompanyAndId = new HashSet<Tuple<string, int>>();

	private NavAppRuntimeMetadata UpgradeAppMetadata { get; set; }

	private INavAppDataArchiveRepository ArchiveRepository { get; set; }

	private NavAppPackageTableDataRetriever PackageDataRetriever { get; set; }

	private NavAppTableDataImporter TableDataImporter { get; set; }

	private NavSession Session { get; set; }

	protected NavAppDataArchiveTenantUpgradeManager(NavSession session, NavAppInstallationContext installContext)
	{
		Session = session;
		UpgradeAppMetadata = installContext.AppBeingTargeted;
		ArchiveRepository = new DatabaseNavAppDataArchiveRepository(session.Tenant.Database);
		PackageDataRetriever = new NavAppPackageTableDataRetriever(UpgradeAppMetadata.PackageId);
		TableDataImporter = new NavAppTableDataImporter(session.Tenant);
	}

	public NavAppDataArchiveTenantUpgradeManager(NavSession session, NavAppUpgradeContext upgradeContext)
	{
		Session = session;
		UpgradeAppMetadata = upgradeContext.AppBeingTargeted;
		ArchiveRepository = new DatabaseNavAppDataArchiveRepository(session.Tenant.Database);
		PackageDataRetriever = new NavAppPackageTableDataRetriever(UpgradeAppMetadata.PackageId);
		TableDataImporter = new NavAppTableDataImporter(session.Tenant);
	}

	public void VerifyAppTablesHaveBeenHandled()
	{
		HashSet<string> hashSet = new HashSet<string>();
		HashSet<string> allCompanies = Session.GetAllCompanies(useSecurityFilter: false);
		IEnumerable<NavAppDataArchiveRecord> recordsForApp = ArchiveRepository.GetRecordsForApp((Guid)UpgradeAppMetadata.AppId);
		foreach (NavAppDataArchiveRecord item2 in recordsForApp)
		{
			if (string.IsNullOrEmpty(item2.CompanyName) || allCompanies.Contains(item2.CompanyName))
			{
				Tuple<string, int> item = Tuple.Create(item2.CompanyName, item2.TableId);
				if (!handledTablesByCompanyAndId.Contains(item))
				{
					NCLMetaTable nCLMetaTable = NavAppDataArchiveMetaTableFactory.CreateOriginalMetaTableFromArchiveRecord(item2);
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000GS", Category.Extensions, Verbosity.Error, PrivacyClassification.OrganizationIdentifiableInformation, "Unhandled archive table -- TableID:{0}; TableName:{1}; Company:{2}", item2.TableId, nCLMetaTable.TableName, item2.CompanyName);
					hashSet.Add(string.Format(CultureInfo.CurrentCulture, "{0} ({1})", nCLMetaTable.TableId, nCLMetaTable.TableName));
				}
			}
		}
		if (hashSet.Any())
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000J2", Category.Extensions, new NavAppException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppUnhandledUpgradeTableError, string.Join(", ", hashSet))));
		}
	}

	public virtual NavRecordRef GetArchiveRecordRef(int tableId)
	{
		Guid value = ((WrappedValue<Guid>)(object)UpgradeAppMetadata.AppId).Value;
		string companyName = NavCurrentThread.Session.CompanyName;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J3", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Getting archive record ref -- TableID:{0}", tableId);
		MarkTableAsHandled(companyName, tableId);
		NavAppDataArchiveRecord record = ArchiveRepository.GetRecord(value, tableId, companyName);
		if (record != null)
		{
			NCLMetaTable metaTable = NavAppDataArchiveMetaTableFactory.CreateArchiveMetaTableFromArchiveRecord(record);
			using NavRecord fromRecordToBeCloned = new NavRecord(NavCurrentThread.Session, record.TableId, metaTable, isTemporary: false, null, record.CompanyName);
			return new NavRecordRef(NavCurrentThread.Session, fromRecordToBeCloned);
		}
		return null;
	}

	public virtual string GetArchiveVersion()
	{
		Guid value = ((WrappedValue<Guid>)(object)UpgradeAppMetadata.AppId).Value;
		string companyName = NavCurrentThread.Session.CompanyName;
		NavAppDataArchiveRecord navAppDataArchiveRecord = ArchiveRepository.GetRecordsForAppAndCompany(value, companyName).FirstOrDefault();
		if (navAppDataArchiveRecord == null)
		{
			return string.Empty;
		}
		return navAppDataArchiveRecord.AppVersion.ToString();
	}

	public virtual bool RestoreArchiveData(DataError errorLevel, int tableId, bool runApplicationTrigger)
	{
		Guid value = ((WrappedValue<Guid>)(object)UpgradeAppMetadata.AppId).Value;
		string companyName = NavCurrentThread.Session.CompanyName;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J4", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Restoring archive data -- TableID:{0}; RunTrigger:{1}", tableId, runApplicationTrigger);
		MarkTableAsHandled(companyName, tableId);
		NavAppDataArchiveRecord record = ArchiveRepository.GetRecord(value, tableId, companyName);
		if (record == null)
		{
			return true;
		}
		NCLMetaTable metaTable = NavAppDataArchiveMetaTableFactory.CreateArchiveMetaTableFromArchiveRecord(record);
		using NavRecord navRecord = new NavRecord(NavCurrentThread.Session, record.TableId, metaTable, isTemporary: false, null, record.CompanyName);
		NCLMetaTable metaTableById = NavCurrentThread.Session.NCLMetadata.GetMetaTableById(tableId);
		using NavRecord navRecord2 = metaTableById.CreateObjectInstance(NavCurrentThread.Session);
		if (KeyFieldsAreIdentical(navRecord.MetaTable.PrimaryKey, navRecord2.MetaTable.PrimaryKey))
		{
			NavAppGroup navAppGroup = NavCurrentThread.ResolveAppGroup();
			NavAppRuntimeMetadata objectOwner = navAppGroup.GetObjectOwner(ObjectType.Table, tableId);
			return (objectOwner != null && ((WrappedValue<Guid>)(object)objectOwner.AppId)?.Value == value) ? RestoreFullTable(errorLevel, navRecord, navRecord2, runApplicationTrigger) : RestoreModifiedTable(errorLevel, navRecord, navRecord2, runApplicationTrigger);
		}
		if (errorLevel == DataError.ThrowError)
		{
			throw new NavAppException(string.Format(CultureInfo.CurrentCulture, Lang.NavAppUpgradePrimaryKeyDoesntMatch, navRecord2.TableID));
		}
		return false;
	}

	public virtual void DeleteArchiveData(int tableId)
	{
		Guid value = ((WrappedValue<Guid>)(object)UpgradeAppMetadata.AppId).Value;
		string companyName = NavCurrentThread.Session.CompanyName;
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J5", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Deleting archive data -- TableID:{0}", tableId);
		MarkTableAsHandled(companyName, tableId);
		NavSqlDdlCommands.PurgeAppArchiveRecordAndTable(NavCurrentThread.Session.Tenant.Database, value, companyName, tableId);
	}

	public virtual void LoadPackageData(int tableId)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J6", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Loading package data -- TableID:{0}", tableId);
		MarkTableAsHandled(string.Empty, tableId);
		Stream dataStreamForTable = PackageDataRetriever.GetDataStreamForTable(tableId);
		if (dataStreamForTable != null)
		{
			TableDataImporter.Import(dataStreamForTable);
		}
	}

	private void MarkTableAsHandled(string companyName, int tableId)
	{
		Tuple<string, int> item = Tuple.Create(companyName, tableId);
		handledTablesByCompanyAndId.Add(item);
	}

	private static bool RestoreFullTable(DataError errorLevel, NavRecord archiveRec, NavRecord destRec, bool runApplicationTrigger)
	{
		try
		{
			if (archiveRec.ALFindSet(errorLevel))
			{
				do
				{
					destRec.Clear();
					foreach (NCLMetaField normalField in archiveRec.MetaTable.NormalFields)
					{
						if (normalField.FieldNavType == NavType.BLOB)
						{
							archiveRec.CalcFields(DataError.ThrowError, new int[1] { normalField.FieldNo });
						}
						NavValue fieldValue = archiveRec.GetFieldValue(normalField.FieldNo);
						if (runApplicationTrigger)
						{
							destRec.ALValidate(normalField.FieldNo, fieldValue, destRec);
						}
						destRec.SetFieldValue(normalField.FieldNo, fieldValue);
					}
					if (!destRec.ALInsert(errorLevel, runApplicationTrigger))
					{
						return false;
					}
				}
				while (archiveRec.ALNext() != 0);
				return true;
			}
		}
		catch (Exception) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	private static bool RestoreModifiedTable(DataError errorLevel, NavRecord archiveRec, NavRecord destRec, bool runApplicationTrigger)
	{
		try
		{
			if (archiveRec.ALFindSet(errorLevel))
			{
				do
				{
					destRec.Clear();
					foreach (NCLMetaField keyFields in archiveRec.MetaTable.PrimaryKey.KeyFieldsList)
					{
						if (keyFields.FieldNavType == NavType.BLOB)
						{
							archiveRec.CalcFields(DataError.ThrowError, new int[1] { keyFields.FieldNo });
						}
						NavValue fieldValue = archiveRec.GetFieldValue(keyFields.FieldNo);
						if (runApplicationTrigger)
						{
							destRec.ALValidate(keyFields.FieldNo, fieldValue, null);
						}
						destRec.SetFieldValue(keyFields.FieldNo, fieldValue);
					}
					if (!destRec.ALFind(DataError.TrapError))
					{
						continue;
					}
					foreach (NCLMetaField normalField in archiveRec.MetaTable.NormalFields)
					{
						if (normalField.FieldNavType == NavType.BLOB)
						{
							archiveRec.CalcFields(DataError.ThrowError, new int[1] { normalField.FieldNo });
						}
						if (!normalField.FieldIsPartOfPrimaryKey && !normalField.FieldIsTimestamp)
						{
							NavValue fieldValue2 = archiveRec.GetFieldValue(normalField.FieldNo);
							if (runApplicationTrigger)
							{
								destRec.ALValidate(normalField.FieldNo, fieldValue2, null);
							}
							destRec.SetFieldValue(normalField.FieldNo, fieldValue2);
						}
					}
					if (!destRec.ALModify(errorLevel, runApplicationTrigger))
					{
						return false;
					}
				}
				while (archiveRec.ALNext() != 0);
				return true;
			}
		}
		catch (Exception) when (errorLevel == DataError.TrapError)
		{
		}
		return false;
	}

	private static bool KeyFieldsAreIdentical(NCLMetaKey key1, NCLMetaKey key2)
	{
		IEnumerable<int> first = key1.KeyFieldsList.Select((NCLMetaField x) => x.FieldNo);
		IEnumerable<int> second = key2.KeyFieldsList.Select((NCLMetaField x) => x.FieldNo);
		return first.SequenceEqual(second);
	}
}
