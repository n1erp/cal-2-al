using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class OptionTypeSymbol : TypeSymbol
{
	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public override SymbolKind Kind => SymbolKind.OptionType;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Option;

	public override NavTypeKind NavTypeKind => NavTypeKind.Option;

	public override string Name { get; }

	public abstract string OptionMembers { get; }

	public virtual int EffectiveOptionsCount => GetMembers().Length;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitOptionType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitOptionType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitOptionType(this, a);
	}

	protected OptionTypeSymbol()
	{
		Name = NavTypeKind.Option.ToString();
	}
}
