using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class SyntaxAndDeclarationManager
{
	internal sealed class State
	{
		internal readonly DeclarationTable DeclarationTable;

		internal readonly ImmutableDictionary<SyntaxTree, int> OrdinalMap;

		internal readonly ImmutableDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> RootObjectCollections;

		internal readonly ImmutableArray<SyntaxTree> SyntaxTrees;

		internal State(ImmutableArray<SyntaxTree> syntaxTrees, ImmutableDictionary<SyntaxTree, int> syntaxTreeOrdinalMap, ImmutableDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> rootObjectCollections, DeclarationTable declarationTable)
		{
			DebugAssertHelper.Assert(syntaxTrees.All((SyntaxTree tree) => syntaxTrees[syntaxTreeOrdinalMap[tree]] == tree));
			DebugAssertHelper.Assert(syntaxTrees.SetEquals(rootObjectCollections.Keys.ToImmutableArray(), EqualityComparer<SyntaxTree>.Default));
			SyntaxTrees = syntaxTrees;
			OrdinalMap = syntaxTreeOrdinalMap;
			RootObjectCollections = rootObjectCollections;
			DeclarationTable = declarationTable;
		}
	}

	internal readonly ImmutableArray<SyntaxTree> ExternalSyntaxTrees;

	internal readonly CommonMessageProvider MessageProvider;

	private State lazyState;

	internal SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree> externalSyntaxTrees, CommonMessageProvider messageProvider, State state)
	{
		ExternalSyntaxTrees = externalSyntaxTrees;
		MessageProvider = messageProvider;
		lazyState = state;
	}

	internal State GetLazyState()
	{
		if (lazyState == null)
		{
			Interlocked.CompareExchange(ref lazyState, CreateState(ExternalSyntaxTrees), null);
		}
		return lazyState;
	}

	private static State CreateState(ImmutableArray<SyntaxTree> externalSyntaxTrees)
	{
		ArrayBuilder<SyntaxTree> instance = ArrayBuilder<SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, int> instance2 = PooledDictionary<SyntaxTree, int>.GetInstance();
		PooledDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> instance3 = PooledDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>>.GetInstance();
		DeclarationTable declTable = DeclarationTable.Empty;
		foreach (SyntaxTree item in externalSyntaxTrees)
		{
			AppendAllSyntaxTrees(instance, item, instance2, instance3, ref declTable);
		}
		return new State(instance.ToImmutableAndFree(), instance2.ToImmutableDictionaryAndFree(), instance3.ToImmutableDictionaryAndFree(), declTable);
	}

	public SyntaxAndDeclarationManager AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		CommonMessageProvider messageProvider = MessageProvider;
		State state = lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.AddRange(trees);
		if (state == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		ImmutableDictionary<SyntaxTree, int>.Builder builder = state.OrdinalMap.ToBuilder();
		ImmutableDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>>.Builder builder2 = state.RootObjectCollections.ToBuilder();
		DeclarationTable declTable = state.DeclarationTable;
		ArrayBuilder<SyntaxTree> instance = ArrayBuilder<SyntaxTree>.GetInstance();
		instance.AddRange(state.SyntaxTrees);
		foreach (SyntaxTree tree in trees)
		{
			AppendAllSyntaxTrees(instance, tree, builder, builder2, ref declTable);
		}
		state = new State(instance.ToImmutableAndFree(), builder.ToImmutableDictionary(), builder2.ToImmutableDictionary(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, messageProvider, state);
	}

	private static void AppendAllSyntaxTrees(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, IDictionary<SyntaxTree, int> ordinalMapBuilder, IDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> declMapBuilder, ref DeclarationTable declTable)
	{
		AddSyntaxTreeToDeclarationMapAndTable(tree, declMapBuilder, ref declTable);
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);
	}

	private static void AddSyntaxTreeToDeclarationMapAndTable(SyntaxTree tree, IDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> declMapBuilder, ref DeclarationTable declTable)
	{
		Lazy<RootObjectCollectionDeclaration> lazy = new Lazy<RootObjectCollectionDeclaration>(() => DeclarationTreeBuilder.ForTree(tree));
		declMapBuilder.Add(tree, lazy);
		declTable = declTable.AddRootDeclaration(lazy);
	}

	public SyntaxAndDeclarationManager RemoveSyntaxTrees(HashSet<SyntaxTree> trees)
	{
		State state = lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.RemoveAll((SyntaxTree t) => trees.Contains(t));
		if (state == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		ImmutableArray<SyntaxTree> syntaxTrees = state.SyntaxTrees;
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		instance.AddAll(trees);
		ArrayBuilder<SyntaxTree> instance2 = ArrayBuilder<SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, int> instance3 = PooledDictionary<SyntaxTree, int>.GetInstance();
		ImmutableDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>>.Builder builder = state.RootObjectCollections.ToBuilder();
		DeclarationTable declTable = state.DeclarationTable;
		foreach (SyntaxTree item in syntaxTrees)
		{
			if (instance.Contains(item))
			{
				RemoveSyntaxTreeFromDeclarationMapAndTable(item, builder, ref declTable);
			}
			else
			{
				UpdateSyntaxTreesAndOrdinalMapOnly(instance2, item, instance3);
			}
		}
		instance.Free();
		state = new State(instance2.ToImmutableAndFree(), instance3.ToImmutableDictionaryAndFree(), builder.ToImmutableDictionary(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, MessageProvider, state);
	}

	private static void RemoveSyntaxTreeFromDeclarationMapAndTable(SyntaxTree tree, IDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>> declMap, ref DeclarationTable declTable)
	{
		Lazy<RootObjectCollectionDeclaration> lazyRootDeclaration = declMap[tree];
		declTable = declTable.RemoveRootDeclaration(lazyRootDeclaration);
		declMap.Remove(tree);
	}

	private static void UpdateSyntaxTreesAndOrdinalMapOnly(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, IDictionary<SyntaxTree, int> ordinalMapBuilder)
	{
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);
	}

	public SyntaxAndDeclarationManager ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		State state = lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.Replace(oldTree, newTree);
		if (state == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		ImmutableArray<SyntaxTree> syntaxTrees = state.SyntaxTrees;
		ImmutableDictionary<SyntaxTree, int> ordinalMap = state.OrdinalMap;
		ImmutableDictionary<SyntaxTree, Lazy<RootObjectCollectionDeclaration>>.Builder builder = state.RootObjectCollections.ToBuilder();
		DeclarationTable declTable = state.DeclarationTable;
		int num = ordinalMap[oldTree];
		RemoveSyntaxTreeFromDeclarationMapAndTable(oldTree, builder, ref declTable);
		AddSyntaxTreeToDeclarationMapAndTable(newTree, builder, ref declTable);
		DebugAssertHelper.Assert(ordinalMap.ContainsKey(oldTree));
		ImmutableArray<SyntaxTree> syntaxTrees2 = syntaxTrees.SetItem(num, newTree);
		ordinalMap = ordinalMap.Remove(oldTree);
		ordinalMap = ordinalMap.SetItem(newTree, num);
		state = new State(syntaxTrees2, ordinalMap, builder.ToImmutable(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, MessageProvider, state);
	}

	internal SyntaxAndDeclarationManager WithExternalSyntaxTrees(ImmutableArray<SyntaxTree> trees)
	{
		return new SyntaxAndDeclarationManager(trees, MessageProvider, null);
	}
}
