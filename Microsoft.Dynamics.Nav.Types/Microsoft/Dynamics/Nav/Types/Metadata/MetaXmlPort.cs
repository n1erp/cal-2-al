using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Xsd;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class MetaXmlPort
{
	public delegate MasterPage RemoveItemsOnPageBasedOnLicenseAndApplicationArea(MasterPage masterPage);

	private int id;

	private string name;

	private int metadataAppGroupId;

	private int languageAppGroupId;

	private bool useDefaultNamespace;

	private string defaultNamespace;

	private string mainPrefix;

	private Dictionary<string, string> namespaces;

	internal Dictionary<string, XmlSchema> namespaceSchemas;

	private bool inlineSchema;

	private bool useLax;

	private XmlPortNodeFormat format;

	private string fileName;

	private string fieldDelimiter;

	private string fieldSeparator;

	private string recordSeparator;

	private string tableSeparator;

	private bool useRequestForm;

	private XsdBuilder xsdBuilder;

	private XmlSchemaSet schemaSet;

	private KeyValuePair<int, string>[] captionML;

	private XmlPortDirection direction = XmlPortDirection.Both;

	private bool defaultFieldsValidation;

	private XmlPortEncoding encoding = XmlPortEncoding.UTF16;

	private XmlPortTextEncoding textEncoding = XmlPortTextEncoding.MSDos;

	private XmlPortXmlVersionNo xmlVersionNo = XmlPortXmlVersionNo.Version10;

	private XmlPortFormatEvaluate formatEvaluate = XmlPortFormatEvaluate.CSIDE;

	private TransactionType transactionType;

	private string permissions;

	private bool preserveWhiteSpace;

	private List<MetaXmlPortNode> nodes = new List<MetaXmlPortNode>();

	private List<string> typeNames = new List<string>();

	private MasterPage requestFormMetadata;

	private RemoveItemsOnPageBasedOnLicenseAndApplicationArea removeItemsOnPageBasedOnLicenseAndApplicationArea;

	public MasterPage RequestFormMetadata
	{
		get
		{
			if (removeItemsOnPageBasedOnLicenseAndApplicationArea != null)
			{
				return removeItemsOnPageBasedOnLicenseAndApplicationArea(requestFormMetadata);
			}
			return requestFormMetadata;
		}
	}

	public int Id => id;

	public string Name => name;

	public int MetadataAppGroupId => metadataAppGroupId;

	public int LanguageAppGroupId => languageAppGroupId;

	public KeyValuePair<int, string>[] CaptionML => captionML;

	public XmlPortDirection Direction => direction;

	public bool UseDefaultNamespace => useDefaultNamespace;

	public string DefaultNamespace => defaultNamespace;

	public Dictionary<string, string> Namespaces => namespaces;

	internal Dictionary<string, XmlSchema> NamespaceSchemas => namespaceSchemas;

	public bool InlineSchema => inlineSchema;

	public bool UseLax => useLax;

	public string FileName => fileName;

	public string FieldDelimiter => fieldDelimiter;

	public string FieldSeparator => fieldSeparator;

	public string RecordSeparator => recordSeparator;

	public string TableSeparator => tableSeparator;

	public bool UseRequestForm => useRequestForm;

	public bool DefaultFieldsValidation => defaultFieldsValidation;

	public XmlPortEncoding Encoding => encoding;

	public XmlPortTextEncoding TextEncoding => textEncoding;

	public XmlPortNodeFormat Format => format;

	public XmlPortXmlVersionNo XMLVersionNo => xmlVersionNo;

	public XmlPortFormatEvaluate FormatEvaluate => formatEvaluate;

	public TransactionType TransactionType => transactionType;

	public string Permissions => permissions;

	public bool PreserveWhiteSpace => preserveWhiteSpace;

	public List<MetaXmlPortNode> Nodes => nodes;

	public XmlSchema Schema
	{
		get
		{
			if (xsdBuilder == null)
			{
				GenerateXmlSchema();
			}
			return xsdBuilder.Schema;
		}
	}

	public XmlSchemaSet SchemaSet
	{
		get
		{
			if (xsdBuilder == null)
			{
				GenerateXmlSchema();
			}
			return schemaSet;
		}
	}

	public XmlQualifiedName SchemaTypeName
	{
		get
		{
			if (xsdBuilder == null)
			{
				GenerateXmlSchema();
			}
			foreach (XmlSchemaObject item in xsdBuilder.Schema.Items)
			{
				if (item is XmlSchemaElement xmlSchemaElement)
				{
					return xmlSchemaElement.QualifiedName;
				}
			}
			return null;
		}
	}

	private void BindParentAndChildrenProperties()
	{
		int num = -1;
		foreach (MetaXmlPortNode node in nodes)
		{
			num++;
			int indentation = node.Indentation;
			for (int i = num + 1; i < nodes.Count; i++)
			{
				MetaXmlPortNode metaXmlPortNode = nodes[i];
				if (metaXmlPortNode.Indentation == indentation + 1)
				{
					node.Children.Add(metaXmlPortNode);
				}
				else if (metaXmlPortNode.Indentation <= indentation)
				{
					break;
				}
			}
		}
		foreach (MetaXmlPortNode node2 in nodes)
		{
			foreach (MetaXmlPortNode child in node2.Children)
			{
				child.Parent = node2;
			}
		}
	}

	private void GenerateXmlSchema()
	{
		if (nodes.Count == 0)
		{
			return;
		}
		MetaXmlPortNode metaXmlPortNode = nodes[0];
		mainPrefix = metaXmlPortNode.NodePrefix;
		string text = string.Empty;
		if (Namespaces != null)
		{
			namespaceSchemas = new Dictionary<string, XmlSchema>();
			foreach (KeyValuePair<string, string> @namespace in Namespaces)
			{
				if (@namespace.Key.Equals(mainPrefix))
				{
					text = @namespace.Value;
					continue;
				}
				xsdBuilder = new XsdBuilder(@namespace.Value);
				BuildSchemaElementForNode(metaXmlPortNode, @namespace.Key, null);
				NamespaceSchemas.Add(@namespace.Key, xsdBuilder.Schema);
			}
		}
		xsdBuilder = new XsdBuilder(useDefaultNamespace ? DefaultNamespace : text);
		BuildSchemaElementForNode(metaXmlPortNode, mainPrefix, null);
		schemaSet = new XmlSchemaSet();
		schemaSet.Add(xsdBuilder.Schema);
		if (Namespaces != null)
		{
			foreach (KeyValuePair<string, string> namespace2 in Namespaces)
			{
				if (!namespace2.Key.Equals(mainPrefix))
				{
					xsdBuilder.Schema.Namespaces.Add(namespace2.Key, namespace2.Value);
					XmlSchemaImport xmlSchemaImport = new XmlSchemaImport();
					xmlSchemaImport.Namespace = namespace2.Value;
					XmlSchema value = null;
					if (!NamespaceSchemas.TryGetValue(namespace2.Key, out value))
					{
						throw new NavNCLXmlPortNodeProcessException(string.Format(CultureInfo.CurrentCulture, Lang.XmlPortPrefixUnknown, namespace2.Key));
					}
					xmlSchemaImport.Schema = value;
					xsdBuilder.Schema.Includes.Add(xmlSchemaImport);
					schemaSet.Add(value);
				}
			}
		}
		schemaSet.Compile();
	}

	public string InlineSchemaAsXmlString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (XmlSchema item in SchemaSet.Schemas())
		{
			StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
			item.Write(stringWriter);
			stringBuilder.Append(stringWriter.ToString().Substring(39));
		}
		return stringBuilder.ToString();
	}

	private void BuildSchemaElementForNode(MetaXmlPortNode node, string prefix, ComplexTypeBuilder complexType)
	{
		if (node.NodeName.Contains(":"))
		{
			throw new NavNCLXmlPortNodeProcessException(string.Format(CultureInfo.CurrentCulture, Lang.XmlSchemaGenerationAndColonNotSupported, node.NodeName));
		}
		if (node.HasChildren)
		{
			BuildComplexTypeForNode(node, prefix, complexType);
		}
		else
		{
			BuildSimpleTypeForNode(node, prefix, complexType);
		}
	}

	private void BuildSimpleTypeForNode(MetaXmlPortNode node, string prefix, ComplexTypeBuilder complexType)
	{
		bool isMainSchema = node.Parent != null && string.Equals(prefix, node.Parent.NodePrefix);
		if (!node.NodePrefix.Equals(prefix))
		{
			if (isMainSchema)
			{
				BuildRefElement(node, node.NodePrefix, complexType);
			}
			return;
		}
		if (!isMainSchema && xsdBuilder.IsRegisteredElement(node.NodeName))
		{
			if (xsdBuilder.IsRegistered(node.NodeName))
			{
				xsdBuilder.UpdateTypeAttributes(node.NodeName, null);
			}
			return;
		}
		NavType type = NavType.Text;
		if (node is MetaXmlPortFieldNode metaXmlPortFieldNode)
		{
			type = metaXmlPortFieldNode.DataType;
		}
		BaseXsdBuilder baseXsdBuilder = xsdBuilder;
		if (complexType != null)
		{
			baseXsdBuilder = complexType;
		}
		baseXsdBuilder.CreateElement(node.NodeName, ElementNumber.None, (ElementBuilder element) =>
		{
			if (FormatEvaluate == XmlPortFormatEvaluate.XML)
			{
				element.XmlType = XmlTypeHelper.XmlTypeNameFrom(type);
				element.DefaultValue = XmlTypeHelper.XmlDefaultValueFromNavType(type);
			}
			else
			{
				element.XmlType = XmlTypeHelper.ClassicXmlPortXmlTypeNameFrom(type);
				element.DefaultValue = XmlTypeHelper.ClassicXmlPortXmlDefaultValueFromNavType(type);
			}
			if (isMainSchema)
			{
				element.MinOccurs = ((node.MinOccurs == XmlPortNodeMinOccurs.Once) ? 1 : 0);
				if (node.MaxOccurs == XmlPortNodeMaxOccurs.Once)
				{
					element.MaxOccurs = 1m;
				}
				else
				{
					element.MaxOccursString = "unbounded";
				}
			}
		});
	}

	private void BuildComplexTypeForNode(MetaXmlPortNode node, string prefix, ComplexTypeBuilder complexType)
	{
		bool isMainSchema = node.Parent != null && string.Equals(prefix, node.Parent.NodePrefix);
		if (!node.NodePrefix.Equals(prefix))
		{
			if (isMainSchema)
			{
				BuildRefElement(node, node.NodePrefix, complexType);
				return;
			}
			{
				foreach (MetaXmlPortNode elementChild in node.ElementChildren)
				{
					BuildSchemaElementForNode(elementChild, prefix, complexType);
				}
				return;
			}
		}
		int num;
		int num2;
		if (!isMainSchema)
		{
			num = (xsdBuilder.IsRegisteredElement(node.NodeName) ? 1 : 0);
			if (num != 0)
			{
				num2 = (xsdBuilder.IsRegistered(node.NodeName) ? 1 : 0);
				goto IL_0108;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 0;
		goto IL_0108;
		IL_0108:
		bool flag = (byte)num2 != 0;
		bool flag2 = num != 0 && !xsdBuilder.IsRegistered(node.NodeName);
		if (flag)
		{
			xsdBuilder.UpdateTypeAttributes(node.NodeName, node.AttributeChildren);
			return;
		}
		if (flag2 && node.HasElementChildren)
		{
			throw new NavNCLXmlPortInvalidXmlException(string.Format(CultureInfo.CurrentCulture, Lang.XmlFirstElementNotGeneral, node.NodeName));
		}
		string nodeTypeName = CreateUniqueTypeName(node.NodeName);
		ComplexTypeBuilder currentComplexType = null;
		xsdBuilder.CreateType(nodeTypeName, UseLax, (ComplexTypeBuilder element) =>
		{
			currentComplexType = element;
			if (node is MetaXmlPortFieldNode || node is MetaXmlPortTextNode)
			{
				element.IsMixed = true;
			}
			foreach (MetaXmlPortNode attributeChild in node.AttributeChildren)
			{
				element.AddAttribute(CreateTextAttribute(attributeChild.NodeName, attributeChild.Occurrence));
			}
			foreach (MetaXmlPortNode elementChild2 in node.ElementChildren)
			{
				BuildSchemaElementForNode(elementChild2, prefix, element);
			}
		});
		if (flag2)
		{
			CheckAttributesAreOptional(node.AttributeChildren, node.NodeName);
			xsdBuilder.UpdateElementSchema(node.NodeName, xsdBuilder.GetQualifiedName(nodeTypeName));
			return;
		}
		BaseXsdBuilder baseXsdBuilder = xsdBuilder;
		ElementNumber cardinality = ElementNumber.None;
		if (complexType != null)
		{
			baseXsdBuilder = complexType;
			cardinality = FindCardinal(node);
		}
		baseXsdBuilder.CreateElement(node.NodeName, cardinality, (ElementBuilder element) =>
		{
			element.XmlType = xsdBuilder.GetQualifiedName(nodeTypeName);
			if (isMainSchema)
			{
				element.MinOccurs = ((node.MinOccurs == XmlPortNodeMinOccurs.Once) ? 1 : 0);
				if (node.MaxOccurs == XmlPortNodeMaxOccurs.Once)
				{
					element.MaxOccurs = 1m;
				}
				else
				{
					element.MaxOccursString = "unbounded";
				}
			}
		});
	}

	private static ElementNumber FindCardinal(MetaXmlPortNode node)
	{
		ElementNumber result = ElementNumber.None;
		if (node.Parent == null || (node.MinOccurs == XmlPortNodeMinOccurs.Once && node.MaxOccurs == XmlPortNodeMaxOccurs.Once))
		{
			result = ElementNumber.One;
		}
		else if (node.MinOccurs == XmlPortNodeMinOccurs.Zero && node.MaxOccurs == XmlPortNodeMaxOccurs.Once)
		{
			result = ElementNumber.ZeroOrOne;
		}
		else if ((node.MinOccurs == XmlPortNodeMinOccurs.Once || node.MinOccurs == XmlPortNodeMinOccurs.Zero) && node.MaxOccurs == XmlPortNodeMaxOccurs.Unbounded)
		{
			result = ElementNumber.Collection;
		}
		return result;
	}

	private void BuildRefElement(MetaXmlPortNode node, string prefix, ComplexTypeBuilder complexType)
	{
		BaseXsdBuilder baseXsdBuilder = xsdBuilder;
		ElementNumber cardinality = ElementNumber.None;
		if (complexType != null)
		{
			baseXsdBuilder = complexType;
			cardinality = FindCardinal(node);
		}
		string nodeTypeName = CreateUniqueTypeName(node.NodeName);
		string value = null;
		if (!Namespaces.TryGetValue(prefix, out value))
		{
			throw new NavNCLXmlPortNodeProcessException(string.Format(CultureInfo.CurrentCulture, Lang.XmlPortPrefixUnknown, prefix));
		}
		baseXsdBuilder.CreateElement(node.NodeName, value, cardinality, (ElementBuilder element) =>
		{
			element.XmlType = xsdBuilder.GetQualifiedName(nodeTypeName);
			if (node.Parent != null)
			{
				element.MinOccurs = ((node.MinOccurs == XmlPortNodeMinOccurs.Once) ? 1 : 0);
				if (node.MaxOccurs == XmlPortNodeMaxOccurs.Once)
				{
					element.MaxOccurs = 1m;
				}
				else
				{
					element.MaxOccursString = "unbounded";
				}
			}
		});
	}

	private string CreateUniqueTypeName(string name)
	{
		if (typeNames.Contains(name))
		{
			return CreateUniqueTypeName(name + "Type");
		}
		typeNames.Add(name);
		return name;
	}

	private static string TranslateDelimiter(string value)
	{
		if (value == null)
		{
			return string.Empty;
		}
		string[] array = new string[6] { "<None>", "<NewLine>", "<CR/LF>", "<CR>", "<LF>", "<TAB>" };
		string[] array2 = new string[6]
		{
			string.Empty,
			"\r\n",
			"\r\n",
			"\r",
			"\n",
			"\t"
		};
		StringBuilder stringBuilder = new StringBuilder(value);
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Replace(array[i], array2[i]);
		}
		return stringBuilder.ToString();
	}

	private static XmlSchemaAttribute CreateTextAttribute(string name, XmlPortNodeOccurrence occurrence)
	{
		XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
		xmlSchemaAttribute.Name = name;
		xmlSchemaAttribute.SchemaTypeName = XmlTypeHelper.XmlTypeNameFrom(NavType.Text);
		switch (occurrence)
		{
		case XmlPortNodeOccurrence.Optional:
			xmlSchemaAttribute.Use = XmlSchemaUse.Optional;
			break;
		case XmlPortNodeOccurrence.Required:
			xmlSchemaAttribute.Use = XmlSchemaUse.Required;
			break;
		default:
			throw new NotSupportedException(occurrence.ToString());
		}
		return xmlSchemaAttribute;
	}

	private static void CheckAttributesAreOptional(List<MetaXmlPortNode> attributes, string nodeName)
	{
		if (attributes == null)
		{
			return;
		}
		foreach (MetaXmlPortNode attribute in attributes)
		{
			if (attribute.Occurrence != XmlPortNodeOccurrence.Optional)
			{
				throw new NavNCLXmlPortInvalidXmlException(string.Format(CultureInfo.CurrentCulture, Lang.XmlAttributeOccurrenceMismatch, attribute.NodeName, nodeName));
			}
		}
	}

	public MetaXmlPort(XmlDocument xmlDoc, MetaReport.CreateRequestForm createRequestForm, int metadataAppGroupId, int languageAppGroupId, RemoveItemsOnPageBasedOnLicenseAndApplicationArea removeItemsOnPageBasedOnLicenseAndApplicationArea = null)
	{
		this.removeItemsOnPageBasedOnLicenseAndApplicationArea = removeItemsOnPageBasedOnLicenseAndApplicationArea;
		string text = null;
		this.metadataAppGroupId = metadataAppGroupId;
		this.languageAppGroupId = languageAppGroupId;
		List<KeyValuePair<int, string>> list = new List<KeyValuePair<int, string>>();
		foreach (XmlNode childNode in xmlDoc.DocumentElement.ChildNodes)
		{
			switch (childNode.Name.ToUpper(CultureInfo.InvariantCulture))
			{
			case "ID":
				id = int.Parse(childNode.InnerText, CultureInfo.InvariantCulture);
				break;
			case "NAME":
				name = childNode.InnerText;
				break;
			case "DEFAULTNAMESPACE":
				defaultNamespace = childNode.InnerText;
				break;
			case "NAMESPACES":
				namespaces = new Dictionary<string, string>();
				if (!childNode.HasChildNodes)
				{
					break;
				}
				foreach (XmlNode childNode2 in childNode.ChildNodes)
				{
					if (childNode2 != null && childNode2.ChildNodes != null && childNode2.ChildNodes.Count == 2 && childNode2.ChildNodes[0].Name.ToUpper(CultureInfo.InvariantCulture).Equals("NAMESPACEPREFIX") && childNode2.ChildNodes[1].Name.ToUpper(CultureInfo.InvariantCulture).Equals("NAMESPACEURI"))
					{
						namespaces.Add(childNode2.ChildNodes[0].InnerText, childNode2.ChildNodes[1].InnerText);
					}
				}
				break;
			case "INLINESCHEMA":
				inlineSchema = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "USELAX":
				useLax = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "USEDEFAULTNAMESPACE":
				useDefaultNamespace = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "REQUESTPAGE":
				text = childNode.FirstChild.OuterXml;
				break;
			case "FORMAT":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "XML":
					format = XmlPortNodeFormat.Xml;
					break;
				case "FIXED TEXT":
					format = XmlPortNodeFormat.FixedText;
					break;
				case "VARIABLE TEXT":
					format = XmlPortNodeFormat.VariableText;
					break;
				default:
					throw new ArgumentException("Format");
				}
				break;
			case "FILENAME":
				fileName = childNode.InnerText;
				break;
			case "FIELDDELIMITER":
				fieldDelimiter = TranslateDelimiter(childNode.InnerText);
				break;
			case "FIELDSEPARATOR":
				fieldSeparator = TranslateDelimiter(childNode.InnerText);
				break;
			case "RECORDSEPARATOR":
				recordSeparator = TranslateDelimiter(childNode.InnerText);
				break;
			case "TABLESEPARATOR":
				tableSeparator = TranslateDelimiter(childNode.InnerText);
				break;
			case "USEREQUESTFORM":
				useRequestForm = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "CAPTIONML":
				if (!childNode.HasChildNodes)
				{
					break;
				}
				foreach (XmlNode childNode3 in childNode.ChildNodes)
				{
					if (childNode3.Attributes != null && childNode3.Attributes.Count > 0)
					{
						int key = int.Parse(childNode3.Attributes["Id"].InnerText, CultureInfo.InvariantCulture);
						string innerText = childNode3.InnerText;
						list.Add(new KeyValuePair<int, string>(key, innerText));
					}
				}
				break;
			case "DIRECTION":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "BOTH":
					direction = XmlPortDirection.Both;
					break;
				case "IMPORT":
					direction = XmlPortDirection.Import;
					break;
				case "EXPORT":
					direction = XmlPortDirection.Export;
					break;
				default:
					throw new ArgumentException("Direction");
				}
				break;
			case "DEFAULTFIELDSVALIDATION":
				defaultFieldsValidation = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			case "ENCODING":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "UTF-16":
					encoding = XmlPortEncoding.UTF16;
					break;
				case "UTF-8":
					encoding = XmlPortEncoding.UTF8;
					break;
				case "ISO-8859-2":
					encoding = XmlPortEncoding.ISO88592;
					break;
				default:
					throw new ArgumentException("Encoding");
				}
				break;
			case "TEXTENCODING":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "UTF-16":
					textEncoding = XmlPortTextEncoding.UTF16;
					break;
				case "UTF-8":
					textEncoding = XmlPortTextEncoding.UTF8;
					break;
				case "MS-DOS":
					textEncoding = XmlPortTextEncoding.MSDos;
					break;
				case "WINDOWS":
					textEncoding = XmlPortTextEncoding.Windows;
					break;
				default:
					throw new ArgumentException("TextEncoding");
				}
				break;
			case "FORMATEVALUATE":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "C/SIDE FORMAT/EVALUATE":
					formatEvaluate = XmlPortFormatEvaluate.CSIDE;
					break;
				case "XML FORMAT/EVALUATE":
					formatEvaluate = XmlPortFormatEvaluate.XML;
					break;
				default:
					throw new ArgumentException("Format/Evaluate");
				}
				break;
			case "TRANSACTIONTYPE":
				switch (childNode.InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "UPDATENOLOCKS":
					transactionType = TransactionType.UpdateNoLocks;
					break;
				case "UPDATE":
					transactionType = TransactionType.Update;
					break;
				case "SNAPSHOT":
					transactionType = TransactionType.Snapshot;
					break;
				case "BROWSE":
					transactionType = TransactionType.Browse;
					break;
				case "REPORT":
					transactionType = TransactionType.Report;
					break;
				default:
					throw new ArgumentException("TransactionType");
				}
				break;
			case "PERMISSIONS":
				permissions = childNode.InnerText;
				break;
			case "NODE":
				switch (childNode.SelectSingleNode("SourceType").InnerText.ToUpper(CultureInfo.InvariantCulture))
				{
				case "TEXT":
				{
					MetaXmlPortTextNode item3 = new MetaXmlPortTextNode(childNode)
					{
						SourceType = XmlPortNodeSourceType.Text
					};
					nodes.Add(item3);
					break;
				}
				case "TABLE":
				{
					MetaXmlPortTableNode item2 = new MetaXmlPortTableNode(childNode)
					{
						SourceType = XmlPortNodeSourceType.Table
					};
					nodes.Add(item2);
					break;
				}
				case "FIELD":
				{
					MetaXmlPortFieldNode item = new MetaXmlPortFieldNode(childNode)
					{
						SourceType = XmlPortNodeSourceType.Field
					};
					nodes.Add(item);
					break;
				}
				default:
					throw new ArgumentException("SourceType");
				}
				break;
			case "PRESERVEWHITESPACE":
				preserveWhiteSpace = PropertiesSolveHelper.SolveBoolean(childNode.InnerText);
				break;
			default:
				throw new ArgumentException(childNode.Name.ToUpper(CultureInfo.InvariantCulture));
			case "XMLVERSIONNO":
			case "METADATAVERSION":
				break;
			}
		}
		list = MetaReport.MultiLanguageDictionaryMerge(list, 0, ObjectType.XmlPort, Id, -1, languageAppGroupId);
		captionML = list.ToArray();
		if (createRequestForm != null && !string.IsNullOrEmpty(text))
		{
			requestFormMetadata = createRequestForm(MetaReport.DeserializePageDefinition(text, ObjectType.XmlPort, id, languageAppGroupId), captionML, id);
		}
		BindParentAndChildrenProperties();
	}
}
