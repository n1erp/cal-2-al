using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavExecutionModeEvaluator : NavEnumTypeEvaluator<ExecutionMode, NavExecutionMode>
{
	private static readonly NavExecutionModeEvaluator instance = new NavExecutionModeEvaluator();

	internal static NavExecutionModeEvaluator Instance => instance;

	private NavExecutionModeEvaluator()
		: base((Func<ExecutionMode, NavExecutionMode>)((ExecutionMode executionMode) => new NavExecutionMode(executionMode)))
	{
	}

	protected override NavExecutionMode GetDefaultValue(INavValueMetadata metadata)
	{
		return NavExecutionMode.Default;
	}
}
