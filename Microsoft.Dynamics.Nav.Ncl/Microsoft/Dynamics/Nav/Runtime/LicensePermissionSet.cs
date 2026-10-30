using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class LicensePermissionSet : PermissionSetBase
{
	public LicensePermissionSet(ApplicationObjectId objectId, NavApplicationObjectBase callingObject, PermissionManager permissionManager)
		: base(objectId, callingObject, permissionManager)
	{
	}

	protected override PermissionMask FetchPermissions()
	{
		PermissionMask licensePermissions = PermissionManager.GetLicensePermissions(ObjectId);
		if (PermissionManager.HasAdminUserPermissions)
		{
			return licensePermissions;
		}
		return PermissionManager.ApplyEntitlementPermission(licensePermissions, NavCurrentThread.Session.User, ObjectId);
	}
}
