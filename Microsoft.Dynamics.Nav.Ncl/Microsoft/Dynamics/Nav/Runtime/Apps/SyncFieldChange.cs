namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncFieldChange
{
	public SyncFieldChangeType Change { get; }

	public NCLMetaField Snapshot { get; }

	public NCLMetaField Target { get; }

	public SyncFieldChange(SyncFieldChangeType change, NCLMetaField snapshot, NCLMetaField target)
	{
		Change = change;
		Snapshot = snapshot;
		Target = target;
	}
}
