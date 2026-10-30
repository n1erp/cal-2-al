using System.Collections.Generic;
using System.IO;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Apps;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavAppMetadataRetriever
{
	IEnumerable<NavAppRuntimeMetadata> GetAllMetadata();

	IEnumerable<NavAppRuntimeMetadata> GetAllMetadataVisibleToTenant(string tenantId);

	IEnumerable<NavAppApiMetadata> GetAllApiMetadata();

	IEnumerable<NavAppApiMetadata> GetAllApiMetadataVisibleToTenant(string tenantId);

	IEnumerable<NavAppApiMetadata> GetApiMetadata(PackageId packageId);

	IEnumerable<string> GetTenantIdsWithAppPublished(PackageId packageId);

	NavAppRuntimeMetadata GetMetadata(PackageId packageId);

	NavAppObjectMetadataBlob GetObjectMetadataBlob(ApplicationObjectId objectId, PackageId packageId, NavAppObjectMetadataBlobType blobType);

	NavAppPublishProperties GetPublishProperties(NavAppRuntimeMetadata metadata);

	Stream GetPackageBlob(PackageId packageId);

	NavAppManifest GetManifest(PackageId packageId);

	NavAppResource GetResource(NavAppResourceId id);

	NavAppScope? GetPackageScopeIfItHasBeenSeenBefore(PackageId packageId);

	void NotifyAboutMetadataChange(PackageId packageId);

	void NotifyAboutPublishReferenceChange(string tenantId);

	void NotifyAboutPublishReferenceChange();

	NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> GetObjectExtensionDeltas(ApplicationObjectId objectId, NavAppRuntimeMetadata runtimeAppMetadata);
}
