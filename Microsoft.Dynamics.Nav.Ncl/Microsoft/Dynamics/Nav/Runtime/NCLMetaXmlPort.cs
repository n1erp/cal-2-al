using System;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaXmlPort : NCLMetaApplicationObject
{
	private delegate NavXmlPort CreateNavXmlPort(ITreeObject parent, NCLMetaXmlPort metaXmlPort);

	private delegate NavXmlPort CreateLegacyNavXmlPort(ITreeObject parent);

	private NCLCaptionStrings captionML;

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavXmlPort);

	protected override Type ApplicationObjectLegacyConstructorDelegateType => typeof(CreateLegacyNavXmlPort);

	public int XmlPortId => ObjectId;

	public string Name { get; private set; }

	public string Caption => captionML.GetValueOrDefault();

	public XmlDocument XmlMetadata => GetMetadataFromLoader().Document;

	private NCLMetaXmlPort(INCLMetaApplicationObjectLoader loader, int xmlPortId, NavAppGroup metadataAppGroup, int appDependencyOrder)
		: base(loader, ObjectType.XmlPort, xmlPortId, metadataAppGroup, appDependencyOrder)
	{
	}

	internal static NCLMetaXmlPort CreateEmptyNCLMetaXmlPort(INCLMetaApplicationObjectLoader loader, int xmlPortId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaXmlPort(loader, xmlPortId, appGroup, appDependencyOrder);
	}

	public NavXmlPort CreateObjectInstance(ITreeObject parent)
	{
		Delegate obj = ApplicationObjectConstructor;
		if (!ApplicationObjectLegacyConstructor)
		{
			return ((CreateNavXmlPort)obj)(parent, this);
		}
		return ((CreateLegacyNavXmlPort)obj)(parent);
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		NCLObjectXmlMetadata metadataFromLoader = GetMetadataFromLoader();
		Timestamp = metadataFromLoader.Timestamp;
		MetaXmlPort metaXmlPort = new MetaXmlPort(metadataFromLoader.Document, null, MetadataAppGroup.GroupId, NavCurrentThread.ResolveAppGroup().GroupId);
		Name = metaXmlPort.Name;
		captionML = new NCLCaptionStrings(metaXmlPort.CaptionML.ToArray(), Name);
	}
}
