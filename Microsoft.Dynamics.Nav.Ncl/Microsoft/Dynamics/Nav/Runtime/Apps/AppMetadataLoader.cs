using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps;
using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AppMetadataLoader
{
	private readonly NavSqlConnectionScope connectionScope;

	public AppMetadataLoader(NavSqlConnectionScope connectionScope)
	{
		this.connectionScope = connectionScope;
	}

	public Result LoadNavAppMetadata(PackageId packageId, NavAppMetadata metadata, string packageFilePath, IEnumerable<NavAppCapability> capabilities, IEnumerable<NavAppDependency> dependencies, IEnumerable<NavAppObjectPrerequisite> prerequisites, IEnumerable<NavAppResource> resources, byte[] symbols = null)
	{
		using FileStream packageFileStream = new FileStream(packageFilePath, FileMode.Open, FileAccess.Read);
		return LoadNavAppMetadata(packageId, metadata, packageFileStream, capabilities, dependencies, prerequisites, resources, symbols);
	}

	public Result LoadNavAppMetadata(PackageId packageId, NavAppMetadata metadata, Stream packageFileStream, IEnumerable<NavAppCapability> capabilities, IEnumerable<NavAppDependency> dependencies, IEnumerable<NavAppObjectPrerequisite> prerequisites, IEnumerable<NavAppResource> resources, byte[] symbols = null)
	{
		try
		{
			AppMetadataStorage metadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			AppCapabilitiesStorage capabilityStorage = new AppCapabilitiesStorage(NavGlobal.AppDatabase, connectionScope);
			AppDependenciesStorage dependencyStorage = new AppDependenciesStorage(NavGlobal.AppDatabase, connectionScope);
			AppObjectPrerequisiteStorage objectPrerequisiteStorage = new AppObjectPrerequisiteStorage(NavGlobal.AppDatabase, connectionScope);
			AppResourceStorage resourceStorage = new AppResourceStorage(NavGlobal.AppDatabase, connectionScope);
			return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(AddCollectionDataToDatabase(capabilities, (NavAppCapability x) =>
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return capabilityStorage.AddAppCapability(packageId, x);
			}, Exceptions.Database_AddingAppCapabilitiesFailed), (Func<Result>)(() => AddCollectionDataToDatabase(dependencies, (NavAppDependency x) => dependencyStorage.AddAppDependency(packageId, x), Exceptions.Database_AddingDependenciesFailed))), (Func<Result>)(() => AddCollectionDataToDatabase(prerequisites, (NavAppObjectPrerequisite x) => objectPrerequisiteStorage.AddAppPrerequisite(packageId, x), Exceptions.Database_AddingAppPrerequisitesFailed))), (Func<Result>)(() => AddCollectionDataToDatabase(resources, (NavAppResource x) => resourceStorage.AddAppResource(x), Exceptions.Database_AddingAppResourcesFailed))), (Func<Result>)(() => metadataStorage.AddAppMetadata(metadata, packageFileStream, symbols)));
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
		catch (NavSqlException ex2)
		{
			return Result.Fail((Exception)ex2);
		}
	}

	public Result VerifyDependenciesArePublished(IEnumerable<NavAppDependency> dependencies)
	{
		if (dependencies == null)
		{
			throw new ArgumentNullException("dependencies");
		}
		try
		{
			AppMetadataStorage metadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			List<NavAppDependency> list = dependencies.Where((NavAppDependency dependency) => metadataStorage.IsCompatibleAppPresent(AppId.op_Implicit(dependency.AppId), dependency.MinVersion, dependency.CompatibilityId).Failure).ToList();
			if (list.Count > 0)
			{
				string text = list.Aggregate(string.Format(CultureInfo.InvariantCulture, "This Extension cannot be published due to the missing dependencies: {0}", Environment.NewLine), (string current, NavAppDependency missingDependency) => string.Format(CultureInfo.InvariantCulture, "{0}{1}.\r\n", current, missingDependency));
				return Result.Fail(Error.FromMessage(text, ErrorType.InvalidOperation));
			}
			return Result.Ok();
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
	}

	public Result VerifyNoPackagePublishedWithSameNamePublisherAndVersion(string name, string publisher, Version version)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(name, publisher, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Any()) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it has the same Publisher, Name, and Version as a previously published Extension.", ErrorType.InvalidOperation))));
	}

	public Result VerifyNoPackagePublishedWithSameAppIdAndVersion(AppId appId, Version version)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(appId, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Any()) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it has the same App ID and Version as a previously published Extension.", ErrorType.InvalidOperation))));
	}

	public Result VerifyNoPackagePublishedWithSameNamePublisherAndVersionOnTenant(string name, string publisher, Version version, string tenantId)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(name, publisher, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Where((NavAppMetadata x) => x.TenantId.Equals(tenantId)).Any()) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it has the same Publisher, Name, and Version as a previously published Extension.", ErrorType.InvalidOperation))));
	}

	public Result VerifyNoPackagePublishedWithSameAppIdAndVersionOnTenant(AppId appId, Version version, string tenantId)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(appId, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Where((NavAppMetadata x) => x.TenantId.Equals(tenantId)).Any()) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it has the same App ID and Version as a previously published Extension.", ErrorType.InvalidOperation))));
	}

	public Result VerifyNoPackagePublishedWithSameNamePublisherAndVersionUnlessItSharesTheSamePackageId(PackageId packageId, string name, string publisher, Version version)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(name, publisher, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Any((NavAppMetadata x) => (WrappedComparable<Guid>)(object)x.PackageId != (WrappedComparable<Guid>)(object)packageId)) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it is already published.", ErrorType.InvalidOperation))));
	}

	public Result VerifyNoPackagePublishedWithSameAppIdAndVersionUnlessItSharesTheSamePackageId(PackageId packageId, AppId appId, Version version)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>>(GetAppMetadata(appId, version), (Func<IEnumerable<NavAppMetadata>, Result>)((IEnumerable<NavAppMetadata> apps) => (!apps.Any((NavAppMetadata x) => (WrappedComparable<Guid>)(object)x.PackageId != (WrappedComparable<Guid>)(object)packageId)) ? Result.Ok() : Result.Fail(Error.FromMessage("This Extension cannot be published as it is already published.", ErrorType.InvalidOperation))));
	}

	public Result VerifyPackageNotPublished(PackageId packageId)
	{
		if (!((Result)GetAppMetadata(packageId)).Success)
		{
			return Result.Ok();
		}
		return Result.Fail(Error.FromMessage("This Extension cannot be published as it is already published.", ErrorType.InvalidOperation));
	}

	public Result VerifyPrerequisites(IEnumerable<NavAppObjectPrerequisite> prerequisites)
	{
		if (prerequisites == null)
		{
			throw new ArgumentNullException("prerequisites");
		}
		try
		{
			AppMetadataStorage metadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			List<NavAppObjectPrerequisite> list = prerequisites.Where((NavAppObjectPrerequisite prerequisite) => metadataStorage.IsPrerequisitePresent(prerequisite.Type, prerequisite.Id).Failure).ToList();
			if (list.Count > 0)
			{
				string text = list.Aggregate(string.Format(CultureInfo.InvariantCulture, "This Extension cannot be published due the missing prerequisites: {0}", Environment.NewLine), (string current, NavAppObjectPrerequisite missingPrerequisite) => string.Format(CultureInfo.CurrentCulture, "{0}{1}.{2}", current, missingPrerequisite, Environment.NewLine));
				return Result.Fail(Error.FromMessage(text, ErrorType.InvalidOperation));
			}
			return Result.Ok();
		}
		catch (NavCSideException ex)
		{
			return Result.Fail(Error.FromException((Exception)ex));
		}
	}

	public Result DeleteAppMetadata(PackageId packageId)
	{
		AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
		AppObjectPrerequisiteStorage prerequisites = new AppObjectPrerequisiteStorage(NavGlobal.AppDatabase, connectionScope);
		AppDependenciesStorage dependencies = new AppDependenciesStorage(NavGlobal.AppDatabase, connectionScope);
		AppCapabilitiesStorage capabilities = new AppCapabilitiesStorage(NavGlobal.AppDatabase, connectionScope);
		AppResourceStorage resources = new AppResourceStorage(NavGlobal.AppDatabase, connectionScope);
		return ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(appMetadataStorage.DeleteAppMetadata(packageId), (Func<Result>)(() => dependencies.DeleteAppDependencies(packageId))), (Func<Result>)(() => prerequisites.DeleteAppPrerequisite(packageId))), (Func<Result>)(() => capabilities.DeleteAppCapabilities(packageId))), (Func<Result>)(() => resources.DeleteAppResources(packageId)));
	}

	public Result<PackageId> ResolvePackageId(string name, string publisher = null, Version version = null, string tenantId = null)
	{
		return ResultExtensions.OnSuccess<IEnumerable<NavAppMetadata>, PackageId>(GetAppMetadata(name, publisher, version), (Func<IEnumerable<NavAppMetadata>, Result<PackageId>>)((IEnumerable<NavAppMetadata> apps) =>
		{
			if (apps.Count() == 1)
			{
				return Result.Ok<PackageId>(apps.First().PackageId);
			}
			if (apps.Count() > 1)
			{
				if (!string.IsNullOrEmpty(tenantId))
				{
					NavAppMetadata val = apps.SingleOrDefault((NavAppMetadata a) => a.TenantId.Equals(tenantId));
					if (val != null)
					{
						return Result.Ok<PackageId>(val.PackageId);
					}
				}
				return Result.Fail<PackageId>(Error.FromMessage(Lang.MultiplePublishedAppsMatch_WhereUniquenessIsNecessary, ErrorType.Argument));
			}
			return Result.Fail<PackageId>(Error.FromMessage("No published package matches the provided arguments.", ErrorType.Argument));
		}));
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(string name, string publisher = null, Version version = null)
	{
		try
		{
			AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			return (!string.IsNullOrWhiteSpace(publisher)) ? ((version == null) ? appMetadataStorage.GetAppMetadata(name, publisher) : appMetadataStorage.GetAppMetadata(name, publisher, version)) : ((version == null) ? appMetadataStorage.GetAppMetadata(name) : appMetadataStorage.GetAppMetadata(name, version));
		}
		catch (NavCSideException ex)
		{
			return Result.Fail<IEnumerable<NavAppMetadata>>(Error.FromException((Exception)ex));
		}
	}

	public Result<IEnumerable<NavAppMetadata>> GetAppMetadata(AppId appId, Version version)
	{
		try
		{
			AppMetadataStorage appMetadataStorage = new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope);
			return appMetadataStorage.GetAppMetadata(appId, version);
		}
		catch (NavCSideException ex)
		{
			return Result.Fail<IEnumerable<NavAppMetadata>>(Error.FromException((Exception)ex));
		}
	}

	public NavAppMetadata GetAppMetadataOrNull(PackageId packageId)
	{
		return new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAppMetadataOrNull(packageId);
	}

	public Result<NavAppMetadata> GetAppMetadata(PackageId packageId)
	{
		try
		{
			return new AppMetadataStorage(NavGlobal.AppDatabase, connectionScope).GetAppMetadata(packageId);
		}
		catch (NavCSideException ex)
		{
			return Result.Fail<NavAppMetadata>(Error.FromException((Exception)ex));
		}
	}

	private static Result AddCollectionDataToDatabase<T>(IEnumerable<T> source, Func<T, Result> writeToDatabase, string errorMessage)
	{
		return IEnumerableResultExtensions.Combine<Result, Result>(source.Select(writeToDatabase), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> success) => Result.Ok()), (Func<IEnumerable<Result>, Result>)((IEnumerable<Result> failures) => Result.Fail(Error.FromMultipleErrors(errorMessage, failures.Select((Result f) => f.Error)))));
	}
}
