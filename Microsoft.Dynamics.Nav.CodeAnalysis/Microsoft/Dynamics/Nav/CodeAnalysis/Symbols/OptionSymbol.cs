using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class OptionSymbol : Symbol, IOptionSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithType, ISymbolWithId
{
	public abstract TypeSymbol Type { get; }

	public abstract int Ordinal { get; }

	public override SymbolKind Kind { get; } = SymbolKind.Option;

	ITypeSymbol IOptionSymbol.Type => Type;

	int ISymbolWithId.Id => Ordinal;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitOption(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitOption(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitOption(this, a);
	}
}
