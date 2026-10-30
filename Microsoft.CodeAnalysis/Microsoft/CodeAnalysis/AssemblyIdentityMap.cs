using System;
using System.Collections.Generic;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal sealed class AssemblyIdentityMap<TValue>
{
	private readonly Dictionary<string, OneOrMany<KeyValuePair<AssemblyIdentity, TValue>>> _map;

	public AssemblyIdentityMap()
	{
		_map = new Dictionary<string, OneOrMany<KeyValuePair<AssemblyIdentity, TValue>>>(AssemblyIdentityComparer.SimpleNameComparer);
	}

	public bool Contains(AssemblyIdentity identity, bool allowHigherVersion = true)
	{
		TValue value;
		return TryGetValue(identity, out value, allowHigherVersion);
	}

	public bool TryGetValue(AssemblyIdentity identity, out TValue value, bool allowHigherVersion = true)
	{
		if (_map.TryGetValue(identity.Name, out var value2))
		{
			int num = -1;
			for (int i = 0; i < value2.Count; i++)
			{
				AssemblyIdentity key = value2[i].Key;
				if (AssemblyIdentity.EqualIgnoringNameAndVersion(key, identity))
				{
					if (key.Version == identity.Version)
					{
						value = value2[i].Value;
						return true;
					}
					if (allowHigherVersion && !(key.Version < identity.Version) && (num == -1 || key.Version < value2[num].Key.Version))
					{
						num = i;
					}
				}
			}
			if (num >= 0)
			{
				value = value2[num].Value;
				return true;
			}
		}
		value = default;
		return false;
	}

	public bool TryGetValue(AssemblyIdentity identity, out TValue value, Func<Version, Version, TValue, bool> comparer)
	{
		if (_map.TryGetValue(identity.Name, out var value2))
		{
			for (int i = 0; i < value2.Count; i++)
			{
				AssemblyIdentity key = value2[i].Key;
				if (comparer(identity.Version, key.Version, value2[i].Value) && AssemblyIdentity.EqualIgnoringNameAndVersion(key, identity))
				{
					value = value2[i].Value;
					return true;
				}
			}
		}
		value = default;
		return false;
	}

	public void Add(AssemblyIdentity identity, TValue value)
	{
		KeyValuePair<AssemblyIdentity, TValue> one = KeyValuePairUtil.Create(identity, value);
		_map[identity.Name] = (_map.TryGetValue(identity.Name, out var value2) ? value2.Add(one) : OneOrMany.Create(one));
	}
}
