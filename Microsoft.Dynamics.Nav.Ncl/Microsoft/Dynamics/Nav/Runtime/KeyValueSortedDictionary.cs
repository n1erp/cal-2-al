using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class KeyValueSortedDictionary<TKey, TValue> : IEquatable<KeyValueSortedDictionary<TKey, TValue>>, IComparer<Tuple<TKey, TValue>>
{
	private bool hashCodeCalculated;

	private int hashCode;

	private readonly Tuple<TKey, TValue>[] items;

	private Dictionary<TKey, TValue> lookup;

	public TValue this[TKey key]
	{
		get
		{
			EnsureLookupCreated();
			return lookup[key];
		}
	}

	public Tuple<TKey, TValue>[] Items
	{
		[DebuggerStepThrough]
		get
		{
			return items;
		}
	}

	public KeyValueSortedDictionary(Dictionary<TKey, TValue> lookup, bool copy = false)
	{
		if (lookup == null)
		{
			throw new ArgumentNullException("lookup");
		}
		if (!copy)
		{
			this.lookup = lookup;
		}
		items = new Tuple<TKey, TValue>[lookup.Count];
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> item in lookup)
		{
			items[num] = Tuple.Create(item.Key, item.Value);
			num++;
		}
		SortItems();
	}

	public KeyValueSortedDictionary(params Tuple<TKey, TValue>[] items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		this.items = items;
		SortItems();
	}

	public KeyValueSortedDictionary(IEnumerable<Tuple<TKey, TValue>> items)
	{
		if (items == null)
		{
			throw new ArgumentNullException("items");
		}
		this.items = items.ToArray();
		SortItems();
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		EnsureLookupCreated();
		return lookup.TryGetValue(key, out value);
	}

	public Dictionary<TKey, TValue> ToDictionary()
	{
		return Items.ToDictionary((Tuple<TKey, TValue> t) => t.Item1, (Tuple<TKey, TValue> t) => t.Item2);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as KeyValueSortedDictionary<TKey, TValue>);
	}

	public override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			int h = 0;
			Tuple<TKey, TValue>[] array = items;
			foreach (Tuple<TKey, TValue> tuple in array)
			{
				h = HashCodeHelper.CombineHashCodes(h, tuple.Item1.GetHashCode(), tuple.Item2.GetHashCode());
			}
			hashCode = h;
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public bool Equals(KeyValueSortedDictionary<TKey, TValue> other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null)
		{
			return false;
		}
		if (items == other.items)
		{
			return true;
		}
		if (items.Length != other.items.Length || GetHashCode() != other.GetHashCode())
		{
			return false;
		}
		for (int i = 0; i < items.Length; i++)
		{
			Tuple<TKey, TValue> tuple = items[i];
			Tuple<TKey, TValue> tuple2 = other.items[i];
			if (!object.Equals(tuple.Item1, tuple2.Item1) || !object.Equals(tuple.Item2, tuple2.Item2))
			{
				return false;
			}
		}
		return true;
	}

	private void SortItems()
	{
		switch (items.Length)
		{
		case 2:
		{
			Tuple<TKey, TValue> tuple = items[0];
			Tuple<TKey, TValue> tuple2 = items[1];
			if (Compare(tuple, tuple2) > 0)
			{
				Tuple<TKey, TValue> tuple3 = tuple;
				items[0] = tuple2;
				items[1] = tuple3;
			}
			break;
		}
		default:
			Array.Sort(items, this);
			break;
		case 0:
		case 1:
			break;
		}
	}

	private void EnsureLookupCreated()
	{
		if (lookup == null)
		{
			lookup = ToDictionary();
		}
	}

	public abstract int Compare(Tuple<TKey, TValue> x, Tuple<TKey, TValue> y);
}
