using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncChangeAdapter_Add
{
	public static SyncTableChanges CalculateChanges(SyncTableBundle bundle)
	{
		List<SyncFieldChange> fieldChanges = new List<SyncFieldChange>();
		List<SyncKeyChange> keyChanges = new List<SyncKeyChange>();
		if (bundle.OriginalSnapshot == null)
		{
			return new SyncTableChanges(SyncTableChangeType.Add, fieldChanges, keyChanges);
		}
		SyncFieldComparisonEntryFactory.CreateComparisonEntries(bundle, allowIdFallback: false).ForEach((SyncFieldComparisonEntry c) =>
		{
			ProcessField(c, fieldChanges);
		});
		SyncKeyComparisonEntryFactory.CreateComparisonEntries(bundle).ForEach((SyncKeyComparisonEntry c) =>
		{
			ProcessSqlIndex(bundle.ObjectType, c, keyChanges);
			ProcessSiftIndex(c, keyChanges);
		});
		SyncTableChangeType changeType = ProcessTable(bundle);
		return new SyncTableChanges(changeType, fieldChanges, keyChanges);
	}

	private static SyncTableChangeType ProcessTable(SyncTableBundle bundle)
	{
		if (bundle.OriginalSnapshotNCL.CompressionType != bundle.TargetNCL.CompressionType && bundle.TargetNCL.CompressionType != CompressionType.Unspecified)
		{
			return SyncTableChangeType.Change;
		}
		return SyncTableChangeType.None;
	}

	private static void ProcessField(SyncFieldComparisonEntry entry, List<SyncFieldChange> fieldChanges)
	{
		if (!entry.HasOld && entry.HasNew)
		{
			fieldChanges.Add(FieldChange(entry, SyncFieldChangeType.Add));
			return;
		}
		List<SyncFieldChange> list = new List<SyncFieldChange>();
		if (entry.NewNcl.FieldDefinedLength > entry.OldNcl.FieldDefinedLength)
		{
			list.Add(FieldChange(entry, SyncFieldChangeType.Length));
		}
		if (!list.Any())
		{
			list.Add(FieldChange(entry, SyncFieldChangeType.NoChange));
		}
		fieldChanges.AddRange(list);
	}

	private static void ProcessSqlIndex(ObjectType objectType, SyncKeyComparisonEntry entry, List<SyncKeyChange> keyChanges)
	{
		if (!entry.HasNew || !entry.NewNcl.MaintainSqlIndex)
		{
			return;
		}
		if (!entry.HasOld || !entry.OldNcl.MaintainSqlIndex)
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.AddSqlIndex));
			return;
		}
		bool flag = objectType == ObjectType.TableExtension && entry.NewNcl.IsPrimaryKey && entry.OldNcl.IsPrimaryKey;
		bool flag2 = !entry.NewNcl.IsPrimaryKey && !entry.OldNcl.IsPrimaryKey;
		if ((flag | flag2) && HasKeyFieldLengthIncreased(entry.OldNcl, entry.NewNcl))
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.IndexFields));
		}
		if (entry.OldNcl.Unique && !entry.NewNcl.Unique)
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.Unique));
		}
	}

	private static bool HasKeyFieldLengthIncreased(NCLMetaKey oldKey, NCLMetaKey newKey)
	{
		bool flag = false;
		List<NCLMetaField> source = oldKey.KeyFieldsList?.ToList() ?? new List<NCLMetaField>();
		List<NCLMetaField> list = newKey.KeyFieldsList?.ToList() ?? new List<NCLMetaField>();
		List<NCLMetaField> source2 = oldKey.SqlIndexFields?.ToList() ?? new List<NCLMetaField>();
		List<NCLMetaField> list2 = newKey.SqlIndexFields?.ToList() ?? new List<NCLMetaField>();
		foreach (NCLMetaField newField in list)
		{
			NCLMetaField nCLMetaField = source.FirstOrDefault((NCLMetaField o) => o.FieldName == newField.FieldName);
			flag |= nCLMetaField != null && newField.FieldDefinedLength > nCLMetaField.FieldDefinedLength;
		}
		foreach (NCLMetaField newField2 in list2)
		{
			NCLMetaField nCLMetaField2 = source2.FirstOrDefault((NCLMetaField o) => o.FieldName == newField2.FieldName);
			flag |= nCLMetaField2 != null && newField2.FieldDefinedLength > nCLMetaField2.FieldDefinedLength;
		}
		return flag;
	}

	private static void ProcessSiftIndex(SyncKeyComparisonEntry entry, List<SyncKeyChange> keyChanges)
	{
		if (entry.HasNew && entry.NewNcl.MaintainSumIndex && (!entry.HasOld || !entry.OldNcl.MaintainSumIndex))
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.AddSiftIndex));
		}
	}

	private static SyncFieldChange FieldChange(SyncFieldComparisonEntry entry, SyncFieldChangeType type)
	{
		return new SyncFieldChange(type, entry.OldNcl, entry.NewNcl);
	}

	private static SyncKeyChange KeyChange(SyncKeyComparisonEntry entry, SyncKeyChangeType type)
	{
		return new SyncKeyChange(type, entry.OldMeta, entry.OldNcl, entry.NewMeta, entry.NewNcl);
	}
}
