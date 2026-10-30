using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SessionAccessLock
{
	private enum DisposalState
	{
		Active,
		StoppingSignaled,
		WaitingForThreadsToComplete,
		Disposing,
		Disposed
	}

	private const int ExclusiveLockIndex = 0;

	private const int CallstackLockStartIndex = 1;

	private const int WaitPeriod = 3000;

	private readonly List<Thread> executingThreads = new List<Thread>(2) { null };

	private bool topCallstackThreadSuspended;

	private int waitingCount;

	private DisposalState disposalState;

	private TaskCompletionSource<bool> readyForDisposingTaskCompletionSource;

	private TaskCompletionSource<bool> disposedTaskCompletionSource;

	private TaskCompletionSource<bool> stoppingTaskCompletionSource;

	private readonly NavSession session;

	public bool ExternalStopRequest { get; private set; }

	public bool IsStoppingOrDisposed => disposalState != DisposalState.Active;

	private Thread ExclusiveThread
	{
		get
		{
			return executingThreads[0];
		}
		set
		{
			executingThreads[0] = value;
		}
	}

	private int CallstackThreadCount => executingThreads.Count - 1;

	private Thread TopCallstackThread
	{
		get
		{
			if (CallstackThreadCount == 0)
			{
				return null;
			}
			return executingThreads[executingThreads.Count - 1];
		}
	}

	private bool AreAnyThreadsExecuting
	{
		get
		{
			if (ExclusiveThread == null)
			{
				return CallstackThreadCount > 0;
			}
			return true;
		}
	}

	public bool AreAnyActiveThreadsExecuting
	{
		get
		{
			Thread.MemoryBarrier();
			if (ExclusiveThread == null)
			{
				if (CallstackThreadCount > 0)
				{
					return !topCallstackThreadSuspended;
				}
				return false;
			}
			return true;
		}
	}

	public bool IsCurrentThreadExecuting => executingThreads.Contains(Thread.CurrentThread);

	internal SessionAccessLock(NavSession session)
	{
		if (session == null)
		{
			throw new ArgumentNullException("session");
		}
		this.session = session;
	}

	public void PushOnCallStackThread(Thread newThread)
	{
		lock (executingThreads)
		{
			if (AreAnyActiveThreadsExecuting && CallstackThreadCount > 0)
			{
				executingThreads.Add(newThread);
			}
		}
	}

	public void PopFromCallStackThread()
	{
		lock (executingThreads)
		{
			if (AreAnyActiveThreadsExecuting && CallstackThreadCount > 0)
			{
				executingThreads.RemoveAt(CallstackThreadCount);
			}
		}
	}

	public void BeginExclusiveExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		lock (executingThreads)
		{
			ThrowIfNotActive();
			while (AreAnyActiveThreadsExecuting)
			{
				DoWait(3000);
				ThrowIfNotActive();
			}
			ExclusiveThread = currentThread;
		}
	}

	public void BeginCallStackExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		lock (executingThreads)
		{
			ThrowIfNotActive();
			if (CallstackThreadCount > 0 && !topCallstackThreadSuspended)
			{
				throw new NavNSConcurrencyException("An internal error has occurred. Multiple concurrent calls have been issued to the server from this client.");
			}
			while (ExclusiveThread != null)
			{
				DoWait(3000);
				ThrowIfNotActive();
			}
			topCallstackThreadSuspended = false;
			executingThreads.Add(currentThread);
		}
	}

	public void EndExclusiveExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		bool flag = false;
		lock (executingThreads)
		{
			if (ExclusiveThread != currentThread)
			{
				throw new InvalidOperationException("The current thread has not entered exclusive execution");
			}
			ExclusiveThread = null;
			if (readyForDisposingTaskCompletionSource != null)
			{
				flag = true;
				disposalState = DisposalState.Disposing;
			}
			if (waitingCount > 0)
			{
				Monitor.PulseAll(executingThreads);
			}
		}
		if (flag)
		{
			readyForDisposingTaskCompletionSource.SetResult(result: true);
		}
	}

	public void EndCallStackExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		bool flag = false;
		lock (executingThreads)
		{
			CheckTopCallStackThread(currentThread);
			CheckNotSuspended();
			executingThreads.RemoveAt(executingThreads.Count - 1);
			if (CallstackThreadCount > 0)
			{
				topCallstackThreadSuspended = true;
			}
			else
			{
				if (readyForDisposingTaskCompletionSource != null)
				{
					flag = true;
					disposalState = DisposalState.Disposing;
				}
				if (waitingCount > 0)
				{
					Monitor.PulseAll(executingThreads);
				}
			}
		}
		if (flag)
		{
			readyForDisposingTaskCompletionSource.SetResult(result: true);
		}
	}

	public void SuspendCallStackExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		lock (executingThreads)
		{
			NavDiagnostics.SuspendCurrentTimingScopeMeasurement();
			if (session.DataAccessSource.SessionTransactionManager.AnyHasWriteTransactionStarted())
			{
				NavApplicationObjectBase applicationObject = NavCurrentThread.Session.CurrentMethodScope.ApplicationObject;
				string text = ((applicationObject == null) ? string.Format(CultureInfo.InvariantCulture, "Attempted to issue a client callback while a write transaction started, this can impact performance: {0} {2} {1} {3}.", string.Empty, string.Empty, string.Empty, string.Empty) : string.Format(CultureInfo.InvariantCulture, "Attempted to issue a client callback while a write transaction started, this can impact performance: {0} {2} {1} {3}.", applicationObject.ObjectId.ObjectType, applicationObject.ObjectName, applicationObject.ObjectId.ObjectNumber, NavCurrentThread.Session.CurrentMethodScope.ScopeName));
				if (!ServerUserSettings.Instance.AllowSessionCallSuspendWhenWriteTransactionStarted.Value)
				{
					throw new NavCSideException(text);
				}
				DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("00000UX", Category.Runtime, Verbosity.Verbose, PrivacyClassification.SystemMetadata, "SuspendCallStackExecution is not allowed in write transaction. Error message: {0}", text);
			}
			CheckTopCallStackThread(currentThread);
			CheckNotSuspended();
			topCallstackThreadSuspended = true;
		}
	}

	public void ResumeCallStackExecution()
	{
		Thread currentThread = Thread.CurrentThread;
		lock (executingThreads)
		{
			NavDiagnostics.ResumeCurrentTimingScopeMeasurement();
			CheckTopCallStackThread(currentThread);
			CheckSuspended();
			topCallstackThreadSuspended = false;
		}
	}

	internal bool SetStoppingOrDisposedIfPossible(bool externalStopRequest, out bool areThreadsExecuting)
	{
		GiveUpExecutionIfAny();
		lock (executingThreads)
		{
			if (disposalState != DisposalState.Active)
			{
				areThreadsExecuting = false;
				return false;
			}
			ExternalStopRequest = externalStopRequest;
			if (AreAnyThreadsExecuting)
			{
				areThreadsExecuting = true;
				disposalState = DisposalState.StoppingSignaled;
				Monitor.PulseAll(executingThreads);
			}
			else
			{
				areThreadsExecuting = false;
				disposalState = DisposalState.Disposing;
			}
			if (stoppingTaskCompletionSource != null)
			{
				stoppingTaskCompletionSource.SetResult(result: true);
			}
			return true;
		}
	}

	internal Task WaitUntilExecutionStoppedAndSetDisposingAsync()
	{
		lock (executingThreads)
		{
			switch (disposalState)
			{
			case DisposalState.Active:
				throw new InvalidOperationException("Must call SetStoppingOrDisposedIfPossible before WaitUntilExecutionStoppedAndSetDisposingAsync");
			case DisposalState.StoppingSignaled:
				disposalState = DisposalState.WaitingForThreadsToComplete;
				if (!AreAnyThreadsExecuting)
				{
					disposalState = DisposalState.Disposing;
					return TaskHelper.CompletedTask;
				}
				readyForDisposingTaskCompletionSource = new TaskCompletionSource<bool>();
				return readyForDisposingTaskCompletionSource.Task;
			default:
				throw new InvalidOperationException("Another thread is disposing or has disposed this session. This thread should be waiting on Disposed");
			}
		}
	}

	internal void SetDisposed()
	{
		bool flag = false;
		lock (executingThreads)
		{
			disposalState = DisposalState.Disposed;
			if (disposedTaskCompletionSource != null)
			{
				flag = true;
			}
		}
		if (flag)
		{
			disposedTaskCompletionSource.SetResult(result: true);
		}
	}

	internal Task WaitForDisposedAsync()
	{
		lock (executingThreads)
		{
			if (disposalState == DisposalState.Disposed)
			{
				return TaskHelper.CompletedTask;
			}
			if (disposedTaskCompletionSource == null)
			{
				disposedTaskCompletionSource = new TaskCompletionSource<bool>();
			}
			return disposedTaskCompletionSource.Task;
		}
	}

	public Task WaitForStoppingAsync()
	{
		lock (executingThreads)
		{
			if (disposalState != DisposalState.Active)
			{
				return TaskHelper.CompletedTask;
			}
			stoppingTaskCompletionSource = new TaskCompletionSource<bool>();
			return stoppingTaskCompletionSource.Task;
		}
	}

	internal void SleepButWakeOnStopping(int millisecondsToSleep)
	{
		NavThread.RunExternalAction(() =>
		{
			lock (executingThreads)
			{
				if (disposalState == DisposalState.Active && Monitor.Wait(executingThreads, millisecondsToSleep) && disposalState == DisposalState.Active)
				{
					throw new InvalidOperationException("AccessLock error: not expected that we would be pulsed with disposalState == Active. We expect to sleep only if we already have the an exclusive or callstack lock and we only pulse when no threads have the lock and there are waiters");
				}
			}
		});
	}

	internal void ThrowSessionTerminatedExceptionIfStopping()
	{
		DisposalState disposalState = this.disposalState;
		if ((uint)(disposalState - 1) <= 1u)
		{
			ThrowSessionTerminatedException();
		}
	}

	private void CheckTopCallStackThread(Thread thread)
	{
		if (thread != TopCallstackThread)
		{
			throw new InvalidOperationException("The current thread is not the topmost callstack thread");
		}
	}

	private void CheckNotSuspended()
	{
		if (topCallstackThreadSuspended)
		{
			throw new InvalidOperationException("The current thread must not have suspended callstack execution");
		}
	}

	private void CheckSuspended()
	{
		if (!topCallstackThreadSuspended)
		{
			throw new InvalidOperationException("The current thread must have suspended callstack execution");
		}
	}

	[Conditional("DEBUG")]
	private void CheckThreadNotExecuting(Thread thread)
	{
		int num = executingThreads.IndexOf(thread);
		if (num < 0)
		{
			return;
		}
		throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "This thread already has entered {0} execution. Recursion is not supported", (num == 0) ? "exclusive" : "callstack"));
	}

	private void GiveUpExecutionIfAny()
	{
		Thread currentThread = Thread.CurrentThread;
		int num;
		lock (executingThreads)
		{
			num = executingThreads.IndexOf(currentThread);
		}
		switch (num)
		{
		case 0:
			EndExclusiveExecution();
			break;
		default:
			EndCallStackExecution();
			break;
		case -1:
			break;
		}
	}

	internal void ThrowIfNotActive()
	{
		if (disposalState != DisposalState.Active)
		{
			ThrowSessionTerminatedException();
		}
	}

	private void ThrowSessionTerminatedException()
	{
		throw new NavSessionTerminatedException(string.Format(CultureInfo.CurrentCulture, Lang.ServerAbortedActivity, session.Id));
	}

	private void DoWait(int milliseconds)
	{
		waitingCount++;
		Monitor.Wait(executingThreads, milliseconds);
		waitingCount--;
	}
}
