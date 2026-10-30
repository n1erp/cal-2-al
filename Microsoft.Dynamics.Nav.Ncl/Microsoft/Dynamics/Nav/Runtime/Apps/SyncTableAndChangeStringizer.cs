using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncTableAndChangeStringizer
{
	public static string Stringize(NavSqlTableAndChange c)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "T | Id:{0} --> {1}; Name:{2} --> {3}; Chg:{4}; PerCompChg:{5}; Rename:{6}; Rebuild:{7}", c.OldTable?.TableId, c.NewTable?.TableId, c.OldTable?.TableName, c.NewTable?.TableName, c.ChangeType, c.DataPerCompanyChanged, c.TableRenamed, c.RebuildTable);
		foreach (NavSqlFieldAndChanges item in c.FieldsAndChanges.Where((NavSqlFieldAndChanges f) => !f.NoChange))
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}  F | Id:{1} --> {2}; Name:{3} --> {4}; Chg:{5}; Type:{6} --> {7}; Len:{8} --> {9}", Environment.NewLine, item.OldField?.FieldNo, item.NewField?.FieldNo, item.OldField?.FieldName, item.NewField?.FieldName, item.Changes, item.OldField?.FieldNclType, item.NewField?.FieldNclType, item.OldField?.FieldDefinedLength, item.NewField?.FieldDefinedLength);
		}
		foreach (NavSqlIndexAndChanges indexesAndChange in c.IndexesAndChanges)
		{
			stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}  I | Name:{1}; Chg:{2}; Fields:{3} --> {4}; Sum:{5} --> {6}", Environment.NewLine, indexesAndChange.NewKey?.KeyName ?? indexesAndChange.OldSqlIndex?.KeyName ?? indexesAndChange.OldSiftIndex?.KeyName, indexesAndChange.Changes, string.Join(",", indexesAndChange.OldSqlIndex?.KeyFieldsList?.Select((NCLMetaField f) => f.FieldName) ?? indexesAndChange.OldSiftIndex?.SumFieldsList?.AsNclMetaFields()?.Select((NCLMetaField f) => f.FieldName) ?? new List<string>()), string.Join(",", indexesAndChange.NewKey?.KeyFieldsList?.Select((NCLMetaField f) => f.FieldName) ?? indexesAndChange.NewKey?.KeyFieldsList?.Select((NCLMetaField f) => f.FieldName) ?? new List<string>()), string.Join(",", indexesAndChange.OldSiftIndex?.SumFieldsList?.AsNclMetaFields()?.Select((NCLMetaField f) => f.FieldName) ?? new List<string>()), string.Join(",", indexesAndChange.NewKey?.SumFieldsList?.AsNclMetaFields()?.Select((NCLMetaField f) => f.FieldName) ?? new List<string>()));
		}
		return stringBuilder.ToString();
	}
}
