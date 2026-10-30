using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Translation;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ReportMetadataEmitter : ObjectMetadataEmitter<SourceReportTypeSymbol>
{
	public ReportMetadataEmitter(SourceReportTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		if (!diagnostics.HasAnyErrors())
		{
			EmitMetadata((XmlWriter xmlWriter) =>
			{
				xmlWriter.WriteStartElement("Report");
				WriteProperties(xmlWriter, Symbol, Symbol.Properties, shouldOutputDefaultValues: true);
				WriteReportNameId(xmlWriter);
				WriteDataItems(xmlWriter);
				RequestPageMetadataEmitter.Write(xmlWriter, Symbol.RequestPage, Symbol, ModuleBuilder, diagnostics);
				WriteLabels(xmlWriter);
				ConsumeReportLayouts(xmlWriter);
				xmlWriter.WriteEndElement();
			});
		}
	}

	private void ConsumeReportLayouts(XmlWriter xmlWriter)
	{
		ConsumeLayout(xmlWriter, PropertyKind.RDLCLayout, Utf8Converter, "Rdl", ReportRdlcUtilities.GetDefaultRdl());
		ConsumeLayout(xmlWriter, PropertyKind.WordLayout, Convert.ToBase64String, "WordLayout");
	}

	private static string Utf8Converter(byte[] bytes)
	{
		int num = FileEncodingUtilities.PreambleOffset(bytes, Encoding.UTF8);
		return Encoding.UTF8.GetString(bytes, num, bytes.Length - num);
	}

	private void ConsumeLayout(XmlWriter xmlWriter, PropertyKind propertyKind, Func<byte[], string> converter, string xmlMetadataConstant, string defaultLayout = null)
	{
		BoundLiteralPropertyValue boundPropertyValue = Symbol.GetBoundPropertyValue<BoundLiteralPropertyValue>(propertyKind);
		if (boundPropertyValue != null)
		{
			try
			{
				string content = converter(FileSystem.ReadBytes(boundPropertyValue.ValueText));
				ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, xmlMetadataConstant, () =>
				{
					xmlWriter.WriteCData(content);
				});
				return;
			}
			catch (IOException)
			{
				return;
			}
		}
		if (!string.IsNullOrEmpty(defaultLayout))
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, xmlMetadataConstant, () =>
			{
				xmlWriter.WriteCData(defaultLayout);
			});
		}
	}

	private void WriteReportNameId(XmlWriter xmlWriter)
	{
		xmlWriter.WriteElementString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteElementString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteElementString("Name", Symbol.Name);
	}

	private void WriteDataItems(XmlWriter xmlWriter)
	{
		if (Symbol.FlattenedDataItems.IsEmpty)
		{
			return;
		}
		foreach (SourceReportDataItemSymbol dataItem in Symbol.FlattenedDataItems)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "DataItem", () =>
			{
				WriteProperties(xmlWriter, dataItem, dataItem.Properties.Where((PropertySymbol p) => p.PropertyKind != PropertyKind.DataItemLinkReference), shouldOutputDefaultValues: true);
				xmlWriter.WriteElementString("DataItemTable", dataItem.RelatedTable.Id.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("DataItemIndent", dataItem.Indentation.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("ID", dataItem.Id.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("DataItemVarName", dataItem.Name);
				PropertySymbol property = dataItem.GetProperty(PropertyKind.DataItemLinkReference);
				if (property == null)
				{
					if (dataItem.ContainingSymbol.Kind == SymbolKind.ReportDataItem)
					{
						ReportDataItemSymbol reportDataItemSymbol = (ReportDataItemSymbol)dataItem.ContainingSymbol;
						xmlWriter.WriteElementString("DataItemLinkReference", reportDataItemSymbol.Name);
					}
				}
				else
				{
					xmlWriter.WriteElementString("DataItemLinkReference", property.Property.PropertyValue.ValueText);
				}
				if (dataItem.Columns != null && !dataItem.Columns.IsEmpty)
				{
					foreach (SourceReportColumnSymbol column in dataItem.Columns)
					{
						WriteDataItemColumn(xmlWriter, column);
					}
				}
			});
		}
	}

	private void WriteDataItemColumn(XmlWriter xmlWriter, SourceReportColumnSymbol column)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "DataItemField", () =>
		{
			WriteProperties(xmlWriter, column, column.Properties, shouldOutputDefaultValues: true);
			xmlWriter.WriteElementString("ID", column.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteElementString("FriendlyFieldName", column.Name);
			xmlWriter.WriteElementString("FieldType", GetColumnType(column));
			xmlWriter.WriteElementString("FieldNo", GetDataItemColumnFieldNo(column).ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteElementString("SourceExpr", column.SourceExpression.Syntax.ToString());
			if (column.SourceExpression.Type.NavTypeKind == NavTypeKind.Decimal)
			{
				string decimalFormatterColumnName = GetDecimalFormatterColumnName(column.Name);
				xmlWriter.WriteElementString("DecimalFormatter", decimalFormatterColumnName);
			}
		});
		WriteDecimalPlacesDataItemColumn(xmlWriter, column);
	}

	private static string GetColumnType(SourceReportColumnSymbol column)
	{
		NavTypeKind navTypeKindSafe = column.SourceExpression.Type.GetNavTypeKindSafe();
		switch (navTypeKindSafe)
		{
		case NavTypeKind.Blob:
		case NavTypeKind.Guid:
			return navTypeKindSafe.ToString().ToUpperInvariant();
		case NavTypeKind.String:
			if (IsComplexExpressionOrLiteral(column))
			{
				return "String";
			}
			return column.SourceExpression.Type.MetadataName;
		case NavTypeKind.TextConst:
		case NavTypeKind.Label:
			return "TextConst";
		case NavTypeKind.Enum:
			return "Option";
		default:
			return column.SourceExpression.Type.MetadataName;
		}
	}

	private static bool IsComplexExpressionOrLiteral(SourceReportColumnSymbol column)
	{
		if (column.SourceExpression.Kind != BoundKind.Literal && column.SourceExpression.Kind != BoundKind.BinaryOperator)
		{
			return column.SourceExpression.Kind == BoundKind.Call;
		}
		return true;
	}

	private static int GetDataItemColumnFieldNo(SourceReportColumnSymbol column)
	{
		if (!(column.SourceExpression is BoundFieldAccess boundFieldAccess))
		{
			return -1;
		}
		if (!(boundFieldAccess.ReceiverOpt is BoundReportDataItemAccess boundReportDataItemAccess))
		{
			return -1;
		}
		if ((object)column.ContainingSymbol == boundReportDataItemAccess.ReportDataItemSymbol)
		{
			return boundFieldAccess.FieldSymbol.Id;
		}
		return -1;
	}

	private static void WriteDecimalPlacesDataItemColumn(XmlWriter xmlWriter, SourceReportColumnSymbol column)
	{
		if (column.SourceExpression.Type.NavTypeKind == NavTypeKind.Decimal)
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "DataItemField", () =>
			{
				string decimalFormatterColumnName = GetDecimalFormatterColumnName(column.Name);
				xmlWriter.WriteElementString("IsDecimalFormatter", "1");
				xmlWriter.WriteElementString("FriendlyFieldName", decimalFormatterColumnName);
				xmlWriter.WriteElementString("FieldType", NavTypeKind.Text.ToString());
				xmlWriter.WriteElementString("AutoCalcField", "0");
				xmlWriter.WriteElementString("SourceExpr", decimalFormatterColumnName);
			});
		}
	}

	private static string GetDecimalFormatterColumnName(string columnName)
	{
		return columnName + "Format";
	}

	private void WriteLabels(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Labels", () =>
		{
			foreach (SourceReportColumnSymbol column in Symbol.FlattenedColumns)
			{
				if (column.GetBooleanPropertyValue(PropertyKind.IncludeCaption) == true)
				{
					ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Label", () =>
					{
						xmlWriter.WriteElementString("Name", column.MetadataName + "Caption");
						FieldSymbol fieldSymbol = ((BoundFieldAccess)column.SourceExpression).FieldSymbol;
						int id = fieldSymbol.Id;
						xmlWriter.WriteElementString("FieldNo", id.ToString(CultureInfo.InvariantCulture));
						int sourceTableIdFromFieldSymbol = GetSourceTableIdFromFieldSymbol(fieldSymbol);
						xmlWriter.WriteElementString("TableNo", sourceTableIdFromFieldSymbol.ToString(CultureInfo.InvariantCulture));
					});
				}
			}
			WriteMultilanguageLabels(xmlWriter, Symbol);
			WriteTranslatableLabels(xmlWriter, Symbol);
		});
	}

	private static int GetSourceTableIdFromFieldSymbol(FieldSymbol sourceFieldSymbol)
	{
		if (sourceFieldSymbol.ContainingSymbol.Kind != SymbolKind.Table)
		{
			return ((TableExtensionTypeSymbol)sourceFieldSymbol.ContainingSymbol).Target.Id;
		}
		return ((TableTypeSymbol)sourceFieldSymbol.ContainingSymbol).Id;
	}

	private static void WriteTranslatableLabels(XmlWriter xmlWriter, SourceReportTypeSymbol symbol)
	{
		foreach (SourceReportLabelSymbol label in (from r in symbol.GetMembersOfKind<ReportLabelSymbol>(SymbolKind.ReportLabel)
			where !r.IsMultilanguageReportLabel
			select r).Cast<SourceReportLabelSymbol>())
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Label", () =>
			{
				xmlWriter.WriteElementString("ID", label.Id.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("Name", label.Name);
				string value = (label.DeclaringCompilation?.LanguagesMetadata)?.GetLanguagesForSymbolByLanguageName(label, label.BoundLabel);
				xmlWriter.WriteElementString("CaptionML", value);
			});
		}
	}

	private static void WriteMultilanguageLabels(XmlWriter xmlWriter, SourceReportTypeSymbol symbol)
	{
		foreach (SourceReportLabelMultilanguageSymbol label in (from r in symbol.GetMembersOfKind<ReportLabelSymbol>(SymbolKind.ReportLabel)
			where r.IsMultilanguageReportLabel
			select r).Cast<SourceReportLabelMultilanguageSymbol>())
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Label", () =>
			{
				xmlWriter.WriteElementString("ID", label.Id.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteElementString("Name", label.Name);
				string multilanguageString = PropertyValueTextHelper.GetMultilanguageString(label.Captions.Select((BoundMultilanguageExpression l) => PropertyValueTextHelper.GetMultilanguageStringItem(l.LanguageId.ConstantValue.StringValue, l.LanguageValue.StringValue)));
				if (!string.IsNullOrEmpty(multilanguageString))
				{
					xmlWriter.WriteElementString("CaptionML", multilanguageString);
				}
			});
		}
	}

	protected override bool TryUseSpecialPropertyValueMetadataEmitter(XmlWriter xmlWriter, PropertySymbol property)
	{
		switch (property.Property.PropertyValue.Kind)
		{
		case BoundKind.TableFilterStatementPropertyValue:
			ObjectMetadataEmitHelper.WriteTableFilterStatementPropertyValue(xmlWriter, (SourcePropertySymbol)property);
			return true;
		case BoundKind.TableViewPropertyValue:
			ObjectMetadataEmitHelper.WriteTableViewMetadataPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.MultilanguagePropertyValue:
			WriteReportMultilanguagePropertyValue(xmlWriter, property);
			return true;
		case BoundKind.LabelPropertyValue:
			WriteReportLabelPropertyValue(xmlWriter, property);
			return true;
		case BoundKind.DataItemLinkPropertyValue:
			xmlWriter.WriteReportDataItemLinkPropertyValue((BoundDataItemLinkPropertyValue)property.Property.PropertyValue);
			return true;
		default:
			return false;
		}
	}

	private static void WriteReportLabelPropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundLabelPropertyValue boundLabelPropertyValue = (BoundLabelPropertyValue)property.Property.PropertyValue;
		List<(int languageId, string translatedText)> list = (property.DeclaringCompilation?.LanguagesMetadata)?.GetLanguagesForSymbolByLanguageId(property, boundLabelPropertyValue.BoundLabel);
		xmlWriter.WriteStartElement(ObjectMetadataEmitHelper.GetCaptionMLElementName(property) + "ML");
		foreach (var item in list)
		{
			xmlWriter.WriteStartElement(ObjectMetadataEmitHelper.GetCaptionMLElementName(property));
			var (num, _) = item;
			xmlWriter.WriteAttributeString("Id", num.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteString(item.translatedText);
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}

	internal static void WriteReportMultilanguagePropertyValue(XmlWriter xmlWriter, PropertySymbol property)
	{
		BoundMultilanguagePropertyValue boundMultilanguagePropertyValue = (BoundMultilanguagePropertyValue)property.Property.PropertyValue;
		xmlWriter.WriteStartElement(ObjectMetadataEmitHelper.GetCaptionMLElementName(property));
		foreach (BoundMultilanguageExpression item in boundMultilanguagePropertyValue.Value)
		{
			xmlWriter.WriteStartElement(ObjectMetadataEmitHelper.GetCaptionElementName(property));
			MultiLanguageHelper.TryFindCulture(((BoundLiteral)item.LanguageId).ConstantValue.StringValue, out var lcid);
			xmlWriter.WriteAttributeString("Id", lcid.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteString(item.LanguageValue.StringValue);
			xmlWriter.WriteEndElement();
		}
		xmlWriter.WriteEndElement();
	}
}
