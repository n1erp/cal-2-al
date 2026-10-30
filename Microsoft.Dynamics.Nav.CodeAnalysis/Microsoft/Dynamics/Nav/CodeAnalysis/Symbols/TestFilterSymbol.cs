using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestFilterSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	private ImmutableArray<Symbol> lazyMembers;

	public override Symbol ContainingSymbol { get; }

	public int Id { get; }

	public override string Name { get; }

	public IPageBaseTypeSymbol RelatedPage { get; }

	public TableTypeSymbol RelatedTable { get; }

	public override SymbolKind Kind => SymbolKind.TestFilter;

	internal override TypeSymbol BaseType => NavCorLib.TestFilterType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestFilter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestFilter(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestFilter(this, a);
	}

	internal TestFilterSymbol(Symbol containingSymbol, int id, string name, IPageBaseTypeSymbol relatedPage, TableTypeSymbol relatedTable)
		: base(NavTypeKind.TestFilter)
	{
		ContainingSymbol = containingSymbol;
		Id = id;
		Name = name;
		RelatedPage = relatedPage;
		RelatedTable = relatedTable;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, LazyGetMembers(), default);
		return lazyMembers;
	}

	private ImmutableArray<Symbol> LazyGetMembers()
	{
		if (RelatedTable == null)
		{
			return ImmutableArray<Symbol>.Empty;
		}
		ArrayBuilder<Symbol> instance = ArrayBuilder<Symbol>.GetInstance();
		try
		{
			foreach (ISymbol item in RelatedTable.QueryAllMembers(ContainingModule))
			{
				if (item.Kind == SymbolKind.Field)
				{
					instance.Add(new TestFilterFieldSymbol((FieldSymbol)item));
				}
			}
			return instance.ToImmutable();
		}
		finally
		{
			instance.Free();
		}
	}
}
