using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppUnpublisher
{
	public static Result Unpublish(Guid? incomingPackageId, string name, string publisher, Version version, string tenantId = null)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Expected Obj, but got Unknown
		string content = string.Format(CultureInfo.InvariantCulture, "Unpublish Extension Request -- PackageId:{0}; Name:{1}; Publisher:{2}; Version:{3}; Tenant:{4}", incomingPackageId?.ToString() ?? string.Empty, name, publisher, version, tenantId);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001YP", DiagnosticParameter.Oii(content), (MonitorType)196609, Category.Extensions))
		{
			try
			{
				tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.Update);
				Result val = null;
				try
				{
					Result<PackageId> val2 = (incomingPackageId.HasValue ? Result.Ok<PackageId>(new PackageId(incomingPackageId.Value)) : ResolvePackageId(navSqlConnectionScope, name, publisher, version, tenantId));
					if (((Result)val2).Failure)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YZ", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(((Result)val2).Error));
						return (Result)(object)val2;
					}
					PackageId value = val2.Value;
					using NavSqlDatabaseLock theLock = NavAppPublishHelper.CreatePackageIdLock(navSqlConnectionScope, value);
					Result val3 = NavAppPublishHelper.EnsureLockWasAcquired(theLock, value);
					if (val3.Failure)
					{
						return val3;
					}
					val = UnpublishInternal(navSqlConnectionScope, value, tenantId);
					if (val.Failure)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001Z0", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(val.Error));
					}
					return val;
				}
				catch
				{
					navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
					throw;
				}
				finally
				{
					if (val != null && val.Failure)
					{
						navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
					}
				}
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001YS", Category.Extensions, exception);
				throw;
			}
		}
	}

	private static Result UnpublishInternal(NavSqlConnectionScope appDatabaseScope, PackageId packageId, string tenantId)
	{
		Result<NavAppMetadata> publishedAppMetadata = GetPublishedAppMetadata(appDatabaseScope, packageId);
		if (((Result)publishedAppMetadata).Failure)
		{
			return (Result)(object)publishedAppMetadata;
		}
		NavAppMetadata value = publishedAppMetadata.Value;
		if (value.Scope == NavAppScope.Tenant)
		{
			if (tenantId == null)
			{
				return UnpublishFromSystem(appDatabaseScope, packageId);
			}
			NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId);
			using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantById.Database, TransactionType.Update);
			Result val = null;
			try
			{
				val = (string.IsNullOrEmpty(value.TenantId) ? UnpublishFromTenant(appDatabaseScope, navSqlConnectionScope, packageId, tenantById) : UnpublishFromTenantAndSystem(appDatabaseScope, navSqlConnectionScope, packageId, tenantById));
				return val;
			}
			catch
			{
				navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				throw;
			}
			finally
			{
				if (val != null && val.Failure)
				{
					navSqlConnectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
				}
			}
		}
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess((tenantId != null) ? Result.Fail(Error.FromMessage(Exceptions.Unpublish_SpecifyingTenantNotValid, ErrorType.InvalidOperation)) : Result.Ok(), (Func<Result>)(() => VerifyAppNotInstalledForAnyTenant(appDatabaseScope, packageId))), (Func<Result>)(() => VerifyNoUnresolvableDependants(appDatabaseScope, packageId))), (Func<Result>)(() => PurgeAppMetadataAndObjectsAndMedia(appDatabaseScope, packageId))), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
		}));
	}

	private static Result UnpublishFromSystem(NavSqlConnectionScope appDatabaseScope, PackageId packageId)
	{
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(VerifyAppNotInstalledForAnyTenant(appDatabaseScope, packageId), (Func<Result>)(() => VerifyNoUnresolvableDependants(appDatabaseScope, packageId))), (Func<Result>)(() => PurgeAppMetadataAndObjectsAndMedia(appDatabaseScope, packageId))), (Action)(() =>
		{
			new AppPublishReferenceStorage(NavGlobal.AppDatabase, appDatabaseScope).DeletePublishReferencesForApp(packageId);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange();
		}));
	}

	private static Result UnpublishFromTenant(NavSqlConnectionScope appDatabaseScope, NavSqlConnectionScope tenantDatabaseScope, PackageId packageId, NavTenant tenant)
	{
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(VerifyAppNotInstalledForATenant(appDatabaseScope, packageId, tenant.Id), (Action)(() =>
		{
			new AppPublishedAppStorage(tenant.Database, tenantDatabaseScope).Delete(packageId);
		})), (Action)(() =>
		{
			new AppPublishReferenceStorage(NavGlobal.AppDatabase, appDatabaseScope).DeletePublishReference(tenant.Id, packageId);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange(tenant.Id);
		}));
	}

	private static Result UnpublishFromTenantAndSystem(NavSqlConnectionScope appDatabaseScope, NavSqlConnectionScope tenantDatabaseScope, PackageId packageId, NavTenant tenant)
	{
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(VerifyAppNotInstalledForAnyTenant(appDatabaseScope, packageId), (Func<Result>)(() => VerifyNoUnresolvableDependants(appDatabaseScope, packageId))), (Action)(() =>
		{
			new AppPublishedAppStorage(tenant.Database, tenantDatabaseScope).Delete(packageId);
		})), (Action)(() =>
		{
			new AppPublishReferenceStorage(NavGlobal.AppDatabase, appDatabaseScope).DeletePublishReference(tenant.Id, packageId);
		})), (Func<Result>)(() => PurgeAppMetadataAndObjectsAndMedia(appDatabaseScope, packageId))), (Action)(() =>
		{
			new AppPublishReferenceStorage(NavGlobal.AppDatabase, appDatabaseScope).DeletePublishReferencesForApp(packageId);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange(tenant.Id);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
		})), (Action)(() =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange();
		}));
	}

	private static Result<PackageId> ResolvePackageId(NavSqlConnectionScope connectionScope, string name, string publisher, Version version, string tenantId)
	{
		return new AppMetadataLoader(connectionScope).ResolvePackageId(name, publisher, version, tenantId);
	}

	private static Result<NavAppMetadata> GetPublishedAppMetadata(NavSqlConnectionScope connectionScope, PackageId packageId)
	{
		return ResultExtensions.OnFailure<Result<NavAppMetadata>>(new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAppMetadata(packageId), (Action)(() =>
		{
			Result.Fail(Error.FromMessage("The Extension cannot be unpublished because it is not published on this server.", ErrorType.InvalidOperation));
		}));
	}

	private static Result VerifyAppNotInstalledForAnyTenant(NavSqlConnectionScope connectionScope, PackageId packageId)
	{
		return ResultExtensions.OnSuccess<List<string>>(new AppTenantAppStorage(NavGlobal.AppDatabase, connectionScope).GetTenantsWithAppInstalled(packageId), (Func<List<string>, Result>)((List<string> tenants) => (!tenants.Any()) ? Result.Ok() : Result.Fail(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "The Extension cannot be unpublished because it is in use by the following tenants: {0}", string.Join(", ", tenants)), ErrorType.InvalidOperation))));
	}

	private static Result VerifyAppNotInstalledForATenant(NavSqlConnectionScope connectionScope, PackageId packageId, string tenantId)
	{
		return ResultExtensions.OnSuccess<List<PackageId>>(new AppTenantAppStorage(NavGlobal.AppDatabase, connectionScope).GetAppsInstalledForTenant(tenantId), (Func<List<PackageId>, Result>)((List<PackageId> packageIds) => (!packageIds.Any((PackageId p) => (WrappedComparable<Guid>)(object)p == (WrappedComparable<Guid>)(object)packageId)) ? Result.Ok() : Result.Fail(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "The Extension cannot be unpublished because it is in use by the following tenants: {0}", tenantId), ErrorType.InvalidOperation))));
	}

	private static Result VerifyNoUnresolvableDependants(NavSqlConnectionScope connectionScope, PackageId packageId)
	{
		Result<IEnumerable<NavAppMetadata>> allMetadataRecords = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAllMetadataRecords();
		Result<List<AppDependencyRecord>> allDependencyRecords = new AppDependenciesStorage(NavGlobal.AppDatabase, connectionScope).GetAllDependencyRecords();
		if (((Result)allMetadataRecords).Failure)
		{
			return (Result)(object)allMetadataRecords;
		}
		if (((Result)allDependencyRecords).Failure)
		{
			return (Result)(object)allDependencyRecords;
		}
		IEnumerable<NavAppMetadata> metadataRecords = allMetadataRecords.Value;
		List<AppDependencyRecord> value = allDependencyRecords.Value;
		NavAppMetadata appMetadata = metadataRecords.First((NavAppMetadata x) => (WrappedComparable<Guid>)(object)x.PackageId == (WrappedComparable<Guid>)(object)packageId);
		List<AppDependencyRecord> source = value.Where((AppDependencyRecord x) => (WrappedComparable<Guid>)(object)x.Id == (WrappedComparable<Guid>)(object)appMetadata.AppId && x.CompatibilityId == appMetadata.CompatibilityId && x.MinVersion <= appMetadata.Version).ToList();
		if (!source.Any())
		{
			return Result.Ok();
		}
		List<NavAppMetadata> alternateVersions = metadataRecords.Where((NavAppMetadata x) => (WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)appMetadata.AppId && x.CompatibilityMajor == appMetadata.CompatibilityMajor && x.CompatibilityMinor == appMetadata.CompatibilityMinor && x.CompatibilityBuild == appMetadata.CompatibilityBuild && x.CompatibilityRevision == appMetadata.CompatibilityRevision && (WrappedComparable<Guid>)(object)x.PackageId != (WrappedComparable<Guid>)(object)appMetadata.PackageId).ToList();
		List<AppDependencyRecord> source2 = source.Where((AppDependencyRecord dependent) => !alternateVersions.Any((NavAppMetadata alternate) => dependent.MinVersion <= alternate.Version)).ToList();
		if (!source2.Any())
		{
			return Result.Ok();
		}
		return Result.Fail(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, "The Extension cannot be unpublished because it is required by the following apps: {0}", string.Join(", ", source2.Select((AppDependencyRecord x) => metadataRecords.First((NavAppMetadata y) => (WrappedComparable<Guid>)(object)y.PackageId == (WrappedComparable<Guid>)(object)x.PackageId).Name))), ErrorType.InvalidOperation));
	}

	private static Result PurgeAppMetadataAndObjectsAndMedia(NavSqlConnectionScope connectionScope, PackageId packageId)
	{
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess<NavAppMetadata>(new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAppMetadata(packageId), (Func<NavAppMetadata, Result>)((NavAppMetadata metadata) => new AppMediaLoader(connectionScope).DeleteAppMedia(metadata.Logo, new MediaSetId[1] { metadata.Screenshots }))), (Func<Result>)(() => new AppMetadataLoader(connectionScope).DeleteAppMetadata(packageId))), (Action)(() =>
		{
			new NavAppObjectStorage(NavGlobal.AppDatabase, connectionScope).DeleteAppRecords(packageId);
		})), (Func<Result>)(() => new AppTenantAppStorage(NavGlobal.AppDatabase, connectionScope).PurgeRecordsForPackage(packageId)));
	}
}
