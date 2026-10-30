using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PageBackgroundTaskCompletionImmediateTrigger : IPageBackgroundTaskCompletionTrigger
{
	private readonly PageBackgroundTask task;

	public PageBackgroundTaskCompletionImmediateTrigger(PageBackgroundTask task)
	{
		this.task = task ?? throw new ArgumentNullException("task");
	}

	public void RaiseCompletionTrigger()
	{
		if (!task.Form.TryGetTarget(out var target) || target.IsDisposed)
		{
			DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("00007TV", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Pagebackground task:" + task.TaskId + " completed for codeunit:" + task.CodeunitId + " has completed with success:" + (task.Error != null).ToString(CultureInfo.InvariantCulture) + ". However the form \"" + task.FormHandle.ToString() + "\" has been disposed, so completion triggers cannot be invoked. Discarding result of PageBackgroundtask.");
		}
		else if (task.Error != null)
		{
			target.RaiseOnPageBackgroundTaskError(task.TaskId, new NavText(task.Error.ErrorCode), new NavText(NavDialog.ReplaceCRLFWithBackSlash(task.Error.ErrorText)), new NavText(task.Error.ErrorCallstack));
		}
		else
		{
			target.RaiseOnPageBackgroundTaskCompleted(task.TaskId, task.Results);
		}
	}
}
