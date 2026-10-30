using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Collections;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter.Utility;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal static class ObjectMetadataEmitHelper
{
	internal static void WriteStartEndElement(XmlWriter xmlWriter, string elementName, Action action)
	{
		xmlWriter.WriteStartElement(elementName);
		action();
		xmlWriter.WriteEndElement();
	}

	internal static void WriteProperties(XmlWriter xmlWriter, Symbol declaringSymbol, IEnumerable<PropertySymbol> properties, Action<XmlWriter, PropertySymbol> writePropertyMetadata, bool shouldOutputDefaultProperties)
	{
		WriteAttributeProperties(xmlWriter, properties, writePropertyMetadata);
		WriteDefaultProperties(xmlWriter, declaringSymbol, properties, shouldOutputDefaultProperties);
		WriteElementProperties(xmlWriter, properties, writePropertyMetadata);
	}

	private static void WriteDefaultProperties(XmlWriter xmlWriter, Symbol declaringSymbol, IEnumerable<PropertySymbol> properties, bool shouldOutputDefaultProperties)
	{
		if (shouldOutputDefaultProperties)
		{
			new DefaultPropertyValuesEmitter(xmlWriter, declaringSymbol, properties).WriteDefaultPropertyValues();
		}
	}

	private static void WriteElementProperties(XmlWriter xmlWriter, IEnumerable<PropertySymbol> properties, Action<XmlWriter, PropertySymbol> writePropertyMetadata)
	{
		foreach (PropertySymbol property in properties)
		{
			if (!property.IsEmittedAsAttribute() && property.ShouldBeEmitted())
			{
				writePropertyMetadata(xmlWriter, property);
			}
		}
	}

	private static void WriteAttributeProperties(XmlWriter xmlWriter, IEnumerable<PropertySymbol> properties, Action<XmlWriter, PropertySymbol> writePropertyMetadata)
	{
		foreach (PropertySymbol property in properties)
		{
			if (property.IsEmittedAsAttribute() && property.ShouldBeEmitted())
			{
				writePropertyMetadata(xmlWriter, property);
			}
		}
	}

	internal static void WriteEnumPropertyValue(XmlWriter xmlWriter, DefaultProperty property)
	{
		WriteEnumPropertyValue(xmlWriter, (EnumPropertyTypeInfo)property.PropertyTypeInfo, property.MetadataName, property.DefaultValue);
	}

	internal static void WritePropertyValueMetadata(XmlWriter xmlWriter, PropertySymbol property)
	{
		DebugAssertHelper.Assert(property != null && property.Property != null && property.Property.PropertyValue != null);
		BoundPropertyValue propertyValue = property.Property.PropertyValue;
		switch (propertyValue.Kind)
		{
		case BoundKind.PermissionPropertyValue:
			xmlWriter.WritePermissionPropertyValue((BoundPermissionPropertyValue)propertyValue);
			return;
		case BoundKind.ApplicationObjectReferencePropertyValue:
			WriteApplicationObjectReferencePropertyValue(xmlWriter, property);
			return;
		case BoundKind.OptionMembersPropertyValue:
			WriteOptionMembersPropertyValue(xmlWriter, property);
			return;
		case BoundKind.EnumPropertyValue:
			WriteEnumPropertyValue(xmlWriter, property);
			return;
		case BoundKind.LabelPropertyValue:
			WriteLabelPropertyValue(xmlWriter, property);
			return;
		case BoundKind.MultilanguagePropertyValue:
			WriteMultilanguagePropertyValue(xmlWriter, property);
			return;
		case BoundKind.ConstantValueListPropertyValue:
			WriteConstantValueListPropertyValue(xmlWriter, property, property.IsEmittedAsAttribute());
			return;
		}
		string value = propertyValue.ValueText;
		BoundProperty property2 = property.Property;
		if (property2.PropertyValue.Syntax.Kind.IsPropertyClientExpresion())
		{
			value = RewriteClientSideBoolean(property2);
		}
		WritePropertyValue(xmlWriter, property.MetadataName, value, property.IsEmittedAsAttribute());
	}

	private static string RewriteClientSideBoolean(BoundProperty boundProperty)
	{
		return ClientSideExpressionGlobalsRewriter.RewrittenSyntax(boundProperty);
	}

	internal static void WriteNodeWithValue(XmlWriter xmlWriter, string nodeName, string nodeValue)
	{
		DebugAssertHelper.Assert(!string.IsNullOrWhiteSpace(nodeName));
		WriteStartEndElement(xmlWriter, nodeName, () =>
		{
			xmlWriter.WriteValue(nodeValue);
		});
	}

	internal static void WritePropertyValue(XmlWriter xmlWriter, string key, string value, bool emitAsAttribute)
	{
		if (emitAsAttribute)
		{
			xmlWriter.WriteAttributeString(key, value);
		}
		else
		{
			xmlWriter.WriteElementString(key, value);
		}
	}

	private static void WriteEnumPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		EnumPropertyTypeInfo info = (EnumPropertyTypeInfo)((BoundEnumPropertyValue)property.Property.PropertyValue).GetPropertyTypeInfo();
		WriteEnumPropertyValue(xmlWriter, info, property.MetadataName, property.ValueText);
	}

	private static void WriteEnumPropertyValue(XmlWriter xmlWriter, EnumPropertyTypeInfo info, string metadataName, string value)
	{
		string text = info.Options.First((string v) => v.Equals(value, StringComparison.OrdinalIgnoreCase));
		switch (info.Kind)
		{
		case PropertyKind.ExtendedDatatype:
		{
			string value2 = ExtendedDataTypeToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		case PropertyKind.TextEncoding:
		{
			string value2 = TextEncodingToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		case PropertyKind.Encoding:
		{
			string value2 = EncodingToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		case PropertyKind.Format:
		{
			string value2 = FormatToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		case PropertyKind.FormatEvaluate:
		{
			string value2 = FormatEvaluateToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		case PropertyKind.XmlVersionNo:
		{
			string value2 = XmlVersionToString(text);
			WritePropertyValue(xmlWriter, metadataName, value2, info.EmitAsAttribute);
			break;
		}
		default:
			WritePropertyValue(xmlWriter, metadataName, text, info.EmitAsAttribute);
			break;
		}
	}

	private static void WriteMultilanguagePropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (TryGetCaptionMLPropertyValue(property, out var propertyValue))
		{
			WritePropertyValue(xmlWriter, GetCaptionMLElementName(property), propertyValue, property.IsEmittedAsAttribute());
		}
	}

	internal static void WriteLabelPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		if (TryGetCaptionMLPropertyValue(property, out var propertyValue))
		{
			string key = GetCaptionMLElementName(property) + "ML";
			WritePropertyValue(xmlWriter, key, propertyValue, property.IsEmittedAsAttribute());
		}
	}

	internal static bool TryGetCaptionMLPropertyValue(PropertySymbol property, out string propertyValue)
	{
		propertyValue = null;
		if (property.Property != null && property.Property.PropertyValue != null)
		{
			BoundPropertyValue propertyValue2 = property.Property.PropertyValue;
			if (propertyValue2.Kind == BoundKind.MultilanguagePropertyValue)
			{
				BoundMultilanguagePropertyValue boundMultilanguagePropertyValue = (BoundMultilanguagePropertyValue)propertyValue2;
				propertyValue = boundMultilanguagePropertyValue.ValueText;
			}
			else if (propertyValue2.Kind == BoundKind.LabelPropertyValue)
			{
				BoundLabelPropertyValue boundLabelPropertyValue = (BoundLabelPropertyValue)propertyValue2;
				propertyValue = (property.DeclaringCompilation?.LanguagesMetadata)?.GetLanguagesForSymbolByLanguageName(property, boundLabelPropertyValue.BoundLabel);
			}
		}
		return propertyValue != null;
	}

	internal static string GetCaptionMLElementName(PropertySymbol property)
	{
		switch (property.PropertyKind)
		{
		case PropertyKind.RequestFilterHeading:
			return "Caption";
		case PropertyKind.RequestFilterHeadingML:
			if (property.ContainingSymbol is XmlPortNodeSymbol { NodeXmlKind: XmlPortNodeXmlKind.Element })
			{
				return property.MetadataName;
			}
			return "CaptionML";
		default:
			return property.MetadataName;
		}
	}

	internal static string GetCaptionElementName(PropertySymbol property)
	{
		PropertyKind propertyKind = property.PropertyKind;
		if (propertyKind == PropertyKind.RequestFilterHeadingML)
		{
			return "Caption";
		}
		string text = property.PropertyKind.ToString();
		return text.Substring(0, text.Length - 2);
	}

	private static void WriteApplicationObjectReferencePropertyValue(XmlWriter xmlWriter, PropertySymbol propertySymbol)
	{
		BoundApplicationObjectReferencePropertyValue boundApplicationObjectReferencePropertyValue = (BoundApplicationObjectReferencePropertyValue)propertySymbol.Property.PropertyValue;
		if (!(boundApplicationObjectReferencePropertyValue.Value == null))
		{
			WritePropertyValue(xmlWriter, propertySymbol.MetadataName, boundApplicationObjectReferencePropertyValue.Value.Id.ToString(CultureInfo.InvariantCulture), propertySymbol.GetPropertyTypeInfo().EmitAsAttribute);
		}
	}

	private static void WriteOptionMembersPropertyValue(XmlWriter writer, PropertySymbol propertySymbol)
	{
		BoundOptionMembersPropertyValue boundOptionMembersPropertyValue = (BoundOptionMembersPropertyValue)propertySymbol.Property.PropertyValue;
		WritePropertyValue(writer, propertySymbol.MetadataName, boundOptionMembersPropertyValue.ValueText, propertySymbol.IsEmittedAsAttribute());
	}

	private static void WriteConstantValueListPropertyValue(XmlWriter writer, PropertySymbol propertySymbol, bool emitAsAttribute)
	{
		string separator = ",";
		PropertyKind propertyKind = propertySymbol.PropertyKind;
		Func<ConstantValue, string> selector;
		if (propertyKind == PropertyKind.ValuesAllowed || propertyKind == PropertyKind.OptionOrdinalValues)
		{
			separator = ";";
			selector = GetConstantValueForValuesAllowedProperty;
		}
		else
		{
			selector = (ConstantValue c) => c.StringValue;
		}
		BoundConstantValueListPropertyValue boundConstantValueListPropertyValue = (BoundConstantValueListPropertyValue)propertySymbol.Property.PropertyValue;
		string value = string.Join(separator, boundConstantValueListPropertyValue.Values.Select(selector));
		WritePropertyValue(writer, propertySymbol.MetadataName, value, emitAsAttribute);
	}

	internal static string GetConstantValueForValuesAllowedProperty(ConstantValue value)
	{
		switch (value.Discriminator)
		{
		case ConstantValueTypeDiscriminator.Boolean:
			if (!value.BooleanValue)
			{
				return "No";
			}
			return "Yes";
		case ConstantValueTypeDiscriminator.String:
			return value.StringValue;
		default:
			return string.Format(CultureInfo.InvariantCulture, "{0}", value.Value);
		}
	}

	internal static string ExtendedDataTypeToString(string dataType)
	{
		ExtendedDatatypeKind extendedDatatypeKind = (ExtendedDatatypeKind)Enum.Parse(typeof(ExtendedDatatypeKind), dataType);
		return extendedDatatypeKind switch
		{
			ExtendedDatatypeKind.None => "None", 
			ExtendedDatatypeKind.PhoneNo => "Phone No.", 
			ExtendedDatatypeKind.URL => "URL", 
			ExtendedDatatypeKind.EMail => "E-Mail", 
			ExtendedDatatypeKind.Ratio => "Ratio", 
			ExtendedDatatypeKind.Masked => "Masked", 
			ExtendedDatatypeKind.Person => "Person", 
			_ => throw ExceptionUtilities.UnexpectedValue(extendedDatatypeKind), 
		};
	}

	private static string TextEncodingToString(string properlyCasedValueText)
	{
		TextEncodingKind textEncodingKind = (TextEncodingKind)Enum.Parse(typeof(TextEncodingKind), properlyCasedValueText);
		return textEncodingKind switch
		{
			TextEncodingKind.MSDOS => "MS-DOS", 
			TextEncodingKind.UTF16 => "UTF-16", 
			TextEncodingKind.UTF8 => "UTF-8", 
			TextEncodingKind.WINDOWS => "WINDOWS", 
			_ => throw ExceptionUtilities.UnexpectedValue(textEncodingKind), 
		};
	}

	private static string FormatToString(string properlyCasedValueText)
	{
		FormatKind formatKind = (FormatKind)Enum.Parse(typeof(FormatKind), properlyCasedValueText);
		return formatKind switch
		{
			FormatKind.FixedText => "Fixed Text", 
			FormatKind.VariableText => "Variable Text", 
			FormatKind.Xml => "Xml", 
			_ => throw ExceptionUtilities.UnexpectedValue(formatKind), 
		};
	}

	private static string EncodingToString(string properlyCasedValueText)
	{
		EncodingKind encodingKind = (EncodingKind)Enum.Parse(typeof(EncodingKind), properlyCasedValueText);
		return encodingKind switch
		{
			EncodingKind.ISO88592 => "ISO-8859-2", 
			EncodingKind.UTF16 => "UTF-16", 
			EncodingKind.UTF8 => "UTF-8", 
			_ => throw ExceptionUtilities.UnexpectedValue(encodingKind), 
		};
	}

	private static string FormatEvaluateToString(string properlyCasedValueText)
	{
		FormatEvaluateKind formatEvaluateKind = (FormatEvaluateKind)Enum.Parse(typeof(FormatEvaluateKind), properlyCasedValueText);
		return formatEvaluateKind switch
		{
			FormatEvaluateKind.Legacy => "C/SIDE Format/Evaluate", 
			FormatEvaluateKind.Xml => "XML Format/Evaluate", 
			_ => throw ExceptionUtilities.UnexpectedValue(formatEvaluateKind), 
		};
	}

	private static string XmlVersionToString(string properlyCasedValueText)
	{
		XmlVersionNoKind xmlVersionNoKind = (XmlVersionNoKind)Enum.Parse(typeof(XmlVersionNoKind), properlyCasedValueText);
		return xmlVersionNoKind switch
		{
			XmlVersionNoKind.V10 => "1.0", 
			XmlVersionNoKind.V11 => "1.1", 
			_ => throw ExceptionUtilities.UnexpectedValue(xmlVersionNoKind), 
		};
	}

	internal static void WriteTableFilterStatementPropertyValue(XmlWriter xmlWriter, SourcePropertySymbol property)
	{
		BoundTableFilterStatementPropertyValue boundTableFilterStatementPropertyValue = (BoundTableFilterStatementPropertyValue)property.Property.PropertyValue;
		DebugAssertHelper.Assert(boundTableFilterStatementPropertyValue.GetPropertyTypeInfo() != null);
		if (!(boundTableFilterStatementPropertyValue.TableFilter?.Condition != null))
		{
			return;
		}
		ArrayBuilder<string> instance = ArrayBuilder<string>.GetInstance();
		try
		{
			foreach (BoundEqualsPropertyExpression item2 in boundTableFilterStatementPropertyValue.TableFilter.Condition)
			{
				int? num = (item2.Left as BoundFieldAccess)?.FieldSymbol.Id;
				int? boundFieldExpressionId = item2.GetBoundFieldExpressionId();
				if (num.HasValue && boundFieldExpressionId.HasValue)
				{
					string item = string.Format(CultureInfo.InvariantCulture, "Field{0}=FIELD(Field{1})", num, boundFieldExpressionId);
					instance.Add(item);
				}
			}
			xmlWriter.WriteElementString(property.MetadataName, string.Join(",", instance));
		}
		finally
		{
			instance.Free();
		}
	}

	internal static bool AllowControlSourceExpressionDuplicate(BoundExpression sourceExpression)
	{
		return !SourceExpressionAnalyzer.CanBeReused(sourceExpression);
	}

	internal static void WriteTableViewMetadataPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundTableViewPropertyValue boundTableViewPropertyValue = (BoundTableViewPropertyValue)property.Property.PropertyValue;
		DebugAssertHelper.Assert(boundTableViewPropertyValue.GetPropertyTypeInfo() != null);
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			if (boundTableViewPropertyValue.Sorting != null && boundTableViewPropertyValue.Sorting.Length > 0)
			{
				instance.Builder.Append("SORTING");
				instance.Builder.Append("(");
				instance.Builder.Append(MetadataEmitterHelper.GetFieldPrefixedAttributeValues(boundTableViewPropertyValue.Sorting.Select((BoundExpression v) => v.ExpressionSymbol).Cast<FieldSymbol>()));
				instance.Builder.Append(")");
			}
			if (boundTableViewPropertyValue.IsAscending.HasValue)
			{
				if (instance.Builder.Length > 0)
				{
					instance.Builder.Append(" ");
				}
				instance.Builder.Append("ORDER");
				instance.Builder.Append("(");
				instance.Builder.Append((!boundTableViewPropertyValue.IsAscending.Value) ? "1" : "0");
				instance.Builder.Append(")");
			}
			if (boundTableViewPropertyValue.TableFilter != null)
			{
				if (instance.Builder.Length > 0)
				{
					instance.Builder.Append(" ");
				}
				instance.Builder.Append("WHERE");
				instance.Builder.Append("(");
				instance.Builder.Append(WriteTableViewWhereClause(boundTableViewPropertyValue));
				instance.Builder.Append(")");
			}
			xmlWriter.WriteElementString(property.MetadataName, instance.Builder.ToString());
		}
		finally
		{
			instance.Free();
		}
	}

	private static string WriteTableViewWhereClause(BoundTableViewPropertyValue tableView)
	{
		PooledStringBuilder instance = PooledStringBuilder.GetInstance();
		try
		{
			foreach (BoundEqualsPropertyExpression item in tableView.TableFilter.Condition)
			{
				if (instance.Builder.Length > 0)
				{
					instance.Builder.Append(",");
				}
				FieldSymbol fieldSymbol = ((BoundFieldAccess)item.Left).FieldSymbol;
				instance.Builder.Append("Field" + fieldSymbol.Id);
				instance.Builder.Append("=");
				instance.Builder.Append((item.Right.Operator == SyntaxKind.FilterExpression) ? "1" : "0");
				instance.Builder.Append("(");
				string value = FilterValueWriterVisitor.CreateFilterOutput(item.Right, emitBooleanAsNumeral: true);
				instance.Builder.Append(value);
				instance.Builder.Append(")");
			}
			return instance.Builder.ToString();
		}
		finally
		{
			instance.Free();
		}
	}
}
