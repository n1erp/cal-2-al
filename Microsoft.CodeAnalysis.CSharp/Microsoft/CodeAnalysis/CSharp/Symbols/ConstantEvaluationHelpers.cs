using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ConstantEvaluationHelpers
{
	[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
	internal struct FieldInfo(SourceFieldSymbolWithSyntaxReference field, bool startsCycle)
	{
		public readonly SourceFieldSymbolWithSyntaxReference Field = field;

		public readonly bool StartsCycle = startsCycle;

		private string GetDebuggerDisplay()
		{
			string text = Field.ToString();
			if (StartsCycle)
			{
				text += " [cycle]";
			}
			return text;
		}
	}

	private struct Node<T> where T : class
	{
		public ImmutableHashSet<T> Dependencies;

		public ImmutableHashSet<T> DependedOnBy;
	}

	private sealed class SourceLocationComparer : IComparer<SourceFieldSymbolWithSyntaxReference>
	{
		private readonly Dictionary<Compilation, int> _compilationOrdering;

		internal SourceLocationComparer(Dictionary<Compilation, int> compilationOrdering)
		{
			_compilationOrdering = compilationOrdering;
		}

		public int Compare(SourceFieldSymbolWithSyntaxReference x, SourceFieldSymbolWithSyntaxReference y)
		{
			CSharpCompilation declaringCompilation = x.DeclaringCompilation;
			CSharpCompilation declaringCompilation2 = y.DeclaringCompilation;
			int num = _compilationOrdering[declaringCompilation] - _compilationOrdering[declaringCompilation2];
			if (num == 0)
			{
				num = declaringCompilation.CompareSourceLocations(x.ErrorLocation, y.ErrorLocation);
			}
			return num;
		}
	}

	internal static void OrderAllDependencies(this SourceFieldSymbolWithSyntaxReference field, ArrayBuilder<FieldInfo> order, bool earlyDecodingWellKnownAttributes)
	{
		PooledDictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> instance = PooledDictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>>.GetInstance();
		CreateGraph(instance, field, earlyDecodingWellKnownAttributes);
		OrderGraph(instance, order);
		instance.Free();
	}

	private static void CreateGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, SourceFieldSymbolWithSyntaxReference field, bool earlyDecodingWellKnownAttributes)
	{
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		instance.Push(field);
		while (instance.Count > 0)
		{
			field = instance.Pop();
			if (graph.TryGetValue(field, out var value))
			{
				if (value.Dependencies != null)
				{
					continue;
				}
			}
			else
			{
				value = new Node<SourceFieldSymbolWithSyntaxReference>
				{
					DependedOnBy = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty
				};
			}
			ImmutableHashSet<SourceFieldSymbolWithSyntaxReference> immutableHashSet = (value.Dependencies = field.GetConstantValueDependencies(earlyDecodingWellKnownAttributes));
			graph[field] = value;
			foreach (SourceFieldSymbolWithSyntaxReference item in immutableHashSet)
			{
				instance.Push(item);
				if (!graph.TryGetValue(item, out value))
				{
					value = new Node<SourceFieldSymbolWithSyntaxReference>
					{
						DependedOnBy = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty
					};
				}
				value.DependedOnBy = value.DependedOnBy.Add(field);
				graph[item] = value;
			}
		}
		instance.Free();
	}

	private static void OrderGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, ArrayBuilder<FieldInfo> order)
	{
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> pooledHashSet = null;
		while (graph.Count > 0)
		{
			IEnumerable<SourceFieldSymbolWithSyntaxReference> enumerable = pooledHashSet;
			IEnumerable<SourceFieldSymbolWithSyntaxReference> obj = enumerable ?? graph.Keys;
			ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
			foreach (SourceFieldSymbolWithSyntaxReference item in obj)
			{
				if (graph.TryGetValue(item, out var value) && value.Dependencies.Count == 0)
				{
					instance.Add(item);
				}
			}
			pooledHashSet?.Free();
			pooledHashSet = null;
			if (instance.Count > 0)
			{
				PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance2 = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
				foreach (SourceFieldSymbolWithSyntaxReference item2 in instance)
				{
					foreach (SourceFieldSymbolWithSyntaxReference item3 in graph[item2].DependedOnBy)
					{
						Node<SourceFieldSymbolWithSyntaxReference> value2 = graph[item3];
						value2.Dependencies = value2.Dependencies.Remove(item2);
						graph[item3] = value2;
						instance2.Add(item3);
					}
					graph.Remove(item2);
				}
				foreach (SourceFieldSymbolWithSyntaxReference item4 in instance)
				{
					order.Add(new FieldInfo(item4, startsCycle: false));
				}
				pooledHashSet = instance2;
			}
			else
			{
				SourceFieldSymbolWithSyntaxReference startOfFirstCycle = GetStartOfFirstCycle(graph);
				foreach (SourceFieldSymbolWithSyntaxReference dependency in graph[startOfFirstCycle].Dependencies)
				{
					Node<SourceFieldSymbolWithSyntaxReference> value3 = graph[dependency];
					value3.DependedOnBy = value3.DependedOnBy.Remove(startOfFirstCycle);
					graph[dependency] = value3;
				}
				Node<SourceFieldSymbolWithSyntaxReference> value4 = graph[startOfFirstCycle];
				value4.Dependencies = ImmutableHashSet<SourceFieldSymbolWithSyntaxReference>.Empty;
				if (value4.DependedOnBy.Count == 0)
				{
					graph.Remove(startOfFirstCycle);
				}
				else
				{
					graph[startOfFirstCycle] = value4;
				}
				order.Add(new FieldInfo(startOfFirstCycle, startsCycle: true));
			}
			instance.Free();
		}
		pooledHashSet?.Free();
	}

	private static SourceFieldSymbolWithSyntaxReference GetStartOfFirstCycle(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph)
	{
		PooledDictionary<Compilation, int> instance = PooledDictionary<Compilation, int>.GetInstance();
		OrderCompilations(graph, instance);
		SourceLocationComparer comparer = new SourceLocationComparer(instance);
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance2 = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		instance2.AddRange(graph.Keys);
		instance2.Sort(comparer);
		SourceFieldSymbolWithSyntaxReference memberOfCycle = GetMemberOfCycle(graph);
		instance2.Clear();
		GetAllReachable(graph, instance2, memberOfCycle);
		instance2.Sort(comparer);
		memberOfCycle = instance2[0];
		instance2.Free();
		instance.Free();
		return memberOfCycle;
	}

	private static void OrderCompilations(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> fields, Dictionary<Compilation, int> compilations)
	{
		foreach (SourceFieldSymbolWithSyntaxReference key in fields.Keys)
		{
			CSharpCompilation declaringCompilation = key.DeclaringCompilation;
			if (!compilations.ContainsKey(declaringCompilation))
			{
				compilations.Add(declaringCompilation, compilations.Count);
			}
		}
	}

	private static SourceFieldSymbolWithSyntaxReference GetMemberOfCycle(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph)
	{
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		SourceFieldSymbolWithSyntaxReference sourceFieldSymbolWithSyntaxReference = graph.First().Key;
		while (true)
		{
			sourceFieldSymbolWithSyntaxReference = graph[sourceFieldSymbolWithSyntaxReference].Dependencies.First();
			if (instance.Contains(sourceFieldSymbolWithSyntaxReference))
			{
				break;
			}
			instance.Add(sourceFieldSymbolWithSyntaxReference);
		}
		instance.Free();
		return sourceFieldSymbolWithSyntaxReference;
	}

	private static void GetAllReachable(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph, ArrayBuilder<SourceFieldSymbolWithSyntaxReference> fields, SourceFieldSymbolWithSyntaxReference field)
	{
		PooledHashSet<SourceFieldSymbolWithSyntaxReference> instance = PooledHashSet<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		ArrayBuilder<SourceFieldSymbolWithSyntaxReference> instance2 = ArrayBuilder<SourceFieldSymbolWithSyntaxReference>.GetInstance();
		instance.Add(field);
		instance2.Push(field);
		while (instance2.Count > 0)
		{
			field = instance2.Pop();
			foreach (SourceFieldSymbolWithSyntaxReference dependency in graph[field].Dependencies)
			{
				if (!instance.Contains(dependency))
				{
					instance.Add(dependency);
					instance2.Push(dependency);
				}
			}
		}
		fields.AddRange(instance);
		instance2.Free();
		instance.Free();
	}

	[Conditional("DEBUG")]
	private static void CheckGraph(Dictionary<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> graph)
	{
		int num = 10;
		foreach (KeyValuePair<SourceFieldSymbolWithSyntaxReference, Node<SourceFieldSymbolWithSyntaxReference>> item in graph)
		{
			_ = item.Key;
			Node<SourceFieldSymbolWithSyntaxReference> value = item.Value;
			foreach (SourceFieldSymbolWithSyntaxReference dependency in value.Dependencies)
			{
				graph.TryGetValue(dependency, out var _);
			}
			foreach (SourceFieldSymbolWithSyntaxReference item2 in value.DependedOnBy)
			{
				graph.TryGetValue(item2, out var _);
			}
			num--;
			if (num == 0)
			{
				break;
			}
		}
	}
}
