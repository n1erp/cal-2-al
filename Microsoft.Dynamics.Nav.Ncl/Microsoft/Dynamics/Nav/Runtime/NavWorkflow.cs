using System;
using System.Activities;
using System.Activities.Hosting;
using System.Collections.ObjectModel;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class NavWorkflow<TStartRequest, TResumeRequest, TStatus> : INavWorkflow<TStartRequest, TResumeRequest, TStatus> where TStartRequest : class where TResumeRequest : class where TStatus : class
{
	protected WorkflowApplication WorkflowHost { get; set; }

	public NavWorkflowState State { get; protected set; }

	public virtual void StartWorkflow(TStartRequest startRequest)
	{
		WorkflowApplication workflowHost = WorkflowHost;
		workflowHost.Aborted = (Action<WorkflowApplicationAbortedEventArgs>)Delegate.Combine(workflowHost.Aborted, new Action<WorkflowApplicationAbortedEventArgs>(OnAborted));
		WorkflowApplication workflowHost2 = WorkflowHost;
		workflowHost2.Completed = (Action<WorkflowApplicationCompletedEventArgs>)Delegate.Combine(workflowHost2.Completed, new Action<WorkflowApplicationCompletedEventArgs>(OnCompleted));
		WorkflowApplication workflowHost3 = WorkflowHost;
		workflowHost3.Idle = (Action<WorkflowApplicationIdleEventArgs>)Delegate.Combine(workflowHost3.Idle, new Action<WorkflowApplicationIdleEventArgs>(OnIdle));
		WorkflowApplication workflowHost4 = WorkflowHost;
		workflowHost4.OnUnhandledException = (Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>)Delegate.Combine(workflowHost4.OnUnhandledException, new Func<WorkflowApplicationUnhandledExceptionEventArgs, UnhandledExceptionAction>(OnUnhandledException));
		WorkflowHost.Run();
		State = NavWorkflowState.InProgress;
	}

	public abstract void StopWorkflow();

	public abstract void ResumeWorkflow(TResumeRequest resumeRequest);

	public abstract TStatus GetWorkflowStatus();

	protected virtual UnhandledExceptionAction OnUnhandledException(WorkflowApplicationUnhandledExceptionEventArgs args)
	{
		return UnhandledExceptionAction.Terminate;
	}

	protected virtual void OnAborted(WorkflowApplicationAbortedEventArgs args)
	{
	}

	protected virtual void OnCompleted(WorkflowApplicationCompletedEventArgs args)
	{
	}

	protected virtual void OnIdle(WorkflowApplicationIdleEventArgs args)
	{
	}

	protected virtual void AddExtension(object extension)
	{
		WorkflowHost.Extensions.Add(extension);
	}

	protected virtual void Cancel()
	{
		WorkflowHost.Cancel();
	}

	protected virtual ReadOnlyCollection<BookmarkInfo> GetBookmarks()
	{
		return WorkflowHost.GetBookmarks();
	}

	protected virtual BookmarkResumptionResult ResumeBookmark(string bookmarkName, object value)
	{
		return WorkflowHost.ResumeBookmark(bookmarkName, value);
	}
}
