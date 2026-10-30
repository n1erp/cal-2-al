using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Xsd;

public class XsdBuilder : BaseXsdBuilder
{
	private Dictionary<string, Tuple<int, XmlSchemaElement>> registeredElements = new Dictionary<string, Tuple<int, XmlSchemaElement>>();

	private Dictionary<string, Tuple<int, XmlSchemaComplexType>> registeredComplexTypes = new Dictionary<string, Tuple<int, XmlSchemaComplexType>>();

	private XmlSchema schema = new XmlSchema();

	public ICollection<string> RegisteredTypes
	{
		[DebuggerStepThrough]
		get
		{
			return registeredComplexTypes.Keys;
		}
	}

	public ICollection<string> RegisteredElements
	{
		[DebuggerStepThrough]
		get
		{
			return registeredElements.Keys;
		}
	}

	public XmlSchema Schema
	{
		[DebuggerStepThrough]
		get
		{
			return schema;
		}
	}

	public XsdBuilder()
		: this((string)null)
	{
	}

	public XsdBuilder(string theNamespace)
		: base(theNamespace)
	{
		if (!string.IsNullOrWhiteSpace(theNamespace))
		{
			schema.ElementFormDefault = XmlSchemaForm.Qualified;
			schema.Namespaces.Add("tns", theNamespace);
			schema.TargetNamespace = theNamespace;
		}
	}

	public XsdBuilder(XmlSchema schemaToEdit)
		: base(schemaToEdit.TargetNamespace)
	{
		schema = schemaToEdit;
		int num = 0;
		foreach (XmlSchemaObject item in schemaToEdit.Items)
		{
			if (item is XmlSchemaComplexType xmlSchemaComplexType)
			{
				registeredComplexTypes.Add(xmlSchemaComplexType.Name, new Tuple<int, XmlSchemaComplexType>(num, xmlSchemaComplexType));
				num++;
			}
		}
	}

	public bool IsRegistered(string typeLocalName)
	{
		return registeredComplexTypes.ContainsKey(typeLocalName);
	}

	public bool IsRegisteredElement(string localName)
	{
		return registeredElements.ContainsKey(localName);
	}

	public XmlQualifiedName GetQualifiedName(string typeLocalName)
	{
		if (IsRegistered(typeLocalName))
		{
			return new XmlQualifiedName(typeLocalName, Namespace);
		}
		return null;
	}

	public override XmlSchemaComplexType CreateType(string name, ComplexType complexDelegate)
	{
		XmlSchemaComplexType xmlSchemaComplexType = base.CreateType(name, complexDelegate);
		int count = schema.Items.Count;
		schema.Items.Add(xmlSchemaComplexType);
		registeredComplexTypes.Add(name, new Tuple<int, XmlSchemaComplexType>(count, xmlSchemaComplexType));
		return xmlSchemaComplexType;
	}

	public override XmlSchemaComplexType CreateType(string name, bool useLax, ComplexType complexDelegate)
	{
		XmlSchemaComplexType xmlSchemaComplexType = base.CreateType(name, useLax, complexDelegate);
		int count = schema.Items.Count;
		schema.Items.Add(xmlSchemaComplexType);
		registeredComplexTypes.Add(name, new Tuple<int, XmlSchemaComplexType>(count, xmlSchemaComplexType));
		return xmlSchemaComplexType;
	}

	public void UpdateTypeAttributes(string name, List<MetaXmlPortNode> newAttributes)
	{
		var (num2, complexType) = registeredComplexTypes[name];
		if (CheckCompatibilityAndUpdateAttributes(ref complexType, newAttributes))
		{
			schema.Items.RemoveAt(num2);
			schema.Items.Insert(num2, complexType);
			registeredComplexTypes[name] = new Tuple<int, XmlSchemaComplexType>(num2, complexType);
		}
	}

	public override XmlSchemaElement CreateElement(string name, ElementNumber cardinality, ElementType elementDelegate)
	{
		XmlSchemaElement xmlSchemaElement = base.CreateElement(name, cardinality, elementDelegate);
		int count = schema.Items.Count;
		schema.Items.Add(xmlSchemaElement);
		registeredElements.Add(name, new Tuple<int, XmlSchemaElement>(count, xmlSchemaElement));
		return xmlSchemaElement;
	}

