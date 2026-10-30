using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class XmlPortMetadataEmitter : ObjectMetadataEmitter<XmlPortTypeSymbol>
{
	internal new SourceXmlPortTypeSymbol Symbol { get; }

	internal XmlPortMetadataEmitter(SourceXmlPortTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base((XmlPortTypeSymbol)symbol, moduleBuilder)
	{
		Symbol = symbol;
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
		default:
			return false;
		}
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("XmlPort");
			WriteXmlPortBaseProperties(xmlWriter);
			WriteDefaultNamespace(xmlWriter);
			WriteXmlPortProperties(xmlWriter);
			WriteDataItems(xmlWriter, Symbol.GetMembersOfKind<XmlPortNodeSymbol>(SymbolKind.XmlPortNode), 0);
			RequestPageMetadataEmitter.Write(xmlWriter, Symbol.RequestPage, Symbol, ModuleBuilder, diagnostics);
			xmlWriter.WriteEndElement();
		});
	}

	private void WriteDefaultNamespace(XmlWriter xmlWriter)
	{
		PropertySymbol property = Symbol.GetProperty(PropertyKind.DefaultNamespace);
		if (property != null)
		{
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, property.MetadataName, property.ValueText);
			return;
		}
		PropertyTypeInfo propertyTypeInfo = Symbol.GetPropertyTypeInfo("DefaultNamespace");
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, propertyTypeInfo.MetadataName, "urn:microsoft-dynamics-nav/xmlports/x" + Symbol.Id);
	}

	private void WriteXmlPortBaseProperties(XmlWriter xmlWriter)
	{
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "ID", () =>
		{
			xmlWriter.WriteAttributeString("Name", Symbol.Name);
			xmlWriter.WriteValue(Symbol.Id.ToString(CultureInfo.InvariantCulture));
		});
		ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "Name", Symbol.Name);
		WriteNamespacesProperty(xmlWriter, Symbol.GetBoundPropertyValue<BoundListOfConstantValuePairsPropertyValue>(PropertyKind.Namespaces));
	}

	private static void WriteNamespacesProperty(XmlWriter xmlWriter, BoundListOfConstantValuePairsPropertyValue namespacesProperty)
	{
		if (namespacesProperty == null)
		{
			return;
		}
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Namespaces", () =>
		{
			foreach (BoundPairOfConstantValues item in namespacesProperty.Value)
			{
				BoundPairOfConstantValues boundPairOfConstantValues = item;
				string prefix = boundPairOfConstantValues.First.StringValue.UnquoteIdentifier();
				string uri = boundPairOfConstantValues.Second.StringValue.UnquoteLiteral();
				ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Namespace", () =>
				{
					ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "NamespacePrefix", prefix);
					ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "NamespaceURI", uri);
				});
			}
		});
	}

	private void WriteXmlPortProperties(XmlWriter xmlWriter)
	{
		WriteProperties(xmlWriter, Symbol, Symbol.Properties, shouldOutputDefaultValues: true);
	}

	private void WriteDataItems(XmlWriter xmlWriter, IEnumerable<XmlPortNodeSymbol> xmlPortNodes, int indentation)
	{
		foreach (SourceXmlPortNodeSymbol xmlPortNode in xmlPortNodes)
		{
			WriteDataItem(xmlWriter, xmlPortNode, indentation);
		}
	}

	private void WriteDataItem(XmlWriter xmlWriter, SourceXmlPortNodeSymbol node, int indentation)
	{
		ObjectMetadataEmitHelper.WriteStartEndElement(xmlWriter, "Node", () =>
		{
			string nodeValue = node.GetProperty(PropertyKind.XmlName)?.ValueText ?? node.Name;
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "NodeName", nodeValue);
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "NodeType", node.NodeXmlKind.ToString());
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "SourceType", node.SourceTypeKind.ToString());
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "Indentation", indentation.ToString(CultureInfo.InvariantCulture));
			ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "ID", XmlPortCodeBuilder.GeneratedGuid(node));
			switch (node.SourceTypeKind)
			{
			case XmlPortSourceTypeKind.Field:
			{
				string nodeValue2 = XmlPortCodeBuilder.GeneratedGuid((SourceXmlPortNodeSymbol)node.ContainingSymbol);
				MemberAccessExpressionSyntax memberAccessExpressionSyntax = (MemberAccessExpressionSyntax)((XmlPortFieldNodeSyntax)node.Syntax).SourceField;
				string text = memberAccessExpressionSyntax.Expression.GetIdentifierOrLiteralValue() + "::" + memberAccessExpressionSyntax.Name.GetIdentifierOrLiteralValue();
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "SourceField", text.ToUpperInvariant());
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "ParentID", nodeValue2);
				string metadataName = ((FieldSymbol)node.Type).Type.MetadataName;
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "DataType", metadataName);
				break;
			}
			case XmlPortSourceTypeKind.Table:
			{
				RecordTypeSymbol recordTypeSymbol = (RecordTypeSymbol)node.Type;
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "SourceTable", recordTypeSymbol.Id.ToString(CultureInfo.InvariantCulture));
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "ParentID", Guid.Empty.ToString("B"));
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "VariableName", node.Name.ToUpperInvariant());
				PropertySymbol property = node.GetProperty(PropertyKind.LinkTable);
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "LinkTable", property?.ValueText.ToUpperInvariant() ?? string.Empty);
				break;
			}
			case XmlPortSourceTypeKind.Text:
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "ParentID", Guid.Empty.ToString("B"));
				ObjectMetadataEmitHelper.WriteNodeWithValue(xmlWriter, "VariableName", node.Name.ToUpperInvariant());
				break;
			}
			WriteProperties(xmlWriter, node, node.Properties, shouldOutputDefaultValues: true);
		});
		WriteDataItems(xmlWriter, node.Nodes, indentation + 1);
	}
}
