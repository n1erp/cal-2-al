using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types.Apps;

public class NavAppInfoResponse : ServerResponse
{
	[DataMember]
	public Guid AppId { get; set; }

	[DataMember]
	public string Name { get; set; }

	[DataMember]
	public string Publisher { get; set; }

	[DataMember]
	public Version Version { get; set; }

	[DataMember]
	public NavAppExtensionType ExtensionType { get; set; }

	[DataMember]
	public string Brief { get; set; }

	[DataMember]
	public string Description { get; set; }

	[DataMember]
	public Version CompatibilityId { get; set; }

	[DataMember]
	public string PrivacyStatement { get; set; }

	[DataMember]
	public string Eula { get; set; }

	[DataMember]
	public string Help { get; set; }

	[DataMember]
	public string Url { get; set; }

	[DataMember]
	public string Logo { get; set; }

	[DataMember]
	public IEnumerable<string> Screenshots { get; set; }

	[DataMember]
	public IEnumerable<NavAppObjectPrerequisiteResponse> Prerequisites { get; set; }

	[DataMember]
	public IEnumerable<NavAppDependencyResponse> Dependencies { get; set; }

	[DataMember]
	public IEnumerable<NavAppCapabilityResponse> Capabilities { get; set; }

	[DataMember]
	public Version ExtensionDataVersion { get; set; }

	[DataMember]
	public bool IsInstalled { get; set; }

	[DataMember]
	public NavAppSyncState SyncState { get; set; }

	[DataMember]
	public bool NeedsUpgrade { get; set; }

	[DataMember]
	public NavAppScope Scope { get; set; }

	[DataMember]
	public bool IsPublished { get; set; }

	[DataMember]
	public string DeveloperTenantId { get; set; }

	[DataMember]
	public Guid PackageId { get; set; }
}
