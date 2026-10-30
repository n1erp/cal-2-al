using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Diagnostics;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavMethodScope<TParent> : NavMethodScope where TParent : NavApplicationObjectBase
{
	public override NavApplicationObjectBase ApplicationObject => Parent;

	public override bool IsStackFrame => true;

	public TParent Parent { get; }

	public NavMethodScope(TParent applicationObject)
		: base(applicationObject, eventSource: false)
	{
		Parent = applicationObject;
	}

	public NavMethodScope(TParent applicationObject, bool eventSource)
		: base(applicationObject, eventSource)
	{
		Parent = applicationObject;
	}
}
public abstract class NavMethodScope : NavScope, ISessionProvider, IRawScopeIdProvider
{
	private class RootMethodScope : NavMethodScope
	{
		public override bool IsInsideTryCall => false;

		public override NavApplicationObjectBase ApplicationObject => null;

		public RootMethodScope(NavSession session)
			: base(session)
		{
		}
	}

	private class TryMethodScope : NavMethodScope
	{
		public override bool IsInsideTryCall => true;

		public override NavApplicationObjectBase ApplicationObject => null;

		internal TryMethodScope(NavApplicationObjectBase applicationObject)
			: base(applicationObject)
		{
		}
	}

	private static ConcurrentDictionary<Type, string> methodNameDictionary = new ConcurrentDictionary<Type, string>();

	internal const int ExitStatementNumber = int.MaxValue;

	private const int MaxStackDepth = 1000;

	private const int CriticalStackDepth = 400;

	private int statementNumber;

	private bool isALFunctionStartEventLogged;

	private Stopwatch executionTimer;

	private Dictionary<TableConnectionType, string> scopedTableConnection;

	public abstract NavApplicationObjectBase ApplicationObject { get; }

	public virtual bool IsDebuggable => true;

