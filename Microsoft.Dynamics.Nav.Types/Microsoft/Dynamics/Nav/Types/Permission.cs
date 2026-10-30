using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types;

internal class Permission
{
	internal PermissionOption Read;

	internal PermissionOption Insert;

	internal PermissionOption Modify;

	internal PermissionOption Delete;

	internal PermissionOption Execute;

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4}", (int)Read, (int)Insert, (int)Modify, (int)Delete, (int)Execute);
	}
}
