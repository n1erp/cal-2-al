using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FieldDictionary<T> : KeyValueSortedDictionary<INavFieldMetadata, T>, IFieldList
{
	private string fieldNames;

	private ulong fieldListCompatibleUInt64Hash = 9223372036854775808uL;

	public static readonly FieldDictionary<T> Empty = new FieldDictionary<T>();

	internal ulong FieldListCompatibleUInt64Hash
	{
		get
		{
			if (fieldListCompatibleUInt64Hash == 9223372036854775808uL)
			{
				ulong num = 0uL;
				for (int i = 0; i < Items.Length; i++)
				{
					INavFieldMetadata item = Items[i].Item1;
					int columnIndex = item.ColumnIndex;
					if (columnIndex >= 0)
					{
						num = ((columnIndex > 61) ? (num | 0x4000000000000000L) : (num | (ulong)(1L << columnIndex)));
					}
				}
				fieldListCompatibleUInt64Hash = num;
			}
			return fieldListCompatibleUInt64Hash;
		}
	}

	public string FieldNames => fieldNames ?? (fieldNames = string.Join(",", Items.Select((Tuple<INavFieldMetadata, T> itemInfo) => itemInfo.Item1.DescriptiveName)));

	ulong IFieldList.UInt64Hash => FieldListCompatibleUInt64Hash;

	public FieldDictionary(params Tuple<INavFieldMetadata, T>[] items)
		: base(items)
	{
	}

	public FieldDictionary(IEnumerable<Tuple<INavFieldMetadata, T>> items)
		: base(items)
	{
	}

	public FieldDictionary(Dictionary<INavFieldMetadata, T> lookup, bool copy = false)
		: base(lookup, copy)
	{
	}

	public bool AreAllFieldsContainedInListAssumingSameParent(FieldList list)
	{
		ulong num = FieldListCompatibleUInt64Hash;
		ulong uInt64Hash = list.UInt64Hash;
		if ((num & uInt64Hash) != num)
		{
			return false;
		}
		if ((num & 0x4000000000000000L) == 0L)
		{
			return true;
		}
		Tuple<INavFieldMetadata, T>[] array = Items;
		foreach (Tuple<INavFieldMetadata, T> tuple in array)
		{
			if (tuple.Item1.ColumnIndex <= 61)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < list.Count; j++)
			{
				if (tuple.Item1.ColumnIndex == list[j].ColumnIndex)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FieldDictionary<T>);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override int Compare(Tuple<INavFieldMetadata, T> x, Tuple<INavFieldMetadata, T> y)
	{
		return x.Item1.ColumnIndex - y.Item1.ColumnIndex;
	}

	public bool ContainsFieldWithColumnIndex(int columnIndex)
	{
		if (columnIndex <= 61)
		{
			return ((ulong)(1L << columnIndex) & FieldListCompatibleUInt64Hash) != 0;
		}
		return ((IFieldList)this).HasFieldWithColumnIndex(columnIndex);
	}

	bool IFieldList.HasFieldWithColumnIndex(int columnIndex)
	{
		int num = 0;
		int num2 = Items.Length - 1;
		while (num <= num2)
		{
			int num3 = num + (num2 - num >> 1);
			int num4 = Items[num3].Item1.ColumnIndex - columnIndex;
			if (num4 == 0)
			{
				return true;
			}
			if (num4 > 0)
			{
				num2 = num3 - 1;
			}
			else
			{
				num = num3 + 1;
			}
		}
		return false;
	}

	public static bool operator ==(FieldDictionary<T> left, FieldDictionary<T> right)
	{
		if ((object)left != right)
		{
			return left?.Equals(right) ?? false;
		}
		return true;
	}

	public static bool operator !=(FieldDictionary<T> left, FieldDictionary<T> right)
	{
		if ((object)left != right)
		{
			if ((object)left != null)
			{
				return !left.Equals(right);
			}
			return true;
		}
		return false;
	}
}
