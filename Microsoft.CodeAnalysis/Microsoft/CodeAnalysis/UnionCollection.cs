using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal class UnionCollection<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	private readonly ImmutableArray<ICollection<T>> _collections;

	private int _count = -1;

	public int Count
	{
		get
		{
			if (_count == -1)
			{
				_count = _collections.Sum((ICollection<T> c) => c.Count);
			}
			return _count;
		}
	}

	public bool IsReadOnly => true;

	public static ICollection<T> Create(ICollection<T> coll1, ICollection<T> coll2)
	{
		if (coll1.Count == 0)
		{
			return coll2;
		}
		if (coll2.Count == 0)
		{
			return coll1;
		}
		return new UnionCollection<T>(ImmutableArray.Create(coll1, coll2));
	}

	public static ICollection<T> Create<TOrig>(ImmutableArray<TOrig> collections, Func<TOrig, ICollection<T>> selector)
	{
		return collections.Length switch
		{
			0 => SpecializedCollections.EmptyCollection<T>(), 
			1 => selector(collections[0]), 
			_ => new UnionCollection<T>(ImmutableArray.CreateRange(collections, selector)), 
		};
	}

	private UnionCollection(ImmutableArray<ICollection<T>> collections)
	{
		_collections = collections;
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
		foreach (ICollection<T> collection in _collections)
		{
			if (collection.Contains(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		int num = arrayIndex;
		foreach (ICollection<T> collection in _collections)
		{
			collection.CopyTo(array, num);
			num += collection.Count;
		}
	}

	public bool Remove(T item)
	{
		throw new NotSupportedException();
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _collections.SelectMany((ICollection<T> c) => c).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
