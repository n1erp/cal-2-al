using System.Xml.Schema;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Xsd;

public class EnumTypeBuilder
{
	public delegate void BuildEnumType(EnumTypeBuilder builder);

	private XmlSchemaSimpleType simpleType;

	private XmlSchemaSimpleTypeRestriction restriction;

	internal XmlSchemaSimpleType Type => simpleType;

	internal EnumTypeBuilder()
	{
		simpleType = new XmlSchemaSimpleType();
		restriction = new XmlSchemaSimpleTypeRestriction();
		restriction.BaseTypeName = XmlTypeHelper.XmlTypeNameFrom(NavType.Text);
		simpleType.Content = restriction;
	}

	internal EnumTypeBuilder(string typeName)
		: this()
	{
		simpleType.Name = typeName;
	}

	public void AddOption(string option)
	{
		XmlSchemaEnumerationFacet xmlSchemaEnumerationFacet = new XmlSchemaEnumerationFacet();
		xmlSchemaEnumerationFacet.Value = option;
		restriction.Facets.Add(xmlSchemaEnumerationFacet);
	}
}
