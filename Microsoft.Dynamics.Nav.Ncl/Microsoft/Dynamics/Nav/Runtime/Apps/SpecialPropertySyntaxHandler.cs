using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SpecialPropertySyntaxHandler
{
	public static List<PropertySyntax> GetSpecialPropertySyntax(XmlElement element)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		List<PropertySyntax> list = new List<PropertySyntax>();
		Func<XmlElement, PropertySyntax>[] array = new Func<XmlElement, PropertySyntax>[6] { GetCaptionPropertySyntax, GetToolTipPropertySyntax, GetRunObjectPropertySyntax, GetRunPageLinkPropertySyntax, GetRunPageViewPropertySyntax, GetAccessByPermissionPropertySyntax };
		Func<XmlElement, PropertySyntax>[] array2 = array;
		foreach (Func<XmlElement, PropertySyntax> func in array2)
		{
			PropertySyntax propertySyntax = func(element);
			if (propertySyntax != null)
			{
				list.Add(propertySyntax);
			}
		}
		return list;
	}

	private static PropertySyntax GetCaptionPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		if (element.Attributes["CaptionML"] != null)
		{
			result = SyntaxFactory.Property(PropertyKind.CaptionML, SyntaxFactory.MultilanguagePropertyValue(SyntaxFactory.CommaSeparatedIdentifierEqualsStringList().AddValues(ConverterHelper.GetIdentifierEqualsStringSyntax(element.Attributes["CaptionML"].Value))));
		}
		else if (element.Attributes["Caption"] != null)
		{
			result = SyntaxFactory.Property(PropertyKind.Caption, SyntaxFactory.LabelPropertyValue(SyntaxFactory.Label(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(element.Attributes["Caption"].Value)))));
		}
		return result;
	}

	private static PropertySyntax GetToolTipPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		if (element.Attributes["ToolTipML"] != null)
		{
			result = SyntaxFactory.Property(PropertyKind.ToolTipML, SyntaxFactory.MultilanguagePropertyValue(SyntaxFactory.CommaSeparatedIdentifierEqualsStringList().AddValues(ConverterHelper.GetIdentifierEqualsStringSyntax(element.Attributes["ToolTipML"].Value))));
		}
		else if (element.Attributes["ToolTip"] != null)
		{
			result = SyntaxFactory.Property(PropertyKind.ToolTip, SyntaxFactory.LabelPropertyValue(SyntaxFactory.Label(SyntaxFactory.StringLiteralValue(SyntaxFactory.Literal(element.Attributes["ToolTip"].Value)))));
		}
		return result;
	}

	private static PropertySyntax GetRunObjectPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		if (element.Attributes["RunObjectType"] != null && int.TryParse(element.Attributes["TargetID"]?.Value, out var result2))
		{
			SyntaxToken? runObjectTypeSyntaxToken = GetRunObjectTypeSyntaxToken(element.Attributes["RunObjectType"].Value);
			if (runObjectTypeSyntaxToken.HasValue)
			{
				result = SyntaxFactory.Property(PropertyKind.RunObject, SyntaxFactory.QualifiedObjectReferencePropertyValue(runObjectTypeSyntaxToken.Value, SyntaxFactory.ObjectNameOrId(SyntaxFactory.LiteralExpression(SyntaxFactory.Int32SignedLiteralValue(SyntaxFactory.Literal(result2))))));
			}
		}
		return result;
	}

	private static PropertySyntax GetRunPageLinkPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		if (int.TryParse(element["RunFormLink"]?["TableFilters"]?.Attributes["FieldID"].Value, out var _))
		{
			int.TryParse(element["RunFormLink"]?["TableFilters"]?.Attributes["FilterValue"].Value, out var _);
		}
		return result;
	}

	private static PropertySyntax GetRunPageViewPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		_ = element["RunFormView"]?["Sorting"];
		return result;
	}

	private static PropertySyntax GetAccessByPermissionPropertySyntax(XmlElement element)
	{
		PropertySyntax result = null;
		if (element.Attributes["AccessByObjectID"] != null && element.Attributes["AccessByObjectType"] != null && int.TryParse(element.Attributes["AccessByObjectID"].Value, out var result2))
		{
			SyntaxToken? accessByObjectTypeSyntaxToken = GetAccessByObjectTypeSyntaxToken(element.Attributes["AccessByObjectType"].Value);
			if (accessByObjectTypeSyntaxToken.HasValue)
			{
				result = SyntaxFactory.Property(PropertyKind.AccessByPermission, SyntaxFactory.PermissionPropertyValue().AddPermissionProperties(SyntaxFactory.Permission(accessByObjectTypeSyntaxToken.Value, SyntaxFactory.ObjectNameOrId(SyntaxFactory.LiteralExpression(SyntaxFactory.Int32SignedLiteralValue(SyntaxFactory.Literal(result2)))))));
			}
		}
		return result;
	}

	private static SyntaxToken? GetRunObjectTypeSyntaxToken(string value)
	{
		if (!ObjectMapper.RunObjectTypeMapper.TryGetValue(value, out var value2))
		{
			return null;
		}
		return value2;
	}

	private static SyntaxToken? GetAccessByObjectTypeSyntaxToken(string value)
	{
		if (!ObjectMapper.AccessByObjectTypeMapper.TryGetValue(value, out var value2))
		{
			return null;
		}
		return value2;
	}
}
