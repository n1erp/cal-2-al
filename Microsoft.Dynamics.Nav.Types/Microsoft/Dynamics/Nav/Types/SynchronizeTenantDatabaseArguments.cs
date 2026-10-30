using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SynchronizeTenantDatabaseArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string TenantId { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public SyncMode SyncMode { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool Async { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool PerTable { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public bool AwaitSyncLock { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int SyncLockWaitTimeInMinutes { get; set; }

	public SynchronizeTenantDatabaseArguments()
	{
	}

	public SynchronizeTenantDatabaseArguments(string tenantId, SyncMode syncMode = SyncMode.Sync, bool async = false, bool perTable = false, bool awaitSyncLock = true, int syncLockWaitTimeInMinutes = 60)
	{
		TenantId = tenantId;
		SyncMode = syncMode;
		Async = async;
		PerTable = perTable;
		AwaitSyncLock = awaitSyncLock;
		SyncLockWaitTimeInMinutes = syncLockWaitTimeInMinutes;
	}
}
