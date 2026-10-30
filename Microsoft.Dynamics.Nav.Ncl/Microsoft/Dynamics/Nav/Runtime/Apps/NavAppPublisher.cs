using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Utility;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppPublisher
{
	private delegate Result DoSystemCompileStep(NavSqlConnectionScope appDatabaseScope, Stream packageStream, NavAppPackageType packageType, bool skipLicenseValidation, NavAppScope scope, bool isDeveloperExtension = false, string tenantId = null, ObjectChangeModelDefinition radChanges = null);

	private static readonly List<NavAppCapability> NoCapabilities = new List<NavAppCapability>();

	private static readonly List<NavAppObjectPrerequisite> NoPrerequisites = new List<NavAppObjectPrerequisite>();

	public static Result PublishFromArguments(PublishNavAppArguments arguments)
	{
		using Stream packageStream = arguments.TakeStreamOwnership();
		return Publish(packageStream, arguments.Scope, arguments.PackageType, arguments.SkipSigningVerification, arguments.TenantId, skipLicenseValidation: false);
	}

	public static Result Publish(Stream packageStream, NavAppScope scope, NavAppPackageType packageType, bool skipSigningVerification, string tenantId, bool skipLicenseValidation, bool isDeveloperExtension = false, bool isRad = false)
	{
		tenantId = NavTenant.ConvertTenantIdToExpectedCase(tenantId);
		ObjectChangeModelDefinition radChanges = null;
		Guid packageId;
		NavAppManifest navAppManifest;
		bool flag;
		try
		{
			using NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(packageStream, leaveOpen: true);
			packageId = navAppPackageReader.PackageId;
			navAppManifest = navAppPackageReader.ReadNavAppManifest();
			flag = IsRuntimePackage(navAppPackageReader);
			if (isRad)
			{
				radChanges = navAppPackageReader.GetRadChanges();
			}
		}
		finally
		{
			packageStream.Seek(0L, SeekOrigin.Begin);
		}
		DoSystemCompileStep systemCompileAction = (flag ? new DoSystemCompileStep(DoSystemCompileStepForRuntimePackage) : new DoSystemCompileStep(DoSystemCompileStepForRegularPackage));
		string content = string.Format(CultureInfo.InvariantCulture, "Publish Extension Request -- PackageId:{0}; AppId:{1}; Name:{2}; Publisher:{3}; Version:{4}; Scope:{5}; Type:{6}; Platform:{7}; Application:{8}; RuntimePackage:{9}; SkipSigning:{10}; SkipLicense:{11}; TenantId:{12}; IsDeveloperExtension {13}; IsRad {14}", packageId, navAppManifest.AppId, navAppManifest.AppName, navAppManifest.AppPublisher, navAppManifest.AppVersion, scope, packageType, navAppManifest.Platform, navAppManifest.Application, flag, skipSigningVerification, skipLicenseValidation, tenantId, isDeveloperExtension, isRad);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001YN", DiagnosticParameter.SystemMetadata(content), (MonitorType)196609, Category.Extensions))
		{
			try
			{
				Result val = NavAppPublishHelper.VerifySignature(packageStream, skipSigningVerification);
				if (val.Failure)
				{
					return val;
				}
				using NavSqlConnectionScope navSqlConnectionScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.Update);
				Result val2 = null;
				using (NavSqlDatabaseLock theLock = NavAppPublishHelper.CreatePackageIdLock(navSqlConnectionScope, PackageId.op_Implicit(packageId)))
				{
					Result val3 = NavAppPublishHelper.EnsureLockWasAcquired(theLock, PackageId.op_Implicit(packageId));
					if (val3.Failure)
					{
						return val3;
					}
					if (scope == NavAppScope.Tenant)
					{
						val2 = PublishIntoTenantScope(navSqlConnectionScope, systemCompileAction, packageStream, PackageId.op_Implicit(packageId), packageType, tenantId, skipLicenseValidation, isDeveloperExtension, radChanges);
						if (val2.Failure)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YV", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(val2.Error));
						}
					}
					else
					{
						NavAppMetadata val4 = NavAppMetadata.FromManifest(PackageId.op_Implicit(packageId), navAppManifest, (MediaId)null, (MediaSetId)null, (string)null, NavAppScope.Global, (string)null);
						val4.PackageType = packageType;
						val2 = PublishIntoGlobalScope(navSqlConnectionScope, systemCompileAction, packageStream, val4, skipLicenseValidation, tenantId);
						if (val2.Failure)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YW", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(val2.Error));
						}
					}
				}
				if (val2.Success)
				{
					navSqlConnectionScope.Connection.CheckEndTransaction(NavSqlEndTransactionType.Commit);
				}
				return val2;
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001YQ", Category.Extensions, exception);
				throw;
			}
		}
	}

	private static Result PublishIntoGlobalScope(NavSqlConnectionScope appDatabaseScope, DoSystemCompileStep systemCompileAction, Stream packageStream, NavAppMetadata partialMetadata, bool skipLicenseValidation, string tenantId)
	{
		if (tenantId != null)
		{
			return Result.Fail(Error.FromException((Exception)new InvalidOperationException(Exceptions.Publish_SpecifyingTenantNotValid)));
		}
		NavAppScope? packageScopeIfItHasBeenSeenBefore = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageScopeIfItHasBeenSeenBefore(partialMetadata.PackageId);
		if (packageScopeIfItHasBeenSeenBefore.HasValue && packageScopeIfItHasBeenSeenBefore.Value == NavAppScope.Tenant)
		{
			return Result.Fail(Error.FromMessage("This extension cannot be published into the global scope because it is currently or was previously published into the tenant scope. An extension cannot be published into multiple scopes at the same time. Also, you cannot change the scope for an extension, unless you make sure that it was unpublished, and you then restart the service.", ErrorType.InvalidOperation));
		}
		AppMetadataLoader appMetadataLoader = new AppMetadataLoader(appDatabaseScope);
		NavAppMetadata appMetadataOrNull = appMetadataLoader.GetAppMetadataOrNull(partialMetadata.PackageId);
		if (appMetadataOrNull != null && appMetadataOrNull.Scope == NavAppScope.Global)
		{
			return Result.Ok();
		}
		if (appMetadataOrNull != null && appMetadataOrNull.Scope == NavAppScope.Tenant)
		{
			return Result.Fail(Error.FromMessage("This extension cannot be published into the global scope because it is currently or was previously published into the tenant scope. An extension cannot be published into multiple scopes at the same time. Also, you cannot change the scope for an extension, unless you make sure that it was unpublished, and you then restart the service.", ErrorType.InvalidOperation));
		}
		return systemCompileAction(appDatabaseScope, packageStream, partialMetadata.PackageType, skipLicenseValidation, NavAppScope.Global);
	}

	private static Result PublishIntoTenantScope(NavSqlConnectionScope appDatabaseScope, DoSystemCompileStep systemCompileAction, Stream packageStream, PackageId packageId, NavAppPackageType packageType, string tenantId, bool skipLicenseValidation, bool isDeveloperExtension, ObjectChangeModelDefinition radChanges)
	{
		NavAppScope? packageScopeIfItHasBeenSeenBefore = NavEnvironment.Instance.NavAppMetadataRetriever.GetPackageScopeIfItHasBeenSeenBefore(packageId);
		if (packageScopeIfItHasBeenSeenBefore.HasValue && packageScopeIfItHasBeenSeenBefore.Value == NavAppScope.Global)
		{
			return Result.Fail(Error.FromMessage("This extension cannot be published into the tenant scope because it is currently or was previously published into the global scope. An extension cannot be published into multiple scopes at the same time. Also, you cannot change the scope for an extension, unless you make sure that it was unpublished, and you then restart the service.", ErrorType.InvalidOperation));
		}
		AppMetadataLoader appMetadataLoader = new AppMetadataLoader(appDatabaseScope);
		NavAppMetadata appMetadataOrNull = appMetadataLoader.GetAppMetadataOrNull(packageId);
		Result val = Result.Ok();
		if (appMetadataOrNull == null)
		{
			val = systemCompileAction(appDatabaseScope, packageStream, packageType, skipLicenseValidation, NavAppScope.Tenant, isDeveloperExtension, isDeveloperExtension ? tenantId : null, radChanges);
		}
		else if (appMetadataOrNull.Scope == NavAppScope.Global)
		{
			return Result.Fail(Error.FromMessage("This extension cannot be published into the tenant scope because it is currently or was previously published into the global scope. An extension cannot be published into multiple scopes at the same time. Also, you cannot change the scope for an extension, unless you make sure that it was unpublished, and you then restart the service.", ErrorType.InvalidOperation));
		}
		if (val != null && val.Failure)
		{
			return val;
		}
		if (!string.IsNullOrWhiteSpace(tenantId))
		{
			NavAppRuntimeMetadata val2 = NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(tenantId).FirstOrDefault((NavAppRuntimeMetadata m) => (WrappedComparable<Guid>)(object)m.PackageId == (WrappedComparable<Guid>)(object)packageId);
			if (val2 != null)
			{
				return Result.Fail(Error.FromMessage("This Extension cannot be published as it is already published for the specified tenant.", ErrorType.InvalidOperation));
			}
			NavAppRuntimeMetadata metadata = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(packageId);
			NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(metadata);
			if (publishProperties.IsDeveloperExtension() && !publishProperties.IsDeveloperExtensionForTenant(tenantId))
			{
				string text = string.Format(CultureInfo.InvariantCulture, "The extension with package ID '{0}' cannot be published for the specified tenant because the same package is in use as a development extension by another tenant.", ((WrappedValue<Guid>)(object)packageId).Value);
				return Result.Fail(Error.FromMessage(text, ErrorType.InvalidOperation));
			}
			NavTenant tenantById = NavEnvironment.Instance.Tenants.GetTenantById(tenantId);
			using (NavSqlConnectionScope connectionScope = NavSqlConnectionScope.CreateForceNewConnection(tenantById.Database, TransactionType.Update))
			{
				new AppPublishedAppStorage(tenantById.Database, connectionScope).InsertOrUpdate(metadata);
			}
			AppPublishReferenceStorage appPublishReferenceStorage = new AppPublishReferenceStorage(NavGlobal.AppDatabase, appDatabaseScope);
			appPublishReferenceStorage.InsertOrUpdatePublishReference(tenantId, packageId);
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutPublishReferenceChange(tenantId);
		}
		return val;
	}

	private static Result DoSystemCompileStepForRegularPackage(NavSqlConnectionScope appDatabaseScope, Stream packageStream, NavAppPackageType packageType, bool skipLicenseValidation, NavAppScope scope, bool isDeveloperExtension = false, string tenantId = null, ObjectChangeModelDefinition radChanges = null)
	{
		return ResultExtensions.OnSuccess<PackageId>(ResultExtensions.OnSuccess<INavAppPackageMetadata, PackageId>(ResultExtensions.OnSuccess<CodeModuleOutputter, INavAppPackageMetadata>(ResultExtensions.OnSuccess<CompilationOutput, CodeModuleOutputter>(NavAppPackageCompiler.Recompile(packageStream, packageType, isDeveloperExtension, null, radChanges), (Func<CompilationOutput, Result<CodeModuleOutputter>>)((CompilationOutput compilerOutput) => ValidateLicense(compilerOutput, skipLicenseValidation))), (Func<CodeModuleOutputter, Result<INavAppPackageMetadata>>)((CodeModuleOutputter outputter) => DirectFillBlobCache(NavEnvironment.Instance.NavAppBlobCache, outputter as NavAppPackageMetadataOutputter))), (Func<INavAppPackageMetadata, Result<PackageId>>)((INavAppPackageMetadata outputter) => CommitToDatabase(appDatabaseScope, packageStream, overwrite: false, outputter as NavAppPackageMetadataOutputter, scope, tenantId, radChanges))), (Action<PackageId>)((PackageId packageId) =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
		}));
	}

	private static Result DoSystemCompileStepForRuntimePackage(NavSqlConnectionScope appDatabaseScope, Stream packageStream, NavAppPackageType packageType, bool skipLicenseValidation, NavAppScope scope, bool isDeveloperExtension = false, string tenantId = null, ObjectChangeModelDefinition radChanges = null)
	{
		ServerUserSettings instance = ServerUserSettings.Instance;
		if (instance.ExtensionAllowedTargetLevel.Value != Microsoft.Dynamics.Nav.Types.CompilationTarget.Internal)
		{
			return Result.Fail(Error.FromMessage(string.Format(CultureInfo.InvariantCulture, Lang.NavAppsHigherThanAllowedCompilationTarget), ErrorType.InvalidOperation));
		}
		return ResultExtensions.OnSuccess<PackageId>(ResultExtensions.OnSuccess<INavAppPackageMetadata, PackageId>(NavAppPackageCompiler.ExtractEmittedContent(packageStream), (Func<INavAppPackageMetadata, Result<PackageId>>)((INavAppPackageMetadata metadataPackage) => CommitToDatabase(appDatabaseScope, packageStream, overwrite: false, metadataPackage, scope))), (Action<PackageId>)((PackageId packageId) =>
		{
			NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
		}));
	}

	private static bool IsRuntimePackage(NavAppPackageReader reader)
	{
		try
		{
			return reader.ReadEmittedFilePaths().Any((string uri) => string.Equals(uri, "/bin/EmittedContent.json", StringComparison.Ordinal));
		}
		catch
		{
			return false;
		}
	}

	public static Result Repair(RepairNavAppArguments arguments)
	{
		string content = string.Format(CultureInfo.InvariantCulture, "Repair Extension Request -- Name:{0}; Publisher:{1}; Version:{2}; SkipSigningVerification:{3}", arguments.Name, arguments.Publisher, arguments.Version, arguments.SkipSigningVerification);
		using (DiagnosticsResolver.GetMostSpecificInstance().CreateTraceScope("00001YO", DiagnosticParameter.SystemMetadata(content), (MonitorType)196609, Category.Extensions))
		{
			Stream packageStream = null;
			try
			{
				NavSqlConnectionScope appDatabaseScope = NavSqlConnectionScope.CreateForceNewConnection(NavGlobal.AppDatabase, TransactionType.Update);
				try
				{
					Result<NavAppMetadata> val = ExtractMetadataFromPublication(appDatabaseScope, arguments);
					if (((Result)val).Failure)
					{
						if (((Result)val).Failure)
						{
							DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YX", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(((Result)val).Error));
						}
						return (Result)(object)val;
					}
					NavAppMetadata metadata = val.Value;
					PackageId packageId = metadata.PackageId;
					using NavSqlDatabaseLock theLock = NavAppPublishHelper.CreatePackageIdLock(appDatabaseScope, packageId);
					Result val2 = NavAppPublishHelper.EnsureLockWasAcquired(theLock, packageId);
					if (val2.Failure)
					{
						return val2;
					}
					Result val3 = ResultExtensions.OnSuccess((Result)(object)ResultExtensions.OnSuccess<CodeModuleOutputter, PackageId>(ResultExtensions.OnSuccess<CompilationOutput, CodeModuleOutputter>(ResultExtensions.OnSuccess<CompilationOutput>((Result)(object)ResultExtensions.OnSuccess<Stream, Stream>(ExtractPackageFromPublication(appDatabaseScope, packageId), (Func<Stream, Stream>)((Stream stream) => packageStream = stream)), (Func<Result<CompilationOutput>>)(() => NavAppPackageCompiler.Recompile(packageStream, metadata.PackageType))), (Func<CompilationOutput, Result<CodeModuleOutputter>>)((CompilationOutput compilerOutput) => ValidateLicense(compilerOutput))), (Func<CodeModuleOutputter, Result<PackageId>>)((CodeModuleOutputter outputter) => CommitToDatabase(appDatabaseScope, packageStream, overwrite: true, outputter as NavAppPackageMetadataOutputter, metadata.Scope, metadata.TenantId))), (Action)(() =>
					{
						NavEnvironment.Instance.NavAppMetadataRetriever.NotifyAboutMetadataChange(packageId);
					}));
					if (val3.Failure)
					{
						DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001YY", Category.Extensions, Verbosity.Error, PrivacyClassification.SystemMetadata, ErrorExtensions.ToSingleMessage(val3.Error));
					}
					return val3;
				}
				finally
				{
					if (appDatabaseScope != null)
					{
						((IDisposable)appDatabaseScope).Dispose();
					}
				}
			}
			catch (Exception exception)
			{
				DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001YR", Category.Extensions, exception);
				throw;
			}
			finally
			{
				if (packageStream != null)
				{
					packageStream.Dispose();
				}
			}
		}
	}

	private static Result<INavAppPackageMetadata> DirectFillBlobCache(INavFilesystemContentCache blobCache, INavAppPackageMetadata outputter)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		List<NavAppObjectMetadata> list = outputter.ObjectMetadata.Where((NavAppObjectMetadata m) =>
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((Enum)m.ObjectFlags).HasFlag((Enum)(object)(NavAppObjectFlags)1);
		}).ToList();
		foreach (NavAppObjectMetadata item in list)
		{
			Tuple<Sha256Hash, string, string>[] array = new Tuple<Sha256Hash, string, string>[2]
			{
				Tuple.Create(item.MetadataHash, "xml", item.Metadata),
				Tuple.Create(item.UserCodeHash, "cs", item.UserCode)
			};
			Tuple<Sha256Hash, string, string>[] array2 = array;
			foreach (Tuple<Sha256Hash, string, string> tuple in array2)
			{
				if (tuple?.Item1 == null)
				{
					continue;
				}
				try
				{
					NavFilesystemContentCacheKey cacheKey = new NavFilesystemContentCacheKey(tuple.Item1, tuple.Item2);
					if (!blobCache.HasContent(cacheKey))
					{
						blobCache.PutContent(cacheKey, Encoding.UTF8.GetBytes(tuple.Item3));
					}
				}
				catch
				{
				}
			}
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074K", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Direct hydrating blob cache -- Objects:{0}; Elapsed:{1}", list.Count, stopwatch.Elapsed.ToStringWithMilliseconds());
		return Result.Ok<INavAppPackageMetadata>(outputter);
	}

	private static Result<PackageId> CommitToDatabase(NavSqlConnectionScope connectionScope, Stream packageStream, bool overwrite, INavAppPackageMetadata outputter, NavAppScope scope, string tenantId = null, ObjectChangeModelDefinition radChanges = null)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		AppMetadataLoader loader = new AppMetadataLoader(connectionScope);
		AppMediaLoader mediaLoader = new AppMediaLoader(connectionScope);
		NavAppMediaIdListing mediaIdListing = outputter.MediaIdListing;
		MediaSetId setId = MediaSetId.op_Implicit((mediaIdListing.MediaSets?.SingleOrDefault((NavAppMediaSetEntry set) => set.Name.Equals("screenshots", StringComparison.OrdinalIgnoreCase)))?.SetId ?? Guid.Empty);
		MediaId logoId = MediaId.op_Implicit(mediaIdListing.Logo?.Id ?? Guid.Empty);
		NavAppMetadata appMetadata = NavAppMetadata.FromManifest(outputter.PackageId, outputter.Manifest, logoId, setId, outputter.ContentHash, scope, tenantId);
		appMetadata.PackageType = outputter.PackageType;
		Result<PackageId> val;
		try
		{
			List<NavAppResource> resources = PrepareResources(outputter);
			val = ResultExtensions.OnSuccess<PackageId>(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(VerifyNotAlreadyPublished(connectionScope, outputter.PackageId, outputter.Manifest, overwrite, tenantId), (Func<Result>)(() => loader.DeleteAppMetadata(outputter.PackageId))), (Func<Result>)(() => mediaLoader.DeleteAppMedia(logoId, new MediaSetId[1] { setId }))), (Action)(() =>
			{
				if (radChanges == null)
				{
					new NavAppObjectStorage(connectionScope.Connection.Database, connectionScope).DeleteAppRecords(outputter.PackageId);
				}
			})), (Func<Result>)(() => (radChanges != null) ? PerformCrudOnRadChanges(connectionScope, outputter.PackageId, outputter.ObjectMetadata, radChanges) : new NavAppObjectLoader(connectionScope).LoadObjectsForApp(outputter.PackageId, outputter.ObjectMetadata))), (Func<Result>)(() => mediaLoader.LoadNavAppMedia(outputter.Media, setId))), (Func<Result>)(() => loader.LoadNavAppMetadata(outputter.PackageId, appMetadata, packageStream, NoCapabilities, outputter.Manifest.Dependencies, NoPrerequisites, resources, outputter.Symbols))), (Func<PackageId>)(() => outputter.PackageId));
		}
		catch (Exception)
		{
			connectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
			throw;
		}
		if (((Result)val).Failure)
		{
			connectionScope.TransactionEnd(NavSqlEndTransactionType.Rollback);
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000074L", Category.Extensions, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Committing compiled artifacts -- Elapsed:{0}", stopwatch.Elapsed.ToStringWithMilliseconds());
		return val;
	}

	private static List<NavAppResource> PrepareResources(INavAppPackageMetadata outputter)
	{
		NavAppResourceId id = new NavAppResourceId(outputter.PackageId, NavAppResourceType.Profile, NavAppProfileResourceNames.Metadata);
		byte[] content = outputter.CombinedProfileMetadata.Serialize();
		Sha256Hash contentHash = Sha256Hash.ComputeHashForContent(content);
		return new List<NavAppResource>
		{
			new NavAppResource(id, content, contentHash)
		};
	}

	private static Result VerifyNotAlreadyPublished(NavSqlConnectionScope connectionScope, PackageId packageId, NavAppManifest manifest, bool overwrite, string tenantId)
	{
		AppMetadataLoader loader = new AppMetadataLoader(connectionScope);
		if (overwrite)
		{
			return ResultExtensions.OnSuccess(loader.VerifyNoPackagePublishedWithSameNamePublisherAndVersionUnlessItSharesTheSamePackageId(packageId, manifest.AppName, manifest.AppPublisher, manifest.AppVersion), (Func<Result>)(() => loader.VerifyNoPackagePublishedWithSameAppIdAndVersionUnlessItSharesTheSamePackageId(packageId, AppId.op_Implicit(manifest.AppId), manifest.AppVersion)));
		}
		if (!string.IsNullOrEmpty(tenantId))
		{
			return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(loader.VerifyPackageNotPublished(packageId), (Func<Result>)(() => loader.VerifyNoPackagePublishedWithSameNamePublisherAndVersionOnTenant(manifest.AppName, manifest.AppPublisher, manifest.AppVersion, tenantId))), (Func<Result>)(() => loader.VerifyNoPackagePublishedWithSameAppIdAndVersionOnTenant(AppId.op_Implicit(manifest.AppId), manifest.AppVersion, tenantId)));
		}
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(loader.VerifyPackageNotPublished(packageId), (Func<Result>)(() => loader.VerifyNoPackagePublishedWithSameNamePublisherAndVersion(manifest.AppName, manifest.AppPublisher, manifest.AppVersion))), (Func<Result>)(() => loader.VerifyNoPackagePublishedWithSameAppIdAndVersion(AppId.op_Implicit(manifest.AppId), manifest.AppVersion)));
	}

	private static Result<Stream> ExtractPackageFromPublication(NavSqlConnectionScope connectionScope, PackageId packageId)
	{
		try
		{
			return new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAppPackageStream(packageId);
		}
		catch (Exception ex)
		{
			return Result.Fail<Stream>(ex);
		}
	}

	private static Result<NavAppMetadata> ExtractMetadataFromPublication(NavSqlConnectionScope connectionScope, RepairNavAppArguments arguments)
	{
		try
		{
			AppMetadataLoader loader = new AppMetadataLoader(connectionScope);
			return ResultExtensions.OnSuccess<PackageId, NavAppMetadata>(loader.ResolvePackageId(arguments.Name, arguments.Publisher, arguments.Version), (Func<PackageId, Result<NavAppMetadata>>)((PackageId packageId) => loader.GetAppMetadata(packageId)));
		}
		catch (Exception ex)
		{
			return Result.Fail<NavAppMetadata>(ex);
		}
	}

	private static Result PerformCrudOnRadChanges(NavSqlConnectionScope connectionScope, PackageId packageId, IList<NavAppObjectMetadata> metadataEntries, ObjectChangeModelDefinition radChanges)
	{
		try
		{
			if (radChanges == null || (radChanges.Added == null && radChanges.Modified == null && radChanges.Removed == null))
			{
				return Result.Ok();
			}
			NavAppObjectStorage navAppObjectStorage = new NavAppObjectStorage(NavGlobal.AppDatabase, connectionScope);
			navAppObjectStorage.AddOrUpdateOrDeleteAppRecords(packageId, radChanges, metadataEntries);
			return Result.Ok();
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
	}

	private static Result<CodeModuleOutputter> ValidateLicense(CompilationOutput compilationOutput, bool skipValidation = false)
	{
		if (skipValidation || compilationOutput.Compilation == null)
		{
			return Result.Ok<CodeModuleOutputter>(compilationOutput.Outputter);
		}
		ImmutableArray<IApplicationObjectTypeSymbol> declaredApplicationObjectSymbols = compilationOutput.Compilation.GetDeclaredApplicationObjectSymbols();
		INavLicense navLicense = NavEnvironment.NavLicense;
		bool flag = ApplicationIdTypes.IsOnPrem(NavCurrentThread.Session.AppId);
		foreach (IApplicationObjectTypeSymbol item in declaredApplicationObjectSymbols)
		{
			PermissionMask licensePermissionMask = navLicense.GetLicensePermissionMask(item.NavTypeKind.ToLicensePermissionObjectType(), item.Id, NavDate.Create(DateTime.Today));
			if (item.IsInFreePublishingRange() && item.IsExtensionObject())
			{
				if (!PermissionHelper.HasInsertPermission(licensePermissionMask))
				{
					IApplicationObjectExtensionTypeSymbol applicationObjectExtensionTypeSymbol = item as IApplicationObjectExtensionTypeSymbol;
					PermissionMask permissionMask = PermissionMask.MaxDirect;
					if (applicationObjectExtensionTypeSymbol.Target != null)
					{
						permissionMask = navLicense.GetLicensePermissionMask(applicationObjectExtensionTypeSymbol.Target.NavTypeKind.ToLicensePermissionObjectType(), applicationObjectExtensionTypeSymbol.Target.Id, NavDate.Create(DateTime.Today));
					}
					if ((permissionMask & PermissionMask.Modify) == 0)
					{
						return Result.Fail<CodeModuleOutputter>(Error.FromMessage(string.Format(CultureInfo.CurrentCulture, Lang.ObjectLicensePublishingError, item.Name), ErrorType.InvalidOperation));
					}
				}
			}
			else if (item.IsSubjectToLicenseCheck() && !PermissionHelper.HasInsertPermission(licensePermissionMask) && (!flag || !PermissionHelper.HasModifyPermission(licensePermissionMask)))
			{
				return Result.Fail<CodeModuleOutputter>(Error.FromMessage(string.Format(CultureInfo.CurrentCulture, Lang.ObjectLicensePublishingError, item.Name), ErrorType.InvalidOperation));
			}
		}
		return Result.Ok<CodeModuleOutputter>(compilationOutput.Outputter);
	}
}
