using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class TableExtensionMetadataEmitter : TableBaseMetadataEmitter<SourceTableExtensionTypeSymbol>
{
	internal TableExtensionMetadataEmitter(SourceTableExtensionTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("MetadataRuntimeDeltas", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
			xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
			xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", Symbol.Name);
			if (Symbol.Properties.Any())
			{
				WriteTableProperties(xmlWriter);
			}
			foreach (FieldSymbol addedField in Symbol.GetAddedFields())
			{
				WriteFieldAddElements(addedField as SourceFieldSymbol, xmlWriter);
			}
			foreach (SourceChangeModifyFieldSymbol item in Symbol.ModifiedFields())
			{
				WriteFieldChangeElements(item, xmlWriter);
			}
			foreach (KeySymbol addedKey in Symbol.GetAddedKeys())
			{
				WriteKeyAddElements(addedKey, xmlWriter);
			}
			foreach (SourceChangeAddFieldGroupSymbol changedFieldGroup in Symbol.GetChangedFieldGroups())
			{
				WriteFieldGroupAddLastElements(changedFieldGroup, xmlWriter);
			}
			xmlWriter.WriteEndElement();
		});
	}

	private void WriteTableProperties(XmlWriter xmlWriter)
	{
		foreach (PropertySymbol property in Symbol.Properties)
		{
			if (property.PropertyKind == PropertyKind.Caption)
			{
				WriteCaptionPropertyChangeElement(xmlWriter, property);
			}
			else
			{
				WritePropertyChangeElements(property, xmlWriter);
			}
		}
	}

	private static void WritePropertyChangeElements(PropertySymbol property, XmlWriter xmlWriter)
	{
		PropertyTypeInfo propertyTypeInfo = property.GetPropertyTypeInfo();
		if (propertyTypeInfo == null || propertyTypeInfo.CanGenerateMetadata)
		{
			xmlWriter.WriteStartElement("PropertyChange");
			string modifiedPropertyValue = GetModifiedPropertyValue(property);
			if (!string.IsNullOrEmpty(modifiedPropertyValue))
			{
				xmlWriter.WriteAttributeString("PropertyName", property.MetadataName);
				xmlWriter.WriteAttributeString("PropertyValue", modifiedPropertyValue);
			}
			xmlWriter.WriteEndElement();
		}
	}

	private static void WriteCaptionPropertyChangeElement(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (ObjectMetadataEmitHelper.TryGetCaptionMLPropertyValue(property, out var propertyValue))
		{
			xmlWriter.WriteStartElement("PropertyChange");
			xmlWriter.WriteAttributeString("PropertyName", "CaptionML");
			xmlWriter.WriteAttributeString("PropertyValue", propertyValue);
			xmlWriter.WriteEndElement();
		}
	}

	private static string GetModifiedPropertyValue(PropertySymbol property)
	{
		BoundPropertyValue propertyValue = property.Property.PropertyValue;
		BoundKind kind = propertyValue.Kind;
		if (kind == BoundKind.ApplicationObjectReferencePropertyValue)
		{
			return ((BoundApplicationObjectReferencePropertyValue)property.Property.PropertyValue).Value.Id.ToString(CultureInfo.InvariantCulture);
		}
		return propertyValue.ValueText;
	}

	private void WriteFieldAddElements(SourceFieldSymbol fieldSymbol, XmlWriter xmlWriter)
	{
		WriteField(xmlWriter, fieldSymbol, "FieldAdd");
	}

	private void WriteFieldChangeElements(SourceChangeModifyFieldSymbol fieldSymbol, XmlWriter xmlWriter)
	{
		FieldSymbol fieldSymbol2 = fieldSymbol.Target as FieldSymbol;
		xmlWriter.WriteStartElement("FieldChange");
		xmlWriter.WriteAttributeString("TargetID", fieldSymbol2.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("TargetType", "MetaField");
		WriteProperties(xmlWriter, fieldSymbol, fieldSymbol.Properties, shouldOutputDefaultValues: false);
		foreach (SourceTriggerSymbol item in fieldSymbol.Triggers.Cast<SourceTriggerSymbol>())
		{
			xmlWriter.WriteAttributeString(item.TriggerInfo.Name, "1");
		}
		xmlWriter.WriteEndElement();
	}

	private void WriteKeyAddElements(FieldNameListSymbol sourceFieldNameListSymbol, XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("KeyAdd");
		WriteKeyAttributes(sourceFieldNameListSymbol, xmlWriter);
		xmlWriter.WriteEndElement();
	}

	private void WriteFieldGroupAddLastElements(SourceChangeAddFieldGroupSymbol sourceFieldNameListSymbol, XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("FieldGroupItemAdd");
		xmlWriter.WriteAttributeString("GroupName", sourceFieldNameListSymbol.Anchor.Name);
		string fieldPrefixedAttributeValues = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(sourceFieldNameListSymbol.AddedSymbols.OfType<FieldSymbol>());
		xmlWriter.WriteAttributeString("GroupFields", fieldPrefixedAttributeValues);
		xmlWriter.WriteEndElement();
	}
}
