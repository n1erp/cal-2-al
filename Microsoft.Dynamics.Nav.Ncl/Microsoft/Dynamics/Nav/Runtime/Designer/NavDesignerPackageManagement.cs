using System;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal static class NavDesignerPackageManagement
{
	internal static Result Unpublish(AppId appId)
	{
		try
		{
			NavAppRuntimeMetadata val = (from m in NavEnvironment.Instance.NavAppMetadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id)
				where (WrappedComparable<Guid>)(object)m.AppId == (WrappedComparable<Guid>)(object)appId
				select m).SingleOrDefault();
			if (val != null)
			{
				return NavAppUnpublisher.Unpublish(((WrappedValue<Guid>)(object)val.PackageId).Value, val.Name, val.Publisher, val.Version, NavCurrentThread.Session.Tenant.Id);
			}
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail(ex);
		}
	}

	internal static Result Publish(Stream packageStream, NavAppPackageType packageType)
	{
		return NavAppPublisher.Publish(packageStream, NavAppScope.Tenant, packageType, skipSigningVerification: true, NavCurrentThread.Session.Tenant.Id, skipLicenseValidation: true, isDeveloperExtension: true);
	}

	internal static Result Sync(PackageId packageId)
	{
		NavAppRuntimeMetadata metadata = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(packageId);
		if (metadata == null)
		{
			return (Result)(object)Result.Fail<string>(Error.FromMessage("Cannot find metadata for package", ErrorType.InvalidOperation));
		}
		SyncNavAppArguments args = new SyncNavAppArguments
		{
			TenantId = NavCurrentThread.Session.Tenant.Id,
			Mode = NavAppSyncMode.Development,
			Name = metadata.Name,
			Publisher = metadata.Publisher,
			Version = metadata.Version,
			Origin = "Designer"
		};
		SyncNavAppResponse syncNavAppResponse = NavAppSyncManagementTasks.SyncNavApp(args);
		SyncNavAppStatus result = syncNavAppResponse.Result;
		if ((uint)(result - 1) <= 1u)
		{
			return Result.Ok();
		}
		return (Result)(object)Result.Fail<string>(Error.FromMessage(syncNavAppResponse.Error, ErrorType.InvalidOperation));
	}

	internal static Result Install(PackageId packageId)
	{
		try
		{
			NavAppRuntimeMetadata metadata = NavEnvironment.Instance.NavAppMetadataRetriever.GetMetadata(packageId);
			InstallNavAppArguments installNavAppArguments = new InstallNavAppArguments
			{
				Name = metadata.Name,
				Publisher = metadata.Publisher,
				Version = metadata.Version,
				TenantId = NavCurrentThread.Session.Tenant.Id,
				Origin = "Designer"
			};
			ChangeNavAppStateResponse changeNavAppStateResponse = new NavManagementTasks(NavCurrentThread.Session).InstallNavApp(installNavAppArguments);
			if (changeNavAppStateResponse.Result != ChangeNavAppStateResult.AppChangeSuccessful)
			{
				return Result.Fail(Error.FromMessage(changeNavAppStateResponse.Error, ErrorType.Unspecified));
			}
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail(ex);
		}
	}

	internal static Result Uninstall(AppId appId)
	{
		try
		{
			NavAppRuntimeMetadata val = NavCurrentThread.Session.Tenant.NavAppGroup.OrderedAppMetadata.SingleOrDefault((NavAppRuntimeMetadata x) => (WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)appId);
			if (val != null)
			{
				UninstallNavAppArguments uninstallNavAppArguments = new UninstallNavAppArguments
				{
					Name = val.Name,
					Publisher = val.Publisher,
					Version = val.Version,
					TenantId = NavCurrentThread.Session.Tenant.Id,
					Origin = "Designer"
				};
				ChangeNavAppStateResponse changeNavAppStateResponse = new NavManagementTasks(NavCurrentThread.Session).UninstallNavApp(uninstallNavAppArguments);
				ChangeNavAppStateResult result = changeNavAppStateResponse.Result;
				if (result == ChangeNavAppStateResult.AppChangeSuccessful || result == ChangeNavAppStateResult.AppNotFound || result == ChangeNavAppStateResult.AppAlreadyChangedWarning)
				{
					return Result.Ok();
				}
				return Result.Fail(Error.FromMessage(changeNavAppStateResponse.Error, ErrorType.Unspecified));
			}
			return Result.Ok();
		}
		catch (Exception ex)
		{
			return Result.Fail(ex);
		}
	}
}
