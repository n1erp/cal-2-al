using System;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class DebugRuntime : IDisposable, IExecutionListener, IDataModificationListener
{
	protected enum DebugState
	{
		Stopped,
		RunUntilBreakpoint,
		StopOnNextHit,
		StepUntilNextHitInScope
	}

	private const int MaxTimeToWaitOnDebugAdapterToSignalItsConfigurationDone = 10000;

	internal static readonly Type[] MappedExceptionTypes = new Type[4]
	{
		typeof(OverflowException),
		typeof(DivideByZeroException),
		typeof(IndexOutOfRangeException),
		typeof(FormatException)
	};

	private readonly ExpressionEvaluator conditionEvaluator;

	private Exception lastHandledException;

	private int state;

	private NavMethodScope scopeToBreakOnWhenExiting;

	private NavMethodScope skipUntilThisScopeIsPopped;

	private bool skipStateKnown;

	private bool ignoreSkip;

	private bool walkFullStackForTriggersToSkip;

	private NavMethodScope stepScope;

	private readonly ManualResetEventSlim debugAdapterFinishedConfigurationWaitHandle;

	private NavSession debuggedSession;

	public bool BreakOnError { get; set; }

	public bool BreakOnRecordWrite { get; set; }

	public bool SkipSystemTriggers { get; set; }

	public NavSession DebuggedSession
	{
		get
		{
			return debuggedSession;
		}
		private set
		{
			debuggedSession?.SqlDebuggingStatistics.StopDebuggingBehavior();
			value?.SqlDebuggingStatistics.StartDebuggingBehavior();
			debuggedSession = value;
		}
	}

	public DebugEnvironment Environment { get; }

	private DebugState State => (DebugState)state;

	public int Priority => DataModificationListener.HighestPriority;

	protected internal DebugRuntime(DebugEnvironment environment, NavSession session, bool enableWaitForBreakpoints = false)
	{
		Environment = environment;
		DebuggedSession = session;
		conditionEvaluator = new ExpressionEvaluator();
		ChangeState(DebugState.RunUntilBreakpoint);
		if (enableWaitForBreakpoints)
		{
			debugAdapterFinishedConfigurationWaitHandle = new ManualResetEventSlim();
		}
	}

	protected void BeforeRecordWriteOperation(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		ProcessRecordWriteOperation();
	}

	public void ReleaseConnection()
	{
		DebuggedSession = null;
		stepScope = null;
		skipStateKnown = false;
		skipUntilThisScopeIsPopped = null;
		ignoreSkip = false;
		walkFullStackForTriggersToSkip = false;
		ChangeState(DebugState.RunUntilBreakpoint);
	}

	public void SignalDebugAdapterHasFinishedItsConfiguration()
	{
		if (debugAdapterFinishedConfigurationWaitHandle != null)
		{
			debugAdapterFinishedConfigurationWaitHandle.Set();
		}
	}

	public void WaitForDebugAdapterToSignalItsConfigurationDone(int timeout = 10000)
	{
		if (debugAdapterFinishedConfigurationWaitHandle != null)
		{
			debugAdapterFinishedConfigurationWaitHandle.Wait(timeout);
		}
	}

	public void BreakOnNextStatement()
	{
		ChangeState(DebugState.StopOnNextHit);
		walkFullStackForTriggersToSkip = true;
	}

	public void CancelBreakOnNextStatement()
	{
		ChangeState(DebugState.RunUntilBreakpoint);
	}

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
		if (CheckOnRightConnection(methodScope) && BreakOnError && lastHandledException != exception && (lastHandledException == null || exception == null || lastHandledException != exception.InnerException) && (ALDebugger.DebuggingSession != null || IsExtensionDebuggingSessionActive()) && (exception is NavBaseException || Array.IndexOf(MappedExceptionTypes, exception.GetType()) >= 0) && !(exception is NavNCLDebuggerActivityAbortedException))
		{
			lastHandledException = exception;
			ProcessBreak(methodScope, exception);
			if (State == DebugState.StepUntilNextHitInScope)
			{
				stepScope = null;
				ChangeState(DebugState.StopOnNextHit);
			}
		}
	}

	private bool IsExtensionDebuggingSessionActive()
	{
		return !string.IsNullOrEmpty(DebuggedSession?.DebuggingContext);
	}

	protected virtual bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return true;
	}

	public void ProcessExit(NavMethodScope methodScope)
	{
		if (!CheckOnRightConnection(methodScope))
		{
			return;
		}
		if (methodScope.HasUnhandledException)
		{
			if (methodScope.ParentScope.IsRootScope)
			{
				ChangeState(DebugState.RunUntilBreakpoint);
				scopeToBreakOnWhenExiting = null;
				stepScope = null;
				skipUntilThisScopeIsPopped = null;
				skipStateKnown = false;
			}
			return;
		}
		ProcessStatementHitOnRightConnection(methodScope);
		if (DebuggedSession == methodScope.Session)
		{
			if (methodScope.ParentScope.IsRootScope)
			{
				skipStateKnown = false;
				skipUntilThisScopeIsPopped = null;
				ignoreSkip = false;
			}
			else
			{
				if (ignoreSkip && skipUntilThisScopeIsPopped == null)
				{
					FindSkipUntilThisScopeIsPopped(methodScope, stackWalk: true);
				}
				if (skipUntilThisScopeIsPopped == methodScope)
				{
					skipStateKnown = false;
					skipUntilThisScopeIsPopped = null;
					ignoreSkip = false;
				}
			}
		}
		switch (State)
		{
		case DebugState.Stopped:
		case DebugState.RunUntilBreakpoint:
			break;
		case DebugState.StopOnNextHit:
		case DebugState.StepUntilNextHitInScope:
		{
			NavMethodScope parentMethodScope = methodScope.GetParentMethodScope();
			if (parentMethodScope == null || methodScope.ParentScope.IsRootScope)
			{
				if (State == DebugState.StepUntilNextHitInScope)
				{
					ChangeState(DebugState.StopOnNextHit);
					scopeToBreakOnWhenExiting = null;
					stepScope = null;
				}
			}
			else if (scopeToBreakOnWhenExiting == methodScope)
			{
				ProcessBreak(parentMethodScope, null);
			}
			break;
		}
		default:
			throw new ArgumentOutOfRangeException(State.ToString());
		}
	}

	public void ProcessStatementHit(NavMethodScope methodScope)
	{
		if ((DebuggedSession == null || DebuggedSession == methodScope.Session) && (State != DebugState.RunUntilBreakpoint || methodScope.HasBreakpointsDefined) && (DebuggedSession != null || !methodScope.Session.IsDebugging()) && IsBreakAllowedInScope(methodScope))
		{
			ProcessStatementHitOnRightConnection(methodScope);
		}
	}

	private static bool IsBreakAllowedInScope(NavMethodScope methodScope)
	{
		if (!methodScope.IsDebuggable)
		{
			return false;
		}
		if (methodScope.ApplicationObject == null || methodScope.ApplicationObject.MetaApplicationObject == null || IsDebuggingDisabledForApplicationObject(methodScope.ApplicationObject.MetaApplicationObject))
		{
			return false;
		}
		return true;
	}

	internal static bool IsDebuggingDisabledForApplicationObject(NCLMetaApplicationObject metaApplicationObject)
	{
		if (ServerUserSettings.Instance.DebuggingBaseAppAllowed.Value)
		{
			return false;
		}
		if (metaApplicationObject.OwningApp != null)
		{
			return string.Equals(metaApplicationObject.OwningApp.Publisher, "Microsoft", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	public void EnsureDetachedFromConnection(NavSession session)
	{
		if (DebuggedSession == session)
		{
			DebuggedSession = null;
			Environment.DebuggerCallback.OnDetachedFromConnection(session.DebuggingContext);
		}
	}

	public (string Content, bool IsNavAppApplicationObjectSource) GetSource(ApplicationObjectId appId)
	{
		return Environment.GetSource(appId);
	}

	public void SetDebugOptions(bool breakOnError, bool breakOnRecordWrite, bool skipSystemCodeunits)
	{
		BreakOnError = breakOnError;
		BreakOnRecordWrite = breakOnRecordWrite;
		SkipSystemTriggers = skipSystemCodeunits;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void ProcessRecordWriteOperation()
	{
		NavSession session = NavCurrentThread.Session;
		if (CheckOnRightConnection(session.CurrentMethodScope) && BreakOnRecordWrite && !session.CurrentMethodScope.IsRootScope)
		{
			ProcessBreak(session.CurrentMethodScope, null);
		}
	}

	protected DebugState ChangeState(DebugState newState)
	{
		return (DebugState)Interlocked.Exchange(ref state, (int)newState);
	}

	private bool CheckOnRightConnection(NavMethodScope methodScope)
	{
		if (DebuggedSession != null && DebuggedSession != methodScope.Session)
		{
			return false;
		}
		if (DebuggedSession == null && methodScope.Session.IsDebugging())
		{
			return false;
		}
		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		try
		{
			debuggedSession?.SqlDebuggingStatistics.StopDebuggingBehavior();
			if (debugAdapterFinishedConfigurationWaitHandle != null)
			{
				debugAdapterFinishedConfigurationWaitHandle.Dispose();
			}
			ExecutionListener.RemoveListener(this);
			DataModificationListener.RemoveListener(this);
		}
		finally
		{
			Environment.Dispose();
		}
	}

	private void ProcessBreak(NavMethodScope methodScope, Exception exception)
	{
		ProcessBreak(methodScope, exception, setIgnoreSkip: false);
	}

	private void ProcessBreak(NavMethodScope methodScope, Exception exception, bool setIgnoreSkip)
	{
		if (!IsBreakAllowedInScope(methodScope) || ChangeState(DebugState.Stopped) == DebugState.Stopped)
		{
			return;
		}
		walkFullStackForTriggersToSkip = false;
		if (setIgnoreSkip)
		{
			ignoreSkip = true;
		}
		try
		{
			if (DebuggedSession == null)
			{
				Environment.DebuggerCallback.OnAttachedToConnection(string.Empty);
				DebuggedSession = methodScope.Session;
			}
			switch (Environment.PerformBreakCallback(DebuggedSession.DebuggingContext, methodScope, exception))
			{
			case BreakpointResponse.StepIn:
				ChangeState(DebugState.StopOnNextHit);
				scopeToBreakOnWhenExiting = methodScope;
				skipStateKnown = skipUntilThisScopeIsPopped != null;
				break;
			case BreakpointResponse.StepOver:
				stepScope = methodScope;
				scopeToBreakOnWhenExiting = methodScope;
				ChangeState(DebugState.StepUntilNextHitInScope);
				break;
			case BreakpointResponse.StepOut:
				scopeToBreakOnWhenExiting = methodScope;
				stepScope = methodScope.GetParentMethodScope();
				ChangeState(DebugState.StepUntilNextHitInScope);
				break;
			case BreakpointResponse.Continue:
				scopeToBreakOnWhenExiting = null;
				stepScope = null;
				ChangeState(DebugState.RunUntilBreakpoint);
				break;
			case BreakpointResponse.AbortActivity:
				scopeToBreakOnWhenExiting = null;
				stepScope = null;
				ChangeState(DebugState.RunUntilBreakpoint);
				throw new NavNCLDebuggerActivityAbortedException(Lang.DebuggerAbortedActivity);
			default:
			{
				string debuggingContext = DebuggedSession?.DebuggingContext;
				DebuggedSession = null;
				Environment.DebuggerCallback.OnDetachedFromConnection(debuggingContext);
				stepScope = null;
				scopeToBreakOnWhenExiting = null;
				ChangeState(DebugState.RunUntilBreakpoint);
				break;
			}
			}
		}
		catch (DebuggerFatalException)
		{
			Dispose();
		}
		finally
		{
			if (State == DebugState.Stopped)
			{
				ChangeState(DebugState.RunUntilBreakpoint);
			}
		}
	}

	private void ProcessStatementHitOnRightConnection(NavMethodScope methodScope)
	{
		switch (State)
		{
		case DebugState.StopOnNextHit:
			if (!SkipSystemTriggers || ignoreSkip || FindSkipUntilThisScopeIsPopped(methodScope, walkFullStackForTriggersToSkip) == null)
			{
				break;
			}
			goto case DebugState.RunUntilBreakpoint;
		case DebugState.StepUntilNextHitInScope:
			if (methodScope == stepScope)
			{
				break;
			}
			goto case DebugState.RunUntilBreakpoint;
		case DebugState.RunUntilBreakpoint:
		{
			Breakpoint breakpoint = Environment.Breakpoints[methodScope.ScopeId, methodScope.StatementNumber];
			if (breakpoint == null)
			{
				return;
			}
			bool flag = EvaluateBreakpointConditionIfAny(methodScope, breakpoint, out var expressionException);
			if (expressionException != null)
			{
				ProcessBreak(methodScope, expressionException);
				return;
			}
			if (!flag)
			{
				return;
			}
			if (FindSkipUntilThisScopeIsPopped(methodScope, stackWalk: true) != null)
			{
				ProcessBreak(methodScope, null, setIgnoreSkip: true);
				return;
			}
			break;
		}
		case DebugState.Stopped:
			return;
		default:
			throw new ArgumentOutOfRangeException(State.ToString());
		}
		ProcessBreak(methodScope, null);
	}

	private NavMethodScope FindSkipUntilThisScopeIsPopped(NavMethodScope currentMethodScope, bool stackWalk)
	{
		if (skipStateKnown && !stackWalk)
		{
			return skipUntilThisScopeIsPopped;
		}
		NavMethodScope result = null;
		NavMethodScope navMethodScope = currentMethodScope;
		while (!navMethodScope.IsRootScope)
		{
			if (navMethodScope.ApplicationObject != null)
			{
				ApplicationObjectId objectId = navMethodScope.ApplicationObject.ObjectId;
				if (objectId.ObjectType == ObjectType.CodeUnit && objectId.ObjectNumber >= 2000000001)
				{
					result = navMethodScope;
					break;
				}
			}
			if (!stackWalk)
			{
				break;
			}
			navMethodScope = navMethodScope.ParentScope;
		}
		if (DebuggedSession == currentMethodScope.Session)
		{
			skipStateKnown = true;
			skipUntilThisScopeIsPopped = result;
		}
		return result;
	}

	private bool EvaluateBreakpointConditionIfAny(NavMethodScope methodScope, Breakpoint bp, out InvalidConditionExpressionException expressionException)
	{
		expressionException = null;
		try
		{
			return conditionEvaluator.EvaluateCondition(bp, methodScope);
		}
		catch (InvalidConditionExpressionException ex)
		{
			expressionException = ex;
			return true;
		}
		catch (Exception ex2) when (!DebugEnvironment.IsCriticalException(ex2))
		{
			Environment.OnFatalDebuggerException(DebuggedSession?.DebuggingContext, ex2);
			Dispose();
			return false;
		}
	}

	public virtual void OnBeforeInsert(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual void OnBeforeModify(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual void OnBeforeDelete(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual void OnBeforeRename(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual void OnBeforeModifyAll(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual void OnBeforeDeleteAll(NavRecord record)
	{
		BeforeRecordWriteOperation(record);
	}

	public virtual bool CanCoexistWith(IDataModificationListener other)
	{
		return true;
	}

	bool IExecutionListener.CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		return CanCoexistWithOtherListener(otherListener);
	}
}
