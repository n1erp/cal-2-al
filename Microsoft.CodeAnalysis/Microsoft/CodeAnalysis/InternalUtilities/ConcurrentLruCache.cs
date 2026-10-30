using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.InternalUtilities;

internal class ConcurrentLruCache<K, V>
{
	private struct CacheValue
	{
		public V Value;

		public LinkedListNode<K> Node;
	}

	private readonly int _capacity;

	private readonly Dictionary<K, CacheValue> _cache;

	private readonly LinkedList<K> _nodeList;

	private readonly object _lockObject = new object();

	internal IEnumerable<KeyValuePair<K, V>> TestingEnumerable
	{
		get
		{
			lock (_lockObject)
			{
				KeyValuePair<K, V>[] array = new KeyValuePair<K, V>[_cache.Count];
				int num = 0;
				foreach (K node in _nodeList)
				{
					array[num++] = new KeyValuePair<K, V>(node, _cache[node].Value);
				}
				return array;
			}
		}
	}

	public V this[K key]
	{
		get
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			throw new KeyNotFoundException();
		}
		set
		{
			lock (_lockObject)
			{
				UnsafeAdd(key, value, throwExceptionIfKeyExists: false);
			}
		}
	}

	public ConcurrentLruCache(int capacity)
	{
		if (capacity <= 0)
		{
			throw new ArgumentOutOfRangeException("capacity");
		}
		_capacity = capacity;
		_cache = new Dictionary<K, CacheValue>(capacity);
		_nodeList = new LinkedList<K>();
	}

	public ConcurrentLruCache(KeyValuePair<K, V>[] array)
		: this(array.Length)
	{
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<K, V> keyValuePair = array[i];
			UnsafeAdd(keyValuePair.Key, keyValuePair.Value, throwExceptionIfKeyExists: true);
		}
	}

	public void Add(K key, V value)
	{
		lock (_lockObject)
		{
			UnsafeAdd(key, value, throwExceptionIfKeyExists: true);
		}
	}

	private void MoveNodeToTop(LinkedListNode<K> node)
	{
		if (_nodeList.First != node)
		{
			_nodeList.Remove(node);
			_nodeList.AddFirst(node);
		}
	}

	private void UnsafeEvictLastNode()
	{
		LinkedListNode<K> last = _nodeList.Last;
		_nodeList.Remove(last);
		_cache.Remove(last.Value);
	}

	private void UnsafeAddNodeToTop(K key, V value)
	{
		LinkedListNode<K> node = new LinkedListNode<K>(key);
		_cache.Add(key, new CacheValue
		{
			Node = node,
			Value = value
		});
		_nodeList.AddFirst(node);
	}

	private void UnsafeAdd(K key, V value, bool throwExceptionIfKeyExists)
	{
		if (_cache.TryGetValue(key, out var value2))
		{
			if (throwExceptionIfKeyExists)
			{
				throw new ArgumentException("Key already exists", "key");
			}
			if (!value2.Value.Equals(value))
			{
				value2.Value = value;
				_cache[key] = value2;
				MoveNodeToTop(value2.Node);
			}
		}
		else
		{
			if (_cache.Count == _capacity)
			{
				UnsafeEvictLastNode();
			}
			UnsafeAddNodeToTop(key, value);
		}
	}

	public bool TryGetValue(K key, out V value)
	{
		lock (_lockObject)
		{
			return UnsafeTryGetValue(key, out value);
		}
	}

	public bool UnsafeTryGetValue(K key, out V value)
	{
		if (_cache.TryGetValue(key, out var value2))
		{
			MoveNodeToTop(value2.Node);
			value = value2.Value;
			return true;
		}
		value = default;
		return false;
	}

	public V GetOrAdd(K key, V value)
	{
		lock (_lockObject)
		{
			if (UnsafeTryGetValue(key, out var value2))
			{
				return value2;
			}
			UnsafeAdd(key, value, throwExceptionIfKeyExists: true);
			return value;
		}
	}

	public V GetOrAdd(K key, Func<V> creator)
	{
		lock (_lockObject)
		{
			if (UnsafeTryGetValue(key, out var value))
			{
				return value;
			}
			V val = creator();
			UnsafeAdd(key, val, throwExceptionIfKeyExists: true);
			return val;
		}
	}

	public V GetOrAdd<T>(K key, T arg, Func<T, V> creator)
	{
		lock (_lockObject)
		{
			if (UnsafeTryGetValue(key, out var value))
			{
				return value;
			}
			V val = creator(arg);
			UnsafeAdd(key, val, throwExceptionIfKeyExists: true);
			return val;
		}
	}
}
