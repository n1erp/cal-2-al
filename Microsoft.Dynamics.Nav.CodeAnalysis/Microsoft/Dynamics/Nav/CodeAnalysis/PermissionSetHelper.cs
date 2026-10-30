using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal static class PermissionSetHelper
{
	internal static PermissionMask GetPermissionMask(this string permission)
	{
		if (string.IsNullOrEmpty(permission))
		{
			return PermissionMask.None;
		}
		PermissionMask permissionMask = PermissionMask.None;
		for (int i = 0; i < permission.Length; i++)
		{
			permissionMask = char.ToUpperInvariant(permission[i]) switch
			{
				'M' => permissionMask | PermissionMask.Modify, 
				'R' => permissionMask | PermissionMask.Read, 
				'X' => permissionMask | PermissionMask.Execute, 
				'I' => permissionMask | PermissionMask.Insert, 
				'D' => permissionMask | PermissionMask.Delete, 
				_ => throw ExceptionUtilities.UnexpectedValue(permission), 
			};
		}
		return permissionMask;
	}
}
