using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavV1PageConfiguration
{
	NavXmlDelta LoadPersonalizationDelta(int pageId, string personalizationId);

	NavXmlDelta LoadPersonalizationDelta(Guid userId, int pageId, string personalizationId);

	IEnumerable<UserMetadataRecord> LoadPersonalizationDeltasForUser(Guid userId, string personalizationId);

	void SavePersonalizationDelta(int pageId, string personalizationId, NavXmlDelta delta);

	void SavePersonalizationDelta(NavUser user, int pageId, string personalizationId, NavXmlDelta delta);

	void SavePersonalizationDelta(Guid userId, int pageId, string personalizationId, NavXmlDelta delta);

	NavXmlDelta LoadConfigurationDelta(NavProfileKey profileKey, int pageId, string configurationId);

	IEnumerable<ProfileMetadataRecord> LoadConfigurationDeltasForProfile(NavProfileKey profileKey, string configurationId);

	void SaveConfigurationDelta(NavProfileKey profileKey, int pageId, string personalizationId, NavXmlDelta delta);

	void DeleteConfiguration(NavProfileKey profileKey, int pageId, string personalizationId);

	void DeletePersonalization(int pageId, string personalizationId);

	void DeletePersonalization(Guid userId, int pageId, string personalizationId);
}
