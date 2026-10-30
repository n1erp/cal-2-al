using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class LabelTypeSymbol : TypeSymbol, ILabelTypeSymbol, ISymbol, IEquatable<ISymbol>
{
	public override SymbolKind Kind => SymbolKind.NamedType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override NavTypeKind NavTypeKind => NavTypeKind.Label;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitLabelType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitLabelType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitLabelType(this, a);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal abstract string GetLabelText();

	string ILabelTypeSymbol.GetLabelText()
	{
		return GetLabelText();
	}
}
