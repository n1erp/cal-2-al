using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public interface INavXmlDeltaConverter
{
	ConverterResult Convert(MasterPage masterPageTemplate, PageDefinition targetDefinition, MasterPage masterPage, NavXmlDelta delta);
}
