using System;
using System.Collections;
using System.Collections.Generic;

namespace Roslyn.Utilities;

internal static class SpecializedCollections
{
	private static class Empty
	{
		internal class Collection<T> : Enumerable<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			public static readonly ICollection<T> Instance = new Collection<T>();

			public int Count => 0;

			public bool IsReadOnly => true;

			protected Collection()
			{
			}

			public void Add(T item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(T item)
			{
				return false;
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException();
			}
		}

		internal class Dictionary<TKey, TValue> : Collection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
		{
			public new static readonly Dictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>();

			public ICollection<TKey> Keys => Collection<TKey>.Instance;

			IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

			IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

			public ICollection<TValue> Values => Collection<TValue>.Instance;

			public TValue this[TKey key]
			{
				get
				{
					throw new NotSupportedException();
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			private Dictionary()
			{
			}

			public void Add(TKey key, TValue value)
			{
				throw new NotSupportedException();
			}

			public bool ContainsKey(TKey key)
			{
				return false;
			}

			public bool Remove(TKey key)
			{
				throw new NotSupportedException();
			}

			public bool TryGetValue(TKey key, out TValue value)
			{
				value = default;
				return false;
			}
		}

		internal class Enumerable<T> : IEnumerable<T>, IEnumerable
		{
			private readonly IEnumerator<T> _enumerator = Enumerator<T>.Instance;

			public IEnumerator<T> GetEnumerator()
			{
				return _enumerator;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		internal class Enumerator : IEnumerator
		{
			public static readonly IEnumerator Instance = new Enumerator();

			public object Current
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			protected Enumerator()
			{
			}

			public bool MoveNext()
			{
				return false;
			}

			public void Reset()
			{
				throw new InvalidOperationException();
			}
		}

		internal class Enumerator<T> : Enumerator, IEnumerator<T>, IEnumerator, IDisposable
		{
			public new static readonly IEnumerator<T> Instance = new Enumerator<T>();

			public new T Current
			{
				get
				{
					throw new InvalidOperationException();
				}
			}

			protected Enumerator()
			{
			}

			public void Dispose()
			{
			}
		}

		internal class List<T> : Collection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
		{
			public new static readonly List<T> Instance = new List<T>();

			public T this[int index]
			{
				get
				{
					throw new ArgumentOutOfRangeException("index");
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			protected List()
			{
			}

			public int IndexOf(T item)
			{
				return -1;
			}

			public void Insert(int index, T item)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
		}

		internal class Set<T> : Collection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlySet<T>
		{
			public new static readonly Set<T> Instance = new Set<T>();

			protected Set()
			{
			}

			public new bool Add(T item)
			{
				throw new NotSupportedException();
			}

			public void ExceptWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public void IntersectWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				return !other.IsEmpty();
			}

			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				return false;
			}

			public bool IsSubsetOf(IEnumerable<T> other)
			{
				return true;
			}

			public bool IsSupersetOf(IEnumerable<T> other)
			{
				return other.IsEmpty();
			}

			public bool Overlaps(IEnumerable<T> other)
			{
				return false;
			}

			public bool SetEquals(IEnumerable<T> other)
			{
				return other.IsEmpty();
			}

			public void SymmetricExceptWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public void UnionWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}
		}
	}

	private static class ReadOnly
	{
		internal class Collection<TUnderlying, T> : Enumerable<TUnderlying, T>, ICollection<T>, IEnumerable<T>, IEnumerable where TUnderlying : ICollection<T>
		{
			public int Count => Underlying.Count;

			public bool IsReadOnly => true;

			public Collection(TUnderlying underlying)
				: base(underlying)
			{
			}

			public void Add(T item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(T item)
			{
				return Underlying.Contains(item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				Underlying.CopyTo(array, arrayIndex);
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException();
			}
		}

		internal class Enumerable<TUnderlying> : IEnumerable where TUnderlying : IEnumerable
		{
			protected readonly TUnderlying Underlying;

			public Enumerable(TUnderlying underlying)
			{
				Underlying = underlying;
			}

			public IEnumerator GetEnumerator()
			{
				return Underlying.GetEnumerator();
			}
		}

		internal class Enumerable<TUnderlying, T> : Enumerable<TUnderlying>, IEnumerable<T>, IEnumerable where TUnderlying : IEnumerable<T>
		{
			public Enumerable(TUnderlying underlying)
				: base(underlying)
			{
			}

			public new IEnumerator<T> GetEnumerator()
			{
				return Underlying.GetEnumerator();
			}
		}

		internal class Set<TUnderlying, T> : Collection<TUnderlying, T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlySet<T> where TUnderlying : ISet<T>
		{
			public Set(TUnderlying underlying)
				: base(underlying)
			{
			}

			public new bool Add(T item)
			{
				throw new NotSupportedException();
			}

			public void ExceptWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public void IntersectWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				return Underlying.IsProperSubsetOf(other);
			}

			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				return Underlying.IsProperSupersetOf(other);
			}

			public bool IsSubsetOf(IEnumerable<T> other)
			{
				return Underlying.IsSubsetOf(other);
			}

			public bool IsSupersetOf(IEnumerable<T> other)
			{
				return Underlying.IsSupersetOf(other);
			}

			public bool Overlaps(IEnumerable<T> other)
			{
				return Underlying.Overlaps(other);
			}

			public bool SetEquals(IEnumerable<T> other)
			{
				return Underlying.SetEquals(other);
			}

			public void SymmetricExceptWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}

			public void UnionWith(IEnumerable<T> other)
			{
				throw new NotSupportedException();
			}
		}
	}

