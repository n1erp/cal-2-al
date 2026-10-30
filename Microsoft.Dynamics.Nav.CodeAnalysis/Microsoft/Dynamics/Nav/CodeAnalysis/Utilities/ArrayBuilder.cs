using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

[DebuggerDisplay("Count = {Count,nq}")]
[DebuggerTypeProxy(typeof(ArrayBuilder<>.DebuggerProxy))]
internal sealed class ArrayBuilder<T> : IReadOnlyCollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>
{
	private sealed class DebuggerProxy
	{
		private readonly ArrayBuilder<T> builder;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public T[] A
		{
			get
			{
				T[] array = new T[builder.Count];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = builder[i];
				}
				return array;
			}
		}

		public DebuggerProxy(ArrayBuilder<T> builder)
		{
			this.builder = builder;
		}
	}

	internal struct Enumerator(ArrayBuilder<T> builder) : IEnumerator<T>, IDisposable, IEnumerator
	{
		private readonly ArrayBuilder<T> builder = builder;

		private int index = -1;

		public T Current => builder[index];

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			index++;
			return index < builder.Count;
		}

		public void Dispose()
		{
		}

		public void Reset()
		{
			index = -1;
		}
	}

	private readonly ImmutableArray<T>.Builder builder;

	private readonly ObjectPool<ArrayBuilder<T>> pool;

	private static readonly ObjectPool<ArrayBuilder<T>> PoolInstance = CreatePool();

	public int Count => builder.Count;

	public T this[int index]
	{
		get
		{
			return builder[index];
		}
		set
		{
			builder[index] = value;
		}
	}

	public ArrayBuilder(int size)
	{
		builder = ImmutableArray.CreateBuilder<T>(size);
	}

	public ArrayBuilder()
		: this(8)
	{
	}

	private ArrayBuilder(ObjectPool<ArrayBuilder<T>> pool)
		: this()
	{
		this.pool = pool;
	}

	public ImmutableArray<T> ToImmutable()
	{
		return builder.ToImmutable();
	}

	public void Add(T item)
	{
		builder.Add(item);
	}

	public void Insert(int index, T item)
	{
		builder.Insert(index, item);
	}

	public void EnsureCapacity(int capacity)
	{
		if (builder.Capacity < capacity)
		{
			builder.Capacity = capacity;
		}
	}

	public void Clear()
	{
		builder.Clear();
	}

	public bool Contains(T item)
	{
		return builder.Contains(item);
	}

	public int IndexOf(T item)
	{
		return builder.IndexOf(item);
	}

	public void RemoveAt(int index)
	{
		builder.RemoveAt(index);
	}

	public void RemoveLast()
	{
		builder.RemoveAt(builder.Count - 1);
	}

	public void ReverseContents()
	{
		builder.Reverse();
	}

	public void Sort()
	{
		builder.Sort();
	}

	public void Sort(IComparer<T> comparer)
	{
		builder.Sort(comparer);
	}

	public void Sort(int startIndex, IComparer<T> comparer)
	{
		builder.Sort(startIndex, builder.Count - startIndex, comparer);
	}

	public T[] ToArray()
	{
		return builder.ToArray();
	}

	public void CopyTo(T[] array, int start)
	{
		builder.CopyTo(array, start);
	}

	public T Last()
	{
		return builder[builder.Count - 1];
	}

	public T First()
	{
		return builder[0];
	}

	public bool Any()
	{
		return builder.Count > 0;
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
		ImmutableArray<T> result = ((builder.Capacity != Count) ? ToImmutable() : builder.MoveToImmutable());
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
		ObjectPool<ArrayBuilder<T>> objectPool = pool;
		if (objectPool != null && Count < 128)
		{
			if (Count != 0)
			{
				Clear();
			}
			objectPool.Free(this);
		}
	}

	public static ArrayBuilder<T> GetInstance()
	{
		ArrayBuilder<T> arrayBuilder = PoolInstance.Allocate();
		DebugAssertHelper.Assert(arrayBuilder.Count == 0);
		return arrayBuilder;
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
		builder.AddRange(items.builder);
	}

	public void AddRange<TDerived>(ArrayBuilder<TDerived> items) where TDerived : T
	{
		builder.AddRange(items.builder);
	}

	public void AddRange(ImmutableArray<T> items)
	{
		builder.AddRange(items);
	}

	public void AddRange(ImmutableArray<T> items, int length)
	{
		builder.AddRange(items, length);
	}

	public void AddRange(IEnumerable<T> items)
	{
		builder.AddRange(items);
	}

	public void AddRange(params T[] items)
	{
		builder.AddRange(items);
	}

	public void AddMany(T item, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Add(item);
		}
	}
}
