using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortExporter : IDisposable
{
	private INavXmlPortAdapter adapter;

	private NavXmlPort xmlPort;

	private string namespacesDefault;

	private static void CalcFields(NavXmlPortTableNode node)
	{
		MetaXmlPortTableNode metaXmlPortTableNode = (MetaXmlPortTableNode)node.MetaData;
		if (metaXmlPortTableNode.CalcFields() != null && metaXmlPortTableNode.CalcFields().Length != 0)
		{
			node.Record.ALCalcFields(metaXmlPortTableNode.CalcFields());
		}
	}

	private static void SetLinkFields(NavXmlPortTableNode node, FilterGroupType filterGroup)
	{
		if (node.LinkTableNode != null)
		{
			int aLFilterGroup = node.Record.ALFilterGroup;
			try
			{
				node.Record.ALFilterGroup = (int)filterGroup;
				node.SetRecordDataItemLink();
			}
			finally
			{
				node.Record.ALFilterGroup = aLFilterGroup;
			}
		}
	}

	private static void SetSourceTableView(NavXmlPortTableNode node, FilterGroupType filterGroup)
	{
		node.SourceTableViewRecord.SecurityFiltering = node.Record.SecurityFiltering;
		node.Record.RecordImplementation.ResetRecordInternal();
		if (node.SourceTableViewRecord != null)
		{
			node.Record.RecordImplementation.CopyFiltersSortingMarkList(node.SourceTableViewRecord);
		}
		if (!string.IsNullOrEmpty(((MetaXmlPortTableNode)node.MetaData).SourceTableView))
		{
			int aLFilterGroup = node.Record.ALFilterGroup;
			try
			{
				node.Record.ALFilterGroup = (int)filterGroup;
				node.SetDataItemTableViewFieldFilters();
			}
			finally
			{
				node.Record.ALFilterGroup = aLFilterGroup;
			}
		}
	}

	private string GetStringFromNavRecord(NavRecord record, NCLMetaField metaField)
	{
		NavValue fieldValue = record.GetFieldValue(metaField.FieldNo);
		if (metaField.FieldNclType == NavNclType.NavDecimal)
		{
			string text = record.GetAutoFormatString(metaField.FieldNo);
			if (string.IsNullOrEmpty(text))
			{
				text = record.GetDecimalPlaces(metaField.FieldNo);
			}
			if (string.IsNullOrEmpty(text))
			{
				text = NavCurrentThread.Session.SystemCodeunitFactory.UIHelperTriggers.InvokeAutoFormatTranslate(1, NavText.Empty);
			}
			if (!string.IsNullOrEmpty(text))
			{
				if (xmlPort.Metadata.FormatEvaluate == XmlPortFormatEvaluate.XML)
				{
					int num = text.LastIndexOf("<Standard Format,", StringComparison.OrdinalIgnoreCase);
					if (num > -1)
					{
						int startIndex = num + 17;
						text = text.Remove(startIndex, 1);
						text = text.Insert(startIndex, "9");
					}
				}
				return NavFormatEvaluateHelper.Format(fieldValue, 0, text);
			}
		}
		if (xmlPort.Metadata.FormatEvaluate == XmlPortFormatEvaluate.XML)
		{
			return NavFormatEvaluateHelper.Format(fieldValue, 0, 9);
		}
		return fieldValue.ToString();
	}

	private bool TryProcessElement(NavXmlPortNode node)
	{
		try
		{
			return ProcessElement(node);
		}
		catch (NavNCLException ex) when (!(ex is NavNCLXmlPortNodeProcessException))
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string nodeName = node.MetaData.NodeName;
			_ = ex.InnerException;
			if (ex == null)
			{
			}
			throw NavNCLXmlPortNodeProcessException.Create(currentCulture, nodeName, ex);
		}
	}

	private bool ProcessElement(NavXmlPortNode node)
	{
		bool result = false;
		xmlPort.AddNodeToPath(node);
		switch (node.MetaData.SourceType)
		{
		case XmlPortNodeSourceType.Text:
			if (!(node is NavXmlPortTextNode node4))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			result = ProcessTextElement(node4);
			break;
		case XmlPortNodeSourceType.Table:
			if (!(node is NavXmlPortTableNode node3))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			result = ProcessTableElement(node3);
			break;
		case XmlPortNodeSourceType.Field:
			if (!(node is NavXmlPortFieldNode node2))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			result = ProcessFieldElement(node2);
			break;
		}
		xmlPort.RemoveNodeFromPath();
		return result;
	}

	private bool ProcessTextElement(NavXmlPortTextNode node)
	{
		switch (node.FireOnBeforePassVariable())
		{
		case NavControlStatement.Skip:
			return true;
		case NavControlStatement.Break:
			node.BreakNode = true;
			return true;
		case NavControlStatement.BreakUnbound:
			if (node.MetaData.Unbound)
			{
				node.BreakNode = true;
				return false;
			}
			break;
		case NavControlStatement.Quit:
			return false;
		}
		if (!(node.MetaData is MetaXmlPortTextNode metaXmlPortTextNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		WriteStartElement(metaXmlPortTextNode.NodePrefix, metaXmlPortTextNode.NodeName, node.MetaData.Parent == null, XmlPortNodeSourceType.Text);
		try
		{
			if (!LoopAttributeChildren(node))
			{
				return false;
			}
			if (!LoopElementChildren(node))
			{
				return false;
			}
			string node2 = node.MetaData.Id.ToString("B").ToUpper(CultureInfo.InvariantCulture);
			adapter.WriteString(string.Format(CultureInfo.InvariantCulture, "{0}", xmlPort.GetTextVariable(node2)), node);
			return true;
		}
		finally
		{
			adapter.WriteEndElement(XmlPortNodeSourceType.Text);
		}
	}

	private void WriteStartElement(string prefix, string elementName, bool isRootElement, XmlPortNodeSourceType sourceType)
	{
		if (xmlPort.Metadata.UseDefaultNamespace && xmlPort.Metadata.DefaultNamespace != null)
		{
			adapter.WriteStartElement(prefix, elementName, xmlPort.Metadata.DefaultNamespace, sourceType);
		}
		else if (isRootElement && !string.IsNullOrEmpty(namespacesDefault))
		{
			adapter.WriteStartElement(prefix, elementName, namespacesDefault, sourceType);
		}
		else
		{
			adapter.WriteStartElement(prefix, elementName, sourceType);
		}
		if (!isRootElement)
		{
			return;
		}
		if (xmlPort.Metadata.Namespaces != null)
		{
			foreach (KeyValuePair<string, string> @namespace in xmlPort.Metadata.Namespaces)
			{
				if (!string.IsNullOrWhiteSpace(@namespace.Key))
				{
					adapter.WriteNamespaceString(@namespace.Key, @namespace.Value);
				}
			}
		}
		if (xmlPort.Metadata.InlineSchema)
		{
			adapter.WriteRaw(xmlPort.Metadata.InlineSchemaAsXmlString());
		}
	}

	private bool ProcessTableElement(NavXmlPortTableNode node)
	{
		NavCurrentThread.Session.ThrowSessionTerminatedExceptionIfStopping();
		SetSourceTableView(node, FilterGroupType.TableViewFilters);
		SetLinkFields(node, FilterGroupType.SubFormLinkFilters);
		switch (NavXmlPortTableNode.FireExportTrigger(node.OnPreXmlItem))
		{
		case NavControlStatement.Break:
			node.BreakNode = true;
			return true;
		case NavControlStatement.Quit:
			return false;
		default:
			if (node.Record.ALFind(DataError.TrapError, "-"))
			{
				do
				{
					CalcFields(node);
					switch (NavXmlPortTableNode.FireExportTrigger(node.OnAfterGetRecord))
					{
					case NavControlStatement.Break:
						return true;
					case NavControlStatement.Quit:
						return false;
					case NavControlStatement.Skip:
						continue;
					}
					WriteStartElement(node.MetaData.NodePrefix, node.MetaData.NodeName, node.MetaData.Parent == null, XmlPortNodeSourceType.Table);
					try
					{
						if (!LoopAttributeChildren(node))
						{
							return false;
						}
						if (!LoopElementChildren(node))
						{
							return false;
						}
					}
					finally
					{
						adapter.WriteEndElement(XmlPortNodeSourceType.Table);
					}
					adapter.WriteNewLine();
				}
				while (node.Record.ALNext() != 0 && node.MetaData.MaxOccurs == XmlPortNodeMaxOccurs.Unbounded);
				adapter.WriteTableSeparator();
			}
			else if (node.MetaData.MinOccurs == XmlPortNodeMinOccurs.Once && xmlPort.Metadata.Format == XmlPortNodeFormat.Xml)
			{
				WriteStartElement(node.MetaData.NodePrefix, node.MetaData.NodeName, node.MetaData.Parent == null, XmlPortNodeSourceType.Table);
				try
				{
					if (!LoopAttributeChildren(node))
					{
						return false;
					}
					if (!LoopElementChildren(node))
					{
						return false;
					}
				}
				finally
				{
					adapter.WriteEndElement(XmlPortNodeSourceType.Table);
				}
				adapter.WriteNewLine();
				adapter.WriteTableSeparator();
			}
			else
			{
				adapter.WriteTableSeparator();
			}
			return true;
		}
	}

	private bool ProcessFieldElement(NavXmlPortFieldNode node)
	{
		switch (node.FireOnBeforePassField())
		{
		case NavControlStatement.Break:
			node.BreakNode = true;
			return true;
		case NavControlStatement.BreakUnbound:
			if (node.MetaData.Unbound)
			{
				node.BreakNode = true;
				return false;
			}
			break;
		case NavControlStatement.Skip:
			return true;
		case NavControlStatement.Quit:
			return false;
		}
		WriteStartElement(node.MetaData.NodePrefix, node.MetaData.NodeName, node.MetaData.Parent == null, XmlPortNodeSourceType.Field);
		try
		{
			if (!LoopAttributeChildren(node))
			{
				return false;
			}
			if (node.SourceFieldTableNode == null)
			{
				throw new NavNCLXmlPortMetadataException();
			}
			NCLMetaField fieldByName = node.SourceFieldTableNode.Record.MetaTable.GetFieldByName(node.SourceFieldName);
			if (node.CalcField)
			{
				node.SourceFieldTableNode.Record.ALCalcFields(fieldByName.FieldNo);
			}
			adapter.WriteString(GetStringFromNavRecord(node.SourceFieldTableNode.Record, fieldByName), node);
			if (!LoopElementChildren(node))
			{
				return false;
			}
			return true;
		}
		finally
		{
			adapter.WriteEndElement(XmlPortNodeSourceType.Field);
		}
	}

	private bool TryProcessAttribute(NavXmlPortNode node)
	{
		xmlPort.AddNodeToPath(node);
		bool result;
		try
		{
			result = ProcessAttribute(node);
		}
		catch (NavNCLException ex) when (!(ex is NavNCLXmlPortNodeProcessException))
		{
			CultureInfo currentCulture = CultureInfo.CurrentCulture;
			string nodeName = node.MetaData.NodeName;
			_ = ex.InnerException;
			if (ex == null)
			{
			}
			throw NavNCLXmlPortNodeProcessException.Create(currentCulture, nodeName, ex);
		}
		xmlPort.RemoveNodeFromPath();
		return result;
	}

	private bool ProcessAttribute(NavXmlPortNode node)
	{
		_ = string.Empty;
		string value;
		switch (node.MetaData.SourceType)
		{
		case XmlPortNodeSourceType.Text:
		{
			NavXmlPortTextNode navXmlPortTextNode = node as NavXmlPortTextNode;
			if (!(node.MetaData is MetaXmlPortTextNode metaXmlPortTextNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			if (navXmlPortTextNode == null)
			{
				throw NavNCLXmlPortNodeNotFoundException.Create(CultureInfo.CurrentCulture, metaXmlPortTextNode.NodeName, node.MetaData.Id);
			}
			switch (navXmlPortTextNode.FireOnBeforePassVariable())
			{
			case NavControlStatement.Skip:
				return true;
			case NavControlStatement.Break:
				node.BreakNode = true;
				return true;
			case NavControlStatement.Quit:
				return false;
			}
			string node2 = node.MetaData.Id.ToString("B").ToUpper(CultureInfo.InvariantCulture);
			value = string.Format(CultureInfo.InvariantCulture, "{0}", xmlPort.GetTextVariable(node2));
			break;
		}
		case XmlPortNodeSourceType.Table:
			throw new NavNCLNotSupportedTypeException(Lang._00095_50104);
		case XmlPortNodeSourceType.Field:
		{
			if (!(node is NavXmlPortFieldNode navXmlPortFieldNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			switch (navXmlPortFieldNode.FireOnBeforePassField())
			{
			case NavControlStatement.Break:
				node.BreakNode = true;
				return true;
			case NavControlStatement.Skip:
				return true;
			case NavControlStatement.Quit:
				return false;
			}
			if (navXmlPortFieldNode.SourceFieldTableNode == null)
			{
				throw new NavNCLXmlPortMetadataException();
			}
			NCLMetaField fieldByName = navXmlPortFieldNode.SourceFieldTableNode.Record.MetaTable.GetFieldByName(navXmlPortFieldNode.SourceFieldName);
			if (navXmlPortFieldNode.CalcField)
			{
				navXmlPortFieldNode.SourceFieldTableNode.Record.ALCalcFields(fieldByName.FieldNo);
			}
			value = GetStringFromNavRecord(navXmlPortFieldNode.SourceFieldTableNode.Record, fieldByName);
			break;
		}
		default:
			throw new NavNCLXmlPortMetadataException();
		}
		adapter.WriteAttributeString(node, value);
		return true;
	}

	private bool LoopElementChildren(NavXmlPortNode node)
	{
		foreach (NavXmlPortNode elementChild in node.ElementChildren)
		{
			do
			{
				if (!TryProcessElement(elementChild))
				{
					if (elementChild.MetaData.Unbound && elementChild.BreakNode)
					{
						elementChild.BreakNode = false;
						return true;
					}
					return false;
				}
			}
			while (elementChild.MetaData.Unbound);
			if (elementChild.BreakNode)
			{
				elementChild.BreakNode = false;
				break;
			}
		}
		return true;
	}

	private bool LoopAttributeChildren(NavXmlPortNode node)
	{
		foreach (NavXmlPortNode attributeChild in node.AttributeChildren)
		{
			if (!TryProcessAttribute(attributeChild))
			{
				return false;
			}
			if (attributeChild.BreakNode)
			{
				attributeChild.BreakNode = false;
				break;
			}
		}
		return true;
	}

	private static void AssertElementNode(NavXmlPortNode node)
	{
		if (node.MetaData.NodeType != XmlPortNodeType.Element)
		{
			throw NavNCLXmlPortInvalidNodeTypeException.CreateUnexpectedNodeType(CultureInfo.CurrentCulture, node.MetaData.NodeName);
		}
	}

	internal NavXmlPortExporter(NavXmlPort xmlPortInstance)
	{
		if (xmlPortInstance.Metadata.Format == XmlPortNodeFormat.VariableText)
		{
			adapter = new NavXmlPortVariableTextAdapter(xmlPortInstance);
		}
		else
		{
			adapter = new NavXmlPortFixedTextAdapter(xmlPortInstance);
		}
		xmlPort = xmlPortInstance;
	}

	internal NavXmlPortExporter(NavXmlPort xmlPortInstance, XmlWriterSettings xmlWriterSettings)
	{
		xmlPort = xmlPortInstance;
		adapter = new NavXmlPortXmlAdapter(xmlPortInstance, xmlWriterSettings);
		namespacesDefault = null;
		if (xmlPort.Metadata.Namespaces == null)
		{
			return;
		}
		foreach (KeyValuePair<string, string> @namespace in xmlPort.Metadata.Namespaces)
		{
			if (string.IsNullOrWhiteSpace(@namespace.Key))
			{
				namespacesDefault = @namespace.Value;
				break;
			}
		}
	}

	internal bool Export()
	{
		try
		{
			xmlPort.OnPreXMLport();
			if (xmlPort.Nodes.Count == 0)
			{
				return true;
			}
			AssertElementNode(xmlPort.Nodes[0]);
			return TryProcessElement(xmlPort.Nodes[0]);
		}
		catch (NavControlException ex) when (ex.ControlStatement == NavControlStatement.Quit)
		{
			return false;
		}
		catch (XmlSchemaException ex2)
		{
			throw new NavNCLXmlPortImportException(ex2.Message, ex2);
		}
		catch (XmlException ex3)
		{
			throw new NavNCLXmlPortImportException(ex3.Message, ex3);
		}
		finally
		{
			adapter.Flush();
			adapter.Close();
			if (xmlPort.Destination.InternalStreamCheckedForWrite != null)
			{
				xmlPort.Destination.Position = xmlPort.Destination.Length;
			}
		}
	}

	public void Dispose()
	{
		if (adapter is NavXmlPortTextAdapter navXmlPortTextAdapter)
		{
			navXmlPortTextAdapter.Dispose();
		}
	}
}
