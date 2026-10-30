using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Roslyn.Utilities;

internal sealed class ImmutableSetWithInsertionOrder<T> : IEnumerable<T>, IEnumerable
{
	public static readonly ImmutableSetWithInsertionOrder<T> Empty = new ImmutableSetWithInsertionOrder<T>(ImmutableDictionary.Create<T, uint>(), 0u);

	private readonly ImmutableDictionary<T, uint> _map;

	private readonly uint _nextElementValue;

	public int Count => _map.Count;

	public IEnumerable<T> InInsertionOrder => from kv in _map
		orderby kv.Value
		select kv.Key;

	private ImmutableSetWithInsertionOrder(ImmutableDictionary<T, uint> map, uint nextElementValue)
	{
		_map = map;
		_nextElementValue = nextElementValue;
	}

	public bool Contains(T value)
	{
		return _map.ContainsKey(value);
	}

	public ImmutableSetWithInsertionOrder<T> Add(T value)
	{
		if (_map.ContainsKey(value))
		{
			return this;
		}
		return new ImmutableSetWithInsertionOrder<T>(_map.Add(value, _nextElementValue), _nextElementValue + 1);
	}

	public ImmutableSetWithInsertionOrder<T> Remove(T value)
	{
		ImmutableDictionary<T, uint> immutableDictionary = _map.Remove(value);
		if (immutableDictionary == _map)
		{
			return this;
		}
		if (Count != 1)
		{
			return new ImmutableSetWithInsertionOrder<T>(immutableDictionary, _nextElementValue);
		}
		return Empty;
	}

	public override string ToString()
	{
		return "{" + string.Join(", ", this) + "}";
	}

	public IEnumerator<T> GetEnumerator()
	{
		return _map.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _map.Keys.GetEnumerator();
	}
}
