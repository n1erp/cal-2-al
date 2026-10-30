using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

public interface INavPageCustomizationRepository
{
	NavAppObjectMetadataRuntimeDeltas GetPersonalizationDelta(int pageId);

	IEnumerable<string> GetPersonalizationALCode(Guid userId);

	void SavePersonalization(int pageId, string delta, string alCode);

	void SavePersonalization(Guid userId, int pageId, string delta, string alCode);

	NavAppObjectMetadataRuntimeDeltas GetConfigurationDeltaForCurrentUser(int pageId, ProfilePageMetadataOwner owner);

	NavAppObjectMetadataRuntimeDeltas GetConfigurationDelta(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner);

	IEnumerable<string> GetConfigurationALCode(NavProfileKey profileKey, ProfilePageMetadataOwner owner, int? pageId = null);

	void SaveConfigurationForCurrentUserProfile(int pageId, string delta, string alCode, ProfilePageMetadataOwner owner);

	void SaveConfiguration(NavProfileKey profileKey, int pageId, string delta, string alCode, ProfilePageMetadataOwner owner);

	void DeleteConfiguration(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner = ProfilePageMetadataOwner.SystemOrApp);

	void DeletePersonalization(int pageId);

	void DeletePersonalization(Guid userId, int pageId);
}
