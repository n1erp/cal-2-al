using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Txt2AlConverter.Exporters.Modification;

internal class PropertyChangeExporter : PropertiesConverter
{
	private readonly IPropertiesConverter propertiesExporter;

	public PropertyChangeExporter(IPropertiesConverter propertiesExporter)
	{
		this.propertiesExporter = propertiesExporter;
	}

	public void Export(IEnumerable<IElement> elements, string target, IAlWriter writer)
	{
		if (target != null && elements.Any())
		{
			ExportSection(elements, writer);
		}
	}

	private void ExportSection(IEnumerable<IElement> elements, IAlWriter writer)
	{
		if (writer == null)
		{
			return;
		}
		List<IElement> source = elements.Where((IElement element) => IsElementPropertySupported(element, writer)).ToList();
		List<IElement> list = elements.Where((IElement element) => !IsElementPropertySupported(element, writer)).ToList();
		if (!source.Any())
		{
			if (!list.Any())
			{
				return;
			}
			{
				foreach (IElement item in list)
				{
					new WarningExporter().Export(item, writer);
				}
				return;
			}
		}
		ElementType extensionTargetType = ModificationContentExporter.GetExtensionTargetType(elements.First());
		if ((uint)(extensionTargetType - 20) <= 1u)
		{
			ExportProperties(writer, elements);
			return;
		}
		string sectionHeader = GetSectionHeader(elements.ElementAt(0));
		writer.WriteSection(sectionHeader, () =>
		{
			ExportProperties(writer, elements);
		});
	}

	private void ExportProperties(IAlWriter writer, IEnumerable<IElement> elements)
	{
		foreach (IElement element in elements)
		{
			string text = element.GetStringProperty(PropertyType.Property);
			PropertyType propertyType = EnumNameLookup<PropertyType>.StringToEnum(text);
			Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo = element.GetPropertyInfo(PropertyType.Property);
			Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo actualPropertyInfo = GetPropertyTypeInfoOfTheModifiedProperty(element, propertyInfo);
			IElement originalElement = CreateElementWithTheActualPropertyValue(element, actualPropertyInfo);
			if (AlUtilities.IsMultilanguageProperty(element, propertyType))
			{
				TranslationUtilities.ProcessAndGetLabelSyntax(writer.State.TranslationExtractor, originalElement, actualPropertyInfo.PropertyType);
				text = text.Substring(0, text.Length - 2);
			}
			if (IsPropertyModificationSupported(text, element))
			{
				writer.WritePropertyWithValueAsAction(text, () =>
				{
					writer.Write(propertiesExporter.GetPropertyValue(originalElement, actualPropertyInfo, writer.State));
				});
			}
			else
			{
				new WarningExporter().Export(element, writer);
			}
		}
	}

	private static IElement CreateElementWithTheActualPropertyValue(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo actualPropertyInfo)
	{
		string stringProperty = owner.GetStringProperty(PropertyType.Target);
		string targetTypeFromTargetProperty = GetTargetTypeFromTargetProperty(stringProperty);
		string targetName = stringProperty.Substring(0, stringProperty.IndexOf("("));
		ElementType elementType = EnumNameLookup<ElementType>.StringToEnum(targetTypeFromTargetProperty);
		return CreateGenericElement(owner, actualPropertyInfo, elementType, targetName);
	}

	private static IElement CreateGenericElement(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo actualPropertyInfo, ElementType elementType, string targetName)
	{
		IElement element = GenericElement.Create(elementType);
		element.SetStringProperty(actualPropertyInfo.PropertyType, owner.GetStringProperty(PropertyType.ModifiedValue));
		element.SetStringProperty(PropertyType.Name, targetName);
		element.Parent = owner.Parent;
		return element;
	}

	private static Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo GetPropertyTypeInfoOfTheModifiedProperty(IElement owner, Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo propertyInfo)
	{
		PropertyType key = EnumNameLookup<PropertyType>.StringToEnum(owner.GetStringProperty(propertyInfo.PropertyType));
		return Microsoft.Dynamics.Nav.MetaMetaModel.PropertyTypeInfo.Lookup.GetValue(key);
	}

	private static string GetTargetTypeFromTargetProperty(string targetValue)
	{
		if (targetValue == null)
		{
			return targetValue;
		}
		int num = targetValue.LastIndexOf("(") + 1;
		int num2 = targetValue.LastIndexOf(" ");
		return targetValue.Substring(num, num2 - num);
	}

	internal static void ExportPropertyChangeSection(IElement element, string propName, string newValue, IAlWriter writer)
	{
		if (writer != null)
		{
			writer.WriteSection(GetSectionHeader(element), () =>
			{
				writer.WriteProperty(propName, newValue);
			});
		}
	}

	private static string GetSectionHeader(IElement element)
	{
		string aLNameFromElementIdentifier = ModificationContentExporter.GetALNameFromElementIdentifier(element.GetStringProperty(PropertyType.Target));
		aLNameFromElementIdentifier = ModificationContentExporter.WrapInQuotesIfNeeded(aLNameFromElementIdentifier);
		return "modify(" + aLNameFromElementIdentifier + ")";
	}

	private bool IsElementPropertySupported(IElement element, IAlWriter writer)
	{
		return IsPropertyModificationSupported(element.GetStringProperty(PropertyType.Property), element);
	}

	internal static bool IsPropertyModificationSupported(string propName, IElement element)
	{
		ElementType extensionTargetType = ModificationContentExporter.GetExtensionTargetType(element);
		Microsoft.Dynamics.Nav.CodeAnalysis.PropertyTypeInfo propertyTypeInfo = null;
		switch (extensionTargetType)
		{
		case ElementType.Table:
			propertyTypeInfo = ObjectParser.LookupTableProperty(propName);
			break;
		case ElementType.Field:
			propertyTypeInfo = ObjectParser.LookupFieldProperty(propName);
			break;
		case ElementType.Key:
			if (string.Equals(propName, PropertyType.Clustered.ToString(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			propertyTypeInfo = ObjectParser.LookupKeyProperty(propName);
			break;
		case ElementType.Page:
			propertyTypeInfo = ObjectParser.LookupPageProperty(propName);
			break;
		case ElementType.Action:
			propertyTypeInfo = ObjectParser.LookupPageActionProperty(propName);
			break;
		case ElementType.Control:
			propertyTypeInfo = ObjectParser.LookupPageFieldProperty(propName);
			break;
		}
		if ((propertyTypeInfo != null && propertyTypeInfo.Modification == ModificationKind.Extensions) || (propertyTypeInfo != null && propertyTypeInfo.Modification == ModificationKind.Customizations))
		{
			return true;
		}
		return false;
	}
}
