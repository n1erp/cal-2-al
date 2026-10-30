using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class NavTypeSymbol : TypeSymbol
{
	public override string Name { get; }

	public override SymbolKind Kind => SymbolKind.NamedType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Simple;

	public override NavTypeKind NavTypeKind { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override Symbol ContainingSymbol => null;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitNavType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitNavType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitNavType(this, a);
	}

	internal NavTypeSymbol(NavTypeKind navTypeKind)
	{
		NavTypeKind = navTypeKind;
		Name = navTypeKind.ToString();
	}
}
