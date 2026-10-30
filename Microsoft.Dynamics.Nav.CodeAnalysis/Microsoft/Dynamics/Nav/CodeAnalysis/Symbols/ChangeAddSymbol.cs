using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ChangeAddSymbol : ChangeSymbol
{
	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	internal abstract Symbol Anchor { get; }

	internal abstract ImmutableArray<Symbol> AddedSymbols { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitChangeAdd(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitChangeAdd(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitChangeAdd(this, a);
	}
}
