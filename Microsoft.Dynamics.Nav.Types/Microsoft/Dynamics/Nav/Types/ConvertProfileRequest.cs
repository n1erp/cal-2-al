using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConvertProfileRequest : IExtensibleDataObject
{
	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string ProfileId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool AllProfiles { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool DownloadResult { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool OverwriteExisting { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public ConvertProfileRequest()
	{
	}

	public ConvertProfileRequest(string tenantId, bool downloadResult, bool overwriteExisting, string origin)
		: this(string.Empty, tenantId, allProfiles: true, downloadResult, overwriteExisting, origin)
	{
	}

	public ConvertProfileRequest(string profileId, string tenantId, bool downloadResult, bool overwriteExisting, string origin)
		: this(profileId, tenantId, allProfiles: false, downloadResult, overwriteExisting, origin)
	{
	}

	private ConvertProfileRequest(string profileId, string tenantId, bool allProfiles, bool downloadResult, bool overwriteExisting, string origin)
	{
		ProfileId = profileId;
		TenantId = tenantId;
		DownloadResult = downloadResult;
		OverwriteExisting = overwriteExisting;
		AllProfiles = allProfiles;
		Origin = origin;
	}
}
