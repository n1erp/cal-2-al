using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavFieldTypeEvaluator : NavEnumTypeEvaluator<NavType, NavFieldType>
{
	internal static NavFieldTypeEvaluator Instance { get; } = new NavFieldTypeEvaluator();

	private NavFieldTypeEvaluator()
		: base((Func<NavType, NavFieldType>)((NavType FieldType) => new NavFieldType(FieldType)))
	{
	}

	protected override NavFieldType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavFieldType.Default;
	}
}
