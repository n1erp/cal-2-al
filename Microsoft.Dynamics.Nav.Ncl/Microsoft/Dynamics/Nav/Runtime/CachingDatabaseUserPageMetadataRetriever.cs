using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CachingDatabaseUserPageMetadataRetriever : IUserPageMetadataRetriever
{
	private readonly Dictionary<int, NavAppObjectMetadataRuntimeDeltas> userPageMetadataDeltas = new Dictionary<int, NavAppObjectMetadataRuntimeDeltas>();

	private readonly INavPageCustomizationRepository pageCustomizationRepository;

	private readonly object metadataLock = new object();

	public CachingDatabaseUserPageMetadataRetriever(INavPageCustomizationRepository repository)
	{
		pageCustomizationRepository = repository;
	}

	public NavAppObjectMetadataRuntimeDeltas Get(int pageId)
	{
		lock (metadataLock)
		{
			if (!userPageMetadataDeltas.ContainsKey(pageId))
			{
				userPageMetadataDeltas[pageId] = pageCustomizationRepository.GetPersonalizationDelta(pageId);
			}
			return userPageMetadataDeltas[pageId];
		}
	}

	public void Remove(int pageId)
	{
		lock (metadataLock)
		{
			userPageMetadataDeltas.Remove(pageId);
		}
	}

	public void RemoveAll()
	{
		lock (metadataLock)
		{
			userPageMetadataDeltas.Clear();
		}
	}
}
