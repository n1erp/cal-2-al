using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class DotNetPropertyTypeSymbol : DotNetMethodBaseTypeSymbol
{
	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetPropertyType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetPropertyType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetPropertyType(this, a);
	}

	public DotNetPropertyTypeSymbol(DotNetTypeSymbol containingSymbol, string name, int id, bool isVisibleInControlAddIn, DotNetInvokeKind invocationKind, bool isStaticDotNetMember = false, Func<TypeSymbol> returnValue = null, bool returnValueIsOptional = false, Func<Symbol, ParameterSymbol[]> getParameters = null)
		: base(containingSymbol, name, isVisibleInControlAddIn, isProperty: true, returnValue, returnValueIsOptional, getParameters, id, invocationKind, isStaticDotNetMember)
	{
	}
}
