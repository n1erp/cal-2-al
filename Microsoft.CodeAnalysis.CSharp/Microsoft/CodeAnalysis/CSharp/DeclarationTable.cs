using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DeclarationTable
{
	private class Cache
	{
		internal readonly Lazy<MergedNamespaceDeclaration> MergedRoot;

		internal readonly Lazy<ISet<string>> TypeNames;

		internal readonly Lazy<ISet<string>> NamespaceNames;

		internal readonly Lazy<ImmutableArray<ReferenceDirective>> ReferenceDirectives;

		public Cache(DeclarationTable table)
		{
			Cache cache = this;
			MergedRoot = new Lazy<MergedNamespaceDeclaration>(() => MergedNamespaceDeclaration.Create(((IEnumerable<SingleNamespaceDeclaration>)table._allOlderRootDeclarations.InInsertionOrder).AsImmutable()));
			TypeNames = new Lazy<ISet<string>>(() => GetTypeNames(cache.MergedRoot.Value));
			NamespaceNames = new Lazy<ISet<string>>(() => GetNamespaceNames(cache.MergedRoot.Value));
			ReferenceDirectives = new Lazy<ImmutableArray<ReferenceDirective>>(() => cache.MergedRoot.Value.Declarations.OfType<RootSingleNamespaceDeclaration>().SelectMany((RootSingleNamespaceDeclaration r) => r.ReferenceDirectives).AsImmutable());
		}
	}

	private sealed class RootNamespaceLocationComparer : IComparer<SingleNamespaceDeclaration>
	{
		private readonly CSharpCompilation _compilation;

		internal RootNamespaceLocationComparer(CSharpCompilation compilation)
		{
			_compilation = compilation;
		}

		public int Compare(SingleNamespaceDeclaration x, SingleNamespaceDeclaration y)
		{
			return _compilation.CompareSourceLocations(x.SyntaxReference, y.SyntaxReference);
		}
	}

	public static readonly DeclarationTable Empty = new DeclarationTable(ImmutableSetWithInsertionOrder<RootSingleNamespaceDeclaration>.Empty, null, null);

	private readonly ImmutableSetWithInsertionOrder<RootSingleNamespaceDeclaration> _allOlderRootDeclarations;

	private readonly Lazy<RootSingleNamespaceDeclaration> _latestLazyRootDeclaration;

	private readonly Cache _cache;

	private MergedNamespaceDeclaration _mergedRoot;

	private readonly Lazy<ICollection<string>> _typeNames;

	private readonly Lazy<ICollection<string>> _namespaceNames;

	private readonly Lazy<ICollection<ReferenceDirective>> _referenceDirectives;

	private static readonly Predicate<Declaration> s_isNamespacePredicate = (Declaration d) => d.Kind == DeclarationKind.Namespace;

	private static readonly Predicate<Declaration> s_isTypePredicate = (Declaration d) => d.Kind != DeclarationKind.Namespace;

	public ICollection<string> TypeNames => _typeNames.Value;

	public ICollection<string> NamespaceNames => _namespaceNames.Value;

	public IEnumerable<ReferenceDirective> ReferenceDirectives => _referenceDirectives.Value;

	private DeclarationTable(ImmutableSetWithInsertionOrder<RootSingleNamespaceDeclaration> allOlderRootDeclarations, Lazy<RootSingleNamespaceDeclaration> latestLazyRootDeclaration, Cache cache)
	{
		_allOlderRootDeclarations = allOlderRootDeclarations;
		_latestLazyRootDeclaration = latestLazyRootDeclaration;
		_cache = cache ?? new Cache(this);
		_typeNames = new Lazy<ICollection<string>>(GetMergedTypeNames);
		_namespaceNames = new Lazy<ICollection<string>>(GetMergedNamespaceNames);
		_referenceDirectives = new Lazy<ICollection<ReferenceDirective>>(GetMergedReferenceDirectives);
	}

	public DeclarationTable AddRootDeclaration(Lazy<RootSingleNamespaceDeclaration> lazyRootDeclaration)
	{
		if (_latestLazyRootDeclaration == null)
		{
			return new DeclarationTable(_allOlderRootDeclarations, lazyRootDeclaration, _cache);
		}
		return new DeclarationTable(_allOlderRootDeclarations.Add(_latestLazyRootDeclaration.Value), lazyRootDeclaration, null);
	}

	public DeclarationTable RemoveRootDeclaration(Lazy<RootSingleNamespaceDeclaration> lazyRootDeclaration)
	{
		if (_latestLazyRootDeclaration == lazyRootDeclaration)
		{
			return new DeclarationTable(_allOlderRootDeclarations, null, _cache);
		}
		return new DeclarationTable(_allOlderRootDeclarations.Remove(lazyRootDeclaration.Value), _latestLazyRootDeclaration, null);
	}

	public MergedNamespaceDeclaration GetMergedRoot(CSharpCompilation compilation)
	{
		if (_mergedRoot == null)
		{
			Interlocked.CompareExchange(ref _mergedRoot, CalculateMergedRoot(compilation), null);
		}
		return _mergedRoot;
	}

	internal MergedNamespaceDeclaration CalculateMergedRoot(CSharpCompilation compilation)
	{
		MergedNamespaceDeclaration value = _cache.MergedRoot.Value;
		if (_latestLazyRootDeclaration == null)
		{
			return value;
		}
		if (value == null)
		{
			return MergedNamespaceDeclaration.Create(_latestLazyRootDeclaration.Value);
		}
		ImmutableArray<SingleNamespaceDeclaration> declarations = value.Declarations;
		ArrayBuilder<SingleNamespaceDeclaration> instance = ArrayBuilder<SingleNamespaceDeclaration>.GetInstance(declarations.Length + 1);
		instance.AddRange(declarations);
		instance.Add(_latestLazyRootDeclaration.Value);
		if (compilation != null)
		{
			instance.Sort(new RootNamespaceLocationComparer(compilation));
		}
		return MergedNamespaceDeclaration.Create(instance.ToImmutableAndFree());
	}

	private ICollection<string> GetMergedTypeNames()
	{
		ISet<string> value = _cache.TypeNames.Value;
		if (_latestLazyRootDeclaration == null)
		{
			return value;
		}
		return UnionCollection<string>.Create(value, GetTypeNames(_latestLazyRootDeclaration.Value));
	}

	private ICollection<string> GetMergedNamespaceNames()
	{
		ISet<string> value = _cache.NamespaceNames.Value;
		if (_latestLazyRootDeclaration == null)
		{
			return value;
		}
		return UnionCollection<string>.Create(value, GetNamespaceNames(_latestLazyRootDeclaration.Value));
	}

	private ICollection<ReferenceDirective> GetMergedReferenceDirectives()
	{
		ImmutableArray<ReferenceDirective> value = _cache.ReferenceDirectives.Value;
		if (_latestLazyRootDeclaration == null)
		{
			return value;
		}
		return UnionCollection<ReferenceDirective>.Create(value, _latestLazyRootDeclaration.Value.ReferenceDirectives);
	}

	private static ISet<string> GetTypeNames(Declaration declaration)
	{
		return GetNames(declaration, s_isTypePredicate);
	}

	private static ISet<string> GetNamespaceNames(Declaration declaration)
	{
		return GetNames(declaration, s_isNamespacePredicate);
	}

	private static ISet<string> GetNames(Declaration declaration, Predicate<Declaration> predicate)
	{
		HashSet<string> hashSet = new HashSet<string>();
		Stack<Declaration> stack = new Stack<Declaration>();
		stack.Push(declaration);
		while (stack.Count > 0)
		{
			Declaration declaration2 = stack.Pop();
			if (declaration2 != null)
			{
				if (predicate(declaration2))
				{
					hashSet.Add(declaration2.Name);
				}
				foreach (Declaration child in declaration2.Children)
				{
					stack.Push(child);
				}
			}
		}
		return SpecializedCollections.ReadOnlySet(hashSet);
	}

	public static bool ContainsName(MergedNamespaceDeclaration mergedRoot, string name, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return ContainsNameHelper(mergedRoot, (string n) => n == name, filter, (SingleTypeDeclaration t) => t.MemberNames.Contains(name), cancellationToken);
	}

	public static bool ContainsName(MergedNamespaceDeclaration mergedRoot, Func<string, bool> predicate, SymbolFilter filter, CancellationToken cancellationToken)
	{
		return ContainsNameHelper(mergedRoot, predicate, filter, (SingleTypeDeclaration t) =>
		{
			foreach (string memberName in t.MemberNames)
			{
				if (predicate(memberName))
				{
					return true;
				}
			}
			return false;
		}, cancellationToken);
	}

	private static bool ContainsNameHelper(MergedNamespaceDeclaration mergedRoot, Func<string, bool> predicate, SymbolFilter filter, Func<SingleTypeDeclaration, bool> typePredicate, CancellationToken cancellationToken)
	{
		bool flag = (filter & SymbolFilter.Namespace) == SymbolFilter.Namespace;
		bool flag2 = (filter & SymbolFilter.Type) == SymbolFilter.Type;
		bool flag3 = (filter & SymbolFilter.Member) == SymbolFilter.Member;
		Stack<MergedNamespaceOrTypeDeclaration> stack = new Stack<MergedNamespaceOrTypeDeclaration>();
		stack.Push(mergedRoot);
		while (stack.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
			MergedNamespaceOrTypeDeclaration mergedNamespaceOrTypeDeclaration = stack.Pop();
			if (mergedNamespaceOrTypeDeclaration == null)
			{
				continue;
			}
			if (mergedNamespaceOrTypeDeclaration.Kind == DeclarationKind.Namespace)
			{
				if (flag && predicate(mergedNamespaceOrTypeDeclaration.Name))
				{
					return true;
				}
			}
			else
			{
				if (flag2 && predicate(mergedNamespaceOrTypeDeclaration.Name))
				{
					return true;
				}
				if (flag3)
				{
					foreach (SingleTypeDeclaration declaration in ((MergedTypeDeclaration)mergedNamespaceOrTypeDeclaration).Declarations)
					{
						if (typePredicate(declaration))
						{
							return true;
						}
					}
				}
			}
			foreach (Declaration child in mergedNamespaceOrTypeDeclaration.Children)
			{
				if (child is MergedNamespaceOrTypeDeclaration mergedNamespaceOrTypeDeclaration2 && ((flag3 | flag2) || mergedNamespaceOrTypeDeclaration2.Kind == DeclarationKind.Namespace))
				{
					stack.Push(mergedNamespaceOrTypeDeclaration2);
				}
			}
		}
		return false;
	}
}
