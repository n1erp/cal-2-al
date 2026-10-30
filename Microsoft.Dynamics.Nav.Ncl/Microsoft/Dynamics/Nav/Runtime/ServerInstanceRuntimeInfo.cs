using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal struct ServerInstanceRuntimeInfo
{
	public int Id { get; private set; }

	public DateTime LastActive { get; private set; }

	public string ServerInstanceName { get; private set; }

	public string ServerComputerName { get; private set; }

	public bool HasCrashed { get; private set; }

	public long LastTenantConfig { get; private set; }

	public ServerInstanceRuntimeInfo(int id, DateTime lastActive, bool hasCrashed, long lastTenantConfig, string serverInstanceName, string serverComputerName)
	{
		Id = id;
		LastActive = lastActive;
		HasCrashed = hasCrashed;
		LastTenantConfig = lastTenantConfig;
		ServerInstanceName = serverInstanceName;
		ServerComputerName = serverComputerName;
	}
}
