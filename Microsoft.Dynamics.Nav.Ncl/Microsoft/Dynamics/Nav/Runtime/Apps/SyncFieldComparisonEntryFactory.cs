using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal static class SyncFieldComparisonEntryFactory
{
	public static IEnumerable<SyncFieldComparisonEntry> CreateComparisonEntries(SyncTableBundle bundle, bool allowIdFallback)
	{
		List<SyncFieldComparisonEntry> list = new List<SyncFieldComparisonEntry>();
		Dictionary<string, NCLMetaField> dictionary = bundle.TargetNCL?.NormalFields?.Where(ShouldConsiderField).ToDictionary((NCLMetaField f) => f.FieldName) ?? new Dictionary<string, NCLMetaField>();
		Dictionary<int, NCLMetaField> dictionary2 = bundle.TargetNCL?.NormalFields?.Where(ShouldConsiderField).ToDictionary((NCLMetaField f) => f.FieldNo) ?? new Dictionary<int, NCLMetaField>();
		Dictionary<string, NCLMetaField> dictionary3 = bundle.OriginalSnapshotNCL?.NormalFields?.Where(ShouldConsiderField).ToDictionary((NCLMetaField f) => f.FieldName) ?? new Dictionary<string, NCLMetaField>();
		Dictionary<int, NCLMetaField> dictionary4 = bundle.OriginalSnapshotNCL?.NormalFields?.Where(ShouldConsiderField).ToDictionary((NCLMetaField f) => f.FieldNo) ?? new Dictionary<int, NCLMetaField>();
		List<int> list2 = dictionary4.Keys.ToList();
		foreach (int item in list2)
		{
			MetaField fieldsById = bundle.OriginalSnapshot.GetFieldsById(item);
			NCLMetaField nCLMetaField = dictionary4[item];
			NCLMetaField nCLMetaField2 = FindFieldMatch(dictionary, dictionary2, nCLMetaField, allowIdFallback);
			if (nCLMetaField2 == null)
			{
				list.Add(new SyncFieldComparisonEntry(fieldsById, nCLMetaField, null, null));
			}
			else
			{
				MetaField fieldsById2 = bundle.Target.GetFieldsById(nCLMetaField2.FieldNo);
				list.Add(new SyncFieldComparisonEntry(fieldsById, nCLMetaField, fieldsById2, nCLMetaField2));
				dictionary.Remove(nCLMetaField2.FieldName);
				dictionary2.Remove(nCLMetaField2.FieldNo);
			}
			dictionary3.Remove(nCLMetaField.FieldName);
			dictionary4.Remove(nCLMetaField.FieldNo);
		}
		foreach (NCLMetaField value in dictionary.Values)
		{
			MetaField fieldsById3 = bundle.Target.GetFieldsById(value.FieldNo);
			list.Add(new SyncFieldComparisonEntry(null, null, fieldsById3, value));
		}
		return list;
		static bool ShouldConsiderField(NCLMetaField f)
		{
			if (!f.FieldIsTimestamp)
			{
				return !f.FieldIsUserDefinedTimestamp;
			}
			return false;
		}
	}

	private static NCLMetaField FindFieldMatch(Dictionary<string, NCLMetaField> newByName, Dictionary<int, NCLMetaField> newById, NCLMetaField target, bool allowIdFallback)
	{
		NCLMetaField value = (newByName.TryGetValue(target.FieldName, out value) ? value : null);
		if ((value == null) & allowIdFallback)
		{
			value = (newById.TryGetValue(target.FieldNo, out value) ? value : null);
		}
		return value;
	}
}
