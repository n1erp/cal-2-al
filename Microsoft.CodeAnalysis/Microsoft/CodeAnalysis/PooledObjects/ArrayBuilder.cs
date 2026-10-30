using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.PooledObjects;

[DebuggerDisplay("Count = {Count,nq}")]
[DebuggerTypeProxy(typeof(ArrayBuilder<>.DebuggerProxy))]
internal sealed class ArrayBuilder<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
{
	private sealed class DebuggerProxy
	{
		private readonly ArrayBuilder<T> _builder;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] A
		{
			get
			{
				T[] array = new T[_builder.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = _builder[i];
				}
				return array;
			}
		}

		public DebuggerProxy(ArrayBuilder<T> builder)
		{
			_builder = builder;
		}
	}

	internal struct Enumerator(ArrayBuilder<T> builder) : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly ArrayBuilder<T> _builder = builder;

		private int _index = -1;

		public T Current => _builder[_index];

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			_index++;
			return _index < _builder.Count;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			_index = -1;
		}
	}

	private readonly ImmutableArray<T>.Builder _builder;

	private readonly ObjectPool<ArrayBuilder<T>> _pool;

	private static readonly ObjectPool<ArrayBuilder<T>> s_poolInstance = CreatePool();

	public int Count
	{
		get
		{
			return _builder.Count;
		}
		set
		{
			_builder.Count = value;
		}
	}

	public T this[int index]
	{
		get
		{
			return _builder[index];
		}
		set
		{
			_builder[index] = value;
		}
	}

	public ArrayBuilder(int size)
	{
		_builder = ImmutableArray.CreateBuilder<T>(size);
	}

	public ArrayBuilder()
		: this(8)
	{
	}

	private ArrayBuilder(ObjectPool<ArrayBuilder<T>> pool)
		: this()
	{
		_pool = pool;
	}

	public ImmutableArray<T> ToImmutable()
	{
		return _builder.ToImmutable();
	}

	public void SetItem(int index, T value)
	{
		while (index > _builder.Count)
		{
			_builder.Add(default);
		}
		if (index == _builder.Count)
		{
			_builder.Add(value);
		}
		else
		{
			_builder[index] = value;
		}
	}

	public void Add(T item)
	{
		_builder.Add(item);
	}

	public void Insert(int index, T item)
	{
		_builder.Insert(index, item);
	}

	public void EnsureCapacity(int capacity)
	{
		if (_builder.Capacity < capacity)
		{
			_builder.Capacity = capacity;
		}
	}

	public void Clear()
	{
		_builder.Clear();
	}

	public bool Contains(T item)
	{
		return _builder.Contains(item);
	}

	public int IndexOf(T item)
	{
		return _builder.IndexOf(item);
	}

	public int IndexOf(T item, IEqualityComparer<T> equalityComparer)
	{
		return _builder.IndexOf(item, 0, _builder.Count, equalityComparer);
	}

	public int IndexOf(T item, int startIndex, int count)
	{
		return _builder.IndexOf(item, startIndex, count);
	}

	public int FindIndex(Predicate<T> match)
	{
		return FindIndex(0, Count, match);
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		return FindIndex(startIndex, Count - startIndex, match);
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		int num = startIndex + count;
		for (int i = startIndex; i < num; i++)
		{
			if (match(_builder[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public void RemoveAt(int index)
	{
		_builder.RemoveAt(index);
	}

	public void RemoveLast()
	{
		_builder.RemoveAt(_builder.Count - 1);
	}

	public void ReverseContents()
	{
		_builder.Reverse();
	}

	public void Sort()
	{
		_builder.Sort();
	}

	public void Sort(IComparer<T> comparer)
	{
		_builder.Sort(comparer);
	}

	public void Sort(Comparison<T> compare)
	{
		Sort(Comparer<T>.Create(compare));
	}

	public void Sort(int startIndex, IComparer<T> comparer)
	{
		_builder.Sort(startIndex, _builder.Count - startIndex, comparer);
	}

	public T[] ToArray()
	{
		return _builder.ToArray();
	}

	public void CopyTo(T[] array, int start)
	{
		_builder.CopyTo(array, start);
	}

	public T Last()
	{
		return _builder[_builder.Count - 1];
	}

	public T First()
	{
		return _builder[0];
	}

	public bool Any()
	{
		return _builder.Count > 0;
	}

	public ImmutableArray<T> ToImmutableOrNull()
	{
		if (Count == 0)
		{
			return default;
		}
		return ToImmutable();
	}

	public ImmutableArray<U> ToDowncastedImmutable<U>() where U : T
	{
		if (Count == 0)
		{
			return ImmutableArray<U>.Empty;
		}
		ArrayBuilder<U> instance = ArrayBuilder<U>.GetInstance(Count);
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				instance.Add((U)(object)current);
			}
		}
		return instance.ToImmutableAndFree();
	}

	public ImmutableArray<T> ToImmutableAndFree()
	{
		ImmutableArray<T> result = ((_builder.Capacity != Count) ? ToImmutable() : _builder.MoveToImmutable());
		Free();
		return result;
	}

	public T[] ToArrayAndFree()
	{
		T[] result = ToArray();
		Free();
		return result;
	}

	public void Free()
	{
		ObjectPool<ArrayBuilder<T>> pool = _pool;
		if (pool != null && _builder.Capacity < 128)
		{
			if (Count != 0)
			{
				Clear();
			}
			pool.Free(this);
		}
	}

	public static ArrayBuilder<T> GetInstance()
	{
		return s_poolInstance.Allocate();
	}

	public static ArrayBuilder<T> GetInstance(int capacity)
	{
		ArrayBuilder<T> instance = GetInstance();
		instance.EnsureCapacity(capacity);
		return instance;
	}

	public static ArrayBuilder<T> GetInstance(int capacity, T fillWithValue)
	{
		ArrayBuilder<T> instance = GetInstance();
		instance.EnsureCapacity(capacity);
		for (int i = 0; i < capacity; i++)
		{
			instance.Add(fillWithValue);
		}
		return instance;
	}

	public static ObjectPool<ArrayBuilder<T>> CreatePool()
	{
		return CreatePool(128);
	}

	public static ObjectPool<ArrayBuilder<T>> CreatePool(int size)
	{
		ObjectPool<ArrayBuilder<T>> pool = null;
		pool = new ObjectPool<ArrayBuilder<T>>(() => new ArrayBuilder<T>(pool), size);
		return pool;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	internal Dictionary<K, ImmutableArray<T>> ToDictionary<K>(Func<T, K> keySelector, IEqualityComparer<K> comparer = null)
	{
		if (Count == 1)
		{
			Dictionary<K, ImmutableArray<T>> dictionary = new Dictionary<K, ImmutableArray<T>>(1, comparer);
			T val = this[0];
			dictionary.Add(keySelector(val), ImmutableArray.Create(val));
			return dictionary;
		}
		if (Count == 0)
		{
			return new Dictionary<K, ImmutableArray<T>>(comparer);
		}
		Dictionary<K, ArrayBuilder<T>> dictionary2 = new Dictionary<K, ArrayBuilder<T>>(Count, comparer);
		for (int i = 0; i < Count; i++)
		{
			T val2 = this[i];
			K key = keySelector(val2);
			if (!dictionary2.TryGetValue(key, out var value))
			{
				value = GetInstance();
				dictionary2.Add(key, value);
			}
			value.Add(val2);
		}
		Dictionary<K, ImmutableArray<T>> dictionary3 = new Dictionary<K, ImmutableArray<T>>(dictionary2.Count, comparer);
		foreach (KeyValuePair<K, ArrayBuilder<T>> item in dictionary2)
		{
			dictionary3.Add(item.Key, item.Value.ToImmutableAndFree());
		}
		return dictionary3;
	}

	public void AddRange(ArrayBuilder<T> items)
	{
		_builder.AddRange(items._builder);
	}

	public void AddRange<U>(ArrayBuilder<U> items) where U : T
	{
		_builder.AddRange(items._builder);
	}

	public void AddRange(ImmutableArray<T> items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(ImmutableArray<T> items, int length)
	{
		_builder.AddRange(items, length);
	}

	public void AddRange<S>(ImmutableArray<S> items) where S : class, T
	{
		AddRange(ImmutableArray<T>.CastUp(items));
	}

	public void AddRange(T[] items, int start, int length)
	{
		int i = start;
		for (int num = start + length; i < num; i++)
		{
			Add(items[i]);
		}
	}

	public void AddRange(IEnumerable<T> items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(params T[] items)
	{
		_builder.AddRange(items);
	}

	public void AddRange(T[] items, int length)
	{
		_builder.AddRange(items, length);
	}

	public void Clip(int limit)
	{
		_builder.Count = limit;
	}

	public void ZeroInit(int count)
	{
		_builder.Clear();
		_builder.Count = count;
	}

	public void AddMany(T item, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Add(item);
		}
	}

	public void RemoveDuplicates()
	{
		PooledHashSet<T> instance = PooledHashSet<T>.GetInstance();
		int num = 0;
		for (int i = 0; i < Count; i++)
		{
			if (instance.Add(this[i]))
			{
				this[num] = this[i];
				num++;
			}
		}
		Clip(num);
		instance.Free();
	}

	public ImmutableArray<S> SelectDistinct<S>(Func<T, S> selector)
	{
		ArrayBuilder<S> instance = ArrayBuilder<S>.GetInstance(Count);
		PooledHashSet<S> instance2 = PooledHashSet<S>.GetInstance();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				T current = enumerator.Current;
				S item = selector(current);
				if (instance2.Add(item))
				{
					instance.Add(item);
				}
			}
		}
		instance2.Free();
		return instance.ToImmutableAndFree();
	}
}