	public virtual NavEventScope EventScope
	{
		get
		{
			return null;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public virtual bool IsEvent => false;

	public virtual bool IsInsideTryCall => ParentScope.IsInsideTryCall;

	public virtual string TryFunctionName
	{
		get
		{
			NavMethodScope parentScope = ParentScope;
			if (parentScope == null)
			{
				return string.Empty;
			}
			if (!(parentScope is TryMethodScope))
			{
				return parentScope.ScopeName;
			}
			return ScopeName;
		}
	}

	public NavSession Session { get; private set; }

	public bool HasUnhandledException { get; private set; }

	public uint[] IndirectPermissionList
	{
		get
		{
			if (ApplicationObject == null)
			{
				return NavApplicationObjectBase.EmptyIndirectPermissionList;
			}
			return ApplicationObject.GetIndirectPermissionList();
		}
	}

	public bool IsAtExitStatement => statementNumber == int.MaxValue;

	public bool IsRootScope => StackDepth == 1;

	public bool IsTopLevelCall => StackDepth == 2;

	public virtual bool IsStackFrame => false;

	public NavMethodScope ParentScope => Tree.Parent as NavMethodScope;

	protected virtual uint RawScopeId
	{
		get
		{
			return 0u;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	uint IRawScopeIdProvider.RawScopeId
	{
		get
		{
			return RawScopeId;
		}
		set
		{
			RawScopeId = value;
		}
	}

	internal bool HasBreakpointsDefined => ALCodeEnvironment.CheckHasBreakpointInScopeFlag(RawScopeId);

	internal int ScopeId => ALCodeEnvironment.RawScopeIdToScopeId(RawScopeId);

	public string ScopeName => MethodName(GetType());

	public IEnumerable<NavMethodScope> StackTrace
	{
		get
		{
			NavMethodScope scope = this;
			while (!scope.IsRootScope)
			{
				if (scope.IsStackFrame)
				{
					yield return scope;
				}
				scope = scope.ParentScope;
			}
		}
	}

	public int StatementNumber => statementNumber;

	public bool IsALFunctionStartEventLogged
	{
		get
		{
			return isALFunctionStartEventLogged;
		}
		set
		{
			isALFunctionStartEventLogged = value;
			if (value && executionTimer == null)
			{
				executionTimer = new Stopwatch();
				executionTimer.Start();
			}
		}
	}

	public long ExecutionMilliseconds => executionTimer?.ElapsedMilliseconds ?? 0;

	public bool IsALFunctionSignatureEventLogged { get; set; }

	internal int StackDepth { get; private set; }

	protected NavMethodScope(NavApplicationObjectBase applicationObject)
		: this(applicationObject, eventSource: false)
	{
	}

	protected NavMethodScope(NavApplicationObjectBase applicationObject, bool eventSource)
		: base(applicationObject.Session.CurrentMethodScope)
	{
		Session = applicationObject.Session;
		if (!eventSource)
		{
			Session.VerifyExecutePermission(applicationObject);
		}
		CheckAndSetStackDepth(Session.CurrentMethodScope, eventSource);
		Session.CurrentMethodScope = this;
	}

	private NavMethodScope(NavSession session)
		: base(session)
	{
		Session = session;
		StackDepth = 1;
	}

	internal NavMethodScope(ITreeObject parent)
		: base(parent)
	{
	}

	public NavEventScope GetEventScope(EventScopeGetOption getOption = EventScopeGetOption.None)
	{
		NavEventScope navEventScope = EventScope;
		if (navEventScope == null && getOption == EventScopeGetOption.CreateIfNotFound)
		{
			lock (this)
			{
				navEventScope = EventScope;
				if (navEventScope == null)
				{
					navEventScope = (EventScope = new NavEventScope());
				}
			}
		}
		if (navEventScope != null && (getOption == EventScopeGetOption.RemoveIfFound || getOption == EventScopeGetOption.SetStaleIfFound))
		{
			lock (this)
			{
				navEventScope = EventScope;
				if (navEventScope.ClearEventScope())
				{
					EventScope = ((getOption == EventScopeGetOption.RemoveIfFound) ? null : NavEventScope.StaleEventScope);
				}
			}
		}
		return navEventScope;
	}

	public NavMethodScope GetParentMethodScope()
	{
		NavMethodScope parentScope = ParentScope;
		while (parentScope != null && !parentScope.IsStackFrame)
		{
			parentScope = parentScope.ParentScope;
		}
		return parentScope;
	}

	internal void SetDefaultTableConnection(TableConnectionType connectionType, string connection)
	{
		if (scopedTableConnection == null)
		{
			scopedTableConnection = new Dictionary<TableConnectionType, string>();
		}
		else
		{
			scopedTableConnection.Remove(connectionType);
		}
		scopedTableConnection.Add(connectionType, connection);
	}

	internal string GetDefaultTableConnection(TableConnectionType connectionType)
	{
		string value = null;
		if (scopedTableConnection == null || !scopedTableConnection.TryGetValue(connectionType, out value))
		{
			return null;
		}
		return value;
	}

	internal string GetDefaultTableConnectionInScope(TableConnectionType connectionType)
	{
		return StackTrace.Select((NavMethodScope scope) => scope.GetDefaultTableConnection(connectionType)).FirstOrDefault((string result) => result != null);
	}

	public static NavMethodScope CreateRootMethodScope(NavSession session)
	{
		return new RootMethodScope(session);
	}

	public void Run()
	{
		try
		{
			if (IsEvent)
			{
				if (Session.IsEventSessionRecorderEnabled)
				{
					WriteEventRaised();
				}
				if (EventScope != null)
				{
					OnRunEvent();
				}
			}
			else
			{
				OnRun();
			}
		}
		catch (Exception exception) when (ProcessException(exception))
		{
		}
		catch (DivideByZeroException ex)
		{
			throw new NavALException(ex.Message, ex);
		}
		catch (IndexOutOfRangeException ex2)
		{
			throw new NavALException(ex2.Message, ex2);
		}
		catch (FormatException ex3)
		{
			throw new NavALException(ex3.Message, ex3);
		}
		catch (OverflowException ex4)
		{
			throw new NavALException(ex4.Message, ex4);
		}
		finally
		{
			Exit();
		}
	}

	private void WriteEventRaised()
	{
		ApplicationObjectId objectId = ApplicationObject.ObjectId;
		if (ParentScope?.ApplicationObject != null)
		{
			ApplicationObjectId objectId2 = ParentScope.ApplicationObject.ObjectId;
			NavEventTelemetryRegistrar eventTelemetryRegistrar = NavCurrentThread.Session.EventTelemetryRegistrar;
			string scopeName = ScopeName;
			string empty = string.Empty;
			int objectNumber = objectId.ObjectNumber;
			NavServerEventWriter.WriteEventRaised(objectType: (int)objectId.ObjectType, objectId: objectNumber, eventName: scopeName, elementName: empty, eventType: 1, callingMethodName: ParentScope.ScopeName, callingObjectId: objectId2.ObjectNumber, registrar: eventTelemetryRegistrar, callingObjectType: (int)objectId2.ObjectType);
		}
		else
		{
			NavServerEventWriter.WriteEventRaised(NavCurrentThread.Session.EventTelemetryRegistrar, eventName: ScopeName, elementName: string.Empty, objectId: objectId.ObjectNumber, objectType: (int)objectId.ObjectType, eventType: 1);
		}
	}

	protected internal virtual void OnRun()
	{
	}

	protected internal void OnRunEvent()
	{
		StmtHit(0);
		EventScope?.CheckAndFireEvents(this);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected bool CStmtHit(int currentStatementNumber)
	{
		statementNumber = currentStatementNumber;
		ExecutionListener.Instance?.ProcessStatementHit(this);
		return true;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal void Exit()
	{
		statementNumber = int.MaxValue;
		ExecutionListener.Instance?.ProcessExit(this);
		Dispose();
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	internal bool ProcessException(Exception exception)
	{
		if (exception is NavControlException)
		{
			return false;
		}
		if (exception is NullReferenceException)
		{
			System.Diagnostics.StackFrame stackFrame = new System.Diagnostics.StackFrame(2);
			NavCurrentThread.Session.Diagnostics.SendExceptionTag("00000HG", Category.EventSubscription, exception, Verbosity.Error, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "NullReferenceException occured in {0}", stackFrame.GetMethod().Name)));
		}
		HasUnhandledException = true;
		if (ParentScope != null && ParentScope.HasUnhandledException)
		{
			return false;
		}
		NavCurrentThread.Session.LastException = exception;
		ExecutionListener.Instance?.ProcessException(this, exception);
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	protected void StmtHit(int currentStatementNumber)
	{
		statementNumber = currentStatementNumber;
		ExecutionListener.Instance?.ProcessStatementHit(this);
		NavThread.Throttle();
	}

	internal static string MethodName(Type scopeType)
	{
		if (scopeType == null)
		{
			throw new ArgumentNullException("scopeType");
		}
		return methodNameDictionary.GetOrAdd(scopeType, (Type key) =>
		{
			object[] customAttributes = scopeType.GetCustomAttributes(typeof(NavNameAttribute), inherit: false);
			int num = 0;
			if (num < customAttributes.Length)
			{
				NavNameAttribute navNameAttribute = (NavNameAttribute)customAttributes[num];
				return navNameAttribute.Name;
			}
			string name = scopeType.Name;
			return name.EndsWith("_Scope", StringComparison.Ordinal) ? name.Substring(0, name.Length - 6) : scopeType.Name;
		});
	}

	protected void AssertError(Action body)
	{
		try
		{
			try
			{
				body();
			}
			catch (DivideByZeroException ex)
			{
				throw new NavALException(ex.Message, ex);
			}
			catch (IndexOutOfRangeException ex2)
			{
				throw new NavALException(ex2.Message, ex2);
			}
			catch (FormatException ex3)
			{
				throw new NavALException(ex3.Message, ex3);
			}
			catch (OverflowException ex4)
			{
				throw new NavALException(ex4.Message, ex4);
			}
		}
		catch (NavBaseException)
		{
			Session.Rollback();
			return;
		}
		catch (Exception innerException)
		{
			while (innerException != null)
			{
				if (innerException is NavBaseException)
				{
					Session.Rollback();
					return;
				}
				innerException = innerException.InnerException;
			}
			throw;
		}
		throw new NavNCLAssertErrorException();
	}

	internal virtual void PushTriggerContext()
	{
		if (ParentScope != null)
		{
			ParentScope.PushTriggerContext();
		}
	}

	protected override void Dispose(bool disposing)
	{
		bool flag = disposing && !Tree.IsDisposed;
		try
		{
			if (flag)
			{
				Tree.DisposeAllChildren();
			}
		}
		finally
		{
			if (flag)
			{
				Session.CurrentMethodScope = ParentScope;
			}
			base.Dispose(disposing);
		}
	}

	private void CheckAndSetStackDepth(NavMethodScope parentScope, bool eventSource)
	{
		StackDepth = parentScope.StackDepth + ((!eventSource) ? 1 : 2);
		if (StackDepth > 1000)
		{
			throw NavCurrentThread.Session.Diagnostics.SendExceptionTag("000004N", Category.Runtime, new NavNCLStackOverflowException());
		}
		if (StackDepth > 400)
		{
			try
			{
				RuntimeHelpers.EnsureSufficientExecutionStack();
			}
			catch (InsufficientExecutionStackException)
			{
				throw NavCurrentThread.Session.Diagnostics.SendExceptionTag("000004O", Category.Runtime, new NavNCLStackOverflowException());
			}
		}
	}

	internal NavMethodScope GetTryMethodScope()
	{
		return new TryMethodScope(ApplicationObject);
	}

	public bool TryInvoke(Action method)
	{
		return NavApplicationObjectBase.TryInvoke(method);
	}
}
