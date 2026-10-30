using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
internal abstract class ProfileTypeSymbol : ObjectTypeSymbol, IProfileTypeSymbol, IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol
{
	public override SymbolKind Kind { get; } = SymbolKind.Profile;

	public override TypeCategoryKind TypeCategory { get; } = TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind { get; } = NavTypeKind.Profile;

	internal override TypeSymbol BaseType { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.ProfileProperties;

	internal override ImmutableArray<VariableSymbol> GlobalVariables { get; } = ImmutableArray<VariableSymbol>.Empty;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitProfileType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitProfileType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitProfileType(this, a);
	}

	protected ProfileTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
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
