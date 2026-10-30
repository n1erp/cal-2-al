using System;
using System.Runtime.Serialization;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class SynchronizeTenantsDatabaseArguments : IExtensibleDataObject
{
	public ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	[TelemetryOii]
	public string Id { get; set; }

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
	public bool SharedSchema { get; set; }

	[DataMember]
	[TelemetrySystemMetadata]
	public int SyncLockWaitTimeInMinutes { get; set; }

	public SynchronizeTenantsDatabaseArguments()
	{
	}

	public SynchronizeTenantsDatabaseArguments(string id, SyncMode syncMode = SyncMode.Sync, bool async = false, bool perTable = false, bool sharedSchema = false, int syncLockWaitTimeInMinutes = 60)
	{
		Id = id;
		SyncMode = syncMode;
		Async = async;
		PerTable = perTable;
		SharedSchema = sharedSchema;
		SyncLockWaitTimeInMinutes = syncLockWaitTimeInMinutes;
	}
}
