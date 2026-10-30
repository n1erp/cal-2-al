using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumDefaultLayoutEvaluator : NavEnumTypeEvaluator<NavDefaultLayout, NavEnumDefaultLayout>
{
	private static readonly NavEnumDefaultLayoutEvaluator instance = new NavEnumDefaultLayoutEvaluator();

	internal static NavEnumDefaultLayoutEvaluator Instance => instance;

	private NavEnumDefaultLayoutEvaluator()
		: base((Func<NavDefaultLayout, NavEnumDefaultLayout>)((NavDefaultLayout objectType) => new NavEnumDefaultLayout(objectType)))
	{
	}

	protected override NavEnumDefaultLayout GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumDefaultLayout.Default;
	}
}
