using System.Globalization;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class EnumExtensionTypeMetadataEmitter : EnumTypeBaseMetadataEmitter<SourceEnumExtensionTypeSymbol>
{
	public EnumExtensionTypeMetadataEmitter(SourceEnumExtensionTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("Enum", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("ID", Symbol.Id.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("Name", Symbol.Name);
			WriteProperties(xmlWriter, Symbol, Symbol.Properties, shouldOutputDefaultValues: false);
			WriteEnumValues(xmlWriter);
			xmlWriter.WriteEndElement();
		});
	}

	private void WriteEnumValues(XmlWriter xmlWriter)
	{
		foreach (EnumValueSymbol value in Symbol.Values)
		{
			WriteEnumValue(xmlWriter, value);
		}
	}
}
