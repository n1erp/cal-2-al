using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class QueryDataItemSymbol : TypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IQueryDataItemSymbol, ISymbolWithType
{
	public abstract TableTypeSymbol RelatedTable { get; }

	public abstract int Id { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.QueryDataItemProperties;

	public TypeSymbol Type => RelatedTable ?? NavCorLib.None;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.QueryElement;

	public override SymbolKind Kind => SymbolKind.QueryDataItem;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	internal override TypeSymbol BaseType => RelatedTable;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryDataItem(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryDataItem(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryDataItem(this, a);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
