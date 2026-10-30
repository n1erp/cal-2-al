using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class BuiltInAttributeArgumentSymbol : AttributeArgumentSymbol
{
	public override string Name { get; }

	public override TypeSymbol Type { get; }

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol { get; }

	public override string ValueText { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitBuiltInAttributeArgument(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitBuiltInAttributeArgument(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitBuiltInAttributeArgument(this, a);
	}

	public BuiltInAttributeArgumentSymbol(Symbol containingSymbol, TypeSymbol type, string name)
	{
		ContainingSymbol = containingSymbol;
		Type = type;
		ValueText = name;
		Name = name;
	}

	public override string GetDocumentation()
	{
		return DocumentationFacts.GetDocumentation(this);
	}
}
