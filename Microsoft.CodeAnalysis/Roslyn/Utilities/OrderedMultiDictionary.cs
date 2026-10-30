using System.Collections;
using System.Collections.Generic;

namespace Roslyn.Utilities;

internal sealed class OrderedMultiDictionary<K, V> : IEnumerable<KeyValuePair<K, SetWithInsertionOrder<V>>>, IEnumerable
{
	private readonly Dictionary<K, SetWithInsertionOrder<V>> _dictionary;

	private readonly List<K> _keys;

	public int Count => _dictionary.Count;

	public IEnumerable<K> Keys => _keys;

	public SetWithInsertionOrder<V> this[K k]
	{
		get
		{
			if (!_dictionary.TryGetValue(k, out var value))
			{
				return new SetWithInsertionOrder<V>();
			}
			return value;
		}
	}

	public OrderedMultiDictionary()
	{
		_dictionary = new Dictionary<K, SetWithInsertionOrder<V>>();
		_keys = new List<K>();
	}

	public void Add(K k, V v)
	{
		if (!_dictionary.TryGetValue(k, out var value))
		{
			_keys.Add(k);
			value = new SetWithInsertionOrder<V>();
		}
		value.Add(v);
		_dictionary[k] = value;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<K, SetWithInsertionOrder<V>>> GetEnumerator()
	{
		foreach (K key in _keys)
		{
			yield return new KeyValuePair<K, SetWithInsertionOrder<V>>(key, _dictionary[key]);
		}
	}
}
