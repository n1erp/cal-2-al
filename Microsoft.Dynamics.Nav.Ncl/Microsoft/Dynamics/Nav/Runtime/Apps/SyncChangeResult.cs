using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Apps.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncChangeResult
{
	public AppId AppId { get; }

	public int TargetTableId { get; }

	public string TargetTableName { get; }

	public int OriginalObjectId { get; }

	public string OriginalObjectName { get; }

	public bool IsTableExtension { get; }

	public NavSqlTableAndChange TableAndChange { get; }

	public MetaTable UpdatedSnapshot { get; }

	public IEnumerable<string> ValidationErrors { get; }

	public SyncChangeResult(AppId appId, int targetTableId, string targetTableName, int originalObjectId, string originalObjectName, bool isTableExtension, NavSqlTableAndChange tableAndChange, MetaTable updatedSnapshot, IEnumerable<string> validationErrors)
	{
		AppId = appId;
		TargetTableId = targetTableId;
		TargetTableName = targetTableName;
		OriginalObjectId = originalObjectId;
		OriginalObjectName = originalObjectName;
		IsTableExtension = isTableExtension;
		TableAndChange = tableAndChange;
		UpdatedSnapshot = updatedSnapshot;
		ValidationErrors = validationErrors.ToList();
	}
}
