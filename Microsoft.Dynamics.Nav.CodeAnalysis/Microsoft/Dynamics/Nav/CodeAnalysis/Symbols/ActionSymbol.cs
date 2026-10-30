using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ActionSymbol : TypeSymbol, IActionSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, ISymbolWithType
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public abstract ActionKind ActionKind { get; }

	public abstract int Id { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos => ActionKind switch
	{
		ActionKind.Action => (IDictionary<string, PropertyTypeInfo>)ObjectParser.PageActionProperties, 
		ActionKind.Area => ObjectParser.PageActionAreaProperties, 
		ActionKind.Group => ObjectParser.PageActionGroupProperties, 
		ActionKind.Separator => ObjectParser.PageActionSeparatorProperties, 
		_ => throw ExceptionUtilities.UnexpectedValue(ActionKind), 
	};

	public abstract ImmutableArray<ActionSymbol> Actions { get; }

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	public abstract TypeSymbol Type { get; }

	public override SymbolKind Kind => SymbolKind.Action;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	ImmutableArray<IActionSymbol> IActionSymbol.Actions => ImmutableArray<IActionSymbol>.CastUp(Actions);

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	ImmutableArray<IMethodSymbol> IActionSymbol.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitAction(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitAction(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitAction(this, a);
	}

	public TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		name = name.ToUpperInvariant();
		switch (ActionKind)
		{
		case ActionKind.Action:
			return TriggerDefinitions.GetActionTriggerInfo(name);
		case ActionKind.Area:
		case ActionKind.Group:
		case ActionKind.Separator:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(ActionKind);
		}
	}

	public ActionAreaKind? GetActionAreaKind()
	{
		if (ActionKind != ActionKind.Area)
		{
			return null;
		}
		return (ActionAreaKind)Enum.Parse(typeof(ActionAreaKind), Name, ignoreCase: true);
	}

	TriggerTypeInfo ISymbolWithTriggers.GetTriggerTypeInfo(string name)
	{
		return TriggerDefinitions.GetActionTriggerInfo(name);
	}
}
