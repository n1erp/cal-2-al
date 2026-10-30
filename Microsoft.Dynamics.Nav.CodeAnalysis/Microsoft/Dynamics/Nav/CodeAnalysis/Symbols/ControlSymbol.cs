using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal abstract class ControlSymbol : TypeSymbol, IControlSymbol, ISymbolWithId, ISymbol, IEquatable<ISymbol>, ISymbolWithTriggers, ISymbolWithType
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public abstract ControlKind ControlKind { get; }

	public abstract int Id { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos
	{
		get
		{
			switch (ControlKind)
			{
			case ControlKind.Area:
				return ObjectParser.PageAreaProperties;
			case ControlKind.Field:
				return ObjectParser.PageFieldProperties;
			case ControlKind.Label:
				return ObjectParser.PageLabelProperties;
			case ControlKind.Part:
				return ObjectParser.PagePartProperties;
			case ControlKind.SystemPart:
				return ObjectParser.PageSystemPartProperties;
			case ControlKind.ChartPart:
				return ObjectParser.PageChartPartProperties;
			case ControlKind.UserControl:
				return ObjectParser.PageUserControlProperties;
			case ControlKind.Group:
			case ControlKind.CueGroup:
			case ControlKind.Repeater:
			case ControlKind.Fixed:
			case ControlKind.Grid:
				return ObjectParser.PageGroupProperties;
			default:
				throw ExceptionUtilities.UnexpectedValue(ControlKind);
			}
		}
	}

	public abstract ImmutableArray<ControlSymbol> Controls { get; }

	public abstract ImmutableArray<ActionSymbol> Actions { get; }

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	public abstract TypeSymbol Type { get; }

	public override SymbolKind Kind => SymbolKind.Control;

	public override NavTypeKind NavTypeKind
	{
		get
		{
			if (ControlKind != ControlKind.UserControl)
			{
				return NavTypeKind.None;
			}
			return NavTypeKind.DotNet;
		}
	}

	public abstract PageTypeSymbol RelatedPartSymbol { get; }

	public abstract ControlAddInTypeSymbol RelatedControlAddInSymbol { get; }

	public abstract FieldSymbol RelatedFieldSymbol { get; }

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	ControlKind IControlSymbol.ControlKind => ControlKind;

	ITypeSymbol IControlSymbol.Type => Type;

	int ISymbolWithId.Id => Id;

	ImmutableArray<IControlSymbol> IControlSymbol.Controls => ImmutableArray<IControlSymbol>.CastUp(Controls);

	IFieldSymbol IControlSymbol.RelatedFieldSymbol => RelatedFieldSymbol;

	IPageTypeSymbol IControlSymbol.RelatedPartSymbol => RelatedPartSymbol;

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitControl(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitControl(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitControl(this, a);
	}

	public TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		switch (ControlKind)
		{
		case ControlKind.Field:
			return TriggerDefinitions.GetControlTriggerInfo(name);
		case ControlKind.Area:
		case ControlKind.Group:
		case ControlKind.CueGroup:
		case ControlKind.Repeater:
		case ControlKind.Fixed:
		case ControlKind.Grid:
		case ControlKind.Part:
		case ControlKind.SystemPart:
		case ControlKind.UserControl:
		case ControlKind.ChartPart:
			return null;
		default:
			throw ExceptionUtilities.UnexpectedValue(ControlKind);
		}
	}

	public TableTypeSymbol TryGetSourceTableByProviderId()
	{
		PropertySymbol property = GetProperty(PropertyKind.Provider);
		if (property == null)
		{
			return null;
		}
		if (!(property.Property.PropertyValue is BoundIdentifierListPropertyValue { Value: var value }))
		{
			return null;
		}
		return value.OfType<BoundControlAccess>().FirstOrDefault()?.ControlSymbol.RelatedPartSymbol?.GetRelatedTableSymbol();
	}

	public bool IsAccessibleFromCode()
	{
		if (ControlKind != ControlKind.Part)
		{
			return ControlKind == ControlKind.UserControl;
		}
		return true;
	}
}
