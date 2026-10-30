using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class FieldList : IEquatable<FieldList>, IEnumerable<INavFieldMetadata>, IEnumerable, IFieldList
{
	public static readonly FieldList Empty;

	internal const ulong UninitializedHash = 9223372036854775808uL;

	internal const ulong ContainsColumnsOutsideHashFlag = 4611686018427387904uL;

	internal const int MaxColumnIndexInHash = 61;

	private INavFieldMetadata[] fields;

	private ulong hash = 9223372036854775808uL;

	private bool? hasFlowFields;

	public INavFieldMetadata this[int index] => fields[index];

	public int Count => fields.Length;

	public bool HasFlowFields
	{
		get
		{
			if (!hasFlowFields.HasValue)
			{
				hasFlowFields = false;
				for (int i = 0; i < fields.Length; i++)
				{
					if (fields[i].FieldClass == FieldClass.FlowField)
					{
						hasFlowFields = true;
						break;
					}
				}
			}
			return hasFlowFields.Value;
		}
	}

	public ulong UInt64Hash
	{
		get
		{
			if (hash == 9223372036854775808uL)
			{
				ulong num = 0uL;
				for (int i = 0; i < fields.Length; i++)
				{
					INavFieldMetadata navFieldMetadata = fields[i];
					int columnIndex = navFieldMetadata.ColumnIndex;
					if (columnIndex >= 0)
					{
						num = ((columnIndex > 61) ? (num | 0x4000000000000000L) : (num | (ulong)(1L << columnIndex)));
					}
				}
				hash = num;
			}
			return hash;
		}
	}

	public FieldList(params INavFieldMetadata[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		this.fields = fields;
	}

	public bool Equals(FieldList other)
	{
		if (this == other)
		{
			return true;
		}
		if (other == null || fields.Length != other.fields.Length || UInt64Hash != other.UInt64Hash)
		{
			return false;
		}
		for (int i = 0; i < fields.Length; i++)
		{
			if (!fields[i].Equals(other.fields[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		return Equals(obj as FieldList);
	}

	public override int GetHashCode()
	{
		ulong uInt64Hash = UInt64Hash;
		return (int)uInt64Hash ^ (int)(uInt64Hash >> 32);
	}

	bool IFieldList.HasFieldWithColumnIndex(int columnIndex)
	{
		for (int i = 0; i < fields.Length; i++)
		{
			if (fields[i].ColumnIndex == columnIndex)
			{
				return true;
			}
		}
		return false;
	}

	internal bool AreAllFieldsContainedInOtherAssumingSameParent(IFieldList other)
	{
		ulong uInt64Hash = UInt64Hash;
		if ((uInt64Hash & other.UInt64Hash) != uInt64Hash)
		{
			return false;
		}
		if ((uInt64Hash & 0x4000000000000000L) == 0L)
		{
			return true;
		}
		INavFieldMetadata[] array = fields;
		foreach (INavFieldMetadata navFieldMetadata in array)
		{
			if (navFieldMetadata.ColumnIndex > 61 && !other.HasFieldWithColumnIndex(navFieldMetadata.ColumnIndex))
			{
				return false;
			}
		}
		return true;
	}

	internal bool Contains(INavFieldMetadata field)
	{
		int columnIndex = field.ColumnIndex;
		if (columnIndex <= 61)
		{
			ulong num = (ulong)(1L << columnIndex);
			return (UInt64Hash & num) == num;
		}
		return Array.IndexOf(fields, field) >= 0;
	}

	internal bool AreAllFieldsContainedInOtherAssumingSameParent(IFieldList other, out List<INavFieldMetadata> fieldsNotContained)
	{
		ulong uInt64Hash = UInt64Hash;
		bool flag = true;
		List<INavFieldMetadata> list = null;
		if ((uInt64Hash & other.UInt64Hash) == uInt64Hash)
		{
			if ((uInt64Hash & 0x4000000000000000L) == 0L)
			{
				fieldsNotContained = null;
				return true;
			}
			INavFieldMetadata[] array = fields;
			foreach (INavFieldMetadata navFieldMetadata in array)
			{
				if (navFieldMetadata.ColumnIndex > 61 && !other.HasFieldWithColumnIndex(navFieldMetadata.ColumnIndex))
				{
					flag = false;
					if (list == null)
					{
						list = new List<INavFieldMetadata>();
					}
					list.Add(navFieldMetadata);
				}
			}
		}
		else
		{
			ulong uInt64Hash2 = other.UInt64Hash;
			flag = false;
			list = new List<INavFieldMetadata>();
			INavFieldMetadata[] array2 = fields;
			foreach (INavFieldMetadata navFieldMetadata2 in array2)
			{
				if ((navFieldMetadata2.ColumnIndex > 61 && !other.HasFieldWithColumnIndex(navFieldMetadata2.ColumnIndex)) || ((ulong)(1L << navFieldMetadata2.ColumnIndex) & uInt64Hash2) == 0L)
				{
					list.Add(navFieldMetadata2);
				}
			}
		}
		fieldsNotContained = (flag ? null : list);
		return flag;
	}

	internal FieldList UnionFieldsAssumingSameParent(FieldList fieldsToAdd)
	{
		if (fieldsToAdd.AreAllFieldsContainedInOtherAssumingSameParent(this, out var fieldsNotContained))
		{
			return this;
		}
		NCLMetaField[] array = new NCLMetaField[fields.Length + fieldsNotContained.Count];
		Array.Copy(fields, array, fields.Length);
		for (int i = 0; i < fieldsNotContained.Count; i++)
		{
			array[i + fields.Length] = (NCLMetaField)fieldsNotContained[i];
		}
		INavFieldMetadata[] array2 = array;
		return new FieldList(array2);
	}

	public IEnumerator<INavFieldMetadata> GetEnumerator()
	{
		return ((IEnumerable<INavFieldMetadata>)fields).GetEnumerator();
	}

	public IEnumerable<NCLMetaField> GetFieldsSortedBySqlColumnIndex()
	{
		IEnumerable<NCLMetaField> enumerable = AsNclMetaFields();
		if (enumerable != null)
		{
			return enumerable.OrderBy((NCLMetaField f) => f.SqlColumnIndex);
		}
		throw new InvalidCastException("The FieldList must be constructed from a list of fields that implement both INavFieldMetadata and INavOrderByColumn");
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public IEnumerable<NCLMetaField> AsNclMetaFields()
	{
		if (fields is NCLMetaField[] result)
		{
			return result;
		}
		NCLMetaField[] array = new NCLMetaField[fields.Length];
		Array.Copy(fields, array, fields.Length);
		INavFieldMetadata[] array2 = array;
		fields = array2;
		return array;
	}

	public ReadOnlyCollection<NCLMetaQueryColumn> AsNCLMetaQueryColumns()
	{
		NCLMetaQueryColumn[] array = fields as NCLMetaQueryColumn[];
		if (array == null)
		{
			array = new NCLMetaQueryColumn[fields.Length];
			Array.Copy(fields, array, fields.Length);
			INavFieldMetadata[] array2 = array;
			fields = array2;
		}
		return new ReadOnlyCollection<NCLMetaQueryColumn>(array);
	}

	static FieldList()
	{
		INavFieldMetadata[] array = new NCLMetaField[0];
		Empty = new FieldList(array);
	}
}
