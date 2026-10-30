using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class AppSyncSnapshotRecord
{
	public AppId AppId { get; set; }

	public int TargetTableId { get; set; }

	public bool IsTableExtension { get; set; }

	public MetaTable TableSnapshot { get; set; }

	public string TargetTableName { get; set; }

	public int OriginalObjectId { get; set; }

	public string OriginalObjectName { get; set; }
}
