using System;
using System.Linq;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FiltersAndMarks : ISizableObject
{
	internal static readonly FiltersAndMarks Empty = new FiltersAndMarks(null, null);

	private readonly FilterFieldDictionary filters;

	private readonly MarkedRecords markedRecords;

	private bool hashCodeCalculated;

	private int hashCode;

	private bool valueInsensitiveHashCodeCalculated;

	private int valueInsensitiveHashCode;

	public int ApproximateByteSize => SizableObject.GetApproximateByteSize(filters, markedRecords) + SizableObject.GetApproximateByteSize(6) + 24;

	public FilterFieldDictionary Filters => filters;

	public MarkedRecords MarkedRecords => markedRecords;

	public bool SetIsGuaranteedEmpty
	{
		get
		{
			if (filters != null && filters.Items.Length != 0)
			{
				Tuple<INavFieldMetadata, FilterExpression>[] items = filters.Items;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
				{
					if (tuple.Item1.FieldClass != FieldClass.FlowFilter)
					{
						FilterExpression item = tuple.Item2;
						if (item.IsConstantFalse())
						{
							return true;
						}
					}
				}
			}
			if (markedRecords != null)
			{
				FilterExpression completeExpression = MarkedRecords.GetCompleteExpression();
				if (completeExpression.IsConstantFalse())
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool IsEmpty
	{
		get
		{
			if (filters.IsNullOrEmpty())
			{
				return markedRecords == null;
			}
			return false;
		}
	}

	public FiltersAndMarks(FilterFieldDictionary filters, MarkedRecords markedRecords)
	{
		this.filters = filters;
		this.markedRecords = markedRecords;
	}

	public FiltersAndMarks(FilterFieldDictionary filters)
	{
		this.filters = filters;
	}

	public FiltersAndMarks(MarkedRecords markedRecords)
	{
		this.markedRecords = markedRecords;
	}

	public bool IsMultiField()
	{
		FilterFieldDictionary filterFieldDictionary = filters;
		if ((object)filterFieldDictionary == null || filterFieldDictionary.Items?.Select((Tuple<INavFieldMetadata, FilterExpression> i) => i.Item1).OfType<NavFieldMetadata>().Count((NavFieldMetadata i) => i.Equals(FilterFieldDictionary.MultipleFields)) != 0)
		{
			return filters?.Items != null;
		}
		return false;
	}

	public FilterExpression GetCombinedFilter()
	{
		return GetCombinedFilterExceptSortingFields(null);
	}

	public FilterExpression GetCombinedFilterExceptSortingFields(SortingFieldList sortingFields)
	{
		FilterExpression filterExpression = null;
		if (filters != null)
		{
			filterExpression = filters.GetConcatenatedFiltersForNormalFieldsExceptSortingFields(sortingFields);
		}
		if (markedRecords != null)
		{
			filterExpression = FilterExpression.And(filterExpression, markedRecords.GetCompleteExpression());
		}
		return filterExpression;
	}

	public override bool Equals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is FiltersAndMarks filtersAndMarks))
		{
			return false;
		}
		if (filters == null != (filtersAndMarks.filters == null) || markedRecords == null != (filtersAndMarks.markedRecords == null) || GetHashCode() != filtersAndMarks.GetHashCode())
		{
			return false;
		}
		if (filters != null && !filters.Equals(filtersAndMarks.filters))
		{
			return false;
		}
		if (markedRecords != null && !markedRecords.Equals(filtersAndMarks.markedRecords))
		{
			return false;
		}
		return true;
	}

	public bool ValueInsensitiveEquals(object obj)
	{
		if (this == obj)
		{
			return true;
		}
		if (!(obj is FiltersAndMarks filtersAndMarks))
		{
			return false;
		}
		if (filters == null != (filtersAndMarks.filters == null) || markedRecords == null != (filtersAndMarks.markedRecords == null) || GetValueInsensitiveHashCode() != filtersAndMarks.GetValueInsensitiveHashCode())
		{
			return false;
		}
		if (filters != null && !filters.FiltersOnNormalFieldsAreValueInsensitiveEqualTo(filtersAndMarks.filters))
		{
			return false;
		}
		if (markedRecords != null && !ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(markedRecords.GetCompleteExpression(), filtersAndMarks.markedRecords.GetCompleteExpression()))
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		if (!hashCodeCalculated)
		{
			hashCode = HashCodeHelper.CombineHashCodes((!(filters == null)) ? filters.GetHashCode() : 0, (markedRecords != null) ? markedRecords.GetHashCode() : 0);
			hashCodeCalculated = true;
		}
		return hashCode;
	}

	public int GetValueInsensitiveHashCode()
	{
		if (!valueInsensitiveHashCodeCalculated)
		{
			if (filters != null)
			{
				valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, filters.ValueInsensitiveHashCodeNormalFields);
			}
			if (markedRecords != null)
			{
				valueInsensitiveHashCode = HashCodeHelper.CombineHashCodes(valueInsensitiveHashCode, ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(markedRecords.GetCompleteExpression()));
			}
			valueInsensitiveHashCodeCalculated = true;
		}
		return valueInsensitiveHashCode;
	}
}
