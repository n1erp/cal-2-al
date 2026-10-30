using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ChangeMoveSymbol : ChangeSymbol
{
	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	internal abstract Symbol Anchor { get; }

	internal abstract ImmutableArray<Symbol> MovedSymbols { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitChangeMove(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitChangeMove(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitChangeMove(this, a);
	}
}
