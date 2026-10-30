using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class PageReferenceTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol
{
	private readonly PageTypeSymbol pageTypeSymbol;

	public override SymbolKind Kind => SymbolKind.PageReference;

	internal bool IsCurrPage { get; }

	public override string Name => OriginalSymbolDefinition.Name;

	public override TypeSymbol OriginalDefinition => pageTypeSymbol;

	public int Id => pageTypeSymbol.Id;

	internal override TypeSymbol BaseType => PageClassTypeSymbol.Instance;

	public override TypeCategoryKind TypeCategory => pageTypeSymbol.TypeCategory;

	protected override Symbol OriginalSymbolDefinition => pageTypeSymbol;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitPageReferenceType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitPageReferenceType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitPageReferenceType(this, a);
	}

	private PageReferenceTypeSymbol(PageTypeSymbol pageTypeSymbol, bool isCurrPage)
		: base(isCurrPage ? NavTypeKind.CurrPage : NavTypeKind.Page)
	{
		this.pageTypeSymbol = pageTypeSymbol;
		IsCurrPage = isCurrPage;
	}

	internal static PageReferenceTypeSymbol Create(PageTypeSymbol pageTypeSymbol, bool isCurrPage = false)
	{
		return new PageReferenceTypeSymbol(pageTypeSymbol, isCurrPage);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return ((PageReferenceTypeSymbol)t2).pageTypeSymbol.Equals(pageTypeSymbol);
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return pageTypeSymbol.GetMembers();
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return pageTypeSymbol.GetMembers(name);
	}

	internal override ImmutableArray<Symbol> GetCodeMembers()
	{
		return pageTypeSymbol.GetCodeMembers();
	}

	internal override ImmutableArray<Symbol> GetCodeMembers(string name)
	{
		return pageTypeSymbol.GetCodeMembers(name);
	}
}
