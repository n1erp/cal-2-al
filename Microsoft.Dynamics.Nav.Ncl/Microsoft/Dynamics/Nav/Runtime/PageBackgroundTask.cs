using System;
using System.Threading;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime;

public class PageBackgroundTask : TreeObject
{
	private static int lastTaskId;

	public int TaskId { get; }

	public Guid FormHandle { get; }

	public WeakReference<NavForm> Form { get; }

	public NavRecordId RecordId { get; }

	public Func<NavSession, NavCodeunitHandle> CodeunitHandleFactory { get; }

	public int CodeunitId { get; }

	public NavDictionary<NavText, NavText> Parameters { get; }

	public bool CancelOnRecordChange { get; }

	public NavDictionary<NavText, NavText> Results { get; set; } = NavDictionary<NavText, NavText>.Default;

	public NavChildSessionTaskError Error { get; internal set; }

	public IPageBackgroundTaskCompletionTrigger TaskCompletionTrigger { get; internal set; }

	public PageBackgroundChildSessionTask ChildSessionTask { get; internal set; }

	public PageBackgroundTask(NavSession parentSession, NavForm navForm, NavRecordId navRecordId, int codeunitId, Func<NavSession, NavCodeunitHandle> codeunitHandleFactory, NavDictionary<NavText, NavText> parameters, bool cancelOnRecordChange, int timeout)
		: base(parentSession)
	{
		if (navForm == null)
		{
			throw new ArgumentNullException("navForm");
		}
		if (parameters == null)
		{
			throw new ArgumentNullException("parameters");
		}
		Parameters = NavDictionary<NavText, NavText>.Clone(parameters);
		Form = new WeakReference<NavForm>(navForm);
		FormHandle = navForm.Handle;
		RecordId = navRecordId ?? throw new ArgumentNullException("navRecordId");
		CodeunitId = codeunitId;
		CodeunitHandleFactory = codeunitHandleFactory ?? throw new ArgumentNullException("codeunitHandleFactory");
		CancelOnRecordChange = cancelOnRecordChange;
		TaskId = Interlocked.Increment(ref lastTaskId);
		ChildSessionTask = new PageBackgroundChildSessionTask(this)
		{
			Timeout = ((timeout > 0) ? TimeSpan.FromMilliseconds(timeout) : TimeSpan.Zero)
		};
	}

	public void Cancel()
	{
		ChildSessionTask.CancellationTokenSource.Cancel();
	}

	public static bool CanPageBackgroundTaskRunAsync(NavSession session)
	{
		if (FeatureSwitches.Instance.DisablePageBackgroundTasksChildSessionAsync)
		{
			return false;
		}
		if (session.TestExecution != null && session.TestExecution.InTest)
		{
			return false;
		}
		if (session.ClientConnectionType == ConnectionType.Desktop || session.ClientConnectionType == ConnectionType.RoleTailoredClient || session.ClientConnectionType == ConnectionType.Tablet || session.ClientConnectionType == ConnectionType.WebClient || session.ClientConnectionType == ConnectionType.ClientService || session.ClientConnectionType == ConnectionType.SharePoint)
		{
			return true;
		}
		return false;
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				ChildSessionTask.Dispose();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}
}
