using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SyntaxAndDeclarationManager : CommonSyntaxAndDeclarationManager
{
	internal sealed class State
	{
		internal readonly ImmutableArray<SyntaxTree> SyntaxTrees;

		internal readonly ImmutableDictionary<SyntaxTree, int> OrdinalMap;

		internal readonly ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> LoadDirectiveMap;

		internal readonly ImmutableDictionary<string, SyntaxTree> LoadedSyntaxTreeMap;

		internal readonly ImmutableDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> RootNamespaces;

		internal readonly DeclarationTable DeclarationTable;

		internal State(ImmutableArray<SyntaxTree> syntaxTrees, ImmutableDictionary<SyntaxTree, int> syntaxTreeOrdinalMap, ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMap, ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap, ImmutableDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> rootNamespaces, DeclarationTable declarationTable)
		{
			SyntaxTrees = syntaxTrees;
			OrdinalMap = syntaxTreeOrdinalMap;
			LoadDirectiveMap = loadDirectiveMap;
			LoadedSyntaxTreeMap = loadedSyntaxTreeMap;
			RootNamespaces = rootNamespaces;
			DeclarationTable = declarationTable;
		}
	}

	private State _lazyState;

	internal SyntaxAndDeclarationManager(ImmutableArray<SyntaxTree> externalSyntaxTrees, string scriptClassName, SourceReferenceResolver resolver, CommonMessageProvider messageProvider, bool isSubmission, State state)
		: base(externalSyntaxTrees, scriptClassName, resolver, messageProvider, isSubmission)
	{
		_lazyState = state;
	}

	internal State GetLazyState()
	{
		if (_lazyState == null)
		{
			Interlocked.CompareExchange(ref _lazyState, CreateState(ExternalSyntaxTrees, ScriptClassName, Resolver, MessageProvider, IsSubmission), null);
		}
		return _lazyState;
	}

	private static State CreateState(ImmutableArray<SyntaxTree> externalSyntaxTrees, string scriptClassName, SourceReferenceResolver resolver, CommonMessageProvider messageProvider, bool isSubmission)
	{
		ArrayBuilder<SyntaxTree> instance = ArrayBuilder<SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, int> instance2 = PooledDictionary<SyntaxTree, int>.GetInstance();
		PooledDictionary<SyntaxTree, ImmutableArray<LoadDirective>> instance3 = PooledDictionary<SyntaxTree, ImmutableArray<LoadDirective>>.GetInstance();
		PooledDictionary<string, SyntaxTree> instance4 = PooledDictionary<string, SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> instance5 = PooledDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>>.GetInstance();
		DeclarationTable declTable = DeclarationTable.Empty;
		foreach (SyntaxTree item in externalSyntaxTrees)
		{
			AppendAllSyntaxTrees(instance, item, scriptClassName, resolver, messageProvider, isSubmission, instance2, instance3, instance4, instance5, ref declTable);
		}
		return new State(instance.ToImmutableAndFree(), instance2.ToImmutableDictionaryAndFree(), instance3.ToImmutableDictionaryAndFree(), instance4.ToImmutableDictionaryAndFree(), instance5.ToImmutableDictionaryAndFree(), declTable);
	}

	public SyntaxAndDeclarationManager AddSyntaxTrees(IEnumerable<SyntaxTree> trees)
	{
		string scriptClassName = ScriptClassName;
		SourceReferenceResolver resolver = Resolver;
		CommonMessageProvider messageProvider = MessageProvider;
		bool isSubmission = IsSubmission;
		State lazyState = _lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.AddRange(trees);
		if (lazyState == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		ImmutableDictionary<SyntaxTree, int>.Builder builder = lazyState.OrdinalMap.ToBuilder();
		ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>>.Builder builder2 = lazyState.LoadDirectiveMap.ToBuilder();
		ImmutableDictionary<string, SyntaxTree>.Builder builder3 = lazyState.LoadedSyntaxTreeMap.ToBuilder();
		ImmutableDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>>.Builder builder4 = lazyState.RootNamespaces.ToBuilder();
		DeclarationTable declTable = lazyState.DeclarationTable;
		ArrayBuilder<SyntaxTree> instance = ArrayBuilder<SyntaxTree>.GetInstance();
		instance.AddRange(lazyState.SyntaxTrees);
		foreach (SyntaxTree tree in trees)
		{
			AppendAllSyntaxTrees(instance, tree, scriptClassName, resolver, messageProvider, isSubmission, builder, builder2, builder3, builder4, ref declTable);
		}
		lazyState = new State(instance.ToImmutableAndFree(), builder.ToImmutableDictionary(), builder2.ToImmutableDictionary(), builder3.ToImmutableDictionary(), builder4.ToImmutableDictionary(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, scriptClassName, resolver, messageProvider, isSubmission, lazyState);
	}

	private static void AppendAllSyntaxTrees(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, string scriptClassName, SourceReferenceResolver resolver, CommonMessageProvider messageProvider, bool isSubmission, IDictionary<SyntaxTree, int> ordinalMapBuilder, IDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMapBuilder, IDictionary<string, SyntaxTree> loadedSyntaxTreeMapBuilder, IDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> declMapBuilder, ref DeclarationTable declTable)
	{
		if (tree.Options.Kind == SourceCodeKind.Script)
		{
			AppendAllLoadedSyntaxTrees(treesBuilder, tree, scriptClassName, resolver, messageProvider, isSubmission, ordinalMapBuilder, loadDirectiveMapBuilder, loadedSyntaxTreeMapBuilder, declMapBuilder, ref declTable);
		}
		AddSyntaxTreeToDeclarationMapAndTable(tree, scriptClassName, isSubmission, declMapBuilder, ref declTable);
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);
	}

	private static void AppendAllLoadedSyntaxTrees(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, string scriptClassName, SourceReferenceResolver resolver, CommonMessageProvider messageProvider, bool isSubmission, IDictionary<SyntaxTree, int> ordinalMapBuilder, IDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMapBuilder, IDictionary<string, SyntaxTree> loadedSyntaxTreeMapBuilder, IDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> declMapBuilder, ref DeclarationTable declTable)
	{
		ArrayBuilder<LoadDirective> arrayBuilder = null;
		foreach (LoadDirectiveTriviaSyntax loadDirective in tree.GetCompilationUnitRoot().GetLoadDirectives())
		{
			SyntaxToken file = loadDirective.File;
			string text = (string)file.Value;
			if (text == null)
			{
				continue;
			}
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			string text2 = null;
			if (resolver == null)
			{
				instance.Add(messageProvider.CreateDiagnostic(8099, loadDirective.Location));
			}
			else
			{
				text2 = resolver.ResolveReference(text, tree.FilePath);
				if (text2 == null)
				{
					instance.Add(messageProvider.CreateDiagnostic(1504, file.GetLocation(), text, CSharpResources.CouldNotFindFile));
				}
				else if (!loadedSyntaxTreeMapBuilder.ContainsKey(text2))
				{
					try
					{
						SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(resolver.ReadText(text2), tree.Options, text2);
						loadedSyntaxTreeMapBuilder.Add(syntaxTree.FilePath, syntaxTree);
						AppendAllSyntaxTrees(treesBuilder, syntaxTree, scriptClassName, resolver, messageProvider, isSubmission, ordinalMapBuilder, loadDirectiveMapBuilder, loadedSyntaxTreeMapBuilder, declMapBuilder, ref declTable);
					}
					catch (Exception e)
					{
						instance.Add(CommonCompiler.ToFileReadDiagnostics(messageProvider, e, text2), file.GetLocation());
					}
				}
			}
			if (arrayBuilder == null)
			{
				arrayBuilder = ArrayBuilder<LoadDirective>.GetInstance();
			}
			arrayBuilder.Add(new LoadDirective(text2, instance.ToReadOnlyAndFree()));
		}
		if (arrayBuilder != null)
		{
			loadDirectiveMapBuilder.Add(tree, arrayBuilder.ToImmutableAndFree());
		}
	}

	private static void AddSyntaxTreeToDeclarationMapAndTable(SyntaxTree tree, string scriptClassName, bool isSubmission, IDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> declMapBuilder, ref DeclarationTable declTable)
	{
		Lazy<RootSingleNamespaceDeclaration> lazy = new Lazy<RootSingleNamespaceDeclaration>(() => DeclarationTreeBuilder.ForTree(tree, scriptClassName, isSubmission));
		declMapBuilder.Add(tree, lazy);
		declTable = declTable.AddRootDeclaration(lazy);
	}

	public SyntaxAndDeclarationManager RemoveSyntaxTrees(HashSet<SyntaxTree> trees)
	{
		State lazyState = _lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.RemoveAll((SyntaxTree t) => trees.Contains(t));
		if (lazyState == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		ImmutableArray<SyntaxTree> syntaxTrees = lazyState.SyntaxTrees;
		ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> immutableDictionary = lazyState.LoadDirectiveMap;
		ImmutableDictionary<string, SyntaxTree> immutableDictionary2 = lazyState.LoadedSyntaxTreeMap;
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		foreach (SyntaxTree tree in trees)
		{
			GetRemoveSet(tree, includeLoadedTrees: true, syntaxTrees, lazyState.OrdinalMap, immutableDictionary, immutableDictionary2, instance, out var _, out var _);
		}
		ArrayBuilder<SyntaxTree> instance2 = ArrayBuilder<SyntaxTree>.GetInstance();
		PooledDictionary<SyntaxTree, int> instance3 = PooledDictionary<SyntaxTree, int>.GetInstance();
		ImmutableDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>>.Builder builder = lazyState.RootNamespaces.ToBuilder();
		DeclarationTable declTable = lazyState.DeclarationTable;
		foreach (SyntaxTree item in syntaxTrees)
		{
			if (instance.Contains(item))
			{
				immutableDictionary = immutableDictionary.Remove(item);
				immutableDictionary2 = immutableDictionary2.Remove(item.FilePath);
				RemoveSyntaxTreeFromDeclarationMapAndTable(item, builder, ref declTable);
			}
			else if (!IsLoadedSyntaxTree(item, immutableDictionary2))
			{
				UpdateSyntaxTreesAndOrdinalMapOnly(instance2, item, instance3, immutableDictionary, immutableDictionary2);
			}
		}
		instance.Free();
		lazyState = new State(instance2.ToImmutableAndFree(), instance3.ToImmutableDictionaryAndFree(), immutableDictionary, immutableDictionary2, builder.ToImmutableDictionary(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, ScriptClassName, Resolver, MessageProvider, IsSubmission, lazyState);
	}

	private static void GetRemoveSet(SyntaxTree oldTree, bool includeLoadedTrees, ImmutableArray<SyntaxTree> syntaxTrees, ImmutableDictionary<SyntaxTree, int> syntaxTreeOrdinalMap, ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMap, ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap, HashSet<SyntaxTree> removeSet, out int totalReferencedTreeCount, out ImmutableArray<LoadDirective> oldLoadDirectives)
	{
		if (includeLoadedTrees && loadDirectiveMap.TryGetValue(oldTree, out oldLoadDirectives))
		{
			GetRemoveSetForLoadedTrees(oldLoadDirectives, loadDirectiveMap, loadedSyntaxTreeMap, removeSet);
		}
		else
		{
			oldLoadDirectives = ImmutableArray<LoadDirective>.Empty;
		}
		removeSet.Add(oldTree);
		totalReferencedTreeCount = removeSet.Count;
		if (removeSet.Count <= 1)
		{
			return;
		}
		for (int i = syntaxTreeOrdinalMap[oldTree] + 1; i < syntaxTrees.Length; i++)
		{
			SyntaxTree key = syntaxTrees[i];
			if (!loadDirectiveMap.TryGetValue(key, out var value))
			{
				continue;
			}
			foreach (LoadDirective item in value)
			{
				if (TryGetLoadedSyntaxTree(loadedSyntaxTreeMap, item, out var loadedTree))
				{
					removeSet.Remove(loadedTree);
				}
			}
		}
	}

	private static void GetRemoveSetForLoadedTrees(ImmutableArray<LoadDirective> loadDirectives, ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMap, ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap, HashSet<SyntaxTree> removeSet)
	{
		foreach (LoadDirective item in loadDirectives)
		{
			if (item.ResolvedPath != null && TryGetLoadedSyntaxTree(loadedSyntaxTreeMap, item, out var loadedTree) && removeSet.Add(loadedTree) && loadDirectiveMap.TryGetValue(loadedTree, out var value))
			{
				GetRemoveSetForLoadedTrees(value, loadDirectiveMap, loadedSyntaxTreeMap, removeSet);
			}
		}
	}

	private static void RemoveSyntaxTreeFromDeclarationMapAndTable(SyntaxTree tree, IDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>> declMap, ref DeclarationTable declTable)
	{
		Lazy<RootSingleNamespaceDeclaration> lazyRootDeclaration = declMap[tree];
		declTable = declTable.RemoveRootDeclaration(lazyRootDeclaration);
		declMap.Remove(tree);
	}

	public SyntaxAndDeclarationManager ReplaceSyntaxTree(SyntaxTree oldTree, SyntaxTree newTree)
	{
		State lazyState = _lazyState;
		ImmutableArray<SyntaxTree> immutableArray = ExternalSyntaxTrees.Replace(oldTree, newTree);
		if (lazyState == null)
		{
			return WithExternalSyntaxTrees(immutableArray);
		}
		IList<LoadDirectiveTriviaSyntax> loadDirectives = newTree.GetCompilationUnitRoot().GetLoadDirectives();
		bool flag = !oldTree.GetCompilationUnitRoot().GetLoadDirectives().SequenceEqual(loadDirectives);
		ImmutableArray<SyntaxTree> syntaxTrees = lazyState.SyntaxTrees;
		ImmutableDictionary<SyntaxTree, int> ordinalMap = lazyState.OrdinalMap;
		ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMap = lazyState.LoadDirectiveMap;
		ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap = lazyState.LoadedSyntaxTreeMap;
		PooledHashSet<SyntaxTree> instance = PooledHashSet<SyntaxTree>.GetInstance();
		GetRemoveSet(oldTree, flag, syntaxTrees, ordinalMap, loadDirectiveMap, loadedSyntaxTreeMap, instance, out var totalReferencedTreeCount, out var oldLoadDirectives);
		ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>>.Builder builder = loadDirectiveMap.ToBuilder();
		ImmutableDictionary<string, SyntaxTree>.Builder builder2 = loadedSyntaxTreeMap.ToBuilder();
		ImmutableDictionary<SyntaxTree, Lazy<RootSingleNamespaceDeclaration>>.Builder builder3 = lazyState.RootNamespaces.ToBuilder();
		DeclarationTable declTable = lazyState.DeclarationTable;
		foreach (SyntaxTree item in instance)
		{
			builder.Remove(item);
			builder2.Remove(item.FilePath);
			RemoveSyntaxTreeFromDeclarationMapAndTable(item, builder3, ref declTable);
		}
		instance.Free();
		int num = ordinalMap[oldTree];
		ImmutableArray<SyntaxTree> syntaxTrees2;
		if (flag)
		{
			ArrayBuilder<SyntaxTree> instance2 = ArrayBuilder<SyntaxTree>.GetInstance();
			PooledDictionary<SyntaxTree, int> instance3 = PooledDictionary<SyntaxTree, int>.GetInstance();
			for (int i = 0; i <= num - totalReferencedTreeCount; i++)
			{
				SyntaxTree syntaxTree = syntaxTrees[i];
				instance2.Add(syntaxTree);
				instance3.Add(syntaxTree, i);
			}
			AppendAllSyntaxTrees(instance2, newTree, ScriptClassName, Resolver, MessageProvider, IsSubmission, instance3, builder, builder2, builder3, ref declTable);
			for (int j = num + 1; j < syntaxTrees.Length; j++)
			{
				SyntaxTree tree = syntaxTrees[j];
				if (!IsLoadedSyntaxTree(tree, loadedSyntaxTreeMap))
				{
					UpdateSyntaxTreesAndOrdinalMapOnly(instance2, tree, instance3, loadDirectiveMap, loadedSyntaxTreeMap);
				}
			}
			syntaxTrees2 = instance2.ToImmutableAndFree();
			ordinalMap = instance3.ToImmutableDictionaryAndFree();
		}
		else
		{
			AddSyntaxTreeToDeclarationMapAndTable(newTree, ScriptClassName, IsSubmission, builder3, ref declTable);
			if (loadDirectives.Any())
			{
				builder[newTree] = oldLoadDirectives;
			}
			syntaxTrees2 = syntaxTrees.SetItem(num, newTree);
			ordinalMap = ordinalMap.Remove(oldTree);
			ordinalMap = ordinalMap.SetItem(newTree, num);
		}
		lazyState = new State(syntaxTrees2, ordinalMap, builder.ToImmutable(), builder2.ToImmutable(), builder3.ToImmutable(), declTable);
		return new SyntaxAndDeclarationManager(immutableArray, ScriptClassName, Resolver, MessageProvider, IsSubmission, lazyState);
	}

	internal SyntaxAndDeclarationManager WithExternalSyntaxTrees(ImmutableArray<SyntaxTree> trees)
	{
		return new SyntaxAndDeclarationManager(trees, ScriptClassName, Resolver, MessageProvider, IsSubmission, null);
	}

	internal static bool IsLoadedSyntaxTree(SyntaxTree tree, ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap)
	{
		if (loadedSyntaxTreeMap.TryGetValue(tree.FilePath, out var value))
		{
			return tree == value;
		}
		return false;
	}

	private static void UpdateSyntaxTreesAndOrdinalMapOnly(ArrayBuilder<SyntaxTree> treesBuilder, SyntaxTree tree, IDictionary<SyntaxTree, int> ordinalMapBuilder, ImmutableDictionary<SyntaxTree, ImmutableArray<LoadDirective>> loadDirectiveMap, ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap)
	{
		if (tree.Options.Kind == SourceCodeKind.Script && loadDirectiveMap.TryGetValue(tree, out var value))
		{
			foreach (LoadDirective item in value)
			{
				if (item.ResolvedPath != null && TryGetLoadedSyntaxTree(loadedSyntaxTreeMap, item, out var loadedTree))
				{
					UpdateSyntaxTreesAndOrdinalMapOnly(treesBuilder, loadedTree, ordinalMapBuilder, loadDirectiveMap, loadedSyntaxTreeMap);
				}
			}
		}
		treesBuilder.Add(tree);
		ordinalMapBuilder.Add(tree, ordinalMapBuilder.Count);
	}

	internal bool MayHaveReferenceDirectives()
	{
		return _lazyState?.DeclarationTable.ReferenceDirectives.Any() ?? ExternalSyntaxTrees.Any((SyntaxTree t) => t.HasReferenceOrLoadDirectives());
	}

	private static bool TryGetLoadedSyntaxTree(ImmutableDictionary<string, SyntaxTree> loadedSyntaxTreeMap, LoadDirective directive, out SyntaxTree loadedTree)
	{
		if (loadedSyntaxTreeMap.TryGetValue(directive.ResolvedPath, out loadedTree))
		{
			return true;
		}
		return false;
	}
}
