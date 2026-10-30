using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal sealed class GenericNavTypeSymbol : NavTypeSymbol
{
	private readonly Lazy<GenericLanguageClassTypeSymbol> lazyBaseType;

	public override string Name { get; }

	public override TypeCategoryKind TypeCategory => BaseType.TypeCategory;

	public override SymbolCompilationScope CompilationScope => BaseType.CompilationScope;

	public ImmutableArray<TypeSymbol> TypeArguments => lazyBaseType.Value.TypeArguments;

	internal override TypeSymbol BaseType => lazyBaseType.Value;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitGenericNavType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitGenericNavType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitGenericNavType(this, a);
	}

	internal GenericNavTypeSymbol(NavTypeKind navTypeKind, Func<GenericLanguageClassTypeSymbol> getBaseType)
		: base(navTypeKind)
	{
		lazyBaseType = new Lazy<GenericLanguageClassTypeSymbol>(getBaseType);
		Name = navTypeKind.ToString();
	}

	public override ImmutableArray<Symbol> GetMembers()
	{
		return ImmutableArray<Symbol>.Empty;
	}

	public override ImmutableArray<Symbol> GetMembers(string name)
	{
		return ImmutableArray<Symbol>.Empty;
	}
}
