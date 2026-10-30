using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Collections;

internal class KeyedStack<T, R>
{
	private readonly Dictionary<T, Stack<R>> _dict = new Dictionary<T, Stack<R>>();

	public void Push(T key, R value)
	{
		if (!_dict.TryGetValue(key, out var value2))
		{
			value2 = new Stack<R>();
			_dict.Add(key, value2);
		}
		value2.Push(value);
	}

	public bool TryPop(T key, out R value)
	{
		if (_dict.TryGetValue(key, out var value2) && value2.Count > 0)
		{
			value = value2.Pop();
			return true;
		}
		value = default;
		return false;
	}
}
