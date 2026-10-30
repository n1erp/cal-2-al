namespace Microsoft.Dynamics.Nav.Runtime;

internal interface ISystemTaskFactory
{
	ISystemTask GetSystemTask(SystemTaskActionId systemTaskActionId);
}
