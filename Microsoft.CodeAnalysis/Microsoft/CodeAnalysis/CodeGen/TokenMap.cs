using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CodeGen;

internal sealed class TokenMap<T> where T : class
{
	private readonly ConcurrentDictionary<T, uint> _itemIdentityToToken = new ConcurrentDictionary<T, uint>(Roslyn.Utilities.ReferenceEqualityComparer.Instance);

	private readonly Dictionary<T, uint> _itemToToken = new Dictionary<T, uint>();

	private readonly ArrayBuilder<T> _items = new ArrayBuilder<T>();

	public uint GetOrAddTokenFor(T item, out bool referenceAdded)
	{
		if (_itemIdentityToToken.TryGetValue(item, out var value))
		{
			referenceAdded = false;
			return value;
		}
		return AddItem(item, out referenceAdded);
	}

	private uint AddItem(T item, out bool referenceAdded)
	{
		uint value;
		lock (_items)
		{
			if (!_itemToToken.TryGetValue(item, out value))
			{
				value = (uint)_items.Count;
				_items.Add(item);
				_itemToToken.Add(item, value);
			}
		}
		referenceAdded = _itemIdentityToToken.TryAdd(item, value);
		return value;
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

	public IEnumerable<T> GetAllItemsAndCount(out int count)
	{
		lock (_items)
		{
			count = _items.Count;
			return _items.ToArray();
		}
	}
}
