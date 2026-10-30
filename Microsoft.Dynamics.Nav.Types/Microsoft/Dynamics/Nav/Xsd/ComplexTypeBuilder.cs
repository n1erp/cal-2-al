using System;
using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;

namespace Microsoft.Dynamics.Nav.Xsd;

public class ComplexTypeBuilder : BaseXsdBuilder
{
	private XmlSchemaComplexType type;

	private XmlSchemaSequence typeSequence;

	private bool useLax;

	internal XmlSchemaComplexType Type
	{
		[DebuggerStepThrough]
		get
		{
			return type;
		}
	}

	public bool IsMixed
	{
		[DebuggerStepThrough]
		get
		{
			return type.IsMixed;
		}
		[DebuggerStepThrough]
		set
		{
			type.IsMixed = value;
		}
	}

	public ComplexTypeBuilder(BaseXsdBuilder parent, string name)
		: this(parent)
	{
		type.Name = name;
	}

	public ComplexTypeBuilder(BaseXsdBuilder parent, XmlSchemaComplexType type)
		: base(parent)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		this.type = type;
		typeSequence = (XmlSchemaSequence)type.Particle;
	}

	public ComplexTypeBuilder(BaseXsdBuilder parent)
		: base(parent)
	{
		type = new XmlSchemaComplexType();
		typeSequence = new XmlSchemaSequence();
		type.Particle = typeSequence;
	}

	public ComplexTypeBuilder(BaseXsdBuilder parent, string name, bool useLax)
		: this(parent, useLax)
	{
		type.Name = name;
	}

	public ComplexTypeBuilder(BaseXsdBuilder parent, bool useLax)
		: this(parent)
	{
		this.useLax = useLax;
		AddLaxElement();
	}

	private void AddLaxElement()
	{
		if (useLax)
		{
			XmlSchemaAny xmlSchemaAny = new XmlSchemaAny();
			xmlSchemaAny.MinOccurs = 0m;
			xmlSchemaAny.MaxOccursString = "unbounded";
			xmlSchemaAny.Namespace = "##other";
			xmlSchemaAny.ProcessContents = XmlSchemaContentProcessing.Lax;
			typeSequence.Items.Add(xmlSchemaAny);
		}
	}

	public void AddAttribute(XmlSchemaAttribute attribute)
	{
		type.Attributes.Add(attribute);
	}

	public void AddElement(string name, ElementNumber cardinality, XmlQualifiedName typeName)
	{
		XmlSchemaElement xmlSchemaElement = base.CreateElement(name, cardinality, null);
		xmlSchemaElement.SchemaTypeName = typeName;
		typeSequence.Items.Add(xmlSchemaElement);
		AddLaxElement();
	}

	public override XmlSchemaElement CreateElement(string name, ElementNumber cardinality, ElementType elementDelegate)
	{
		XmlSchemaElement xmlSchemaElement = base.CreateElement(name, cardinality, elementDelegate);
		typeSequence.Items.Add(xmlSchemaElement);
		AddLaxElement();
		return xmlSchemaElement;
	}

	public override XmlSchemaElement CreateElement(string name, string namespaceUri, ElementNumber cardinality, ElementType elementDelegate)
	{
		XmlSchemaElement xmlSchemaElement = base.CreateElement(name, namespaceUri, cardinality, elementDelegate);
		typeSequence.Items.Add(xmlSchemaElement);
		AddLaxElement();
		return xmlSchemaElement;
	}
}
