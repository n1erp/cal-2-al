using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TaskScheduleInfo
{
	public Guid Id { get; }

	public string TenantId { get; }

	public int State { get; protected set; }

	public int Retries { get; protected set; }

	public NavTaskSchedulerTaskType TaskType { get; set; }

	public bool IsReady
	{
		get
		{
			if (State != 1)
			{
				return State == 2;
			}
			return true;
		}
	}

	public bool IsDone => State == 4;

	public bool IsStartingOrRunning
	{
		get
		{
			if (State != 3)
			{
				return State == 2;
			}
			return true;
		}
	}

	public TaskScheduleInfo(Guid id, string tenantId, NavTaskSchedulerTaskType taskType, int state = 1)
	{
		Id = id;
		TenantId = tenantId;
		State = state;
		TaskType = taskType;
	}

	public virtual bool SetStarting()
	{
		State = 2;
		return true;
	}

	public virtual bool SetRunning()
	{
		State = 3;
		return true;
	}

	public virtual bool SetDone(bool throwOnError)
	{
		State = 4;
		return true;
	}

	public virtual bool SetReady()
	{
		State = 1;
		return true;
	}

	public virtual bool SetReadyWithRetry(bool throwOnError)
	{
		return true;
	}

	public bool ExceedsRetryLimit()
	{
		return Retries >= 10;
	}

	public virtual string GetFullTraceText()
	{
		return FormattableString.Invariant($"TaskId={Id}, TenantId={TenantId}, State={State}, Retries={Retries}");
	}
}
