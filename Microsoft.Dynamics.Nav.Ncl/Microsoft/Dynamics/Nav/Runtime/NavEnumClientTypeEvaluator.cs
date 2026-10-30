using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumClientTypeEvaluator : NavEnumTypeEvaluator<NavClientType, NavEnumClientType>
{
	private static readonly NavEnumClientTypeEvaluator instance = new NavEnumClientTypeEvaluator();

	internal static NavEnumClientTypeEvaluator Instance => instance;

	private NavEnumClientTypeEvaluator()
		: base((Func<NavClientType, NavEnumClientType>)((NavClientType clientType) => new NavEnumClientType(clientType)))
	{
	}

	protected override NavEnumClientType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumClientType.Default;
	}
}
