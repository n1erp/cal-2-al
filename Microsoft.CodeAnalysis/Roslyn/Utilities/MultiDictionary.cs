using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslyn.Utilities;

internal sealed class MultiDictionary<K, V> : IEnumerable<KeyValuePair<K, MultiDictionary<K, V>.ValueSet>>, IEnumerable
{
	public struct ValueSet : IEnumerable<V>, IEnumerable
	{
		public struct Enumerator : IEnumerator<V>, IEnumerator, IDisposable
		{
			private readonly V _value;

			private ImmutableHashSet<V>.Enumerator _values;

			private int _count;

			object IEnumerator.Current => Current;

			public V Current
			{
				get
				{
					if (_count <= 1)
					{
						return _value;
					}
					return _values.Current;
				}
			}

			public Enumerator(ValueSet v)
			{
				if (v._value == null)
				{
					_value = default;
					_values = default;
					_count = 0;
				}
				else if (!(v._value is ImmutableHashSet<V> immutableHashSet))
				{
					_value = (V)v._value;
					_values = default;
					_count = 1;
				}
				else
				{
					_value = default;
					_values = immutableHashSet.GetEnumerator();
					_count = immutableHashSet.Count;
				}
			}

			public void Dispose()
			{
			}

			public void Reset()
			{
				throw new NotSupportedException();
			}

			public bool MoveNext()
			{
				switch (_count)
				{
				case 0:
					return false;
				case 1:
					_count = 0;
					return true;
				default:
					if (_values.MoveNext())
					{
						return true;
					}
					_count = 0;
					return false;
				}
			}
		}

		private readonly object _value;

		public int Count
		{
			get
			{
				if (_value == null)
				{
					return 0;
				}
				if (!(_value is ImmutableHashSet<V> immutableHashSet))
				{
					return 1;
				}
				return immutableHashSet.Count;
			}
		}

		public ValueSet(object value)
		{
			_value = value;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		IEnumerator<V> IEnumerable<V>.GetEnumerator()
		{
			return GetEnumerator();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this);
		}

		public ValueSet Add(V v)
		{
			ImmutableHashSet<V> immutableHashSet = _value as ImmutableHashSet<V>;
			if (immutableHashSet == null)
			{
				if (ImmutableHashSet<V>.Empty.KeyComparer.Equals((V)_value, v))
				{
					return this;
				}
				immutableHashSet = ImmutableHashSet.Create((V)_value);
			}
			return new ValueSet(immutableHashSet.Add(v));
		}

		public V Single()
		{
			return (V)_value;
		}
	}

	private readonly Dictionary<K, ValueSet> _dictionary;

	public int Count => _dictionary.Count;

	public IEnumerable<K> Keys => _dictionary.Keys;

	public ValueSet this[K k]
	{
		get
		{
			if (!_dictionary.TryGetValue(k, out var value))
			{
				return default;
			}
			return value;
		}
	}

	public MultiDictionary()
	{
		_dictionary = new Dictionary<K, ValueSet>();
	}

	public MultiDictionary(IEqualityComparer<K> comparer)
	{
		_dictionary = new Dictionary<K, ValueSet>(comparer);
	}

	public MultiDictionary(int capacity, IEqualityComparer<K> comparer)
	{
		_dictionary = new Dictionary<K, ValueSet>(capacity, comparer);
	}

	public void Add(K k, V v)
	{
		_dictionary[k] = (_dictionary.TryGetValue(k, out var value) ? value.Add(v) : new ValueSet(v));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<K, ValueSet>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	public bool ContainsKey(K k)
	{
		return _dictionary.ContainsKey(k);
	}

	internal void Clear()
	{
		_dictionary.Clear();
	}

	public void Remove(K key)
	{
		_dictionary.Remove(key);
	}
}
