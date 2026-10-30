using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

internal class DatabaseNavPageCustomizationRepository : INavPageCustomizationRepository
{
	public NavAppObjectMetadataRuntimeDeltas GetPersonalizationDelta(int pageId)
	{
		return DatabaseNavPersonalizationRepository.GetPersonalizationDelta(NavCurrentThread.Session.User.Id, pageId);
	}

	public IEnumerable<string> GetPersonalizationALCode(Guid userId)
	{
		return DatabaseNavPersonalizationRepository.GetPersonalizationALCode(userId);
	}

	public void SavePersonalization(int pageId, string delta, string alCode)
	{
		SavePersonalization(NavCurrentThread.Session.User.Id, pageId, delta, alCode);
	}

	public void SavePersonalization(Guid userId, int pageId, string delta, string alCode)
	{
		DatabaseNavPersonalizationRepository.SavePersonalization(userId, pageId, delta, alCode);
	}

	public void DeletePersonalization(int pageId)
	{
		DeletePersonalization(NavCurrentThread.Session.User.Id, pageId);
	}

	public void DeletePersonalization(Guid userId, int pageId)
	{
		DatabaseNavPersonalizationRepository.DeletePersonalization(userId, pageId);
	}

	public NavAppObjectMetadataRuntimeDeltas GetConfigurationDeltaForCurrentUser(int pageId, ProfilePageMetadataOwner owner)
	{
		NavProfileKey userProfile = NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id);
		return GetConfigurationDelta(userProfile, pageId, owner);
	}

	public NavAppObjectMetadataRuntimeDeltas GetConfigurationDelta(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		if (profileKey == null)
		{
			return null;
		}
		if (profileKey.Scope == ProfileScope.System && owner == ProfilePageMetadataOwner.SystemOrApp)
		{
			return DatabaseNavConfigurationRepository.GetBaseConfigurationDelta(profileKey, pageId);
		}
		return DatabaseNavConfigurationRepository.GetTenantConfigurationDelta(profileKey, pageId, owner);
	}

	public IEnumerable<string> GetConfigurationALCode(NavProfileKey profileKey, ProfilePageMetadataOwner owner, int? pageId = null)
	{
		if (profileKey == null)
		{
			return null;
		}
		if (profileKey.Scope == ProfileScope.System && owner != ProfilePageMetadataOwner.Tenant)
		{
			return DatabaseNavConfigurationRepository.GetBaseConfigurationALCode(profileKey, pageId);
		}
		return DatabaseNavConfigurationRepository.GetTenantConfigurationALCode(profileKey, owner, pageId);
	}

	public void SaveConfigurationForCurrentUserProfile(int pageId, string delta, string alCode, ProfilePageMetadataOwner owner)
	{
		NavProfileKey userProfile = NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id);
		SaveConfiguration(userProfile, pageId, delta, alCode, owner);
	}

	public void SaveConfiguration(NavProfileKey profileKey, int pageId, string delta, string alCode, ProfilePageMetadataOwner owner)
	{
		if (profileKey == null)
		{
			throw new ArgumentNullException("profileKey");
		}
		if (profileKey.Scope == ProfileScope.System && owner != ProfilePageMetadataOwner.Tenant)
		{
			DatabaseNavConfigurationRepository.SaveBaseConfiguration(profileKey, pageId, delta, alCode);
		}
		else
		{
			DatabaseNavConfigurationRepository.SaveTenantConfiguration(profileKey, pageId, delta, alCode, owner);
		}
	}

	public void DeleteConfiguration(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		DatabaseNavConfigurationRepository.DeleteConfiguration(profileKey, pageId, owner);
	}
}
