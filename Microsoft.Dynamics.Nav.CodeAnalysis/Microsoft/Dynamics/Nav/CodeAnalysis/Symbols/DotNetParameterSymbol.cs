using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class DotNetParameterSymbol : BuiltInParameterSymbol
{
	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetParameter(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetParameter(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetParameter(this, a);
	}

	public DotNetParameterSymbol(Symbol containingSymbol, TypeSymbol type, string name, int ordinal, bool isVar = false, bool isOptional = false, bool isParams = false)
		: base(containingSymbol, type, name, ordinal, isVar, isOptional, isParams)
	{
	}
}
