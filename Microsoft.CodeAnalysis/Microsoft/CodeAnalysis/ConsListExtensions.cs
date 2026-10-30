using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis;

internal static class ConsListExtensions
{
	public static ConsList<T> Prepend<T>(this ConsList<T> list, T head)
	{
		return new ConsList<T>(head, list ?? ConsList<T>.Empty);
	}

	public static bool ContainsReference<T>(this ConsList<T> list, T element)
	{
		while (list != ConsList<T>.Empty)
		{
			if ((object)list.Head == (object)element)
			{
				return true;
			}
			list = list.Tail;
		}
		return false;
	}
}
