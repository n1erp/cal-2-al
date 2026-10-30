using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class TableBaseMetadataEmitter<T> : ObjectMetadataEmitter<T> where T : ApplicationObjectTypeSymbol
{
	private readonly ExtensionEmitValues extensionEmitValues;

	private readonly string sourceAppId;

	protected TableBaseMetadataEmitter(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		extensionEmitValues = ModuleBuilder.Options.ExtensionEmitValues;
		if (extensionEmitValues != null)
		{
			sourceAppId = extensionEmitValues.AppId.ToString();
		}
	}

	protected void WriteField(XmlWriter xmlWriter, SourceFieldSymbol field, string elementName = "Field")
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, elementName, () =>
		{
			WriteFieldAttributeStrings(field, xmlWriter);
			IEnumerable<PropertySymbol> enumerable = field.Properties.Where((PropertySymbol property) => property.PropertyKind != PropertyKind.CaptionClass);
			if (enumerable.All((PropertySymbol p) => p.PropertyKind != PropertyKind.CaptionML && p.PropertyKind != PropertyKind.Caption))
			{
				TryWriteDefaultCaptionProperty(xmlWriter, field);
			}
			WriteProperties(xmlWriter, field, enumerable, shouldOutputDefaultValues: true);
		});
	}

	private static void TryWriteDefaultCaptionProperty(XmlWriter xmlWriter, SourceFieldSymbol field)
	{
		string value = (field.DeclaringCompilation?.LanguagesMetadata)?.GetLanguagesForSymbolByLanguageName(field, null);
		ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, PropertyKind.CaptionML.ToString(), value, emitAsAttribute: true);
	}

	protected virtual void WriteFieldAttributeStrings(FieldSymbol fieldSymbol, XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("Name", fieldSymbol.Name);
		xmlWriter.WriteAttributeString("ID", fieldSymbol.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("Datatype", fieldSymbol.Type.NavTypeKind.GetNavTypeEnumNameString());
		if (fieldSymbol.Type.IsTextType())
		{
			TypeSymbol type = fieldSymbol.Type;
			DebugAssertHelper.Assert(type != null);
			xmlWriter.WriteAttributeString("DataLength", type.Length.ToString(CultureInfo.InvariantCulture));
		}
		else if (fieldSymbol.Type.Kind == SymbolKind.Enum)
		{
			WriteFieldEnumAttributes(xmlWriter, fieldSymbol);
		}
		WriteFieldCaptionClassAttribute(xmlWriter, fieldSymbol);
		foreach (SourceTriggerSymbol item in fieldSymbol.Triggers.Cast<SourceTriggerSymbol>())
		{
			xmlWriter.WriteAttributeString(item.TriggerInfo.Name, "1");
		}
		WriteExtensionSourceAttributesIfNecessary(xmlWriter);
	}

	private static void WriteFieldEnumAttributes(XmlWriter xmlWriter, FieldSymbol fieldSymbol)
	{
		EnumTypeSymbol enumTypeSymbol = fieldSymbol.Type as EnumTypeSymbol;
		xmlWriter.WriteAttributeString("EnumTypeName", enumTypeSymbol.MetadataName);
		xmlWriter.WriteAttributeString("EnumTypeId", enumTypeSymbol.Id.ToString(CultureInfo.InvariantCulture));
	}

	private static void WriteFieldCaptionClassAttribute(XmlWriter xmlWriter, FieldSymbol field)
	{
		PropertySymbol property = field.GetProperty(PropertyKind.CaptionClass);
		if (property != null)
		{
			WriteCaptionClassAttribute(xmlWriter, property);
		}
	}

	private static void WriteCaptionClassAttribute(XmlWriter xmlWriter, PropertySymbol captionClassProperty)
	{
		string text = captionClassProperty.Property.PropertyValue.ValueText;
		if (text.IsQuotedStringLiteral())
		{
			text = text.UnquoteLiteral();
		}
		ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, captionClassProperty.MetadataName, text, captionClassProperty.IsEmittedAsAttribute());
	}

	protected void WriteKeyAttributes(FieldNameListSymbol sourceFieldNameListSymbol, XmlWriter xmlWriter)
	{
		string fieldPrefixedAttributeValues = MetadataEmitterHelper.GetFieldPrefixedAttributeValues(sourceFieldNameListSymbol.Fields);
		xmlWriter.WriteAttributeString("Key", fieldPrefixedAttributeValues);
		xmlWriter.WriteAttributeString("KeyName", sourceFieldNameListSymbol.Name);
		WriteExtensionSourceAttributesIfNecessary(xmlWriter);
		WriteProperties(xmlWriter, sourceFieldNameListSymbol, sourceFieldNameListSymbol.Properties, shouldOutputDefaultValues: true);
	}

	protected override bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		switch (property.Property.PropertyValue.Kind)
		{
		case BoundKind.TableRelationPropertyValue:
			PropertyExpressionEmitterHelper.WriteTableRelationPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.CalculationFormulaPropertyValue:
			PropertyExpressionEmitterHelper.WriteCalculationFormulaPropertyValue(xmlWriter, property);
			return true;
		default:
			switch (property.PropertyKind)
			{
			case PropertyKind.MinValue:
			case PropertyKind.MaxValue:
			case PropertyKind.InitValue:
				if (property.Property.PropertyValue.ConstantValue == null)
				{
					break;
				}
				switch (property.Property.PropertyValue.ConstantValue.Discriminator)
				{
				case ConstantValueTypeDiscriminator.Date:
				{
					int dateValue = property.Property.PropertyValue.ConstantValue.DateValue;
					if (dateValue == 0L)
					{
						return true;
					}
					xmlWriter.WriteAttributeString(property.MetadataName, DateTimeUtilities.FormatCSideDate(dateValue, "yyyy-MM-dd"));
					return true;
				}
				case ConstantValueTypeDiscriminator.Time:
					xmlWriter.WriteAttributeString(property.MetadataName, DateTimeUtilities.FormatCSideTime(property.Property.PropertyValue.ConstantValue.TimeValue, "HH:mm:ss.fff"));
					return true;
				}
				break;
			case PropertyKind.CaptionClass:
				WriteCaptionClassAttribute(xmlWriter, property);
				return true;
			}
			return false;
		}
	}

	protected void WriteExtensionSourceAttributesIfNecessary(XmlWriter xmlWriter)
	{
		if (extensionEmitValues != null)
		{
			string value = 2.ToString(CultureInfo.InvariantCulture);
			xmlWriter.WriteAttributeString("SourceAppId", sourceAppId);
			xmlWriter.WriteAttributeString("SourceExtensionType", value);
		}
	}
}
