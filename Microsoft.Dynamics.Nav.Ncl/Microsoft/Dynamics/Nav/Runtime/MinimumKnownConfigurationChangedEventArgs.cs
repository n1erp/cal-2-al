using System;

namespace Microsoft.Dynamics.Nav.Runtime;

public class MinimumKnownConfigurationChangedEventArgs : EventArgs
{
	public long ConfigurationVersion { get; private set; }

	public MinimumKnownConfigurationChangedEventArgs(long configurationVersion)
	{
		ConfigurationVersion = configurationVersion;
	}
}
