using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavExecutionContextEvaluator : NavEnumTypeEvaluator<ExecutionContext, NavExecutionContext>
{
	internal static NavExecutionContextEvaluator Instance { get; } = new NavExecutionContextEvaluator();

	private NavExecutionContextEvaluator()
		: base((Func<ExecutionContext, NavExecutionContext>)((ExecutionContext executionContext) => new NavExecutionContext(executionContext)))
	{
	}

	protected override NavExecutionContext GetDefaultValue(INavValueMetadata metadata)
	{
		return NavExecutionContext.Default;
	}
}
