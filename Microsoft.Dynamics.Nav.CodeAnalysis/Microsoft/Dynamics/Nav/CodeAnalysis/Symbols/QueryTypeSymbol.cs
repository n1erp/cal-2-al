using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class QueryTypeSymbol : ApplicationObjectTypeSymbol, IQueryTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, IRootTypeSymbol
{
	private readonly Lazy<ImmutableArray<QueryDataItemSymbol>> flattenedDataItems;

	private readonly Lazy<ImmutableArray<QueryColumnSymbol>> flattenedColumns;

	private readonly Lazy<ImmutableArray<QueryFilterSymbol>> flattenedFilters;

	public override SymbolKind Kind => SymbolKind.Query;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind => NavTypeKind.Query;

	public override string MetadataName => "Query" + Id.ToString(CultureInfo.InvariantCulture);

	public abstract QueryTypeKind QueryType { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.QueryProperties;

	internal override TypeSymbol BaseType => QueryInstanceClassTypeSymbol.Instance;

	internal ImmutableArray<QueryDataItemSymbol> FlattenedDataItems => flattenedDataItems.Value;

	internal ImmutableArray<QueryColumnSymbol> FlattenedColumns => flattenedColumns.Value;

	internal ImmutableArray<QueryFilterSymbol> FlattenedFilters => flattenedFilters.Value;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryType(this, a);
	}

	protected QueryTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
		flattenedDataItems = new Lazy<ImmutableArray<QueryDataItemSymbol>>(() => new FlattenedMemberList<QueryDataItemSymbol>(this.GetMembersOfKind<QueryDataItemSymbol>(SymbolKind.QueryDataItem).ToImmutableArray(), (QueryDataItemSymbol nestedDataItem) => (from m in nestedDataItem.GetMembers()
			where m.Kind == SymbolKind.QueryDataItem
			select m).Cast<QueryDataItemSymbol>().ToImmutableArray()).FlattenedList.ToImmutableArray());
		flattenedColumns = new Lazy<ImmutableArray<QueryColumnSymbol>>(() => FlattenedDataItems.SelectMany((QueryDataItemSymbol dataItem) => dataItem.GetMembersOfKind<QueryColumnSymbol>(SymbolKind.QueryColumn)).ToImmutableArray());
		flattenedFilters = new Lazy<ImmutableArray<QueryFilterSymbol>>(() => FlattenedDataItems.SelectMany((QueryDataItemSymbol dataItem) => dataItem.GetMembersOfKind<QueryFilterSymbol>(SymbolKind.QueryFilter)).ToImmutableArray());
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetQueryTriggerInfo(name);
	}
}
