using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncKeyChange
{
	public SyncKeyChangeType Change { get; }

	public MetaKey Snapshot { get; }

	public NCLMetaKey SnapshotNCL { get; }

	public MetaKey Target { get; }

	public NCLMetaKey TargetNCL { get; }

	public SyncKeyChange(SyncKeyChangeType change, MetaKey snapshot, NCLMetaKey snapshotNCL, MetaKey target, NCLMetaKey targetNCL)
	{
		Change = change;
		Snapshot = snapshot;
		SnapshotNCL = snapshotNCL;
		Target = target;
		TargetNCL = targetNCL;
	}
}
