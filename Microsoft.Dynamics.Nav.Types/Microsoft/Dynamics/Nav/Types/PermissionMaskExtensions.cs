namespace Microsoft.Dynamics.Nav.Types;

public static class PermissionMaskExtensions
{
	public static PermissionMask Combine(this PermissionMask permission, PermissionMask other)
	{
		return (PermissionMask)((uint)(permission & other) | ((uint)((int)other << 5) & (uint)permission) | ((uint)((int)permission << 5) & (uint)other));
	}
}
