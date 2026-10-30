using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class SpecializedCollections
{
	private class Empty
	{
		[SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
		internal class Array<T>
		{
			public static readonly T[] Instance = new T[0];

			private Array()
			{
			}
		}

		internal class Enumerator : IEnumerator
		{
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

		internal class Enumerator<T> : Enumerator, IEnumerator<T>, IDisposable, IEnumerator
		{
			public static readonly IEnumerator<T> Instance = new Enumerator<T>();

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

		internal class Set<T> : Collection<T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
		{
			public static readonly ISet<T> Instance = new Set<T>();

			private Set()
			{
			}

			public new bool Add(T item)
			{
				throw new NotImplementedException();
			}

			public void ExceptWith(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public void IntersectWith(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool IsProperSubsetOf(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool IsProperSupersetOf(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool IsSubsetOf(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool IsSupersetOf(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool Overlaps(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public bool SetEquals(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public void SymmetricExceptWith(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public void UnionWith(IEnumerable<T> other)
			{
				throw new NotImplementedException();
			}

			public new IEnumerator GetEnumerator()
			{
				return Instance.GetEnumerator();
			}
		}

		internal class List<T> : Collection<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyList<T>, IReadOnlyCollection<T>
		{
			public static readonly List<T> Instance = new List<T>();

			public new T this[int index]
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

			public new int IndexOf(T item)
			{
				return -1;
			}

			public new void Insert(int index, T item)
			{
				throw new NotSupportedException();
			}

			public new void RemoveAt(int index)
			{
				throw new NotSupportedException();
			}
		}

		internal class Dictionary<TKey, TValue> : Collection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
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

			public static readonly IDictionary<TKey, TValue> Instance = new Dictionary<TKey, TValue>();

			public ICollection<TKey> Keys => Collection<TKey>.Instance;

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
			private readonly IEnumerator<T> enumerator = Enumerator<T>.Instance;

			public IEnumerator<T> GetEnumerator()
			{
				return enumerator;
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		private Empty()
		{
		}
	}

	private static class Singleton
	{
		internal sealed class Collection<T> : ICollection<T>, IEnumerable<T>, IEnumerable, IReadOnlyCollection<T>
		{
			private readonly T loneValue;

			public int Count => 1;

			public bool IsReadOnly => true;

			public Collection(T value)
			{
				loneValue = value;
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
				return EqualityComparer<T>.Default.Equals(loneValue, item);
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				array[arrayIndex] = loneValue;
			}

			public bool Remove(T item)
			{
				throw new NotSupportedException();
			}

			public IEnumerator<T> GetEnumerator()
			{
				return new Enumerator<T>(loneValue);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}

		internal class Enumerator<T> : IEnumerator<T>, IDisposable, IEnumerator
		{
			private readonly T loneValue;

			private bool moveNextCalled;

			public T Current => loneValue;

			object IEnumerator.Current => loneValue;

			public Enumerator(T value)
			{
				loneValue = value;
				moveNextCalled = false;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (!moveNextCalled)
				{
					moveNextCalled = true;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				moveNextCalled = false;
			}
		}
	}

	private class ReadOnly
	{
		internal class Set<TUnderlying, T> : Collection<TUnderlying, T>, ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable where TUnderlying : ISet<T>
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

		private ReadOnly()
		{
		}
	}

	public static readonly byte[] EmptyBytes = EmptyArray<byte>();

	public static readonly object[] EmptyObjects = EmptyArray<object>();

	public static T[] EmptyArray<T>()
	{
		return Empty.Array<T>.Instance;
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

	public static ISet<T> EmptySet<T>()
	{
		return Empty.Set<T>.Instance;
	}

	public static IReadOnlyList<T> EmptyReadOnlyList<T>()
	{
		return Empty.List<T>.Instance;
	}

	public static IDictionary<TKey, TValue> EmptyDictionary<TKey, TValue>()
	{
		return Empty.Dictionary<TKey, TValue>.Instance;
	}

	public static IEnumerable<T> SingletonEnumerable<T>(T value)
	{
		return new Singleton.Collection<T>(value);
	}

	public static ISet<T> ReadOnlySet<T>(ISet<T> set)
	{
		if (set != null && set.Count != 0)
		{
			return new ReadOnly.Set<ISet<T>, T>(set);
		}
		return EmptySet<T>();
	}
}
