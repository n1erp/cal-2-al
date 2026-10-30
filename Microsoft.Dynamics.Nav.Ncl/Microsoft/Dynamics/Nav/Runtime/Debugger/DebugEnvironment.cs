using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class DebugEnvironment : IDisposable
{
	private static LocalNode[] EmptyLocalNode = new LocalNode[0];

	private static readonly Regex memberNameDoesNotNeedQuotesRegex = new Regex("^[_\\w][_\\w\\d]*$", RegexOptions.Compiled);

	private readonly ALCodeEnvironment alCodeEnvironment;

	private readonly object lockObject = new object();

	private readonly Dictionary<int, MethodSourceInfo> methodScopesById;

	private readonly MethodScopeScaffolding localsScaffolding;

	private readonly WatchItemsScaffolding watchItemScaffolding;

	private FastBreakpointList breakpoints;

	private NavMethodScope currentBreakMethodScope;

	public FastBreakpointList Breakpoints
	{
		[DebuggerStepThrough]
		get
		{
			return breakpoints;
		}
	}

	public INavDebuggerCallback DebuggerCallback { get; set; }

	internal DebugEnvironment(ALCodeEnvironment alCodeEnvironment)
	{
		methodScopesById = new Dictionary<int, MethodSourceInfo>();
		breakpoints = new FastBreakpointList();
		this.alCodeEnvironment = alCodeEnvironment;
		this.alCodeEnvironment.MetaObjectReplaced += OnMetaObjectReplaced;
		localsScaffolding = new MethodScopeScaffolding();
		watchItemScaffolding = new WatchItemsScaffolding();
	}

	internal DebugEnvironment(INavDebuggerCallback debuggerCallback, ALCodeEnvironment alCodeEnvironment)
		: this(alCodeEnvironment)
	{
		if (debuggerCallback == null)
		{
			throw new ArgumentNullException("debuggerCallback");
		}
		DebuggerCallback = debuggerCallback;
	}

	public BreakpointDefinition[] GetBreakpointsForObject(ApplicationObjectId objectId)
	{
		EnsureLatestAppTypeLoaded(objectId);
		lock (lockObject)
		{
			IEnumerable<BreakpointDefinition> source = from _003C_003Eh__TransparentIdentifier0 in methodScopesById.Select((KeyValuePair<int, MethodSourceInfo> pair) =>
				{
					KeyValuePair<int, MethodSourceInfo> keyValuePair = pair;
					return new
					{
						pair = pair,
						scopeId = keyValuePair.Key
					};
				})
				let methodDebugInfo = pair.Value
				where methodDebugInfo.ApplicationObject.AppQualifiedObjectId.UnqualifiedId == objectId
				join breakpoint in breakpoints on scopeId equals breakpoint.ScopeId
				let statement = methodDebugInfo.GetStatement(breakpoint.StatementNumber)
				select new BreakpointDefinition(objectId, statement.SourceSpan, statement.RelativeSourceSpan, breakpoint.Id, methodDebugInfo.Name, methodDebugInfo.LocalsType.Name, breakpoint.ConditionExpression);
			return source.ToArray();
		}
	}

	public BreakpointDefinition AddBreakpoint(ApplicationObjectId objectId, SourcePosition position, string condition)
	{
		EnsureLatestAppTypeLoaded(objectId);
		StatementSourceInfo statementFromSourcePosition = GetStatementFromSourcePosition(objectId, position);
		if (statementFromSourcePosition == null)
		{
			throw new NavDebuggerInvalidBreakpointException();
		}
		return AddBreakpoint(objectId, statementFromSourcePosition, condition);
	}

	public BreakpointDefinition AddBreakpoint(ApplicationObjectId objectId, StatementSourceInfo statement, string condition)
	{
		BreakpointDefinition result = AddBreakpointInternal(objectId, statement, condition, out var breakpointAdded);
		if (!breakpointAdded)
		{
			throw new NavDebuggerDuplicateBreakpointException();
		}
		return result;
	}

	public BreakpointDefinition AddBreakpoint(ApplicationObjectId objectId, SourcePosition position, string condition, out bool breakpointAdded)
	{
		EnsureLatestAppTypeLoaded(objectId);
		StatementSourceInfo statementFromSourcePosition = GetStatementFromSourcePosition(objectId, position);
		if (statementFromSourcePosition == null)
		{
			throw new NavDebuggerInvalidBreakpointException();
		}
		return AddBreakpointInternal(objectId, statementFromSourcePosition, condition, out breakpointAdded);
	}

	public BreakpointDefinition AddBreakpoint(ApplicationObjectId objectId, string methodName, int relativeLineNumber)
	{
		EnsureLatestAppTypeLoaded(objectId);
		StatementSourceInfo statementFromRelativeLineNumberInMethod = GetStatementFromRelativeLineNumberInMethod(objectId, methodName, relativeLineNumber);
		if (statementFromRelativeLineNumberInMethod == null)
		{
			throw new NavDebuggerInvalidBreakpointException();
		}
		return AddBreakpoint(objectId, statementFromRelativeLineNumberInMethod, null);
	}

	public BreakpointDefinition GetBreakpointDefinition(ApplicationObjectId objectId, SourcePosition position, string condition)
	{
		EnsureLatestAppTypeLoaded(objectId);
		StatementSourceInfo statementFromSourcePosition = GetStatementFromSourcePosition(objectId, position);
		if (statementFromSourcePosition == null)
		{
			throw new NavDebuggerInvalidBreakpointException();
		}
		int orAssignScopeId = alCodeEnvironment.GetOrAssignScopeId(objectId, statementFromSourcePosition.Method.LocalsType, setBreakpointFlag: true);
		return new BreakpointDefinition(objectId, statementFromSourcePosition.SourceSpan, statementFromSourcePosition.RelativeSourceSpan, Breakpoint.BreakpointId(orAssignScopeId, statementFromSourcePosition.StatementNumber), statementFromSourcePosition.Method.Name, statementFromSourcePosition.Method.LocalsType.Name, condition);
	}

	public BreakpointDefinition GetBreakpointDefinition(ApplicationObjectId objectId, string methodName, int relativeLineNumber)
	{
		EnsureLatestAppTypeLoaded(objectId);
		StatementSourceInfo statementFromRelativeLineNumberInMethod = GetStatementFromRelativeLineNumberInMethod(objectId, methodName, relativeLineNumber);
		if (statementFromRelativeLineNumberInMethod == null)
		{
			throw new NavDebuggerInvalidBreakpointException();
		}
		int orAssignScopeId = alCodeEnvironment.GetOrAssignScopeId(objectId, statementFromRelativeLineNumberInMethod.Method.LocalsType, setBreakpointFlag: true);
		return new BreakpointDefinition(objectId, statementFromRelativeLineNumberInMethod.SourceSpan, statementFromRelativeLineNumberInMethod.RelativeSourceSpan, Breakpoint.BreakpointId(orAssignScopeId, statementFromRelativeLineNumberInMethod.StatementNumber), statementFromRelativeLineNumberInMethod.Method.Name, statementFromRelativeLineNumberInMethod.Method.LocalsType.Name, null);
	}

	public bool RemoveBreakpoint(long breakpointId)
	{
		lock (lockObject)
		{
			breakpoints = breakpoints.Remove(breakpointId, out var breakpointRemoved);
			if (breakpointRemoved == null)
			{
				return false;
			}
			if (!breakpoints.Any((Breakpoint bp) => bp.ScopeId == breakpointRemoved.ScopeId) && methodScopesById.TryGetValue(breakpointRemoved.ScopeId, out var value))
			{
				ALCodeEnvironment.ClearBreakpointOnMethodScopeFlag(value.LocalsType);
				methodScopesById.Remove(breakpointRemoved.ScopeId);
			}
			return true;
		}
	}

	public void RemoveAllBreakpoints()
	{
		lock (lockObject)
		{
			foreach (MethodSourceInfo value in methodScopesById.Values)
			{
				ALCodeEnvironment.ClearBreakpointOnMethodScopeFlag(value.LocalsType);
			}
			methodScopesById.Clear();
			breakpoints = new FastBreakpointList();
		}
	}

	public void UpdateBreakpointCondition(BreakpointDefinition breakpoint)
	{
		lock (lockObject)
		{
			breakpoints = breakpoints.Replace(new Breakpoint(breakpoint.BreakpointId, breakpoint.Condition));
		}
	}

	public void UpdateBreakpointCondition(long id, string condition)
	{
		lock (lockObject)
		{
			breakpoints = breakpoints.Replace(new Breakpoint(id, condition));
		}
	}

	public string AddWatch(string pathExpression, int? stackFrameIndex)
	{
		NavMethodScope methodScope = ((stackFrameIndex.HasValue && currentBreakMethodScope != null) ? currentBreakMethodScope.StackTrace.ElementAt(stackFrameIndex.Value) : null);
		return watchItemScaffolding.AddPath(pathExpression, methodScope);
	}

	public void DeleteWatch(string pathExpression)
	{
		watchItemScaffolding.DeletePath(pathExpression);
	}

	public void RemoveAllWatches()
	{
		watchItemScaffolding.RemoveAllPaths();
	}

	public LocalNode[] ExpandGlobals(int index)
	{
		if (currentBreakMethodScope == null)
		{
			return EmptyLocalNode;
		}
		NavMethodScope navMethodScope = GetNavMethodScope(index);
		if (navMethodScope == null)
		{
			return EmptyLocalNode;
		}
		return localsScaffolding.ForceUpdateGlobals(navMethodScope, showTextConstants: false);
	}

	public LocalNode[] ExpandLocal(int stackFrameIndex, string path, bool showTextConstants)
	{
		if (currentBreakMethodScope == null)
		{
			return EmptyLocalNode;
		}
		NavMethodScope navMethodScope = GetNavMethodScope(stackFrameIndex);
		if (navMethodScope == null)
		{
			return EmptyLocalNode;
		}
		return localsScaffolding.ExpandPath(path, navMethodScope, showTextConstants).ToArray();
	}

	public void CollapseLocal(int stackFrameIndex, string path)
	{
		if (currentBreakMethodScope != null)
		{
			NavMethodScope navMethodScope = GetNavMethodScope(stackFrameIndex);
			if (navMethodScope != null)
			{
				localsScaffolding.CollapsePath(path, navMethodScope);
			}
		}
	}

	public LocalNode[] GetLocals(int index, ExpandLevels expandLevel = ExpandLevels.None)
	{
		if (currentBreakMethodScope == null)
		{
			return EmptyLocalNode;
		}
		NavMethodScope navMethodScope = GetNavMethodScope(index);
		if (navMethodScope == null)
		{
			return EmptyLocalNode;
		}
		return localsScaffolding.Update(navMethodScope, getNodes: true, showTextConstants: false, expandLevel);
	}

	public (string Content, bool IsNavAppApplicationObjectSource) GetSource(ApplicationObjectId appId)
	{
		ObjectAlSourceCodeInfo sourceCodeInfo = alCodeEnvironment.GetSourceCodeInfo(appId);
		if (sourceCodeInfo == null)
		{
			return (Content: string.Empty, IsNavAppApplicationObjectSource: false);
		}
		return (Content: sourceCodeInfo.SourceCode, IsNavAppApplicationObjectSource: sourceCodeInfo.IsNavAppApplicationObjectSource);
	}

	internal IEnumerable<LocalNodeContext> BuildVariables(int stackFrameIndex, LocalNode[] localNodes, int indentation, string parentPath, bool referencedGlobalsProcessed)
	{
		foreach (LocalNode node in localNodes)
		{
			bool globalsStateForNode = referencedGlobalsProcessed;
			if (node.Name == "<Globals>" && string.IsNullOrEmpty(node.TypeName))
			{
				if (globalsStateForNode)
				{
					continue;
				}
				globalsStateForNode = true;
			}
			string path = ((!string.IsNullOrEmpty(parentPath)) ? (parentPath + '.' + QuoteNameIfNeeded(node.Name)) : QuoteNameIfNeeded(node.Name));
			yield return new LocalNodeContext(indentation, path, node);
			if (!node.HasChildren)
			{
				continue;
			}
			LocalNode[] localNodes2 = ExpandLocal(stackFrameIndex, path, showTextConstants: true);
			foreach (LocalNodeContext item in BuildVariables(stackFrameIndex, localNodes2, indentation + 1, path, globalsStateForNode))
			{
				yield return item;
			}
		}
	}

	internal static string QuoteNameIfNeeded(string name)
	{
		if (!memberNameDoesNotNeedQuotesRegex.IsMatch(name))
		{
			return "\"" + name + "\"";
		}
		return name;
	}

	public void SetLocal(int stackFrameIndex, string pathExpression, string newValue)
	{
		NavMethodScope methodScope = currentBreakMethodScope.StackTrace.ElementAt(stackFrameIndex);
		localsScaffolding.TrySetDescendantValue(pathExpression, methodScope, newValue);
	}

	public LocalNode[] GetWatchNodes(int stackFrameIndex)
	{
		NavMethodScope navMethodScope = currentBreakMethodScope;
		if (navMethodScope == null)
		{
			return watchItemScaffolding.UpdateNoMethodScope().ToArray();
		}
		NavMethodScope navMethodScope2 = GetNavMethodScope(stackFrameIndex);
		if (navMethodScope2 == null)
		{
			return watchItemScaffolding.UpdateNoMethodScope().ToArray();
		}
		return watchItemScaffolding.Update(navMethodScope2).ToArray();
	}

	public LocalNode GetWatchNode(string expression, int stackFrameIndex)
	{
		NavMethodScope navMethodScope = GetNavMethodScope(stackFrameIndex);
		if (navMethodScope == null)
		{
			return new LocalNode(expression, string.Empty, string.Empty, hasChildren: false, LocalNodeChangeState.New, null);
		}
		return watchItemScaffolding.GetLocalNode(expression, navMethodScope);
	}

	public void OnFatalDebuggerException(string debuggingContext, Exception exception)
	{
		FaultException<ServerFault> ex = ExceptionHandler.CreateFaultException(exception);
		try
		{
			DebuggerCallback.OnFatalDebuggerException(debuggingContext, ex.Detail);
		}
		catch (CommunicationException)
		{
		}
	}

	public BreakpointResponse PerformBreakCallback(string debuggingContext, NavMethodScope methodScope, Exception exception)
	{
		currentBreakMethodScope = methodScope;
		try
		{
			ApplicationObjectId objectId;
			Microsoft.Dynamics.Nav.Types.StackFrame[] stack;
			LocalNode[] localNodes;
			try
			{
				objectId = ALCodeEnvironment.EffectiveObjectId(methodScope.ApplicationObject);
				stack = BuildStackTrace(methodScope);
				localNodes = (string.IsNullOrEmpty(debuggingContext) ? localsScaffolding.Update(methodScope, getNodes: true) : null);
			}
			catch (Exception ex) when (!IsCriticalException(ex))
			{
				OnFatalDebuggerException(debuggingContext, ex);
				throw new DebuggerFatalException(Lang.FatalDebuggerError, ex);
			}
			try
			{
				return DebuggerCallback.Break(debuggingContext, objectId, stack, localNodes, exception?.Message);
			}
			catch (CommunicationException inner)
			{
				throw new DebuggerFatalException(Lang.FatalDebuggerError, inner);
			}
		}
		finally
		{
			currentBreakMethodScope = null;
		}
	}

	public string GetObjectName(ApplicationObjectId id)
	{
		return alCodeEnvironment.GetSourceCodeInfo(id)?.Name;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	internal static bool IsCriticalException(Exception ex)
	{
		if (!(ex is NullReferenceException) && !(ex is StackOverflowException) && !(ex is OutOfMemoryException) && !(ex is ThreadAbortException) && !(ex is IndexOutOfRangeException))
		{
			return ex is AccessViolationException;
		}
		return true;
	}

	private BreakpointDefinition AddBreakpointInternal(ApplicationObjectId objectId, StatementSourceInfo statement, string condition, out bool breakpointAdded)
	{
		if (statement == null)
		{
			throw new ArgumentNullException("statement");
		}
		int orAssignScopeId = alCodeEnvironment.GetOrAssignScopeId(objectId, statement.Method.LocalsType, setBreakpointFlag: true);
		breakpointAdded = false;
		lock (lockObject)
		{
			BreakpointDefinition result = new BreakpointDefinition(objectId, statement.SourceSpan, statement.RelativeSourceSpan, Breakpoint.BreakpointId(orAssignScopeId, statement.StatementNumber), statement.Method.Name, statement.Method.LocalsType.Name, condition);
			if (breakpoints[orAssignScopeId, statement.StatementNumber] == null)
			{
				methodScopesById[orAssignScopeId] = statement.Method;
				Breakpoint breakpoint = new Breakpoint(orAssignScopeId, statement.StatementNumber, condition);
				breakpoints = breakpoints.Add(breakpoint);
				breakpointAdded = true;
			}
			return result;
		}
	}

	private static Microsoft.Dynamics.Nav.Types.StackFrame[] BuildStackTrace(NavMethodScope topMethodScope)
	{
		return topMethodScope.StackTrace.Select((NavMethodScope scope) => new Microsoft.Dynamics.Nav.Types.StackFrame(ALCodeEnvironment.EffectiveObjectId(scope.ApplicationObject), scope.ApplicationObject.ObjectName, scope.ScopeName, MethodSourceInfo.GetCurrentStatementSourceSpan(scope))).ToArray();
	}

	private void EnsureLatestAppTypeLoaded(ApplicationObjectId objectId)
	{
		alCodeEnvironment.GetMetaApplicationObject(objectId);
	}

	private void Dispose(bool disposing)
	{
		if (!disposing)
		{
			return;
		}
		alCodeEnvironment.MetaObjectReplaced -= OnMetaObjectReplaced;
		lock (lockObject)
		{
			foreach (MethodSourceInfo value in methodScopesById.Values)
			{
				ALCodeEnvironment.ClearBreakpointOnMethodScopeFlag(value.LocalsType);
			}
		}
	}

	private StatementSourceInfo GetStatementFromSourcePosition(ApplicationObjectId objectId, SourcePosition position)
	{
		lock (lockObject)
		{
			ObjectSourceInfo sourceInfo = alCodeEnvironment.GetSourceInfo(objectId);
			return sourceInfo.FindStatementForPosition(position);
		}
	}

	private StatementSourceInfo GetStatementFromRelativeLineNumberInMethod(ApplicationObjectId objectId, string methodName, int relativeLineNumber)
	{
		lock (lockObject)
		{
			ObjectSourceInfo sourceInfo = alCodeEnvironment.GetSourceInfo(objectId);
			return sourceInfo.FindStatementFromRelativeLineNumberInMethod(methodName, relativeLineNumber);
		}
	}

	private void OnMetaObjectReplaced(object sender, EventArgs<ApplicationObjectId> e)
	{
		ApplicationObjectId objectId = e.Data;
		lock (lockObject)
		{
			List<KeyValuePair<int, MethodSourceInfo>> list = methodScopesById.Where((KeyValuePair<int, MethodSourceInfo> p) => p.Value.ApplicationObject.AppQualifiedObjectId.UnqualifiedId == objectId).ToList();
			if (list.Count == 0)
			{
				return;
			}
			List<Func<Breakpoint, bool>> filters = new List<Func<Breakpoint, bool>>();
			List<Func<Breakpoint, Breakpoint>> maps = new List<Func<Breakpoint, Breakpoint>>();
			ObjectSourceInfo sourceInfo = alCodeEnvironment.GetSourceInfo(objectId);
			foreach (KeyValuePair<int, MethodSourceInfo> item in list)
			{
				string name = item.Value.LocalsType.Name;
				MethodSourceInfo newMethod = sourceInfo.Methods.FirstOrDefault((MethodSourceInfo m) => m.LocalsType.Name == name);
				int scopeId = item.Key;
				if (newMethod == null)
				{
					ALCodeEnvironment.ClearBreakpointOnMethodScopeFlag(item.Value.LocalsType);
					methodScopesById.Remove(scopeId);
					filters.Add((Breakpoint bp) => bp.ScopeId != scopeId);
					continue;
				}
				methodScopesById[scopeId] = newMethod;
				filters.Add((Breakpoint bp) => bp.ScopeId != scopeId || bp.StatementNumber < newMethod.LastStatementNumber || (bp.StatementNumber == newMethod.LastStatementNumber && breakpoints[scopeId, int.MaxValue] == null) || bp.StatementNumber == int.MaxValue);
				maps.Add((Breakpoint bp) => (bp.StatementNumber != newMethod.LastStatementNumber) ? bp : new Breakpoint(scopeId, int.MaxValue, bp.ConditionExpression));
			}
			IEnumerable<Breakpoint> enumerable = from bp in breakpoints
				where filters.TrueForAll((Func<Breakpoint, bool> f) => f(bp))
				select maps.Aggregate(bp, (Breakpoint arg, Func<Breakpoint, Breakpoint> map) => map(arg));
			breakpoints = new FastBreakpointList(enumerable);
		}
	}

	private NavMethodScope GetNavMethodScope(int stackFrameIndex)
	{
		if (currentBreakMethodScope == null)
		{
			return null;
		}
		List<NavMethodScope> list = currentBreakMethodScope.StackTrace.ToList();
		if (stackFrameIndex >= list.Count || stackFrameIndex < 0)
		{
			return null;
		}
		return list[stackFrameIndex];
	}
}
