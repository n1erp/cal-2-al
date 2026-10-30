using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncKeyComparisonEntry
{
	public bool HasOld => OldNcl != null;

	public MetaKey OldMeta { get; }

	public NCLMetaKey OldNcl { get; }

	public bool HasNew => NewNcl != null;

	public MetaKey NewMeta { get; }

	public NCLMetaKey NewNcl { get; }

	public string BestKeyName { get; }

	public SyncKeyComparisonEntry(MetaKey oldMetaKey, NCLMetaKey oldNclKey, MetaKey newMetaKey, NCLMetaKey newNclKey)
	{
		OldMeta = oldMetaKey;
		OldNcl = oldNclKey;
		NewMeta = newMetaKey;
		NewNcl = newNclKey;
		BestKeyName = NewNcl?.KeyName ?? OldNcl?.KeyName ?? NewNcl?.GetKeyName() ?? OldNcl?.GetKeyName() ?? "UNKNOWN";
	}
}
