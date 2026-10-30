using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

public class LeastRecentlyUsedCache<TKey, TValue>
{
	private readonly LinkedList<TValue> values = new LinkedList<TValue>();

	private readonly Dictionary<TKey, LinkedListNode<TValue>> lookup = new Dictionary<TKey, LinkedListNode<TValue>>();

	private readonly int capacity;

	private readonly Func<TKey, TValue> creator;

	private readonly Func<TValue, TKey> keyFromValue;

	private readonly object syncRoot = new object();

	public IList<TKey> Keys
	{
		get
		{
			lock (syncRoot)
			{
				return lookup.Keys.ToList();
			}
		}
	}

	public LeastRecentlyUsedCache(int capacity, Func<TKey, TValue> creator, Func<TValue, TKey> keyFromValue)
	{
		if (creator == null)
		{
			throw new ArgumentNullException("creator");
		}
		if (keyFromValue == null)
		{
			throw new ArgumentNullException("keyFromValue");
		}
		this.capacity = capacity;
		this.creator = creator;
		this.keyFromValue = keyFromValue;
	}

	public TValue GetItem(TKey key)
	{
		LinkedListNode<TValue> value;
		lock (syncRoot)
		{
			if (lookup.TryGetValue(key, out value))
			{
				if (value.List != null)
				{
					if (values.First != value)
					{
						values.Remove(value);
						values.AddFirst(value);
					}
					return value.Value;
				}
			}
			else
			{
				value = new LinkedListNode<TValue>(default);
				lookup.Add(key, value);
			}
		}
		lock (value)
		{
			if (value.List != null)
			{
				return value.Value;
			}
			value.Value = creator(key);
			lock (syncRoot)
			{
				if (value.List != null)
				{
					return value.Value;
				}
				if (lookup.ContainsKey(key))
				{
					values.AddFirst(value);
					if (values.Count > capacity)
					{
						LinkedListNode<TValue> last = values.Last;
						values.RemoveLast();
						lookup.Remove(keyFromValue(last.Value));
					}
				}
				return value.Value;
			}
		}
	}

	public void RemoveItem(TKey key)
	{
		LinkedListNode<TValue> value;
		lock (syncRoot)
		{
			if (!lookup.TryGetValue(key, out value))
			{
				return;
			}
		}
		lock (value)
		{
			lock (syncRoot)
			{
				lookup.Remove(key);
				if (value.List != null)
				{
					values.Remove(value);
				}
			}
		}
	}
}
