using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class FieldNameListSymbol : Symbol, IFieldNameListSymbol, ISymbol, IEquatable<ISymbol>
{
	public abstract ImmutableArray<FieldSymbol> Fields { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	ImmutableArray<IFieldSymbol> IFieldNameListSymbol.Fields => StaticCast<IFieldSymbol>.From(Fields);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitFieldNameList(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitFieldNameList(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitFieldNameList(this, a);
	}
}
