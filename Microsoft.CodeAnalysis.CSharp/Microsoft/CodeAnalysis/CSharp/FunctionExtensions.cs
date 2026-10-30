using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class FunctionExtensions
{
	public static HashSet<T> TransitiveClosure<T>(this Func<T, IEnumerable<T>> relation, T item)
	{
		HashSet<T> hashSet = new HashSet<T>();
		Stack<T> stack = new Stack<T>();
		stack.Push(item);
		while (stack.Count > 0)
		{
			T arg = stack.Pop();
			foreach (T item2 in relation(arg))
			{
				if (hashSet.Add(item2))
				{
					stack.Push(item2);
				}
			}
		}
		return hashSet;
	}
}
