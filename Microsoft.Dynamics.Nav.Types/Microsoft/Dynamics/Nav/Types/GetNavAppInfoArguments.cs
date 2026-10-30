using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class GetNavAppInfoArguments : IExtensibleDataObject
{
	[DataMember]
	[TelemetrySystemMetadata]
	public string AppId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Name { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Publisher { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public Version Version { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public Version CompatibilityId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SymbolsOnly { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool TenantSpecificProperties { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public GetNavAppInfoArguments()
	{
	}

	public GetNavAppInfoArguments(string appId, string name, string publisher, Version version, Version compatibilityId, bool symbolsOnly = false, string tenantId = null, bool tenantSpecificProperties = false)
	{
		AppId = appId;
		Name = name;
		Publisher = publisher;
		Version = version;
		CompatibilityId = compatibilityId;
		SymbolsOnly = symbolsOnly;
		TenantId = tenantId;
		TenantSpecificProperties = tenantSpecificProperties;
	}
}
