using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.MetaModel;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class FilterFieldDictionary : FieldDictionary<FilterExpression>, ISizableObject
{
	private static readonly NavFieldMetadata multipleFields = NavFieldMetadata.DefaultMetadata(NavValueMetadata.DefaultMetadata(NavNclType.None));

	public new static readonly FilterFieldDictionary Empty = new FilterFieldDictionary();

	private static readonly BooleanConstantFilterExpression NoFilter = BooleanConstantFilterExpression.Get(value: true);

	private FilterExpression concatenatedFilterNormalFields;

	private FilterExpression concatenatedFilterFlowFilters;

	private FilterExpression concatenatedFilterFlowFields;

	private int? normalFieldsValueInsensitiveHashCode;

	private int? flowFiltersValueInsensitiveHashCode;

	private int? flowFieldsValueInsensitiveHashCode;

	public int ApproximateByteSize => SizableObject.GetApproximateByteSize(ConcatenatedFilterNormalFields, ConcatenatedFilterFlowFilters, ConcatenatedFilterFlowFields) + SizableObject.GetApproximateByteSize(6) + 24;

	internal static NavFieldMetadata MultipleFields => multipleFields;

	internal bool AnyFiltersOnNormalFields => ConcatenatedFilterNormalFields != null;

	internal bool AnyFiltersOnFlowFilterFields => ConcatenatedFilterFlowFilters != null;

	internal bool AnyFiltersOnFlowFields => ConcatenatedFilterFlowFields != null;

	internal FilterExpression ConcatenatedFilterNormalFields => ConcatenatedCachedFilter(ref concatenatedFilterNormalFields, FieldClass.Normal);

	internal FilterExpression ConcatenatedFilterFlowFilters => ConcatenatedCachedFilter(ref concatenatedFilterFlowFilters, FieldClass.FlowFilter);

	internal FilterExpression ConcatenatedFilterFlowFields => ConcatenatedCachedFilter(ref concatenatedFilterFlowFields, FieldClass.FlowField);

	internal int ValueInsensitiveHashCodeNormalFields => CachedHashCode(ref normalFieldsValueInsensitiveHashCode, ConcatenatedFilterNormalFields);

	internal int ValueInsensitiveHashCodeFlowFilters => CachedHashCode(ref flowFiltersValueInsensitiveHashCode, ConcatenatedFilterFlowFilters);

	internal int ValueInsensitiveHashCodeFlowFields => CachedHashCode(ref flowFieldsValueInsensitiveHashCode, ConcatenatedFilterFlowFields);

	internal FilterFieldDictionary(params Tuple<INavFieldMetadata, FilterExpression>[] items)
		: base(items)
	{
	}

	internal FilterFieldDictionary(IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> items)
		: base(items)
	{
	}

	internal FilterFieldDictionary(Dictionary<INavFieldMetadata, FilterExpression> lookup, bool copy = false)
		: base(lookup, copy)
	{
	}

	private FilterExpression BuildConcatenatedFilterFromFieldDictionaryExceptSortingFields(FieldClass includeOnlyThisFieldClass, SortingFieldList sortingFields)
	{
		return GetConcatenatedFilterExpression((Tuple<INavFieldMetadata, FilterExpression> filter) => filter.Item1.FieldClass == includeOnlyThisFieldClass && (sortingFields == null || !sortingFields.Contains(filter.Item1)));
	}

	private FilterExpression ConcatenatedCachedFilter(ref FilterExpression cachedValue, FieldClass fieldClass)
	{
		if (cachedValue == null)
		{
			cachedValue = BuildConcatenatedFilterFromFieldDictionaryExceptSortingFields(fieldClass, null) ?? NoFilter;
		}
		if (cachedValue != NoFilter)
		{
			return cachedValue;
		}
		return null;
	}

	private static int CachedHashCode(ref int? hashCode, FilterExpression filter)
	{
		if (!hashCode.HasValue)
		{
			hashCode = ValueInsensitiveFilterExpressionEqualityComparer.Instance.GetHashCode(filter);
		}
		return hashCode.Value;
	}

	internal FilterExpression GetFilterOnField(INavFieldMetadata field)
	{
		if (!TryGetValue(field, out var value))
		{
			return FilterExpression.BooleanConstant(value: true);
		}
		return value;
	}

	internal FilterExpression GetConcatenatedFilterExpression(Func<Tuple<INavFieldMetadata, FilterExpression>, bool> whereFunc)
	{
		FilterExpression filterExpression = null;
		foreach (Tuple<INavFieldMetadata, FilterExpression> item in Items.Where((Tuple<INavFieldMetadata, FilterExpression> filter) => whereFunc(filter)))
		{
			filterExpression = FilterExpression.And(filterExpression, item.Item2);
		}
		return filterExpression;
	}

	internal FilterExpression GetConcatenatedFiltersForNormalFieldsExceptSortingFields(SortingFieldList sortingFields)
	{
		if (sortingFields == null)
		{
			return ConcatenatedFilterNormalFields;
		}
		if (!AnyFiltersOnNormalFields)
		{
			return null;
		}
		return BuildConcatenatedFilterFromFieldDictionaryExceptSortingFields(FieldClass.Normal, sortingFields);
	}

	internal bool FiltersOnNormalFieldsAreValueInsensitiveEqualTo(FilterFieldDictionary otherFilterFieldDictionary)
	{
		if (otherFilterFieldDictionary == null)
		{
			return false;
		}
		if ((object)this == otherFilterFieldDictionary)
		{
			return true;
		}
		if (ValueInsensitiveHashCodeNormalFields == otherFilterFieldDictionary.ValueInsensitiveHashCodeNormalFields)
		{
			return ValueInsensitiveFilterExpressionEqualityComparer.Instance.Equals(ConcatenatedFilterNormalFields, otherFilterFieldDictionary.ConcatenatedFilterNormalFields);
		}
		return false;
	}

	internal FilterFieldDictionary AndNegatedFilters(FilterFieldDictionary filtersToNegate)
	{
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = ToDictionary();
		Tuple<INavFieldMetadata, FilterExpression>[] array = filtersToNegate.Items;
		foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in array)
		{
			FilterExpression filterExpression = tuple.Item2.Negate();
			if (dictionary.TryGetValue(tuple.Item1, out var value))
			{
				dictionary[tuple.Item1] = FilterExpression.And(value, filterExpression);
			}
			else
			{
				dictionary[tuple.Item1] = filterExpression;
			}
		}
		return new FilterFieldDictionary(dictionary);
	}

	public HashSet<NavAppOrigin> GetCompanionAppsOriginForTheUsedFilters()
	{
		HashSet<NavAppOrigin> distinctOrigins = new HashSet<NavAppOrigin>();
		Items?.Select((Tuple<INavFieldMetadata, FilterExpression> i) => i.Item1).OfType<NCLMetaField>().Where((NCLMetaField f) => f.IsCompanionTableField)
			.Select((NCLMetaField f) => f.AppOrigin)
			.ForEach((NavAppOrigin o) =>
			{
				distinctOrigins.Add(o);
			});
		return distinctOrigins;
	}
}
