using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Apps.Configuration;
using Microsoft.Dynamics.Nav.Runtime.Apps.OperationManagement;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavManagementTasks : INCLAdminService
{
	private class Activity<T>
	{
		public T Result { get; set; }

		public bool Done { get; private set; }

		public void MarkAsDone()
		{
			Done = true;
		}
	}

	[Flags]
	private enum UserValueFlags
	{
		None = 0,
		HasSid = 1,
		HasUserName = 2,
		OnlyOne = 4,
		Both = 8
	}

	private const string GlobalLicenseTable = "[master].[dbo].[$ndo$srvproperty]";

	private const string ApplicationLicenseTable = "[{0}].[dbo].[$ndo$dbproperty]";

	private const string TenantLicenseTable = "[{0}].[dbo].[$ndo$tenantproperty]";

	private const string LicenseFieldName = "[license]";

	private const int SandboxConfigCleanupCodeunitId = 1882;

	public const string ImportExportExtension = "navdata";

	private readonly NavSession session;

	public NavManagementTasks(NavSession session)
	{
		this.session = session;
	}

	public ServerResponse ImportLicense(byte[] license, LicenseDatabase licenseDatabase)
	{
		NavDatabase database = null;
		switch (licenseDatabase)
		{
		case LicenseDatabase.Default:
			if (session.Tenant.TenantType == NavTenantType.Normal && HasDatabaseLicense(LicenseDatabase.Tenant, session.Database))
			{
				licenseDatabase = LicenseDatabase.Tenant;
				database = session.Database;
			}
			else if (HasDatabaseLicense(LicenseDatabase.NavDatabase, session.AppDatabase))
			{
				licenseDatabase = LicenseDatabase.NavDatabase;
				database = session.AppDatabase;
			}
			else
			{
				licenseDatabase = LicenseDatabase.Master;
				database = session.AppDatabase;
			}
			break;
		case LicenseDatabase.Master:
		case LicenseDatabase.NavDatabase:
			database = session.AppDatabase;
			break;
		case LicenseDatabase.Tenant:
			database = session.Tenant.Database;
			if (session.Tenant.TenantType != NavTenantType.Normal)
			{
				throw new NavTenantNotFoundException("Tenant not specified. A tenant must be specified when importing a license into a tenant.");
			}
			break;
		}
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(session.AppDatabase, updateOrlockingActivity: true);
		using NavSqlConnectionScope navSqlConnectionScope2 = NavSqlConnectionScope.Create(session.Tenant.Database, updateOrlockingActivity: true);
		try
		{
			if (licenseDatabase == LicenseDatabase.Tenant && session.Tenant.TenantType != NavTenantType.Normal)
			{
				throw new NavTenantNotFoundException("Tenant not specified. A tenant must be specified when importing a license into a tenant.");
			}
			if (licenseDatabase == LicenseDatabase.NavDatabase || licenseDatabase == LicenseDatabase.Tenant)
			{
				NavCurrentThread.Session.VerifyExecutePermission(new ApplicationObjectId(ObjectType.System, 6300));
			}
			if (licenseDatabase != LicenseDatabase.Tenant)
			{
				if (session.Tenant.TenantType == NavTenantType.Normal)
				{
					SetDatabaseLicense(LicenseDatabase.Tenant, null, session.Tenant.Database);
				}
				if (licenseDatabase != LicenseDatabase.NavDatabase)
				{
					SetDatabaseLicense(LicenseDatabase.NavDatabase, null, session.AppDatabase);
				}
			}
			SetDatabaseLicense(licenseDatabase, license, database);
			if (licenseDatabase == LicenseDatabase.Tenant)
			{
				session.Tenant.Database.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: true);
				if (session.Tenant.GenerateTenantAppInsightsGuidIfNeeded())
				{
					AzureLicenseUsageService.Instance.EnqueueTenant(session.Tenant);
				}
			}
			else
			{
				foreach (NavTenant item in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: true))
				{
					item.Database.PermissionSetupMonitor.ResetPermissionSetup(clearLicenseInfo: true);
					if (item.GenerateTenantAppInsightsGuidIfNeeded())
					{
						AzureLicenseUsageService.Instance.EnqueueTenant(item);
					}
				}
			}
			return new ServerResponse();
		}
		catch (Exception exception)
		{
			navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
			navSqlConnectionScope2.TransactionEnd(NavSqlEndTransactionType.Rollback);
			LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
			throw;
		}
	}

	public ServerResponse ImportConfigurationPackageFile(byte[] packageData, string setupType, int? processingOrder, bool force)
	{
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(session.AppDatabase, updateOrlockingActivity: true))
		{
			bool flag = false;
			try
			{
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				RapidStartManagement.ImportConfigurationPackageFile(session, packageData, setupType, processingOrder, force);
			}
			catch (Exception exception)
			{
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
				throw;
			}
			finally
			{
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(commit: true);
				}
			}
		}
		return new ServerResponse();
	}

	public ServerResponse ImportMembershipEntitlement(byte[] entitlementsFile, string type, string id, string name, string entitlementSetId, string entitlementSetName, bool isEvaluation, bool force)
	{
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(session.AppDatabase, updateOrlockingActivity: true))
		{
			bool flag = false;
			bool flag2 = false;
			int[] tableObjects = new int[3] { 2000000195, 2000000190, 2000000191 };
			try
			{
				session.MaximizePermissions(tableObjects);
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				EntitlementProvider.ImportMembershipEntitlement(session, entitlementsFile, type, id, name, entitlementSetId, entitlementSetName, isEvaluation, force);
			}
			catch (Exception exception)
			{
				flag2 = true;
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
				throw;
			}
			finally
			{
				session.RemoveMaximizedPermissions(tableObjects);
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(!flag2);
				}
			}
		}
		return new ServerResponse();
	}

	public ServerResponse RemoveConfigurationPackageFile(string code)
	{
		using (NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(session.AppDatabase, updateOrlockingActivity: true))
		{
			bool flag = false;
			try
			{
				if (!session.IsTransactionActive())
				{
					session.BeginTransaction();
					flag = true;
				}
				RapidStartManagement.RemoveConfigurationPackageFile(session, code);
			}
			catch (Exception exception)
			{
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				LogWriterHelper.LogExceptionEvent(exception, EventLogEntryType.Error);
				throw;
			}
			finally
			{
				if (flag && session.IsTransactionActive())
				{
					session.EndTransaction(commit: true);
				}
			}
		}
		return new ServerResponse();
	}

	public Version GetReleaseVersion()
	{
		if (Version.TryParse(NavCurrentThread.Session.SystemCodeunitFactory.VersionTriggers.InvokeReleaseVersion().Value, out var result))
		{
			return result;
		}
		return new Version(0, 0, 0, 0);
	}

	public DataTable GetNavDataEntries(int tableId, NavFilterGroup[] filterGroups)
	{
		NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(tableId);
		DataTable dataTable = new DataTable(metaTableById.TableName.Replace(" ", string.Empty));
		dataTable.Locale = CultureInfo.InvariantCulture;
		List<NCLMetaField> list = (from f in metaTableById.Fields
			orderby f.FieldIndex
			where f.FieldNo != 0
			select f).ToList();
		foreach (NCLMetaField item in list)
		{
			DataColumn dataColumn = new DataColumn();
			dataColumn.ColumnName = item.FieldName.Replace(" ", string.Empty);
			dataColumn.DataType = item.FieldClrType;
			dataColumn.ExtendedProperties.Add("FieldNo", item.FieldNo);
			dataTable.Columns.Add(dataColumn);
		}
		using (new MaximizedPermissionScope(session, new int[1] { tableId }))
		{
			using NavRecord navRecord = new NavRecord(session, tableId, SecurityFiltering.Ignored);
			navRecord.ALInit();
			navRecord.SetAllowedTableViewFilters(filterGroups);
			if (navRecord.ALFind(DataError.ThrowError, "-"))
			{
				navRecord.CalcFields(DataError.ThrowError, navRecord.GetAutoCalcFields());
				do
				{
					List<object> list2 = new List<object>();
					foreach (NCLMetaField item2 in list)
					{
						bool flag = item2.FieldNavType == NavType.BLOB;
						if (flag)
						{
							navRecord.CalcFields(DataError.ThrowError, new int[1] { item2.FieldNo });
						}
						NavValue fieldValue = navRecord.GetFieldValue(item2.FieldNo);
						list2.Add(flag ? fieldValue.GetBytes() : fieldValue.ClientObject);
					}
					dataTable.Rows.Add(list2.ToArray());
				}
				while (navRecord.ALNext() != 0);
			}
		}
		dataTable.ExtendedProperties.Add("NavApplicationVersion", session.SystemCodeunitFactory.VersionTriggers.InvokeApplicationVersion().Value);
		return dataTable;
	}

	public void ExportNavData(string fileName, string description, bool includeApplication, bool includeApplicationData, bool includeGlobalData, bool allCompanies, ReadOnlyCollection<string> companyNames)
	{
		if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
		{
			fileName = Path.ChangeExtension(fileName, "navdata");
		}
		if (File.Exists(fileName))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.ExportFileAlreadyExists, fileName));
		}
		try
		{
			if ((includeGlobalData || ((companyNames != null) | allCompanies)) && session.Tenant.TenantType != NavTenantType.Normal)
			{
				throw new NavTenantNotFoundException();
			}
			using FileStream stream = File.Create(fileName);
			using (session.CreateExclusiveTenantActionScope())
			{
				NavSqlImportExportData.Export(stream, description, includeApplication, includeApplicationData, includeGlobalData, allCompanies, companyNames, session);
			}
		}
		catch (Exception ex)
		{
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: false);
			throw;
		}
	}

	public void ImportNavData(string fileName, bool includeApplicationData, bool includeGlobalData, bool commitPerTable, IReadOnlyDictionary<string, bool> companyImports)
	{
		if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
		{
			fileName = Path.ChangeExtension(fileName, "navdata");
		}
		try
		{
			if (!File.Exists(fileName))
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.ExportFileDoesNotExist, fileName));
			}
			using FileStream stream = File.OpenRead(fileName);
			if ((includeGlobalData || (companyImports != null && companyImports.Any())) && session.Tenant.TenantType != NavTenantType.Normal)
			{
				throw new NavTenantNotFoundException();
			}
			NavSqlImportExportData.Import(stream, includeApplication: false, includeApplicationData, session.Tenant.Database, includeGlobalData, companyImports, null, 0L, leaveStreamOpenWhenDone: false, commitPerTable);
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: false);
			throw;
		}
	}

	internal static NavAppInstallPreCheckHelper InstallNavAppPrecheck(string name, string publisher, Version version, NavTenant tenant, bool force, bool doNotLoadData, NavSession navSession, bool performLanguageCheck)
	{
		NavAppInstallPreCheckHelper helper = new NavAppInstallPreCheckHelper
		{
			Installed = new List<NavAppRuntimeMetadata>(),
			Published = new List<NavAppRuntimeMetadata>(),
			NeedToChange = new List<NavAppRuntimeMetadata>(),
			DependenciesToInstall = new List<NavAppRuntimeMetadata>(),
			HasData = new List<INavAppReference>(),
			Result = ChangeNavAppStateResult.AppChangeSuccessful
		};
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		if (performLanguageCheck && !WindowsLanguageDataProvider.IsAvailableLanguage(navSession.LCID))
		{
			helper.Result = ChangeNavAppStateResult.LanguageNotSupported;
			try
			{
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.LanguageNotEnabledOrMissingLocalization, CultureInfo.GetCultureInfo(navSession.LCID).Name);
			}
			catch (Exception)
			{
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.ExceptionLanguageNotInstalled, navSession.LCID);
			}
		}
		else
		{
			helper.Published = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(tenant.Id).ToList();
			List<NavAppRuntimeMetadata> list = helper.Published.Where((NavAppRuntimeMetadata app) => (string.IsNullOrEmpty(name) || string.Equals(app.Name, name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || string.Equals(app.Publisher, publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || app.Version == version)).ToList();
			if (list.Count > 1)
			{
				helper.Result = ChangeNavAppStateResult.MultipleAppsFound;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.MultiplePublishedAppsMatch_WhereUniquenessIsNecessary);
			}
			else
			{
				NavAppRuntimeMetadata val = list.SingleOrDefault();
				if (val == null)
				{
					helper.Result = ChangeNavAppStateResult.AppNotFound;
					helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppPublishCheck_UnknownApp);
				}
				else if (val.ExtensionType == NavExtensionType.CSide)
				{
					helper.Result = ChangeNavAppStateResult.V1ExtensionsUnsupported;
					helper.Error = Lang.V1ExtensionUnsupportedAction;
				}
				else
				{
					helper.Target = val;
					NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(val);
					if (publishProperties.PackageType == NavAppPackageType.SymbolsOnly)
					{
						helper.Result = ChangeNavAppStateResult.SymbolsPackageCannotBeInstalled;
						helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.SymbolsPackageCannotBeInstalled, val.Name, val.Version, val.Publisher);
					}
					else
					{
						CheckInstalledAndUninstalledState(helper, tenant, val, doNotLoadData);
						if (helper.Result == ChangeNavAppStateResult.AppChangeSuccessful)
						{
							helper.Synced = NavAppInstallationManagement.GetAppsSyncedForTenantInTenantDatabase(tenant.Database).ToList();
							IEnumerable<UninstalledAppRecord> enumerable;
							using (NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(navSession.Tenant.Database, TransactionType.Browse))
							{
								DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(navSession.Tenant.Database, conScope);
								enumerable = databaseUninstalledAppRepository.GetAllForTenant(navSession.Tenant.Id).ToList();
							}
							helper.HasData = enumerable.Where((UninstalledAppRecord u) => u.HasData).Cast<INavAppReference>().ToList();
							NavAppV2DependencyResolver.NavAppDependencyCheckResult navAppDependencyCheckResult = NavAppV2DependencyResolver.PackagesAppDependsOn(val, (IEnumerable<INavAppReference>)helper.Installed, enumerable, helper.Published, helper.Synced);
							if (navAppDependencyCheckResult.UnsatisfiedDependencies.Any())
							{
								helper.Result = ChangeNavAppStateResult.UnsatisfiedDependenciesForInstall;
								helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppInstallCheck_UnsatisfiedDependencies, val.TechnicalDisplayName, navAppDependencyCheckResult.UnsatisfiedDependenciesString);
							}
							else
							{
								helper.DependenciesToInstall = navAppDependencyCheckResult.SatisfiedDependencies.ToList();
								if (helper.DependenciesToInstall.Any((NavAppRuntimeMetadata dependency) => !helper.Installed.Exists((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.PackageId == (WrappedComparable<Guid>)(object)dependency.PackageId)) && !force)
								{
									helper.Result = ChangeNavAppStateResult.DependenciesFoundWithoutForceSet;
									helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppWasNotInstalledDueToDependencies, val.TechnicalDisplayName, tenant.Id, string.Join(", ", helper.DependenciesToInstall.Select((NavAppRuntimeMetadata m) => m.Name)));
								}
								else
								{
									helper.NeedToChange.AddRange(helper.DependenciesToInstall);
									helper.NeedToChange.Add(val);
									helper.NeedToChange.RemoveAll((NavAppRuntimeMetadata p) => helper.Installed.Any((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.PackageId == (WrappedComparable<Guid>)(object)p.PackageId));
									IEnumerable<NavAppRuntimeMetadata> source = helper.NeedToChange.Where((NavAppRuntimeMetadata p) => helper.Installed.Any((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)p.AppId));
									if (source.Any())
									{
										helper.Result = ChangeNavAppStateResult.DependencyCompatibilityError;
										helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppWasNotInstalledDueToIncompatibility, val.TechnicalDisplayName, tenant.Id, string.Join(", ", source.Select((NavAppRuntimeMetadata m) => m.Name)));
									}
									else if (val.ExtensionType == NavExtensionType.ModernDev)
									{
										CheckSyncedState(helper);
										_ = helper.Result;
									}
								}
							}
						}
					}
				}
			}
		}
		return helper;
	}

	private static void CheckSyncedState(NavAppInstallPreCheckHelper helper)
	{
		foreach (NavAppRuntimeMetadata app in helper.NeedToChange)
		{
			AppSyncTrackingRecord appSyncTrackingRecord = helper.Synced.FirstOrDefault((AppSyncTrackingRecord synced) => (WrappedComparable<Guid>)(object)synced.AppId == (WrappedComparable<Guid>)(object)app.AppId);
			if (appSyncTrackingRecord == null)
			{
				helper.Result = ChangeNavAppStateResult.AppNotSynced;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_NotSynced, app.TechnicalDisplayName);
				break;
			}
			if (app.Version > appSyncTrackingRecord.Version)
			{
				helper.Result = ChangeNavAppStateResult.SyncedVersionTooOld;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_SyncedVersionTooOld, app.TechnicalDisplayName, appSyncTrackingRecord.Version);
				break;
			}
			if (app.Version < appSyncTrackingRecord.BaselineVersion)
			{
				helper.Result = ChangeNavAppStateResult.SyncBaselineVersionTooNew;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSyncCheck_BaselineVersionTooNew, app.TechnicalDisplayName, appSyncTrackingRecord.BaselineVersion);
				break;
			}
		}
	}

	private static void CheckInstalledAndUninstalledState(NavAppInstallPreCheckHelper helper, NavTenant tenant, NavAppRuntimeMetadata runtimeMetadata, bool doNotLoadData)
	{
		helper.Installed = tenant.NavAppGroup.OrderedAppMetadata.ToList();
		NavAppRuntimeMetadata val = helper.Installed.FirstOrDefault((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)runtimeMetadata.AppId);
		if (val != null)
		{
			if ((WrappedComparable<Guid>)(object)val.PackageId == (WrappedComparable<Guid>)(object)runtimeMetadata.PackageId)
			{
				helper.Result = ChangeNavAppStateResult.AppAlreadyChangedWarning;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppAlreadyInstalledWarning, runtimeMetadata.TechnicalDisplayName, tenant.Id);
			}
			else
			{
				helper.Result = ChangeNavAppStateResult.AppAlreadyChangedError;
				helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppAlreadyInstalled, runtimeMetadata.TechnicalDisplayName, tenant.Id);
			}
		}
		else
		{
			if (doNotLoadData || runtimeMetadata.ExtensionType != NavExtensionType.ModernDev)
			{
				return;
			}
			List<UninstalledAppRecord> source = (from u in NavAppInstallationManagement.GetAppsUninstalledForTenantInTenantDatabase(tenant.Database, tenant.Id)
				where u.HasData
				select u).ToList();
			UninstalledAppRecord uninstalledAppRecord = source.FirstOrDefault((UninstalledAppRecord m) => (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)runtimeMetadata.AppId);
			if (uninstalledAppRecord != null)
			{
				if (uninstalledAppRecord.Version < runtimeMetadata.Version)
				{
					helper.Result = ChangeNavAppStateResult.InstallOlderVersionUninstalled;
					helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppInstallCheck_OlderVersionFound, runtimeMetadata.TechnicalDisplayName, tenant.Id);
				}
				else if (uninstalledAppRecord.Version > runtimeMetadata.Version)
				{
					helper.Result = ChangeNavAppStateResult.InstallNewerVersionUninstalled;
					helper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppInstallCheck_NewerVersionFound, runtimeMetadata.TechnicalDisplayName);
				}
			}
		}
	}

	public UnpublishNavAppResponse UnpublishNavApp(UnpublishNavAppArguments arguments)
	{
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		Result val = NavAppUnpublisher.Unpublish(arguments.PackageId, arguments.Name, arguments.Publisher, arguments.Version, arguments.TenantId);
		if (!val.Success)
		{
			return UnpublishNavAppResponse.CreateForError(ErrorExtensions.ToSingleMessage(val.Error));
		}
		return UnpublishNavAppResponse.CreateForSuccess();
	}

	public PublishNavAppResponse PublishNavApp(PublishNavAppArguments arguments)
	{
		Result val = NavAppPublisher.PublishFromArguments(arguments);
		if (!val.Success)
		{
			return PublishNavAppResponse.CreateForError(ErrorExtensions.ToSingleMessage(val.Error));
		}
		return PublishNavAppResponse.CreateForSuccess();
	}

	public GetNavAppRuntimePackageResponse GetNavAppRuntimePackage(GetNavAppRuntimePackageArguments arguments)
	{
		Result<Stream> val = NavAppRuntimePackageGenerator.GenerateRuntimePackage(arguments);
		if (!((Result)val).Success)
		{
			return GetNavAppRuntimePackageResponse.CreateForError(ErrorExtensions.ToSingleMessage(((Result)val).Error));
		}
		return GetNavAppRuntimePackageResponse.CreateForSuccess(val.Value);
	}

	public RepairNavAppResponse RepairNavApp(RepairNavAppArguments arguments)
	{
		Result val = NavAppPublisher.Repair(arguments);
		if (!val.Success)
		{
			return RepairNavAppResponse.CreateForError(ErrorExtensions.ToSingleMessage(val.Error));
		}
		return RepairNavAppResponse.CreateForSuccess();
	}

	public ChangeNavAppStateResponse StartNavAppDataUpgrade(StartNavAppDataUpgradeArguments startNavAppDataUpgradeArguments)
	{
		return NavAppUpgrader.UpgradeNavApp(startNavAppDataUpgradeArguments);
	}

	public Guid InstallNavAppAsync(InstallNavAppArguments installNavAppArguments)
	{
		return InstallNavApp(installNavAppArguments, async: true).TaskId;
	}

	public ChangeNavAppStateResponse InstallNavApp(InstallNavAppArguments installNavAppArguments)
	{
		return InstallNavApp(installNavAppArguments, async: false);
	}

	private ChangeNavAppStateResponse InstallNavApp(InstallNavAppArguments installNavAppArguments, bool async)
	{
		string name = installNavAppArguments.Name;
		string tenantId = installNavAppArguments.TenantId;
		string publisher = installNavAppArguments.Publisher;
		Version version = installNavAppArguments.Version;
		bool doNotLoadData = installNavAppArguments.DoNotLoadData;
		bool force = installNavAppArguments.Force;
		string origin = installNavAppArguments.Origin;
		LanguageSetting language = ((installNavAppArguments.Language != null) ? new LanguageSetting(installNavAppArguments.Language) : null);
		string activityId = Guid.NewGuid().ToString();
		string scopeName = string.Format(CultureInfo.InvariantCulture, "Extension install request -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; DoNotLoadData:{5}; Force:{6}; Origin:{7}; Language:{8}", NavAppTraceHelper.CreateActivityIdString(activityId), name, publisher, version, tenantId, doNotLoadData, force, origin, language);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000BE", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension install request starting -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; DoNotLoadData:{5}; Force:{6}; Origin:{7}; Language:{8}", DiagnosticParameter.SystemMetadata(NavAppTraceHelper.CreateActivityIdString(activityId)), DiagnosticParameter.SystemMetadata(name), DiagnosticParameter.SystemMetadata(publisher), DiagnosticParameter.SystemMetadata(version), DiagnosticParameter.Oii(tenantId), DiagnosticParameter.SystemMetadata(doNotLoadData), DiagnosticParameter.SystemMetadata(force), DiagnosticParameter.SystemMetadata(origin), DiagnosticParameter.SystemMetadata(language));
		NavTenant targetTenant = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true);
		Activity<ChangeNavAppStateResponse> activity = new Activity<ChangeNavAppStateResponse>();
		Action action = () =>
		{
			NavAppRuntimeMetadata resolvedExtension = null;
			using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016S", DiagnosticParameter.Oii(scopeName), MonitorType.None, Category.Extensions, createNewActivity: false))
			{
				try
				{
					((ITenantSessionHandler)targetTenant).RunTenantActionInSystemSession((Action<NavTenant, NavSession>)((NavTenant tenant, NavSession session) =>
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000190", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "System session transition -- {0}", NavAppTraceHelper.CreateActivityIdString(activityId));
						lock (tenant.ExtensionInstallUninstallLockObject)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000BF", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "In-process tenant lock acquired");
							using NavSqlConnectionScope tenantDatabaseLockScope = NavSqlConnectionScope.CreateForceNewConnection(tenant.Database, TransactionType.UpdateNoLocks);
							using NavSqlDatabaseLock navSqlDatabaseLock = NavDatabase.AcquireNavAppTenantCrossProcessLock(tenantDatabaseLockScope, tenant.Id);
							if (!navSqlDatabaseLock.IsLockAcquired())
							{
								DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000WF", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension Cross-Process lock acquisition failed");
								activity.Result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeInProgress, string.Format(CultureInfo.CurrentCulture, Lang.NavAppTenantInUseInstall, name, tenant.Id));
							}
							else
							{
								DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008B", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Cross-process tenant lock acquired");
								NavAppTenantGuard.BlockInstallIfTenantIsInImproperState(tenant);
								NavAppInstallPreCheckHelper navAppInstallPreCheckHelper = InstallNavAppPrecheck(name, publisher, version, tenant, force, doNotLoadData, session, language != null);
								resolvedExtension = navAppInstallPreCheckHelper.Target;
								if (navAppInstallPreCheckHelper.Result == ChangeNavAppStateResult.AppChangeSuccessful)
								{
									using (new NavAppUsageEventSuppressionScope(NavCurrentThread.Session, "Extension install"))
									{
										activity.Result = InternalInstallNavApp(navAppInstallPreCheckHelper, name, tenant, doNotLoadData, activityId);
										return;
									}
								}
								DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000090", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Precheck failed -- Result:{0}; Error:{1}", navAppInstallPreCheckHelper.Result, navAppInstallPreCheckHelper.Error);
								activity.Result = new ChangeNavAppStateResponse(navAppInstallPreCheckHelper.Result, navAppInstallPreCheckHelper.Error);
							}
						}
					}), true, true, false, language);
					ChangeNavAppStateResponse result = activity.Result;
					if (result != null && result.Result.In(ChangeNavAppStateResult.AppChangeSuccessful, ChangeNavAppStateResult.AppAlreadyChangedWarning))
					{
						NotifyInstallOperationSuccess(resolvedExtension, targetTenant);
					}
					else
					{
						NotifyInstallOperationFailure(resolvedExtension, targetTenant, activity.Result?.Error);
					}
				}
				catch (Exception ex)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000BG", Category.Extensions, ex);
					activity.Result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.UnknownAppError, ex.Message);
					NotifyInstallOperationFailure(resolvedExtension, targetTenant, activity.Result?.Error);
				}
				finally
				{
					ChangeNavAppStateResponse result2 = activity.Result;
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008C", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension install request ending -- {0}; Result:{1}; Error:{2}", NavAppTraceHelper.CreateActivityIdString(activityId), result2?.Result, result2?.Error);
					activity.MarkAsDone();
				}
			}
		};
		if (async)
		{
			Guid guid = Guid.NewGuid();
			targetTenant.ActiveManagementTasks.TryAdd(guid, activity);
			Task.Factory.StartNew(ExecuteActionInExclusiveAccessModeIfNecessary(targetTenant, action));
			ChangeNavAppStateResponse changeNavAppStateResponse = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeInProgress);
			changeNavAppStateResponse.TaskId = guid;
			return changeNavAppStateResponse;
		}
		action();
		return activity.Result;
	}

	private static void NotifyInstallOperationSuccess(NavAppRuntimeMetadata target, NavTenant tenant)
	{
		NotifyInstallOperation(target, tenant, NavAppOperationResultStatus.Success, Lang.NavAppInstallSuccess.FormatWithResolvedCulture((target != null) ? target.FriendlyDisplayName : null));
	}

	private static void NotifyInstallOperationFailure(NavAppRuntimeMetadata target, NavTenant tenant, string error)
	{
		NotifyInstallOperation(target, tenant, NavAppOperationResultStatus.Failure, error);
	}

	private static void NotifyInstallOperation(NavAppRuntimeMetadata target, NavTenant tenant, NavAppOperationResultStatus status, string details)
	{
		if (target != null)
		{
			NavAppOperationMetadataResult navAppOperationMetadataResult = ((target.Scope != NavAppScope.Tenant) ? ((NavAppOperationMetadataResult)new NavAppDeployTargetOperationResult()) : ((NavAppOperationMetadataResult)new NavAppDeployPackageOperationResult
			{
				Version = target.Version
			}));
			navAppOperationMetadataResult.AppId = ((WrappedValue<Guid>)(object)target.AppId).Value;
			navAppOperationMetadataResult.TenantId = tenant.Id;
			navAppOperationMetadataResult.Details = details;
			navAppOperationMetadataResult.ResultStatus = status;
			NavGlobal.NavAppTenantOperationManagementProvider.CreateForTenant(tenant).NotifyExtensionInstallOperation(navAppOperationMetadataResult);
		}
	}

	private Action ExecuteActionInExclusiveAccessModeIfNecessary(NavTenant tenant, Action action)
	{
		if (ExclusiveTenantActionElevatedScope.HasElevatedAccess)
		{
			return () =>
			{
				using (new ExclusiveTenantActionElevatedScope(new ExclusiveTenantActionScope(tenant, session.ExclusiveAccessTicket, NavEnvironment.Instance.Tenants.Configuration)))
				{
					action();
				}
			};
		}
		return action;
	}

	public ChangeNavAppStateResponse CheckInstallNavAppStatus(string tenantId, Guid taskId)
	{
		NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true);
		if (tenantById.ActiveManagementTasks.TryGetValue(taskId, out var value))
		{
			if (value is Activity<ChangeNavAppStateResponse> { Done: not false } activity)
			{
				tenantById.ActiveManagementTasks.TryRemove(taskId, out value);
				return activity.Result;
			}
			return null;
		}
		throw new NavAsyncJobNotFoundException();
	}

	internal static ChangeNavAppStateResponse InternalInstallNavApp(NavAppInstallPreCheckHelper precheckResult, string name, NavTenant tenant, bool doNotLoadData, string activityId)
	{
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		if (NavTenantManagementTasks.TenantHasPendingChanges(tenant))
		{
			return new ChangeNavAppStateResponse(ChangeNavAppStateResult.MetadataOutOfSync, string.Format(CultureInfo.CurrentCulture, Lang.NavAppMetadataInstallOutOfSync, name, tenant.Id));
		}
		PrepareAllAppGroupsForInstallChain(precheckResult);
		ChangeNavAppStateResponse result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeSuccessful);
		List<PackageId> list = new List<PackageId>(precheckResult.Installed.Select((NavAppRuntimeMetadata m) => m.PackageId));
		foreach (NavAppRuntimeMetadata appRuntimeMetadata in precheckResult.NeedToChange)
		{
			try
			{
				bool flag = precheckResult.HasData.Any((INavAppReference r) => (WrappedComparable<Guid>)(object)r.AppId == (WrappedComparable<Guid>)(object)appRuntimeMetadata.AppId);
				list.Add(appRuntimeMetadata.PackageId);
				NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(list);
				NavAppSyncOptions syncOptions = ((!doNotLoadData) ? NavAppSyncOptions.Default : NavAppSyncOptions.None);
				NavAppInstallationContext context = NavAppInstallationContext.CreateForInstall(tenant, orCreateGroupForAppCombination, appRuntimeMetadata, syncOptions, activityId, flag);
				string name2 = NavCurrentThread.Session.Culture.Name;
				string content = string.Format(CultureInfo.InvariantCulture, "Extension internal install -- {0}; PackageId:{1}; AppId:{2}; Name:{3}; Publisher:{4}; Version:{5}; Tenant:{6}; Culture:{7}; HasData:{8}", NavAppTraceHelper.CreateActivityIdString(activityId), appRuntimeMetadata.PackageId, appRuntimeMetadata.AppId, appRuntimeMetadata.Name, appRuntimeMetadata.Publisher, appRuntimeMetadata.Version, tenant.Id, name2, flag);
				using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016T", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
				{
					try
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J9", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension internal install starting -- {0}; PackageId:{1}; AppId:{2}; Name:{3}; Publisher:{4}; Version:{5}; Tenant:{6}; Culture:{7}; HasData:{8}", NavAppTraceHelper.CreateActivityIdString(activityId), appRuntimeMetadata.PackageId, appRuntimeMetadata.AppId, appRuntimeMetadata.Name, appRuntimeMetadata.Publisher, appRuntimeMetadata.Version, tenant.Id, name2, flag);
						NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(context);
						navAppInstallationProcessor.InstallNavApp(doNotLoadData);
					}
					catch (Exception exception)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000JA", Category.Extensions, exception);
						throw;
					}
					finally
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JB", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension internal install ending -- {0}", NavAppTraceHelper.CreateActivityIdString(activityId));
					}
				}
				try
				{
					NavGlobal.MetadataProvider.UpdateMetadataVersion();
					NavEnvironment.Instance.NavAppGroupUpdater.UpdateGroupForChangedTenant(tenant);
					tenant.StartNasIfNotRunning();
				}
				catch (Exception exception2)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000YH", Category.Extensions, exception2);
				}
			}
			catch (NavBaseException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000008G", Category.Extensions, ex);
				ChangeNavAppStateResult result2;
				string error;
				if (ex is NavNCLEvaluateException)
				{
					result2 = ChangeNavAppStateResult.EvaluationError;
					error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSecurityFilterEvaluationFailure, name, tenant.Id, ex.Message);
				}
				else if (ex is NavSqlDatabaseSyncException)
				{
					result2 = ChangeNavAppStateResult.TenantSyncFailure;
					error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppInstallSyncFailure, name, tenant.Id, ex.Message);
				}
				else if (ex is NavAppException)
				{
					result2 = ChangeNavAppStateResult.UnknownAppError;
					error = ex.Message;
				}
				else
				{
					if (!(ex is NavUpgradeActivityExecutionException))
					{
						throw;
					}
					result2 = ChangeNavAppStateResult.AppInstallCodeExecutionFailure;
					error = Lang.NavAppInstallCodeExecutionFailure.FormatWithResolvedCulture(name, tenant.Id, ex.Message);
				}
				result = new ChangeNavAppStateResponse(result2, error);
				break;
			}
			catch (Exception exception3)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000007G", Category.Extensions, exception3);
				throw;
			}
		}
		return result;
	}

	private static void PrepareAllAppGroupsForInstallChain(NavAppInstallPreCheckHelper precheckResult)
	{
		List<PackageId> list = new List<PackageId>(precheckResult.Installed.Select((NavAppRuntimeMetadata m) => m.PackageId));
		foreach (NavAppRuntimeMetadata item in precheckResult.NeedToChange)
		{
			list.Add(item.PackageId);
			NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(list);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000J8", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing extension group for install case -- Group:{0}", orCreateGroupForAppCombination.GroupId);
			NavEnvironment.Instance.NavAppGroupUpdater.PrepareAppGroup(orCreateGroupForAppCombination);
		}
	}

	internal NavAppUninstallPreCheckHelper UninstallAppPrecheck(string name, string publisher, Version version, NavTenant tenant, bool force)
	{
		NavAppUninstallPreCheckHelper navAppUninstallPreCheckHelper = new NavAppUninstallPreCheckHelper
		{
			Installed = new List<NavAppRuntimeMetadata>(),
			Published = new List<NavAppRuntimeMetadata>(),
			NeedToChange = new List<NavAppRuntimeMetadata>(),
			DependentsToUninstall = new List<NavAppRuntimeMetadata>(),
			Result = ChangeNavAppStateResult.AppChangeSuccessful
		};
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		navAppUninstallPreCheckHelper.Published = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(tenant.Id).ToList();
		List<NavAppRuntimeMetadata> apps = navAppUninstallPreCheckHelper.Published.Where((NavAppRuntimeMetadata app) => (string.IsNullOrEmpty(name) || string.Equals(app.Name, name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || string.Equals(app.Publisher, publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || app.Version == version)).ToList();
		if (apps.Count == 0)
		{
			navAppUninstallPreCheckHelper.Result = ChangeNavAppStateResult.AppNotFound;
			navAppUninstallPreCheckHelper.Error = string.Format(CultureInfo.CurrentCulture, Lang.AppNotFound, name);
		}
		else
		{
			navAppUninstallPreCheckHelper.Installed = tenant.NavAppGroup.OrderedAppMetadata.ToList();
			List<PackageId> installedPackages = navAppUninstallPreCheckHelper.Installed.Select((NavAppRuntimeMetadata m) => m.PackageId).ToList();
			List<PackageId> intersect = installedPackages.Where((PackageId id) => apps.Any((NavAppRuntimeMetadata app) => (WrappedComparable<Guid>)(object)app.PackageId == (WrappedComparable<Guid>)(object)id)).ToList();
			if (intersect.Count > 1)
			{
				navAppUninstallPreCheckHelper.Result = ChangeNavAppStateResult.MultipleAppsFound;
				navAppUninstallPreCheckHelper.Error = string.Format(CultureInfo.CurrentCulture, Lang.MultiplePublishedAppsMatch_WhereUniquenessIsNecessary);
			}
			else if (intersect.Count == 0)
			{
				navAppUninstallPreCheckHelper.Result = ChangeNavAppStateResult.AppAlreadyChangedWarning;
				navAppUninstallPreCheckHelper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppAlreadyUninstalledWarning, name, tenant.Id);
			}
			else
			{
				NavAppRuntimeMetadata val = apps.Single((NavAppRuntimeMetadata app) => (WrappedComparable<Guid>)(object)app.PackageId == (WrappedComparable<Guid>)(object)intersect.Single());
				if (val.ExtensionType == NavExtensionType.CSide)
				{
					navAppUninstallPreCheckHelper.Result = ChangeNavAppStateResult.V1ExtensionsUnsupported;
					navAppUninstallPreCheckHelper.Error = Lang.V1ExtensionUnsupportedAction;
				}
				else
				{
					navAppUninstallPreCheckHelper.Target = val;
					navAppUninstallPreCheckHelper.DependentsToUninstall = new List<NavAppRuntimeMetadata>(NavAppV2DependencyResolver.AppsDependentOnPackage(val, navAppUninstallPreCheckHelper.Installed, navAppUninstallPreCheckHelper.Published));
					if (navAppUninstallPreCheckHelper.DependentsToUninstall.Any((NavAppRuntimeMetadata app) => installedPackages.Contains(app.PackageId)) && !force)
					{
						navAppUninstallPreCheckHelper.Result = ChangeNavAppStateResult.DependenciesFoundWithoutForceSet;
						navAppUninstallPreCheckHelper.Error = string.Format(CultureInfo.CurrentCulture, Lang.NavAppWasNotUninstalledDueToDependents, name, tenant.Id, string.Join(", ", navAppUninstallPreCheckHelper.DependentsToUninstall.Select((NavAppRuntimeMetadata m) => m.Name)));
					}
					else
					{
						navAppUninstallPreCheckHelper.NeedToChange.AddRange(navAppUninstallPreCheckHelper.DependentsToUninstall);
						navAppUninstallPreCheckHelper.NeedToChange.Add(val);
						navAppUninstallPreCheckHelper.NeedToChange.RemoveAll((NavAppRuntimeMetadata app) => !installedPackages.Contains(app.PackageId));
					}
				}
			}
		}
		return navAppUninstallPreCheckHelper;
	}

	public ChangeNavAppStateResponse UninstallNavApp(UninstallNavAppArguments uninstallNavAppArguments)
	{
		string name = uninstallNavAppArguments.Name;
		string publisher = uninstallNavAppArguments.Publisher;
		Version version = uninstallNavAppArguments.Version;
		string tenantId = uninstallNavAppArguments.TenantId;
		bool doNotSaveData = uninstallNavAppArguments.DoNotSaveData;
		bool force = uninstallNavAppArguments.Force;
		string origin = uninstallNavAppArguments.Origin;
		string activityId = Guid.NewGuid().ToString();
		ChangeNavAppStateResponse changeNavAppStateResponse = null;
		string content = string.Format(CultureInfo.InvariantCulture, "Extension uninstall request -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; DoNotSaveData:{5}; Force:{6}; Origin:{7}", NavAppTraceHelper.CreateActivityIdString(activityId), name, publisher, version, tenantId, doNotSaveData, force, origin);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016U", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
		{
			try
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000BH", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension uninstall request starting -- {0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}; DoNotSaveData:{5}; Force:{6}; Origin:{7}", NavAppTraceHelper.CreateActivityIdString(activityId), name, publisher, version, tenantId, doNotSaveData, force, origin);
				NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true);
				lock (tenantById.ExtensionInstallUninstallLockObject)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000BI", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "In-process tenant lock acquired");
					using NavSqlConnectionScope tenantDatabaseLockScope = NavSqlConnectionScope.CreateForceNewConnection(tenantById.Database, TransactionType.UpdateNoLocks);
					using NavSqlDatabaseLock navSqlDatabaseLock = NavDatabase.AcquireNavAppTenantCrossProcessLock(tenantDatabaseLockScope, tenantById.Id);
					if (!navSqlDatabaseLock.IsLockAcquired())
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000WH", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension Cross-Process lock acquisition failed");
						changeNavAppStateResponse = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeInProgress, string.Format(CultureInfo.CurrentCulture, Lang.NavAppTenantInUseUninstall, name, tenantById.Id));
					}
					else
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008H", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Cross-process tenant lock acquired");
						NavAppTenantGuard.BlockUninstallIfTenantIsInImproperState(tenantById);
						NavAppUninstallPreCheckHelper navAppUninstallPreCheckHelper = UninstallAppPrecheck(name, publisher, version, tenantById, force);
						if (navAppUninstallPreCheckHelper.Result != ChangeNavAppStateResult.AppChangeSuccessful)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("0000091", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Precheck failed -- Result:{0}; Error:{1}", navAppUninstallPreCheckHelper.Result, navAppUninstallPreCheckHelper.Error);
							changeNavAppStateResponse = new ChangeNavAppStateResponse(navAppUninstallPreCheckHelper.Result, navAppUninstallPreCheckHelper.Error);
						}
						else
						{
							using (new NavAppUsageEventSuppressionScope(NavCurrentThread.Session, "Extension uninstall"))
							{
								changeNavAppStateResponse = InternalUninstallNavApp(navAppUninstallPreCheckHelper, name, tenantById, doNotSaveData, activityId);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000BJ", Category.Extensions, ex);
				changeNavAppStateResponse = new ChangeNavAppStateResponse(ChangeNavAppStateResult.UnknownAppError, ex.Message);
			}
			finally
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000008I", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension uninstall request ending -- {0}; Result:{1}; Error:{2}", NavAppTraceHelper.CreateActivityIdString(activityId), changeNavAppStateResponse?.Result, changeNavAppStateResponse?.Error);
			}
		}
		return changeNavAppStateResponse;
	}

	internal static ChangeNavAppStateResponse InternalUninstallNavApp(NavAppUninstallPreCheckHelper precheckResult, string name, NavTenant tenant, bool doNotSaveData, string activityId)
	{
		NavGlobal.AppDatabase.ChangeListener?.ForceHandleTrackingEvent();
		if (NavTenantManagementTasks.TenantHasPendingChanges(tenant))
		{
			return new ChangeNavAppStateResponse(ChangeNavAppStateResult.MetadataOutOfSync, string.Format(CultureInfo.CurrentCulture, Lang.NavAppMetadataUninstallOutOfSync, name, tenant.Id));
		}
		PrepareAllAppGroupsForUninstallChain(precheckResult);
		ChangeNavAppStateResponse result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.AppChangeSuccessful);
		List<PackageId> list = new List<PackageId>(precheckResult.Installed.Select((NavAppRuntimeMetadata m) => m.PackageId));
		foreach (NavAppRuntimeMetadata item in precheckResult.NeedToChange)
		{
			try
			{
				list.Remove(item.PackageId);
				NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(list);
				NavAppSyncOptions syncOptions = ((!doNotSaveData) ? NavAppSyncOptions.Default : NavAppSyncOptions.None);
				NavAppInstallationContext context = NavAppInstallationContext.CreateForUninstall(tenant, orCreateGroupForAppCombination, item, syncOptions, activityId);
				string content = string.Format(CultureInfo.InvariantCulture, "Extension internal uninstall -- {0}; PackageId:{1}; AppId:{2}; Name:{3}; Publisher:{4}; Version:{5}; Tenant:{6}", NavAppTraceHelper.CreateActivityIdString(activityId), item.PackageId, item.AppId, item.Name, item.Publisher, item.Version, tenant.Id);
				using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("000016V", DiagnosticParameter.Oii(content), MonitorType.None, Category.Extensions, createNewActivity: false))
				{
					try
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JD", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Extension internal uninstall starting -- {0}; PackageId:{1}; AppId:{2}; Name:{3}; Publisher:{4}; Version:{5}; Tenant:{6}", NavAppTraceHelper.CreateActivityIdString(activityId), item.PackageId, item.AppId, item.Name, item.Publisher, item.Version, tenant.Id);
						NavAppInstallationProcessor navAppInstallationProcessor = new NavAppInstallationProcessor(context);
						navAppInstallationProcessor.UninstallNavApp(doNotSaveData);
					}
					catch (Exception exception)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000JE", Category.Extensions, exception);
						throw;
					}
					finally
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000JF", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Extension internal uninstall ending  -- {0}", NavAppTraceHelper.CreateActivityIdString(activityId));
					}
				}
				try
				{
					NavGlobal.MetadataProvider.UpdateMetadataVersion();
					NavEnvironment.Instance.NavAppGroupUpdater.UpdateGroupForChangedTenant(tenant);
				}
				catch (Exception exception2)
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00000YI", Category.Extensions, exception2);
				}
			}
			catch (NavBaseException ex)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000008M", Category.Extensions, ex);
				if (ex is NavSqlDatabaseSyncException)
				{
					result = new ChangeNavAppStateResponse(ChangeNavAppStateResult.TenantSyncFailure, string.Format(CultureInfo.CurrentCulture, Lang.NavAppUninstallSyncFailure, name, tenant, ex.Message));
					break;
				}
				throw;
			}
			catch (Exception exception3) when (DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTagExceptionFilter("000007I", Category.Extensions, exception3))
			{
				throw;
			}
		}
		return result;
	}

	private static void PrepareAllAppGroupsForUninstallChain(NavAppUninstallPreCheckHelper precheckResult)
	{
		List<PackageId> list = new List<PackageId>(precheckResult.Installed.Select((NavAppRuntimeMetadata m) => m.PackageId));
		foreach (NavAppRuntimeMetadata item in precheckResult.NeedToChange)
		{
			list.Remove(item.PackageId);
			NavAppGroup orCreateGroupForAppCombination = NavGlobal.NavAppGroupResolver.GetOrCreateGroupForAppCombination(list);
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000R8", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Initializing extension group for uninstall case -- Group:{0}", orCreateGroupForAppCombination.GroupId);
			NavEnvironment.Instance.NavAppGroupUpdater.PrepareAppGroup(orCreateGroupForAppCombination);
		}
	}

	public IEnumerable<NavAppInfoResponse> GetNavAppInfo(GetNavAppInfoArguments getNavAppInfoArguments)
	{
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		string name = getNavAppInfoArguments.Name;
		string appId = getNavAppInfoArguments.AppId;
		string publisher = getNavAppInfoArguments.Publisher;
		Version version = getNavAppInfoArguments.Version;
		Version compatibilityId = getNavAppInfoArguments.CompatibilityId;
		List<Tuple<NavAppRuntimeMetadata, NavAppInfoResponse>> list = (from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadata().Where((NavAppRuntimeMetadata rec) =>
			{
				NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(rec);
				if (getNavAppInfoArguments.TenantSpecificProperties && publishProperties.IsDeveloperExtension() && !publishProperties.IsDeveloperExtensionForTenant(getNavAppInfoArguments.TenantId))
				{
					return false;
				}
				return GetNavAppInfoIsExpectedPackageType(publishProperties.PackageType, getNavAppInfoArguments) && (string.IsNullOrEmpty(name) || rec.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(appId) || ((object)rec.AppId).ToString().Equals(appId, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || rec.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || version.Equals(rec.Version)) && (compatibilityId == null || compatibilityId.Equals(rec.CompatibilityId));
			})
			select Tuple.Create<NavAppRuntimeMetadata, NavAppInfoResponse>(m, ToResponse(m))).ToList();
		if (getNavAppInfoArguments.TenantSpecificProperties)
		{
			IEnumerable<NavAppRuntimeMetadata> allMetadataVisibleToTenant = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(getNavAppInfoArguments.TenantId);
			List<NavAppInfoResponse> source = GetInstalledNavAppInfo(getNavAppInfoArguments.TenantId).ToList();
			NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(getNavAppInfoArguments.TenantId);
			List<INavAppMetadataReference> appsInstalledForTenantInTenantDatabase = NavAppInstallationManagement.GetAppsInstalledForTenantInTenantDatabase(tenantById.Database);
			List<AppSyncTrackingRecord> syncedApps = GetSyncedApps(getNavAppInfoArguments.TenantId);
			List<NavAppInfoResponse> appsWithData = GetAppsWithData(getNavAppInfoArguments.TenantId);
			foreach (Tuple<NavAppRuntimeMetadata, NavAppInfoResponse> item2 in list)
			{
				NavAppRuntimeMetadata meta = item2.Item1;
				NavAppInfoResponse item = item2.Item2;
				NavAppInfoResponse matchingInstallRecord = source.FirstOrDefault((NavAppInfoResponse i) => i.AppId == ((WrappedValue<Guid>)(object)meta.AppId).Value && i.Version == meta.Version);
				if (matchingInstallRecord != null)
				{
					item.IsInstalled = true;
					if (appsInstalledForTenantInTenantDatabase.Any((INavAppMetadataReference e) => ((WrappedValue<Guid>)(object)((INavAppReference)e).AppId).Value == matchingInstallRecord.AppId && ((INavAppReference)e).Version < matchingInstallRecord.Version))
					{
						item.NeedsUpgrade = true;
					}
				}
				item.SyncState = NavAppSyncState.NotSynced;
				AppSyncTrackingRecord appSyncTrackingRecord = syncedApps.FirstOrDefault((AppSyncTrackingRecord s) => ((WrappedValue<Guid>)(object)s.AppId).Value == ((WrappedValue<Guid>)(object)meta.AppId).Value);
				if (appSyncTrackingRecord != null && meta.Version >= appSyncTrackingRecord.BaselineVersion && meta.Version <= appSyncTrackingRecord.Version)
				{
					item.SyncState = NavAppSyncState.Synced;
				}
				NavAppInfoResponse navAppInfoResponse = appsWithData.FirstOrDefault((NavAppInfoResponse d) => d.AppId == ((WrappedValue<Guid>)(object)meta.AppId).Value);
				if (navAppInfoResponse != null)
				{
					item.ExtensionDataVersion = navAppInfoResponse.Version;
				}
				item.IsPublished = allMetadataVisibleToTenant.Any((NavAppRuntimeMetadata tevia) => (WrappedComparable<Guid>)(object)tevia.PackageId == (WrappedComparable<Guid>)(object)meta.PackageId);
			}
		}
		else
		{
			foreach (Tuple<NavAppRuntimeMetadata, NavAppInfoResponse> item3 in list)
			{
				item3.Item2.IsPublished = true;
			}
		}
		return list.Select((Tuple<NavAppRuntimeMetadata, NavAppInfoResponse> mar) => mar.Item2);
	}

	public IEnumerable<NavAppInfoResponse> GetInstalledNavAppInfo(string tenantId)
	{
		NavGlobal.AppDatabase.ChangeListener.ForceHandleTrackingEvent();
		return GetNavAppInfoResponses(NavEnvironment.Instance.Tenants.GetTenantById(tenantId, includeFailed: true).NavAppGroup.OrderedAppMetadata);
	}

	private static List<AppSyncTrackingRecord> GetSyncedApps(string tenantId)
	{
		NavDatabase database = NavEnvironment.Instance.Tenants.GetTenantById(tenantId).Database;
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Browse);
		DatabaseAppSyncTrackingRepository databaseAppSyncTrackingRepository = new DatabaseAppSyncTrackingRepository(database, conScope);
		return databaseAppSyncTrackingRepository.GetAll().ToList();
	}

	private List<NavAppInfoResponse> GetAppsWithData(string tenantId)
	{
		List<NavAppInfoResponse> list = GetInstalledNavAppInfo(tenantId).ToList();
		NavDatabase database = NavEnvironment.Instance.Tenants.GetTenantById(tenantId).Database;
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Browse);
		DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(database, conScope);
		foreach (UninstalledAppRecord item in databaseUninstalledAppRepository.GetAllForTenant(tenantId))
		{
			if (item.HasData)
			{
				list.Add(new NavAppInfoResponse
				{
					AppId = ((WrappedValue<Guid>)(object)item.AppId).Value,
					Version = item.Version,
					Name = item.Name,
					Publisher = item.Publisher
				});
			}
		}
		DatabaseNavAppDataArchiveRepository databaseNavAppDataArchiveRepository = new DatabaseNavAppDataArchiveRepository(database);
		foreach (Tuple<Guid, Version> app in databaseNavAppDataArchiveRepository.GetAppsForTenant())
		{
			if (list.All((NavAppInfoResponse ext) => ext.AppId != app.Item1))
			{
				list.Add(new NavAppInfoResponse
				{
					AppId = app.Item1,
					Version = app.Item2
				});
			}
		}
		return list;
	}

	public NavTenantRuntimeSettingsList GetTenantsWithAppInstalled(GetNavAppTenantArguments getNavAppTenantArguments)
	{
		string name = getNavAppTenantArguments.Name;
		string appId = getNavAppTenantArguments.AppId;
		string publisher = getNavAppTenantArguments.Publisher;
		Version version = getNavAppTenantArguments.Version;
		bool includeFailed = getNavAppTenantArguments.IncludeFailed;
		NavTenantRuntimeSettingsList navTenantRuntimeSettingsList = new NavTenantRuntimeSettingsList(new List<NavTenantRuntimeSettings>());
		foreach (NavTenant item in NavEnvironment.Instance.Tenants.AllTenants(includeSystemTenant: false, includeFailed))
		{
			NavTenantRuntimeSettings runtimeTenantSettings = item.GetRuntimeTenantSettings();
			if (runtimeTenantSettings == null || navTenantRuntimeSettingsList.TenantRuntimeSettingsList.Contains(runtimeTenantSettings))
			{
				continue;
			}
			foreach (NavAppRuntimeMetadata orderedAppMetadatum in item.NavAppGroup.OrderedAppMetadata)
			{
				if ((string.IsNullOrEmpty(name) || orderedAppMetadatum.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(appId) || ((object)orderedAppMetadatum.AppId).ToString().Equals(appId, StringComparison.OrdinalIgnoreCase)) && (string.IsNullOrEmpty(publisher) || orderedAppMetadatum.Publisher.Equals(publisher, StringComparison.OrdinalIgnoreCase)) && (version == null || version.Equals(orderedAppMetadatum.Version)))
				{
					navTenantRuntimeSettingsList.TenantRuntimeSettingsList.Add(runtimeTenantSettings);
					break;
				}
			}
		}
		return navTenantRuntimeSettingsList;
	}

	private static bool GetNavAppInfoIsExpectedPackageType(NavAppPackageType actualPackageType, GetNavAppInfoArguments arguments)
	{
		if (arguments.SymbolsOnly)
		{
			return actualPackageType == NavAppPackageType.SymbolsOnly;
		}
		return actualPackageType != NavAppPackageType.SymbolsOnly;
	}

	private static IEnumerable<NavAppInfoResponse> GetNavAppInfoResponses(IEnumerable<NavAppRuntimeMetadata> records)
	{
		List<NavAppInfoResponse> list = new List<NavAppInfoResponse>();
		foreach (NavAppRuntimeMetadata record in records)
		{
			list.Add(ToResponse(record));
		}
		return list;
	}

	private static NavAppInfoResponse ToResponse(NavAppRuntimeMetadata metadata)
	{
		NavAppManifest manifest = NavEnvironment.Instance.NavAppMetadataRetriever.GetManifest(metadata.PackageId);
		NavAppInfoResponse navAppInfoResponse = NavAppTypeExtensions.ToServerResponseType(manifest);
		navAppInfoResponse.ExtensionType = NavAppTypeExtensions.ToServerResponseType(metadata.ExtensionType);
		navAppInfoResponse.Scope = metadata.Scope;
		navAppInfoResponse.DeveloperTenantId = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(metadata).DeveloperTenantId;
		navAppInfoResponse.PackageId = ((WrappedValue<Guid>)(object)metadata.PackageId).Value;
		return navAppInfoResponse;
	}

	public NavAppTableDataExportResult ExportNavAppTableData(string folderPath, int tableId, bool force)
	{
		try
		{
			if (session.Tenant.TenantType != NavTenantType.Normal)
			{
				throw new NavTenantNotFoundException();
			}
			if (File.Exists(folderPath))
			{
				string message = string.Format(CultureInfo.InvariantCulture, "A valid directory could not be found for this path: {0}.", folderPath);
				throw new NavNCLArgumentException(message);
			}
			if (!Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}
			string text = string.Format(CultureInfo.InvariantCulture, "TAB{0}{1}", tableId, ".navxdata");
			string path = Path.Combine(folderPath, text);
			if (File.Exists(path) && !force)
			{
				string message2 = string.Format(CultureInfo.InvariantCulture, "The file {0} already exists. Use the Force parameter to overwrite.", text);
				throw new NavNCLArgumentException(message2);
			}
			try
			{
				using FileStream destination = File.Create(path);
				int records = new NavAppTableDataExporter(session.Tenant).Export(destination, tableId);
				return new NavAppTableDataExportResult
				{
					Path = path,
					Records = records
				};
			}
			catch
			{
				File.Delete(path);
				throw;
			}
		}
		catch (Exception ex)
		{
			NavFileHelper.MapAndRethrowClrException(ex, writeExceptionToEventLog: false);
			throw;
		}
	}

	public static DataTable GetNavDataExportInformation(string fileName)
	{
		if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
		{
			fileName = Path.ChangeExtension(fileName, "navdata");
		}
		if (!File.Exists(fileName))
		{
			throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.ExportFileDoesNotExist, fileName));
		}
		using FileStream stream = File.OpenRead(fileName);
		return NavSqlImportExportData.GetHeaderInformation(stream);
	}

	public IEnumerable<ReadOnlyRecordBuffer> GetUser(out NCLMetaQueryDefinition queryDefinition)
	{
		NCLMetaTable userTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000120);
		NCLMetaTable userPropertyTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000121);
		NCLMetaTable userPersonalizationTable = NavGlobal.NCLMetadata.GetMetaTableById(2000000073);
		NCLMetaField profileIdfield = userPersonalizationTable.GetFieldByNo(9);
		NCLMetaField companyfield = userPersonalizationTable.GetFieldByNo(15);
		NCLMetaField languageIdfield = userPersonalizationTable.GetFieldByNo(12);
		int nextColumnId = 0;
		queryDefinition = NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, 2000000120, DataLockState.None, userTable.TableName, (NCLMetaQueryDataItem dataItemBeingCreated) =>
		{
			INavFieldMetadata[] fields = userTable.NormalFields.Where((NCLMetaField field) => !field.FieldIsTimestamp).Select((NCLMetaField field) =>
			{
				NCLMetaQueryDataItem dataItem = dataItemBeingCreated;
				int fieldNo = field.FieldNo;
				NavType fieldNavType = field.FieldNavType;
				int fieldDefinedLength = field.FieldDefinedLength;
				string fieldName = field.FieldName;
				int num = nextColumnId;
				nextColumnId = num + 1;
				return NCLMetaQueryColumn.Create(dataItem, fieldNo, fieldNavType, fieldDefinedLength, fieldName, num);
			}).ToArray();
			return new FieldList(fields);
		}).InnerJoin(queryBeingCreated, 2000000121, DataLockState.None, userPropertyTable.TableName, (NCLMetaQueryDataItem dataItemBeingCreated) =>
		{
			INavFieldMetadata[] fields = userPropertyTable.NormalFields.Where((NCLMetaField field) => !field.FieldIsTimestamp && !field.FieldIsPartOfPrimaryKey).Select((NCLMetaField field) =>
			{
				NCLMetaQueryDataItem dataItem = dataItemBeingCreated;
				int fieldNo = field.FieldNo;
				NavType fieldNavType = field.FieldNavType;
				int fieldDefinedLength = field.FieldDefinedLength;
				string fieldName = field.FieldName;
				int num = nextColumnId;
				nextColumnId = num + 1;
				return NCLMetaQueryColumn.Create(dataItem, fieldNo, fieldNavType, fieldDefinedLength, fieldName, num);
			}).ToArray();
			return new FieldList(fields);
		}, (NCLMetaQueryDataItem dataItemBeingCreated) => FiltersAndMarks.Empty, (NCLMetaQueryDataItem dataItemBeingCreated) => new ReadOnlyCollection<NCLMetaDataItemLink>(new NCLMetaDataItemLink[1] { NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, userTable.TableName, 1, 1, NCLMetaFilterModes.None) })).LeftOuterJoin(queryBeingCreated, 2000000073, DataLockState.None, userPersonalizationTable.TableName, (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, 9, profileIdfield.FieldNavType, profileIdfield.FieldDefinedLength, profileIdfield.FieldName, nextColumnId++), NCLMetaQueryColumn.Create(dataItemBeingCreated, 15, companyfield.FieldNavType, companyfield.FieldDefinedLength, companyfield.FieldName, nextColumnId++), NCLMetaQueryColumn.Create(dataItemBeingCreated, 12, languageIdfield.FieldNavType, languageIdfield.FieldDefinedLength, languageIdfield.FieldName, nextColumnId++)), (NCLMetaQueryDataItem dataItemBeingCreated) => FiltersAndMarks.Empty, (NCLMetaQueryDataItem dataItemBeingCreated) => new ReadOnlyCollection<NCLMetaDataItemLink>(new NCLMetaDataItemLink[1] { NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, userTable.TableName, 1, 3, NCLMetaFilterModes.None) })), null);
		return session.DataAccessSource.TenantDataAccess.DataProvider.GetQueryResult(queryDefinition, session.Database.GetCompanyNameToken(string.Empty), 0, 0, FiltersAndMarks.Empty, null, null, SecurityFiltering.Ignored);
	}

	public ServerResponse NewUser(string userName, string fullName, NavUserState state, DateTime expiryDate, string sid, bool changePassword, LicenseType licenseType, string passwordHash, string authenticationKey, bool webServicesKey, DateTime webServicesKeyExpiryDate, string authenticationEmail, string contactEmail, string profileId, Guid applicationId, string company = null, string languageId = null)
	{
		UserValueFlags userValueFlag = GetUserValueFlag(sid, userName);
		if (!userValueFlag.HasFlag(UserValueFlags.OnlyOne))
		{
			throw new NavNCLArgumentException("You can only use one of the following parameters: UserName, Sid or WindowsAccount.");
		}
		bool commit = false;
		Guid guid = Guid.NewGuid();
		try
		{
			session.BeginTransaction();
			using (NavRecord navRecord = new NavRecord(session, 2000000120))
			{
				navRecord.ALInit();
				navRecord.SetFieldValue(1, new NavGuid(guid));
				navRecord.SetFieldValue(2, new NavCode(2048, userName));
				navRecord.SetFieldValue(7, new NavText(int.MaxValue, sid));
				if (userValueFlag.HasFlag(UserValueFlags.HasSid))
				{
					string text = ALDatabase.UserName(sid);
					navRecord.SetFieldValue(2, new NavCode(text.Length, text));
				}
				navRecord.SetFieldValue(3, new NavText(int.MaxValue, fullName));
				navRecord.SetFieldValue(4, NavOption.Create(NCLOptionMetadata.Default, (int)state));
				navRecord.SetFieldValue(5, NavDateTime.Create(expiryDate));
				navRecord.SetFieldValue(10, NavOption.Create(NCLOptionMetadata.Default, (int)licenseType));
				navRecord.SetFieldValue(11, new NavText(int.MaxValue, authenticationEmail));
				navRecord.SetFieldValue(14, new NavText(int.MaxValue, contactEmail));
				navRecord.SetFieldValue(16, new NavGuid(applicationId));
				if (passwordHash != null)
				{
					navRecord.SetFieldValue(8, NavBoolean.Create(changePassword));
				}
				using NavSqlConnectionScope conScope = NavSqlConnectionScope.Create(session.Database, TransactionType.Update);
				using NavSqlDatabaseLock navSqlDatabaseLock = NavSqlDatabaseLock.LockResource(conScope, session.Tenant.Id + userName);
				if (!navSqlDatabaseLock.IsLockAcquired())
				{
					throw NavNCLUserTableUserNameMustBeUniqueException.Create();
				}
				navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
			}
			if (passwordHash != null)
			{
				using NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000121);
				navRecord2.ALGet(new NavGuid(guid));
				navRecord2.SetFieldValueSafe(2, NavType.Text, new NavText(CryptographyHelper.GenerateSaltedPasswordHash(passwordHash, guid)));
				navRecord2.ALModify(runApplicationTrigger: true);
			}
			if (authenticationKey != null)
			{
				UserManagement.SetAuthenticationKey(session, guid, authenticationKey);
			}
			if (webServicesKey)
			{
				UserManagement.CreateWebServicesKey(session, guid, webServicesKeyExpiryDate);
			}
			if (!string.IsNullOrWhiteSpace(profileId))
			{
				NavConfigurationProfile.SetUserProfile(guid, new NavProfileKey(profileId));
			}
			SaveUserPersonalization(new NavGuid(guid), userName, sid, company, languageId);
			commit = true;
			return new ServerResponse();
		}
		finally
		{
			session.EndTransaction(commit);
		}
	}

	public RemoveUserResponse RemoveUser(string userName, string sid)
	{
		NavGuid userIdFromSidOrUserName = GetUserIdFromSidOrUserName(sid, userName);
		using NavRecord navRecord = new NavRecord(session, 2000000120);
		navRecord.ALGet(userIdFromSidOrUserName);
		try
		{
			navRecord.ALDelete(runApplicationTrigger: true);
			return new RemoveUserResponse(RemoveUserResult.UserRemoved);
		}
		catch (NavNCLUserTableUserCannotBeDeletedAlreadyLoggedInException)
		{
			navRecord.SetFieldValue(4, NavOption.Create(NCLOptionMetadata.Default, 1));
			navRecord.ALModify(runApplicationTrigger: true);
			return new RemoveUserResponse(RemoveUserResult.UserDisabled);
		}
	}

	public ServerResponse RenameUser(string currentUserName, string newUserName, string fullName, NavUserState state, DateTime expiryDate, string currentSid, string newSid, bool changePassword, LicenseType licenseType, bool setLicenseType, string passwordHash, string authenticationKey, bool webServicesKey, DateTime webServicesKeyExpiryDate, string authenticationEmail, string contactEmail, string profileId, Guid applicationId, string authenticationObjectId, string company = null, string languageId = null)
	{
		bool commit = false;
		NavGuid userIdFromSidOrUserName = GetUserIdFromSidOrUserName(currentSid, currentUserName);
		try
		{
			session.BeginTransaction();
			using (NavRecord navRecord = new NavRecord(session, 2000000120))
			{
				navRecord.ALGet(userIdFromSidOrUserName);
				UserValueFlags userValueFlag = GetUserValueFlag(newSid, newUserName);
				if (userValueFlag.HasFlag(UserValueFlags.HasSid))
				{
					string text = ALDatabase.UserName(newSid);
					navRecord.SetFieldValue(2, new NavCode(text.Length, text));
					navRecord.SetFieldValue(7, new NavText(int.MaxValue, newSid));
				}
				else if (userValueFlag.HasFlag(UserValueFlags.HasUserName))
				{
					navRecord.SetFieldValue(2, new NavCode(newUserName.Length, newUserName));
				}
				else if (userValueFlag.HasFlag(UserValueFlags.Both))
				{
					throw new NavNCLArgumentException("You can only use one of the following parameters: UserName, Sid or WindowsAccount.");
				}
				if (fullName != null)
				{
					navRecord.SetFieldValue(3, new NavText(int.MaxValue, fullName));
				}
				navRecord.SetFieldValue(4, NavOption.Create(NCLOptionMetadata.Default, (int)state));
				if (expiryDate != DateTime.MinValue)
				{
					navRecord.SetFieldValue(5, NavDateTime.Create(expiryDate));
				}
				if (setLicenseType)
				{
					navRecord.SetFieldValue(10, NavOption.Create(NCLOptionMetadata.Default, (int)licenseType));
				}
				if (authenticationEmail != null)
				{
					navRecord.SetFieldValue(11, new NavText(int.MaxValue, authenticationEmail));
				}
				if (contactEmail != null)
				{
					navRecord.SetFieldValue(14, new NavText(int.MaxValue, contactEmail));
				}
				if (passwordHash != null)
				{
					navRecord.SetFieldValue(8, NavBoolean.Create(changePassword));
				}
				navRecord.SetFieldValue(16, new NavGuid(applicationId));
				navRecord.ALModify(runApplicationTrigger: true);
			}
			if (passwordHash != null || authenticationObjectId != null)
			{
				using NavRecord navRecord2 = new NavRecord(NavCurrentThread.Session, 2000000121);
				navRecord2.ALGet(userIdFromSidOrUserName);
				if (passwordHash != null)
				{
					navRecord2.SetFieldValue(2, new NavText(CryptographyHelper.GenerateSaltedPasswordHash(passwordHash, userIdFromSidOrUserName.ToGuid())));
				}
				if (authenticationObjectId != null)
				{
					navRecord2.SetFieldValue(7, new NavText(authenticationObjectId));
				}
				navRecord2.ALModify(runApplicationTrigger: true);
			}
			if (authenticationKey != null)
			{
				UserManagement.SetAuthenticationKey(session, userIdFromSidOrUserName.ToGuid(), authenticationKey);
			}
			if (webServicesKey)
			{
				UserManagement.CreateWebServicesKey(session, userIdFromSidOrUserName.ToGuid(), webServicesKeyExpiryDate);
			}
			if (!string.IsNullOrWhiteSpace(profileId))
			{
				NavConfigurationProfile.SetUserProfile(userIdFromSidOrUserName.ToGuid(), new NavProfileKey(profileId));
			}
			SaveUserPersonalization(userIdFromSidOrUserName, currentUserName, currentSid, company, languageId);
			commit = true;
			return new ServerResponse();
		}
		finally
		{
			session.EndTransaction(commit);
		}
	}

	private void SaveUserPersonalization(NavGuid userId, string userName, string sid, string company, string languageId)
	{
		bool flag = false;
		bool flag2 = false;
		if (company != null)
		{
			if (string.IsNullOrWhiteSpace(company))
			{
				flag = true;
			}
			else
			{
				if (!session.GetAllCompanies(useSecurityFilter: false).Contains(company))
				{
					session.Diagnostics.SendTraceTag("00001OI", Category.Management, Verbosity.Normal, "Invalid default company name {0} for user {1} with SID {2}.", DiagnosticParameter.Oii(company), DiagnosticParameter.Euii(userName), DiagnosticParameter.Eupi(sid));
					throw new NavCSideDataException(85132272, string.Format(CultureInfo.CurrentCulture, Lang.CompanyNameDoesNotExist, company));
				}
				flag = true;
			}
		}
		int languageId2 = 0;
		if (languageId != null)
		{
			if (string.IsNullOrWhiteSpace(languageId) || languageId.Equals(languageId2.ToString(CultureInfo.InvariantCulture)))
			{
				flag2 = true;
			}
			else
			{
				CultureInfo cultureInfo;
				try
				{
					cultureInfo = ConfigSettings.ParseCultureInfo(languageId);
				}
				catch (CultureNotFoundException)
				{
					session.Diagnostics.SendTraceTag("00001OJ", Category.Management, Verbosity.Normal, "Invalid default language {0} for user {1} with SID {2}.", DiagnosticParameter.SystemMetadata(languageId), DiagnosticParameter.Euii(userName), DiagnosticParameter.Eupi(sid));
					throw new NavNotSupportedLanguageException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLanguageCode, languageId));
				}
				catch (ArgumentOutOfRangeException)
				{
					session.Diagnostics.SendTraceTag("00001OK", Category.Management, Verbosity.Normal, "Invalid default language {0} for user {1} with SID {2}.", DiagnosticParameter.SystemMetadata(languageId), DiagnosticParameter.Euii(userName), DiagnosticParameter.Eupi(sid));
					throw new NavNotSupportedLanguageException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidLanguageCode, languageId));
				}
				if (!CultureInfo.GetCultures(CultureTypes.SpecificCultures).Contains(cultureInfo) || !WindowsLanguageDataProvider.IsAvailableLanguage(cultureInfo.LCID))
				{
					session.Diagnostics.SendTraceTag("00001OL", Category.Management, Verbosity.Normal, "Invalid default language {0} for user {1} with SID {2}.", DiagnosticParameter.SystemMetadata(languageId), DiagnosticParameter.Euii(userName), DiagnosticParameter.Eupi(sid));
					throw new NavNotSupportedLanguageException(string.Format(CultureInfo.CurrentCulture, Lang.LanguageNotEnabledOrMissingLocalization, cultureInfo.Name));
				}
				languageId2 = cultureInfo.LCID;
				flag2 = true;
			}
		}
		if (flag | flag2)
		{
			SaveUserPersonalization(userId, languageId2, 0, overrideLocaleIdInUserProfile: false, company, flag2);
		}
	}

	private void SaveUserPersonalization(NavGuid id, int languageId, int formattingCulture, bool overrideLocaleIdInUserProfile, string companyName = null, bool setLanguage = false)
	{
		using (session.Diagnostics.CreateTimingScope("00001ON", Category.Management, DiagnosticParameter.SystemMetadata("Save User Personalization")))
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000073 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000073);
				session.BeginTransaction();
				bool commit = false;
				try
				{
					bool flag = navRecord.ALGet(DataError.TrapError, id);
					bool flag2 = setLanguage;
					flag2 &= !flag || navRecord.GetFieldValue(12).ToInt32() != languageId;
					if (flag2)
					{
						navRecord.SetFieldValue(12, NavInteger.Create(languageId));
					}
					bool flag3 = formattingCulture != 0;
					int num = ((formattingCulture == 0) ? NavEnvironment.DefaultLanguage : formattingCulture);
					flag3 = flag3 || !flag || ((navRecord.GetFieldValue(27).ToInt32() != num) & overrideLocaleIdInUserProfile);
					if (flag3)
					{
						navRecord.SetFieldValue(27, NavInteger.Create(num));
					}
					bool flag4 = companyName != null && (!flag || navRecord.GetFieldValue(15).ToString() != companyName);
					if (flag4)
					{
						navRecord.SetFieldValue(15, new NavText(int.MaxValue, companyName));
					}
					if (flag)
					{
						if (flag2 | flag4 | flag3)
						{
							navRecord.ALModify(DataError.TrapError);
							session.Diagnostics.SendTraceTag("00001OM", Category.Management, Verbosity.Normal, "Setting user personalization - nav user id is {6} - storing language true/false {0} - value = {1}, storing formattingCulture true/false {2} - value = {3}, storing company true/false {4} - value = {5}", DiagnosticParameter.SystemMetadata(flag2), DiagnosticParameter.SystemMetadata(languageId), DiagnosticParameter.SystemMetadata(flag3), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(flag4), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Eupi(id.ToString()));
						}
					}
					else
					{
						navRecord.SetFieldValue(3, id);
						navRecord.ALInsert(DataError.TrapError);
						session.Diagnostics.SendTraceTag("00001QY", Category.Management, Verbosity.Normal, "Creating user personalization - nav user id is {6} - storing language true/false {0} - value = {1}, storing formattingCulture true/false {2} - value = {3}, storing company true/false {4} - value = {5}", DiagnosticParameter.SystemMetadata(flag2), DiagnosticParameter.SystemMetadata(languageId), DiagnosticParameter.SystemMetadata(flag3), DiagnosticParameter.SystemMetadata(num), DiagnosticParameter.SystemMetadata(flag4), DiagnosticParameter.Oii(companyName), DiagnosticParameter.Eupi(id));
					}
					commit = true;
				}
				finally
				{
					session.EndTransaction(commit);
				}
			}
		}
	}

	public ConvertProfileResponse ConvertProfileConfiguration(ConvertProfileRequest request)
	{
		List<NavXmlConfigurationConversionOptions> list = new List<NavXmlConfigurationConversionOptions>();
		ConvertProfileResponse result = null;
		try
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000072 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000072);
				if (!request.AllProfiles)
				{
					navRecord.ALSetFilter(3, "%1", new NavOemCode(2048, request.ProfileId));
				}
				if (navRecord.ALFindFirst(DataError.TrapError))
				{
					HashSet<string> hashSet = new HashSet<string>();
					do
					{
						string text = navRecord.GetFieldValue(3).ToString();
						if (hashSet.Add(text))
						{
							list.Add(new NavXmlConfigurationConversionOptions(text, request.OverwriteExisting));
						}
					}
					while (navRecord.NextRecord());
				}
			}
			List<(NavXmlConfigurationConversionOptions, Exception)> list2 = new List<(NavXmlConfigurationConversionOptions, Exception)>();
			bool flag = false;
			byte[] conversionResults;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
				{
					foreach (NavXmlConfigurationConversionOptions item3 in list)
					{
						try
						{
							INavXmlConverter navXmlConverter = NavXmlConverterFactory.CreateForConfiguration(item3, new NavXmlConverterMetadataProviderAdapter(session.MetadataProvider), NavEnvironment.Instance.NavV1PageConfigurationRepository, new DatabaseNavPageCustomizationRepository(), request.DownloadResult, archive);
							navXmlConverter.ConvertAndStore();
							flag = true;
						}
						catch (NavAppConverterException item)
						{
							list2.Add((item3, item));
						}
						catch (Exception item2)
						{
							list2.Add((item3, item2));
						}
					}
				}
				if (request.DownloadResult)
				{
					memoryStream.Seek(0L, SeekOrigin.Begin);
					conversionResults = memoryStream.ToArray();
				}
				else
				{
					conversionResults = Array.Empty<byte>();
				}
			}
			if (list2.Any())
			{
				string error = string.Join(";" + Environment.NewLine, list2.Select(((NavXmlConfigurationConversionOptions options, Exception exception) e) => string.Format(CultureInfo.CurrentCulture, Lang.NavAppProfileConfigurationConversionFailed, e.options.SourceProfileKey.ProfileId, e.exception.Message)));
				result = (flag ? new ConvertProfileResponse(ConvertProfileResult.ConversionWithMixedResults, error, conversionResults) : new ConvertProfileResponse(ConvertProfileResult.ConversionError, error));
			}
			else
			{
				result = (list.Any() ? new ConvertProfileResponse(ConvertProfileResult.ConversionSuccess, conversionResults) : new ConvertProfileResponse(ConvertProfileResult.ProfileNotFoundWarning));
			}
		}
		catch (Exception ex)
		{
			result = new ConvertProfileResponse(ConvertProfileResult.UnknownConversionError, ex.Message);
		}
		finally
		{
			session.MetadataProvider.UpdateMetadataVersion();
			session.UserPageMetadataCache.RemoveAll();
		}
		return result;
	}

	public ConvertUserPersonalizationResponse ConvertUserPersonalization(ConvertUserPersonalizationRequest request)
	{
		List<NavXmlPersonalizationConversionOptions> list = new List<NavXmlPersonalizationConversionOptions>();
		ConvertUserPersonalizationResponse result = null;
		try
		{
			using (new MaximizedPermissionScope(session, new int[1] { 2000000075 }))
			{
				using NavRecord navRecord = new NavRecord(session, 2000000075);
				if (!request.AllUsers)
				{
					navRecord.ALSetFilter(3, "%1", new NavGuid(request.UserSid));
				}
				if (navRecord.ALFindFirst(DataError.TrapError))
				{
					HashSet<Guid> hashSet = new HashSet<Guid>();
					do
					{
						Guid value = ((NavGuid)navRecord.GetFieldValue(3)).Value;
						if (hashSet.Add(value))
						{
							list.Add(new NavXmlPersonalizationConversionOptions(value));
						}
					}
					while (navRecord.NextRecord());
				}
			}
			bool flag = false;
			List<(NavXmlPersonalizationConversionOptions, Exception)> list2 = new List<(NavXmlPersonalizationConversionOptions, Exception)>();
			foreach (NavXmlPersonalizationConversionOptions item3 in list)
			{
				try
				{
					INavXmlConverter navXmlConverter = NavXmlConverterFactory.CreateForPersonalization(item3, new NavXmlConverterMetadataProviderAdapter(session.MetadataProvider), NavEnvironment.Instance.NavV1PageConfigurationRepository, new DatabaseNavPageCustomizationRepository());
					navXmlConverter.ConvertAndStore();
					flag = true;
				}
				catch (NavAppConverterException item)
				{
					list2.Add((item3, item));
				}
				catch (Exception item2)
				{
					list2.Add((item3, item2));
				}
			}
			if (list2.Any())
			{
				string error = string.Join(";" + Environment.NewLine, list2.Select(((NavXmlPersonalizationConversionOptions options, Exception exception) e) => string.Format(CultureInfo.CurrentCulture, Lang.NavAppUserPersonalizationConversionFailed, e.options.UserSid, e.exception.Message)));
				result = (flag ? new ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult.ConversionWithMixedResults, error) : new ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult.ConversionError, error));
			}
			else
			{
				result = (list.Any() ? new ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult.ConversionSuccess) : new ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult.UserPersonalizationsNotFoundWarning));
			}
		}
		catch (Exception ex)
		{
			result = new ConvertUserPersonalizationResponse(ConvertUserPersonalizationResult.UnknownError, ex.Message);
		}
		finally
		{
			session.MetadataProvider.UpdateMetadataVersion();
			session.UserPageMetadataCache.RemoveAll();
		}
		return result;
	}

	public void NewCompany(string name, string displayName = null)
	{
		NewCompany(name, evaluationCompany: false, displayName);
	}

	public void NewEvaluationCompany(string name, string displayName = null)
	{
		NewCompany(name, evaluationCompany: true, displayName);
	}

	private void NewCompany(string name, bool evaluationCompany, string displayName)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000006);
		navRecord.ALInit();
		navRecord.SetFieldValue(1, new NavText(name));
		navRecord.SetFieldValue(3, new NavText(displayName));
		navRecord.SetFieldValue(2, NavBoolean.Create(evaluationCompany));
		navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
	}

	public void RemoveCompany(string name, bool forceImmediateDataDeletion)
	{
		if (forceImmediateDataDeletion)
		{
			try
			{
				if (session.Database.TenantCompanies.GetState(name) == CompanyState.Deleting)
				{
					DeleteCompanySystemTask.DeleteCompanyData(session.Tenant, name, commitIndividualDeleteOperations: true);
					return;
				}
			}
			catch (NavCompanyNotFoundException)
			{
			}
			session.CompanyDeletionMode = NavSession.CompanyDeletionType.Complete;
		}
		using NavRecord navRecord = new NavRecord(session, 2000000006);
		navRecord.SetFieldValue(1, new NavText(name));
		navRecord.ALFind("=");
		navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
	}

	public void RenameCompany(string name, string newName)
	{
		bool flag = false;
		if (!session.IsCompanyOpen)
		{
			session.OpenCompany(name, updateUserPersonalization: false, openedForRenameOrDelete: true);
			flag = true;
		}
		try
		{
			using NavRecord navRecord = new NavRecord(session, 2000000006);
			navRecord.SetFieldValue(1, new NavText(name));
			navRecord.ALFind("=");
			navRecord.ALRename(DataError.ThrowError, new NavText(newName));
		}
		finally
		{
			if (flag)
			{
				session.CloseCompany();
			}
		}
	}

	public void CopyCompany(string sourceName, string destinationName)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000006);
		try
		{
			session.CopyFromCompany = session.Database.GetCompanyNameToken(sourceName);
			navRecord.SetFieldValue(1, new NavText(sourceName));
			navRecord.ALFind("=");
			NavValue fieldValue = navRecord.GetFieldValue(2);
			navRecord.ALInit();
			navRecord.SetFieldValue(1, new NavText(destinationName));
			navRecord.SetFieldValue(2, fieldValue);
			navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		finally
		{
			session.CopyFromCompany = 0;
		}
	}

	public DataTable GetCompanies(bool includePartiallyDeletedCompanies)
	{
		DataTable dataTable = new DataTable("Company");
		dataTable.Locale = CultureInfo.CurrentCulture;
		dataTable.Columns.Add("CompanyName");
		dataTable.Columns.Add("CompanyDisplayName");
		dataTable.Columns.Add("EvaluationCompany");
		dataTable.Columns.Add("State");
		using (NavRecord navRecord = new NavRecord(session, 2000000006))
		{
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					dataTable.Rows.Add(navRecord.GetFieldValue(1).ToString(), navRecord.GetFieldValue(3).ToString(), navRecord.GetFieldValue(2).ToBoolean(), CompanyState.Normal);
				}
				while (navRecord.ALNext() > 0);
			}
		}
		if (includePartiallyDeletedCompanies)
		{
			List<string> listOfCompanyNamesWithState = session.Database.TenantCompanies.GetListOfCompanyNamesWithState(CompanyState.Deleting);
			foreach (string item in listOfCompanyNamesWithState)
			{
				dataTable.Rows.Add(item, string.Empty, false, CompanyState.Deleting);
			}
		}
		return dataTable;
	}

	public void SetCompany(SetCompanyArguments setCompanyArguments)
	{
		using NavRecord navRecord = new NavRecord(session, 2000000006);
		navRecord.SetFieldValue(1, new NavText(setCompanyArguments.Name));
		navRecord.ALFind("=");
		if (setCompanyArguments.DisplayName != null)
		{
			navRecord.SetFieldValue(3, new NavText(setCompanyArguments.DisplayName));
		}
		navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
	}

	internal virtual void SetDatabaseLicense(LicenseDatabase licenseDatabase, byte[] license, NavDatabase database)
	{
		string text = FullyQualifiedLicenseTableName(licenseDatabase, database.DatabaseName);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database, updateOrlockingActivity: true);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		if (license == null)
		{
			navSqlCommand.SetCommandText("UPDATE " + text + " SET [license] = null");
		}
		else
		{
			navSqlCommand.SetCommandText("UPDATE " + text + " SET [license] = @License");
			navSqlCommand.Command.Parameters.Add("@License", SqlDbType.Binary, license.Length).Value = license;
		}
		navSqlCommand.ExecuteNonQuery();
	}

	internal static bool HasDatabaseLicense(LicenseDatabase licenseDatabase, NavDatabase database)
	{
		string text = FullyQualifiedLicenseTableName(licenseDatabase, database.DatabaseName);
		using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.Create(database);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText("SELECT DATALENGTH([license]) FROM " + text);
		object obj = navSqlCommand.ExecuteScalar();
		if (Convert.IsDBNull(obj) || (int)obj == 0)
		{
			return false;
		}
		return true;
	}

	private static string FullyQualifiedLicenseTableName(LicenseDatabase licenseDatabase, string databaseName)
	{
		return licenseDatabase switch
		{
			LicenseDatabase.NavDatabase => string.Format(CultureInfo.InvariantCulture, "[{0}].[dbo].[$ndo$dbproperty]", databaseName), 
			LicenseDatabase.Tenant => string.Format(CultureInfo.InvariantCulture, "[{0}].[dbo].[$ndo$tenantproperty]", databaseName), 
			_ => "[master].[dbo].[$ndo$srvproperty]", 
		};
	}

	public ServerResponse NewUserRole(string userName, string sid, string roleId, PermissionScope scope, string appName, string publisher, string companyName)
	{
		NavGuid userIdFromSidOrUserName = GetUserIdFromSidOrUserName(sid, userName);
		AppId val = AppId.None;
		if (!string.IsNullOrEmpty(appName))
		{
			NavAppMetadata metadataForDistinctAppInTenant = DatabaseNavAppSpecialQueries.GetMetadataForDistinctAppInTenant(appName, publisher);
			val = ((metadataForDistinctAppInTenant != null) ? metadataForDistinctAppInTenant.AppId : AppId.None);
			scope = PermissionScope.Tenant;
		}
		using (NavRecord navRecord = new NavRecord(session, 2000000167))
		{
			NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(1).FieldOptionMetadata;
			navRecord.ALGet(NavOption.Create(fieldOptionMetadata, (int)scope), new NavGuid(((WrappedValue<Guid>)(object)val).Value), new NavText(roleId));
		}
		if (!string.IsNullOrEmpty(companyName))
		{
			using NavRecord navRecord2 = new NavRecord(session, 2000000006);
			navRecord2.ALGet(new NavText(companyName));
		}
		using (NavRecord navRecord3 = new NavRecord(session, 2000000053))
		{
			NCLOptionMetadata fieldOptionMetadata2 = navRecord3.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
			navRecord3.ALInit();
			navRecord3.SetFieldValue(1, userIdFromSidOrUserName);
			navRecord3.SetFieldValue(2, new NavCode(roleId.Length, roleId));
			navRecord3.SetFieldValue(8, NavOption.Create(fieldOptionMetadata2, (int)scope));
			navRecord3.SetFieldValue(9, new NavGuid(((WrappedValue<Guid>)(object)val).Value));
			if (!string.IsNullOrEmpty(companyName))
			{
				navRecord3.SetFieldValue(3, new NavText(companyName.Length, companyName));
			}
			navRecord3.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse RemoveUserRole(string userName, string sid, string roleId, PermissionScope scope, string appName, string publisher, string companyName)
	{
		NavGuid userIdFromSidOrUserName = GetUserIdFromSidOrUserName(sid, userName);
		AppId val = AppId.None;
		if (!string.IsNullOrEmpty(appName))
		{
			NavAppMetadata metadataForDistinctAppInTenant = DatabaseNavAppSpecialQueries.GetMetadataForDistinctAppInTenant(appName, publisher);
			val = ((metadataForDistinctAppInTenant != null) ? metadataForDistinctAppInTenant.AppId : AppId.None);
			scope = PermissionScope.Tenant;
		}
		using (NavRecord navRecord = new NavRecord(session, 2000000053))
		{
			NCLOptionMetadata fieldOptionMetadata = navRecord.MetaTable.GetFieldByNo(8).FieldOptionMetadata;
			navRecord.ALInit();
			navRecord.ALSetFilter(1, "%1", userIdFromSidOrUserName);
			navRecord.ALSetFilter(2, "%1", new NavCode(roleId.Length, roleId));
			navRecord.ALSetFilter(3, "%1", new NavText(companyName.Length, companyName));
			navRecord.ALSetFilter(8, "%1", NavOption.Create(fieldOptionMetadata, (int)scope));
			navRecord.ALSetFilter(9, "%1", new NavGuid(((WrappedValue<Guid>)(object)val).Value));
			navRecord.ALFind("-");
			navRecord.ALDelete();
		}
		return new ServerResponse();
	}

	public ServerResponse NewRole(string roleId, string roleName)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000004))
		{
			navRecord.ALInit();
			navRecord.SetFieldValue(1, new NavText(roleId.Length, roleId));
			navRecord.SetFieldValue(2, new NavText(roleName.Length, roleName));
			navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse RemoveRole(string roleId)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000004))
		{
			navRecord.ALGet(new NavText(roleId));
			navRecord.ALDelete();
		}
		return new ServerResponse();
	}

	public ServerResponse RenameRole(string currentRoleId, string newRoleName)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000004))
		{
			navRecord.ALGet(new NavText(currentRoleId));
			if (!string.IsNullOrEmpty(newRoleName))
			{
				navRecord.SetFieldValue(2, new NavText(newRoleName.Length, newRoleName));
			}
			navRecord.ALModify();
		}
		return new ServerResponse();
	}

	public ServerResponse NewPermission(string roleId, ObjectType objectType, int objectId, int permissionMask, string securityFilter)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000004))
		{
			navRecord.ALGet(new NavText(roleId));
		}
		using (NavRecord navRecord2 = new NavRecord(session, 2000000038))
		{
			navRecord2.ALSetFilter(1, "%1", NavInteger.Create((int)objectType));
			if (objectId > 0)
			{
				navRecord2.ALSetFilter(3, "%1", NavInteger.Create(objectId));
			}
			navRecord2.ALFind("-");
		}
		using (NavRecord navRecord3 = new NavRecord(session, 2000000005))
		{
			navRecord3.ALInit();
			navRecord3.SetFieldValue(1, new NavText(roleId.Length, roleId));
			navRecord3.SetFieldValue(3, NavInteger.Create((int)objectType));
			navRecord3.SetFieldValue(4, NavInteger.Create(objectId));
			navRecord3.SetFieldValue(6, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Read | PermissionMask.IndirectRead)));
			navRecord3.SetFieldValue(7, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Insert | PermissionMask.IndirectInsert)));
			navRecord3.SetFieldValue(8, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Modify | PermissionMask.IndirectModify)));
			navRecord3.SetFieldValue(9, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Delete | PermissionMask.IndirectDelete)));
			navRecord3.SetFieldValue(10, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Execute | PermissionMask.IndirectExecute)));
			navRecord3.SetFieldValue(11, ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, navRecord3.GetNCLMetaTable().GetFieldByNo(11), securityFilter, 9));
			navRecord3.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse RemovePermission(string roleId, ObjectType objectType, int objectId)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000005))
		{
			navRecord.ALInit();
			navRecord.ALSetFilter(1, "%1", new NavText(roleId.Length, roleId));
			navRecord.ALSetFilter(3, "%1", NavInteger.Create((int)objectType));
			navRecord.ALSetFilter(4, "%1", NavInteger.Create(objectId));
			navRecord.ALFind("-");
			navRecord.ALDelete();
		}
		return new ServerResponse();
	}

	public ServerResponse SetPermission(string roleId, ObjectType objectType, int objectId, int permissionMask, string securityFilter, int permissionModifiedMask)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000005))
		{
			navRecord.ALInit();
			navRecord.ALSetFilter(1, "%1", new NavText(roleId.Length, roleId));
			navRecord.ALSetFilter(3, "%1", NavInteger.Create((int)objectType));
			navRecord.ALSetFilter(4, "%1", NavInteger.Create(objectId));
			navRecord.ALFind("-");
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.Read))
			{
				navRecord.SetFieldValue(6, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Read | PermissionMask.IndirectRead)));
			}
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.Insert))
			{
				navRecord.SetFieldValue(7, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Insert | PermissionMask.IndirectInsert)));
			}
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.Modify))
			{
				navRecord.SetFieldValue(8, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Modify | PermissionMask.IndirectModify)));
			}
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.Delete))
			{
				navRecord.SetFieldValue(9, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Delete | PermissionMask.IndirectDelete)));
			}
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.Execute))
			{
				navRecord.SetFieldValue(10, NavInteger.Create((int)PermissionHelper.GetPermissionOptionFromPermissionMask((PermissionMask)permissionMask, PermissionMask.Execute | PermissionMask.IndirectExecute)));
			}
			if (PermissionMaskModified((PermissionModifiedMask)permissionModifiedMask, PermissionModifiedMask.SecurityFilter))
			{
				navRecord.SetFieldValue(11, ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, navRecord.GetNCLMetaTable().GetFieldByNo(11), securityFilter, 9));
			}
			navRecord.ALModify(DataError.ThrowError, runApplicationTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse InvokeCodeunit(ConnectionRequest connectionRequest, int codeunitId, string methodName, string argument, string companyName, InvokeCodeunitTimeZone invokeTimeZone)
	{
		if (!string.IsNullOrEmpty(companyName))
		{
			session.OpenCompany(companyName);
		}
		else if (!string.IsNullOrEmpty(session.Tenant.TenantSettings.DefaultCompany))
		{
			session.OpenCompany(session.Tenant.TenantSettings.DefaultCompany);
		}
		TimeZoneInfo clientTimeZone = session.ClientTimeZone;
		switch (invokeTimeZone)
		{
		case InvokeCodeunitTimeZone.TenantTimeZone:
			session.ClientTimeZone = session.Tenant.TenantSettings.DefaultTimeZone ?? TimeZoneInfo.Utc;
			break;
		case InvokeCodeunitTimeZone.ServicesDefaultTimeZone:
			session.ClientTimeZone = ServerUserSettings.Instance.ServicesDefaultTimeZone.Value ?? TimeZoneInfo.Utc;
			break;
		case InvokeCodeunitTimeZone.ClientTimeZone:
			try
			{
				session.ClientTimeZone = TimeZoneInfo.FindSystemTimeZoneById(connectionRequest.TimeZoneId ?? TimeZoneInfo.Utc.Id);
			}
			catch (TimeZoneNotFoundException innerException)
			{
				throw new NavTimeZoneException(string.Format(CultureInfo.CurrentCulture, Lang.UnrecognizedTimeZone, connectionRequest.TimeZoneId), innerException);
			}
			catch (InvalidTimeZoneException innerException2)
			{
				throw new NavTimeZoneException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidTimeZone, connectionRequest.TimeZoneId), innerException2);
			}
			break;
		default:
			session.ClientTimeZone = TimeZoneInfo.Utc;
			break;
		}
		session.TransactionScope(() =>
		{
			if (string.IsNullOrEmpty(methodName))
			{
				NavCodeunit.InvokeRun(codeunitId, string.IsNullOrEmpty(argument) ? null : new object[1] { argument });
			}
			else
			{
				NavCodeunit.Invoke(codeunitId, methodName, string.IsNullOrEmpty(argument) ? null : new object[1] { argument });
			}
		});
		session.ClientTimeZone = clientTimeZone;
		return new ServerResponse();
	}

	private static bool PermissionMaskModified(PermissionModifiedMask permissionModifiedMask, PermissionModifiedMask basePermissionModifiedMask)
	{
		return (permissionModifiedMask & basePermissionModifiedMask) == basePermissionModifiedMask;
	}

	public NavGuid GetUserIdFromSidOrUserName(string sid, string userName)
	{
		NavUser user = null;
		UserValueFlags userValueFlag = GetUserValueFlag(sid, userName);
		if (userValueFlag.HasFlag(UserValueFlags.HasSid))
		{
			session.Database.NavUsers.TryGetNavUserFromSid(sid, out user);
		}
		else
		{
			if (!userValueFlag.HasFlag(UserValueFlags.HasUserName))
			{
				throw new NavNCLArgumentException("You can only use one of the following parameters: UserName, Sid or WindowsAccount.");
			}
			session.Database.NavUsers.TryGetNavUserFromUserName(userName, out user);
		}
		if (user == null)
		{
			throw new NavUserNotFoundException(string.Format(CultureInfo.CurrentCulture, Lang.UnableToFindUser));
		}
		return new NavGuid(user.Id);
	}

	public DataTable GetAddIn(string addInName, string publicKeyToken, string version, int category, bool includeResource)
	{
		DataTable dataTable = new DataTable("AddIn")
		{
			Locale = CultureInfo.CurrentCulture
		};
		dataTable.Columns.Add("AddInName");
		dataTable.Columns.Add("Description");
		dataTable.Columns.Add("PublicKeyToken");
		dataTable.Columns.Add("Version");
		dataTable.Columns.Add("Category");
		if (includeResource)
		{
			dataTable.Columns.Add("Resource");
		}
		using (NavRecord navRecord = new NavRecord(session, 2000000069))
		{
			if (addInName != null)
			{
				navRecord.ALSetFilter(1, "%1", new NavText(addInName));
			}
			if (publicKeyToken != null)
			{
				navRecord.ALSetFilter(5, "%1", new NavText(publicKeyToken));
			}
			if (version != null)
			{
				navRecord.ALSetFilter(8, "%1", new NavText(version));
			}
			if (Enum.IsDefined(typeof(AddInCategory), category))
			{
				navRecord.ALSetFilter(15, "%1", NavOption.Create(NCLOptionMetadata.Default, category));
			}
			if (navRecord.ALFindSet(DataError.ThrowError))
			{
				do
				{
					DataRow dataRow = dataTable.Rows.Add(navRecord.GetFieldValue(1).ToString(), navRecord.GetFieldValue(20).ToString(), navRecord.GetFieldValue(5).ToString(), navRecord.GetFieldValue(8).ToString(), navRecord.GetFieldValue(15).ToString());
					if (includeResource)
					{
						dataRow["Resource"] = navRecord.GetFieldValue(15).GetBytes();
					}
				}
				while (navRecord.ALNext() > 0);
			}
		}
		return dataTable;
	}

	public ServerResponse NewAddIn(string addInName, string publicKeyToken, string version, string description, AddInCategory category, byte[] resource)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000069))
		{
			navRecord.ALInit();
			navRecord.SetFieldValue(1, new NavText(addInName));
			navRecord.SetFieldValue(5, new NavText(publicKeyToken));
			navRecord.SetFieldValue(8, new NavText(version));
			navRecord.SetFieldValue(20, new NavText(description));
			navRecord.SetFieldValue(15, NavOption.Create(NCLOptionMetadata.Default, (int)category));
			if (resource != null)
			{
				navRecord.SetFieldValue(25, new NavBLOB(resource));
			}
			navRecord.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse SetAddIn(string addInName, string publicKeyToken, string version, string description, int category, byte[] resource)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000069))
		{
			navRecord.ALGet(DataError.ThrowError, new NavText(addInName), new NavText(publicKeyToken), new NavText(version));
			if (description != null)
			{
				navRecord.SetFieldValue(20, new NavText(description));
			}
			if (Enum.IsDefined(typeof(AddInCategory), category))
			{
				navRecord.SetFieldValue(15, NavOption.Create(NCLOptionMetadata.Default, category));
			}
			if (resource != null)
			{
				navRecord.SetFieldValue(25, new NavBLOB(resource));
			}
			navRecord.Modify(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse RemoveAddIn(string addInName, string publicKeyToken, string version)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000069))
		{
			navRecord.ALGet(DataError.ThrowError, new NavText(addInName), new NavText(publicKeyToken), new NavText(version));
			navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
		}
		return new ServerResponse();
	}

	public DataTable GetWebService()
	{
		DataTable dataTable = new DataTable("Web Service")
		{
			Locale = CultureInfo.CurrentCulture
		};
		dataTable.Columns.Add("ObjectId");
		dataTable.Columns.Add("ObjectType");
		dataTable.Columns.Add("ServiceName");
		dataTable.Columns.Add("Published");
		dataTable.Columns.Add("AllowOverride");
		using (NavRecord navRecord = new NavRecord(session, 2000000076))
		{
			if (navRecord.ALFindSet(DataError.TrapError))
			{
				do
				{
					dataTable.Rows.Add(navRecord.GetFieldValue(6).ToInt32(), navRecord.GetFieldValue(3).ToString(), navRecord.GetFieldValue(9).ToString(), navRecord.GetFieldValue(12).ToBoolean());
				}
				while (navRecord.ALNext() > 0);
			}
		}
		return dataTable;
	}

	public ServerResponse NewWebService(ObjectType objectType, int objectId, string name, bool published)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000038))
		{
			navRecord.ALSetFilter(1, "%1", NavInteger.Create((int)objectType));
			if (objectId > 0)
			{
				navRecord.ALSetFilter(3, "%1", NavInteger.Create(objectId));
			}
			navRecord.ALFind("-");
		}
		using (NavRecord navRecord2 = new NavRecord(session, 2000000076))
		{
			navRecord2.ALInit();
			navRecord2.SetFieldValue(3, NavInteger.Create((int)objectType));
			navRecord2.SetFieldValue(9, NavText.Create(name));
			navRecord2.SetFieldValue(6, NavInteger.Create(objectId));
			navRecord2.SetFieldValue(12, NavBoolean.Create(published));
			navRecord2.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
		}
		return new ServerResponse();
	}

	public ServerResponse RemoveWebService(ObjectType objectType, string serviceName)
	{
		using (NavRecord navRecord = new NavRecord(session, 2000000076))
		{
			navRecord.ALGet(DataError.ThrowError, NavInteger.Create((int)objectType), NavText.Create(serviceName));
			navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger: true);
		}
		return new ServerResponse();
	}

	public IList<TableConnectionSettingsBase> GetTableConnection(TableConnectionType tableType, string connectionId)
	{
		return TableConnectionManagement.GetTableConnection(tableType, connectionId);
	}

	public void AddTableConnection(TableConnectionSettingsBase connectionSettings)
	{
		TableConnectionManagement.AddTableConnection(connectionSettings);
	}

	public void RemoveTableConnection(TableConnectionType tableType, string connectionId)
	{
		TableConnectionManagement.RemoveTableConnection(tableType, connectionId);
	}

	public ServerResponse SandboxOutboundConfigCleanUp(ConnectionRequest connectionRequest)
	{
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		InvokeCodeunitTimeZone invokeTimeZone = InvokeCodeunitTimeZone.Utc;
		return InvokeCodeunit(connectionRequest, 1882, empty, empty2, empty3, invokeTimeZone);
	}

	private static UserValueFlags GetUserValueFlag(string sid, string userName)
	{
		bool flag = !string.IsNullOrEmpty(sid);
		bool flag2 = !string.IsNullOrEmpty(userName);
		return (UserValueFlags)((flag ? 1 : 0) | (flag2 ? 2 : 0) | ((flag ^ flag2) ? 4 : 0) | ((flag & flag2) ? 8 : 0));
	}

	public void RemoveAllClientConnections()
	{
		foreach (NavTenant item in NavEnvironment.Instance.Tenants.AllTenants())
		{
			item.StopAllSessionsExcept(session);
		}
	}
}
