using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class SortingFieldList : IEnumerable<INavSortingField>, IEnumerable
{
	public static readonly SortingFieldList Empty = new SortingFieldList(new SortingField[0]);

	private static readonly SortingFieldEqualityComparer SortingFieldEqualityComparer = new SortingFieldEqualityComparer();

	private readonly INavSortingField[] sortingFields;

	private SortingFieldList sortingFieldsReversed;

	private bool? isAscendingOnAllFields;

	private bool? isDescendingOnAllFields;

	private bool? hasFlowFields;

	private FieldList fieldList;

	private int? hashCode;

	public int Count
	{
		get
		{
			if (sortingFields != null)
			{
				return sortingFields.Length;
			}
			return 0;
		}
	}

	public FieldList FieldList
	{
		get
		{
			if (fieldList == null)
			{
				fieldList = new FieldList(sortingFields.Select((INavSortingField sortingField) => sortingField.Field).ToArray());
			}
			return fieldList;
		}
	}

	public SortingFieldList Reversed
	{
		get
		{
			if (sortingFieldsReversed == null)
			{
				sortingFieldsReversed = new SortingFieldList(fieldList, sortingFields.Select((INavSortingField sortingField) => new SortingField(sortingField.Field, (sortingField.Order == SortOrder.Ascending) ? SortOrder.Descending : SortOrder.Ascending)));
			}
			return sortingFieldsReversed;
		}
	}

	public INavSortingField this[int index] => sortingFields[index];

	public bool IsAscendingOnAllFields
	{
		get
		{
			if (!isAscendingOnAllFields.HasValue)
			{
				isAscendingOnAllFields = sortingFields.All((INavSortingField sortingField) => sortingField.Order == SortOrder.Ascending);
			}
			return isAscendingOnAllFields.Value;
		}
	}

	public bool IsDescendingOnAllFields
	{
		get
		{
			if (!isDescendingOnAllFields.HasValue)
			{
				isDescendingOnAllFields = sortingFields.All((INavSortingField sortingField) => sortingField.Order == SortOrder.Descending);
			}
			return isDescendingOnAllFields.Value;
		}
	}

	public bool HasFlowFields
	{
		get
		{
			if (!hasFlowFields.HasValue)
			{
				hasFlowFields = sortingFields.Any((INavSortingField sortingField) => sortingField.Field.FieldClass == FieldClass.FlowField);
			}
			return hasFlowFields.Value;
		}
	}

	private SortingFieldList(IEnumerable<INavSortingField> sortingFields)
	{
		this.sortingFields = sortingFields.ToArray();
	}

	private SortingFieldList(FieldList fieldList, IEnumerable<INavSortingField> sortingFields)
	{
		this.fieldList = fieldList;
		this.sortingFields = sortingFields.ToArray();
	}

	private SortingFieldList(FieldList sortingFields)
	{
		this.sortingFields = sortingFields.Cast<INavSortingField>().ToArray();
		fieldList = sortingFields;
	}

	private SortingFieldList(FieldList fieldList, SortOrder sortOrder)
	{
		INavSortingField[] array = fieldList.Select((INavFieldMetadata field) => new SortingField(field, sortOrder)).ToArray();
		sortingFields = array;
		this.fieldList = fieldList;
	}

	public static SortingFieldList Create(FieldList fieldList, SortOrder sortOrder)
	{
		if (fieldList != null && fieldList.Count != 0)
		{
			return new SortingFieldList(fieldList, sortOrder);
		}
		return Empty;
	}

	public static SortingFieldList Create(FieldList sortingFields)
	{
		if (sortingFields != null && sortingFields.Count != 0)
		{
			return new SortingFieldList(sortingFields);
		}
		return Empty;
	}

	public static SortingFieldList Create(IEnumerable<INavSortingField> sortingFields)
	{
		if (sortingFields != null && sortingFields.Any())
		{
			return new SortingFieldList(sortingFields);
		}
		return Empty;
	}

	public SortingFieldList DuplicateWithNewOrder(SortOrder sortOrder)
	{
		if (IsSameOrderOnAllFields(sortOrder))
		{
			return this;
		}
		return new SortingFieldList(fieldList, sortingFields.Select((INavSortingField sortingField) => new SortingField(sortingField.Field, sortOrder)));
	}

	public SortingFieldList CreateNewWithSingleFieldSetToOrder(INavFieldMetadata fieldToChange, SortOrder sortOrder)
	{
		INavSortingField navSortingField = sortingFields.FirstOrDefault((INavSortingField sortingField) => fieldToChange == sortingField.Field);
		if (navSortingField == null)
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.InvalidSortingField, fieldToChange.DescriptiveName, string.Join(",", sortingFields.Select((INavSortingField sf) => sf.Field.DescriptiveName).ToArray()));
			throw new NavInvalidSortingFieldsException(message);
		}
		if (navSortingField.Order == sortOrder)
		{
			return this;
		}
		return new SortingFieldList(fieldList, sortingFields.Select((INavSortingField sortingField) => (sortingField.Field != fieldToChange) ? sortingField : new SortingField(sortingField.Field, sortOrder)));
	}

	public IEnumerator<INavSortingField> GetEnumerator()
	{
		return ((IEnumerable<INavSortingField>)sortingFields).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override bool Equals(object obj)
	{
		if (!(obj is SortingFieldList sortingFieldList))
		{
			return false;
		}
		if (sortingFieldList == this)
		{
			return true;
		}
		return sortingFields.SequenceEqual(sortingFieldList, SortingFieldEqualityComparer);
	}

	public override int GetHashCode()
	{
		if (!hashCode.HasValue)
		{
			int num = 0;
			for (int i = 0; i < sortingFields.Length; i++)
			{
				INavSortingField obj = sortingFields[i];
				num = HashCodeHelper.CombineHashCodes(num, i, SortingFieldEqualityComparer.GetHashCode(obj));
			}
			hashCode = num;
		}
		return hashCode.Value;
	}

	public bool TryGetValue(INavFieldMetadata field, out SortOrder sortOrder)
	{
		INavSortingField navSortingField = sortingFields.FirstOrDefault((INavSortingField sortingField) => sortingField.Field == field);
		if (navSortingField != null)
		{
			sortOrder = navSortingField.Order;
			return true;
		}
		sortOrder = SortOrder.Ascending;
		return false;
	}

	internal bool Contains(INavFieldMetadata field)
	{
		return sortingFields.Any((INavSortingField sortingField) => sortingField.Field == field);
	}

	public bool IsSameOrderOnAllFields(SortOrder order)
	{
		if (order == SortOrder.Ascending)
		{
			return IsAscendingOnAllFields;
		}
		return IsDescendingOnAllFields;
	}
}
