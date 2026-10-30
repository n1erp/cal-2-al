using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class TableState
{
	private const int UnknownKeyIndex = -1;

	private readonly DataAccess dataAccess;

	private readonly NCLMetaTable metaTable;

	private FiltersAndMarks filtersAndMarks;

	private MarkedRecords markedRecords;

	private SortingFields sortingFields;

	private bool markedRecordsOnly;

	private SortOrder defaultSortOrder;

	private int companyNameToken;

	private int versionBumperToken;

	private FieldList autoCalcFields;

	private readonly Dictionary<int, Dictionary<INavFieldMetadata, FilterExpression>> filterGroups = new Dictionary<int, Dictionary<INavFieldMetadata, FilterExpression>>();

	private FilterFieldDictionary allFiltersDictionary;

	private int lastGlobalFilterBumperToken;

	internal SortingFields SortingFields
	{
		get
		{
			return sortingFields;
		}
		set
		{
			sortingFields = value;
			versionBumperToken++;
		}
	}

	internal NCLMetaKey Key { get; private set; }

	internal int FilterGroupNumber { get; set; }

	internal FieldList AutoCalcFields
	{
		get
		{
			return autoCalcFields;
		}
		set
		{
			if (autoCalcFields != value)
			{
				versionBumperToken++;
			}
			autoCalcFields = value;
		}
	}

	internal SortOrder DefaultSortOrder
	{
		get
		{
			return defaultSortOrder;
		}
		set
		{
			if (defaultSortOrder != value)
			{
				versionBumperToken++;
			}
			defaultSortOrder = value;
		}
	}

	internal int CompanyNameToken
	{
		get
		{
			return companyNameToken;
		}
		set
		{
			if (companyNameToken != value)
			{
				versionBumperToken++;
			}
			companyNameToken = value;
		}
	}

	internal bool MarkedRecordsOnly
	{
		get
		{
			return markedRecordsOnly;
		}
		set
		{
			if (value != markedRecordsOnly)
			{
				InvalidateCombinedFilterGroups();
			}
			markedRecordsOnly = value;
		}
	}

	public int VersionBumperToken => versionBumperToken;

	internal FilterFieldDictionary CurrentFilterGroup
	{
		get
		{
			if (FilterGroupNumber == 1)
			{
				return dataAccess.GlobalFilters.GetGlobalFilters(metaTable.TableId);
			}
			if (!filterGroups.TryGetValue(FilterGroupNumber, out var value))
			{
				return null;
			}
			return new FilterFieldDictionary(value);
		}
	}

	internal FiltersAndMarks FiltersAndMarks
	{
		get
		{
			ValidateGlobalFilterBumperToken();
			if (filtersAndMarks != null)
			{
				return filtersAndMarks;
			}
			MarkedRecords markedRecords;
			if (markedRecordsOnly)
			{
				this.markedRecords = this.markedRecords ?? MarkedRecords.CreateEmpty(metaTable);
				this.markedRecords.ReadOnly = true;
				markedRecords = this.markedRecords;
			}
			else
			{
				markedRecords = null;
			}
			filtersAndMarks = new FiltersAndMarks(AllFiltersDictionary, markedRecords);
			return filtersAndMarks;
		}
	}

	internal FilterFieldDictionary AllFiltersDictionary
	{
		get
		{
			ValidateGlobalFilterBumperToken();
			if (allFiltersDictionary == null)
			{
				allFiltersDictionary = CombineFiltersFromAllGroups(FilterExpression.And);
			}
			return allFiltersDictionary;
		}
	}

	internal int CurrentKeyIndex
	{
		get
		{
			if (Key != null)
			{
				return Key.KeyIndex + 1;
			}
			return -1;
		}
	}

	internal bool HasFilterInCurrentGroup
	{
		get
		{
			if (FilterGroupNumber == 1)
			{
				FieldDictionary<FilterExpression> globalFilters = dataAccess.GlobalFilters.GetGlobalFilters(metaTable.TableId);
				if (globalFilters != null)
				{
					return globalFilters.Items.Length != 0;
				}
				return false;
			}
			if (filterGroups.TryGetValue(FilterGroupNumber, out var value) && value.Count > 0)
			{
				return true;
			}
			return false;
		}
	}

	public IEnumerable<NavRecordId> MarkedRecordIds
	{
		get
		{
			if (markedRecords != null)
			{
				return markedRecords.MarkedRecordIds;
			}
			return new NavRecordId[0];
		}
	}

	internal int[] CurrentSortingFieldIds => SortingFields.DefinedSortingFields.Select((INavSortingField sortingField) => ((NCLMetaField)sortingField.Field).FieldNo).ToArray();

	internal TableState(NCLMetaTable metaTable, DataAccess dataAccess)
	{
		this.dataAccess = dataAccess;
		this.metaTable = metaTable;
	}

	private void ValidateGlobalFilterBumperToken()
	{
		if (allFiltersDictionary != null)
		{
			int globalFilterBumperToken = dataAccess.GlobalFilters.GlobalFilterBumperToken;
			if (globalFilterBumperToken != lastGlobalFilterBumperToken)
			{
				InvalidateCombinedFilterGroups();
				lastGlobalFilterBumperToken = globalFilterBumperToken;
			}
		}
	}

	private static KeyValuePair<int, Dictionary<INavFieldMetadata, FilterExpression>> HandleMultiFieldFilters(KeyValuePair<int, Dictionary<INavFieldMetadata, FilterExpression>> filterGroup)
	{
		if (filterGroup.Key != -1 || !filterGroup.Value.Any())
		{
			return filterGroup;
		}
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = new Dictionary<INavFieldMetadata, FilterExpression>();
		dictionary.Add(FilterFieldDictionary.MultipleFields, filterGroup.Value.Values.Aggregate((FilterExpression sumExpression, FilterExpression nextExpression) => FilterExpression.Or(sumExpression, nextExpression)));
		return new KeyValuePair<int, Dictionary<INavFieldMetadata, FilterExpression>>(filterGroup.Key, dictionary);
	}

	internal FilterFieldDictionary CombineFiltersFromAllGroups(Func<FilterExpression, FilterExpression, FilterExpression> combineFiltersFromSameField)
	{
		FilterFieldDictionary globalFilters = dataAccess.GlobalFilters.GetGlobalFilters(metaTable.TableId);
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = null;
		if (globalFilters == null)
		{
			switch (filterGroups.Count)
			{
			case 0:
				return FilterFieldDictionary.Empty;
			case 1:
				dictionary = HandleMultiFieldFilters(filterGroups.First()).Value;
				break;
			}
		}
		else if (filterGroups.Count == 0)
		{
			return globalFilters;
		}
		if (dictionary == null)
		{
			dictionary = new Dictionary<INavFieldMetadata, FilterExpression>();
			IEnumerable<KeyValuePair<INavFieldMetadata, FilterExpression>> enumerable = filterGroups.Select((KeyValuePair<int, Dictionary<INavFieldMetadata, FilterExpression>> filterGroup) => HandleMultiFieldFilters(filterGroup).Value).SelectMany((Dictionary<INavFieldMetadata, FilterExpression> groupDictionary) => groupDictionary);
			if (globalFilters != null)
			{
				enumerable = enumerable.Concat(globalFilters.Items.Select((Tuple<INavFieldMetadata, FilterExpression> t) => new KeyValuePair<INavFieldMetadata, FilterExpression>(t.Item1, t.Item2)));
			}
			foreach (KeyValuePair<INavFieldMetadata, FilterExpression> item in enumerable)
			{
				dictionary[item.Key] = (dictionary.TryGetValue(item.Key, out var value) ? combineFiltersFromSameField(value, item.Value) : item.Value);
			}
		}
		Tuple<INavFieldMetadata, FilterExpression>[] array = new Tuple<INavFieldMetadata, FilterExpression>[dictionary.Count];
		int num = 0;
		foreach (KeyValuePair<INavFieldMetadata, FilterExpression> item2 in dictionary)
		{
			array[num] = Tuple.Create(item2.Key, (item2.Key == FilterFieldDictionary.MultipleFields) ? item2.Value : item2.Value.Simplify());
			num++;
		}
		return new FilterFieldDictionary(array);
	}

	internal bool TryGetFilterOnFieldInCurrentGroup(NCLMetaField field, out FilterExpression filterExpression)
	{
		if (FilterGroupNumber > 255)
		{
			filterExpression = null;
			return false;
		}
		if (FilterGroupNumber == 1)
		{
			FieldDictionary<FilterExpression> globalFilters = dataAccess.GlobalFilters.GetGlobalFilters(metaTable.TableId);
			if (globalFilters != null && globalFilters.TryGetValue(field, out filterExpression))
			{
				return true;
			}
			filterExpression = null;
			return false;
		}
		if (filterGroups.TryGetValue(FilterGroupNumber, out var value) && value.TryGetValue(field, out filterExpression))
		{
			return true;
		}
		filterExpression = null;
		return false;
	}

	internal bool SetFiltersOnCurrentGroup(IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> fieldFilters, bool clearExistingFilters, bool checkChanged)
	{
		if (FilterGroupNumber > 255)
		{
			return false;
		}
		bool flag = false;
		if (FilterGroupNumber == 1)
		{
			foreach (Tuple<INavFieldMetadata, FilterExpression> fieldFilter in fieldFilters)
			{
				flag |= dataAccess.GlobalFilters.SetGlobalFilter((NCLMetaField)fieldFilter.Item1, fieldFilter.Item2);
			}
		}
		else
		{
			Dictionary<INavFieldMetadata, FilterExpression> orCreateCurrentFilterGroup = GetOrCreateCurrentFilterGroup();
			if (clearExistingFilters)
			{
				flag = true;
				orCreateCurrentFilterGroup.Clear();
			}
			foreach (Tuple<INavFieldMetadata, FilterExpression> fieldFilter2 in fieldFilters)
			{
				if (!flag & checkChanged)
				{
					flag |= !orCreateCurrentFilterGroup.TryGetValue(fieldFilter2.Item1, out var value) || !value.Equals(fieldFilter2.Item2);
				}
				orCreateCurrentFilterGroup[fieldFilter2.Item1] = fieldFilter2.Item2;
			}
		}
		if (flag || !checkChanged)
		{
			InvalidateCombinedFilterGroups();
		}
		return flag;
	}

	internal bool SetFilterOnCurrentGroup(NCLMetaField field, FilterExpression expression, bool checkForChange)
	{
		if (FilterGroupNumber > 255)
		{
			return false;
		}
		if (field.FieldClass == FieldClass.FlowField && FilterGroupNumber == -1)
		{
			return false;
		}
		bool flag = true;
		if (FilterGroupNumber == 1)
		{
			flag = dataAccess.GlobalFilters.SetGlobalFilter(field, expression);
		}
		else
		{
			Dictionary<INavFieldMetadata, FilterExpression> orCreateCurrentFilterGroup = GetOrCreateCurrentFilterGroup();
			if (expression == null)
			{
				flag = orCreateCurrentFilterGroup.Remove(field);
			}
			else
			{
				if (checkForChange)
				{
					flag = !orCreateCurrentFilterGroup.TryGetValue(field, out var value) || !expression.Equals(value);
				}
				if (flag)
				{
					orCreateCurrentFilterGroup[field] = expression;
				}
			}
		}
		if (flag || !checkForChange)
		{
			InvalidateCombinedFilterGroups();
		}
		return flag;
	}

	internal TableState Clone()
	{
		TableState tableState = new TableState(metaTable, dataAccess);
		tableState.CopyFrom(this);
		return tableState;
	}

	internal void CopyFrom(TableState source)
	{
		CopyFrom(source, CopyFilterOptions.CopyFilter | CopyFilterOptions.CopySorting | CopyFilterOptions.CopyMarkedList);
	}

	internal bool IsEquivalentTo(TableState other)
	{
		if (companyNameToken != other.companyNameToken || CurrentKeyIndex != other.CurrentKeyIndex || (CurrentKeyIndex == -1 && !SortingFields.ActualSortingFields.Equals(other.SortingFields.ActualSortingFields)) || markedRecordsOnly != other.MarkedRecordsOnly || !markedRecordsOnly)
		{
			return AllFiltersDictionary.Equals(other.AllFiltersDictionary);
		}
		return object.Equals(FiltersAndMarks, other.FiltersAndMarks);
	}

	internal void AssignKeyAndSortingFields(NCLMetaKey key, SortingFields newSortingFields)
	{
		if (key != Key)
		{
			versionBumperToken++;
		}
		Key = key;
		SortingFields = newSortingFields ?? new SortingFields(key.DefinedSortingFields.DuplicateWithNewOrder(defaultSortOrder), key.SortingFieldsWithPrimaryKeyFields.DuplicateWithNewOrder(defaultSortOrder));
	}

	internal bool AssignCustomSortingFields(NCLMetaField[] metaFields)
	{
		SortingFields sortingFields = SortingFields;
		bool flag = Key != null;
		bool result = false;
		Key = null;
		FieldList definedKeyFields = metaTable.PrimaryKey.DefinedKeyFields;
		INavFieldMetadata[] fields = metaFields.Where((NCLMetaField f) => f.FieldActive && !f.FieldObsoleted).ToArray();
		FieldList fieldList = new FieldList(fields);
		FieldList fieldList2 = fieldList.UnionFieldsAssumingSameParent(definedKeyFields);
		SortingFields = new SortingFields(SortingFieldList.Create(fieldList, defaultSortOrder), SortingFieldList.Create(fieldList2, defaultSortOrder));
		if (flag || !SortingFields.ActualSortingFields.Equals(sortingFields.ActualSortingFields))
		{
			versionBumperToken++;
			result = true;
		}
		return result;
	}

	internal void Reset()
	{
		FilterGroupNumber = 0;
		DefaultSortOrder = SortOrder.Ascending;
		AssignKeyAndSortingFields(metaTable.PrimaryKey, null);
		filterGroups.Clear();
		markedRecords = null;
		InvalidateCombinedFilterGroups();
		markedRecordsOnly = false;
		versionBumperToken++;
	}

	internal bool ClearFilterGroup(int filterGroupNumber)
	{
		if (filterGroups.ContainsKey(filterGroupNumber))
		{
			filterGroups.Remove(filterGroupNumber);
			InvalidateCombinedFilterGroups();
			return true;
		}
		return false;
	}

	public bool ClearAllFilters()
	{
		bool flag = true;
		if (filterGroups.Count > 0)
		{
			flag = false;
		}
		filterGroups.Clear();
		if (flag)
		{
			InvalidateCombinedFilterGroups();
		}
		return flag;
	}

	internal NavTableView GetNavTableView()
	{
		return new NavTableView
		{
			TableId = metaTable.TableId,
			Ascending = (defaultSortOrder == SortOrder.Ascending),
			CurrentFilters = GetNavFilterGroups(),
			CurrentSortingFieldIds = CurrentSortingFieldIds
		};
	}

	internal NavFilterGroup[] GetNavFilterGroups()
	{
		IEnumerable<NavFilterGroup> enumerable = from g in filterGroups
			where g.Value.Count > 0
			select new NavFilterGroup(g.Key, g.Value.Select((KeyValuePair<INavFieldMetadata, FilterExpression> f) => CreateNavFilter((NCLMetaField)f.Key, f.Value)).ToArray());
		FieldDictionary<FilterExpression> globalFilters = dataAccess.GlobalFilters.GetGlobalFilters(metaTable.TableId);
		if (globalFilters != null && globalFilters.Items.Length != 0)
		{
			NavFilterGroup navFilterGroup = new NavFilterGroup(1, globalFilters.Items.Select((Tuple<INavFieldMetadata, FilterExpression> gf) => CreateNavFilter((NCLMetaField)gf.Item1, gf.Item2)).ToArray());
			enumerable = enumerable.Concat(new NavFilterGroup[1] { navFilterGroup });
		}
		return enumerable.ToArray();
	}

	private static NavFilter CreateNavFilter(NCLMetaField field, FilterExpression filter)
	{
		string text = filter.ToString(FilterExpressionStringOptions.OptionsAsIntegers);
		string optionsAsCaptionsFilterValue = filter.ToString(FilterExpressionStringOptions.None);
		bool isExactValue = filter.ExpressionType == FilterExpressionType.Equal && text.Length > 0 && text[0] != '\'' && text[0] != '=';
		return new NavFilter(field.FieldNo, text)
		{
			IsExactValue = isExactValue,
			OptionsAsCaptionsFilterValue = optionsAsCaptionsFilterValue
		};
	}

	internal bool ClearMarks()
	{
		bool flag = markedRecordsOnly && markedRecords != null;
		if (flag)
		{
			versionBumperToken++;
		}
		filtersAndMarks = null;
		markedRecords = null;
		return flag;
	}

	internal bool IsMarked(MutableRecordBuffer recordBuffer)
	{
		if (markedRecords != null)
		{
			return markedRecords.IsMarked(recordBuffer);
		}
		return false;
	}

	internal bool SetMarkedState(MutableRecordBuffer recordBuffer, bool marked)
	{
		PrepareMarkedRecordsForModification();
		if (markedRecords.SetMarked(recordBuffer, marked) && markedRecordsOnly)
		{
			filtersAndMarks = null;
			versionBumperToken++;
			return true;
		}
		return false;
	}

	internal void ResetMarks(NavRecordId[] recordIds, bool allExceptThese)
	{
		ClearMarks();
		markedRecords = (allExceptThese ? MarkedRecords.CreateWithExclusions(metaTable, AllFiltersDictionary, recordIds) : MarkedRecords.CreateWithMarks(metaTable, recordIds));
		versionBumperToken++;
	}

	internal void CopyFrom(TableState source, CopyFilterOptions options)
	{
		if (source.metaTable != metaTable)
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotCopyAllFiltersBetweenDifferentTables, metaTable.TableCaptions.GetValueOrDefault(), source.metaTable.TableCaptions.GetValueOrDefault()));
		}
		versionBumperToken++;
		if ((options & CopyFilterOptions.CopyFilter) != 0)
		{
			filterGroups.Clear();
			foreach (KeyValuePair<int, Dictionary<INavFieldMetadata, FilterExpression>> filterGroup in source.filterGroups)
			{
				filterGroups.Add(filterGroup.Key, new Dictionary<INavFieldMetadata, FilterExpression>(filterGroup.Value));
			}
			FilterGroupNumber = source.FilterGroupNumber;
			allFiltersDictionary = source.allFiltersDictionary;
			lastGlobalFilterBumperToken = source.lastGlobalFilterBumperToken;
		}
		if ((options & CopyFilterOptions.CopyMarkedList) != 0)
		{
			markedRecordsOnly = source.markedRecordsOnly;
			markedRecords = ((source.markedRecords == null) ? null : source.markedRecords.Clone());
		}
		if ((options & CopyFilterOptions.CopySorting) != 0)
		{
			DefaultSortOrder = source.DefaultSortOrder;
			AssignKeyAndSortingFields(source.Key, source.SortingFields);
		}
		filtersAndMarks = null;
	}

	private Dictionary<INavFieldMetadata, FilterExpression> GetOrCreateCurrentFilterGroup()
	{
		if (!filterGroups.TryGetValue(FilterGroupNumber, out var value))
		{
			value = new Dictionary<INavFieldMetadata, FilterExpression>();
			filterGroups.Add(FilterGroupNumber, value);
		}
		return value;
	}

	private void InvalidateCombinedFilterGroups()
	{
		allFiltersDictionary = null;
		filtersAndMarks = null;
		versionBumperToken++;
	}

	private void PrepareMarkedRecordsForModification()
	{
		if (markedRecords != null && markedRecords.ReadOnly)
		{
			markedRecords = markedRecords.Clone();
		}
		else if (markedRecords == null)
		{
			markedRecords = MarkedRecords.CreateEmpty(metaTable);
		}
	}
}
