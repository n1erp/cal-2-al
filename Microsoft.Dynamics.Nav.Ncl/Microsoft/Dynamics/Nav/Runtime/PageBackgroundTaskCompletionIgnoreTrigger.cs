using System;
using System.Globalization;
using Microsoft.Dynamics.Nav.Diagnostic;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PageBackgroundTaskCompletionIgnoreTrigger : IPageBackgroundTaskCompletionTrigger
{
	private readonly PageBackgroundTask task;

	public PageBackgroundTaskCompletionIgnoreTrigger(PageBackgroundTask task)
	{
		this.task = task ?? throw new ArgumentNullException("task");
	}

	public void RaiseCompletionTrigger()
	{
		DiagnosticsResolver.GetMostSpecificInstance(NavCurrentThread.Session).SendTraceTag("00007TW", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Pagebackground task:" + task.TaskId + " completed for codeunit:" + task.CodeunitId + " has completed with success:" + (task.Error != null).ToString(CultureInfo.InvariantCulture) + ". However the current session configuration is set to not invoke the completion triggers. The result of the PageBackgroundtask is discarded.");
	}
}
