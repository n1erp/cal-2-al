using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestFieldSymbol : NavTypeSymbol
{
	public override Symbol ContainingSymbol { get; }

	public int Id => ControlSymbol.Id;

	public ControlSymbol ControlSymbol { get; }

	public override string Name => ControlSymbol.Name;

	public override SymbolKind Kind => SymbolKind.TestField;

	internal override TypeSymbol BaseType => NavCorLib.TestFieldType;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestField(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestField(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestField(this, a);
	}

	internal TestFieldSymbol(Symbol containingSymbol, ControlSymbol controlSymbol)
		: base(NavTypeKind.TestField)
	{
		ControlSymbol = controlSymbol;
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
