using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Designer.Objects;

internal static class NavDesignerSyntaxHelper
{
	internal static PageSyntax GetOrCreatePageArea(this PageSyntax pageSyntax, AreaKind areaKind, out PageAreaSyntax pageAreaSyntax)
	{
		if (pageSyntax.Layout == null)
		{
			pageSyntax = pageSyntax.AddLayoutAreas(SyntaxFactory.PageArea(areaKind.ToString()));
		}
		pageAreaSyntax = pageSyntax.GetArea(areaKind);
		if (pageAreaSyntax == null)
		{
			pageAreaSyntax = SyntaxFactory.PageArea(areaKind.ToString());
			PageLayoutSyntax layout = pageSyntax.Layout;
			PageLayoutSyntax newNode = pageSyntax.Layout.AddAreas(pageAreaSyntax);
			pageSyntax = pageSyntax.ReplaceNode(layout, newNode);
		}
		return pageSyntax;
	}

	internal static ControlBaseSyntax GetAnyControl(this PageSyntax pageSyntax, string controlName, bool allowArea)
	{
		ControlBaseSyntax field = pageSyntax.GetField(controlName);
		if (field != null)
		{
			return field;
		}
		field = pageSyntax.GetGroup(controlName);
		if (field != null)
		{
			return field;
		}
		if (allowArea)
		{
			return pageSyntax.GetArea(controlName);
		}
		return null;
	}

	internal static PageAreaSyntax GetArea(this PageSyntax pageSyntax, AreaKind area)
	{
		PageLayoutSyntax layout = pageSyntax.Layout;
		if (layout == null)
		{
			return null;
		}
		return layout.Areas.SingleOrDefault((PageAreaSyntax a) => SemanticFacts.IsSameName(a.GetNameStringValue(), area.ToString()));
	}

	internal static PageFieldSyntax GetField(this PageSyntax pageSyntax, string fieldName)
	{
		return pageSyntax.FindControl<PageFieldSyntax>(fieldName);
	}

	internal static PageGroupSyntax GetGroup(this PageSyntax pageSyntax, string groupName)
	{
		return pageSyntax.FindControl<PageGroupSyntax>(groupName);
	}

	internal static T FindControlInList<T>(SyntaxList<ControlBaseSyntax> list, string fieldName) where T : ControlBaseSyntax
	{
		return list.OfType<T>().SingleOrDefault((T c) => SemanticFacts.IsSameName(c.GetNameStringValue(), fieldName));
	}

	private static PageAreaSyntax GetArea(this PageSyntax pageSyntax, string areaName)
	{
		if (Enum.TryParse<AreaKind>(areaName, out var result))
		{
			return pageSyntax.GetArea(result);
		}
		return null;
	}

	private static T FindControl<T>(this PageSyntax pageSyntax, string controlName) where T : ControlBaseSyntax
	{
		T val = FindControlInArea<T>(pageSyntax, AreaKind.Content, controlName);
		if (val != null)
		{
			return val;
		}
		val = FindControlInArea<T>(pageSyntax, AreaKind.FactBoxes, controlName);
		if (val != null)
		{
			return val;
		}
		return FindControlInArea<T>(pageSyntax, AreaKind.RoleCenter, controlName);
	}

