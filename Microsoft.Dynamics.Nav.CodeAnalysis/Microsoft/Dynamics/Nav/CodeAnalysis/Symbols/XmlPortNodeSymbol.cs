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
internal abstract class XmlPortNodeSymbol : TypeSymbol, IXmlPortNodeSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, ISymbolWithTriggers, ISymbolWithType
{
	public override TypeCategoryKind TypeCategory => TypeCategoryKind.Complex;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public abstract XmlPortSourceTypeKind SourceTypeKind { get; }

	public abstract XmlPortNodeXmlKind NodeXmlKind { get; }

	public abstract TypeSymbol Type { get; }

	public abstract int Id { get; }

	public abstract string MemberName { get; }

	public override SymbolKind Kind => SymbolKind.XmlPortNode;

	public abstract ImmutableArray<MethodSymbol> Triggers { get; }

	public override IDictionary<string, PropertyTypeInfo> PropertyTypeInfos
	{
		get
		{
			switch (SourceTypeKind)
			{
			case XmlPortSourceTypeKind.Table:
				return ObjectParser.XmlPortTableElementProperties;
			case XmlPortSourceTypeKind.Field:
				if (NodeXmlKind != XmlPortNodeXmlKind.Element)
				{
					return ObjectParser.XmlPortFieldAttributeProperties;
				}
				return ObjectParser.XmlPortFieldElementProperties;
			case XmlPortSourceTypeKind.Text:
				if (NodeXmlKind != XmlPortNodeXmlKind.Element)
				{
					return ObjectParser.XmlPortTextAttributeProperties;
				}
				return ObjectParser.XmlPortTextElementProperties;
			default:
				throw ExceptionUtilities.UnexpectedValue(SourceTypeKind);
			}
		}
	}

	ImmutableArray<IMethodSymbol> ISymbolWithTriggers.Triggers => ImmutableArray<IMethodSymbol>.CastUp(Triggers);

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitXmlPortNode(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitXmlPortNode(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitXmlPortNode(this, a);
	}

	public TriggerTypeInfo GetTriggerTypeInfo(string name)
	{
		switch (SourceTypeKind)
		{
		case XmlPortSourceTypeKind.Table:
			return TriggerDefinitions.GetXmlPortTableElementTriggerInfo(name);
		case XmlPortSourceTypeKind.Field:
			if (NodeXmlKind != XmlPortNodeXmlKind.Element)
			{
				return TriggerDefinitions.GetXmlPortFieldAttributeTriggerInfo(name);
			}
			return TriggerDefinitions.GetXmlPortFieldElementTriggerInfo(name);
		case XmlPortSourceTypeKind.Text:
			if (NodeXmlKind != XmlPortNodeXmlKind.Element)
			{
				return TriggerDefinitions.GetXmlPortTextAttributeTriggerInfo(name);
			}
			return TriggerDefinitions.GetXmlPortTextElementTriggerInfo(name);
		default:
			throw ExceptionUtilities.UnexpectedValue(SourceTypeKind);
		}
	}
}
