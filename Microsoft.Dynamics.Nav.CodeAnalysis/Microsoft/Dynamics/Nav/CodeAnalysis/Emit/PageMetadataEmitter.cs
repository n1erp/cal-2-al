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

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class PageMetadataEmitter : PageBaseMetadataEmitter<SourcePageTypeSymbol>
{
	internal PageMetadataEmitter(SourcePageTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("PageDefinition", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			WritePageDefinitionProperties(xmlWriter);
			WritePageProperties(xmlWriter);
			WriteContent(xmlWriter);
			ImmutableArray<ActionSymbol> actions = Symbol.GetMembers().WhereAsCastArray<ActionSymbol, Symbol>((Symbol m) => m.Kind == SymbolKind.Action);
			WriteActions(xmlWriter, actions);
			if (Symbol.PageType.IsViewSupported())
			{
				ImmutableArray<ViewSymbol> immutableArray = Symbol.GetMembers().WhereAsCastArray<ViewSymbol, Symbol>((Symbol m) => m.Kind == SymbolKind.View);
				WriteViewActions(xmlWriter, immutableArray);
				WriteViewDefinitions(xmlWriter, immutableArray);
			}
			WriteExpressions(xmlWriter);
			PageMetadataEmitHelper.WriteTriggers(xmlWriter, Symbol.Triggers);
			xmlWriter.WriteEndElement();
		});
	}

	private void WritePageDefinitionProperties(XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
		xmlWriter.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
		xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
		xmlWriter.WriteAttributeString("Name", Symbol.Name);
		PropertySymbol propertySymbol = Symbol.Properties.FirstOrDefault((PropertySymbol p) => p.PropertyKind == PropertyKind.CaptionML);
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
			xmlWriter.WriteAttributeString("ControlGUID", MetadataEmitterHelper.GenerateMemberId(0, Symbol.Id, Symbol.Kind));
			PropertySymbol property = null;
			bool flag = false;
			PooledList<PropertySymbol> sourceObjectProperties = PooledList<PropertySymbol>.GetInstance();
			PooledList<PropertySymbol> instance = PooledList<PropertySymbol>.GetInstance();
			foreach (PropertySymbol property2 in Symbol.Properties)
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
			WriteExtensionSpecificMetadataAttributes(xmlWriter);
			WriteProperties(xmlWriter, Symbol, instance, shouldOutputDefaultValues: true);
			ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "SourceObject", () =>
			{
				WriteProperties(xmlWriter, Symbol, sourceObjectProperties, shouldOutputDefaultValues: false);
			});
		});
	}

	private void WriteImplicitPageProperties(XmlWriter xmlWriter)
	{
		if (Symbol.Triggers.Cast<SourceTriggerSymbol>().Any((SourceTriggerSymbol x) => x.TriggerInfo.Kind == TriggerTypeKind.PageOnAfterGetCurrRecord))
		{
			xmlWriter.WriteAttributeString("OnAfterGetCurrentRecordEnabled", "1");
		}
	}

	private void WriteContent(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("Content", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
		Dictionary<string, string> addedSourceExpressions = new Dictionary<string, string>();
		foreach (SourceControlSymbol item in (from m in Symbol.GetMembers()
			where m.Kind == SymbolKind.Control && !m.IsImplicitlyDeclared
			select m).Cast<SourceControlSymbol>())
		{
			WriteControl(xmlWriter, item, addedSourceExpressions);
		}
		xmlWriter.WriteEndElement();
	}

	private void WriteExpressions(XmlWriter xmlWriter)
	{
		xmlWriter.WriteStartElement("Expressions");
		ImmutableHashSet<Symbol> implicitInDataset = InDataSetVisitor.GetAllImplicitInDatasetVariables(Symbol);
		foreach (VariableSymbol item in Symbol.GlobalVariables.Where((VariableSymbol g) => g.IsInDataSet || implicitInDataset.Contains(g)))
		{
			PageMetadataEmitHelper.WriteVariableInDataSetExpression(xmlWriter, item);
		}
		HashSet<string> alreadyAddedSourceExpressions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (SourceControlSymbol item2 in Symbol.FlattenedControls.Where((ControlSymbol c) => !c.IsImplicitlyDeclared).Cast<SourceControlSymbol>())
		{
			PageMetadataEmitHelper.WriteControlExpressions(xmlWriter, item2, alreadyAddedSourceExpressions);
		}
		BoundCodeExpressionPropertyValue boundPropertyValue = Symbol.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.DataCaptionExpression);
		PageBaseMetadataEmitter<PageTypeSymbol>.WriteDataCaptionExpression(xmlWriter, boundPropertyValue);
		xmlWriter.WriteEndElement();
	}
}
