using System.Collections.Immutable;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class MissingNamespaceSymbol : NamespaceSymbol
{
	private readonly string _name;

	private readonly Symbol _containingSymbol;

	public override string Name => _name;

	public override Symbol ContainingSymbol => _containingSymbol;

	public override AssemblySymbol ContainingAssembly => _containingSymbol.ContainingAssembly;

	internal override NamespaceExtent Extent
	{
		get
		{
			if (_containingSymbol.Kind == SymbolKind.NetModule)
			{
				return new NamespaceExtent((ModuleSymbol)_containingSymbol);
			}
			return ((NamespaceSymbol)_containingSymbol).Extent;
		}
	}

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public MissingNamespaceSymbol(MissingModuleSymbol containingModule)
	{
		_containingSymbol = containingModule;
		_name = string.Empty;
	}

	public MissingNamespaceSymbol(NamespaceSymbol containingNamespace, string name)
	{
		_containingSymbol = containingNamespace;
		_name = name;
	}

	public override int GetHashCode()
	{
		return Hash.Combine(_containingSymbol.GetHashCode(), _name.GetHashCode());
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (obj is MissingNamespaceSymbol missingNamespaceSymbol && _name.Equals(missingNamespaceSymbol._name))
		{
			return _containingSymbol.Equals(missingNamespaceSymbol._containingSymbol);
		}
		return false;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers()
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<NamedTypeSymbol> GetTypeMembers(string name, int arity)
	{
		return ImmutableArray<NamedTypeSymbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
