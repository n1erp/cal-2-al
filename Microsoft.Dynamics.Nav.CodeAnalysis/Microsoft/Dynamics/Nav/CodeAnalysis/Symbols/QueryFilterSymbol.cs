using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class QueryFilterSymbol : QueryElementSymbol, IQueryFilterSymbol, IQueryElementSymbol, ISymbol, IEquatable<ISymbol>
{
	public override SymbolKind Kind => SymbolKind.QueryFilter;

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.QueryFilterProperties;

	public override TypeSymbol Type => NavCorLib.QueryFilterType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryFilter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryFilter(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryFilter(this, a);
	}
}
