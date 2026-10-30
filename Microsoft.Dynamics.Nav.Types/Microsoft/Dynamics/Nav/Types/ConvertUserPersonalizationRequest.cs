using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ConvertUserPersonalizationRequest : IExtensibleDataObject
{
	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetryEupi]
	public Guid UserSid { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool AllUsers { get; set; }

	public bool Force { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string Origin { get; set; }

	public ExtensionDataObject ExtensionData { get; set; }

	public ConvertUserPersonalizationRequest()
	{
	}

	public ConvertUserPersonalizationRequest(string tenantId, bool force, string origin)
		: this(Guid.Empty, tenantId, allUsers: true, force, origin)
	{
	}

	public ConvertUserPersonalizationRequest(Guid userSid, string tenantId, bool force, string origin)
		: this(userSid, tenantId, allUsers: false, force, origin)
	{
	}

	private ConvertUserPersonalizationRequest(Guid userSid, string tenantId, bool allUsers, bool force, string origin)
	{
		UserSid = userSid;
		TenantId = tenantId;
		AllUsers = allUsers;
		Force = force;
		Origin = origin;
	}
}
