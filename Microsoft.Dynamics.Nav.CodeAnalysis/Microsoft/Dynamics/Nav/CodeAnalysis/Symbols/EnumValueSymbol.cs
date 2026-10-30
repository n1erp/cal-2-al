using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class EnumValueSymbol : OptionSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IEnumValueSymbol
{
	public override SymbolKind Kind { get; } = SymbolKind.EnumValue;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitEnumValue(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitEnumValue(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitEnumValue(this, a);
	}
}
