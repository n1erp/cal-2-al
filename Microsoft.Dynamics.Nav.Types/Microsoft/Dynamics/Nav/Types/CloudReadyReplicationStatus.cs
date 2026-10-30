using System;

namespace Microsoft.Dynamics.Nav.Types;

[Serializable]
public class CloudReadyReplicationStatus
{
	public string TableName { get; set; } = string.Empty;

	public long SyncedVersion { get; set; }

	public int CompanyId { get; set; } = -1;

	public string CompanyName { get; set; } = string.Empty;

	public bool Blocked { get; set; }

	public int TableId { get; set; }
}
