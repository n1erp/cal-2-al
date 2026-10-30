using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class GroupAndApplicationObjectIdComparer : IEqualityComparer<NCLMetaApplicationObject>
{
	public bool Equals(NCLMetaApplicationObject x, NCLMetaApplicationObject y)
	{
		if (x == null)
		{
			return y == null;
		}
		if (x.MetadataAppGroup.GroupId != y?.MetadataAppGroup.GroupId)
		{
			return false;
		}
		return x.ApplicationObjectId.Equals(y?.ApplicationObjectId);
	}

	public int GetHashCode(NCLMetaApplicationObject obj)
	{
		if (obj == null)
		{
			return 0;
		}
		return (obj.MetadataAppGroup.GroupId << 3) ^ obj.ApplicationObjectId.GetHashCode();
	}
}
