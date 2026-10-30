using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ParameterSymbol : Symbol, IParameterSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithType
{
	public override SymbolKind Kind => SymbolKind.Parameter;

	public abstract TypeSymbol Type { get; }

	public abstract bool IsVar { get; }

	public abstract bool IsParams { get; }

	public abstract int Ordinal { get; }

	public virtual bool IsOptional => false;

	public virtual bool IsMember => false;

	public virtual bool IsMemberReference => false;

	public virtual bool IsArray => Type.IsArray();

	public virtual bool TypeFromFirstArgument => false;

	public virtual bool IsSameVar => false;

	public virtual bool AddExpectedType => false;

	public override Accessibility DeclaredAccessibility => Accessibility.NotApplicable;

	public ITypeSymbol ParameterType => Type;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitParameter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitParameter(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitParameter(this, a);
	}
}
