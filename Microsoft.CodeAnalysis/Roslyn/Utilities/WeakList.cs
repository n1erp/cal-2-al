using System;
using System.Collections;
using System.Collections.Generic;

namespace Roslyn.Utilities;

internal sealed class WeakList<T> : IEnumerable<T>, IEnumerable where T : class
{
	private WeakReference<T>[] _items;

	private int _size;

	private const int MinimalNonEmptySize = 4;

	public int WeakCount => _size;

	internal WeakReference<T>[] TestOnly_UnderlyingArray => _items;

	public WeakList()
	{
		_items = Array.Empty<WeakReference<T>>();
	}

	private void Resize()
	{
		int num = _items.Length;
		int num2 = -1;
		for (int i = 0; i < _items.Length; i++)
		{
			if (!_items[i].TryGetTarget(out var _))
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				num--;
			}
		}
		if (num < _items.Length / 4)
		{
			Shrink(num2, num);
		}
		else if (num >= 3 * _items.Length / 4)
		{
			WeakReference<T>[] array = new WeakReference<T>[GetExpandedSize(_items.Length)];
			if (num2 >= 0)
			{
				Compact(num2, array);
			}
			else
			{
				Array.Copy(_items, 0, array, 0, _items.Length);
			}
			_items = array;
		}
		else
		{
			Compact(num2, _items);
		}
	}

	private void Shrink(int firstDead, int alive)
	{
		int expandedSize = GetExpandedSize(alive);
		WeakReference<T>[] array = ((expandedSize == _items.Length) ? _items : new WeakReference<T>[expandedSize]);
		Compact(firstDead, array);
		_items = array;
	}

	private static int GetExpandedSize(int baseSize)
	{
		return Math.Max(baseSize * 2 + 1, 4);
	}

	private void Compact(int firstDead, WeakReference<T>[] result)
	{
		if (_items != result)
		{
			Array.Copy(_items, 0, result, 0, firstDead);
		}
		int size = _size;
		int num = firstDead;
		for (int i = firstDead + 1; i < size; i++)
		{
			WeakReference<T> weakReference = _items[i];
			if (weakReference.TryGetTarget(out var _))
			{
				result[num++] = weakReference;
			}
		}
		_size = num;
		if (_items == result)
		{
			while (num < size)
			{
				_items[num++] = null;
			}
		}
	}

	public WeakReference<T> GetWeakReference(int index)
	{
		if (index < 0 || index >= _size)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		return _items[index];
	}

	public void Add(T item)
	{
		if (_size == _items.Length)
		{
			Resize();
		}
		_items[_size++] = new WeakReference<T>(item);
	}

	public IEnumerator<T> GetEnumerator()
	{
		int count = _size;
		int alive = _size;
		int firstDead = -1;
		for (int i = 0; i < count; i++)
		{
			if (_items[i].TryGetTarget(out var target))
			{
				yield return target;
				continue;
			}
			if (firstDead < 0)
			{
				firstDead = i;
			}
			alive--;
		}
		if (alive == 0)
		{
			_items = Array.Empty<WeakReference<T>>();
			_size = 0;
		}
		else if (alive < _items.Length / 4)
		{
			Shrink(firstDead, alive);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
