using System;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.CodeAnalysis.PooledObjects;

internal class ObjectPool<T> where T : class
{
	[DebuggerDisplay("{Value,nq}")]
	private struct Element
	{
		internal T Value;
	}

	internal delegate T Factory();

	private T _firstItem;

	private readonly Element[] _items;

	private readonly Factory _factory;

	internal ObjectPool(Factory factory)
		: this(factory, Environment.ProcessorCount * 2)
	{
	}

	internal ObjectPool(Factory factory, int size)
	{
		_factory = factory;
		_items = new Element[size - 1];
	}

	private T CreateInstance()
	{
		return _factory();
	}

	internal T Allocate()
	{
		T val = _firstItem;
		if (val == null || val != Interlocked.CompareExchange(ref _firstItem, null, val))
		{
			val = AllocateSlow();
		}
		return val;
	}

	private T AllocateSlow()
	{
		Element[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			T value = items[i].Value;
			if (value != null && value == Interlocked.CompareExchange(ref items[i].Value, null, value))
			{
				return value;
			}
		}
		return CreateInstance();
	}

	internal void Free(T obj)
	{
		if (_firstItem == null)
		{
			_firstItem = obj;
		}
		else
		{
			FreeSlow(obj);
		}
	}

	private void FreeSlow(T obj)
	{
		Element[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].Value == null)
			{
				items[i].Value = obj;
				break;
			}
		}
	}

	[Conditional("DEBUG")]
	internal void ForgetTrackedObject(T old, T replacement = null)
	{
	}

	[Conditional("DEBUG")]
	private void Validate(object obj)
	{
		Element[] items = _items;
		for (int i = 0; i < items.Length && items[i].Value != null; i++)
		{
		}
	}
}
