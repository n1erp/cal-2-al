using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSystemScheduledTask
{
	internal Guid TaskId { get; set; }

	internal SystemTaskActionId SystemTaskActionId { get; set; }

	internal string Parameters { get; set; }

	internal string TenantId { get; set; }

	internal DateTime NotBefore { get; set; }

	public NavSystemScheduledTask(Guid taskId, SystemTaskActionId systemTaskActionId, string parameters, string tenantId, DateTime notBefore)
	{
		TaskId = taskId;
		SystemTaskActionId = systemTaskActionId;
		Parameters = parameters;
		TenantId = tenantId;
		NotBefore = notBefore;
	}
}
