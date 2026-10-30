using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumTestPermissionsEvaluator : NavEnumTypeEvaluator<NavTestPermissions, NavEnumTestPermissions>
{
	private static readonly NavEnumTestPermissionsEvaluator instance = new NavEnumTestPermissionsEvaluator();

	internal static NavEnumTestPermissionsEvaluator Instance => instance;

	private NavEnumTestPermissionsEvaluator()
		: base((Func<NavTestPermissions, NavEnumTestPermissions>)((NavTestPermissions objectType) => new NavEnumTestPermissions(objectType)))
	{
	}

	protected override NavEnumTestPermissions GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumTestPermissions.Default;
	}
}
