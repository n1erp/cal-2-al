using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavExecutionContext : NavEnumBase<ExecutionContext>
{
	public override NavNclType NclType { get; } = NavNclType.NavExecutionContext;

	public static NavExecutionContext Default { get; } = new NavExecutionContext(ExecutionContext.Normal);

	public NavExecutionContext(ExecutionContext initValue)
		: base(initValue)
	{
	}

	public NavExecutionContext(byte[] value)
		: base((ExecutionContext)BitConverter.ToInt32(value, 0))
	{
	}

	public static implicit operator NavExecutionContext(ExecutionContext value)
	{
		return new NavExecutionContext(value);
	}

	public static implicit operator ExecutionContext(NavExecutionContext value)
	{
		return value.Value;
	}
}
