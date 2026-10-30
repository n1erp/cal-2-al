using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface ITenantConfigurationSynchronizer
{
	int ServiceInstanceId { get; }

	event EventHandler<MinimumKnownConfigurationChangedEventArgs> MinimumKnownConfigurationChanged;

	void AcknowledgeConfigurationChange(long newConfigurationVersion);

	void WaitForConfigurationSynchronization(long requiredConfigurationVersion, int timeout);
}
