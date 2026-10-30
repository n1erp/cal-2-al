using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TaskScheduleAppTableEntry
{
	internal Guid Id { get; }

	internal DateTime NotBefore { get; }

	internal int State { get; }

	internal NavTaskSchedulerTaskType TaskType { get; }

	public TaskScheduleAppTableEntry(Guid id, DateTime notBefore, int state, NavTaskSchedulerTaskType taskType)
	{
		Id = id;
		NotBefore = notBefore;
		State = state;
		TaskType = taskType;
	}
}
