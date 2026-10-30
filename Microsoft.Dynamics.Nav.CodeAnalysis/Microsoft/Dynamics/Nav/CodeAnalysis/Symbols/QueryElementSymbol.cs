using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class QueryElementSymbol : TypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, ISymbolWithType, IQueryElementSymbol
{
	public abstract int Id { get; }

	public abstract TypeSymbol Type { get; }

	public abstract FieldSymbol RelatedField { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.QueryElement;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryElement(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryElement(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryElement(this, a);
	}
}
