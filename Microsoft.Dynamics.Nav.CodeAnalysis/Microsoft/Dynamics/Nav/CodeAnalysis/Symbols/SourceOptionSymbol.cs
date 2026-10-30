using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal sealed class SourceOptionSymbol : OptionSymbol
{
	private readonly SyntaxNode optionsDefinition;

	public override Symbol ContainingSymbol { get; }

	public override string Name { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override int Ordinal { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations { get; }

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray.Create(optionsDefinition.Parent.GetReference());

	public static SourceOptionSymbol Create(Symbol owner, string name, int ordinal, SyntaxNode optionsDefinition)
	{
		ImmutableArray<Location> locations = ImmutableArray.Create((Location)new SourceLocation(optionsDefinition));
		return new SourceOptionSymbol(owner, name, ordinal, optionsDefinition, locations);
	}

	private SourceOptionSymbol(Symbol containingSymbol, string name, int ordinal, SyntaxNode optionsDefinition, ImmutableArray<Location> locations)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		Ordinal = ordinal;
		this.optionsDefinition = optionsDefinition;
		Type = containingSymbol as TypeSymbol;
		DebugAssertHelper.Assert(Type != null);
		Locations = locations;
	}
}
