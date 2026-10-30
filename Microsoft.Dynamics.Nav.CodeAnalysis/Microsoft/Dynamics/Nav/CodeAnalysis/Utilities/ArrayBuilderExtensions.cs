using System;
using System.Collections.Immutable;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

internal static class ArrayBuilderExtensions
{
	public static bool Any<T>(this ArrayBuilder<T> builder, Func<T, bool> predicate)
	{
		foreach (T item in builder)
		{
			if (predicate(item))
			{
				return true;
			}
		}
		return false;
	}

	public static bool All<T>(this ArrayBuilder<T> builder, Func<T, bool> predicate)
	{
		foreach (T item in builder)
		{
			if (!predicate(item))
			{
				return false;
			}
		}
		return true;
	}

	public static ImmutableArray<TResult> SelectAsArray<TItem, TResult>(this ArrayBuilder<TItem> items, Func<TItem, TResult> map)
	{
		switch (items.Count)
		{
		case 0:
			return ImmutableArray<TResult>.Empty;
		case 1:
			return ImmutableArray.Create(map(items[0]));
		case 2:
			return ImmutableArray.Create(map(items[0]), map(items[1]));
		case 3:
			return ImmutableArray.Create(map(items[0]), map(items[1]), map(items[2]));
		case 4:
			return ImmutableArray.Create(map(items[0]), map(items[1]), map(items[2]), map(items[3]));
		default:
		{
			ArrayBuilder<TResult> instance = ArrayBuilder<TResult>.GetInstance(items.Count);
			foreach (TItem item in items)
			{
				instance.Add(map(item));
			}
			return instance.ToImmutableAndFree();
		}
		}
	}

	public static void AddOptional<T>(this ArrayBuilder<T> builder, T item) where T : class
	{
		if (item != null)
		{
			builder.Add(item);
		}
	}

	public static void Push<T>(this ArrayBuilder<T> builder, T e)
	{
		builder.Add(e);
	}

	public static T Pop<T>(this ArrayBuilder<T> builder)
	{
		T result = builder.Peek();
		builder.RemoveAt(builder.Count - 1);
		return result;
	}

	public static T Peek<T>(this ArrayBuilder<T> builder)
	{
		return builder[builder.Count - 1];
	}

	public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T value) where T : class
	{
		if (value != null)
		{
			builder.Add(value);
		}
	}
}
