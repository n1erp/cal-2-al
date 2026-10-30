using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class DotNetMethodTypeSymbol : DotNetMethodBaseTypeSymbol
{
	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetMethodType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetMethodType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetMethodType(this, a);
	}

	public DotNetMethodTypeSymbol(DotNetTypeSymbol containingSymbol, string name, int id, bool isVisibleInControlAddIn, bool isStaticDotNetMember = false, Func<TypeSymbol> returnValue = null, bool returnValueIsOptional = false, Func<Symbol, ParameterSymbol[]> getParameters = null)
		: base(containingSymbol, name, isVisibleInControlAddIn, isProperty: false, returnValue, returnValueIsOptional, getParameters, id, DotNetInvokeKind.InvokeMethod, isStaticDotNetMember)
	{
	}
}
