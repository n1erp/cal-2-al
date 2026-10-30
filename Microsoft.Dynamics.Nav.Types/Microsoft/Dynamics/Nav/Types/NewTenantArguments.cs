using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NewTenantArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public NavTenantSettings Settings { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int TimeoutSeconds { get; set; }

	public NewTenantArguments()
	{
	}

	public NewTenantArguments(NavTenantSettings settings, int timeoutSeconds = 0)
	{
		if (settings == null)
		{
			throw new ArgumentNullException("settings");
		}
		if (string.IsNullOrEmpty(settings.Id))
		{
			throw new ArgumentException("A Tenant ID is required", "settings");
		}
		Settings = settings;
		TimeoutSeconds = timeoutSeconds;
	}
}
