using System;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavRoleKey : Tuple<string, PermissionScope, AppId>
{
	public string Id => Item1;

	public PermissionScope Scope => Item2;

	public AppId AppId => Item3;

	public NavRoleKey(string roleId, PermissionScope scope, AppId appId)
		: base(roleId, scope, appId)
	{
	}
}
