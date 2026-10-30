using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal class SynthesizedSymbolContainer<T> : TypeSymbol where T : TypeSymbol
{
	private ImmutableArray<Symbol> members;

	public override string Name => ContainedSymbol.Name;

	public override string MetadataName => ContainedSymbol.MetadataName;

	public override TypeCategoryKind TypeCategory => ContainedSymbol.TypeCategory;

	public override SymbolKind Kind => ContainedSymbol.Kind;

	public override Symbol ContainingSymbol => ContainedSymbol.ContainingSymbol;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ContainedSymbol.Locations;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ContainedSymbol.DeclaringSyntaxReferences;

	public T ContainedSymbol { get; }

	public SynthesizedSymbolContainer(T originalSymbol)
	{
		ContainedSymbol = originalSymbol;
		members = ImmutableArray.Create((Symbol)originalSymbol);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return members;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		if (SemanticFacts.IsSameName(ContainedSymbol.Name, name))
		{
			return members;
		}
		return ImmutableArray<Symbol>.Empty;
	}
}
