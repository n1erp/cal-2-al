using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestPartSymbol : NavTypeSymbol
{
	private readonly PageTypeSymbol pagePart;

	private ImmutableArray<Symbol> lazyMembers;

	public override string Name => ControlSymbol.Name;

	public int Id => ControlSymbol.Id;

	public ControlSymbol ControlSymbol { get; }

	public override Symbol ContainingSymbol { get; }

	public override SymbolKind Kind => SymbolKind.TestPart;

	public override NavTypeKind NavTypeKind => NavTypeKind.TestPart;

	internal override TypeSymbol BaseType => TestPartClassTypeSymbol.Instance;

	public override TypeCategoryKind TypeCategory => pagePart.TypeCategory;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestPart(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestPart(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestPart(this, a);
	}

	internal static TestPartSymbol Create(Symbol containingSymbol, ControlSymbol control, PageTypeSymbol pagePart)
	{
		return new TestPartSymbol(containingSymbol, control, pagePart);
	}

	private TestPartSymbol(Symbol containingSymbol, ControlSymbol control, PageTypeSymbol pagePart)
		: base(NavTypeKind.TestPage)
	{
		ControlSymbol = control;
		ContainingSymbol = containingSymbol;
		this.pagePart = pagePart;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		if (lazyMembers != null)
		{
			return lazyMembers;
		}
		ImmutableArray<Symbol> value = LazyGetMembers();
		ImmutableInterlocked.InterlockedCompareExchange(ref lazyMembers, value, default);
		return lazyMembers;
	}

	private ImmutableArray<Symbol> LazyGetMembers()
	{
		using TestPageMembersBuilder testPageMembersBuilder = new TestPageMembersBuilder(ContainingModule, this);
		testPageMembersBuilder.AddPageElements(pagePart, addPageParts: false);
		return testPageMembersBuilder.ToImmutable();
	}
}
