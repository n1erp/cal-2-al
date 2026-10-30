using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal class ObjectPool<T> where T : class
{
	[DebuggerDisplay("{Value,nq}")]
	private struct Element
	{
		internal T Value;
	}

	internal delegate T Factory();

	private T firstItem;

	private readonly Element[] items;

	private readonly Factory factory;

	internal ObjectPool(Factory factory)
		: this(factory, Environment.ProcessorCount * 2)
	{
	}

	internal ObjectPool(Factory factory, int size)
	{
		DebugAssertHelper.Assert(size >= 1);
		this.factory = factory;
		items = new Element[size - 1];
	}

	private T CreateInstance()
	{
		return factory();
	}

	internal T Allocate()
	{
		T val = firstItem;
		if (val == null || val != Interlocked.CompareExchange(ref firstItem, null, val))
		{
			val = AllocateSlow();
		}
		return val;
	}

	private T AllocateSlow()
	{
		Element[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			T value = array[i].Value;
			if (value != null && value == Interlocked.CompareExchange(ref array[i].Value, null, value))
			{
				return value;
			}
		}
		return CreateInstance();
	}

	internal void Free(T obj)
	{
		if (firstItem == null)
		{
			firstItem = obj;
		}
		else
		{
			FreeSlow(obj);
		}
	}

	private void FreeSlow(T obj)
	{
		Element[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Value == null)
			{
				array[i].Value = obj;
				break;
			}
		}
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "By design")]
	[Conditional("DEBUG")]
	internal void ForgetTrackedObject(T old, T replacement = null)
	{
	}

	[Conditional("DEBUG")]
	private void Validate(object obj)
	{
		DebugAssertHelper.Assert(obj != null, "freeing null?");
		Element[] array = items;
		for (int i = 0; i < array.Length; i++)
		{
			T value = array[i].Value;
			if (value == null)
			{
				break;
			}
			DebugAssertHelper.Assert(value != obj, "freeing twice?");
		}
	}
}
