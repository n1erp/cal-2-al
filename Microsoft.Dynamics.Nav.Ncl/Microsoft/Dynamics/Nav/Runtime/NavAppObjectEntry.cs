namespace Microsoft.Dynamics.Nav.Runtime;

public struct NavAppObjectEntry(int groupId, int metadataGroupId, NCLMetaApplicationObject groupObject)
{
	private readonly int groupId = groupId;

	private readonly int metadataGroupId = metadataGroupId;

	private readonly NCLMetaApplicationObject groupObject = groupObject;

	public int GroupId => groupId;

	public int MetadataGroupId => metadataGroupId;

	public NCLMetaApplicationObject GroupObject => groupObject;

	public override bool Equals(object obj)
	{
		if (!(obj is NavAppObjectEntry navAppObjectEntry))
		{
			return false;
		}
		if (GroupId == navAppObjectEntry.GroupId)
		{
			return GroupObject == navAppObjectEntry.GroupObject;
		}
		return false;
	}

	public static bool operator ==(NavAppObjectEntry entry1, NavAppObjectEntry entry2)
	{
		return entry1.Equals(entry2);
	}

	public static bool operator !=(NavAppObjectEntry entry1, NavAppObjectEntry entry2)
	{
		return !(entry1 == entry2);
	}

	public override int GetHashCode()
	{
		return GroupId ^ GroupObject.GetHashCode();
	}
}
