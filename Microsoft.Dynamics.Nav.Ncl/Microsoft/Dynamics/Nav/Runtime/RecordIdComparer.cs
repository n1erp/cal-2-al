using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class RecordIdComparer : IComparer<NavRecordId>, IEqualityComparer<NavRecordId>
{
	private readonly NCLMetaKey primaryKey;

	public RecordIdComparer(NCLMetaTable metaTable)
	{
		primaryKey = metaTable.PrimaryKey;
	}

	public int Compare(NavRecordId x, NavRecordId y)
	{
		for (int i = 0; i < primaryKey.KeyFieldCount; i++)
		{
			INavFieldMetadata field = primaryKey.DefinedKeyFields[i];
			int num = primaryKey.GetKeyFieldByIndex(i).ExpressionContext.Compare(x.FieldValueByIndex(field, i), y.FieldValueByIndex(field, i));
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	public bool Equals(NavRecordId x, NavRecordId y)
	{
		return Compare(x, y) == 0;
	}

	public int GetHashCode(NavRecordId obj)
	{
		return obj.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is RecordIdComparer recordIdComparer)
		{
			return primaryKey == recordIdComparer.primaryKey;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return primaryKey.Parent.TableId;
	}
}
