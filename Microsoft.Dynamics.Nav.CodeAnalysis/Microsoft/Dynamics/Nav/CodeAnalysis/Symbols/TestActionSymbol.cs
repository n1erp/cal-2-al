using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestActionSymbol : NavTypeSymbol
{
	public ActionSymbol ActionSymbol { get; }

	public override Symbol ContainingSymbol { get; }

	public override string Name => ActionSymbol.Name;

	public int Id => ActionSymbol.Id;

	public override SymbolKind Kind => SymbolKind.TestAction;

	internal override TypeSymbol BaseType => NavCorLib.TestActionType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestAction(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestAction(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestAction(this, a);
	}

	internal TestActionSymbol(Symbol containingSymbol, ActionSymbol actionSymbol)
		: base(NavTypeKind.TestAction)
	{
		ActionSymbol = actionSymbol;
		ContainingSymbol = containingSymbol;
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
