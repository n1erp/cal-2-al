using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class EnumTypeBaseMetadataEmitter<T> : ObjectMetadataEmitter<T> where T : ApplicationObjectTypeSymbol
{
	public EnumTypeBaseMetadataEmitter(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	protected void WriteEnumValue(XmlWriter xmlWriter, EnumValueSymbol value)
	{
		xmlWriter.WriteStartElement("Value");
		xmlWriter.WriteAttributeString("Name", value.Name);
		xmlWriter.WriteAttributeString("Ordinal", value.Ordinal.ToString(CultureInfo.InvariantCulture));
		WriteProperties(xmlWriter, value, value.Properties, shouldOutputDefaultValues: false);
		xmlWriter.WriteEndElement();
	}
}
