using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Runtime.XmlMetadata;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class CachingDatabaseProfilePageMetadataRetriever : IProfilePageMetadataRetriever
{
	private struct CompositeKey
	{
		public string SystemOrTenantId { get; set; }

		public NavProfileKey ProfileKey { get; set; }

		public int PageId { get; set; }

		public ProfilePageMetadataOwner Owner { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is CompositeKey compositeKey)
			{
				if (SystemOrTenantId.Equals(compositeKey.SystemOrTenantId, StringComparison.OrdinalIgnoreCase) && ProfileKey.Equals(compositeKey.ProfileKey) && PageId == compositeKey.PageId)
				{
					return Owner == compositeKey.Owner;
				}
				return false;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return SystemOrTenantId.GetHashCode() ^ ProfileKey.GetHashCode() ^ PageId.GetHashCode() ^ Owner.GetHashCode();
		}

		public static CompositeKey Create(string scope, NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
		{
			return new CompositeKey
			{
				SystemOrTenantId = scope,
				ProfileKey = profileKey,
				PageId = pageId,
				Owner = owner
			};
		}
	}

	private readonly Dictionary<CompositeKey, NavAppObjectMetadataRuntimeDeltas> tenantProfilePageMetadataDeltas = new Dictionary<CompositeKey, NavAppObjectMetadataRuntimeDeltas>();

	private readonly INavPageCustomizationRepository pageCustomizationRepository;

	private readonly object metadataLock = new object();

	public CachingDatabaseProfilePageMetadataRetriever(INavPageCustomizationRepository repository)
	{
		pageCustomizationRepository = repository;
	}

	public NavAppObjectMetadataRuntimeDeltas Get(NavProfileKey profileKey, int pageId)
	{
		NavAppObjectMetadataRuntimeDeltas systemDeltas = Get(profileKey, pageId, ProfilePageMetadataOwner.SystemOrApp);
		NavAppObjectMetadataRuntimeDeltas val = Get(profileKey, pageId, ProfilePageMetadataOwner.Tenant);
		if (val != null && systemDeltas != null)
		{
			((NavAppObjectMetadataDeltaCreator<Delta>)(object)val)?.AllDeltas.ForEach((Delta d) =>
			{
				((NavAppObjectMetadataDeltaCreator<Delta>)(object)systemDeltas).Add(d);
			});
		}
		return systemDeltas ?? val;
	}

	public NavAppObjectMetadataRuntimeDeltas Get(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		lock (metadataLock)
		{
			if (profileKey == null)
			{
				return null;
			}
			EnsureCacheIsPopulatedForProfile(profileKey, pageId, owner);
			CompositeKey key = CompositeKey.Create(GetProfileScopeId(profileKey, owner), profileKey, pageId, owner);
			return tenantProfilePageMetadataDeltas[key];
		}
	}

	private void EnsureCacheIsPopulatedForProfile(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		if (!IsCached(profileKey, pageId, owner))
		{
			NavAppObjectMetadataRuntimeDeltas configurationDelta = pageCustomizationRepository.GetConfigurationDelta(profileKey, pageId, owner);
			Add(profileKey, pageId, configurationDelta, owner);
		}
	}

	private void Add(NavProfileKey profileKey, int pageId, NavAppObjectMetadataRuntimeDeltas deltas, ProfilePageMetadataOwner owner)
	{
		string profileScopeId = GetProfileScopeId(profileKey, owner);
		CompositeKey key = CompositeKey.Create(profileScopeId, profileKey, pageId, owner);
		if (!tenantProfilePageMetadataDeltas.ContainsKey(key))
		{
			tenantProfilePageMetadataDeltas[key] = deltas;
		}
	}

	private bool IsCached(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		string profileScopeId = GetProfileScopeId(profileKey, owner);
		CompositeKey key = CompositeKey.Create(profileScopeId, profileKey, pageId, owner);
		return tenantProfilePageMetadataDeltas.ContainsKey(key);
	}

	public void Remove(NavProfileKey profileKey)
	{
		Remove(profileKey, ProfilePageMetadataOwner.SystemOrApp);
		Remove(profileKey, ProfilePageMetadataOwner.Tenant);
	}

	public void Remove(NavProfileKey profileKey, ProfilePageMetadataOwner owner)
	{
		lock (metadataLock)
		{
			if (profileKey != null)
			{
				string profileScopeId = GetProfileScopeId(profileKey, owner);
				List<CompositeKey> list = tenantProfilePageMetadataDeltas.Keys.Where((CompositeKey k) => k.SystemOrTenantId.Equals(profileScopeId, StringComparison.OrdinalIgnoreCase) && k.ProfileKey.Equals(profileKey) && k.Owner == owner).ToList();
				list.ForEach((CompositeKey k) =>
				{
					tenantProfilePageMetadataDeltas.Remove(k);
				});
			}
		}
	}

	public void Remove(NavProfileKey profileKey, int pageId)
	{
		Remove(profileKey, pageId, ProfilePageMetadataOwner.SystemOrApp);
		Remove(profileKey, pageId, ProfilePageMetadataOwner.Tenant);
	}

	public void Remove(NavProfileKey profileKey, int pageId, ProfilePageMetadataOwner owner)
	{
		lock (metadataLock)
		{
			if (profileKey != null)
			{
				string profileScopeId = GetProfileScopeId(profileKey, owner);
				if (IsCached(profileKey, pageId, owner))
				{
					CompositeKey key = CompositeKey.Create(profileScopeId, profileKey, pageId, owner);
					tenantProfilePageMetadataDeltas.Remove(key);
				}
			}
		}
	}

	public void Remove(string tenantId, Guid appId)
	{
		lock (metadataLock)
		{
			List<CompositeKey> list = tenantProfilePageMetadataDeltas.Keys.Where((CompositeKey k) => k.SystemOrTenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase) && k.ProfileKey.AppId.Value.Equals(appId)).ToList();
			list.ForEach((CompositeKey key) =>
			{
				tenantProfilePageMetadataDeltas.Remove(key);
			});
		}
	}

	public void RemoveAll()
	{
		lock (metadataLock)
		{
			tenantProfilePageMetadataDeltas.Clear();
		}
	}

	private static string GetProfileScopeId(NavProfileKey profileKey, ProfilePageMetadataOwner owner)
	{
		if (profileKey.Scope != ProfileScope.System || owner == ProfilePageMetadataOwner.Tenant)
		{
			return NavCurrentThread.Session.Tenant.Id;
		}
		return "SYSTEM";
	}
}
