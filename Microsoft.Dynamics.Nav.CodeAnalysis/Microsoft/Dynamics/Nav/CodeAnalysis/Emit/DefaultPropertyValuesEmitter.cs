using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class DefaultPropertyValuesEmitter
{
	private readonly XmlWriter xmlWriter;

	private readonly Symbol declaringSymbol;

	private readonly IEnumerable<PropertySymbol> properties;

	private readonly NavTypeKind declaringSymbolKind;

	internal DefaultPropertyValuesEmitter(XmlWriter xmlWriter, Symbol declaringSymbol, IEnumerable<PropertySymbol> properties)
	{
		this.xmlWriter = xmlWriter;
		this.declaringSymbol = declaringSymbol;
		this.properties = properties;
		declaringSymbolKind = this.declaringSymbol.GetNavTypeKind();
	}

	internal void WriteDefaultPropertyValues()
	{
		IEnumerable<PropertyTypeInfo> enumerable = declaringSymbol.PropertyTypeInfos?.Values;
		if (enumerable != null)
		{
			WriteDefaultPropertyValues(enumerable);
		}
	}

	private void WriteDefaultPropertyValues(IEnumerable<PropertyTypeInfo> propertyTypeInfos)
	{
		HashSet<string> addedProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		IDictionary<string, PropertySymbol> declaredPropertiesByName = properties.ToImmutableDictionary((PropertySymbol p) => p.Name, (PropertySymbol p) => p, StringComparer.OrdinalIgnoreCase);
		IDictionary<string, DefaultProperty> dictionary = propertyTypeInfos.Where((PropertyTypeInfo p) => p.AlwaysOutputValue).ToImmutableDictionary((PropertyTypeInfo pti) => pti.Name, (PropertyTypeInfo pti) => new DefaultProperty(pti), StringComparer.OrdinalIgnoreCase);
		foreach (DefaultProperty item in dictionary.Values.Where((DefaultProperty p) => p.EmitAsAttribute))
		{
			WriteDefaultPropertyValue(item, declaredPropertiesByName, dictionary, addedProperties);
		}
		foreach (DefaultProperty item2 in dictionary.Values.Where((DefaultProperty p) => !p.EmitAsAttribute))
		{
			WriteDefaultPropertyValue(item2, declaredPropertiesByName, dictionary, addedProperties);
		}
	}

	private void WriteDefaultPropertyValue(DefaultProperty property, IDictionary<string, PropertySymbol> declaredPropertiesByName, IDictionary<string, DefaultProperty> defaultProperties, ISet<string> addedProperties)
	{
		if (!property.AlwaysOutputMetadata || addedProperties.Contains(property.Name) || declaredPropertiesByName.ContainsKey(property.Name) || !WriteDependentPropertiesDefaultValues(property, declaredPropertiesByName, defaultProperties, addedProperties))
		{
			return;
		}
		if (declaringSymbolKind != NavTypeKind.None)
		{
			foreach (DependentProperty dependentParentProperty in property.DependentParentProperties)
			{
				if (!dependentParentProperty.AllowedValues.Contains(declaringSymbolKind.ToString()))
				{
					return;
				}
			}
		}
		WriteDefaultPropertyValue(property, addedProperties);
	}

	private bool WriteDependentPropertiesDefaultValues(DefaultProperty property, IDictionary<string, PropertySymbol> declaredPropertiesByName, IDictionary<string, DefaultProperty> defaultProperties, ISet<string> addedProperties)
	{
		foreach (DependentProperty dependentProperty in property.DependentProperties)
		{
			DefaultProperty value2;
			if (declaredPropertiesByName.TryGetValue(dependentProperty.Name, out var value))
			{
				NavTypeKind navTypeKindSafe = value.Property.PropertyValue.Type.GetNavTypeKindSafe();
				if (!string.IsNullOrEmpty(value.ValueText) && (navTypeKindSafe != NavTypeKind.Boolean || !(bool)value.Value) && !dependentProperty.AllowedValues.Contains(value.ValueText, SemanticFacts.NameEqualityComparer))
				{
					return false;
				}
			}
			else if (defaultProperties.TryGetValue(dependentProperty.Name, out value2))
			{
				WriteDefaultPropertyValue(value2, declaredPropertiesByName, defaultProperties, addedProperties);
				if (!addedProperties.Contains(value2.Name))
				{
					return false;
				}
			}
		}
		return true;
	}

	private void WriteDefaultPropertyValue(DefaultProperty property, ISet<string> addedProperties)
	{
		if (property.DefaultValue != null && property.AlwaysOutputMetadata && !addedProperties.Contains(property.Name))
		{
			if (string.Equals(property.PropertyTypeInfo.TypeName, "Enum", StringComparison.OrdinalIgnoreCase))
			{
				ObjectMetadataEmitHelper.WriteEnumPropertyValue(xmlWriter, property);
			}
			else
			{
				ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, property.MetadataName, property.DefaultValue, property.EmitAsAttribute);
			}
			addedProperties.Add(property.Name);
		}
	}
}
