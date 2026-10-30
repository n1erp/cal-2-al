using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class EnumTypeSymbol : ApplicationObjectTypeSymbol, IRootTypeSymbol, ISymbol, IEquatable<ISymbol>, IEnumTypeSymbol, IApplicationObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbolWithId
{
	public override Accessibility DeclaredAccessibility => Accessibility.Public;

	public override SymbolKind Kind => SymbolKind.Enum;

	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Option;

	public override NavTypeKind NavTypeKind => NavTypeKind.Enum;

	public override string Name { get; }

	public abstract ImmutableArray<EnumValueSymbol> Values { get; }

	internal override ImmutableArray<VariableSymbol> GlobalVariables
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public override ImmutableArray<MethodSymbol> Triggers => ImmutableArray<MethodSymbol>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitEnumType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitEnumType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitEnumType(this, a);
	}

	protected EnumTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}
}
