using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavExecutionMode : NavEnumBase<ExecutionMode>
{
	private static NavExecutionMode defaultValue = new NavExecutionMode(ExecutionMode.Standard);

	public override NavNclType NclType => NavNclType.NavExecutionMode;

	public static NavExecutionMode Default => defaultValue;

	public NavExecutionMode(ExecutionMode initValue)
		: base(initValue)
	{
	}

	public NavExecutionMode(byte[] bytes)
		: base((ExecutionMode)BitConverter.ToInt32(bytes, 0))
	{
	}
}
