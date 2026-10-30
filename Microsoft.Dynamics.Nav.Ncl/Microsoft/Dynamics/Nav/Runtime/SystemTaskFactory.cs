using System;
using Microsoft.Dynamics.Nav.Runtime.TaskScheduling.SystemTasks;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SystemTaskFactory : ISystemTaskFactory
{
	public ISystemTask GetSystemTask(SystemTaskActionId systemTaskActionId)
	{
		return systemTaskActionId switch
		{
			SystemTaskActionId.None => (ISystemTask)null, 
			SystemTaskActionId.DeleteCompany => new DeleteCompanySystemTask(), 
			SystemTaskActionId.RemoveTenant => new RemoveTenantSystemTask(), 
			SystemTaskActionId.DeleteExpiredFiles => new DeleteExpiredFilesSystemTask(), 
			_ => throw new InvalidOperationException("System task does not exist"), 
		};
	}
}
