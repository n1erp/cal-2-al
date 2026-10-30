using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ChangeModifySymbol : ChangeSymbol, ISymbolWithTriggers, ISymbol, IEquatable<ISymbol>, ISymbolWithType
{
	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public TypeSymbol Type => Target.GetTypeSymbol() as TypeSymbol;

	internal abstract ISymbol Target { get; }

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitChangeModify(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitChangeModify(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitChangeModify(this, a);
	}

	public abstract TriggerTypeInfo GetTriggerTypeInfo(string name);

	internal abstract Location GetNameLocation();
}
