using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppObjectMetadataByKeyComparer : IEqualityComparer<NavAppObjectMetadata>
{
	public bool Equals(NavAppObjectMetadata x, NavAppObjectMetadata y)
	{
		if ((WrappedComparable<Guid>)(object)x.AppPackageId == (WrappedComparable<Guid>)(object)y.AppPackageId && x.ObjectType == y.ObjectType)
		{
			return x.ObjectId == y.ObjectId;
		}
		return false;
	}

	public int GetHashCode(NavAppObjectMetadata obj)
	{
		return ((object)obj.AppPackageId).GetHashCode() ^ obj.ObjectType.GetHashCode() ^ obj.ObjectId.GetHashCode();
	}
}
