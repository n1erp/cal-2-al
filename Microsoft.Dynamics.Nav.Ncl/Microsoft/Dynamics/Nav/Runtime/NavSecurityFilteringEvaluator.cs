using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSecurityFilteringEvaluator : NavEnumTypeEvaluator<SecurityFiltering, NavSecurityFiltering>
{
	private static readonly NavSecurityFilteringEvaluator instance = new NavSecurityFilteringEvaluator();

	internal static NavSecurityFilteringEvaluator Instance => instance;

	private NavSecurityFilteringEvaluator()
		: base((Func<SecurityFiltering, NavSecurityFiltering>)((SecurityFiltering securityFiltering) => new NavSecurityFiltering(securityFiltering)))
	{
	}

	protected override NavSecurityFiltering GetDefaultValue(INavValueMetadata metadata)
	{
		return NavSecurityFiltering.Default;
	}
}
