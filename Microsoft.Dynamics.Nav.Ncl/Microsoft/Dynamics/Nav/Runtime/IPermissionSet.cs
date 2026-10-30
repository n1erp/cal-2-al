using System;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface IPermissionSet
{
	bool HasPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal);

	void VerifyPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true);
}
