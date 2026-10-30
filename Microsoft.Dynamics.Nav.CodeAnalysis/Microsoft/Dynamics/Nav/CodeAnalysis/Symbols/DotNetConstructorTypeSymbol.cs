using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class DotNetConstructorTypeSymbol : DotNetMethodBaseTypeSymbol
{
	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetConstructorType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetConstructorType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetConstructorType(this, a);
	}

	public DotNetConstructorTypeSymbol(DotNetTypeSymbol containingSymbol, string name, Func<Symbol, ParameterSymbol[]> getParameters)
		: base(containingSymbol, name, isVisibleInControlAddIn: false, isProperty: false, containingSymbol, returnValueIsOptional: false, getParameters, 0, DotNetInvokeKind.InvokeConstructor)
	{
	}
}
