using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Apps.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Designer;

internal class NavDesignerExtensionStorage : INavDesignerStorage
{
	private readonly INavAppMetadataRetriever metadataRetriever;

	public NavDesignerExtensionStorage()
	{
		metadataRetriever = NavEnvironment.Instance.NavAppMetadataRetriever;
	}

	public NavAppPackageType GetPackageType(AppId appId, Version version = null)
	{
		NavAppRuntimeMetadata metadata = FindDesignerExtensionMetadata(appId, version);
		NavAppPublishProperties publishProperties = metadataRetriever.GetPublishProperties(metadata);
		return publishProperties.PackageType;
	}

	public NavAppManifest GetManifest(AppId appId, Version version = null)
	{
		NavAppRuntimeMetadata val = FindDesignerExtensionMetadata(appId, version);
		using Stream stream = metadataRetriever.GetPackageBlob(val.PackageId);
		NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream);
		return navAppPackageReader.ReadNavAppManifest().WithSystemReferences();
	}

	public NavDesignerObjectCollection GetObjects(AppId appId, Version version = null)
	{
		NavDesignerObjectCollection navDesignerObjectCollection = new NavDesignerObjectCollection();
		NavAppRuntimeMetadata val = FindDesignerExtensionMetadata(appId, version);
		using Stream stream = metadataRetriever.GetPackageBlob(val.PackageId);
		NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream);
		IEnumerable<string> source = navAppPackageReader.ReadSourceContents();
		foreach (NavDesignerObject item in source.Select(NavDesignerObjectFactory.CreateFromSource).SelectMany((IEnumerable<NavDesignerObject> a) => a))
		{
			navDesignerObjectCollection.AddObject(item);
		}
		return navDesignerObjectCollection;
	}

	public IDictionary<string, byte[]> GetPackageFiles(AppId appId, Version version = null)
	{
		NavAppRuntimeMetadata val = FindDesignerExtensionMetadata(appId, version);
		using Stream stream = metadataRetriever.GetPackageBlob(val.PackageId);
		NavAppPackageReader navAppPackageReader = NavAppPackageReader.Create(stream);
		NavAppManifest manifest = navAppPackageReader.ReadNavAppManifest();
		return NavDesignerUtil.MakePackageFilesDictionary(navAppPackageReader, manifest);
	}

	public Result SaveData(AppId appId, Stream packageStream, NavAppPackageType packageType)
	{
		NavAppPackage package = NavAppPackage.Open(packageStream, allowWrite: false);
		Result val = ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(ResultExtensions.OnSuccess(NavDesignerPackageManagement.Uninstall(appId), (Func<Result>)(() => NavDesignerPackageManagement.Unpublish(appId))), (Func<Result>)(() => NavDesignerPackageManagement.Publish(packageStream, packageType))), (Func<Result>)(() => NavDesignerPackageManagement.Sync(PackageId.op_Implicit(package.PackageId)))), (Func<Result>)(() => NavDesignerPackageManagement.Install(PackageId.op_Implicit(package.PackageId))));
		if (!val.Success)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00001O6", Category.Development, Verbosity.Error, "Failed to save the extension. {0}", DiagnosticParameter.SystemMetadata(val.Error.Message));
		}
		return val;
	}

	private NavAppRuntimeMetadata FindDesignerExtensionMetadata(AppId appId, Version version = null)
	{
		NavAppRuntimeMetadata val = ((version == null) ? metadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id).SingleOrDefault((NavAppRuntimeMetadata x) => (WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)appId) : metadataRetriever.GetAllMetadataVisibleToTenant(NavCurrentThread.Session.Tenant.Id).SingleOrDefault((NavAppRuntimeMetadata x) => (WrappedComparable<Guid>)(object)x.AppId == (WrappedComparable<Guid>)(object)appId && x.Version == version));
		if (val == null)
		{
			if (version == null)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.NavApp_NoInstalledMatchFound, appId));
			}
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.NavApp_NoInstalledMatchFoundWithVersion, appId, version));
		}
		return val;
	}
}
