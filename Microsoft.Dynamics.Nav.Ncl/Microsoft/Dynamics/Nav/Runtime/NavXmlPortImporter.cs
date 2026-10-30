using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavXmlPortImporter
{
	internal delegate NavControlStatement XmlPortNodeTriggerDelegate();

	internal delegate NavControlStatement XmlPortNodeTableTriggerDelegate(NavTrigger tableTrigger);

	private const string XmlNameSpace = "xmlns";

	private const string SchemaTagName = "'schema'";

	private const string SchemaNameSpace = "http://www.w3.org/2001/XMLSchema";

	private NavXmlPort xmlPort;

	private XmlReader reader;

	private int previousNodeCounter;

	private bool compareLocalName;

	private readonly Stack<string> tagNames;

	private string defaultNamespace;

	private List<string> validNamespaceList;

	private bool ReadNextNode()
	{
		if (previousNodeCounter > 0)
		{
			previousNodeCounter--;
			return true;
		}
		bool flag = false;
		while (!flag)
		{
			if (reader.NodeType == XmlNodeType.EndElement && reader.Depth == 0)
			{
				return false;
			}
			if (reader.EOF || !reader.Read())
			{
				return false;
			}
			switch (reader.NodeType)
			{
			case XmlNodeType.Element:
				if (VerifyNamespace())
				{
					flag = true;
					if (!reader.IsEmptyElement)
					{
						tagNames.Push(reader.Name);
					}
				}
				else
				{
					MoveToNextElement();
				}
				break;
			case XmlNodeType.EndElement:
			{
				string strA = ((tagNames.Count > 0) ? tagNames.Pop() : string.Empty);
				if (string.Compare(strA, reader.Name, StringComparison.Ordinal) != 0)
				{
					throw NavNCLXmlPortUnexpectedElementException.Create(CultureInfo.CurrentCulture, reader.Name);
				}
				flag = true;
				break;
			}
			default:
				flag = true;
				break;
			case XmlNodeType.Comment:
			case XmlNodeType.Whitespace:
			case XmlNodeType.SignificantWhitespace:
				break;
			}
		}
		return true;
	}

	private bool NodeNameEqual(NavXmlPortNode node)
	{
		if (compareLocalName)
		{
			return reader.LocalName == node.MetaData.NodeName;
		}
		if (reader.LocalName == node.MetaData.NodeName)
		{
			return reader.Prefix == node.MetaData.NodePrefix;
		}
		return false;
	}

	internal static NavControlStatement XmlPortNodeTriggerSwitch(NavXmlPortNode node, XmlPortNodeTableTriggerDelegate triggerDelegate, NavTrigger trigger)
	{
		NavControlStatement navControlStatement = triggerDelegate(trigger);
		UpdateNodeFromControlStatement(node, navControlStatement);
		return navControlStatement;
	}

	internal static NavControlStatement XmlPortNodeTriggerSwitch(NavXmlPortNode node, XmlPortNodeTriggerDelegate triggerDelegate)
	{
		NavControlStatement navControlStatement = triggerDelegate();
		UpdateNodeFromControlStatement(node, navControlStatement);
		return navControlStatement;
	}

	private static void UpdateNodeFromControlStatement(NavXmlPortNode node, NavControlStatement controlStatement)
	{
		switch (controlStatement)
		{
		case NavControlStatement.Break:
			node.RecordInitialized = false;
			node.BreakNode = true;
			break;
		case NavControlStatement.Skip:
			node.RecordInitialized = false;
			node.SkipNode = true;
			break;
		case NavControlStatement.None:
		case NavControlStatement.BreakUnbound:
			break;
		}
	}

	private static void CheckMaxOccurs(NavXmlPortNode node)
	{
		if (node.MetaData.MaxOccurs == XmlPortNodeMaxOccurs.Once && node.Occurs > 1)
		{
			throw NavNCLXmlPortDuplicateElementException.Create(CultureInfo.CurrentCulture, node.MetaData.NodeName);
		}
	}

	private static void CheckMinOccurs(int index, NavXmlPortNode node)
	{
		while (index < node.ElementChildren.Count)
		{
			if (node.ElementChildren[index].MetaData.MinOccurs == XmlPortNodeMinOccurs.Once)
			{
				throw NavNCLXmlPortMetadataException.CreateOccurrenceExceeded(CultureInfo.CurrentCulture, node.ElementChildren[index].MetaData.NodeName, node.ElementChildren[index].MetaData.MinOccurs.ToString(), string.Empty);
			}
			index++;
		}
	}

	private bool CheckBreak(NavXmlPortNode node)
	{
		bool flag;
		for (flag = NavigatorMoveToNext(); flag && (node.MetaData.SourceType != XmlPortNodeSourceType.Table || NodeNameEqual(node)); flag = NavigatorMoveToNext())
		{
		}
		node.BreakNode = false;
		return flag;
	}

	private static void InitRecord(NavXmlPortNode node)
	{
		if (node.MetaData.SourceType == XmlPortNodeSourceType.Table && !node.BreakNode && !node.SkipNode)
		{
			if (!(node is NavXmlPortTableNode navXmlPortTableNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			navXmlPortTableNode.Initialize();
			XmlPortNodeTriggerSwitch(navXmlPortTableNode, NavXmlPortTableNode.FireImportTrigger, navXmlPortTableNode.OnAfterInitRecord);
		}
	}

	private static void ImportRecord(NavXmlPortNode node)
	{
		if (!node.BreakNode && !node.SkipNode)
		{
			node.Occurs++;
		}
		if (node.MetaData.SourceType != XmlPortNodeSourceType.Table)
		{
			return;
		}
		NavCurrentThread.Session.ThrowSessionTerminatedExceptionIfStopping();
		if (node.BreakNode || node.SkipNode)
		{
			return;
		}
		if (!(node is NavXmlPortTableNode navXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		try
		{
			if (navXmlPortTableNode.LinkRecordInserted)
			{
				navXmlPortTableNode.WriteRecord(alwaysModify: true);
				navXmlPortTableNode.LinkRecordInserted = false;
			}
			else
			{
				navXmlPortTableNode.WriteRecord(alwaysModify: false);
			}
		}
		finally
		{
			navXmlPortTableNode.RecordInitialized = false;
			navXmlPortTableNode.Record.Clear();
		}
	}

	private void AssignFieldValue(NavXmlPortNode node, string value, bool assignEmpty)
	{
		if (node.MetaData.SourceType != XmlPortNodeSourceType.Field || (node.NodeTableParent != null && node.NodeTableParent.BreakNode) || (node.NodeTableParent != null && node.NodeTableParent.SkipNode))
		{
			return;
		}
		if (!(node is NavXmlPortFieldNode { SourceFieldTableNode: var sourceFieldTableNode } navXmlPortFieldNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		NCLMetaField fieldByName = sourceFieldTableNode.Record.MetaTable.GetFieldByName(navXmlPortFieldNode.SourceFieldName);
		if (fieldByName.FieldClass != FieldClass.Normal)
		{
			return;
		}
		NavValue navValueFromString = GetNavValueFromString(fieldByName, value, assignEmpty);
		if (navValueFromString == null)
		{
			return;
		}
		if (!(sourceFieldTableNode.MetaData is MetaXmlPortTableNode metaXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (!string.IsNullOrEmpty(metaXmlPortTableNode.LinkTable) && !string.IsNullOrEmpty(metaXmlPortTableNode.LinkFields))
		{
			string[] array = metaXmlPortTableNode.LinkFields.Split(new char[1] { ',' }, StringSplitOptions.None);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(new char[1] { '=' }, StringSplitOptions.None);
				int num = int.Parse(array3[0].Substring(5, array3[0].Length - 5), CultureInfo.InvariantCulture);
				if (num == fieldByName.FieldNo)
				{
					int fieldNo = int.Parse(array3[1].Substring(11, array3[1].Length - 12), CultureInfo.InvariantCulture);
					NavValue fieldValue = sourceFieldTableNode.LinkTableNode.Record.GetFieldValue(fieldNo);
					if (!fieldValue.Equals(navValueFromString))
					{
						throw NavNCLXmlPortDataException.Create(CultureInfo.CurrentCulture, sourceFieldTableNode.MetaData.NodeName);
					}
				}
			}
		}
		sourceFieldTableNode.AssignField(fieldByName, navValueFromString);
		XmlPortNodeTriggerSwitch(navXmlPortFieldNode, navXmlPortFieldNode.FireOnAfterAssignField);
		MetaXmlPortFieldNode metaXmlPortFieldNode = navXmlPortFieldNode.MetaData as MetaXmlPortFieldNode;
		if (!metaXmlPortTableNode.Temporary && ((metaXmlPortFieldNode.FieldValidate == XmlPortFieldNodeFieldValidate.Undefined && xmlPort.Metadata.DefaultFieldsValidation) || metaXmlPortFieldNode.FieldValidate == XmlPortFieldNodeFieldValidate.Yes))
		{
			sourceFieldTableNode.Record.Validate(fieldByName, null, null);
		}
	}

	private void AssignTextVariable(NavXmlPortNode node, string elementValue, bool assignEmpty)
	{
		if (node.MetaData.SourceType != XmlPortNodeSourceType.Text)
		{
			return;
		}
		if (!(node is NavXmlPortTextNode navXmlPortTextNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (reader.NodeType != XmlNodeType.Text && reader.NodeType != XmlNodeType.CDATA && !((reader.NodeType == XmlNodeType.EndElement) & assignEmpty) && node.MetaData.NodeType != XmlPortNodeType.Attribute)
		{
			return;
		}
		string node2 = navXmlPortTextNode.MetaData.Id.ToString("B").ToUpper(CultureInfo.InvariantCulture);
		string text = FormatXmlValue(elementValue);
		if (text.Length > 2048)
		{
			if (!(navXmlPortTextNode.MetaData is MetaXmlPortTextNode metaXmlPortTextNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			if (metaXmlPortTextNode.TextType == XmlPortNodeTextType.Text)
			{
				throw NavNCLXmlPortTypeSizeExceededException.Create(CultureInfo.CurrentCulture, text, text.Length, metaXmlPortTextNode.TextType.ToString());
			}
		}
		xmlPort.SetTextVariable(node2, text);
	}

	private static void AfterAssignTextVariableTrigger(NavXmlPortNode node)
	{
		if (node.MetaData.SourceType == XmlPortNodeSourceType.Text && (node.NodeTableParent == null || !node.NodeTableParent.BreakNode) && (node.NodeTableParent == null || !node.NodeTableParent.SkipNode))
		{
			if (!(node is NavXmlPortTextNode navXmlPortTextNode))
			{
				throw new NavNCLXmlPortMetadataException();
			}
			XmlPortNodeTriggerSwitch(navXmlPortTextNode, navXmlPortTextNode.FireOnAfterAssignVariable);
		}
	}

	private static void CheckLinkTableForceInsert(NavXmlPortNode node)
	{
		if (node.MetaData.SourceType != XmlPortNodeSourceType.Table)
		{
			return;
		}
		if (!(node is NavXmlPortTableNode navXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (!(navXmlPortTableNode.MetaData is MetaXmlPortTableNode metaXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (metaXmlPortTableNode.LinkTableForceInsert && !string.IsNullOrEmpty(metaXmlPortTableNode.LinkTable) && navXmlPortTableNode.LinkTableNode != null && !navXmlPortTableNode.LinkTableNode.LinkRecordInserted && !navXmlPortTableNode.LinkTableNode.SkipNode)
		{
			if (navXmlPortTableNode.LinkTableNode.WriteRecord(alwaysModify: false))
			{
				navXmlPortTableNode.LinkTableNode.LinkRecordInserted = true;
			}
			InitRecord(navXmlPortTableNode);
		}
	}

	private static void CheckLinkTableAndFields(NavXmlPortNode node)
	{
		if (node.MetaData.SourceType != XmlPortNodeSourceType.Table)
		{
			return;
		}
		if (!(node is NavXmlPortTableNode navXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (!(navXmlPortTableNode.MetaData is MetaXmlPortTableNode metaXmlPortTableNode))
		{
			throw new NavNCLXmlPortMetadataException();
		}
		if (!string.IsNullOrEmpty(metaXmlPortTableNode.LinkTable) && !string.IsNullOrEmpty(metaXmlPortTableNode.LinkFields))
		{
			string[] array = metaXmlPortTableNode.LinkFields.Split(new char[1] { ',' }, StringSplitOptions.None);
			string[] array2 = array;
			foreach (string text in array2)
			{
				string[] array3 = text.Split(new char[1] { '=' }, StringSplitOptions.None);
				int fieldNo = int.Parse(array3[0].Substring(5, array3[0].Length - 5), CultureInfo.InvariantCulture);
				int fieldNo2 = int.Parse(array3[1].Substring(11, array3[1].Length - 12), CultureInfo.InvariantCulture);
				NavValue fieldValue = navXmlPortTableNode.LinkTableNode.Record.GetFieldValue(fieldNo2);
				navXmlPortTableNode.AssignField(navXmlPortTableNode.Record.GetNCLMetaTable().GetFieldByNo(fieldNo), fieldValue);
			}
		}
	}

	private string FormatXmlValue(string value)
	{
		if (xmlPort.Metadata.PreserveWhiteSpace)
		{
			return value;
		}
		return RemoveMultipleWhiteSpaces(value);
	}

	private static string RemoveMultipleWhiteSpaces(string value)
	{
		StringBuilder stringBuilder = new StringBuilder(value.Length);
		bool flag = true;
		bool flag2 = false;
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (char.IsWhiteSpace(c))
			{
				flag2 = !flag;
				continue;
			}
			flag = false;
			if (flag2)
			{
				stringBuilder.Append(' ');
				flag2 = false;
			}
			stringBuilder.Append(c);
		}
		return stringBuilder.ToString();
	}

	private NavValue GetNavValueFromString(NCLMetaField metaField, string value, bool assignEmpty)
	{
		value = FormatXmlValue(value);
		if (string.IsNullOrEmpty(value) && (value == null || !assignEmpty))
		{
			return null;
		}
		int num = ((xmlPort.Metadata.FormatEvaluate == XmlPortFormatEvaluate.XML) ? 9 : 0);
		switch (metaField.FieldNavType)
		{
		case NavType.Date:
			if (num == 9 && value.Equals("0001-01-01"))
			{
				return NavDate.Default;
			}
			break;
		case NavType.Integer:
			if (string.IsNullOrEmpty(value))
			{
				return NavInteger.Create(0);
			}
			break;
		case NavType.BigInteger:
			if (string.IsNullOrEmpty(value))
			{
				return NavBigInteger.Create(0L);
			}
			break;
		case NavType.Boolean:
			if (string.IsNullOrEmpty(value))
			{
				return NavBoolean.Create(value: false);
			}
			break;
		case NavType.Decimal:
			if (string.IsNullOrEmpty(value))
			{
				return NavDecimal.Create(0);
			}
			break;
		case NavType.Option:
			if (string.IsNullOrEmpty(value))
			{
				return NavOption.Create(NCLOptionMetadata.Default, 0);
			}
			break;
		}
		return ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, metaField, value, num);
	}

	private void LoopChildren(NavXmlPortNode node)
	{
		bool flag = false;
		XmlNodeType nodeType = reader.NodeType;
		string localName = reader.LocalName;
		ReadNextNode();
		bool flag2 = nodeType == XmlNodeType.Element && reader.NodeType == XmlNodeType.EndElement && localName == reader.LocalName;
		if (((reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA) | flag2) || (reader.NodeType == XmlNodeType.EndElement && node.MetaData.SourceType == XmlPortNodeSourceType.Table))
		{
			string text = ((reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA) ? reader.Value : string.Empty);
			AssignTextVariable(node, text, assignEmpty: true);
			AssignFieldValue(node, text, assignEmpty: true);
			ImportRecord(node);
			if (reader.NodeType == XmlNodeType.Text || reader.NodeType == XmlNodeType.CDATA)
			{
				int depth = reader.Depth;
				ReadNextNode();
				_ = reader.Depth > depth;
			}
		}
		if (node.ElementChildren.Count >= 1)
		{
			int depth2 = reader.Depth;
			bool flag3 = true;
			foreach (NavXmlPortNode elementChild in node.ElementChildren)
			{
				if (!NodeNameEqual(elementChild))
				{
					if (elementChild.MetaData.MinOccurs != XmlPortNodeMinOccurs.Zero)
					{
						throw NavNCLXmlPortMetadataException.CreateOccurrenceExceeded(CultureInfo.CurrentCulture, elementChild.MetaData.NodeName, elementChild.MetaData.MinOccurs.ToString(), reader.Name);
					}
					continue;
				}
				if (HasAttributes() && !elementChild.HasAttributes)
				{
					MoveToFirstAttribute();
					throw NavNCLXmlPortUnexpectedAttributeException.Create(CultureInfo.CurrentCulture, reader.Name);
				}
				if (HasAttributes() || NavigatorHasChildren() || (elementChild.HasChildren && xmlPort.Metadata.Format == XmlPortNodeFormat.Xml))
				{
					LoopXmlPortNode(elementChild);
					if (NodeNameEqual(elementChild) && elementChild.MetaData.MaxOccurs == XmlPortNodeMaxOccurs.Once && elementChild.Occurs >= 1)
					{
						throw NavNCLXmlPortDuplicateElementException.Create(CultureInfo.CurrentCulture, elementChild.MetaData.NodeName);
					}
				}
				else
				{
					flag2 = reader.NodeType == XmlNodeType.Element && reader.IsEmptyElement && NodeNameEqual(elementChild);
					AssignTextVariable(elementChild, reader.Value, flag2);
					AfterAssignTextVariableTrigger(elementChild);
					AssignFieldValue(elementChild, reader.Value, flag2);
					if (xmlPort.Metadata.Format == XmlPortNodeFormat.Xml)
					{
						LoopXmlPortNode(elementChild);
					}
					flag = true;
				}
				if (elementChild.BreakNode)
				{
					flag3 = CheckBreak(elementChild);
					if (elementChild.MetaData.SourceType != XmlPortNodeSourceType.Table)
					{
						break;
					}
					continue;
				}
				flag3 = ReadNextNode();
				if (!flag3)
				{
					int num = node.ElementChildren.LastIndexOf(elementChild) + 1;
					if (num < node.ElementChildren.Count)
					{
						CheckMinOccurs(num, node);
					}
					break;
				}
				if (flag && NodeNameEqual(elementChild) && elementChild.MetaData.MaxOccurs == XmlPortNodeMaxOccurs.Once && elementChild.Occurs >= 1)
				{
					throw NavNCLXmlPortDuplicateElementException.Create(CultureInfo.CurrentCulture, elementChild.MetaData.NodeName);
				}
			}
			if (flag3 && reader.NodeType == XmlNodeType.Element && reader.Depth == depth2 && VerifyNamespace())
			{
				throw NavNCLXmlPortUnexpectedElementException.Create(CultureInfo.CurrentCulture, reader.Name);
			}
		}
		AfterAssignTextVariableTrigger(node);
	}

	private void LoopAttributes(NavXmlPortNode node)
	{
		bool flag = MoveToFirstAttribute();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		while (flag)
		{
			if (!reader.Name.Contains(':'))
			{
				dictionary.Add(reader.LocalName, reader.Value);
			}
			flag = MoveToNextAttribute();
		}
		foreach (NavXmlPortNode attributeChild in node.AttributeChildren)
		{
			string value = null;
			if (!dictionary.TryGetValue(attributeChild.MetaData.NodeName, out value))
			{
				if (attributeChild.MetaData.Occurrence != XmlPortNodeOccurrence.Optional)
				{
					throw NavNCLXmlPortAttributeMismatchException.Create(CultureInfo.CurrentCulture, attributeChild.MetaData.NodeName);
				}
				continue;
			}
			dictionary.Remove(attributeChild.MetaData.NodeName);
			AssignTextVariable(attributeChild, value, assignEmpty: false);
			AfterAssignTextVariableTrigger(attributeChild);
			AssignFieldValue(attributeChild, value, assignEmpty: false);
			if (attributeChild.BreakNode)
			{
				attributeChild.BreakNode = false;
				dictionary.Clear();
				break;
			}
		}
		if (dictionary.Count > 0)
		{
			throw NavNCLXmlPortUnexpectedAttributeException.Create(CultureInfo.CurrentCulture, dictionary.Keys.First());
		}
		reader.MoveToElement();
	}

	private void LoopXmlPortNode(NavXmlPortNode xmlPortNode)
	{
		do
		{
			xmlPortNode.SkipNode = false;
			if (!NodeNameEqual(xmlPortNode) || (xmlPortNode.MetaData.MaxOccurs == XmlPortNodeMaxOccurs.Once && xmlPortNode.Occurs == 1) || reader.NodeType == XmlNodeType.EndElement)
			{
				MoveToPrevious();
				break;
			}
			CheckLinkTableForceInsert(xmlPortNode);
			CheckLinkTableAndFields(xmlPortNode);
			CheckMaxOccurs(xmlPortNode);
			if (!xmlPortNode.RecordInitialized)
			{
				InitRecord(xmlPortNode);
				if (xmlPortNode.BreakNode)
				{
					break;
				}
				if (xmlPortNode.SkipNode)
				{
					NavigatorMoveToNext();
					MoveToPrevious();
					continue;
				}
			}
			if (HasAttributes())
			{
				LoopAttributes(xmlPortNode);
			}
			else if (xmlPortNode.HasAttributes)
			{
				foreach (NavXmlPortNode attributeChild in xmlPortNode.AttributeChildren)
				{
					if (attributeChild.MetaData.Occurrence == XmlPortNodeOccurrence.Required)
					{
						throw NavNCLXmlPortAttributeMismatchException.Create(CultureInfo.CurrentCulture, attributeChild.MetaData.NodeName);
					}
				}
			}
			if (NavigatorHasChildren())
			{
				foreach (NavXmlPortNode elementChild in xmlPortNode.ElementChildren)
				{
					elementChild.Occurs = 0;
				}
				LoopChildren(xmlPortNode);
			}
			else
			{
				if (xmlPortNode.HasChildren)
				{
					foreach (NavXmlPortNode elementChild2 in xmlPortNode.ElementChildren)
					{
						if (elementChild2.MetaData.MinOccurs == XmlPortNodeMinOccurs.Once)
						{
							throw NavNCLXmlPortMetadataException.CreateOccurrenceExceeded(CultureInfo.CurrentCulture, elementChild2.MetaData.NodeName, elementChild2.MetaData.MinOccurs.ToString(), string.Empty);
						}
					}
				}
				AfterAssignTextVariableTrigger(xmlPortNode);
			}
			ImportRecord(xmlPortNode);
			if (xmlPortNode.BreakNode)
			{
				break;
			}
		}
		while (ReadNextNode());
	}

	private void ValidationCallback(object sender, ValidationEventArgs args)
	{
		if (!xmlPort.Metadata.InlineSchema || !(args.Exception is XmlSchemaValidationException) || args.Message == null || !args.Message.Contains("'schema'") || !args.Message.Contains("http://www.w3.org/2001/XMLSchema"))
		{
			throw NavNCLXmlPortInvalidXmlException.Create(CultureInfo.CurrentCulture, args.Message);
		}
	}

	private bool VerifyNamespace()
	{
		if (reader.NamespaceURI == defaultNamespace)
		{
			return true;
		}
		if (xmlPort.Metadata.Namespaces != null)
		{
			return validNamespaceList.Contains(reader.NamespaceURI);
		}
		return false;
	}

	private void MoveToPrevious()
	{
		previousNodeCounter++;
	}

	private bool NavigatorMoveToNext()
	{
		if (previousNodeCounter > 0)
		{
			return true;
		}
		int depth = reader.Depth;
		if (reader.NodeType == XmlNodeType.EndElement)
		{
			ReadNextNode();
			if (reader.Depth == depth)
			{
				return true;
			}
			if (reader.Depth < depth)
			{
				return false;
			}
		}
		while (ReadNextNode())
		{
			if (reader.Depth < depth)
			{
				return false;
			}
			if (reader.Depth != depth)
			{
				continue;
			}
			if (reader.NodeType == XmlNodeType.EndElement)
			{
				ReadNextNode();
				if (reader.Depth < depth)
				{
					return false;
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private void MoveToNextElement()
	{
		if (!reader.IsEmptyElement)
		{
			int depth = reader.Depth;
			while (!reader.EOF && reader.Read() && (reader.Depth != depth || reader.NodeType != XmlNodeType.EndElement))
			{
			}
		}
	}

	private bool NavigatorHasChildren()
	{
		if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
		{
			return true;
		}
		return false;
	}

	private bool IsNamespaceAttribute()
	{
		if (string.Compare("xmlns", reader.Name, StringComparison.Ordinal) != 0)
		{
			return string.Compare("xmlns", reader.Prefix, StringComparison.Ordinal) == 0;
		}
		return true;
	}

	private bool HasAttributes()
	{
		if (!reader.HasAttributes)
		{
			return false;
		}
		if (reader.AttributeCount == 1)
		{
			reader.MoveToFirstAttribute();
			if (IsNamespaceAttribute())
			{
				reader.MoveToElement();
				return false;
			}
			reader.MoveToElement();
		}
		return true;
	}

	private bool MoveToFirstAttribute()
	{
		bool flag = reader.MoveToFirstAttribute();
		if (flag && IsNamespaceAttribute())
		{
			return reader.MoveToNextAttribute();
		}
		return flag;
	}

	private bool MoveToNextAttribute()
	{
		bool flag = reader.MoveToNextAttribute();
		if (flag && IsNamespaceAttribute())
		{
			return reader.MoveToNextAttribute();
		}
		return flag;
	}

	private static void AssertElementNode(NavXmlPortNode node)
	{
		if (node.MetaData.NodeType != XmlPortNodeType.Element)
		{
			throw NavNCLXmlPortInvalidNodeTypeException.CreateUnexpectedNodeType(CultureInfo.CurrentCulture, node.MetaData.NodeName);
		}
	}

	internal NavXmlPortImporter(NavXmlPort xmlPortInstance)
	{
		if (xmlPortInstance == null)
		{
			throw new ArgumentNullException("xmlPortInstance");
		}
		xmlPort = xmlPortInstance;
		tagNames = new Stack<string>();
		defaultNamespace = ((xmlPort.Metadata.UseDefaultNamespace && xmlPort.Metadata.DefaultNamespace != null) ? xmlPort.Metadata.DefaultNamespace : string.Empty);
		compareLocalName = xmlPort.Metadata.UseLax || xmlPort.Metadata.Namespaces == null;
		if (xmlPort.Metadata.Namespaces == null)
		{
			return;
		}
		validNamespaceList = new List<string>();
		foreach (string value in xmlPort.Metadata.Namespaces.Values)
		{
			validNamespaceList.Add(value);
		}
	}

	internal void Import()
	{
		Stream internalStreamCheckedForRead = xmlPort.Source.InternalStreamCheckedForRead;
		long position = -1L;
		if (internalStreamCheckedForRead.CanSeek)
		{
			position = internalStreamCheckedForRead.Position;
		}
		try
		{
			if (internalStreamCheckedForRead.CanSeek)
			{
				internalStreamCheckedForRead.Position = 0L;
			}
			XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
			xmlReaderSettings.DtdProcessing = DtdProcessing.Prohibit;
			xmlReaderSettings.XmlResolver = null;
			if (xmlPort.Metadata.Format == XmlPortNodeFormat.Xml)
			{
				xmlReaderSettings.ValidationType = ValidationType.Schema;
				xmlReaderSettings.Schemas = xmlPort.Metadata.SchemaSet;
				xmlReaderSettings.CheckCharacters = false;
				xmlReaderSettings.ValidationEventHandler += ValidationCallback;
			}
			reader = XmlReader.Create(new NonDisposableStream(internalStreamCheckedForRead), xmlReaderSettings);
		}
		catch (XmlSchemaException ex)
		{
			throw new NavNCLXmlPortImportException(ex.Message, ex);
		}
		catch (XmlException ex2)
		{
			throw new NavNCLXmlPortImportException(ex2.Message, ex2);
		}
		AssertElementNode(xmlPort.Nodes[0]);
		try
		{
			reader.MoveToContent();
			if (reader.NodeType == XmlNodeType.Element)
			{
				tagNames.Push(reader.Name);
			}
		}
		catch (XmlException ex3)
		{
			throw new NavNCLXmlPortImportException(ex3.Message, ex3);
		}
		if (reader.NodeType != XmlNodeType.Element || !NodeNameEqual(xmlPort.Nodes[0]) || !VerifyNamespace())
		{
			throw NavNCLXmlPortElementNotFoundException.Create(CultureInfo.CurrentCulture, xmlPort.Nodes[0].MetaData.NodeName);
		}
		try
		{
			LoopXmlPortNode(xmlPort.Nodes[0]);
			while (!reader.EOF)
			{
				reader.Read();
				if (reader.NodeType != XmlNodeType.Whitespace)
				{
					break;
				}
			}
		}
		catch (XmlException ex4)
		{
			throw new NavNCLXmlPortImportException(ex4.Message, ex4);
		}
		finally
		{
			if (internalStreamCheckedForRead.CanSeek)
			{
				internalStreamCheckedForRead.Position = position;
			}
		}
	}
}
