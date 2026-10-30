using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class GenericLanguageClassTypeSymbol : LanguageClassTypeSymbol
{
	public ImmutableArray<TypeSymbol> TypeArguments { get; }

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitGenericLanguageClassType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitGenericLanguageClassType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitGenericLanguageClassType(this, a);
	}

	protected GenericLanguageClassTypeSymbol(string name, bool implicitMemberAccess, TypeCategoryKind typeCategory, NavTypeKind kind, SymbolCompilationScope compilationScope, Func<Symbol, Symbol[]> getMethods, TypeSymbol[] typeArguments)
		: base(name, implicitMemberAccess, typeCategory, kind, compilationScope, getMethods)
	{
		TypeArguments = ImmutableArray.Create(typeArguments);
	}
}
