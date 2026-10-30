using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TaskScheduleAppAction
{
	private Action<NavSqlConnectionScope> action;

	internal TaskScheduleAppAction(Action<NavSqlConnectionScope> action)
	{
		this.action = action;
	}

	internal void Execute(NavSqlConnectionScope conScope)
	{
		action(conScope);
	}
}
