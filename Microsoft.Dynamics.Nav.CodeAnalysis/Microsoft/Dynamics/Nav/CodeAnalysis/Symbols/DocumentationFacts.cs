using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class DocumentationFacts
{
	private const string propertyBaseUrl = "https://docs.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/properties/devenv-";

	private const string propertyEndUrl = "-property";

	internal static string GetDocumentation(this ISymbol symbol)
	{
		if (symbol == null)
		{
			return string.Empty;
		}
		string documentationFromSymbol = GetDocumentationFromSymbol(symbol);
		if (string.IsNullOrEmpty(documentationFromSymbol))
		{
			return documentationFromSymbol;
		}
		string helpUrlFromSymbol = GetHelpUrlFromSymbol(symbol);
		return MarkdownHelper.CreateDocumentationWithHelpLink(documentationFromSymbol, helpUrlFromSymbol);
	}

	internal static string GetEnumValuePropertyDocumentation(this IPropertySymbol symbol, string optionValue)
	{
		if (symbol == null)
		{
			return string.Empty;
		}
		string documentationKey = GetDocumentationKey((PropertySymbol)symbol, optionValue);
		return EnumValuePropertyDocumentationResources.ResourceManager.GetString(documentationKey);
	}

	internal static string GetEnumValuePropertyDetailText(this IPropertySymbol symbol, string optionValue)
	{
		if (symbol == null)
		{
			return string.Empty;
		}
		return (((PropertySymbol)symbol).GetPropertyTypeInfo() as EnumPropertyTypeInfo).GetEnumValuePropertyDetailText(optionValue);
	}

	internal static string GetEnumValuePropertyDocumentation(this EnumPropertyTypeInfo info, string optionValue)
	{
		if (info == null)
		{
			return string.Empty;
		}
		string documentationKey = info.GetDocumentationKey(optionValue);
		return EnumValuePropertyDocumentationResources.ResourceManager.GetString(documentationKey);
	}

	internal static string GetEnumValuePropertyDetailText(this EnumPropertyTypeInfo info, string optionValue)
	{
		if (info == null)
		{
			return string.Empty;
		}
		if (!string.Equals(optionValue, info.DefaultValue, StringComparison.OrdinalIgnoreCase))
		{
			return string.Empty;
		}
		return "Default value";
	}

	internal static string GetDocumentation(this PropertyTypeInfo info)
	{
		if (info == null)
		{
			return string.Empty;
		}
		string documentation = PropertyDocumentationResources.ResourceManager.GetString(info.GetDocumentationKey());
		string propertyHelpUrl = GetPropertyHelpUrl(info.Name);
		return MarkdownHelper.CreateDocumentationWithHelpLink(documentation, propertyHelpUrl);
	}

	internal static string GetAttributeDocumentation(string attributeName)
	{
		if (attributeName == null)
		{
			return string.Empty;
		}
		string documentation = AttributeDocumentationResources.ResourceManager.GetString(attributeName);
		string attributeHelpUrl = GetAttributeHelpUrl(attributeName);
		return MarkdownHelper.CreateDocumentationWithHelpLink(documentation, attributeHelpUrl);
	}

	internal static string GetDocumentation(this SyntaxNode syntaxNode)
	{
		if (syntaxNode == null)
		{
			return string.Empty;
		}
		string documentationKey = syntaxNode.GetDocumentationKey();
		return SyntaxDocumentationResources.ResourceManager.GetString(documentationKey);
	}

	internal static string GetDocumentation(this SyntaxNodeDefinition syntaxDefinition)
	{
		if (syntaxDefinition == null)
		{
			return string.Empty;
		}
		string documentationKey = syntaxDefinition.GetDocumentationKey();
		return SyntaxDocumentationResources.ResourceManager.GetString(documentationKey);
	}

	internal static string GetParameterDocumentation(this SyntaxNodeDefinition syntaxDefinition, string parameterName)
	{
		if (syntaxDefinition == null)
		{
			return string.Empty;
		}
		string documentationKey = syntaxDefinition.GetDocumentationKey(parameterName);
		return SyntaxDocumentationResources.ResourceManager.GetString(documentationKey);
	}

	internal static string GetAttributeHelpUrl(string attributeName)
	{
		return string.Empty;
	}

	internal static string GetPropertyHelpUrl(string propertyName)
	{
		return "https://docs.microsoft.com/en-us/dynamics365/business-central/dev-itpro/developer/properties/devenv-" + propertyName.ToLower(CultureInfo.InvariantCulture) + "-property";
	}

	internal static string GetDocumentationKey(this PropertyTypeInfo info, string optionValue = null)
	{
		if (!string.IsNullOrEmpty(optionValue))
		{
			return info.SyntaxOwner + "_" + info.Name + "_" + optionValue;
		}
		return info.SyntaxOwner + "_" + info.Name;
	}

	internal static string GetDocumentationKey(this SyntaxNodeDefinition syntaxDefinition)
	{
		return syntaxDefinition.GetType().Name;
	}

	internal static string GetDocumentationKey(this SyntaxNode syntaxDefinition)
	{
		return syntaxDefinition.GetType().Name + "Definition";
	}

	internal static string GetDocumentationKey(this SyntaxNodeDefinition syntaxDefinition, string parameter)
	{
		return syntaxDefinition.GetDocumentationKey() + "_" + parameter;
	}

	private static string GetDocumentationKey(MethodSymbol method)
	{
		return method.ContainingType.Name + "_" + method.Name;
	}

	private static string GetDocumentationKey(ParameterSymbol parameter)
	{
		return parameter.ContainingSymbol.ContainingType.Name + "_" + parameter.ContainingSymbol.Name + "_" + parameter.Name;
	}

	internal static string GetDocumentationKey(this AttributeSymbol attribute)
	{
		return attribute.Name;
	}

	internal static string GetDocumentationKey(this AttributeArgumentSymbol argument)
	{
		return argument.ContainingSymbol.Name + "_" + argument.Name;
	}

	private static string GetDocumentationFromSymbol(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
		case SymbolKind.Method:
		{
			string documentationKey = GetDocumentationKey((MethodSymbol)symbol);
			if (string.IsNullOrEmpty(documentationKey))
			{
				return string.Empty;
			}
			return ClassDocumentationResources.ResourceManager.GetString(documentationKey);
		}
		case SymbolKind.Parameter:
		{
			string documentationKey = GetDocumentationKey((ParameterSymbol)symbol);
			if (string.IsNullOrEmpty(documentationKey))
			{
				return string.Empty;
			}
			return ClassDocumentationResources.ResourceManager.GetString(documentationKey);
		}
		case SymbolKind.Property:
		{
			string documentationKey = GetDocumentationKey((PropertySymbol)symbol);
			if (string.IsNullOrEmpty(documentationKey))
			{
				return string.Empty;
			}
			return PropertyDocumentationResources.ResourceManager.GetString(documentationKey);
		}
		case SymbolKind.Attribute:
		{
			string documentationKey = ((AttributeSymbol)symbol).GetDocumentationKey();
			if (string.IsNullOrEmpty(documentationKey))
			{
				return string.Empty;
			}
			return AttributeDocumentationResources.ResourceManager.GetString(documentationKey);
		}
		case SymbolKind.AttributeArgument:
		{
			string documentationKey = ((AttributeArgumentSymbol)symbol).GetDocumentationKey();
			if (string.IsNullOrEmpty(documentationKey))
			{
				return string.Empty;
			}
			return AttributeDocumentationResources.ResourceManager.GetString(documentationKey);
		}
		default:
			return string.Empty;
		}
	}

	private static string GetDocumentationKey(PropertySymbol property, string optionValue = null)
	{
		SourcePropertySymbol obj = property as SourcePropertySymbol;
		optionValue = optionValue ?? string.Empty;
		return (obj.PropertySyntax?.GetPropertyTypeInfo())?.GetDocumentationKey(optionValue);
	}

	private static string GetHelpUrlFromSymbol(ISymbol symbol)
	{
		if (symbol.Kind == SymbolKind.Property)
		{
			return GetPropertyHelpUrl(symbol.Name);
		}
		if (symbol.Kind == SymbolKind.Attribute)
		{
			return GetAttributeHelpUrl(symbol.Name);
		}
		return string.Empty;
	}
}
