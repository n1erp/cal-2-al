using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavTableConnectionTypeEvaluator : NavEnumTypeEvaluator<TableConnectionType, NavTableConnectionType>
{
	private static readonly NavTableConnectionTypeEvaluator instance = new NavTableConnectionTypeEvaluator();

	internal static NavTableConnectionTypeEvaluator Instance => instance;

	private NavTableConnectionTypeEvaluator()
		: base((Func<TableConnectionType, NavTableConnectionType>)((TableConnectionType TableConnectionType) => new NavTableConnectionType(TableConnectionType)))
	{
	}

	protected override NavTableConnectionType GetDefaultValue(INavValueMetadata metadata)
	{
		return NavTableConnectionType.Default;
	}
}
