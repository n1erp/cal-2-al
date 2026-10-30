using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavWorkflowTenantSessionManager
{
	Guid ClientSessionId { get; set; }

	Guid ClientActivityId { get; set; }

	bool SingleSessionMode { get; }

	void RunActionInSystemSession(Action<NavSession> action);

	void RunActionInSystemSessionWithoutTransaction(Action<NavSession> action);

	void StopSynchronouslyExecutionAtAllSessions();

	void StopSynchronouslyExecutionAndDisposeSession(int sessionId, bool removedFromTrackedCollection);

	bool IsPendingStopOperation(int sessionId);

	void CreateTenantExclusiveActionScope();
}
