using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class ItemTokenMap<T> where T : class
{
	private readonly ConcurrentDictionary<T, uint> _itemToToken = new ConcurrentDictionary<T, uint>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	private readonly ArrayBuilder<T> _items = new ArrayBuilder<T>();

	public uint GetOrAddTokenFor(T item)
	{
		if (_itemToToken.TryGetValue(item, out var value))
		{
			return value;
		}
		return AddItem(item);
	}

	private uint AddItem(T item)
	{
		lock (_items)
		{
			if (_itemToToken.TryGetValue(item, out var value))
			{
				return value;
			}
			value = (uint)_items.Count;
			_items.Add(item);
			_itemToToken.Add(item, value);
			return value;
		}
	}

	public T GetItem(uint token)
	{
		lock (_items)
		{
			return _items[(int)token];
		}
	}

	public IEnumerable<T> GetAllItems()
	{
		lock (_items)
		{
			return _items.ToArray();
		}
	}
}
