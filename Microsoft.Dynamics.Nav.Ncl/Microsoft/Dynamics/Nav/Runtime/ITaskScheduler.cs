using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal interface ITaskScheduler : IDisposable
{
	void Start();

	void Stop();

	void CreateTask(Guid id, string tenantId, DateTime notBefore, bool isReady, NavTaskSchedulerTaskType taskType);

	bool TaskExists(Guid id, string tenantId);

	bool CancelTask(Guid id, string tenantId);

	bool SetReady(Guid id, string tenantId, DateTime notBefore);

	void ForceTaskListSynchronization(string tenantId);
}
