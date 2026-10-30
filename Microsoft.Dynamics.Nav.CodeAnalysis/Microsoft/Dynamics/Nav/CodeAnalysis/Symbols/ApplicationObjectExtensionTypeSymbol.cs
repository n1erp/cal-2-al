using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class ApplicationObjectExtensionTypeSymbol : ApplicationObjectTypeSymbol, IApplicationObjectExtensionTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId
{
	internal abstract ApplicationObjectTypeSymbol Target { get; }

	IApplicationObjectTypeSymbol IApplicationObjectExtensionTypeSymbol.Target => Target;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitApplicationObjectExtensionType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitApplicationObjectExtensionType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitApplicationObjectExtensionType(this, a);
	}

	protected ApplicationObjectExtensionTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}
}
