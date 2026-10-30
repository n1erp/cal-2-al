using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SwitchDictionary<T> : IEnumerable<T>, IEnumerable
{
	private const int NotFoundIndex = -1;

	private readonly Tuple<int, T>[] itemsByKey;

	private readonly Func<int, int> lookupFunc;

	public SwitchDictionary()
		: this((IEnumerable<Tuple<int, T>>)new Tuple<int, T>[0])
	{
	}

	private SwitchDictionary(IEnumerable<Tuple<int, T>> newItemsByKey)
	{
		itemsByKey = newItemsByKey.ToArray();
		if (itemsByKey.Length == 0)
		{
			lookupFunc = (int k) => -1;
			return;
		}
		ParameterExpression parameterExpression = Expression.Parameter(typeof(int));
		SwitchExpression body = Expression.Switch(parameterExpression, Expression.Constant(-1), null, itemsByKey.Select((Tuple<int, T> pair, int index) => Expression.SwitchCase(Expression.Constant(index), Expression.Constant(pair.Item1))));
		lookupFunc = Expression.Lambda<Func<int, int>>(body, new ParameterExpression[1] { parameterExpression }).Compile();
	}

	public SwitchDictionary<T> Add(int key, T item)
	{
		if (lookupFunc(key) != -1)
		{
			throw new ArgumentException("The given key is already present in the list.");
		}
		return new SwitchDictionary<T>(itemsByKey.Concat(new Tuple<int, T>[1] { Tuple.Create(key, item) }));
	}

	public SwitchDictionary<T> RemoveKey(int key)
	{
		return new SwitchDictionary<T>(itemsByKey.Where((Tuple<int, T> tuple) => tuple.Item1 != key));
	}

	public bool TryGetValue(int key, out T item)
	{
		int num = lookupFunc(key);
		if (num == -1)
		{
			item = default;
			return false;
		}
		item = itemsByKey[num].Item2;
		return true;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return itemsByKey.Select((Tuple<int, T> tuple) => tuple.Item2).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
