using System.Xml;
using System.Xml.Schema;

namespace Microsoft.Dynamics.Nav.Xsd;

public class ElementBuilder : BaseXsdBuilder
{
	private XmlSchemaElement element;

	public ElementNumber Cardinality
	{
		get
		{
			if (element.MinOccurs == 0m)
			{
				if (element.MinOccursString.Equals("unbounded"))
				{
					return ElementNumber.Collection;
				}
				return ElementNumber.ZeroOrOne;
			}
			return ElementNumber.One;
		}
		set
		{
			if (value != ElementNumber.None)
			{
				element.MinOccurs = 0m;
				element.MaxOccurs = 1m;
				switch (value)
				{
				case ElementNumber.Collection:
					element.MinOccurs = 1m;
					element.MaxOccursString = "unbounded";
					break;
				case ElementNumber.One:
					element.MinOccurs = 1m;
					break;
				}
			}
		}
	}

	public decimal MinOccurs
	{
		get
		{
			return element.MinOccurs;
		}
		set
		{
			element.MinOccurs = value;
		}
	}

	public decimal MaxOccurs
	{
		get
		{
			return element.MaxOccurs;
		}
		set
		{
			element.MaxOccurs = value;
		}
	}

	public string MinOccursString
	{
		get
		{
			return element.MinOccursString;
		}
		set
		{
			element.MinOccursString = value;
		}
	}

	public string MaxOccursString
	{
		get
		{
			return element.MaxOccursString;
		}
		set
		{
			element.MaxOccursString = value;
		}
	}

	public XmlQualifiedName XmlType
	{
		get
		{
			return element.SchemaTypeName;
		}
		set
		{
			element.SchemaTypeName = value;
		}
	}

	public string DefaultValue
	{
		get
		{
			return element.DefaultValue;
		}
		set
		{
			element.DefaultValue = value;
		}
	}

	public XmlSchemaElement Element => element;

	public ElementBuilder(BaseXsdBuilder parent, string name)
		: base(parent)
	{
		element = new XmlSchemaElement();
		element.Name = name;
	}

	public ElementBuilder(BaseXsdBuilder parent, string name, string namespaceUri)
		: base(parent)
	{
		element = new XmlSchemaElement();
		element.RefName = new XmlQualifiedName(name, namespaceUri);
	}

	public override XmlSchemaComplexType CreateType(string name, ComplexType complexDelegate)
	{
		element.SchemaType = base.CreateType(name, complexDelegate);
		return (XmlSchemaComplexType)element.SchemaType;
	}

	public override XmlSchemaComplexType CreateType(ComplexType complexDelegate)
	{
		element.SchemaType = base.CreateType(complexDelegate);
		return (XmlSchemaComplexType)element.SchemaType;
	}

	public void CreateEnumType(EnumTypeBuilder.BuildEnumType handler)
	{
		if (handler != null)
		{
			EnumTypeBuilder enumTypeBuilder = new EnumTypeBuilder();
			element.SchemaType = enumTypeBuilder.Type;
			handler(enumTypeBuilder);
		}
	}
}
