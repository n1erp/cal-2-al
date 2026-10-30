using System;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class ModificationContentExporter : IElementExporter
{
	private readonly IElementExporter contentExporter;

	public ModificationContentExporter(AlExportOptions options)
	{
		ChangeContentExporter nextExporter = new ChangeContentExporter(options);
		contentExporter = new ElementWithDocumentationExporter(nextExporter);
	}

	public void Export(IElement element, IAlWriter writer)
	{
		contentExporter.Export(element, writer);
	}

	public static string GetModificationObjectDeclaration(ApplicationObject ao, int assignedId)
	{
		return GetExtensionHeaderText(ao, assignedId);
	}

	internal static string GetExtensionHeaderText(ApplicationObject ao, int id)
	{
		string extensionTypeDisplayName = GetExtensionTypeDisplayName(ao.RootElement);
		string extensionObjectName = GetExtensionObjectName(ao.RootElement, id, extensionTypeDisplayName);
		string value = GetALNameFromElementIdentifier(ao.RootElement.GetStringProperty(PropertyType.Target)) ?? string.Empty;
		value = AlUtilities.QuoteIdentifierIfNeeded(value);
		return string.Format(CultureInfo.InvariantCulture, "{0} {1} {2} extends {3} ", extensionTypeDisplayName, id, extensionObjectName, value);
	}

	internal static string GetALNameFromElementIdentifier(string elementIdentifier)
	{
		string text = ExtractElementNameFromInput(elementIdentifier);
		if (string.IsNullOrWhiteSpace(text))
		{
			text = elementIdentifier;
		}
		return text;
	}

	internal static string GetExtensionTypeDisplayName(IElement ao)
	{
		string result = null;
		if (ao == null)
		{
			return null;
		}
		switch (GetExtensionTargetType(ao))
		{
		case ElementType.Page:
			result = SyntaxKind.PageExtensionKeyword.GetText();
			break;
		case ElementType.Table:
			result = SyntaxKind.TableExtensionKeyword.GetText();
			break;
		}
		return result;
	}

	private static string ExtractElementNameFromInput(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		int num = input.Length - 1;
		while (num >= 0 && input[num] != '(')
		{
			num--;
		}
		return input.Substring(0, Math.Max(num, 0));
	}

	internal static string WrapInQuotesIfNeeded(string input)
	{
		if (string.IsNullOrWhiteSpace(input))
		{
			return string.Empty;
		}
		if (input[0] == '"' && input[input.Length - 1] == ')')
		{
			int num = input.LastIndexOf("\"", StringComparison.Ordinal);
			return input.Substring(0, num) + input.Substring(num + 1) + "\"";
		}
		return AlUtilities.QuoteIdentifierIfNeeded(input);
	}

	internal static string GetExtensionObjectName(IElement rootElement, int id, string typeDisplayName)
	{
		_ = string.Empty;
		return WrapInQuotesIfNeeded(((!string.IsNullOrWhiteSpace(typeDisplayName)) ? typeDisplayName : rootElement.GetStringProperty(PropertyType.Name)) + id);
	}

	internal static ElementType GetExtensionTargetType(IElement element)
	{
		return GetExtensionTargetType(element?.GetStringProperty(PropertyType.Target));
	}

	internal static ElementType GetExtensionTargetContextType(IElement element)
	{
		string text = element?.GetStringProperty(PropertyType.Target);
		if (text != null && text.Count((char c) => c == '(') == 2)
		{
			return GetExtensionTargetType(text.Split('.')[0]);
		}
		return ElementType.Unknown;
	}

	private static ElementType GetExtensionTargetType(string targetValue)
	{
		ElementType result = ElementType.Unknown;
		if (string.IsNullOrWhiteSpace(targetValue))
		{
			return result;
		}
		if (targetValue[targetValue.Length - 1] != ')')
		{
			result = GetElementTypeFromStringIdentifier(targetValue);
			if (result != ElementType.Unknown)
			{
				return result;
			}
		}
		result = ExtractTypeFromTargetDeclaration(targetValue);
		if (result != ElementType.Unknown)
		{
			return result;
		}
		return ExtractTypeFromTargetDeclaration(targetValue.Split('.')[0]);
	}

	internal static ElementType ExtractTypeFromTargetDeclaration(string targetValue)
	{
		ElementType elementType = ElementType.Unknown;
		int num = targetValue.Length - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			switch (targetValue[num2])
			{
			case '(':
				elementType = (ElementType)Enum.Parse(typeof(ElementType), targetValue.Substring(num2 + 1, num - num2 - 1), ignoreCase: true);
				break;
			case ' ':
			case ')':
				num = num2;
				break;
			}
			if (elementType != ElementType.Unknown)
			{
				break;
			}
		}
		return elementType;
	}

	internal static ElementType GetElementTypeFromStringIdentifier(string identifier)
	{
		ElementType result = ElementType.Unknown;
		if (!string.IsNullOrWhiteSpace(identifier))
		{
			if (!identifier.Contains(' '))
			{
				Enum.TryParse<ElementType>(identifier, out result);
			}
			else
			{
				for (int i = 0; i < identifier.Length; i++)
				{
					if (identifier[i] == ' ')
					{
						Enum.TryParse<ElementType>(identifier.Substring(0, i), out result);
					}
				}
			}
		}
		return result;
	}
}
