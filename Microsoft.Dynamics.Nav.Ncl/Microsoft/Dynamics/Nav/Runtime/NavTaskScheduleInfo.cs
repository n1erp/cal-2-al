using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("State={State}, NotBefore={NotBefore}, TenantId={TenantId}")]
internal class NavTaskScheduleInfo : TaskScheduleInfo
{
	private readonly int currentServiceInstanceId;

	internal byte[] Timestamp { get; }

	internal DateTime? Ended { get; }

	internal DateTime NotBefore { get; }

	internal int ServiceInstanceId { get; }

	public NavTaskScheduleInfo(Guid id, byte[] timestamp, string tenantId, int state, int serviceInstanceId, DateTime notBefore, DateTime? ended, int retries, int currentServiceInstanceId, NavTaskSchedulerTaskType taskType)
		: base(id, tenantId, taskType, state)
	{
		Timestamp = timestamp;
		ServiceInstanceId = serviceInstanceId;
		Ended = ended;
		NotBefore = notBefore;
		Retries = retries;
		this.currentServiceInstanceId = currentServiceInstanceId;
	}

	public override bool SetStarting()
	{
		if (State == 2)
		{
			return ServiceInstanceId == currentServiceInstanceId;
		}
		if (NavTaskScheduler.SqlDml.SetTaskState(Id, TenantId, 2, State, currentServiceInstanceId, throwOnError: false))
		{
			State = 2;
			return true;
		}
		return false;
	}

	public override bool SetRunning()
	{
		if (NavTaskScheduler.SqlDml.SetTaskState(Id, TenantId, 3, State, currentServiceInstanceId, throwOnError: false))
		{
			State = 3;
			return true;
		}
		return false;
	}

	public override bool SetDone(bool throwOnError)
	{
		if (NavTaskScheduler.SqlDml.SetTaskState(Id, TenantId, 4, State, currentServiceInstanceId, throwOnError))
		{
			State = 4;
			return true;
		}
		return false;
	}

	public override bool SetReady()
	{
		if (NavTaskScheduler.SqlDml.SetTaskReady(Id, TenantId))
		{
			State = 1;
			return true;
		}
		return false;
	}

	public override bool SetReadyWithRetry(bool throwOnError)
	{
		return NavTaskScheduler.SqlDml.SetTaskRetry(Id, TenantId, GetRetryTime(), throwOnError);
	}

	private DateTime GetRetryTime()
	{
		switch (Retries)
		{
		case 0:
		case 1:
			return DateTime.UtcNow.AddMinutes(2.0);
		case 2:
			return DateTime.UtcNow.AddMinutes(4.0);
		default:
			return DateTime.UtcNow.AddMinutes(15.0);
		}
	}

	public override string GetFullTraceText()
	{
		return string.Format(CultureInfo.InvariantCulture, "TaskId={0}, TenantId={1}, State={2}, ServiceInstanceId={3}, NotBefore={4}, Retries={5}", Id, TenantId, State, ServiceInstanceId, NotBefore, Retries);
	}
}