	public void CreateElement(string name, ElementNumber cardinality, XmlQualifiedName type)
	{
		CreateElement(name, cardinality, (ElementBuilder builder) =>
		{
			builder.XmlType = type;
		});
	}

	public void UpdateElementSchema(string name, XmlQualifiedName schemaTypeName)
	{
		var (num2, xmlSchemaElement2) = registeredElements[name];
		xmlSchemaElement2.SchemaTypeName = schemaTypeName;
		schema.Items.RemoveAt(num2);
		schema.Items.Insert(num2, xmlSchemaElement2);
		registeredElements[name] = new Tuple<int, XmlSchemaElement>(num2, xmlSchemaElement2);
	}

	public void CreateEnumType(string name, EnumTypeBuilder.BuildEnumType enumDelegate)
	{
		if (enumDelegate != null)
		{
			EnumTypeBuilder enumTypeBuilder = new EnumTypeBuilder(name);
			schema.Items.Add(enumTypeBuilder.Type);
			enumDelegate(enumTypeBuilder);
		}
	}

	private static bool CheckCompatibilityAndUpdateAttributes(ref XmlSchemaComplexType complexType, List<MetaXmlPortNode> newAttributes)
	{
		Dictionary<string, XmlSchemaUse> dictionary = CreateAttributeNameAndUseMapping(complexType.Attributes);
		newAttributes = newAttributes ?? new List<MetaXmlPortNode>();
		bool result = false;
		foreach (MetaXmlPortNode newAttribute in newAttributes)
		{
			if (dictionary.TryGetValue(newAttribute.NodeName, out var value))
			{
				if (!IsSameOccurence(newAttribute.Occurrence, value))
				{
					throw new NavNCLXmlPortInvalidXmlException(string.Format(CultureInfo.CurrentCulture, Lang.XmlAttributeOccurrenceMismatch, newAttribute.NodeName, complexType.Name));
				}
				dictionary.Remove(newAttribute.NodeName);
				continue;
			}
			if (newAttribute.Occurrence != XmlPortNodeOccurrence.Optional)
			{
				throw new NavNCLXmlPortInvalidXmlException(string.Format(CultureInfo.CurrentCulture, Lang.XmlAttributeOccurrenceMismatch, newAttribute.NodeName, complexType.Name));
			}
			complexType.Attributes.Add(CreateAttribute(newAttribute.NodeName, newAttribute.Occurrence));
			result = true;
		}
		foreach (string key in dictionary.Keys)
		{
			if (dictionary[key] != XmlSchemaUse.Optional)
			{
				throw new NavNCLXmlPortInvalidXmlException(string.Format(CultureInfo.CurrentCulture, Lang.XmlAttributeOccurrenceMismatch, key, complexType.Name));
			}
		}
		return result;
	}

	private static Dictionary<string, XmlSchemaUse> CreateAttributeNameAndUseMapping(XmlSchemaObjectCollection attributes)
	{
		Dictionary<string, XmlSchemaUse> dictionary = new Dictionary<string, XmlSchemaUse>();
		foreach (XmlSchemaObject attribute in attributes)
		{
			if (attribute is XmlSchemaAttribute xmlSchemaAttribute)
			{
				dictionary.Add(xmlSchemaAttribute.Name, xmlSchemaAttribute.Use);
			}
		}
		return dictionary;
	}

	private static bool IsSameOccurence(XmlPortNodeOccurrence occurrence, XmlSchemaUse use)
	{
		if (occurrence != XmlPortNodeOccurrence.Optional || use != XmlSchemaUse.Optional)
		{
			if (occurrence == XmlPortNodeOccurrence.Required)
			{
				return use == XmlSchemaUse.Required;
			}
			return false;
		}
		return true;
	}

	private static XmlSchemaAttribute CreateAttribute(string attributeName, XmlPortNodeOccurrence occurrence)
	{
		XmlSchemaAttribute xmlSchemaAttribute = new XmlSchemaAttribute();
		xmlSchemaAttribute.Name = attributeName;
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
}
