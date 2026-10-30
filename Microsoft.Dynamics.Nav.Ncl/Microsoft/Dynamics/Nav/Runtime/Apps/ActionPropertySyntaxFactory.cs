using System;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class ActionPropertySyntaxFactory : PropertySyntaxAbstractFactory
{
	internal override PropertySyntax GetPropertySyntax(PropertyKind propertyKind, string value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		PropertyValueSyntax propertyValue;
		switch (propertyKind)
		{
		case PropertyKind.CaptionML:
		case PropertyKind.Caption:
		case PropertyKind.AccessByPermission:
		case PropertyKind.ToolTipML:
		case PropertyKind.ToolTip:
		case PropertyKind.PromotedOnly:
		case PropertyKind.PromotedCategory:
		case PropertyKind.ShortcutKey:
		case PropertyKind.RunObject:
		case PropertyKind.RunPageView:
		case PropertyKind.RunPageLink:
			return null;
		case PropertyKind.Description:
			propertyValue = SyntaxFactory.StringPropertyValue(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(value)));
			break;
		case PropertyKind.Image:
			propertyValue = SyntaxFactory.ImagePropertyValue(SyntaxFactory.IdentifierName(value));
			break;
		case PropertyKind.Scope:
		case PropertyKind.Importance:
		case PropertyKind.RunPageMode:
		case PropertyKind.Gesture:
			propertyValue = SyntaxFactory.EnumPropertyValue(SyntaxFactory.IdentifierName(value));
			break;
		case PropertyKind.ApplicationArea:
			propertyValue = SyntaxFactory.CommaSeparatedPropertyValue().AddValues(SyntaxFactory.IdentifierName(value));
			break;
		case PropertyKind.Enabled:
		case PropertyKind.Visible:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.Ellipsis:
		case PropertyKind.RunPageOnRec:
		case PropertyKind.InFooterBar:
			propertyValue = PropertySyntaxAbstractFactory.CreateBooleanPropertyValue(value);
			break;
		default:
			throw new InvalidOperationException("The type is not supported right now");
		}
		return SyntaxFactory.Property(propertyKind, propertyValue);
	}
}
