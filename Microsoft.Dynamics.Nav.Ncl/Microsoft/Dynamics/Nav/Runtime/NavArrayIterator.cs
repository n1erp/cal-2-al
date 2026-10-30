using System;
using System.Collections;

namespace Microsoft.Dynamics.Nav.Runtime;

internal sealed class NavArrayIterator : IEnumerable
{
	private Array array;

	private bool recurse;

	internal NavArrayIterator(Array array, bool recurse)
	{
		this.array = array;
		this.recurse = recurse;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		for (int i = 0; i < array.GetLength(0); i++)
		{
			object value = array.GetValue(i);
			if (recurse && value is Array)
			{
				foreach (object item in (IEnumerable)new NavArrayIterator((Array)value, recurse: true))
				{
					yield return item;
				}
			}
			else
			{
				yield return value;
			}
		}
	}
}
