using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal sealed class MultiDictionary<K, V> : IEnumerable<KeyValuePair<K, MultiDictionary<K, V>.ValueSet>>, IEnumerable
{
	public struct ValueSet : IEnumerable<V>, IEnumerable
	{
		public struct Enumerator : IEnumerator<V>, IDisposable, IEnumerator
		{
			private readonly V value;

			private ImmutableHashSet<V>.Enumerator values;

			private int count;

			object IEnumerator.Current => Current;

			public V Current
			{
				get
				{
					if (count <= 1)
					{
						return value;
					}
					return values.Current;
				}
			}

			public Enumerator(ValueSet v)
			{
				if (v.value == null)
				{
					value = default;
					values = default;
					count = 0;
					return;
				}
				if (!(v.value is ImmutableHashSet<V> immutableHashSet))
				{
					value = (V)v.value;
					values = default;
					count = 1;
				}
				else
				{
					value = default;
					values = immutableHashSet.GetEnumerator();
					count = immutableHashSet.Count;
					DebugAssertHelper.Assert(count > 1);
				}
				DebugAssertHelper.Assert(count == v.Count);
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
				switch (count)
				{
				case 0:
					return false;
				case 1:
					count = 0;
					return true;
				default:
					if (values.MoveNext())
					{
						return true;
					}
					count = 0;
					return false;
				}
			}
		}

		private readonly object value;

		public int Count
		{
			get
			{
				if (value == null)
				{
					return 0;
				}
				if (!(value is ImmutableHashSet<V> immutableHashSet))
				{
					return 1;
				}
				return immutableHashSet.Count;
			}
		}

		public ValueSet(object value)
		{
			this.value = value;
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
			DebugAssertHelper.Assert(value != null);
			ImmutableHashSet<V> immutableHashSet = value as ImmutableHashSet<V>;
			if (immutableHashSet == null)
			{
				if (ImmutableHashSet<V>.Empty.KeyComparer.Equals((V)value, v))
				{
					return this;
				}
				immutableHashSet = ImmutableHashSet.Create((V)value);
			}
			return new ValueSet(immutableHashSet.Add(v));
		}

		public V Single()
		{
			DebugAssertHelper.Assert(value is V);
			return (V)value;
		}
	}

	private readonly Dictionary<K, ValueSet> dictionary;

	public int Count => dictionary.Count;

	public IEnumerable<K> Keys => dictionary.Keys;

	public ValueSet this[K k]
	{
		get
		{
			if (!dictionary.TryGetValue(k, out var value))
			{
				return default;
			}
			return value;
		}
	}

	public MultiDictionary()
	{
		dictionary = new Dictionary<K, ValueSet>();
	}

	public MultiDictionary(IEqualityComparer<K> comparer)
	{
		dictionary = new Dictionary<K, ValueSet>(comparer);
	}

	public MultiDictionary(int capacity, IEqualityComparer<K> comparer)
	{
		dictionary = new Dictionary<K, ValueSet>(capacity, comparer);
	}

	public void Add(K k, V v)
	{
		dictionary[k] = (dictionary.TryGetValue(k, out var value) ? value.Add(v) : new ValueSet(v));
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerator<KeyValuePair<K, ValueSet>> GetEnumerator()
	{
		return dictionary.GetEnumerator();
	}

	public bool ContainsKey(K k)
	{
		return dictionary.ContainsKey(k);
	}

	internal void Clear()
	{
		dictionary.Clear();
	}
}
