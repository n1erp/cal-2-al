using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class NavChildSessionTaskRuntimeHelper
{
	public static void ValidateTaskCanStart(this NavChildSessionTask task)
	{
		if (!((task.Status == NavChildSessionTaskStatus.Created) | (task.Status == NavChildSessionTaskStatus.Queued)))
		{
			throw new InvalidOperationException("The task can't be run twice, or is currently running. Task status: " + task.Status);
		}
	}
}
