using System;
using System.Xml;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INCLObjectXmlMetadataLoader
{
	NCLObjectXmlMetadata GetMetaObjectAsXmlDocument(ApplicationObjectId objectId, NavAppGroup appGroup);

	NCLObjectXmlMetadata GetUnmodifiedBaseObjectXml(ApplicationObjectId objectId);

	Tuple<XmlDocument, int> GetMetaObjectAsXmlDocument(ObjectType navObjectType, string filter, bool returnFirst);

	NavAppObjectMetadataTimestampRecord<NavAppObjectMetadataRuntimeDeltas> GetExtensionDeltasForAppObject(ApplicationObjectId objectId, NavAppRuntimeMetadata runtimeAppMetadata);
}
