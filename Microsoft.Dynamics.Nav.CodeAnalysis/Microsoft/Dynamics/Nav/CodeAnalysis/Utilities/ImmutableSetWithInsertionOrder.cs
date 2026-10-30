using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal sealed class ImmutableSetWithInsertionOrder<T> : IEnumerable<T>, IEnumerable
{
	public static readonly ImmutableSetWithInsertionOrder<T> Empty = new ImmutableSetWithInsertionOrder<T>(ImmutableDictionary.Create<T, uint>(), 0u);

	private readonly ImmutableDictionary<T, uint> immutableMap;

	private readonly uint nextElementValue;

	public int Count => immutableMap.Count;

	public IEnumerable<T> InInsertionOrder => from kv in immutableMap
		orderby kv.Value
		select kv.Key;

	private ImmutableSetWithInsertionOrder(ImmutableDictionary<T, uint> map, uint nextElementValue)
	{
		immutableMap = map;
		this.nextElementValue = nextElementValue;
	}

	public bool Contains(T value)
	{
		return immutableMap.ContainsKey(value);
	}

	public ImmutableSetWithInsertionOrder<T> Add(T value)
	{
		if (immutableMap.ContainsKey(value))
		{
			return this;
		}
		return new ImmutableSetWithInsertionOrder<T>(immutableMap.Add(value, nextElementValue), nextElementValue + 1);
	}

	public ImmutableSetWithInsertionOrder<T> Remove(T value)
	{
		if (!immutableMap.ContainsKey(value))
		{
			return this;
		}
		if (Count != 1)
		{
			return new ImmutableSetWithInsertionOrder<T>(immutableMap.Remove(value), nextElementValue);
		}
		return Empty;
	}

	public override string ToString()
	{
		return "{" + string.Join(", ", this) + "}";
	}

	public IEnumerator<T> GetEnumerator()
	{
		return immutableMap.Keys.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return immutableMap.Keys.GetEnumerator();
	}
}
