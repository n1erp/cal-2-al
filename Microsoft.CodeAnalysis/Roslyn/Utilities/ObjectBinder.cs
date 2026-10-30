using System;
using System.Collections.Generic;

namespace Roslyn.Utilities;

internal static class ObjectBinder
{
	private static object s_gate = new object();

	private static ObjectBinderSnapshot? s_lastSnapshot = null;

	private static readonly Dictionary<Type, int> s_typeToIndex = new Dictionary<Type, int>();

	private static readonly List<Type> s_types = new List<Type>();

	private static readonly List<Func<ObjectReader, IObjectWritable>> s_typeReaders = new List<Func<ObjectReader, IObjectWritable>>();

	public static ObjectBinderSnapshot GetSnapshot()
	{
		lock (s_gate)
		{
			if (!s_lastSnapshot.HasValue)
			{
				s_lastSnapshot = new ObjectBinderSnapshot(s_typeToIndex, s_types, s_typeReaders);
			}
			return s_lastSnapshot.Value;
		}
	}

	public static void RegisterTypeReader(Type type, Func<ObjectReader, IObjectWritable> typeReader)
	{
		lock (s_gate)
		{
			if (!s_typeToIndex.ContainsKey(type))
			{
				int count = s_typeReaders.Count;
				s_types.Add(type);
				s_typeReaders.Add(typeReader);
				s_typeToIndex.Add(type, count);
				s_lastSnapshot = null;
			}
		}
	}
}
