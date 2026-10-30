using Microsoft.Dynamics.Nav.Apps.AppMetadata;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class NavAppRuntimeMetadataExtensions
{
	public static NavAppMetadata AsNavAppMetadata(this NavAppRuntimeMetadata runtimeMetadata)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected Obj, but got Unknown
		return new NavAppMetadata
		{
			PackageId = runtimeMetadata.PackageId,
			AppId = runtimeMetadata.AppId,
			Name = runtimeMetadata.Name,
			Publisher = runtimeMetadata.Publisher,
			VersionMajor = runtimeMetadata.Version.Major,
			VersionMinor = runtimeMetadata.Version.Minor,
			VersionBuild = runtimeMetadata.Version.Build,
			VersionRevision = runtimeMetadata.Version.Revision,
			CompatibilityMajor = runtimeMetadata.CompatibilityId.Major,
			CompatibilityMinor = runtimeMetadata.CompatibilityId.Minor,
			CompatibilityBuild = runtimeMetadata.CompatibilityId.Build,
			CompatibilityRevision = runtimeMetadata.CompatibilityId.Revision,
			ContentHash = runtimeMetadata.ContentHash,
			PackageType = NavAppPackageType.Extension
		};
	}

	public static bool IsDevExtension(this NavAppRuntimeMetadata target, string tenantId)
	{
		NavAppPublishProperties publishProperties = NavEnvironment.Instance.NavAppMetadataRetriever.GetPublishProperties(target);
		if (publishProperties == null)
		{
			return false;
		}
		return publishProperties.IsDeveloperExtensionForTenant(tenantId);
	}
}
