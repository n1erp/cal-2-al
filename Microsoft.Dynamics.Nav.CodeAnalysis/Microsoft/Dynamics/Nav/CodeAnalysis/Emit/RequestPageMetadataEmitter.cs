using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class RequestPageMetadataEmitter : PageBaseMetadataEmitter<RequestPageTypeSymbol>
{
	private class DataItemTableView
	{
		public int DataItemIdOrIndex { get; }

		public int RelatedTableId { get; }

		public ImmutableArray<PropertySymbol> Properties { get; }

		public DataItemTableView(int dataItemIdOrIndex, int relatedTableId, ImmutableArray<PropertySymbol> properties)
		{
			DataItemIdOrIndex = dataItemIdOrIndex;
			RelatedTableId = relatedTableId;
			Properties = properties;
		}
	}

	private RequestPageTypeSymbol RequestPage { get; }

	private XmlWriter XmlWriter { get; }

	private ApplicationObjectTypeSymbol ContainingSymbol { get; }

	internal RequestPageMetadataEmitter(RequestPageTypeSymbol symbol, ApplicationObjectTypeSymbol parent, ModuleBuilder moduleBuilder, XmlWriter xmlWriter)
		: base(symbol, moduleBuilder)
	{
		DebugAssertHelper.Assert(parent != null);
		DebugAssertHelper.Assert(parent.Kind == SymbolKind.Report || parent.Kind == SymbolKind.XmlPort);
		RequestPage = symbol;
		ContainingSymbol = parent;
		XmlWriter = xmlWriter;
	}

	public static void Write(XmlWriter writer, RequestPageTypeSymbol requestPage, ApplicationObjectTypeSymbol applicationObject, ModuleBuilder builder, DiagnosticBag diagnostics)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(writer, "RequestPage", () =>
		{
			new RequestPageMetadataEmitter(requestPage, applicationObject, builder, writer).EmitMetadata(diagnostics);
		});
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		XmlWriter.WriteStartElement("PageDefinition", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		WritePageDefinitionProperties(XmlWriter);
		WritePageProperties(XmlWriter);
		WriteContent(XmlWriter);
		ImmutableArray<ActionSymbol> actions = Symbol.GetMembers().WhereAsCastArray<ActionSymbol, Symbol>((Symbol m) => m.Kind == SymbolKind.Action);
		WriteActions(XmlWriter, actions);
		WriteExpressions(XmlWriter);
		PageMetadataEmitHelper.WriteTriggers(XmlWriter, RequestPage.Triggers);
		XmlWriter.WriteEndElement();
	}

	private void WriteTableFilter(XmlWriter xmlWriter, DataItemTableView tableView)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Controls", () =>
		{
			xmlWriter.WriteAttributeString("xsi", "type", null, "ControlGroupDefinition");
			int memberId = IdSpace.GetMemberId(tableView.DataItemIdOrIndex, "ControlGroupDefinition");
			string value = memberId.ToString(CultureInfo.InvariantCulture);
			xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GenerateMemberId(memberId, ContainingSymbol.Id, ContainingSymbol.Kind));
			xmlWriter.WriteAttributeString("ID", value);
			WriteReqFilterHeadingPropertyIfPresent(xmlWriter, tableView);
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Controls", () =>
			{
				xmlWriter.WriteAttributeString("xsi", "type", null, "FilterControlDefinition");
				int memberId2 = IdSpace.GetMemberId(tableView.DataItemIdOrIndex, "FilterControlDefinition");
				xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GenerateMemberId(memberId2, ContainingSymbol.Id, ContainingSymbol.Kind));
				xmlWriter.WriteAttributeString("ID", memberId2.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("DataColumnName", MetadataEmitterHelper.GetDataItemTableViewName(tableView.DataItemIdOrIndex));
				WriteFilterTableId(xmlWriter, tableView);
				WriteTableViewIfPresent(xmlWriter, tableView);
				WriteRequestFilterFieldsIfPresent(xmlWriter, tableView);
				ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "FieldVariable", () =>
				{
					xmlWriter.WriteAttributeString("Datatype", "Binary");
				});
				foreach (PropertySymbol property in tableView.Properties)
				{
					if (property.PropertyKind == PropertyKind.DataItemTableView || property.PropertyKind == PropertyKind.SourceTableView)
					{
						PageMetadataEmitHelper.WriteTableViewMetadataPropertyValue(xmlWriter, property);
					}
				}
			});
		});
	}

	private static void WriteReqFilterHeadingPropertyIfPresent(XmlWriter xmlWriter, DataItemTableView tableView)
	{
		PropertySymbol propertySymbol = tableView.Properties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.RequestFilterHeading);
		PropertySymbol propertySymbol2 = tableView.Properties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.RequestFilterHeadingML);
		if (propertySymbol2 != null)
		{
			xmlWriter.WriteAttributeString("CaptionML", propertySymbol2.ValueText);
		}
		if (propertySymbol != null)
		{
			string stringValue = ((BoundLabelPropertyValue)((SourcePropertySymbol)propertySymbol).Property.PropertyValue).BoundLabel.Text.StringValue;
			xmlWriter.WriteAttributeString("Name", stringValue);
			if (propertySymbol2 == null)
			{
				xmlWriter.WriteAttributeString("CaptionML", "ENU=" + propertySymbol.ValueText);
			}
		}
	}

	private static void WriteFilterTableId(XmlWriter xmlWriter, DataItemTableView tableView)
	{
		string value = tableView.RelatedTableId.ToString(CultureInfo.InvariantCulture);
		xmlWriter.WriteAttributeString("FilterTableID", value);
	}

	private static void WriteTableViewIfPresent(XmlWriter xmlWriter, DataItemTableView tableView)
	{
		PropertySymbol propertySymbol = tableView.Properties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.DataItemTableView || x.PropertyKind == PropertyKind.SourceTableView);
		if (propertySymbol != null)
		{
			if (((BoundTableViewPropertyValue)propertySymbol.Property.PropertyValue).Sorting.IsEmpty)
			{
				xmlWriter.WriteAttributeString("Sorting", "0");
			}
			else
			{
				xmlWriter.WriteAttributeString("Sorting", "1");
			}
		}
	}

	private static void WriteRequestFilterFieldsIfPresent(XmlWriter xmlWriter, DataItemTableView tableView)
	{
		PropertySymbol propertySymbol = tableView.Properties.FirstOrDefault((PropertySymbol x) => x.PropertyKind == PropertyKind.RequestFilterFields);
		if (propertySymbol != null)
		{
			xmlWriter.WriteAttributeString(propertySymbol.MetadataName, propertySymbol.ValueText);
		}
	}

	private static void WriteExpressionsForTableViews(XmlWriter xmlWriter, ApplicationObjectTypeSymbol parentSymbol)
	{
		GetTableViews(parentSymbol).Do((DataItemTableView dataItemTableView) =>
		{
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Expression", () =>
			{
				string dataItemTableViewName = MetadataEmitterHelper.GetDataItemTableViewName(dataItemTableView.DataItemIdOrIndex);
				xmlWriter.WriteAttributeString("Name", dataItemTableViewName);
				xmlWriter.WriteAttributeString("Datatype", "Binary");
				xmlWriter.WriteAttributeString("ExpressionType", "DataItem");
				xmlWriter.WriteAttributeString("SourceExpression", dataItemTableViewName);
			});
		});
	}

	private static IEnumerable<DataItemTableView> GetTableViews(ApplicationObjectTypeSymbol parentSymbol)
	{
		return parentSymbol.Kind switch
		{
			SymbolKind.Report => ((SourceReportTypeSymbol)parentSymbol).FlattenedDataItems.Select((ReportDataItemSymbol x, int i) => new DataItemTableView(i, x.RelatedTable.Id, x.Properties)), 
			SymbolKind.XmlPort => from SourceXmlPortNodeSymbol x in ((SourceXmlPortTypeSymbol)parentSymbol).FlattenedNodes.Where((XmlPortNodeSymbol m) => m.Kind == SymbolKind.XmlPortNode)
				where x.SourceTypeKind == XmlPortSourceTypeKind.Table
				select new DataItemTableView(x.Id, ((RecordTypeSymbol)x.Type).Id, x.Properties), 
			_ => throw ExceptionUtilities.UnexpectedValue(parentSymbol.Kind), 
		};
	}

	private static string GetRequestPageParentIdName(PageTypeKind kind)
	{
		switch (kind)
		{
		case PageTypeKind.XmlPort:
			return "XMLportID";
		case PageTypeKind.ReportPreview:
		case PageTypeKind.ReportProcessingOnly:
			return "ReportID";
		default:
			throw ExceptionUtilities.UnexpectedValue(kind);
		}
	}

	private void WriteRequestPageSpecificProperties(XmlWriter xmlWriter)
	{
		string value = ContainingSymbol.Id.ToString(CultureInfo.InvariantCulture);
		ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, GetRequestPageParentIdName(RequestPage.PageType), value, emitAsAttribute: true);
		ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, "PageType", RequestPage.PageType.ToString(), emitAsAttribute: true);
		ObjectMetadataEmitHelper.WritePropertyValue(xmlWriter, "PromotedActionCategoriesML", string.Empty, emitAsAttribute: true);
	}

	private void WritePageDefinitionProperties(XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
		xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
		xmlWriter.WriteAttributeString("xmlns", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("ID", "0");
		xmlWriter.WriteAttributeString("Name", ContainingSymbol.Name);
		PropertySymbol propertySymbol = RequestPage.Properties.FirstOrDefault((PropertySymbol p) => p.PropertyKind == PropertyKind.CaptionML);
		if (propertySymbol != null)
		{
			xmlWriter.WriteAttributeString("CaptionML", propertySymbol.ValueText);
			return;
		}
		PropertySymbol propertySymbol2 = Symbol.Properties.FirstOrDefault((PropertySymbol p) => p.PropertyKind == PropertyKind.Caption);
		if (propertySymbol2 != null)
		{
			ObjectMetadataEmitHelper.WriteLabelPropertyValue(xmlWriter, propertySymbol2);
		}
	}

	private void WritePageProperties(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Properties", () =>
		{
			WriteRequestPageSpecificProperties(xmlWriter);
			PropertySymbol property = null;
			bool flag = false;
			PooledList<PropertySymbol> sourceObjectProperties = PooledList<PropertySymbol>.GetInstance();
			PooledList<PropertySymbol> instance = PooledList<PropertySymbol>.GetInstance();
			foreach (PropertySymbol property2 in RequestPage.Properties)
			{
				if (property2.PropertyKind == PropertyKind.ContextSensitiveHelpPage)
				{
					property = property2;
				}
				else if (property2.PropertyKind == PropertyKind.HelpLink)
				{
					flag = true;
					instance.Add(property2);
				}
				else if (property2.PropertyKind.IsSourceObjectAttribute())
				{
					sourceObjectProperties.Add(property2);
				}
				else if (!property2.PropertyKind.IsEitherCaptionProperty())
				{
					instance.Add(property2);
				}
			}
			if (!flag)
			{
				PageMetadataEmitHelper.WriteContextSpecificHelpUrlPropertyIfNeeded(xmlWriter, Symbol, property);
			}
			PageMetadataEmitHelper.WriteSupportedLocales(xmlWriter, Symbol);
			WriteImplicitPageProperties(xmlWriter);
			WriteProperties(xmlWriter, Symbol, instance, shouldOutputDefaultValues: true);
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "SourceObject", () =>
			{
				WriteProperties(xmlWriter, Symbol, sourceObjectProperties, shouldOutputDefaultValues: false);
			});
		});
	}

	private void WriteImplicitPageProperties(XmlWriter xmlWriter)
	{
		if (RequestPage.Triggers.Cast<SourceTriggerSymbol>().Any((SourceTriggerSymbol x) => x.TriggerInfo.Kind == TriggerTypeKind.PageOnAfterGetCurrRecord))
		{
			xmlWriter.WriteAttributeString("OnAfterGetCurrentRecordEnabled", "1");
		}
	}

	private void WriteContent(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("Content", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		Dictionary<string, string> addedSourceExpressions = new Dictionary<string, string>();
		foreach (SourceControlSymbol item in RequestPage.GetMembersOfKind<SourceControlSymbol>(SymbolKind.Control))
		{
			WriteControl(xmlWriter, item, addedSourceExpressions);
		}
		WriteRequestPageFilters(xmlWriter);
		xmlWriter.WriteEndElement();
	}

	private void WriteRequestPageFilters(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Containers", () =>
		{
			xmlWriter.WriteAttributeString("xsi", "type", null, "ControlContainerDefinition");
			xmlWriter.WriteAttributeString("ContainerType", "RequestPageFilters");
			GetTableViews(ContainingSymbol).Do((DataItemTableView dataItemTableView) =>
			{
				WriteTableFilter(xmlWriter, dataItemTableView);
			});
		});
	}

	private void WriteExpressions(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("Expressions");
		ImmutableHashSet<Symbol> implicitInDataset = InDataSetVisitor.GetAllImplicitInDatasetVariables(ContainingSymbol);
		implicitInDataset = implicitInDataset.Union(InDataSetVisitor.GetAllImplicitInDatasetVariables(RequestPage));
		foreach (VariableSymbol item in from g in Enumerable.Concat(ContainingSymbol.GlobalVariables, RequestPage.GlobalVariables)
			where g.IsInDataSet || implicitInDataset.Contains(g)
			select g)
		{
			PageMetadataEmitHelper.WriteVariableInDataSetExpression(xmlWriter, item);
		}
		WriteExpressionsForTableViews(xmlWriter, ContainingSymbol);
		HashSet<string> alreadyAddedSourceExpressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (SourceControlSymbol item2 in RequestPage.FlattenedControls.Cast<SourceControlSymbol>())
		{
			PageMetadataEmitHelper.WriteControlExpressions(xmlWriter, item2, alreadyAddedSourceExpressions);
		}
		BoundCodeExpressionPropertyValue boundPropertyValue = RequestPage.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.DataCaptionExpression);
		PageBaseMetadataEmitter<SourcePageTypeSymbol>.WriteDataCaptionExpression(xmlWriter, boundPropertyValue);
		xmlWriter.WriteEndElement();
	}
}
