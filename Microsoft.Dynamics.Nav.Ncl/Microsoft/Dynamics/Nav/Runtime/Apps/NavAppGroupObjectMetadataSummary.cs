using Microsoft.Dynamics.Nav.Apps.Runtime;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppGroupObjectMetadataSummary
{
	public NavAppGroup MetadataAppGroup { get; }

	public NavAppRuntimeMetadata FullObjectOwner { get; }

	public ObjectType ObjectType { get; }

	public int ObjectId { get; }

	public string ObjectName { get; }

	public bool OriginatesFromBase => FullObjectOwner == null;

	public static NavAppGroupObjectMetadataSummary CreateForKnownGroup(NavAppGroup metadataAppGroup, NavAppRuntimeMetadata fullObjectOwner, ObjectType objectType, int objectId, string objectName)
	{
		return new NavAppGroupObjectMetadataSummary(metadataAppGroup, fullObjectOwner, objectType, objectId, objectName);
	}

	public static NavAppGroupObjectMetadataSummary CreateForFutureGroup(NavAppRuntimeMetadata fullObjectOwner, ObjectType objectType, int objectId, string objectName)
	{
		return new NavAppGroupObjectMetadataSummary(null, fullObjectOwner, objectType, objectId, objectName);
	}

	private NavAppGroupObjectMetadataSummary(NavAppGroup metadataAppGroup, NavAppRuntimeMetadata fullObjectOwner, ObjectType objectType, int objectId, string objectName)
	{
		MetadataAppGroup = metadataAppGroup;
		FullObjectOwner = fullObjectOwner;
		ObjectType = objectType;
		ObjectId = objectId;
		ObjectName = objectName;
	}
}
