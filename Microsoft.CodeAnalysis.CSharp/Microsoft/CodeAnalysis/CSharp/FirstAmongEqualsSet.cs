using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class FirstAmongEqualsSet<T> : IEnumerable<T>, IEnumerable
{
	private readonly HashSet<T> _hashSet;

	private readonly Dictionary<T, T> _dictionary;

	private readonly Func<T, T, int> _canonicalComparer;

	public FirstAmongEqualsSet(IEnumerable<T> items, IEqualityComparer<T> equalityComparer, Func<T, T, int> canonicalComparer)
	{
		_canonicalComparer = canonicalComparer;
		_dictionary = new Dictionary<T, T>(equalityComparer);
		_hashSet = new HashSet<T>(equalityComparer);
		UnionWith(items);
	}

	public void UnionWith(IEnumerable<T> items)
	{
		foreach (T item in items)
		{
			if (!_dictionary.TryGetValue(item, out var value) || IsMoreCanonical(item, value))
			{
				_dictionary[item] = item;
			}
		}
	}

	private bool IsMoreCanonical(T newItem, T oldItem)
	{
		return _canonicalComparer(newItem, oldItem) > 0;
	}

	public void IntersectWith(IEnumerable<T> items)
	{
		_hashSet.UnionWith(items);
		foreach (T item in _dictionary.Keys.ToList())
		{
			if (!_hashSet.Contains(item))
			{
				_dictionary.Remove(item);
			}
		}
		foreach (T item2 in _hashSet)
		{
			if (_dictionary.TryGetValue(item2, out var value) && IsMoreCanonical(item2, value))
			{
				_dictionary[item2] = item2;
			}
		}
		_hashSet.Clear();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _dictionary.Values.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
