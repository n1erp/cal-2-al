using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

public static class PermissionHelper
{
	public static PermissionMask GetPermissionMaskFromPermissionOption(PermissionOption permissionOption, PermissionMask basePermissionMask)
	{
		return permissionOption switch
		{
			PermissionOption.Yes => (PermissionMask)((int)basePermissionMask | ((int)basePermissionMask << 5)), 
			PermissionOption.Indirect => (PermissionMask)((int)basePermissionMask << 5), 
			_ => PermissionMask.None, 
		};
	}

	public static PermissionOption GetPermissionOptionFromPermissionMask(PermissionMask permissionMask, PermissionMask basePermissionMask)
	{
		switch (permissionMask & basePermissionMask)
		{
		case PermissionMask.Read:
		case PermissionMask.Insert:
		case PermissionMask.Modify:
		case PermissionMask.Delete:
		case PermissionMask.Execute:
		case PermissionMask.Read | PermissionMask.IndirectRead:
		case PermissionMask.Insert | PermissionMask.IndirectInsert:
		case PermissionMask.Modify | PermissionMask.IndirectModify:
		case PermissionMask.Delete | PermissionMask.IndirectDelete:
		case PermissionMask.Execute | PermissionMask.IndirectExecute:
			return PermissionOption.Yes;
		case PermissionMask.IndirectRead:
		case PermissionMask.IndirectInsert:
		case PermissionMask.IndirectModify:
		case PermissionMask.IndirectDelete:
		case PermissionMask.IndirectExecute:
			return PermissionOption.Indirect;
		default:
			return PermissionOption.No;
		}
	}

	public static bool HasExecutePermission(PermissionMask permission)
	{
		return (permission & (PermissionMask.Execute | PermissionMask.IndirectExecute)) != 0;
	}

	public static bool HasInsertPermission(PermissionMask permission)
	{
		return (permission & (PermissionMask.Insert | PermissionMask.IndirectInsert)) != 0;
	}

	public static bool HasModifyPermission(PermissionMask permission)
	{
		return (permission & (PermissionMask.Modify | PermissionMask.IndirectModify)) != 0;
	}

	internal static Permission GetPermission(PermissionMask permMask)
	{
		return new Permission
		{
			Read = GetPermissionOptionFromPermissionMask(permMask, PermissionMask.Read | PermissionMask.IndirectRead),
			Insert = GetPermissionOptionFromPermissionMask(permMask, PermissionMask.Insert | PermissionMask.IndirectInsert),
			Modify = GetPermissionOptionFromPermissionMask(permMask, PermissionMask.Modify | PermissionMask.IndirectModify),
			Delete = GetPermissionOptionFromPermissionMask(permMask, PermissionMask.Delete | PermissionMask.IndirectDelete),
			Execute = GetPermissionOptionFromPermissionMask(permMask, PermissionMask.Execute | PermissionMask.IndirectExecute)
		};
	}

	public static PermissionMask GetIndirectPermissionMask(SourceObjectDefinition sourceObjectDefinition, int tableId)
	{
		if (sourceObjectDefinition == null || string.IsNullOrEmpty(sourceObjectDefinition.IndirectPermissions) || sourceObjectDefinition.IndirectPermissions.Length <= 2)
		{
			return PermissionMask.None;
		}
		uint[] array = (from x in sourceObjectDefinition.IndirectPermissions.Split(',')
			select uint.Parse(x, CultureInfo.InvariantCulture)).ToArray();
		for (uint num = 0u; num < array.Length; num += 2)
		{
			if (tableId == array[num])
			{
				return (PermissionMask)array[num + 1];
			}
		}
		return PermissionMask.None;
	}
}
