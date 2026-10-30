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
internal abstract class ControlAddInTypeSymbol : ObjectTypeSymbol, IControlAddInTypeSymbol, IObjectTypeSymbol, ITypeSymbol, IContainerSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, IRootTypeSymbol, IEquatable<ControlAddInTypeSymbol>
{
	public abstract string PublicKeyToken { get; }

	public override SymbolKind Kind { get; } = SymbolKind.ControlAddIn;

	public override TypeCategoryKind TypeCategory { get; } = TypeCategoryKind.ApplicationObject;

	public override NavTypeKind NavTypeKind { get; } = NavTypeKind.ControlAddIn;

	internal override TypeSymbol BaseType { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ObjectParser.ControlAddInObjectProperties;

	internal sealed override ImmutableArray<VariableSymbol> GlobalVariables { get; } = ImmutableArray<VariableSymbol>.Empty;

	public override ImmutableArray<MethodSymbol> Triggers { get; } = ImmutableArray<MethodSymbol>.Empty;

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitControlAddInType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitControlAddInType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitControlAddInType(this, a);
	}

	protected ControlAddInTypeSymbol(ModuleSymbol containingSymbol)
		: base(containingSymbol)
	{
	}

	public override TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		return null;
	}

	public bool Equals(ControlAddInTypeSymbol other)
	{
		if ((object)other == null)
		{
			return false;
		}
		if ((object)other == this)
		{
			return true;
		}
		return SemanticFacts.IsSameName(Name, other.Name);
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as ControlAddInTypeSymbol);
	}

	internal override bool Equals(TypeSymbol t2)
	{
		return Equals(t2 as ControlAddInTypeSymbol);
	}

	public override int GetHashCode()
	{
		return Name.ToUpperInvariant().GetHashCode();
	}
}