	private static T FindControlInArea<T>(PageSyntax pageSyntax, AreaKind areaKind, string actionName) where T : ControlBaseSyntax
	{
		PageAreaSyntax area = pageSyntax.GetArea(areaKind);
		if (area == null)
		{
			return null;
		}
		T val = FindControlInList<T>(area.Controls, actionName);
		if (val != null)
		{
			return val;
		}
		foreach (PageGroupSyntax item in area.Controls.OfType<PageGroupSyntax>())
		{
			val = FindControlInGroup<T>(item, actionName);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static T FindControlInGroup<T>(PageGroupSyntax pageGroupSyntax, string controlName) where T : ControlBaseSyntax
	{
		T val = FindControlInList<T>(pageGroupSyntax.Controls, controlName);
		if (val != null)
		{
			return val;
		}
		foreach (PageGroupSyntax item in pageGroupSyntax.Controls.OfType<PageGroupSyntax>())
		{
			val = FindControlInGroup<T>(item, controlName);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	internal static PageSyntax GetOrCreatePageActionArea(this PageSyntax pageSyntax, ActionAreaKind areaKind, out PageActionAreaSyntax actionAreaSyntax)
	{
		if (pageSyntax.Actions == null)
		{
			pageSyntax = pageSyntax.AddActionsAreas(SyntaxFactory.PageActionArea(areaKind.ToString()));
		}
		actionAreaSyntax = pageSyntax.GetActionArea(areaKind);
		if (actionAreaSyntax == null)
		{
			actionAreaSyntax = SyntaxFactory.PageActionArea(areaKind.ToString());
			PageActionListSyntax actions = pageSyntax.Actions;
			PageActionListSyntax newNode = pageSyntax.Actions.AddAreas(actionAreaSyntax);
			pageSyntax = pageSyntax.ReplaceNode(actions, newNode);
		}
		return pageSyntax;
	}

	internal static ActionBaseSyntax GetAnyAction(this PageSyntax pageSyntax, string actionName, bool allowArea)
	{
		ActionBaseSyntax action = pageSyntax.GetAction(actionName);
		if (action != null)
		{
			return action;
		}
		action = pageSyntax.GetActionGroup(actionName);
		if (action != null)
		{
			return action;
		}
		if (allowArea)
		{
			return pageSyntax.GetActionArea(actionName);
		}
		return null;
	}

	internal static PageActionAreaSyntax GetActionArea(this PageSyntax pageSyntax, ActionAreaKind area)
	{
		PageActionListSyntax actions = pageSyntax.Actions;
		if (actions == null)
		{
			return null;
		}
		return actions.Areas.SingleOrDefault((PageActionAreaSyntax a) => SemanticFacts.IsSameName(a.GetNameStringValue(), area.ToString()));
	}

	internal static PageActionSyntax GetAction(this PageSyntax pageSyntax, string actionName)
	{
		return pageSyntax.FindAction<PageActionSyntax>(actionName);
	}

	internal static PageActionGroupSyntax GetActionGroup(this PageSyntax pageSyntax, string groupName)
	{
		return pageSyntax.FindAction<PageActionGroupSyntax>(groupName);
	}

	internal static T FindActionInList<T>(SyntaxList<ActionBaseSyntax> list, string fieldName) where T : ActionBaseSyntax
	{
		return list.OfType<T>().SingleOrDefault((T c) => SemanticFacts.IsSameName(c.GetNameStringValue(), fieldName));
	}

	internal static PageActionAreaSyntax FindAreaInList(SyntaxList<PageActionAreaSyntax> list, ActionAreaKind actionAreaKind)
	{
		return list.SingleOrDefault((PageActionAreaSyntax c) => SemanticFacts.IsSameName(c.GetNameStringValue(), actionAreaKind.ToString()));
	}

	private static PageActionAreaSyntax GetActionArea(this PageSyntax pageSyntax, string areaName)
	{
		if (Enum.TryParse<ActionAreaKind>(areaName, out var result))
		{
			return pageSyntax.GetActionArea(result);
		}
		return null;
	}

	private static T FindAction<T>(this PageSyntax pageSyntax, string actionName) where T : ActionBaseSyntax
	{
		T val = FindActionInArea<T>(pageSyntax, ActionAreaKind.Creation, actionName);
		if (val != null)
		{
			return val;
		}
		val = FindActionInArea<T>(pageSyntax, ActionAreaKind.Processing, actionName);
		if (val != null)
		{
			return val;
		}
		val = FindActionInArea<T>(pageSyntax, ActionAreaKind.Reporting, actionName);
		if (val != null)
		{
			return val;
		}
		val = FindActionInArea<T>(pageSyntax, ActionAreaKind.Navigation, actionName);
		if (val != null)
		{
			return val;
		}
		val = FindActionInArea<T>(pageSyntax, ActionAreaKind.Sections, actionName);
		if (val != null)
		{
			return val;
		}
		return FindActionInArea<T>(pageSyntax, ActionAreaKind.Embedding, actionName);
	}

	private static T FindActionInArea<T>(PageSyntax pageSyntax, ActionAreaKind areaKind, string actionName) where T : ActionBaseSyntax
	{
		PageActionAreaSyntax actionArea = pageSyntax.GetActionArea(areaKind);
		if (actionArea == null)
		{
			return null;
		}
		T val = FindActionInList<T>(actionArea.Actions, actionName);
		if (val != null)
		{
			return val;
		}
		foreach (PageActionGroupSyntax item in actionArea.Actions.OfType<PageActionGroupSyntax>())
		{
			val = FindActionInGroup<T>(item, actionName);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	private static T FindActionInGroup<T>(PageActionGroupSyntax pageGroupSyntax, string actionName) where T : ActionBaseSyntax
	{
		T val = FindActionInList<T>(pageGroupSyntax.Actions, actionName);
		if (val != null)
		{
			return val;
		}
		foreach (PageActionGroupSyntax item in pageGroupSyntax.Actions.OfType<PageActionGroupSyntax>())
		{
			val = FindActionInGroup<T>(item, actionName);
			if (val != null)
			{
				return val;
			}
		}
		return null;
	}

	internal static PageViewSyntax GetView(this PageSyntax pageSyntax, string viewName)
	{
		PageViewListSyntax views = pageSyntax.Views;
		if (views == null)
		{
			return null;
		}
		return views.Views.OfType<PageViewSyntax>().SingleOrDefault((PageViewSyntax a) => SemanticFacts.IsSameName(a.GetNameStringValue(), viewName));
	}

	internal static PageViewSyntax FindViewInList(SyntaxList<PageViewSyntax> list, string viewName)
	{
		return list.SingleOrDefault((PageViewSyntax c) => SemanticFacts.IsSameName(c.GetNameStringValue(), viewName));
	}

	internal static PageSyntax UpdatePropertyList(PageSyntax pageSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(pageSyntax.PropertyList, propertyKind, propertyValue);
		return pageSyntax.WithPropertyList(propertyList);
	}

	internal static PageActionSyntax UpdatePropertyList(PageActionSyntax actionSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(actionSyntax.PropertyList, propertyKind, propertyValue);
		return actionSyntax.WithPropertyList(propertyList);
	}

	internal static PageActionGroupSyntax UpdatePropertyList(PageActionGroupSyntax actionGroupSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(actionGroupSyntax.PropertyList, propertyKind, propertyValue);
		return actionGroupSyntax.WithPropertyList(propertyList);
	}

	internal static PageFieldSyntax UpdatePropertyList(PageFieldSyntax pageFieldSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(pageFieldSyntax.PropertyList, propertyKind, propertyValue);
		return pageFieldSyntax.WithPropertyList(propertyList);
	}

	internal static PageGroupSyntax UpdatePropertyList(PageGroupSyntax groupSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(groupSyntax.PropertyList, propertyKind, propertyValue);
		return groupSyntax.WithPropertyList(propertyList);
	}

	internal static PageViewSyntax UpdatePropertyList(PageViewSyntax viewSyntax, PropertyKind propertyKind, object propertyValue)
	{
		PropertyListSyntax propertyList = UpdatePropertyList(viewSyntax.PropertyList, propertyKind, propertyValue);
		return viewSyntax.WithPropertyList(propertyList);
	}

	internal static PropertySyntax CreatePropertySyntax(PropertyKind propertyKind, object propertyValue)
	{
		PropertyValueSyntax propertyValue2;
		switch (propertyKind)
		{
		case PropertyKind.Visible:
		case PropertyKind.Promoted:
		case PropertyKind.PromotedIsBig:
		case PropertyKind.PromotedOnly:
			propertyValue2 = CreateBooleanPropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.ApplicationArea:
			propertyValue2 = CreateCommaSeparatedPropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.QuickEntry:
			propertyValue2 = CreateClientSideBooleanExpressionPropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.PageType:
		case PropertyKind.Importance:
		case PropertyKind.PromotedCategory:
			propertyValue2 = CreateEnumValueSyntax(propertyValue);
			break;
		case PropertyKind.FreezeColumn:
			propertyValue2 = CreateFieldReferencePropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.Width:
			propertyValue2 = CreateInt32PropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.Caption:
		case PropertyKind.PromotedActionCategories:
			propertyValue2 = CreateLabelPropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.SourceTable:
			propertyValue2 = CreateObjectReferencePropertyValueSyntax(propertyValue);
			break;
		case PropertyKind.Filters:
			propertyValue2 = SyntaxFactory.ParseFiltersPropertyValue(propertyValue.ToString());
			break;
		case PropertyKind.OrderBy:
			propertyValue2 = SyntaxFactory.ParseOrderByPropertyValue(propertyValue.ToString());
			break;
		default:
			return null;
		}
		return SyntaxFactory.Property(propertyKind, propertyValue2);
	}

	private static PropertyListSyntax UpdatePropertyList(PropertyListSyntax propertyList, PropertyKind propertyKind, object propertyValue)
	{
		PropertySyntaxOrEmpty property = GetProperty(propertyList, propertyKind);
		PropertyListSyntax newNode;
		if (property == null)
		{
			if (propertyValue == null)
			{
				return propertyList;
			}
			PropertySyntax propertySyntax = CreatePropertySyntax(propertyKind, propertyValue);
			newNode = propertyList.AddProperties(propertySyntax);
		}
		else if (propertyValue == null)
		{
			SyntaxList<PropertySyntaxOrEmpty> properties = propertyList.Properties.Remove(property);
			newNode = SyntaxFactory.PropertyList(properties);
		}
		else
		{
			PropertySyntax newNode2 = CreatePropertySyntax(propertyKind, propertyValue);
			SyntaxList<PropertySyntaxOrEmpty> properties2 = propertyList.Properties.Remove(property).Add(newNode2);
			newNode = SyntaxFactory.PropertyList(properties2);
		}
		return propertyList.ReplaceNode(propertyList, newNode);
	}

	private static PropertySyntax GetProperty(PropertyListSyntax propertyList, PropertyKind propertyKind)
	{
		return propertyList.Properties.OfType<PropertySyntax>().SingleOrDefault((PropertySyntax p) => SemanticFacts.IsSameName(p.Name.Identifier.ValueText, propertyKind.ToString()));
	}

	private static BooleanPropertyValueSyntax CreateBooleanPropertyValueSyntax(object propertyValue)
	{
		if (!Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture))
		{
			return SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.FalseKeyword)));
		}
		return SyntaxFactory.BooleanPropertyValue(SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.TrueKeyword)));
	}

