namespace Microsoft.Dynamics.Nav.Runtime;

public class ServiceIdentity
{
	internal string ServerComputerName { get; }

	internal string ServiceName { get; }

	internal string ServerInstanceName { get; }

	public int ServiceInstanceId { get; }

	public ServiceIdentity(string serverComputerName, string serviceName, string serverInstanceName, int serviceInstanceId)
	{
		ServerComputerName = serverComputerName;
		ServiceName = serviceName;
		ServerInstanceName = serverInstanceName;
		ServiceInstanceId = serviceInstanceId;
	}
}
