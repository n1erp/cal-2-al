using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal sealed class SynthesizedControlSymbol : ControlSymbol
{
	public override ControlKind ControlKind { get; }

	public override Symbol ContainingSymbol { get; }

	public override string MetadataName { get; }

	public override string Name { get; }

	public override int Id { get; }

	public override ImmutableArray<ControlSymbol> Controls => ImmutableArray<ControlSymbol>.Empty;

	public override ImmutableArray<ActionSymbol> Actions => ImmutableArray<ActionSymbol>.Empty;

	public override ImmutableArray<PropertySymbol> Properties => ImmutableArray<PropertySymbol>.Empty;

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override TypeSymbol Type => NavCorLib.None;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override PageTypeSymbol RelatedPartSymbol => null;

	public override ControlAddInTypeSymbol RelatedControlAddInSymbol => null;

	public override bool IsImplicitlyDeclared => true;

	internal override bool RequiresCompletion => false;

	public override FieldSymbol RelatedFieldSymbol => null;

	public static SynthesizedControlSymbol Create(Symbol owner, AreaKind areaKind, int id)
	{
		return new SynthesizedControlSymbol(owner, ControlKind.Area, areaKind.ToString(), id);
	}

	private SynthesizedControlSymbol(Symbol containingSymbol, ControlKind kind, string name, int id)
	{
		ContainingSymbol = containingSymbol;
		Name = name;
		MetadataName = Name;
		Id = id;
		ControlKind = kind;
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
