using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PageBackgroundChildSessionTask : NavChildSessionTask
{
	private readonly PageBackgroundTask task;

	public PageBackgroundChildSessionTask(PageBackgroundTask task)
	{
		this.task = task ?? throw new ArgumentNullException("task");
	}

	public override bool BeforeRunTask()
	{
		if (task.TaskCompletionTrigger == null)
		{
			throw new InvalidOperationException("PageBackgroundTask must have completion trigger property set.");
		}
		return true;
	}

	public override void RunTaskInChildSession()
	{
		NavSession session = NavCurrentThread.Session;
		session.PageBackgroundTask = task;
		try
		{
			using NavCodeunitHandle navCodeunitHandle = task.CodeunitHandleFactory(session);
			navCodeunitHandle.Target.Run();
		}
		finally
		{
			session.PageBackgroundTask = null;
		}
	}

	public override void AfterRunTask()
	{
		task.TaskCompletionTrigger.RaiseCompletionTrigger();
	}

	public override void AfterRunTaskError(NavChildSessionTaskError childSessionTaskError)
	{
		task.Error = childSessionTaskError;
		task.Results = NavDictionary<NavText, NavText>.Default;
		task.TaskCompletionTrigger.RaiseCompletionTrigger();
	}
}
