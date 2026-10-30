using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NCLObjectMetadataCacheKeyComparer : IEqualityComparer<NCLObjectMetadataCacheKey>
{
	public bool Equals(NCLObjectMetadataCacheKey x, NCLObjectMetadataCacheKey y)
	{
		if (x != y)
		{
			if (x?.ObjectType == y?.ObjectType && x?.ObjectId == y?.ObjectId && x?.ObjectVersion == y?.ObjectVersion && x?.Timestamp == y?.Timestamp)
			{
				return string.Equals(x?.Hash?.Value, y?.Hash?.Value, StringComparison.Ordinal);
			}
			return false;
		}
		return true;
	}

	public int GetHashCode(NCLObjectMetadataCacheKey obj)
	{
		return (int)((uint)obj.ObjectType ^ (uint)obj.ObjectId ^ (uint)obj.ObjectVersion ^ (uint)obj.Timestamp.GetHashCode()) ^ obj.Hash.GetHashCode();
	}
}
