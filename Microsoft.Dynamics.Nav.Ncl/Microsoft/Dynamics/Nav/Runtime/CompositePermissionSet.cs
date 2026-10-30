using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class CompositePermissionSet : IPermissionSet
{
	internal readonly HashSet<IPermissionSet> PermissionSets = new HashSet<IPermissionSet>();

	public void AddPermissionSet(IPermissionSet permissionSet)
	{
		if (permissionSet == null)
		{
			throw new ArgumentNullException("permissionSet");
		}
		PermissionSets.Add(permissionSet);
	}

	public bool HasPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true, PermissionValidationCategory validationCategory = PermissionValidationCategory.Normal)
	{
		return InternalHasPermisisons(securableObject, permissionMask, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable, validationCategory);
	}

	public void VerifyPermissions(NavApplicationObjectBase securableObject, PermissionMask permissionMask, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty = null, PermissionMask additionalIndirectPermissions = PermissionMask.None, bool countAsLimitedUserAllowedTable = true)
	{
		InternalVerifyPermisisons(securableObject, permissionMask, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable);
	}

	private bool InternalHasPermisisons(NavApplicationObjectBase securableObject, PermissionMask permissions, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable, PermissionValidationCategory validationCategory)
	{
		return PermissionSets.All((IPermissionSet permissionSet) => permissionSet.HasPermissions(securableObject, permissions, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable, validationCategory));
	}

	private void InternalVerifyPermisisons(NavApplicationObjectBase securableObject, PermissionMask permissions, LimitedUserPermissionChecker limitedUserPermissionChecker, Func<int, bool> checkTableEmpty, PermissionMask additionalIndirectPermissions, bool countAsLimitedUserAllowedTable)
	{
		StringBuilder stringBuilder = null;
		int num = 0;
		foreach (IPermissionSet permissionSet in PermissionSets)
		{
			try
			{
				permissionSet.VerifyPermissions(securableObject, permissions, limitedUserPermissionChecker, checkTableEmpty, additionalIndirectPermissions, countAsLimitedUserAllowedTable);
			}
			catch (NavPermissionException ex)
			{
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				stringBuilder.AppendLine(ex.Message);
				if (num == 0)
				{
					num = ex.ErrorCode;
				}
			}
		}
		if (stringBuilder != null)
		{
			throw new NavPermissionException(num, stringBuilder.ToString());
		}
	}
}
