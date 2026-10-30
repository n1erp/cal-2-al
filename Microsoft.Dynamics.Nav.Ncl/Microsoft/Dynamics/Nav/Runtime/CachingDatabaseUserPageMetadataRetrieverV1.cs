using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CachingDatabaseUserPageMetadataRetrieverV1 : IUserPageMetadataRetrieverV1
{
	private readonly Dictionary<int, Dictionary<string, NavXmlDelta>> userPageMetadataDeltas = new Dictionary<int, Dictionary<string, NavXmlDelta>>();

	private readonly object metadataLock = new object();

	private readonly INavV1PageConfiguration userPageConfigurationConfigurationrepository;

	public CachingDatabaseUserPageMetadataRetrieverV1(INavV1PageConfiguration v1PageConfigurationRepository)
	{
		userPageConfigurationConfigurationrepository = v1PageConfigurationRepository;
	}

	public NavXmlDelta Get(int pageId, string personalizationId)
	{
		lock (metadataLock)
		{
			personalizationId = DatabaseNavV1PageConfigurationRepository.AdjustPersonalizationId(personalizationId, pageId);
			EnsureCacheIsPopulatedForPage(pageId, personalizationId);
			return userPageMetadataDeltas[pageId][personalizationId];
		}
	}

	public void Remove(int pageId)
	{
		lock (metadataLock)
		{
			if (userPageMetadataDeltas.ContainsKey(pageId))
			{
				userPageMetadataDeltas.Remove(pageId);
			}
		}
	}

	public void Remove(int pageId, string personalizationId)
	{
		lock (metadataLock)
		{
			if (userPageMetadataDeltas.ContainsKey(pageId) && userPageMetadataDeltas[pageId].ContainsKey(personalizationId))
			{
				userPageMetadataDeltas[pageId].Remove(personalizationId);
			}
		}
	}

	public void RemoveAll()
	{
		lock (metadataLock)
		{
			userPageMetadataDeltas.Clear();
		}
	}

	private void EnsureCacheIsPopulatedForPage(int pageId, string personalizationId = null)
	{
		if (!IsCached(pageId, personalizationId))
		{
			NavXmlDelta deltas = userPageConfigurationConfigurationrepository.LoadPersonalizationDelta(pageId, personalizationId);
			Add(pageId, personalizationId, deltas);
		}
	}

	private void Add(int pageId, string personalizationId, NavXmlDelta deltas)
	{
		if (!userPageMetadataDeltas.ContainsKey(pageId))
		{
			userPageMetadataDeltas[pageId] = new Dictionary<string, NavXmlDelta>(StringComparer.OrdinalIgnoreCase);
		}
		userPageMetadataDeltas[pageId][personalizationId] = deltas;
	}

	private bool IsCached(int pageId, string personalizationId)
	{
		if (userPageMetadataDeltas.ContainsKey(pageId))
		{
			return userPageMetadataDeltas[pageId].ContainsKey(personalizationId);
		}
		return false;
	}
}
