using System.Collections.Generic;
using System.Globalization;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

internal class SyncFieldValidator : ISyncValidator
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
		IEnumerable<SyncFieldComparisonEntry> enumerable = SyncFieldComparisonEntryFactory.CreateComparisonEntries(bundle, allowIdFallback: false);
		foreach (SyncFieldComparisonEntry item3 in enumerable)
		{
			if (!item3.HasNew)
			{
				string item = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_UnsupportedFieldChange, item3.OldNcl.FieldName, "Remove");
				list.Add(item);
			}
			else if (item3.HasOld && item3.HasNew)
			{
				UnsupportedAppFieldChangeTypes unsupportedAppFieldChangeTypes = DetermineUnsupportedFieldChanges(objectType, item3.OldNcl, item3.NewNcl);
				if (unsupportedAppFieldChangeTypes != UnsupportedAppFieldChangeTypes.None)
				{
					string item2 = string.Format(CultureInfo.CurrentCulture, Lang.NavAppSync_UnsupportedFieldChange, item3.NewNcl.FieldName, unsupportedAppFieldChangeTypes);
					list.Add(item2);
				}
			}
		}
		return list;
	}

	private static UnsupportedAppFieldChangeTypes DetermineUnsupportedFieldChanges(ObjectType objectType, NCLMetaField oldField, NCLMetaField newField)
	{
		UnsupportedAppFieldChangeTypes unsupportedAppFieldChangeTypes = UnsupportedAppFieldChangeTypes.None;
		if (oldField.FieldNclType != newField.FieldNclType)
		{
			unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.DataType;
		}
		else
		{
			if (oldField.FieldNclType == NavNclType.NavCode && oldField.SqlDataTypeWithDefaultMapping != newField.SqlDataTypeWithDefaultMapping)
			{
				unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.SqlDataType;
			}
			if (oldField.IsStringType && newField.IsStringType && newField.SqlDataTypeWithDefaultMapping != SqlDataType.Variant && newField.FieldDefinedLength < oldField.FieldDefinedLength)
			{
				unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.LengthReduced;
			}
			if (oldField.FieldIsAutoIncrement != newField.FieldIsAutoIncrement)
			{
				unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.AutoIncrement;
			}
			if (oldField.FieldNclType == NavNclType.NavBlob && oldField.FieldIsCompressed != newField.FieldIsCompressed)
			{
				unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.Compression;
			}
		}
		if (string.CompareOrdinal(oldField.FieldName, newField.FieldName) != 0)
		{
			unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.Name;
		}
		if ((oldField.FieldIsPartOfPrimaryKey || newField.FieldIsPartOfPrimaryKey) && objectType == ObjectType.Table && oldField.FieldDefinedLength != newField.FieldDefinedLength)
		{
			unsupportedAppFieldChangeTypes |= UnsupportedAppFieldChangeTypes.LengthChanged;
		}
		return unsupportedAppFieldChangeTypes;
	}
}
