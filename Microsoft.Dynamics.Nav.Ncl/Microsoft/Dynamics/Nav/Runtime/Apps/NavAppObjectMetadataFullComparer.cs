using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Apps.AppObjects;
using Microsoft.Dynamics.Nav.Apps.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class NavAppObjectMetadataFullComparer : IEqualityComparer<NavAppObjectMetadata>
{
	public bool Equals(NavAppObjectMetadata x, NavAppObjectMetadata y)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if ((WrappedComparable<Guid>)(object)x.AppPackageId == (WrappedComparable<Guid>)(object)y.AppPackageId && x.ObjectType == y.ObjectType && x.ObjectId == y.ObjectId && x.Format == y.Format && x.MetadataVersion == y.MetadataVersion && x.ObjectSubtype == y.ObjectSubtype && x.Metadata == y.Metadata && x.UserCode == y.UserCode)
		{
			return x.UserALCode == y.UserALCode;
		}
		return false;
	}

	public int GetHashCode(NavAppObjectMetadata obj)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		return HashCodeHelper.CombineHashCodes(((object)obj.AppPackageId).GetHashCode(), obj.ObjectType.GetHashCode(), obj.ObjectId, ((object)obj.Format/*cast due to constrained. prefix*/).GetHashCode(), obj.MetadataVersion);
	}
}