	private static BooleanLiteralValueSyntax CreateBooleanLiteralValueSyntax(object propertyValue)
	{
		if (!Convert.ToBoolean(propertyValue, CultureInfo.InvariantCulture))
		{
			return SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.FalseKeyword));
		}
		return SyntaxFactory.BooleanLiteralValue(SyntaxFactory.Token(SyntaxKind.TrueKeyword));
	}

	private static CommaSeparatedPropertyValueSyntax CreateCommaSeparatedPropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.CommaSeparatedPropertyValue().AddValues(SyntaxFactory.IdentifierName(propertyValue.ToString()));
	}

	private static ClientSideBooleanExpressionPropertyValueSyntax CreateClientSideBooleanExpressionPropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.ClientSideBooleanExpressionPropertyValue(SyntaxFactory.LiteralExpression(CreateBooleanLiteralValueSyntax(propertyValue)));
	}

	private static EnumPropertyValueSyntax CreateEnumValueSyntax(object propertyValue)
	{
		return SyntaxFactory.EnumPropertyValue(SyntaxFactory.IdentifierName(propertyValue.ToString()));
	}

	private static PageFieldReferencePropertyValueSyntax CreateFieldReferencePropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.PageFieldReferencePropertyValue(SyntaxFactory.IdentifierName(propertyValue.ToString()));
	}

	private static Int32PropertyValueSyntax CreateInt32PropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.Int32PropertyValue(SyntaxFactory.Int32SignedLiteralValue(SyntaxFactory.Literal(Convert.ToInt32(propertyValue, CultureInfo.InvariantCulture))));
	}

	private static LabelPropertyValueSyntax CreateLabelPropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.LabelPropertyValue(SyntaxFactory.Label(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(propertyValue.ToString()))));
	}

	private static ObjectReferencePropertyValueSyntax CreateObjectReferencePropertyValueSyntax(object propertyValue)
	{
		return SyntaxFactory.ObjectReferencePropertyValue(SyntaxFactory.ObjectNameOrId(SyntaxFactory.IdentifierName(propertyValue.ToString())));
	}
}
