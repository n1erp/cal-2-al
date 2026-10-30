using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal static class NavAppInstallationManagement
{
	internal static List<PackageId> GetAppsInstalledForTenant(NavDatabase applicationDatabase, string tenantId)
	{
		string text = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SG", Category.Extensions, Verbosity.Normal, "Selecting app records from application database -- Tenant:{0}", DiagnosticParameter.Oii(text));
		List<PackageId> list = new List<PackageId>();
		using NavSqlConnectionScope navSqlConnectionScope = CreateNavReadOnlySqlConnectionScope(applicationDatabase);
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand.SetCommandText("SELECT [App Package ID] FROM [NAV App Tenant App] WHERE [Tenant ID] = @0");
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, text);
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				list.Add(PackageId.op_Implicit(navSqlCommand.Reader.GetGuid(0)));
			}
			return list;
		}
	}

	internal static List<INavAppMetadataReference> GetAppsInstalledForTenantInTenantDatabase(NavDatabase tenantDatabase)
	{
		List<INavAppMetadataReference> list = new List<INavAppMetadataReference>();
		using NavSqlConnectionScope tenantConScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.Browse);
		list.AddRange(GetAppsInstalledForTenantInTenantDatabase(tenantConScope));
		return list;
	}

	internal static IEnumerable<AppSyncTrackingRecord> GetAppsSyncedForTenantInTenantDatabase(NavDatabase tenantDatabase)
	{
		using NavSqlConnectionScope conScope = NavSqlConnectionScope.CreateForceNewConnection(tenantDatabase, TransactionType.Browse);
		DatabaseAppSyncTrackingRepository databaseAppSyncTrackingRepository = new DatabaseAppSyncTrackingRepository(tenantDatabase, conScope);
		return databaseAppSyncTrackingRepository.GetAll();
	}

	internal static IEnumerable<UninstalledAppRecord> GetAppsUninstalledForTenantInTenantDatabase(NavDatabase tenantDatabase, string tenantId)
	{
		using NavSqlConnectionScope conScope = CreateNavSqlConnectionScope(tenantDatabase);
		DatabaseUninstalledAppRepository databaseUninstalledAppRepository = new DatabaseUninstalledAppRepository(tenantDatabase, conScope);
		return databaseUninstalledAppRepository.GetAllForTenant(tenantId);
	}

	private static int TenantDatabaseHasFields(NavSqlConnectionScope tenantConScope, string[] fieldList)
	{
		string arg = "N'" + string.Join("',N'", fieldList) + "'";
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantConScope.Connection);
		string commandText = string.Format(CultureInfo.InvariantCulture, "SELECT COUNT(*) FROM sys.columns WHERE Name IN ({1}) AND OBJECT_ID = OBJECT_ID('[{0}].[NAV App Installed App]', 'U')", "dbo", arg);
		navSqlCommand.SetCommandText(commandText);
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			navSqlCommand.ReadNextRow();
			int @int = navSqlCommand.Reader.GetInt32(0);
			navSqlCommand.CloseReader(forceClose: false);
			return @int;
		}
	}

	internal static List<NavAppRuntimeMetadata> GetAppsPublishedToApplicationDatabase(NavSqlConnectionScope conScope, string tenantId)
	{
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected Obj, but got Unknown
		List<NavAppRuntimeMetadata> list = new List<NavAppRuntimeMetadata>();
		Func<NavSqlCommand, int, NavExtensionType> func = (NavSqlCommand cmd, int ordinal) =>
		{
			using NavBLOB navBLOB = new NavBLOB();
			cmd.GetBlobDataFromReader(isCompressed: true, ordinal, navBLOB);
			using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(navBLOB.GetNewStream());
			return (navAppPackageReader.MetadataVersion == 1) ? NavExtensionType.CSide : NavExtensionType.ModernDev;
		};
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(conScope.Connection);
		string commandText = "SELECT [Package Hash], [Package ID], [ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build],\r\n                                      [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], \r\n                                      [Blob], [Content Hash], [Scope], [Tenant ID] FROM [NAV App]";
		navSqlCommand.SetCommandText(commandText);
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				string text = navSqlCommand.Reader.GetString(0);
				Sha256Hash sha256Hash = ((!string.IsNullOrEmpty(text)) ? Sha256Hash.CreateFromHexString(text) : null);
				PackageId val = PackageId.op_Implicit(navSqlCommand.Reader.GetGuid(1));
				AppId val2 = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(2));
				string text2 = navSqlCommand.Reader.GetString(3);
				string text3 = navSqlCommand.Reader.GetString(4);
				Version version = VersionHelper.Create(navSqlCommand.Reader.GetInt32(5), navSqlCommand.Reader.GetInt32(6), navSqlCommand.Reader.GetInt32(7), navSqlCommand.Reader.GetInt32(8));
				Version version2 = VersionHelper.Create(navSqlCommand.Reader.GetInt32(9), navSqlCommand.Reader.GetInt32(10), navSqlCommand.Reader.GetInt32(11), navSqlCommand.Reader.GetInt32(12));
				NavExtensionType navExtensionType = func(navSqlCommand, 13);
				NavAppRuntimeMetadata item = new NavAppRuntimeMetadata(val, val2, text2, text3, version, version2, navSqlCommand.Reader.GetString(14), navExtensionType, (NavAppScope)navSqlCommand.Reader.GetInt32(15), sha256Hash);
				string text4 = navSqlCommand.Reader.GetString(16);
				if (string.IsNullOrEmpty(text4) || string.Equals(text4, tenantId, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(item);
				}
			}
			return list;
		}
	}

	internal static List<INavAppMetadataReference> GetAppsInstalledForTenantInTenantDatabase(NavSqlConnectionScope tenantConScope)
	{
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Expected Obj, but got Unknown
		List<INavAppMetadataReference> list = new List<INavAppMetadataReference>();
		int num = TenantDatabaseHasFields(tenantConScope, new string[2] { "Content Hash", "Extension Type" });
		bool flag = num > 0;
		bool flag2 = num == 2;
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantConScope.Connection);
		string text = string.Format(CultureInfo.InvariantCulture, "IF (OBJECT_ID('[{0}].[NAV App Installed App]', 'U') IS NOT NULL)\r\n    SELECT \r\n        [Package ID], [App ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], \r\n        [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision]{1}{2}\r\n    FROM \r\n        [{0}].[NAV App Installed App]", "dbo", flag ? ", [Content Hash], [Hash Algorithm]" : string.Empty, flag2 ? ", [Extension Type]" : string.Empty);
		if (tenantConScope.Connection.Database.IsSharedTenantSchema && !tenantConScope.Connection.Database.Tenant.IsSystemTenant)
		{
			CompanyId? companyIdForSystemTables = tenantConScope.Connection.Database.GetCompanyIdForSystemTables(2000000153);
			if (!companyIdForSystemTables.HasValue)
			{
				return new List<INavAppMetadataReference>(0);
			}
			text = text + " WHERE " + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(0);
			NavSqlStatementHelper.SetSqlParameter(NavSqlStatementHelper.GetOrCreateSqlParameterNo(navSqlCommand.Command.Parameters, 0), companyIdForSystemTables.Value);
		}
		navSqlCommand.SetCommandText(text);
		navSqlCommand.PrepareStatement();
		using (navSqlCommand.ExecuteReader())
		{
			while (navSqlCommand.ReadNextRow())
			{
				NavAppMetadata item = new NavAppMetadata
				{
					PackageId = PackageId.op_Implicit(navSqlCommand.Reader.GetGuid(0)),
					AppId = AppId.op_Implicit(navSqlCommand.Reader.GetGuid(1)),
					Name = navSqlCommand.Reader.GetString(2),
					Publisher = navSqlCommand.Reader.GetString(3),
					VersionMajor = navSqlCommand.Reader.GetInt32(4),
					VersionMinor = navSqlCommand.Reader.GetInt32(5),
					VersionBuild = navSqlCommand.Reader.GetInt32(6),
					VersionRevision = navSqlCommand.Reader.GetInt32(7),
					CompatibilityMajor = navSqlCommand.Reader.GetInt32(8),
					CompatibilityMinor = navSqlCommand.Reader.GetInt32(9),
					CompatibilityBuild = navSqlCommand.Reader.GetInt32(10),
					CompatibilityRevision = navSqlCommand.Reader.GetInt32(11),
					ContentHash = (flag ? navSqlCommand.Reader.GetString(12) : string.Empty),
					HashAlgorithm = (NavAppHashAlgorithm)(flag ? navSqlCommand.Reader.GetInt32(13) : 0),
					ExtensionType = (flag2 ? NavAppTypeExtensions.ToMetadataType((NavAppExtensionType)navSqlCommand.Reader.GetInt32(14)) : NavExtensionType.None)
				};
				list.Add((INavAppMetadataReference)(object)item);
			}
			return list;
		}
	}

	internal static void UpdatePackageIdAndContentHashInForAppInTenantDatabase(INavAppMetadataReference appFromAppDb, NavSqlConnectionScope tenantConScope)
	{
		using NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantConScope.Connection);
		string text = string.Format(CultureInfo.InvariantCulture, "UPDATE [{0}].[NAV App Installed App] SET [Content Hash] = @0, [Hash Algorithm] = @1, [Package ID] = @2  WHERE [App ID] = @3", "dbo");
		CompanyId? companyId = null;
		if (tenantConScope.Connection.Database.IsSharedTenantSchema && !tenantConScope.Connection.Database.Tenant.IsSystemTenant)
		{
			companyId = tenantConScope.Connection.Database.GetCompanyIdForSystemTables(2000000153);
			if (!companyId.HasValue)
			{
				return;
			}
			text = text + " AND " + NavSqlStatementHelper.SqlStatementCompanyIdEqualsToParameterNo(4);
		}
		navSqlCommand.SetCommandText(text);
		navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 250, appFromAppDb.ContentHash);
		navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, (object)(NavAppHashAlgorithm)1);
		navSqlCommand.Command.Parameters.Add(2, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appFromAppDb.PackageId).Value);
		navSqlCommand.Command.Parameters.Add(3, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)((INavAppReference)appFromAppDb).AppId).Value);
		if (companyId.HasValue)
		{
			navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, companyId.Value.Value);
		}
		navSqlCommand.PrepareStatement();
		navSqlCommand.ExecuteNonQuery();
	}

	internal static void PurgeAndInsertNewTenantAppInstallationRecords(NavDatabase applicationDatabase, List<PackageId> tenantApps, string tenantId)
	{
		using NavSqlConnectionScope conScope = CreateNavSqlConnectionScope(applicationDatabase);
		PurgeAndInsertNewTenantAppInstallationRecordsInternal(applicationDatabase, conScope, tenantApps, tenantId);
	}

	internal static void PurgeAndInsertNewTenantAppInstallationRecordsInternal(NavDatabase applicationDatabase, NavSqlConnectionScope conScope, List<PackageId> tenantApps, string tenantId)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SH", Category.Extensions, Verbosity.Normal, "Purging and inserting tenant apps into application database -- Tenant:{0}", DiagnosticParameter.Oii(tenantIdWithConsistentCasing));
		conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("PurgeAndInsertNewTenantAppInstallationRecords_Purge", applicationDatabase), () => "DELETE FROM [NAV App Tenant App] WHERE [Tenant ID] = @0", (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters.Add("@0", SqlDbType.NVarChar, 128);
		}, (NavSqlCommand cmd) =>
		{
			cmd.Command.Parameters[0].Value = tenantIdWithConsistentCasing;
		}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		foreach (PackageId tenantAppPackageId in tenantApps)
		{
			conScope.Connection.ExecuteCommandWithParameters(new NavSqlStringBasedCommandCacheKey("PurgeAndInsertNewTenantAppInstallationRecords_Insert", applicationDatabase), () => "INSERT INTO [NAV App Tenant App] ([Tenant ID], [App Package ID]) VALUES (@0, @1)", (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters.Add("@0", SqlDbType.NVarChar, 128);
				cmd.Command.Parameters.Add("@1", SqlDbType.UniqueIdentifier);
			}, (NavSqlCommand cmd) =>
			{
				cmd.Command.Parameters[0].Value = tenantIdWithConsistentCasing;
				cmd.Command.Parameters[1].Value = ((WrappedValue<Guid>)(object)tenantAppPackageId).Value;
			}, (NavSqlCommand cmd) => cmd.ExecuteNonQuery());
		}
	}

	internal static void UpdateAppInApplicationDatabaseWithRetries(NavDatabase applicationDatabase, string tenantId, INavAppMetadataReference newMetadata, INavAppMetadataReference oldMetadata)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000014I", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Updating app record in application database -- Tenant:{0}; New PackageId:{1}; Old PackageId:{2}", tenantIdWithConsistentCasing, ((WrappedValue<Guid>)(object)newMetadata.PackageId).Value, ((WrappedValue<Guid>)(object)oldMetadata.PackageId).Value);
		PerformExtensionOperationWithRetries(upgradeOperation);
		void upgradeOperation()
		{
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(applicationDatabase, TransactionType.Update);
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
			{
				navSqlCommand.SetCommandText("UPDATE [NAV App Tenant App] SET [App Package ID] = @2 WHERE [Tenant ID] = @0 AND [App Package ID] = @1");
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, tenantIdWithConsistentCasing);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)oldMetadata.PackageId).Value);
				navSqlCommand.Command.Parameters.Add(2, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)newMetadata.PackageId).Value);
				navSqlCommand.PrepareStatement();
				navSqlCommand.ExecuteNonQuery();
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}
	}

	internal static void InstallAppToApplicationDatabaseWithRetries(NavDatabase applicationDatabase, string tenantId, INavAppMetadataReference metadata)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SI", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Inserting new app record into application database -- Tenant:{0}; PackageId:{1}", tenantIdWithConsistentCasing, ((WrappedValue<Guid>)(object)metadata.PackageId).Value);
		PerformExtensionOperationWithRetries(installOperation);
		void installOperation()
		{
			using NavSqlConnectionScope navSqlConnectionScope = CreateNavSqlConnectionScope(applicationDatabase);
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
			{
				navSqlCommand.SetCommandText("INSERT INTO [NAV App Tenant App] ([Tenant ID], [App Package ID]) VALUES (@0, @1)");
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, tenantIdWithConsistentCasing);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)metadata.PackageId).Value);
				navSqlCommand.PrepareStatement();
				navSqlCommand.ExecuteNonQuery();
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}
	}

	private static void PerformExtensionOperationWithRetries(Action operation)
	{
		int num = 9;
		while (num > 0)
		{
			try
			{
				operation();
				num = 0;
			}
			catch (NavCSideException ex)
			{
				string content = string.Format(CultureInfo.InvariantCulture, "{0} failed -- {1}", operation.Method.Name, ex.ToString());
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001O0", Category.Extensions, ex, Verbosity.Error, DiagnosticParameter.SystemMetadata(content));
				num--;
				if (num == 0)
				{
					throw;
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001O2", Category.Extensions, Verbosity.Warning, PrivacyClassification.SystemMetadata, string.Format(CultureInfo.InvariantCulture, "Retry :: Extension operation {0}", operation.Method.Name));
			}
		}
	}

	internal static void AddInstalledAppRecord(NavTenant tenant, NavSqlConnectionScope tenantDbScope, INavAppMetadataReference runtimeMetadata)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantDbScope.Connection))
		{
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "INSERT INTO [NAV App Installed App] ([Package ID], [App ID], [Name], [Publisher], [Version Major], [Version Minor], [Version Build], [Version Revision], [Compatibility Major], [Compatibility Minor], [Compatibility Build], [Compatibility Revision], [Content Hash], [Hash Algorithm], [Extension Type]{0}) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9, @10, @11, @12, @13, @14{1})", tenant.Database.IsSharedTenantSchema ? ", \"$companyId\"" : string.Empty, tenant.Database.IsSharedTenantSchema ? ", @15" : string.Empty));
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)runtimeMetadata.PackageId).Value);
			navSqlCommand.Command.Parameters.Add(1, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)((INavAppReference)runtimeMetadata).AppId).Value);
			navSqlCommand.Command.Parameters.Add(2, SqlDbType.NVarChar, 500, ((INavAppReference)runtimeMetadata).Name);
			navSqlCommand.Command.Parameters.Add(3, SqlDbType.NVarChar, 500, ((INavAppReference)runtimeMetadata).Publisher);
			navSqlCommand.Command.Parameters.Add(4, SqlDbType.Int, ((INavAppReference)runtimeMetadata).Version.Major);
			navSqlCommand.Command.Parameters.Add(5, SqlDbType.Int, ((INavAppReference)runtimeMetadata).Version.Minor);
			navSqlCommand.Command.Parameters.Add(6, SqlDbType.Int, ((INavAppReference)runtimeMetadata).Version.Build);
			navSqlCommand.Command.Parameters.Add(7, SqlDbType.Int, ((INavAppReference)runtimeMetadata).Version.Revision);
			navSqlCommand.Command.Parameters.Add(8, SqlDbType.Int, runtimeMetadata.CompatibilityId.Major);
			navSqlCommand.Command.Parameters.Add(9, SqlDbType.Int, runtimeMetadata.CompatibilityId.Minor);
			navSqlCommand.Command.Parameters.Add(10, SqlDbType.Int, runtimeMetadata.CompatibilityId.Build);
			navSqlCommand.Command.Parameters.Add(11, SqlDbType.Int, runtimeMetadata.CompatibilityId.Revision);
			navSqlCommand.Command.Parameters.Add(12, SqlDbType.NVarChar, 250, runtimeMetadata.ContentHash);
			navSqlCommand.Command.Parameters.Add(13, SqlDbType.Int, 1);
			navSqlCommand.Command.Parameters.Add(14, SqlDbType.Int, NavAppTypeExtensions.ToServerResponseType(runtimeMetadata.ExtensionType));
			if (tenant.Database.IsSharedTenantSchema)
			{
				navSqlCommand.Command.Parameters.Add(15, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000153).Value);
			}
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074V", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Adding installed app record -- Tenant:{0}; PackageId:{1}; AppId:{2}; Elapsed:{3}", tenant.Id, ((WrappedValue<Guid>)(object)runtimeMetadata.PackageId).Value, ((WrappedValue<Guid>)(object)((INavAppReference)runtimeMetadata).AppId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	internal static void PurgeInstalledAndPublishedTenantAppsFromApplicationDatabase(NavDatabase applicationDatabase, string tenantId)
	{
		string text = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SJ", Category.Extensions, Verbosity.Normal, "Purging tenant apps from application database -- Tenant:{0}", DiagnosticParameter.Oii(text));
		using NavSqlConnectionScope navSqlConnectionScope = CreateNavSqlConnectionScope(applicationDatabase);
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
		{
			navSqlCommand.SetCommandText("DELETE FROM [NAV App Tenant App] WHERE [Tenant ID] = @0");
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, text);
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		}
		using NavSqlCommand navSqlCommand2 = NavSqlCommand.Create(navSqlConnectionScope.Connection);
		navSqlCommand2.SetCommandText("DELETE FROM [NAV App Publish Reference] WHERE [Tenant ID] = @0");
		navSqlCommand2.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, text);
		navSqlCommand2.PrepareStatement();
		navSqlCommand2.ExecuteNonQuery();
	}

	internal static void UninstallAppFromApplicationDatabaseWithRetries(NavDatabase applicationDatabase, string tenantId, PackageId packageId)
	{
		string tenantIdWithConsistentCasing = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000SK", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Deleting app record from application database -- Tenant:{0}; PackageId:{1}", tenantIdWithConsistentCasing, ((WrappedValue<Guid>)(object)packageId).Value);
		PerformExtensionOperationWithRetries(uninstallOperation);
		void uninstallOperation()
		{
			using NavSqlConnectionScope navSqlConnectionScope = CreateNavSqlConnectionScope(applicationDatabase);
			using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(navSqlConnectionScope.Connection))
			{
				navSqlCommand.SetCommandText("DELETE FROM [NAV App Tenant App] WHERE [Tenant ID] = @0 AND [App Package ID] = @1");
				navSqlCommand.Command.Parameters.Add(0, SqlDbType.NVarChar, 128, tenantIdWithConsistentCasing);
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)packageId).Value);
				navSqlCommand.PrepareStatement();
				navSqlCommand.ExecuteNonQuery();
			}
			navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
		}
	}

	internal static void RemoveInstalledAppRecord(NavTenant tenant, NavSqlConnectionScope tenantDatabaseScope, AppId appId)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		using (NavSqlCommand navSqlCommand = NavSqlCommand.Create(tenantDatabaseScope.Connection))
		{
			navSqlCommand.SetCommandText(string.Format(CultureInfo.InvariantCulture, "DELETE FROM [NAV App Installed App] WHERE {0} [App ID] = @0", tenant.Database.IsSharedTenantSchema ? "\"$companyId\" = @1 AND " : string.Empty));
			navSqlCommand.Command.Parameters.Add(0, SqlDbType.UniqueIdentifier, ((WrappedValue<Guid>)(object)appId).Value);
			if (tenant.Database.IsSharedTenantSchema)
			{
				navSqlCommand.Command.Parameters.Add(1, SqlDbType.Int, tenant.Database.GetCompanyIdForSystemTables(2000000153).Value);
			}
			navSqlCommand.PrepareStatement();
			navSqlCommand.ExecuteNonQuery();
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074W", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Removing installed app record -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)appId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static NavSqlConnectionScope CreateNavSqlConnectionScope(NavDatabase database)
	{
		return NavSqlConnectionScope.CreateForceNewConnection(database, TransactionType.Update);
	}

	private static NavSqlConnectionScope CreateNavReadOnlySqlConnectionScope(NavDatabase database)
	{
		return NavSqlConnectionScope.Create(database, TransactionType.Browse);
	}

	internal static void UpdateCompanionTablesForApp(NavAppRuntimeMetadata runtimeMetadata, NavAppGroup targetGroup, NavTenant tenant)
	{
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074X", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Ensuring companion table record integrity -- Tenant:{0}; AppId:{1}", tenant.Id, ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value);
		Stopwatch stopwatch = Stopwatch.StartNew();
		int[] companyTokens = tenant.Database.GetCompanyTokens();
		foreach (NavAppObjectMetadataSummary item in runtimeMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) => s.ObjectType == ObjectType.TableExtension))
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(item.TargetObjectId, targetGroup.GroupId, requireCompiled: false);
			if (!metaTableById.DistinctCompanionTableOrigins.Any((NavAppOrigin o) =>
			{
				Guid? appId = o.AppId;
				Guid value = ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value;
				if (!appId.HasValue)
				{
					return false;
				}
				return !appId.HasValue || appId.GetValueOrDefault() == value;
			}))
			{
				continue;
			}
			NavAppTableContext companionTableContext = NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value));
			if (!metaTableById.DataPerCompany)
			{
				NavSqlDdlCommands.UpdateCompanionTableRecordsForExtensionInstall(metaTableById, tenant.Database, 0, companionTableContext);
				continue;
			}
			int[] array = companyTokens;
			foreach (int companyIndex in array)
			{
				NavSqlDdlCommands.UpdateCompanionTableRecordsForExtensionInstall(metaTableById, tenant.Database, companyIndex, companionTableContext);
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074Y", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Ensured companion table record integrity -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	public static void PurgeAppTablesForTenant(NavAppRuntimeMetadata runtimeMetadata, NavAppGroup targetGroup, NavTenant tenant)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		PurgeAppCompanionTablesForTenant(runtimeMetadata, targetGroup, tenant);
		PurgeAppNewTablesForTenant(runtimeMetadata, targetGroup, tenant);
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074Z", Category.Extensions, Verbosity.Normal, PrivacyClassification.OrganizationIdentifiableInformation, "Purging app tables -- Tenant:{0}; AppId:{1}; Elapsed:{2}", tenant.Id, ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value, stopwatch.Elapsed.ToStringWithMilliseconds());
	}

	private static void PurgeAppCompanionTablesForTenant(NavAppRuntimeMetadata runtimeMetadata, NavAppGroup targetGroup, NavTenant tenant)
	{
		foreach (NavAppObjectMetadataSummary item in runtimeMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) => s.ObjectType == ObjectType.TableExtension))
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(item.TargetObjectId, targetGroup.GroupId);
			if (!metaTableById.DistinctCompanionTableOrigins.Any((NavAppOrigin o) =>
			{
				Guid? appId = o.AppId;
				Guid value = ((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value;
				if (!appId.HasValue)
				{
					return false;
				}
				return !appId.HasValue || appId.GetValueOrDefault() == value;
			}))
			{
				continue;
			}
			NavAppTableContext tableContext = NavAppTableContext.Companion(NavAppOrigin.CreateForApp(((WrappedValue<Guid>)(object)runtimeMetadata.AppId).Value));
			if (!metaTableById.DataPerCompany)
			{
				NavSqlDdlCommands.PurgeNavAppDataFromCompanionTable(metaTableById, tenant.Database, 0, tableContext);
				continue;
			}
			int[] companyTokens = tenant.Database.GetCompanyTokens();
			foreach (int companyIndex in companyTokens)
			{
				NavSqlDdlCommands.PurgeNavAppDataFromCompanionTable(metaTableById, tenant.Database, companyIndex, tableContext);
			}
		}
	}

	private static void PurgeAppNewTablesForTenant(NavAppRuntimeMetadata runtimeMetadata, NavAppGroup targetGroup, NavTenant tenant)
	{
		foreach (NavAppObjectMetadataSummary item in runtimeMetadata.ObjectMetadataSummaries.Where((NavAppObjectMetadataSummary s) => s.ObjectType == ObjectType.Table))
		{
			NCLMetaTable metaTableById = NavGlobal.NCLMetadata.GetMetaTableById(item.TargetObjectId, targetGroup.GroupId);
			if (metaTableById.TableType != TableType.Normal)
			{
				continue;
			}
			if (!metaTableById.DataPerCompany)
			{
				NavSqlDdlCommands.PurgeNavAppDataFromCompanionTable(metaTableById, tenant.Database, 0, NavAppTableContext.Main);
				continue;
			}
			int[] companyTokens = tenant.Database.GetCompanyTokens();
			foreach (int companyIndex in companyTokens)
			{
				NavSqlDdlCommands.PurgeNavAppDataFromCompanionTable(metaTableById, tenant.Database, companyIndex, NavAppTableContext.Main);
			}
		}
	}
}
