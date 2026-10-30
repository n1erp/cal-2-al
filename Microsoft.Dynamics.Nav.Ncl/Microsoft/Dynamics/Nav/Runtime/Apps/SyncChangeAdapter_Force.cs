using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncChangeAdapter_Force
{
	public static SyncTableChanges CalculateChanges(SyncTableBundle bundle)
	{
		List<SyncFieldChange> fieldChanges = new List<SyncFieldChange>();
		List<SyncKeyChange> keyChanges = new List<SyncKeyChange>();
		if (bundle.OriginalSnapshot == null)
		{
			return new SyncTableChanges(SyncTableChangeType.Add, fieldChanges, keyChanges);
		}
		if (bundle.Target == null)
		{
			return new SyncTableChanges(SyncTableChangeType.Remove, fieldChanges, keyChanges);
		}
		SyncFieldComparisonEntryFactory.CreateComparisonEntries(bundle, allowIdFallback: true).ForEach((SyncFieldComparisonEntry c) =>
		{
			ProcessField(c, fieldChanges);
		});
		SyncKeyComparisonEntryFactory.CreateComparisonEntries(bundle).ForEach((SyncKeyComparisonEntry c) =>
		{
			ProcessSqlIndex(c, keyChanges, fieldChanges);
			ProcessSiftIndex(c, keyChanges, fieldChanges);
		});
		SyncTableChangeType changeType = ProcessTable(bundle);
		return new SyncTableChanges(changeType, fieldChanges, keyChanges);
	}

	private static SyncTableChangeType ProcessTable(SyncTableBundle bundle)
	{
		if (bundle.ObjectType == ObjectType.TableExtension && !bundle.TargetNCL.NormalFields.Any((NCLMetaField f) => !f.FieldIsTimestamp && !f.FieldIsPartOfPrimaryKey))
		{
			return SyncTableChangeType.Remove;
		}
		if (bundle.OriginalSnapshotNCL.CompressionType != bundle.TargetNCL.CompressionType && bundle.TargetNCL.CompressionType != CompressionType.Unspecified)
		{
			return SyncTableChangeType.Change;
		}
		return SyncTableChangeType.None;
	}

	private static void ProcessField(SyncFieldComparisonEntry entry, List<SyncFieldChange> fieldChanges)
	{
		if (entry.HasOld && !entry.HasNew)
		{
			fieldChanges.Add(FieldChange(entry, SyncFieldChangeType.Remove));
			return;
		}
		if (!entry.HasOld && entry.HasNew)
		{
			fieldChanges.Add(FieldChange(entry, SyncFieldChangeType.Add));
			return;
		}
		List<SyncFieldChange> list = new List<SyncFieldChange>();
		if (!string.Equals(entry.NewNcl.FieldName, entry.OldNcl.FieldName, StringComparison.Ordinal))
		{
			list.Add(FieldChange(entry, SyncFieldChangeType.Name));
		}
		if (entry.NewNcl.FieldNclType != entry.OldNcl.FieldNclType)
		{
			list.Add(FieldChange(entry, SyncFieldChangeType.DataType));
		}
		else
		{
			if (entry.OldNcl.FieldNclType == NavNclType.NavCode && entry.OldNcl.SqlDataTypeWithDefaultMapping != entry.NewNcl.SqlDataTypeWithDefaultMapping)
			{
				list.Add(FieldChange(entry, SyncFieldChangeType.SqlDataType));
			}
			if (entry.OldNcl.IsStringType && entry.NewNcl.IsStringType && entry.NewNcl.SqlDataTypeWithDefaultMapping != SqlDataType.Variant && entry.OldNcl.FieldDefinedLength != entry.NewNcl.FieldDefinedLength)
			{
				list.Add(FieldChange(entry, SyncFieldChangeType.Length));
			}
			if (entry.OldNcl.FieldIsAutoIncrement != entry.NewNcl.FieldIsAutoIncrement)
			{
				list.Add(FieldChange(entry, SyncFieldChangeType.AutoIncrement));
			}
			if (entry.OldNcl.FieldNclType == NavNclType.NavBlob && entry.OldNcl.FieldIsCompressed != entry.NewNcl.FieldIsCompressed)
			{
				list.Add(FieldChange(entry, SyncFieldChangeType.Compression));
			}
		}
		if (!list.Any())
		{
			list.Add(FieldChange(entry, SyncFieldChangeType.NoChange));
		}
		fieldChanges.AddRange(list);
	}

	private static void ProcessSqlIndex(SyncKeyComparisonEntry entry, List<SyncKeyChange> keyChanges, IReadOnlyList<SyncFieldChange> fieldChanges)
	{
		if (entry.HasNew && entry.NewNcl.MaintainSqlIndex)
		{
			if (!entry.HasOld || !entry.OldNcl.MaintainSqlIndex)
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.AddSqlIndex));
				return;
			}
			if (entry.NewNcl.IsPrimaryKey && entry.OldNcl.IsPrimaryKey)
			{
				bool flag = false;
				foreach (NCLMetaField newField in entry.NewNcl.KeyFieldsList)
				{
					NCLMetaField nCLMetaField = entry.OldNcl.KeyFieldsList.FirstOrDefault((NCLMetaField o) => o.FieldName == newField.FieldName);
					flag |= nCLMetaField != null && newField.FieldDefinedLength > nCLMetaField.FieldDefinedLength;
				}
				if (flag)
				{
					keyChanges.Add(KeyChange(entry, SyncKeyChangeType.IndexFields));
				}
			}
			if (HaveFieldListsChangedInAnyWay(entry.OldNcl, entry.NewNcl, fieldChanges, (NCLMetaKey k) => k.KeyFieldsList))
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.IndexFields));
			}
			if (HaveFieldListsChangedInAnyWay(entry.OldNcl, entry.NewNcl, fieldChanges, (NCLMetaKey k) => k.SqlIndexFields))
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.IndexFields));
			}
			if (entry.OldNcl.Clustered != entry.NewNcl.Clustered)
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.Clustered));
			}
			if (entry.OldNcl.IsPrimaryKey != entry.NewNcl.IsPrimaryKey)
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.PrimaryKey));
			}
			if (entry.OldNcl.Unique != entry.NewNcl.Unique)
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.Unique));
			}
		}
		else if (entry.HasOld && entry.OldNcl.MaintainSqlIndex)
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.RemoveSqlIndex));
		}
	}

	private static void ProcessSiftIndex(SyncKeyComparisonEntry entry, List<SyncKeyChange> keyChanges, IReadOnlyList<SyncFieldChange> fieldChanges)
	{
		if (entry.HasNew && entry.NewNcl.MaintainSumIndex)
		{
			if (!entry.HasOld || !entry.OldNcl.MaintainSumIndex)
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.AddSiftIndex));
				return;
			}
			if (HaveFieldListsChangedInAnyWay(entry.OldNcl, entry.NewNcl, fieldChanges, (NCLMetaKey k) => k.SumFieldsList?.AsNclMetaFields()))
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.SumFields));
			}
			if (HaveFieldListsChangedInAnyWay(entry.OldNcl, entry.NewNcl, fieldChanges, (NCLMetaKey k) => k.KeyFieldsList))
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.SumFields));
			}
			if (!string.Equals(entry.OldNcl.Parent.TableName, entry.NewNcl.Parent.TableName, StringComparison.Ordinal))
			{
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.RemoveSiftIndex));
				keyChanges.Add(KeyChange(entry, SyncKeyChangeType.AddSiftIndex));
			}
		}
		else if (entry.HasOld && entry.OldNcl.MaintainSumIndex)
		{
			keyChanges.Add(KeyChange(entry, SyncKeyChangeType.RemoveSiftIndex));
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

	private static bool HaveFieldListsChangedInAnyWay(NCLMetaKey key1, NCLMetaKey key2, IReadOnlyList<SyncFieldChange> fieldChanges, Func<NCLMetaKey, IEnumerable<NCLMetaField>> selector)
	{
		List<NCLMetaField> list = selector(key1)?.ToList() ?? new List<NCLMetaField>();
		List<NCLMetaField> list2 = selector(key2)?.ToList() ?? new List<NCLMetaField>();
		List<SyncFieldChange> source = fieldChanges.Where((SyncFieldChange fc) => fc.Target != null && fc.Change != SyncFieldChangeType.NoChange).ToList();
		if (list.Count != list2.Count)
		{
			return true;
		}
		bool flag = false;
		foreach (NCLMetaField f1 in list)
		{
			NCLMetaField nCLMetaField = list2.FirstOrDefault((NCLMetaField nCLMetaField2) => nCLMetaField2.FieldName == f1.FieldName);
			flag |= nCLMetaField == null || source.Any((SyncFieldChange fc) => fc.Target.FieldName == f1.FieldName);
		}
		return flag;
	}
}
