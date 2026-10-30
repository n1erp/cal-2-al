using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ReturnValueSymbol : Symbol, IReturnValueSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithType
{
	internal static readonly BuiltInReturnValueSymbol None = new BuiltInReturnValueSymbol(NavCorLib.None, isOptional: true);

	internal static readonly BuiltInReturnValueSymbol Boolean = new BuiltInReturnValueSymbol(NavCorLib.BooleanType, isOptional: true);

	public override SymbolKind Kind => SymbolKind.ReturnValue;

	public abstract bool IsNamed { get; }

	public virtual bool IsOptional => true;

	public virtual bool TypeFromFirstArgument => false;

	public abstract TypeSymbol Type { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public ITypeSymbol ReturnType => Type;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitReturnValue(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitReturnValue(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitReturnValue(this, a);
	}
}
