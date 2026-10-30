using System;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class StandaloneTenantConfigurationSynchronizer : ITenantConfigurationSynchronizer
{
	public static ITenantConfigurationSynchronizer Instance { get; } = new StandaloneTenantConfigurationSynchronizer();

	public int ServiceInstanceId => NavEnvironment.Instance.GetServiceInstanceId();

	public event EventHandler<MinimumKnownConfigurationChangedEventArgs> MinimumKnownConfigurationChanged;

	public void AcknowledgeConfigurationChange(long newConfigurationVersion)
	{
	}

	public void WaitForConfigurationSynchronization(long requiredConfigurationVersion, int timeout)
	{
		MinimumKnownConfigurationChanged?.Invoke(this, new MinimumKnownConfigurationChangedEventArgs(requiredConfigurationVersion));
	}
}
