using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceNamespaceSymbol : NamespaceSymbol
{
	private struct NameToSymbolMapBuilder(int capacity)
	{
		private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>(capacity, StringOrdinalComparer.Instance);

		public void Add(NamespaceOrTypeSymbol symbol)
		{
			string name = symbol.Name;
			if (_dictionary.TryGetValue(name, out var value))
			{
				ArrayBuilder<NamespaceOrTypeSymbol> arrayBuilder = value as ArrayBuilder<NamespaceOrTypeSymbol>;
				if (arrayBuilder == null)
				{
					arrayBuilder = ArrayBuilder<NamespaceOrTypeSymbol>.GetInstance();
					arrayBuilder.Add((NamespaceOrTypeSymbol)value);
					_dictionary[name] = arrayBuilder;
				}
				arrayBuilder.Add(symbol);
			}
			else
			{
				_dictionary[name] = symbol;
			}
		}

		public Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> CreateMap()
		{
			Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> dictionary = new Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>>(_dictionary.Count, StringOrdinalComparer.Instance);
			foreach (KeyValuePair<string, object> item in _dictionary)
			{
				object value = item.Value;
				ImmutableArray<NamespaceOrTypeSymbol> value2;
				if (value is ArrayBuilder<NamespaceOrTypeSymbol> arrayBuilder)
				{
					bool flag = false;
					for (int i = 0; i < arrayBuilder.Count; i++)
					{
						if (flag)
						{
							break;
						}
						flag |= arrayBuilder[i].Kind == SymbolKind.Namespace;
					}
					value2 = (flag ? arrayBuilder.ToImmutable() : StaticCast<NamespaceOrTypeSymbol>.From(arrayBuilder.ToDowncastedImmutable<NamedTypeSymbol>()));
					arrayBuilder.Free();
				}
				else
				{
					NamespaceOrTypeSymbol namespaceOrTypeSymbol = (NamespaceOrTypeSymbol)value;
					value2 = ((namespaceOrTypeSymbol.Kind == SymbolKind.Namespace) ? ImmutableArray.Create(namespaceOrTypeSymbol) : StaticCast<NamespaceOrTypeSymbol>.From(ImmutableArray.Create((NamedTypeSymbol)namespaceOrTypeSymbol)));
				}
				dictionary.Add(item.Key, value2);
			}
			return dictionary;
		}
	}

	private readonly SourceModuleSymbol _module;

	private readonly Symbol _container;

	private readonly MergedNamespaceDeclaration _mergedDeclaration;

	private SymbolCompletionState _state;

	private ImmutableArray<Location> _locations;

	private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> _nameToMembersMap;

	private Dictionary<string, ImmutableArray<NamedTypeSymbol>> _nameToTypeMembersMap;

	private ImmutableArray<Symbol> _lazyAllMembers;

	private ImmutableArray<NamedTypeSymbol> _lazyTypeMembersUnordered;

	private const int LazyAllMembersIsSorted = 1;

	private int _flags;

	private LexicalSortKey _lazyLexicalSortKey = LexicalSortKey.NotInitialized;

	private static readonly Func<SingleNamespaceDeclaration, SyntaxReference> s_declaringSyntaxReferencesSelector = (SingleNamespaceDeclaration d) => new NamespaceDeclarationSyntaxReference(d.SyntaxReference);

	internal MergedNamespaceDeclaration MergedDeclaration => _mergedDeclaration;

	public override Symbol ContainingSymbol => _container;

	public override AssemblySymbol ContainingAssembly => _module.ContainingAssembly;

	public override string Name => _mergedDeclaration.Name;

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (_locations.IsDefault)
			{
				ImmutableInterlocked.InterlockedCompareExchange(ref _locations, _mergedDeclaration.NameLocations, default);
			}
			return _locations;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ComputeDeclaringReferencesCore();

	internal override ModuleSymbol ContainingModule => _module;

	internal override NamespaceExtent Extent => new NamespaceExtent(_module);

	internal SourceNamespaceSymbol(SourceModuleSymbol module, Symbol container, MergedNamespaceDeclaration mergedDeclaration, DiagnosticBag diagnostics)
	{
		_module = module;
		_container = container;
		_mergedDeclaration = mergedDeclaration;
		foreach (SingleNamespaceDeclaration declaration in mergedDeclaration.Declarations)
		{
			diagnostics.AddRange(declaration.Diagnostics);
		}
	}

	internal IEnumerable<Imports> GetBoundImportsMerged()
	{
		CSharpCompilation compilation = DeclaringCompilation;
		foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
		{
			if (declaration.HasUsings || declaration.HasExternAliases)
			{
				yield return compilation.GetImports(declaration);
			}
		}
	}

	internal override LexicalSortKey GetLexicalSortKey()
	{
		if (!_lazyLexicalSortKey.IsInitialized)
		{
			_lazyLexicalSortKey.SetFrom(_mergedDeclaration.GetLexicalSortKey(DeclaringCompilation));
		}
		return _lazyLexicalSortKey;
	}

	private ImmutableArray<SyntaxReference> ComputeDeclaringReferencesCore()
	{
		return _mergedDeclaration.Declarations.SelectAsArray(s_declaringSyntaxReferencesSelector);
	}

	internal override ImmutableArray<Symbol> GetMembersUnordered()
	{
		ImmutableArray<Symbol> lazyAllMembers = _lazyAllMembers;
		if (lazyAllMembers.IsDefault)
		{
			ImmutableArray<Symbol> value = StaticCast<Symbol>.From(GetNameToMembersMap().Flatten());
			ImmutableInterlocked.InterlockedInitialize(ref _lazyAllMembers, value);
			lazyAllMembers = _lazyAllMembers;
		}
		return lazyAllMembers;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if ((_flags & 1) != 0)
		{
			return _lazyAllMembers;
		}
		ImmutableArray<Symbol> immutableArray = GetMembersUnordered();
		if (immutableArray.Length >= 2)
		{
			immutableArray = immutableArray.Sort(LexicalOrderSymbolComparer.Instance);
			ImmutableInterlocked.InterlockedExchange(ref _lazyAllMembers, immutableArray);
		}
		ThreadSafeFlagOperations.Set(ref _flags, 1);
		return immutableArray;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (!GetNameToMembersMap().TryGetValue(name, out var value))
		{
			return ImmutableArray<Symbol>.Empty;
		}
		return value.Cast<NamespaceOrTypeSymbol, Symbol>();
	}

	internal override ImmutableArray<NamedTypeSymbol> GetTypeMembersUnordered()
	{
		if (_lazyTypeMembersUnordered.IsDefault)
		{
			ImmutableArray<NamedTypeSymbol> value = GetNameToTypeMembersMap().Flatten();
			ImmutableInterlocked.InterlockedInitialize(ref _lazyTypeMembersUnordered, value);
		}
		return _lazyTypeMembersUnordered;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return GetNameToTypeMembersMap().Flatten(LexicalOrderSymbolComparer.Instance);
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		if (!GetNameToTypeMembersMap().TryGetValue(name, out var value))
		{
			return ImmutableArray<NamedTypeSymbol>.Empty;
		}
		return value;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return GetTypeMembers(name).WhereAsArray((NamedTypeSymbol s) => s.Arity == arity);
	}

	private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> GetNameToMembersMap()
	{
		if (_nameToMembersMap == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			if (Interlocked.CompareExchange(ref _nameToMembersMap, MakeNameToMembersMap(instance), null) == null)
			{
				DeclaringCompilation.DeclarationDiagnostics.AddRange(instance);
				RegisterDeclaredCorTypes();
				DeclaringCompilation.SymbolDeclaredEvent(this);
				_state.NotePartComplete(CompletionPart.Members);
			}
			instance.Free();
		}
		return _nameToMembersMap;
	}

	private Dictionary<string, ImmutableArray<NamedTypeSymbol>> GetNameToTypeMembersMap()
	{
		if (_nameToTypeMembersMap == null)
		{
			Dictionary<string, ImmutableArray<NamedTypeSymbol>> dictionary = new Dictionary<string, ImmutableArray<NamedTypeSymbol>>(StringOrdinalComparer.Instance);
			foreach (KeyValuePair<string, ImmutableArray<NamespaceOrTypeSymbol>> item in GetNameToMembersMap())
			{
				ImmutableArray<NamespaceOrTypeSymbol> value = item.Value;
				bool flag = false;
				bool flag2 = false;
				foreach (NamespaceOrTypeSymbol item2 in value)
				{
					if (item2.Kind == SymbolKind.NamedType)
					{
						flag = true;
						if (flag2)
						{
							break;
						}
					}
					else
					{
						flag2 = true;
						if (flag)
						{
							break;
						}
					}
				}
				if (flag)
				{
					if (flag2)
					{
						dictionary.Add(item.Key, value.OfType<NamedTypeSymbol>().AsImmutable());
					}
					else
					{
						dictionary.Add(item.Key, value.As<NamedTypeSymbol>());
					}
				}
			}
			Interlocked.CompareExchange(ref _nameToTypeMembersMap, dictionary, null);
		}
		return _nameToTypeMembersMap;
	}

	private Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> MakeNameToMembersMap(DiagnosticBag diagnostics)
	{
		NameToSymbolMapBuilder nameToSymbolMapBuilder = new NameToSymbolMapBuilder(_mergedDeclaration.Children.Length);
		foreach (MergedNamespaceOrTypeDeclaration child in _mergedDeclaration.Children)
		{
			nameToSymbolMapBuilder.Add(BuildSymbol(child, diagnostics));
		}
		Dictionary<string, ImmutableArray<NamespaceOrTypeSymbol>> dictionary = nameToSymbolMapBuilder.CreateMap();
		Symbol[] array = new Symbol[10];
		MergedNamespaceSymbol mergedNamespaceSymbol = null;
		if (ContainingAssembly.Modules.Length > 1)
		{
			mergedNamespaceSymbol = ContainingAssembly.GetAssemblyNamespace(this) as MergedNamespaceSymbol;
		}
		foreach (string key in dictionary.Keys)
		{
			Array.Clear(array, 0, array.Length);
			foreach (NamespaceOrTypeSymbol item in dictionary[key])
			{
				NamedTypeSymbol namedTypeSymbol = item as NamedTypeSymbol;
				int num = namedTypeSymbol?.Arity ?? 0;
				if (num >= array.Length)
				{
					Array.Resize(ref array, num + 1);
				}
				Symbol symbol = array[num];
				if ((object)symbol == null && (object)mergedNamespaceSymbol != null)
				{
					foreach (NamespaceSymbol constituentNamespace in mergedNamespaceSymbol.ConstituentNamespaces)
					{
						if ((object)constituentNamespace != this)
						{
							ImmutableArray<NamedTypeSymbol> typeMembers = constituentNamespace.GetTypeMembers(item.Name, num);
							if (typeMembers.Length > 0)
							{
								symbol = typeMembers[0];
								break;
							}
						}
					}
				}
				if ((object)symbol != null)
				{
					SourceNamedTypeSymbol obj = namedTypeSymbol as SourceNamedTypeSymbol;
					if ((object)obj != null && obj.IsPartial)
					{
						SourceNamedTypeSymbol obj2 = symbol as SourceNamedTypeSymbol;
						if ((object)obj2 != null && obj2.IsPartial)
						{
							diagnostics.Add(ErrorCode.ERR_PartialTypeKindConflict, item.Locations[0], item);
							goto IL_01e1;
						}
					}
					diagnostics.Add(ErrorCode.ERR_DuplicateNameInNS, item.Locations[0], key, this);
				}
				goto IL_01e1;
				IL_01e1:
				array[num] = item;
				if ((object)namedTypeSymbol != null)
				{
					Accessibility declaredAccessibility = namedTypeSymbol.DeclaredAccessibility;
					if (declaredAccessibility != Accessibility.Public && declaredAccessibility != Accessibility.Internal)
					{
						diagnostics.Add(ErrorCode.ERR_NoNamespacePrivate, item.Locations[0]);
					}
				}
			}
		}
		return dictionary;
	}

	private NamespaceOrTypeSymbol BuildSymbol(MergedNamespaceOrTypeDeclaration declaration, DiagnosticBag diagnostics)
	{
		switch (declaration.Kind)
		{
		case DeclarationKind.Namespace:
			return new SourceNamespaceSymbol(_module, this, (MergedNamespaceDeclaration)declaration, diagnostics);
		case DeclarationKind.Class:
		case DeclarationKind.Interface:
		case DeclarationKind.Struct:
		case DeclarationKind.Enum:
		case DeclarationKind.Delegate:
			return new SourceNamedTypeSymbol(this, (MergedTypeDeclaration)declaration, diagnostics);
		case DeclarationKind.Script:
		case DeclarationKind.Submission:
		case DeclarationKind.ImplicitClass:
			return new ImplicitNamedTypeSymbol(this, (MergedTypeDeclaration)declaration, diagnostics);
		default:
			throw ExceptionUtilities.UnexpectedValue(declaration.Kind);
		}
	}

	private void RegisterDeclaredCorTypes()
	{
		AssemblySymbol containingAssembly = ContainingAssembly;
		if (!containingAssembly.KeepLookingForDeclaredSpecialTypes)
		{
			return;
		}
		foreach (ImmutableArray<NamespaceOrTypeSymbol> value in _nameToMembersMap.Values)
		{
			foreach (NamespaceOrTypeSymbol item in value)
			{
				if (item is NamedTypeSymbol { SpecialType: not SpecialType.None } namedTypeSymbol)
				{
					containingAssembly.RegisterDeclaredSpecialType(namedTypeSymbol);
					if (!containingAssembly.KeepLookingForDeclaredSpecialTypes)
					{
						return;
					}
				}
			}
		}
	}

	internal override bool IsDefinedInSourceTree(SyntaxTree tree, TextSpan? definedWithinSpan, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (IsGlobalNamespace)
		{
			return true;
		}
		foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
		{
			cancellationToken.ThrowIfCancellationRequested();
			SyntaxReference syntaxReference = declaration.SyntaxReference;
			if (syntaxReference.SyntaxTree == tree)
			{
				if (!definedWithinSpan.HasValue)
				{
					return true;
				}
				if (NamespaceDeclarationSyntaxReference.GetSyntax(syntaxReference, cancellationToken).FullSpan.IntersectsWith(definedWithinSpan.Value))
				{
					return true;
				}
			}
		}
		return false;
	}

	internal override void ForceComplete(SourceLocation locationOpt, CancellationToken cancellationToken)
	{
		while (true)
		{
			cancellationToken.ThrowIfCancellationRequested();
			CompletionPart nextIncompletePart = _state.NextIncompletePart;
			switch (nextIncompletePart)
			{
			case CompletionPart.Members:
				GetNameToMembersMap();
				break;
			case CompletionPart.MembersCompleted:
			{
				foreach (SingleNamespaceDeclaration declaration in _mergedDeclaration.Declarations)
				{
					if ((locationOpt == null || locationOpt.SourceTree == declaration.SyntaxReference.SyntaxTree) && (declaration.HasUsings || declaration.HasExternAliases))
					{
						DeclaringCompilation.GetImports(declaration).Complete(cancellationToken);
					}
				}
				ImmutableArray<Symbol> members = GetMembers();
				bool flag = true;
				if (DeclaringCompilation.Options.ConcurrentBuild)
				{
					ParallelOptions parallelOptions = (cancellationToken.CanBeCanceled ? new ParallelOptions
					{
						CancellationToken = cancellationToken
					} : CSharpCompilation.DefaultParallelOptions);
					Parallel.For(0, members.Length, parallelOptions, UICultureUtilities.WithCurrentUICulture((int i) =>
					{
						Symbol member = members[i];
						Symbol.ForceCompleteMemberByLocation(locationOpt, member, cancellationToken);
					}));
					foreach (Symbol item in members)
					{
						if (!item.HasComplete(CompletionPart.All))
						{
							flag = false;
							break;
						}
					}
				}
				else
				{
					foreach (Symbol item2 in members)
					{
						Symbol.ForceCompleteMemberByLocation(locationOpt, item2, cancellationToken);
						flag = flag && item2.HasComplete(CompletionPart.All);
					}
				}
				if (flag)
				{
					_state.NotePartComplete(CompletionPart.MembersCompleted);
					break;
				}
				CompletionPart part = ((locationOpt == null) ? CompletionPart.NamespaceSymbolAll : CompletionPart.Members);
				_state.SpinWaitComplete(part, cancellationToken);
				return;
			}
			case CompletionPart.None:
				return;
			default:
				_state.NotePartComplete(CompletionPart.AssemblySymbolAll | CompletionPart.ReturnTypeAttributes | CompletionPart.Parameters | CompletionPart.Type | CompletionPart.FinishInterfaces | CompletionPart.TypeParameters | CompletionPart.TypeMembers | CompletionPart.SynthesizedExplicitImplementations | CompletionPart.StartMemberChecks | CompletionPart.FinishMemberChecks);
				break;
			}
			_state.SpinWaitComplete(nextIncompletePart, cancellationToken);
		}
	}

	internal override bool HasComplete(CompletionPart part)
	{
		return _state.HasComplete(part);
	}
}
