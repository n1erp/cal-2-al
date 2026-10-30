using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Runtime.Debugger;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class ALDebugger : INavDebuggerCallback, IDisposable
{
	private class UpdateBreakpointsThread : IDisposable
	{
		private Thread thread;

		private AutoResetEvent updateBreakpoints = new AutoResetEvent(initialState: false);

		private bool stop;

		private readonly object debuggerLock = new object();

		public UpdateBreakpointsThread()
		{
			thread = new Thread(Main);
			thread.IsBackground = true;
			thread.Name = "UpdateBreakpointsThread";
			thread.Start();
		}

		public void Update()
		{
			updateBreakpoints.Set();
		}

		private void Main()
		{
			NavCurrentThread.Session = debuggingSession;
			if (NavCurrentThread.Session == null)
			{
				return;
			}
			while (true)
			{
				updateBreakpoints.WaitOne();
				if (stop)
				{
					break;
				}
				lock (debuggerLock)
				{
					UpdateBreakpoints();
				}
			}
		}

		private static void UpdateBreakpoints()
		{
			bool commit = false;
			debuggingSession.BeginNonServiceCallExecution();
			try
			{
				debuggingSession.BeginTransaction();
				NavDebuggerBreakpointSystemRecord.LoadBreakpoints();
				commit = true;
			}
			finally
			{
				try
				{
					if (NavCurrentThread.Session.IsTransactionActive())
					{
						debuggingSession.EndTransaction(commit);
					}
				}
				finally
				{
					debuggingSession.EndNonServiceCallExecution();
				}
			}
		}

		public void Dispose()
		{
			lock (debuggerLock)
			{
				stop = true;
				updateBreakpoints.Set();
			}
			thread.Join();
			updateBreakpoints.Dispose();
		}
	}

	private static NavSession debuggingSession;

	private static ALDebugger instance;

	private Exception debugRuntimeException;

	private SingleInstanceDebugRuntime debugRuntime;

	private static readonly object debuggingConnectionLock = new object();

	private AutoResetEvent waitForDebuggerCommand = new AutoResetEvent(initialState: false);

	private BreakpointResponse breakpointResponse;

	private StackFrame[] callStack;

	private bool isBreakpointHit;

	private UpdateBreakpointsThread updatebreakpointsThread;

	internal static SingleInstanceDebugRuntime DebugRuntimeInstance
	{
		get
		{
			if (Instance != null)
			{
				return Instance.debugRuntime;
			}
			return null;
		}
	}

	public static ALDebugger Instance
	{
		get
		{
			if (instance != null && instance.debugRuntimeException != null)
			{
				Exception ex = instance.debugRuntimeException;
				instance.Dispose();
				throw ex;
			}
			return instance;
		}
	}

	public static NavSession DebuggingSession => debuggingSession;

	public bool IsBreakpointHit => isBreakpointHit;

	public event EventHandler<BreakHitEventArgs> BreakHit;

	private ALDebugger(NavSession sessionToBeDebugged)
	{
		debugRuntime = DebugRuntimeFactory.CreateSingleInstanceDebugRuntime(this, sessionToBeDebugged, NavGlobal.NCLMetadata.CodeEnvironment);
		debuggingSession = NavCurrentThread.Session;
		BreakHit += ALDebugger_BreakHit;
		updatebreakpointsThread = new UpdateBreakpointsThread();
		NavGlobal.AppDatabase.BreakpointsChanged += Database_BreakpointsChanged;
	}

	private void Database_BreakpointsChanged(object sender, NavTrackingEventArgs e)
	{
		updatebreakpointsThread.Update();
	}

	private void ALDebugger_BreakHit(object sender, BreakHitEventArgs e)
	{
		NavSession session = NavCurrentThread.Session;
		bool commit = false;
		session.BeginNonServiceCallExecution();
		try
		{
			session.BeginTransaction();
			InvalidateVirtualTables();
			NavCurrentThread.Session.SystemCodeunitFactory.DebuggerTriggers.InvokeOnDebuggerBreak(new NavText(e.ErrorMessage));
			commit = true;
		}
		catch (NavBaseException ex)
		{
			debugRuntimeException = new NavDebuggerBreakException(ex.Message, ex);
		}
		finally
		{
			try
			{
				if (NavCurrentThread.Session.IsTransactionActive())
				{
					session.EndTransaction(commit);
				}
			}
			finally
			{
				session.EndNonServiceCallExecution();
			}
		}
	}

	private void Dispose(bool disposing)
	{
		if (disposing)
		{
			NavGlobal.AppDatabase.BreakpointsChanged -= Database_BreakpointsChanged;
			updatebreakpointsThread.Dispose();
			debugRuntime.Dispose();
			if (isBreakpointHit)
			{
				Go(BreakpointResponse.Continue);
			}
			waitForDebuggerCommand.Close();
			instance = null;
			debuggingSession = null;
		}
	}

	private static void InvalidateVirtualTables()
	{
		DataAccessTableVersionTokens tableVersionTokens = debuggingSession.DataAccessSource.TableVersionTokens;
		tableVersionTokens.IncrementUpdateBumperToken(0, 2000000101);
		tableVersionTokens.IncrementUpdateBumperToken(0, 2000000102);
		tableVersionTokens.IncrementUpdateBumperToken(0, 2000000103);
	}

	private static void CheckConnection()
	{
		CheckConnection(throwException: true);
	}

	private static bool CheckConnection(bool throwException)
	{
		if (debuggingSession == null || NavCurrentThread.Session != debuggingSession)
		{
			if (throwException)
			{
				throw new NavDebuggerConnectionException(Lang.NavDebuggerConnectionIsNotAttached);
			}
			return false;
		}
		return true;
	}

	private static void CheckDebuggingAllowed()
	{
		if (!ServerUserSettings.Instance.DebuggingAllowed.Value)
		{
			throw NavDebuggerDebuggingNotAllowedException.Create();
		}
	}

	public static void CheckPermissionToDebug()
	{
		NavSession session = NavCurrentThread.Session;
		if (!session.Permissions.IsSuperForAllCompanies && session.Database.SecurityAndLicense.IsPermissionSystemEnabled())
		{
			throw NavNCLSuperRoleRequiredException.Create();
		}
	}

	private bool CheckBreakpointHit(bool throwException)
	{
		if (!IsBreakpointHit)
		{
			if (throwException)
			{
				throw new NavDebuggerNotAtBreakpointException();
			}
			return false;
		}
		return true;
	}

	private void Go(BreakpointResponse command)
	{
		CheckBreakpointHit(throwException: true);
		breakpointResponse = command;
		isBreakpointHit = false;
		InvalidateVirtualTables();
		waitForDebuggerCommand.Set();
	}

	internal StackFrame[] GetCurrentCallStack()
	{
		NavSession navSession = DebuggingSession;
		if (!CheckBreakpointHit(throwException: false) || debugRuntimeException != null || navSession == null || navSession != NavCurrentThread.Session)
		{
			return new StackFrame[0];
		}
		return callStack;
	}

	internal IEnumerable<LocalNodeContext> GetVariablesForStackFrame(int stackFrameNumber)
	{
		NavSession navSession = DebuggingSession;
		if (!CheckBreakpointHit(throwException: false) || debugRuntimeException != null || navSession == null || navSession != NavCurrentThread.Session)
		{
			return new LocalNodeContext[0];
		}
		int num = stackFrameNumber - 1;
		return SingleInstanceDebugRuntime.Instance.Environment.BuildVariables(num, SingleInstanceDebugRuntime.Instance.Environment.GetLocals(num), 0, string.Empty, referencedGlobalsProcessed: false);
	}

	internal IEnumerable<LocalNode> GetWatchesForStackFrame(int stackFrameNumber)
	{
		NavSession navSession = DebuggingSession;
		if (!CheckBreakpointHit(throwException: false) || debugRuntimeException != null || navSession == null || navSession != NavCurrentThread.Session)
		{
			return new LocalNode[0];
		}
		return DebugRuntimeInstance.Environment.GetWatchNodes(stackFrameNumber - 1);
	}

	public static bool ALActivate(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALActivate();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALActivate();
		return true;
	}

	public static void ALActivate()
	{
		CheckDebuggingAllowed();
		CheckPermissionToDebug();
		lock (debuggingConnectionLock)
		{
			if (debuggingSession == null)
			{
				instance = new ALDebugger(null);
				try
				{
					NavCurrentThread.Session.BeginTransaction();
					NavDebuggerBreakpointSystemRecord.LoadBreakpoints();
					NavDebuggerWatchSystemRecord.LoadWatches();
					return;
				}
				finally
				{
					if (NavCurrentThread.Session.IsTransactionActive())
					{
						NavCurrentThread.Session.EndTransaction(commit: true);
					}
				}
			}
			if (debuggingSession == NavCurrentThread.Session)
			{
				throw new NavDebuggerConnectionException(Lang.NavDebuggerConnectionIsDebugging);
			}
			throw new NavDebuggerConnectionException(Lang.NavDebuggerOtherConnectionDebugging, debuggingSession.User.Name);
		}
	}

	public static bool ALDeactivate(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALDeactivate();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALDeactivate();
		return true;
	}

	public static void ALDeactivate()
	{
		CheckConnection();
		lock (debuggingConnectionLock)
		{
			Instance.Dispose();
		}
	}

	public static bool ALAttach(DataError errorLevel, int sessionId)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALAttach(sessionId);
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALAttach(sessionId);
		return true;
	}

	public static void ALAttach(int sessionId)
	{
		CheckDebuggingAllowed();
		CheckPermissionToDebug();
		NavSession session = NavCurrentThread.Session;
		if (sessionId == session.Id)
		{
			throw new NavDebuggerConnectionException(Lang.NavDebuggerNoDebugOwnSession);
		}
		NavSession sessionById = session.Tenant.GetSessionById(sessionId);
		if (sessionById == null)
		{
			throw new NavDebuggerConnectionException(string.Format(CultureInfo.CurrentCulture, Lang.NavDebuggerConnectionCannotBeFound, sessionId));
		}
		lock (debuggingConnectionLock)
		{
			if (debuggingSession == null)
			{
				instance = new ALDebugger(sessionById);
				try
				{
					session.BeginTransaction();
					NavDebuggerBreakpointSystemRecord.LoadBreakpoints();
					NavDebuggerWatchSystemRecord.LoadWatches();
					return;
				}
				finally
				{
					if (session.IsTransactionActive())
					{
						session.EndTransaction(commit: true);
					}
				}
			}
			if (debuggingSession == session)
			{
				throw new NavDebuggerConnectionException(Lang.NavDebuggerConnectionIsDebugging);
			}
			throw new NavDebuggerConnectionException(Lang.NavDebuggerOtherConnectionDebugging, debuggingSession.User.Name);
		}
	}

	public static bool ALBreak(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALBreak();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALBreak();
		return true;
	}

	public static void ALBreak()
	{
		CheckConnection();
		DebugRuntimeInstance.BreakOnNextStatement();
	}

	public static bool ALBreakOnError(DataError errorLevel, bool breakOnError)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALBreakOnError(breakOnError);
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALBreakOnError(breakOnError);
		return true;
	}

	public static void ALBreakOnError(bool breakOnError)
	{
		CheckConnection();
		DebugRuntimeInstance.BreakOnError = breakOnError;
	}

	public static bool ALBreakOnRecordChanges(DataError errorLevel, bool breakOnRecordChanges)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALBreakOnRecordChanges(breakOnRecordChanges);
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALBreakOnRecordChanges(breakOnRecordChanges);
		return true;
	}

	public static void ALBreakOnRecordChanges(bool breakOnRecordChanges)
	{
		CheckConnection();
		DebugRuntimeInstance.BreakOnRecordWrite = breakOnRecordChanges;
	}

	public static bool ALSkipSystemTriggers(DataError errorLevel, bool skipSystemTriggers)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALSkipSystemTriggers(skipSystemTriggers);
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALSkipSystemTriggers(skipSystemTriggers);
		return true;
	}

	public static void ALSkipSystemTriggers(bool skipSystemTriggers)
	{
		CheckConnection();
		DebugRuntimeInstance.SkipSystemTriggers = skipSystemTriggers;
	}

	public static bool ALContinue(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALContinue();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALContinue();
		return true;
	}

	public static void ALContinue()
	{
		CheckConnection();
		Instance.Go(BreakpointResponse.Continue);
	}

	public static bool ALStepInto(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALStepInto();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALStepInto();
		return true;
	}

	public static void ALStepInto()
	{
		CheckConnection();
		Instance.Go(BreakpointResponse.StepIn);
	}

	public static bool ALStepOut(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALStepOut();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALStepOut();
		return true;
	}

	public static void ALStepOut()
	{
		CheckConnection();
		Instance.Go(BreakpointResponse.StepOut);
	}

	public static bool ALStepOver(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALStepOver();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALStepOver();
		return true;
	}

	public static void ALStepOver()
	{
		CheckConnection();
		Instance.Go(BreakpointResponse.StepOver);
	}

	public static bool ALStop(DataError errorLevel)
	{
		if (errorLevel == DataError.TrapError)
		{
			try
			{
				ALStop();
			}
			catch (NavBaseException)
			{
				return false;
			}
			return true;
		}
		ALStop();
		return true;
	}

	public static void ALStop()
	{
		CheckConnection();
		Instance.Go(BreakpointResponse.AbortActivity);
	}

	public static bool ALIsAttached()
	{
		if (ALIsActive())
		{
			return Instance.debugRuntime.DebuggedSession != null;
		}
		return false;
	}

	public static bool ALIsActive()
	{
		return instance != null;
	}

	public static bool ALIsBreakpointHit()
	{
		if (!CheckConnection(throwException: false))
		{
			return false;
		}
		return instance.IsBreakpointHit;
	}

	public static int ALDebuggedSessionId()
	{
		if (instance != null)
		{
			return Instance.debugRuntime.DebuggedSession?.Id ?? 0;
		}
		return 0;
	}

	public static int ALDebuggingSessionId()
	{
		if (instance != null)
		{
			return DebuggingSession?.Id ?? 0;
		}
		return 0;
	}

	public static string ALGetLastErrorText()
	{
		if (instance != null && instance.IsBreakpointHit)
		{
			return instance.debugRuntime.DebuggedSession.GetLastErrorText;
		}
		return string.Empty;
	}

	public static bool ALEnableSqlTrace(int sessionId, bool newIsEnabled)
	{
		NavSession session = NavCurrentThread.Session;
		if (session == null)
		{
			return false;
		}
		if (sessionId != 0)
		{
			NavSession sessionById = session.Tenant.GetSessionById(sessionId);
			if (sessionById == null)
			{
				return false;
			}
			return sessionById.SqlProfilerCommentProvider.IsEnabled = newIsEnabled;
		}
		foreach (NavSession activeSession in session.Tenant.ActiveSessions)
		{
			activeSession.SqlProfilerCommentProvider.IsEnabled = newIsEnabled;
		}
		return SqlProfilerCommentProvider.IsEnabledForNewConnections = newIsEnabled;
	}

	public static bool ALEnableSqlTrace(int sessionId)
	{
		if (sessionId != 0)
		{
			NavSession session = NavCurrentThread.Session;
			return session.Tenant.GetSessionById(sessionId)?.SqlProfilerCommentProvider.IsEnabled ?? false;
		}
		return SqlProfilerCommentProvider.IsEnabledForNewConnections;
	}

	BreakpointResponse INavDebuggerCallback.Break(string debuggingContext, ApplicationObjectId objectId, StackFrame[] stack, LocalNode[] localNodes, string errorMessage)
	{
		isBreakpointHit = true;
		callStack = stack;
		InvokeBreakHitAndWait(errorMessage);
		callStack = null;
		return breakpointResponse;
	}

	private void InvokeBreakHitAndWait(string errorMessage)
	{
		NavSession session = NavCurrentThread.Session;
		NavCurrentThread.Session = debuggingSession;
		if (NavCurrentThread.Session == null)
		{
			NavCurrentThread.Session = session;
			isBreakpointHit = false;
			breakpointResponse = BreakpointResponse.Continue;
			return;
		}
		try
		{
			BreakHit(this, new BreakHitEventArgs(errorMessage));
		}
		catch (NavBaseException ex)
		{
			debugRuntimeException = ex;
			callStack = null;
			isBreakpointHit = false;
			breakpointResponse = BreakpointResponse.Continue;
			return;
		}
		catch (Exception)
		{
			callStack = null;
			isBreakpointHit = false;
			throw;
		}
		finally
		{
			NavCurrentThread.Session = session;
		}
		waitForDebuggerCommand.WaitOne();
	}

	void INavDebuggerCallback.InvalidateSource(string debuggingContext, ApplicationObjectId id)
	{
	}

	void INavDebuggerCallback.OnFatalDebuggerException(string debuggingContext, ServerFault fault)
	{
		debugRuntimeException = new NavDebuggerFatalException(Lang.NavDebuggerFatalError, ExceptionHandler.GetOriginalException(fault));
	}

	void INavDebuggerCallback.OnAttachedToConnection(string debuggingContext)
	{
	}

	void INavDebuggerCallback.OnDetachedFromConnection(string debuggingContext)
	{
		Dispose();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
