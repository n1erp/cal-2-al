using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class ResumeTenantDatabaseUpgradeArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int CodeunitId { get; set; }

	[DataMember]
	[TelemetryOii]
	public string CompanyName { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public string MethodName { get; set; }

	public ResumeTenantDatabaseUpgradeArguments()
	{
	}

	public ResumeTenantDatabaseUpgradeArguments(string tenantId, int codeunitId, string methodName, string companyName)
	{
		if (string.IsNullOrEmpty(tenantId))
		{
			tenantId = "default";
		}
		TenantId = tenantId;
		CodeunitId = codeunitId;
		MethodName = methodName;
		CompanyName = companyName;
	}
}
