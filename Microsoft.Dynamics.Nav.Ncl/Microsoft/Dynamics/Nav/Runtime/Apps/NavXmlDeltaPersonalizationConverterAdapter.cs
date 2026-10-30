using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavXmlDeltaPersonalizationConverterAdapter : INavXmlDeltaConverter
{
	public ConverterResult Convert(MasterPage masterPageTemplate, PageDefinition targetDefinition, MasterPage masterPage, NavXmlDelta delta)
	{
		XmlDocument masterPageTemplate2 = SerializeObject(MetadataProvider.MasterPageSerializer.Value, masterPageTemplate);
		XmlDocument masterPageDocument = SerializeObject(MetadataProvider.MasterPageSerializer.Value, masterPage);
		XmlDocument pageDefinition = SerializeObject(MetadataProvider.PageDefinitionSerializer.Value, targetDefinition);
		return Converter.ProcessDelta(masterPageDocument, masterPageTemplate2, pageDefinition, delta, targetDefinition.Name);
	}

	private static XmlDocument SerializeObject(XmlSerializer serializer, object obj)
	{
		XmlDocument xmlDocument = XmlHelper.SafeXmlDocument();
		using XmlWriter xmlWriter = xmlDocument.CreateNavigator().AppendChild();
		serializer.Serialize(xmlWriter, obj, new XmlSerializerNamespaces());
		return xmlDocument;
	}
}
