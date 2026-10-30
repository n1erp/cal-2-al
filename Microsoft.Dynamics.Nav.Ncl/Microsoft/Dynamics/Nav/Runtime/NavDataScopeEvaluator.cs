using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavDataScopeEvaluator : NavEnumTypeEvaluator<DataScope, NavEnumDataScope>
{
	private static readonly NavDataScopeEvaluator instance = new NavDataScopeEvaluator();

	internal static NavDataScopeEvaluator Instance => instance;

	private NavDataScopeEvaluator()
		: base((Func<DataScope, NavEnumDataScope>)((DataScope datascope) => new NavEnumDataScope(datascope)))
	{
	}

	protected override NavEnumDataScope GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumDataScope.Default;
	}
}
