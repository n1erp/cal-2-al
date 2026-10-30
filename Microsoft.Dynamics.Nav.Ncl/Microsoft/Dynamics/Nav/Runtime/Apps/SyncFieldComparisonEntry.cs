using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncFieldComparisonEntry
{
	public bool HasOld => OldNcl != null;

	public MetaField OldMeta { get; }

	public NCLMetaField OldNcl { get; }

	public bool HasNew => NewNcl != null;

	public MetaField NewMeta { get; }

	public NCLMetaField NewNcl { get; }

	public string BestFieldName { get; }

	public SyncFieldComparisonEntry(MetaField oldMetaField, NCLMetaField oldNclField, MetaField newMetaField, NCLMetaField newNclField)
	{
		OldMeta = oldMetaField;
		OldNcl = oldNclField;
		NewMeta = newMetaField;
		NewNcl = newNclField;
		BestFieldName = NewNcl?.FieldName ?? OldNcl?.FieldName ?? "UNKNOWN";
	}
}
