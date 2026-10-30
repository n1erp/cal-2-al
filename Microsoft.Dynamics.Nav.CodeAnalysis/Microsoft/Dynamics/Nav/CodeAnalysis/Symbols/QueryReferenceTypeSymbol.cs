using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class QueryReferenceTypeSymbol : NavTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
	private readonly QueryTypeSymbol queryTypeSymbol;

	public override TypeSymbol OriginalDefinition => queryTypeSymbol;

	protected override Symbol OriginalSymbolDefinition => queryTypeSymbol;

	public override TypeCategoryKind TypeCategory => queryTypeSymbol.TypeCategory;

	public override SymbolKind Kind => SymbolKind.QueryReference;

	public override string Name => OriginalSymbolDefinition.Name;

	public int Id => queryTypeSymbol.Id;

	internal bool IsCurrQuery { get; }

	internal override TypeSymbol BaseType => QueryInstanceClassTypeSymbol.Instance;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitQueryReferenceType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitQueryReferenceType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitQueryReferenceType(this, a);
	}

	private QueryReferenceTypeSymbol(QueryTypeSymbol queryTypeSymbol, bool isCurrQuery)
		: base(isCurrQuery ? NavTypeKind.CurrQuery : NavTypeKind.Query)
	{
		this.queryTypeSymbol = queryTypeSymbol;
		IsCurrQuery = isCurrQuery;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return queryTypeSymbol.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return queryTypeSymbol.GetMembers(name);
	}

	internal static QueryReferenceTypeSymbol Create(QueryTypeSymbol queryTypeSymbol, bool isCurrQuery = false)
	{
		return new QueryReferenceTypeSymbol(queryTypeSymbol, isCurrQuery);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return ((QueryReferenceTypeSymbol)t2).queryTypeSymbol.Equals(queryTypeSymbol);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return queryTypeSymbol.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return queryTypeSymbol.GetCodeMembers(name);
	}
}
