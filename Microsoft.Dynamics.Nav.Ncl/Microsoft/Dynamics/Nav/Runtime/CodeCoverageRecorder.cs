using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal abstract class CodeCoverageRecorder : TreeObject, IExecutionListener
{
	private NavSession owningSession;

	private readonly ALCodeEnvironment codeEnvironment;

	public NavSession OwningSession => owningSession;

	public ALCodeEnvironment CodeEnvironment => codeEnvironment;

	public abstract IEnumerable<CodeCoverageScopeCounter> ScopeCounters { get; }

	protected CodeCoverageRecorder(NavSession owningSession, ALCodeEnvironment codeEnvironment)
		: base(owningSession.Company)
	{
		if (codeEnvironment == null)
		{
			throw new ArgumentNullException("codeEnvironment");
		}
		this.owningSession = owningSession;
		this.codeEnvironment = codeEnvironment;
	}

	public abstract void StopRecording();

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing)
			{
				StopRecording();
				owningSession = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	public abstract void ProcessExit(NavMethodScope methodScope);

	public abstract void ProcessStatementHit(NavMethodScope methodScope);

	public abstract void SetHitCounts(Dictionary<int, CodeCoverageScopeCounter> scopeCounters);

	public void ProcessException(NavMethodScope methodScope, Exception exception)
	{
	}

	public bool CanCoexistWithOtherListener(IExecutionListener otherListener)
	{
		if (otherListener is CodeCoverageRecorder codeCoverageRecorder)
		{
			return codeCoverageRecorder.OwningSession != OwningSession;
		}
		return true;
	}
}
