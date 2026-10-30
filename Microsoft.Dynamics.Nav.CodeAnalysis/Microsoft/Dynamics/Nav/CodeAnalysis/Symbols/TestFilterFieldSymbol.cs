using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal class TestFilterFieldSymbol : NavTypeSymbol
{
	public FieldSymbol RelatedField { get; }

	public override string Name => RelatedField.Name;

	public override SymbolKind Kind => SymbolKind.TestFilterField;

	internal override TypeSymbol BaseType => NavCorLib.TestFilterFieldType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitTestFilterField(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitTestFilterField(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitTestFilterField(this, a);
	}

	internal TestFilterFieldSymbol(FieldSymbol field)
		: base(NavTypeKind.TestFilterField)
	{
		RelatedField = field;
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
