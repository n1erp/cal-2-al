using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Collections;

internal sealed class OrderPreservingMultiDictionary<K, V>
{
	private readonly ObjectPool<OrderPreservingMultiDictionary<K, V>> pool;

	private static readonly ObjectPool<OrderPreservingMultiDictionary<K, V>> poolInstance = CreatePool();

	private PooledDictionary<K, object> dictionary;

	public bool IsEmpty => dictionary == null;

	public ImmutableArray<V> this[K k]
	{
		get
		{
			if (!IsEmpty && dictionary.TryGetValue(k, out var value))
			{
				if (!(value is ArrayBuilder<V> arrayBuilder))
				{
					DebugAssertHelper.Assert(value is V, "Item must be either a V or an ArrayBuilder<V>");
					return ImmutableArray.Create((V)value);
				}
				return arrayBuilder.ToImmutable();
			}
			return ImmutableArray<V>.Empty;
		}
	}

	public ICollection<K> Keys
	{
		get
		{
			if (!IsEmpty)
			{
				return dictionary.Keys;
			}
			return SpecializedCollections.EmptyCollection<K>();
		}
	}

	private OrderPreservingMultiDictionary(ObjectPool<OrderPreservingMultiDictionary<K, V>> pool)
	{
		this.pool = pool;
	}

	public void Free()
	{
		if (dictionary != null)
		{
			dictionary.Free();
			dictionary = null;
		}
		if (pool != null)
		{
			pool.Free(this);
		}
	}

	public static ObjectPool<OrderPreservingMultiDictionary<K, V>> CreatePool()
	{
		ObjectPool<OrderPreservingMultiDictionary<K, V>> pool = null;
		pool = new ObjectPool<OrderPreservingMultiDictionary<K, V>>(() => new OrderPreservingMultiDictionary<K, V>(pool), 16);
		return pool;
	}

	public static OrderPreservingMultiDictionary<K, V> GetInstance()
	{
		OrderPreservingMultiDictionary<K, V> orderPreservingMultiDictionary = poolInstance.Allocate();
		DebugAssertHelper.Assert(orderPreservingMultiDictionary.IsEmpty);
		return orderPreservingMultiDictionary;
	}

	public OrderPreservingMultiDictionary()
	{
	}

	private void EnsureDictionary()
	{
		dictionary = dictionary ?? PooledDictionary<K, object>.GetInstance();
	}

	public void Add(K k, V v)
	{
		if (!IsEmpty && dictionary.TryGetValue(k, out var value))
		{
			if (!(value is ArrayBuilder<V> arrayBuilder))
			{
				DebugAssertHelper.Assert(value is V, "Item must be either a V or an ArrayBuilder<V>");
				ArrayBuilder<V> arrayBuilder2 = new ArrayBuilder<V>(2);
				arrayBuilder2.Add((V)value);
				arrayBuilder2.Add(v);
				dictionary[k] = arrayBuilder2;
			}
			else
			{
				arrayBuilder.Add(v);
			}
		}
		else
		{
			EnsureDictionary();
			dictionary[k] = v;
		}
	}

	public void AddRange(K k, ImmutableArray<V> values)
	{
		if (values.IsEmpty)
		{
			return;
		}
		if (!IsEmpty && dictionary.TryGetValue(k, out var value))
		{
			if (!(value is ArrayBuilder<V> arrayBuilder))
			{
				DebugAssertHelper.Assert(value is V, "Item must be either a V or an ArrayBuilder<V>");
				ArrayBuilder<V> arrayBuilder2 = new ArrayBuilder<V>(1 + values.Length);
				arrayBuilder2.Add((V)value);
				arrayBuilder2.AddRange(values);
				dictionary[k] = arrayBuilder2;
			}
			else
			{
				arrayBuilder.AddRange(values);
			}
		}
		else
		{
			EnsureDictionary();
			if (values.Length == 1)
			{
				dictionary[k] = values[0];
				return;
			}
			ArrayBuilder<V> arrayBuilder2 = new ArrayBuilder<V>(values.Length);
			arrayBuilder2.AddRange(values);
			dictionary[k] = arrayBuilder2;
		}
	}

	public int GetCountForKey(K k)
	{
		if (!IsEmpty && dictionary.TryGetValue(k, out var value))
		{
			if (!(value is ArrayBuilder<V> arrayBuilder))
			{
				return 1;
			}
			return arrayBuilder.Count;
		}
		return 0;
	}

	public bool ContainsKey(K k)
	{
		if (!IsEmpty)
		{
			return dictionary.ContainsKey(k);
		}
		return false;
	}
}
