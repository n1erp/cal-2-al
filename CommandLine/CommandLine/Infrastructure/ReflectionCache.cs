using System;
using System.Collections.Generic;

namespace CommandLine.Infrastructure;

internal sealed class ReflectionCache
{
	private static readonly ReflectionCache Singleton;

	private readonly IDictionary<Pair<Type, object>, WeakReference> _cache;

	public static ReflectionCache Instance => Singleton;

	public object this[Pair<Type, object> key]
	{
		get
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			if (!_cache.ContainsKey(key))
			{
				return null;
			}
			return _cache[key].Target;
		}
		set
		{
			if (key == null)
			{
				throw new ArgumentNullException("key");
			}
			_cache[key] = new WeakReference(value);
		}
	}

	static ReflectionCache()
	{
		Singleton = new ReflectionCache();
	}

	private ReflectionCache()
	{
		_cache = new Dictionary<Pair<Type, object>, WeakReference>();
	}
}
