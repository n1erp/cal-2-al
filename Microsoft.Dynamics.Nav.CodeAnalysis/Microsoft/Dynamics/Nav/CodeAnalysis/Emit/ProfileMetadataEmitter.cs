using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class ProfileMetadataEmitter : ObjectMetadataEmitter<SourceProfileTypeSymbol>
{
	public ProfileMetadataEmitter(SourceProfileTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	internal override void EmitMetadata(DiagnosticBag diagnostics)
	{
		EmitMetadata((XmlWriter xmlWriter) =>
		{
			xmlWriter.WriteStartElement("Profile", "urn:schemas-microsoft-com:dynamics:NAV:MetaObjects");
			xmlWriter.WriteAttributeString("MetadataVersion", ModuleBuilder.Options.RuntimeMetadataVersion.ToString(CultureInfo.InvariantCulture));
			WriteProfileHeader(xmlWriter);
			WriteProfilePageExtensionsElement(xmlWriter);
			xmlWriter.WriteEndElement();
		});
	}

	private void WriteProfileHeader(XmlWriter xmlWriter)
	{
		xmlWriter.WriteAttributeString("ProfileID", Symbol.Name);
		foreach (PropertySymbol item in Symbol.Properties.Where((PropertySymbol p) => !p.MetadataName.Equals("PageExtensions")))
		{
			PropertyKind propertyKind = item.PropertyKind;
			if (propertyKind == PropertyKind.RoleCenter)
			{
				xmlWriter.WriteAttributeString(item.MetadataName, (item.Value as PageTypeSymbol)?.Id.ToString(CultureInfo.InvariantCulture) ?? string.Empty);
			}
			else
			{
				xmlWriter.WriteAttributeString(item.MetadataName, item.Value.ToString());
			}
		}
	}

	private void WriteProfilePageExtensionsElement(XmlWriter xmlWriter)
	{
		PropertySymbol propertySymbol = Symbol.Properties.FirstOrDefault((PropertySymbol p) => p.MetadataName.Equals("PageExtensions"));
		xmlWriter.WriteStartElement("PageExtensions");
		if (propertySymbol != null)
		{
			foreach (object item in (propertySymbol.Value as IEnumerable<object>) ?? Enumerable.Empty<object>())
			{
				BoundApplicationObjectAccess boundApplicationObjectAccess = item as BoundApplicationObjectAccess;
				xmlWriter.WriteStartElement("PageExtension");
				xmlWriter.WriteAttributeString("Name", boundApplicationObjectAccess.ApplicationObjectTypeSymbol.Name);
				xmlWriter.WriteEndElement();
			}
		}
		xmlWriter.WriteEndElement();
	}

	protected override void SetObjectMetadata(IObjectTypeSymbol symbol, MemoryStream outputStream)
	{
		ModuleBuilder.SetObjectMetadata(Symbol, outputStream, Symbol.DeclaringSyntaxReferences.Single().GetSyntax().ToString());
	}
}
