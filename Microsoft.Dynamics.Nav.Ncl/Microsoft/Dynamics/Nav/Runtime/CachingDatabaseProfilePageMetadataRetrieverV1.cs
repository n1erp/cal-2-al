using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CachingDatabaseProfilePageMetadataRetrieverV1 : IProfilePageMetadataRetrieverV1
{
	private readonly Dictionary<NavProfileKey, Dictionary<int, Dictionary<string, NavXmlDelta>>> profilePageMetadataDeltas = new Dictionary<NavProfileKey, Dictionary<int, Dictionary<string, NavXmlDelta>>>();

	private readonly object metadataLock = new object();

	private readonly INavV1PageConfiguration navV1PageConfigurationRepository;

	public CachingDatabaseProfilePageMetadataRetrieverV1(INavV1PageConfiguration profileAndPageV1PageConfigurationRepository)
	{
		navV1PageConfigurationRepository = profileAndPageV1PageConfigurationRepository;
	}

	public NavXmlDelta Get(int pageId, string personalizationId)
	{
		NavProfileKey navProfileKey = NavCurrentThread.Session.ProfileKey ?? NavConfigurationProfile.GetUserProfile(NavCurrentThread.Session.User.Id);
		if (NavCurrentThread.Session.ProfileKey == null && navProfileKey != null)
		{
			NavCurrentThread.Session.ProfileKey = navProfileKey;
		}
		return Get(navProfileKey, pageId, personalizationId);
	}

	public NavXmlDelta Get(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		lock (metadataLock)
		{
			if (profileKey == null || profileKey.Scope == ProfileScope.Tenant)
			{
				return null;
			}
			personalizationId = DatabaseNavV1PageConfigurationRepository.AdjustPersonalizationId(personalizationId, pageId);
			EnsureCacheIsPopulatedForProfile(profileKey, pageId, personalizationId);
			return profilePageMetadataDeltas[profileKey][pageId][personalizationId];
		}
	}

	public void Remove(NavProfileKey profileKey)
	{
		lock (metadataLock)
		{
			if (profileKey != null && profileKey.Scope != ProfileScope.Tenant && profilePageMetadataDeltas.ContainsKey(profileKey))
			{
				profilePageMetadataDeltas.Remove(profileKey);
			}
		}
	}

	public void Remove(NavProfileKey profileKey, int pageId)
	{
		lock (metadataLock)
		{
			if (profileKey != null && profileKey.Scope != ProfileScope.Tenant && profilePageMetadataDeltas.ContainsKey(profileKey) && profilePageMetadataDeltas[profileKey].ContainsKey(pageId))
			{
				Dictionary<int, Dictionary<string, NavXmlDelta>> dictionary = profilePageMetadataDeltas[profileKey];
				dictionary.Remove(pageId);
				profilePageMetadataDeltas[profileKey] = dictionary;
			}
		}
	}

	public void Remove(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		lock (metadataLock)
		{
			if (profileKey != null && profileKey.Scope != ProfileScope.Tenant && profilePageMetadataDeltas.ContainsKey(profileKey) && profilePageMetadataDeltas[profileKey].ContainsKey(pageId) && profilePageMetadataDeltas[profileKey][pageId].ContainsKey(personalizationId))
			{
				Dictionary<string, NavXmlDelta> dictionary = profilePageMetadataDeltas[profileKey][pageId];
				dictionary.Remove(personalizationId);
				profilePageMetadataDeltas[profileKey][pageId] = dictionary;
			}
		}
	}

	public void RemoveAll()
	{
		lock (metadataLock)
		{
			profilePageMetadataDeltas.Clear();
		}
	}

	private void EnsureCacheIsPopulatedForProfile(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		if (!IsCached(profileKey, pageId, personalizationId))
		{
			NavXmlDelta delta = navV1PageConfigurationRepository.LoadConfigurationDelta(profileKey, pageId, personalizationId);
			Add(profileKey, pageId, personalizationId, delta);
		}
	}

	private bool IsCached(NavProfileKey profileKey, int pageId, string personalizationId)
	{
		if (profilePageMetadataDeltas.ContainsKey(profileKey) && profilePageMetadataDeltas[profileKey].ContainsKey(pageId))
		{
			return profilePageMetadataDeltas[profileKey][pageId].ContainsKey(personalizationId);
		}
		return false;
	}

	private void Add(NavProfileKey profileKey, int pageId, string personalizationId, NavXmlDelta delta)
	{
		if (!profilePageMetadataDeltas.ContainsKey(profileKey))
		{
			profilePageMetadataDeltas[profileKey] = new Dictionary<int, Dictionary<string, NavXmlDelta>>();
		}
		if (!profilePageMetadataDeltas[profileKey].ContainsKey(pageId))
		{
			profilePageMetadataDeltas[profileKey][pageId] = new Dictionary<string, NavXmlDelta>(StringComparer.OrdinalIgnoreCase);
		}
		profilePageMetadataDeltas[profileKey][pageId][personalizationId] = delta;
	}
}
