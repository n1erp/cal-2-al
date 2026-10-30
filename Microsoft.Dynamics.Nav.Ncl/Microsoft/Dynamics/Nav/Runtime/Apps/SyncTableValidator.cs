using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncTableValidator : ISyncValidator
{
	public IReadOnlyList<string> Validate(ObjectType objectType, int objectId, SyncTableBundle bundle)
	{
		List<string> list = new List<string>();
		NCLMetaTable originalSnapshotNCL = bundle.OriginalSnapshotNCL;
		NCLMetaTable targetNCL = bundle.TargetNCL;
		if (originalSnapshotNCL == null)
		{
			return list;
		}
		string tableName = originalSnapshotNCL.TableName;
		if (targetNCL == null)
		{
			string item = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_UnsupportedTableChange, tableName, "Remove");
			list.Add(item);
			return list;
		}
		List<string> list2 = new List<string>();
		if (!originalSnapshotNCL.TableName.Equals(targetNCL.TableName))
		{
			list2.Add("Name");
		}
		if (originalSnapshotNCL.DataPerCompany != targetNCL.DataPerCompany)
		{
			list2.Add("DataPerCompany");
		}
		if (list2.Any())
		{
			string arg = string.Join(", ", list2);
			string item2 = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_UnsupportedTableChange, tableName, arg);
			list.Add(item2);
		}
		return list;
	}
}
