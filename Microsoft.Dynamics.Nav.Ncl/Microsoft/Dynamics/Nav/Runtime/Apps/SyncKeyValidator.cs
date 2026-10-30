using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncKeyValidator : ISyncValidator
{
	public IReadOnlyList<string> Validate(ObjectType objectType, int objectId, SyncTableBundle bundle)
	{
		List<string> list = new List<string>();
		NCLMetaTable originalSnapshotNCL = bundle.OriginalSnapshotNCL;
		NCLMetaTable targetNCL = bundle.TargetNCL;
		if (originalSnapshotNCL == null || targetNCL == null)
		{
			return list;
		}
		IEnumerable<SyncKeyComparisonEntry> enumerable = SyncKeyComparisonEntryFactory.CreateComparisonEntries(bundle);
		foreach (SyncKeyComparisonEntry item in enumerable)
		{
			if (!item.HasNew)
			{
				if (item.OldNcl.MaintainSqlIndex || item.OldNcl.MaintainSumIndex)
				{
					list.Add(CreateUnsupportedKeyChange(item, "Remove"));
				}
			}
			else if (!item.HasOld)
			{
				if (item.NewNcl.Unique)
				{
					list.Add(CreateUnsupportedKeyChange(item, "Unique"));
				}
			}
			else
			{
				CheckForUnsupportedChanges(item, list);
			}
		}
		return list;
	}

	private static void CheckForUnsupportedChanges(SyncKeyComparisonEntry c, List<string> errors)
	{
		string[] first = (from f in c.OldNcl.KeyFieldsList
			select f.FieldName into name
			orderby name
			select name).ToArray();
		string[] second = (from f in c.NewNcl.KeyFieldsList
			select f.FieldName into name
			orderby name
			select name).ToArray();
		string[] first2 = c.OldNcl.SqlIndexFields?.Select((NCLMetaField f) => f.FieldName).OrderBy((string name) => name).ToArray() ?? new string[0];
		string[] second2 = c.NewNcl.SqlIndexFields?.Select((NCLMetaField f) => f.FieldName).OrderBy((string name) => name).ToArray() ?? new string[0];
		string[] first3 = c.OldNcl.SumFieldsList?.AsNclMetaFields()?.Select((NCLMetaField f) => f.FieldName).OrderBy((string name) => name).ToArray() ?? new string[0];
		string[] second3 = c.NewNcl.SumFieldsList?.AsNclMetaFields()?.Select((NCLMetaField f) => f.FieldName).OrderBy((string name) => name).ToArray() ?? new string[0];
		if (c.OldNcl.IsPrimaryKey != c.NewNcl.IsPrimaryKey)
		{
			errors.Add(CreateUnsupportedKeyChange(c, "PrimaryKey"));
		}
		if (c.OldNcl.Clustered != c.NewNcl.Clustered)
		{
			errors.Add(CreateUnsupportedKeyChange(c, "Clustered"));
		}
		if (c.OldNcl.MaintainSqlIndex && !c.NewNcl.MaintainSqlIndex)
		{
			errors.Add(CreateUnsupportedKeyChange(c, "MaintainSqlIndex"));
		}
		if (c.OldNcl.MaintainSumIndex && !c.NewNcl.MaintainSumIndex)
		{
			errors.Add(CreateUnsupportedKeyChange(c, "MaintainSumIndex"));
		}
		if (!c.OldNcl.Unique && c.NewNcl.Unique)
		{
			errors.Add(CreateUnsupportedKeyChange(c, "Unique"));
		}
		if ((c.OldNcl.MaintainSqlIndex || c.OldNcl.MaintainSumIndex) && !first.SequenceEqual(second))
		{
			errors.Add(CreateUnsupportedKeyChange(c, "KeyFieldList"));
		}
		if (c.OldNcl.MaintainSqlIndex && !first2.SequenceEqual(second2))
		{
			errors.Add(CreateUnsupportedKeyChange(c, "SqlIndexFields"));
		}
		if (c.OldNcl.MaintainSumIndex && !first3.SequenceEqual(second3))
		{
			errors.Add(CreateUnsupportedKeyChange(c, "SumIndexFields"));
		}
	}

	private static string CreateUnsupportedKeyChange(SyncKeyComparisonEntry c, string change)
	{
		return string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_UnsupportedKeyChange, c.BestKeyName, change);
	}
}
