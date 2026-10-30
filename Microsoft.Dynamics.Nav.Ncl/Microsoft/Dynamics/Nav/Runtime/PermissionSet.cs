using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PermissionSet : PermissionSetBase
{
	public PermissionSet(ApplicationObjectId objectId, NavApplicationObjectBase callingObject, PermissionManager permissionManager)
		: base(objectId, callingObject, permissionManager)
	{
	}

	protected override PermissionMask FetchPermissions()
	{
		return PermissionManager.GetPermissions(ObjectId);
	}
}
