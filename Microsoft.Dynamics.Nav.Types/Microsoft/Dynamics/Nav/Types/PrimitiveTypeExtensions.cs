using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Types;

public static class PrimitiveTypeExtensions
{
	public static bool In(this int value, params int[] matches)
	{
		return matches.Contains(value);
	}

	public static bool In(this string value, params string[] matches)
	{
		return matches.Contains(value, StringComparer.Ordinal);
	}

	public static bool In(this string value, IEqualityComparer<string> comparer, params string[] matches)
	{
		return matches.Contains(value, comparer);
	}

	public static bool In<T>(this T value, params T[] matches) where T : struct
	{
		return matches.Contains(value);
	}
}
