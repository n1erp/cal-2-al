using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncTableBundle
{
	public ObjectType ObjectType { get; }

	public int ObjectId { get; }

	public string ObjectName { get; }

	public int TargetTableId { get; }

	public MetaTable OriginalSnapshot { get; }

	public NCLMetaTable OriginalSnapshotNCL { get; }

	public MetaTable Target { get; }

	public NCLMetaTable TargetNCL { get; }

	public SyncTableBundle(ObjectType objectType, int objectId, string objectName, MetaTable snapshot, MetaTable target)
	{
		ObjectType = objectType;
		ObjectName = objectName;
		ObjectId = objectId;
		TargetTableId = target?.Id ?? snapshot.Id;
		OriginalSnapshot = snapshot;
		Target = target;
		OriginalSnapshotNCL = ((snapshot != null) ? NCLMetaTable.CreateEmptyNCLMetaTable(null, snapshot.Id, NavAppGroup.BaseGroup) : null);
		OriginalSnapshotNCL?.AssignFromMetaTable(snapshot, databaseConvertedFromNav2013: false);
		TargetNCL = ((target != null) ? NCLMetaTable.CreateEmptyNCLMetaTable(null, target.Id, NavAppGroup.BaseGroup) : null);
		TargetNCL?.AssignFromMetaTable(target, databaseConvertedFromNav2013: false);
	}

	public IEnumerable<string> Validate(IEnumerable<ISyncValidator> validators)
	{
		List<string> list = new List<string>();
		foreach (ISyncValidator validator in validators)
		{
			IReadOnlyList<string> readOnlyList = validator.Validate(ObjectType, ObjectId, this);
			foreach (string item2 in readOnlyList)
			{
				string item = string.Format(CultureInfo.InvariantCulture, "{0} {1} :: {2}", ObjectType, ObjectName, item2);
				list.Add(item);
			}
		}
		return list;
	}
}