	private static class Singleton
	{
		internal sealed class List<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
		{
			private readonly T _loneValue;

			public int Count => 1;

			public bool IsReadOnly => true;

			public T this[int index]
			{
				get
				{
					if (index != 0)
					{
						throw new IndexOutOfRangeException();
					}
					return _loneValue;
				}
				set
				{
					throw new NotSupportedException();
				}
			}

			public List(T value)
			{
				_loneValue = value;
			}

			public void Add(T item)
			{
				throw new NotSupportedException();
			}

			public void Clear()
			{
				throw new NotSupportedException();
			}

			public bool Contains(T item)
			{
				return EqualityComparer<T>.Default.Equals(_loneValue, item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				array[arrayIndex] = _loneValue;
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new Enumerator<T>(_loneValue);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public int IndexOf(T item)
			{
				if (object.Equals(_loneValue, item))
				{
					return 0;
				}
				return -1;
			}

			public void Insert(int index, T item)
			{
				throw new NotSupportedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
		}

		internal class Enumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
		{
			private readonly T _loneValue;

			private bool _moveNextCalled;

			public T Current => _loneValue;

			object IEnumerator.Current => _loneValue;

			public Enumerator(T value)
			{
				_loneValue = value;
				_moveNextCalled = false;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!_moveNextCalled)
				{
					_moveNextCalled = true;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				_moveNextCalled = false;
			}
		}
	}

	public static IEnumerator<T> EmptyEnumerator<T>()
	{
		return Empty.Enumerator<T>.Instance;
	}

	public static IEnumerable<T> EmptyEnumerable<T>()
	{
		return Empty.List<T>.Instance;
	}

	public static ICollection<T> EmptyCollection<T>()
	{
		return Empty.List<T>.Instance;
	}

	public static IList<T> EmptyList<T>()
	{
		return Empty.List<T>.Instance;
	}

	public static IReadOnlyList<T> EmptyReadOnlyList<T>()
	{
		return Empty.List<T>.Instance;
	}

	public static ISet<T> EmptySet<T>()
	{
		return Empty.Set<T>.Instance;
	}

	public static IReadOnlySet<T> EmptyReadOnlySet<T>()
	{
		return Empty.Set<T>.Instance;
	}

	public static IDictionary<TKey, TValue> EmptyDictionary<TKey, TValue>()
	{
		return Empty.Dictionary<TKey, TValue>.Instance;
	}

	public static IReadOnlyDictionary<TKey, TValue> EmptyReadOnlyDictionary<TKey, TValue>()
	{
		return Empty.Dictionary<TKey, TValue>.Instance;
	}

	public static IEnumerable<T> SingletonEnumerable<T>(T value)
	{
		return new Singleton.List<T>(value);
	}

	public static ICollection<T> SingletonCollection<T>(T value)
	{
		return new Singleton.List<T>(value);
	}

	public static IEnumerator<T> SingletonEnumerator<T>(T value)
	{
		return new Singleton.Enumerator<T>(value);
	}

	public static IList<T> SingletonList<T>(T value)
	{
		return new Singleton.List<T>(value);
	}

	public static IEnumerable<T> ReadOnlyEnumerable<T>(IEnumerable<T> values)
	{
		return new ReadOnly.Enumerable<IEnumerable<T>, T>(values);
	}

	public static ICollection<T> ReadOnlyCollection<T>(ICollection<T> collection)
	{
		if (collection != null && collection.Count != 0)
		{
			return new ReadOnly.Collection<ICollection<T>, T>(collection);
		}
		return EmptyCollection<T>();
	}

	public static ISet<T> ReadOnlySet<T>(ISet<T> set)
	{
		if (set != null && set.Count != 0)
		{
			return new ReadOnly.Set<ISet<T>, T>(set);
		}
		return EmptySet<T>();
	}

	public static IReadOnlySet<T> StronglyTypedReadOnlySet<T>(ISet<T> set)
	{
		if (set != null && set.Count != 0)
		{
			return new ReadOnly.Set<ISet<T>, T>(set);
		}
		return EmptyReadOnlySet<T>();
	}
}
