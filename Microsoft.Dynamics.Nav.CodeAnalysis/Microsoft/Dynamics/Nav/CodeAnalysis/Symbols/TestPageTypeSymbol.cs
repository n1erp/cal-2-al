using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestPageTypeSymbol : NavTypeSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>
{
	private ImmutableArray<Symbol> lazyMembers;

	internal override ModuleSymbol ContainingModule { get; }

	public override string Name => Target.Name;

	public int Id => Target.Id;

	public PageTypeSymbol Target { get; }

	public override SymbolKind Kind => SymbolKind.TestPage;

	public override NavTypeKind NavTypeKind => NavTypeKind.TestPage;

	public override TypeCategoryKind TypeCategory => Target.TypeCategory;

	internal override TypeSymbol BaseType => NavCorLib.TestPageType;

	protected override Symbol OriginalSymbolDefinition => Target;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestPageType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestPageType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestPageType(this, a);
	}

	internal static TestPageTypeSymbol Create(ModuleSymbol containingSymbol, PageTypeSymbol pageTypeSymbol)
	{
		return new TestPageTypeSymbol(containingSymbol, pageTypeSymbol);
	}

	private TestPageTypeSymbol(ModuleSymbol containingModule, PageTypeSymbol pageTypeSymbol)
		: base(NavTypeKind.TestPage)
	{
		ContainingModule = containingModule;
		Target = pageTypeSymbol;
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

	internal override bool Equals(TypeSymbol t2)
	{
		if (t2?.NavTypeKind != NavTypeKind)
		{
			return false;
		}
		return ((TestPageTypeSymbol)t2).Target.Equals(Target);
	}

	private ImmutableArray<Symbol> LazyGetMembers()
	{
		using TestPageMembersBuilder testPageMembersBuilder = new TestPageMembersBuilder(ContainingModule, this);
		testPageMembersBuilder.AddPageElements(Target, addPageParts: true);
		return testPageMembersBuilder.ToImmutable();
	}
}
