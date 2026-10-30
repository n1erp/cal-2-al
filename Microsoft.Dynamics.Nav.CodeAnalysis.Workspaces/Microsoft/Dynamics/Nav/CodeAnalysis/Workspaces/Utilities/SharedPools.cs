using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal static class SharedPools
{
	private static class DefaultBigPool<T> where T : class, new()
	{
		public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 100);
	}

	private static class DefaultNormalPool<T> where T : class, new()
	{
		public static readonly ObjectPool<T> Instance = new ObjectPool<T>(() => new T(), 20);
	}

	private static class StringIgnoreCaseDictionaryNormalPool<T>
	{
		public static readonly ObjectPool<Dictionary<string, T>> Instance = new ObjectPool<Dictionary<string, T>>(() => new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase), 20);
	}

	public static readonly ObjectPool<HashSet<string>> StringIgnoreCaseHashSet = new ObjectPool<HashSet<string>>(() => new HashSet<string>(StringComparer.OrdinalIgnoreCase), 20);

	public static readonly ObjectPool<HashSet<string>> StringHashSet = new ObjectPool<HashSet<string>>(() => new HashSet<string>(StringComparer.Ordinal), 20);

	public static readonly ObjectPool<byte[]> ByteArray = new ObjectPool<byte[]>(() => new byte[4096], 512);

	public const int ByteBufferSize = 4096;

	private const int ByteBufferCount = 512;

	public static ObjectPool<T> BigDefault<T>() where T : class, new()
	{
		return DefaultBigPool<T>.Instance;
	}

	public static ObjectPool<T> Default<T>() where T : class, new()
	{
		return DefaultNormalPool<T>.Instance;
	}

	public static ObjectPool<Dictionary<string, T>> StringIgnoreCaseDictionary<T>()
	{
		return StringIgnoreCaseDictionaryNormalPool<T>.Instance;
	}
}
