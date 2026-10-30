using System.Collections.Generic;
using System.Collections.Immutable;

namespace Roslyn.Utilities;

internal static class ImmutableListExtensions
{
	internal static ImmutableList<T> ToImmutableListOrEmpty<T>(this T[] items)
	{
		if (items == null)
		{
			return ImmutableList.Create<T>();
		}
		return ImmutableList.Create(items);
	}

	internal static ImmutableList<T> ToImmutableListOrEmpty<T>(this IEnumerable<T> items)
	{
		if (items == null)
		{
			return ImmutableList.Create<T>();
		}
		return ImmutableList.CreateRange(items);
	}
}
