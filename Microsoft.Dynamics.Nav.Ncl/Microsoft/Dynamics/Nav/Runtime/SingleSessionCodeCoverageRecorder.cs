using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SingleSessionCodeCoverageRecorder : CodeCoverageRecorder
{
	private readonly Action<SingleSessionCodeCoverageRecorder> onStop;

	private readonly Stack<CodeCoverageScopeCounter> stack = new Stack<CodeCoverageScopeCounter>(16);

	private Dictionary<int, CodeCoverageScopeCounter> scopeCounterLookup = new Dictionary<int, CodeCoverageScopeCounter>(128);

	public override IEnumerable<CodeCoverageScopeCounter> ScopeCounters => scopeCounterLookup.Values;

	internal Dictionary<int, CodeCoverageScopeCounter> ScopeCounterLookup
	{
		[DebuggerStepThrough]
		get
		{
			return scopeCounterLookup;
		}
	}

	internal SingleSessionCodeCoverageRecorder(NavSession targetSession, ALCodeEnvironment codeEnvironment, Action<SingleSessionCodeCoverageRecorder> onStop)
		: base(targetSession, codeEnvironment)
	{
		if (onStop == null)
		{
			throw new ArgumentNullException("onStop");
		}
		this.onStop = onStop;
	}

	public override void ProcessExit(NavMethodScope methodScope)
	{
		if (methodScope.Session == OwningSession && stack.Count > 0)
		{
			stack.Pop();
		}
	}

	public override void ProcessStatementHit(NavMethodScope methodScope)
	{
		if (methodScope.Session != OwningSession)
		{
			return;
		}
		int orAssignScopeId = CodeEnvironment.GetOrAssignScopeId(methodScope);
		CodeCoverageScopeCounter value;
		if (stack.Count == 0 || (value = stack.Peek()).ScopeId != orAssignScopeId)
		{
			if (!scopeCounterLookup.TryGetValue(orAssignScopeId, out value))
			{
				value = new CodeCoverageScopeCounter(orAssignScopeId);
				scopeCounterLookup.Add(orAssignScopeId, value);
			}
			stack.Push(value);
		}
		value.IncrementHitCount(methodScope.StatementNumber);
	}

	public override void SetHitCounts(Dictionary<int, CodeCoverageScopeCounter> scopeCounters)
	{
		scopeCounterLookup = scopeCounters;
		stack.Clear();
	}

	public override void StopRecording()
	{
		onStop(this);
	}
}
