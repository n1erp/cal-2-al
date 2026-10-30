using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Roslyn.Utilities;

internal struct OneOrMany<T>
{
	internal struct Enumerator
	{
		private readonly OneOrMany<T> _collection;

		private int _index;

		public T Current => _collection[_index];

		internal Enumerator(OneOrMany<T> collection)
		{
			_collection = collection;
			_index = -1;
		}

		public bool MoveNext()
		{
			_index++;
			return _index < _collection.Count;
		}
	}

	private readonly T _one;

	private readonly ImmutableArray<T> _many;

	public T this[int index]
	{
		get
		{
			if (_many.IsDefault)
			{
				if (index != 0)
				{
					throw new IndexOutOfRangeException();
				}
				return _one;
			}
			return _many[index];
		}
	}

	public int Count
	{
		get
		{
			if (!_many.IsDefault)
			{
				return _many.Length;
			}
			return 1;
		}
	}

	public OneOrMany(T one)
	{
		_one = one;
		_many = default;
	}

	public OneOrMany(ImmutableArray<T> many)
	{
		if (many.IsDefault)
		{
			throw new ArgumentNullException("many");
		}
		_one = default;
		_many = many;
	}

	public OneOrMany<T> Add(T one)
	{
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		if (_many.IsDefault)
		{
			instance.Add(_one);
		}
		else
		{
			instance.AddRange(_many);
		}
		instance.Add(one);
		return new OneOrMany<T>(instance.ToImmutableAndFree());
	}

	public bool Contains(T item)
	{
		if (Count == 1)
		{
			object obj = _one;
			return item.Equals(obj);
		}
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			object obj2 = enumerator.Current;
			if (item.Equals(obj2))
			{
				return true;
			}
		}
		return false;
	}

	public OneOrMany<T> RemoveAll(T item)
	{
		if (_many.IsDefault)
		{
			object obj = _one;
			if (!item.Equals(obj))
			{
				return this;
			}
			return default;
		}
		ArrayBuilder<T> instance = ArrayBuilder<T>.GetInstance();
		Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			object obj2 = enumerator.Current;
			if (!item.Equals(obj2))
			{
				instance.Add(enumerator.Current);
			}
		}
		if (instance.Count == 0)
		{
			return default;
		}
		if (instance.Count != Count)
		{
			return new OneOrMany<T>(instance.ToImmutableAndFree());
		}
		return this;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
}
internal static class OneOrMany
{
	public static OneOrMany<T> Create<T>(T one)
	{
		return new OneOrMany<T>(one);
	}

	public static OneOrMany<T> Create<T>(ImmutableArray<T> many)
	{
		return new OneOrMany<T>(many);
	}
}
