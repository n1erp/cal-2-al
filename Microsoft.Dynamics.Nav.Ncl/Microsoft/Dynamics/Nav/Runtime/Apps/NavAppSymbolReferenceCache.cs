using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppSymbolReferenceCache
{
	private class VersionAwareModuleDefinition
	{
		private readonly Version version;

		public ModuleDefinition Definition { get; }

		private VersionAwareModuleDefinition(Version version, ModuleDefinition definition)
		{
			this.version = version;
			Definition = definition ?? throw new ArgumentNullException("definition");
		}

		public static VersionAwareModuleDefinition CreateVersioned(Version version, ModuleDefinition definition)
		{
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			return new VersionAwareModuleDefinition(version, definition);
		}

		public static VersionAwareModuleDefinition CreateVersionAgnostic(ModuleDefinition definition)
		{
			return new VersionAwareModuleDefinition(null, definition);
		}

		public bool SatisfiesMinimumVersionRequirement(Version minimumRequiredVersion)
		{
			if (!(version == null))
			{
				return version >= minimumRequiredVersion;
			}
			return true;
		}
	}

	private readonly object cacheLock = new object();

	private readonly Dictionary<string, (VersionAwareModuleDefinition Module, PackageId Id)> latestVersionCache = new Dictionary<string, (VersionAwareModuleDefinition, PackageId)>();

	public bool TryGetModuleDefinition(string name, string publisher, Version version, out ModuleDefinition module)
	{
		lock (cacheLock)
		{
			string key = CreateCacheKey(name, publisher);
			if (latestVersionCache.TryGetValue(key, out (VersionAwareModuleDefinition, PackageId) value) && value.Item1.SatisfiesMinimumVersionRequirement(version))
			{
				module = value.Item1.Definition;
				return true;
			}
		}
		module = null;
		return false;
	}

	public void AddOrUpdateModuleDefinition(string name, string publisher, Version version, ModuleDefinition module, PackageId packageId)
	{
		lock (cacheLock)
		{
			string key = CreateCacheKey(name, publisher);
			latestVersionCache[key] = ((version == null) ? (VersionAwareModuleDefinition.CreateVersionAgnostic(module), packageId) : (VersionAwareModuleDefinition.CreateVersioned(version, module), packageId));
		}
	}

	public void InvalidateModuleDefinition(string name, string publisher)
	{
		lock (cacheLock)
		{
			string key = CreateCacheKey(name, publisher);
			latestVersionCache.Remove(key);
		}
	}

	public void InvalidateModuleDefinition(PackageId packageId)
	{
		lock (cacheLock)
		{
			foreach (KeyValuePair<string, (VersionAwareModuleDefinition, PackageId)> item in latestVersionCache)
			{
				if ((WrappedComparable<Guid>)(object)item.Value.Item2 == (WrappedComparable<Guid>)(object)packageId)
				{
					latestVersionCache.Remove(item.Key);
					break;
				}
			}
		}
	}

	private static string CreateCacheKey(string name, string publisher)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (publisher == null)
		{
			throw new ArgumentNullException("publisher");
		}
		return name.Trim().ToUpperInvariant() + "_" + publisher.Trim().ToUpperInvariant();
	}
}
