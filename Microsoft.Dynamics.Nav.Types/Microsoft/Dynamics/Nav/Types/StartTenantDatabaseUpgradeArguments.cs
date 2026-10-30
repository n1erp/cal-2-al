using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class StartTenantDatabaseUpgradeArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int Lcid { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ParallelExecutionEnabled { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ContinueOnErrorEnabled { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SingleTransaction { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SkipCompanyInitialization { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SkipAppVersionCheck { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SkipUserSessionCheck { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool ExcludeExtensions { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool SkipIfAlreadyUpgraded { get; set; }

	public StartTenantDatabaseUpgradeArguments()
	{
	}

	public StartTenantDatabaseUpgradeArguments(string tenantId, int lcid = 0, bool parallelExecutionEnabled = true, bool continueOnErrorEnabled = false, bool skipCompanyInitialization = false, bool singleTransaction = false, bool skipAppVersionCheck = false, bool excludeExtensions = false, bool skipIfAlreadyUpgraded = false, bool skipUserSessionCheck = false)
	{
		TenantId = tenantId;
		Lcid = lcid;
		ParallelExecutionEnabled = parallelExecutionEnabled;
		ContinueOnErrorEnabled = continueOnErrorEnabled;
		SingleTransaction = singleTransaction;
		SkipCompanyInitialization = skipCompanyInitialization;
		SkipAppVersionCheck = skipAppVersionCheck;
		ExcludeExtensions = excludeExtensions;
		SkipIfAlreadyUpgraded = skipIfAlreadyUpgraded;
		SkipUserSessionCheck = skipUserSessionCheck;
	}
}
