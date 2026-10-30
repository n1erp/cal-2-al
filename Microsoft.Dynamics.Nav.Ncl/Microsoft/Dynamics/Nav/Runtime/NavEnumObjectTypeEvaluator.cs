using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavEnumObjectTypeEvaluator : NavEnumTypeEvaluator<NavObjectType, NavEnumObjectType>
{
	private static readonly NavEnumObjectTypeEvaluator instance = new NavEnumObjectTypeEvaluator();

	internal static NavEnumObjectTypeEvaluator Instance => instance;

	private NavEnumObjectTypeEvaluator()
		: base((Func<NavObjectType, NavEnumObjectType>)((NavObjectType objectType) => new NavEnumObjectType(objectType)))
	{
	}

	protected override NavEnumObjectType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavEnumObjectType.Default;
	}
}
