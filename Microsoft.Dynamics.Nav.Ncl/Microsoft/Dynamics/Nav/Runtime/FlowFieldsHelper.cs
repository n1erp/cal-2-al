using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public static class FlowFieldsHelper
{
	private const int MaxNoOfColumnsInSumIndexKey = 20;

	private const int MaxRecursionLevel = 50;

	internal static bool CanCoerce(NCLMetaField sourceType, NCLMetaField targetType)
	{
		if (!sourceType.IsStringType || !targetType.IsStringType)
		{
			if (sourceType.FieldNclType == NavNclType.NavOption)
			{
				return targetType.FieldNclType == NavNclType.NavOption;
			}
			return false;
		}
		return true;
	}

	internal static void CheckFlowFieldProperties(NCLMetaField flowField)
	{
		NCLMetaField sourceField = flowField.CalculationFormula.SourceField;
		switch (flowField.CalculationFormula.CalculationMethod)
		{
		case NCLMetaCalculationMethod.Count:
			if (flowField.CalculationFormula.CalculationMethod == NCLMetaCalculationMethod.Count && flowField.FieldNavType != NavType.Integer && flowField.FieldNavType != NavType.BigInteger)
			{
				throw new NavCSideException(18023676, string.Format(CultureInfo.CurrentCulture, Lang.FlowFieldMustBeIntegerError, flowField.FieldName, flowField.Parent.TableName));
			}
			break;
		case NCLMetaCalculationMethod.Sum:
		case NCLMetaCalculationMethod.Average:
			if (!sourceField.CanBeAggregated)
			{
				throw new NavCSideException(18023674, string.Format(CultureInfo.CurrentCulture, Lang.SumAverageOnlySupportedForNumericFields, flowField.DescriptiveName, flowField.DescriptiveParentName));
			}
			if (!NavValueMetadata.CompareTypeAndMetadata(flowField, sourceField) && !CanCoerce(sourceField, flowField))
			{
				throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.FieldTypeMismatchError, flowField.FieldName, sourceField.FieldName, flowField.Parent.TableName, sourceField.Parent.TableName, flowField.FieldNavType.ToString(), sourceField.FieldNavType.ToString()));
			}
			break;
		case NCLMetaCalculationMethod.Exists:
			if (flowField.FieldNavType != NavType.Boolean)
			{
				throw new NavCSideException(18023675, string.Format(CultureInfo.CurrentCulture, Lang.FlowFieldMustBeBooleanError, flowField.FieldName, flowField.Parent.TableName));
			}
			break;
		case NCLMetaCalculationMethod.Min:
		case NCLMetaCalculationMethod.Max:
		case NCLMetaCalculationMethod.Lookup:
			if (!NavValueMetadata.CompareTypeAndMetadata(flowField, sourceField) && !CanCoerce(sourceField, flowField))
			{
				throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.FieldTypeMismatchError, flowField.FieldName, sourceField.FieldName, flowField.Parent.TableName, sourceField.Parent.TableName, flowField.FieldNavType.ToString(), sourceField.FieldNavType.ToString()));
			}
			break;
		}
	}

	internal static ICollection<DistinctSourceTable> GetDistinctSourceTablesFromFlowFields(NavSession session, int companyToken, IRecordBuffer currentRecord, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, IEnumerable<INavFieldMetadata> flowFields, int recursionLevel = 0)
	{
		Dictionary<Tuple<NCLMetaTable, FilterFieldDictionary>, DistinctSourceTable> dictionary = new Dictionary<Tuple<NCLMetaTable, FilterFieldDictionary>, DistinctSourceTable>();
		foreach (INavFieldMetadata flowField in flowFields)
		{
			NCLMetaField nCLMetaField = (NCLMetaField)flowField;
			if (nCLMetaField.FieldClass != FieldClass.FlowField)
			{
				throw new NavCSideException(18023494, string.Format(CultureInfo.CurrentCulture, Lang.OnlyFlowFieldsAllowedInCallsToCalcFields, nCLMetaField.FieldName, nCLMetaField.Parent.TableName));
			}
			NCLMetaTable parent = nCLMetaField.CalculationFormula.SourceField.Parent;
			FilterFieldDictionary filterFromMetaFilterCollection = GetFilterFromMetaFilterCollection(session, companyToken, currentRecord, filtersAndMarks, flowFieldSecurityFiltering, nCLMetaField.CalculationFormula.Filters, recursionLevel);
			filterFromMetaFilterCollection = DataProviderHelper.MergeFilterFieldDictionaries(filterFromMetaFilterCollection, NavCurrentThread.Session.DataAccessSource.GlobalFilters.GetGlobalFilters(parent.TableId));
			if (!dictionary.TryGetValue(new Tuple<NCLMetaTable, FilterFieldDictionary>(parent, filterFromMetaFilterCollection), out var value))
			{
				value = new DistinctSourceTable(parent, filterFromMetaFilterCollection);
				dictionary.Add(new Tuple<NCLMetaTable, FilterFieldDictionary>(parent, filterFromMetaFilterCollection), value);
			}
			value.AddField(nCLMetaField);
		}
		return dictionary.Values;
	}

	internal static NCLMetaKey CalculateBestSIFTIndex(NCLMetaTable table, FieldList aggregatedFields, FieldList groupByFields, FilterFieldDictionary filters)
	{
		int num = 1048575;
		int num2 = num;
		NCLMetaKey result = null;
		for (int i = 0; i < table.KeyCount; i++)
		{
			NCLMetaKey keyByIndex = table.GetKeyByIndex(i);
			if (keyByIndex.KeyIsObsoleted || !keyByIndex.KeyIsActive || !keyByIndex.MaintainSumIndex || keyByIndex.SumFieldsList == null || (filters != null && !filters.AreAllFieldsContainedInListAssumingSameParent(keyByIndex.DefinedSortingFields.FieldList)) || (groupByFields != null && !groupByFields.AreAllFieldsContainedInOtherAssumingSameParent(keyByIndex.DefinedSortingFields.FieldList, out var _)))
			{
				continue;
			}
			if (aggregatedFields != null)
			{
				foreach (INavFieldMetadata aggregatedField in aggregatedFields)
				{
					NCLMetaField nCLMetaField = aggregatedField as NCLMetaField;
					if (!nCLMetaField.CanBeAggregated)
					{
						throw new NavCSideException(18023674, string.Format(CultureInfo.CurrentCulture, Lang.SumAverageOnlySupportedForNumericFields, nCLMetaField.DescriptiveName, nCLMetaField.DescriptiveParentName));
					}
				}
				if (!aggregatedFields.AreAllFieldsContainedInOtherAssumingSameParent(keyByIndex.SumFieldsList, out var _))
				{
					continue;
				}
			}
			int num3 = 0;
			if (filters == null)
			{
				int count = keyByIndex.DefinedSortingFields.Count;
				num3 = num >> 20 - count << 20 - count;
			}
			else
			{
				for (int j = 0; j < keyByIndex.KeyFieldCount; j++)
				{
					INavFieldMetadata navFieldMetadata = keyByIndex.DefinedKeyFields[j];
					if (!filters.ContainsFieldWithColumnIndex(navFieldMetadata.ColumnIndex))
					{
						num3 += 1 << 20 - j - 1;
					}
				}
			}
			if (num3 < num2)
			{
				num2 = num3;
				result = keyByIndex;
			}
		}
		return result;
	}

	internal static FieldDictionary<NavValue> CalcFields(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, NCLMetaField[] fieldsToCalc, bool onlyFieldsSourcedFromVirtualTables, SecurityFiltering flowFieldSecurityFiltering, int recursionLevel = 0)
	{
		if (recursionLevel > 50)
		{
			throw session.Diagnostics.SendExceptionTag("000001X", Category.Runtime, new NavNCLStackOverflowException());
		}
		FieldDictionary<NavValue> fieldDictionary = FieldDictionary<NavValue>.Empty;
		_ = FieldDictionary<NavValue>.Empty;
		if (((NCLMetaTable)recordBuffer.MetaApplicationObject).HasFlowFieldsSourcedFromVirtualTables)
		{
			List<NCLMetaField> list = new List<NCLMetaField>();
			List<NCLMetaField> list2 = new List<NCLMetaField>();
			NCLMetaField[] array = fieldsToCalc;
			foreach (NCLMetaField nCLMetaField in array)
			{
				if (nCLMetaField.FieldNclType != NavNclType.NavBlob && nCLMetaField.SourceTableField.Parent.IsVirtualTable)
				{
					list.Add(nCLMetaField);
				}
				else
				{
					list2.Add(nCLMetaField);
				}
			}
			if (list.Count > 0)
			{
				fieldDictionary = CalcFieldsFromVirtualTables(session, companyToken, recordBuffer, filtersAndMarks, flowFieldSecurityFiltering, list);
				fieldsToCalc = list2.ToArray();
			}
		}
		if (fieldsToCalc.Length != 0 && !onlyFieldsSourcedFromVirtualTables)
		{
			FieldDictionary<NavValue> fieldDictionary2 = CalcFieldsFromNonVirtualTables(session, companyToken, recordBuffer, filtersAndMarks, flowFieldSecurityFiltering, fieldsToCalc, (recursionLevel == 0) ? 1 : (++recursionLevel));
			if (fieldDictionary.Items.Length != 0)
			{
				return new FieldDictionary<NavValue>(fieldDictionary2.Items.Concat(fieldDictionary.Items));
			}
			return fieldDictionary2;
		}
		return fieldDictionary;
	}

	private static FieldDictionary<NavValue> CalcNumeric(NavSession session, int companyToken, IRecordBuffer recordBuffer, NCLMetaTable sourceTable, FiltersAndMarks sourceFiltersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalc)
	{
		CalcNumericCacheRequest request = new CalcNumericCacheRequest(companyToken, sourceTable, cacheable: true, sourceFiltersAndMarks, flowFieldSecurityFiltering, recordBuffer, fieldsToCalc);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(sourceTable);
		return dataAccessForTable.CalcNumeric(request);
	}

	private static FieldDictionary<NavValue> CalcMinMax(NavSession session, int companyToken, IRecordBuffer recordBuffer, NCLMetaTable sourceTable, FiltersAndMarks sourceFiltersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalc)
	{
		CalcMinMaxCacheRequest request = new CalcMinMaxCacheRequest(companyToken, sourceTable, cacheable: true, sourceFiltersAndMarks, flowFieldSecurityFiltering, recordBuffer, fieldsToCalc);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(sourceTable);
		return dataAccessForTable.CalcMinMax(request);
	}

	private static NavValue CoerceSourceTableValue(NCLMetaField field, MutableRecordBuffer recordBuffer)
	{
		NCLMetaField sourceField = field.CalculationFormula.SourceField;
		NavValue navValue = recordBuffer[sourceField.FieldIndex];
		if (field.FieldNclType != sourceField.FieldNclType)
		{
			return NavValue.CreateNavValueFromObject(field, navValue);
		}
		return navValue;
	}

	private static FieldDictionary<NavValue> CalcLookup(NavSession session, int companyToken, NCLMetaTable sourceTable, FiltersAndMarks sourceFiltersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalc)
	{
		FindCacheRequest request = new FindCacheRequest(companyToken, sourceTable, cacheable: true, sourceFiltersAndMarks, null, flowFieldSecurityFiltering, null, sourceTable.PrimaryKey.DefinedSortingFields, FindType.FirstOnly, 1);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(sourceTable);
		if (dataAccessForTable.Find(request).TryMoveNext(out var resultRecordBuffer) == ResultSetMoveResult.Ok)
		{
			return new FieldDictionary<NavValue>(from lookupField in fieldsToCalc.AsNclMetaFields()
				select Tuple.Create((INavFieldMetadata)lookupField, CoerceSourceTableValue(lookupField, resultRecordBuffer)));
		}
		return new FieldDictionary<NavValue>(fieldsToCalc.Select((INavFieldMetadata field) => Tuple.Create(field, field.InitValue)));
	}

	private static FieldDictionary<NavValue> CalcExists(NavSession session, int companyToken, NCLMetaTable sourceTable, FiltersAndMarks sourceFiltersAndMarks, SecurityFiltering flowFieldSecurityFiltering, FieldList fieldsToCalc)
	{
		ExistsCacheRequest request = new ExistsCacheRequest(companyToken, sourceTable, cacheable: true, sourceFiltersAndMarks, flowFieldSecurityFiltering, forceNonLockingProviderRequest: false);
		DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(sourceTable);
		bool exists = dataAccessForTable.Exists(request);
		return new FieldDictionary<NavValue>(from existsField in fieldsToCalc.AsNclMetaFields()
			select Tuple.Create((INavFieldMetadata)existsField, (NavValue)(existsField.CalculationFormula.NegateResult ? NavBoolean.Create(!exists) : NavBoolean.Create(exists))));
	}

	internal static bool FieldsAndFormulaAreSelfReferencing(NCLMetaField[] fieldsToCalc)
	{
		if (fieldsToCalc == null)
		{
			throw new ArgumentNullException("fieldsToCalc");
		}
		foreach (NCLMetaField nCLMetaField in fieldsToCalc)
		{
			NCLMetaCalculationFormula calculationFormula = nCLMetaField.CalculationFormula;
			foreach (NCLMetaFilter filter in calculationFormula.Filters)
			{
				if (filter.FilterType == NCLMetaFilterType.Field)
				{
					NCLMetaFilterField nCLMetaFilterField = (NCLMetaFilterField)filter;
					if (nCLMetaFilterField.ValueField == nCLMetaField)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	internal static FieldDictionary<NavValue> CalcFieldsFromNonVirtualTables(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, NCLMetaField[] fieldsToCalc, int recursionLevel = 0)
	{
		if (FieldsAndFormulaAreSelfReferencing(fieldsToCalc))
		{
			throw session.Diagnostics.SendExceptionTag("000001Y", Category.Runtime, new NavNCLStackOverflowException());
		}
		ICollection<DistinctSourceTable> distinctSourceTablesFromFlowFields = GetDistinctSourceTablesFromFlowFields(session, companyToken, recordBuffer, filtersAndMarks, flowFieldSecurityFiltering, fieldsToCalc.Where((NCLMetaField f) => f.FieldNclType != NavNclType.NavBlob), recursionLevel);
		List<Tuple<INavFieldMetadata, NavValue>> list = new List<Tuple<INavFieldMetadata, NavValue>>();
		foreach (DistinctSourceTable item in distinctSourceTablesFromFlowFields)
		{
			int num = companyToken;
			if (item.Table.DataPerCompany)
			{
				if (companyToken == 0)
				{
					num = NavCurrentThread.Session.Company.CompanyNameToken;
				}
			}
			else
			{
				num = 0;
			}
			FilterFieldDictionary filterFieldDictionary = item.Filters;
			if (flowFieldSecurityFiltering != SecurityFiltering.Ignored)
			{
				FilterFieldDictionary securityFilters = session.GetSecurityFilters(num, item.Table.TableId, SecurityFilterType.Read, NavCurrentThread.Session.CurrentMethodScope.ApplicationObject, null);
				if (securityFilters != FilterFieldDictionary.Empty)
				{
					switch (flowFieldSecurityFiltering)
					{
					case SecurityFiltering.Validated:
					{
						ExistsCacheRequest request = new ExistsCacheRequest(num, item.Table, cacheable: true, new FiltersAndMarks(filterFieldDictionary.AndNegatedFilters(securityFilters)), SecurityFiltering.Ignored, forceNonLockingProviderRequest: false);
						DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(item.Table);
						if (dataAccessForTable.Exists(request))
						{
							throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterViolated, item.Table.TableName));
						}
						break;
					}
					case SecurityFiltering.Disallowed:
						throw new NavPermissionException(string.Format(CultureInfo.CurrentCulture, Lang.SecurityFilterNotAllowed, item.Table.TableName));
					case SecurityFiltering.Filtered:
						filterFieldDictionary = DataProviderHelper.MergeFilterFieldDictionaries(filterFieldDictionary, securityFilters);
						break;
					}
				}
			}
			if (item.NumericFlowFields != null)
			{
				list.AddRange(CalcNumeric(session, num, recordBuffer, item.Table, new FiltersAndMarks(filterFieldDictionary), flowFieldSecurityFiltering, item.NumericFlowFields).Items);
			}
			if (item.MinMaxFlowFields != null)
			{
				list.AddRange(CalcMinMax(session, num, recordBuffer, item.Table, new FiltersAndMarks(filterFieldDictionary), flowFieldSecurityFiltering, item.MinMaxFlowFields).Items);
			}
			if (item.LookupFlowFields != null)
			{
				list.AddRange(CalcLookup(session, num, item.Table, new FiltersAndMarks(filterFieldDictionary), flowFieldSecurityFiltering, item.LookupFlowFields).Items);
			}
			if (item.ExistsFlowFields != null)
			{
				list.AddRange(CalcExists(session, num, item.Table, new FiltersAndMarks(filterFieldDictionary), flowFieldSecurityFiltering, item.ExistsFlowFields).Items);
			}
		}
		return new FieldDictionary<NavValue>(list);
	}

	private static FieldDictionary<NavValue> CalcFieldsFromVirtualTables(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering, IEnumerable<NCLMetaField> fieldsSourcedFromVirtualTables)
	{
		return new FieldDictionary<NavValue>(fieldsSourcedFromVirtualTables.Select((NCLMetaField f) => new Tuple<INavFieldMetadata, NavValue>(f, CalcSingleFieldFromVirtualTable(session, companyToken, recordBuffer, filtersAndMarks, securityFiltering, f))));
	}

	private static NavValue CalcSingleFieldFromVirtualTable(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, SecurityFiltering flowFieldSecurityFiltering, NCLMetaField flowField)
	{
		NCLMetaField sourceTableField = flowField.SourceTableField;
		NCLMetaTable parent = sourceTableField.Parent;
		NCLMetaCalculationFormula calculationFormula = flowField.CalculationFormula;
		FilterFieldDictionary filterFromMetaFilterCollection = GetFilterFromMetaFilterCollection(session, companyToken, recordBuffer, filtersAndMarks, flowFieldSecurityFiltering, calculationFormula.Filters);
		using NavRecord navRecord = parent.CreateObjectInstance(session, isTemporary: false, null, session.Database.GetCompanyNameFromToken(companyToken), SecurityFiltering.Ignored);
		if (filterFromMetaFilterCollection != null)
		{
			Tuple<INavFieldMetadata, FilterExpression>[] items = filterFromMetaFilterCollection.Items;
			foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
			{
				navRecord.RecordImplementation.SetFieldFilter((NCLMetaField)tuple.Item1, tuple.Item2);
			}
		}
		switch (calculationFormula.CalculationMethod)
		{
		case NCLMetaCalculationMethod.Exists:
			return NavBoolean.Create(!navRecord.ALIsEmpty);
		case NCLMetaCalculationMethod.Count:
			return NavInteger.Create(navRecord.ALCount);
		case NCLMetaCalculationMethod.Lookup:
		{
			navRecord.ALFindFirst(DataError.TrapError);
			NavValue fieldValue = navRecord.GetFieldValue(sourceTableField);
			if (!fieldValue.CompareTypeAndMetadata(flowField))
			{
				if (!CanCoerce(sourceTableField, flowField))
				{
					throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.FieldTypeMismatchError, flowField.FieldName, sourceTableField.FieldName, flowField.Parent.TableName, sourceTableField.Parent.TableName, flowField.FieldNavType.ToString(), sourceTableField.FieldNavType.ToString()));
				}
				return NavValue.CreateNavValueFromObject(flowField, navRecord.GetFieldValue(sourceTableField));
			}
			return fieldValue;
		}
		case NCLMetaCalculationMethod.Min:
			if (!navRecord.ALSetCurrentKey(DataError.TrapError, sourceTableField.FieldNo))
			{
				break;
			}
			goto case NCLMetaCalculationMethod.Lookup;
		case NCLMetaCalculationMethod.Max:
			navRecord.ALAscending = false;
			goto case NCLMetaCalculationMethod.Min;
		}
		return CalcSingleFieldFromVirtualTableSlow(sourceTableField, flowField, navRecord);
	}

	private static NavValue CalcSingleFieldFromVirtualTableSlow(NCLMetaField sourceField, NCLMetaField flowField, NavRecord sourceRecord)
	{
		NCLMetaCalculationFormula calculationFormula = flowField.CalculationFormula;
		Decimal18 @decimal = 0;
		int num = 0;
		if (sourceRecord.ALFind(DataError.TrapError, "-"))
		{
			do
			{
				Decimal18 decimal2 = sourceRecord.GetFieldValue(sourceField).ToDecimal();
				num++;
				switch (calculationFormula.CalculationMethod)
				{
				case NCLMetaCalculationMethod.Sum:
				case NCLMetaCalculationMethod.Average:
					@decimal += decimal2;
					break;
				case NCLMetaCalculationMethod.Min:
					if (num == 1 || decimal2 < @decimal)
					{
						@decimal = decimal2;
					}
					break;
				case NCLMetaCalculationMethod.Max:
					if (num == 1 || decimal2 > @decimal)
					{
						@decimal = decimal2;
					}
					break;
				default:
					throw new NotSupportedException();
				}
			}
			while (sourceRecord.NextRecord());
		}
		switch (calculationFormula.CalculationMethod)
		{
		case NCLMetaCalculationMethod.Sum:
			return NavValue.CreateNavValueFromObject(flowField, calculationFormula.NegateResult ? (-@decimal) : @decimal);
		case NCLMetaCalculationMethod.Min:
		case NCLMetaCalculationMethod.Max:
			return NavValue.CreateNavValueFromObject(flowField, @decimal);
		case NCLMetaCalculationMethod.Average:
		{
			Decimal18 decimal3 = ((num > 0) ? (@decimal / num) : ((Decimal18)0));
			return NavValue.CreateNavValueFromObject(flowField, calculationFormula.NegateResult ? (-decimal3) : decimal3);
		}
		default:
			throw new NotSupportedException();
		}
	}

	internal static bool AnyFiltersOnFlowFields(FiltersAndMarks filters)
	{
		if (filters.Filters != null)
		{
			return filters.Filters.AnyFiltersOnFlowFields;
		}
		return false;
	}

	internal static bool RecordIsWithinFilteredFlowFields(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering)
	{
		NCLMetaField[] flowFieldsWithAFilter = (from t in filtersAndMarks.Filters.Items
			where t.Item1.FieldClass == FieldClass.FlowField
			select (NCLMetaField)t.Item1).ToArray();
		return RecordIsWithinFilteredFlowFields(session, companyToken, recordBuffer, filtersAndMarks, flowFieldsWithAFilter, securityFiltering);
	}

	internal static bool RecordIsWithinFilteredFlowFields(NavSession session, int companyToken, IRecordBuffer recordBuffer, FiltersAndMarks filtersAndMarks, NCLMetaField[] flowFieldsWithAFilter, SecurityFiltering securityFiltering)
	{
		FieldDictionary<NavValue> fieldDictionary = CalcFields(session, companyToken, recordBuffer, filtersAndMarks, flowFieldsWithAFilter, onlyFieldsSourcedFromVirtualTables: false, securityFiltering);
		return fieldDictionary.Items.All((Tuple<INavFieldMetadata, NavValue> tuple) => filtersAndMarks.Filters[tuple.Item1].Evaluate(tuple.Item2));
	}

	internal static FilterExpression GetValueBasedFilter(NCLMetaFilterField metaFilter, NavValue value)
	{
		NCLMetaField nCLMetaField = (NCLMetaField)metaFilter.ValueField;
		NCLMetaField sourceField = metaFilter.SourceField;
		NavValue navValue = value ?? nCLMetaField.InitValue;
		if ((metaFilter.FilterModes & NCLMetaFilterModes.ValueIsFilter) == 0)
		{
			if (metaFilter.ValueField.NclType != metaFilter.SourceField.FieldNclType)
			{
				return FilterExpression.Equal(RecordImplementationHelper.TransferFieldValue(sourceField, nCLMetaField, navValue), sourceField.ExpressionContext);
			}
			return FilterExpression.Equal(navValue, sourceField.ExpressionContext);
		}
		NavStringValue navStringValue = navValue as NavStringValue;
		if (navStringValue == null)
		{
			throw new NavCSideException(18023644, string.Format(CultureInfo.CurrentCulture, Lang.FieldMustBeTextType, nCLMetaField.FieldName, sourceField.FieldNavType.ToString(), nCLMetaField.Parent.TableName));
		}
		if (string.IsNullOrEmpty(navStringValue.Value))
		{
			return null;
		}
		FilterExpression filterExpression = FilterExpressionParser.Parse(navStringValue.Value, sourceField.ExpressionContext);
		if ((metaFilter.FilterModes & NCLMetaFilterModes.OnlyMaxLimit) != 0)
		{
			RangeList rangeList = filterExpression.ToRangeList();
			if (rangeList.IsEmpty)
			{
				return filterExpression.TransferToDifferentContext(sourceField.ExpressionContext);
			}
		}
		return filterExpression;
	}

	internal static FilterExpression GetFlowFilterBasedFilter(NCLMetaFilterField metaFilter, FilterFieldDictionary fieldFilters)
	{
		NCLMetaField nCLMetaField = (NCLMetaField)metaFilter.ValueField;
		NCLMetaField sourceField = metaFilter.SourceField;
		if (fieldFilters == null || !fieldFilters.TryGetValue(nCLMetaField, out var value))
		{
			return null;
		}
		if ((metaFilter.FilterModes & NCLMetaFilterModes.OnlyMaxLimit) != 0)
		{
			RangeList rangeList = value.ToRangeList();
			if (rangeList.IsEmpty)
			{
				return value.TransferToDifferentContext(sourceField.ExpressionContext);
			}
			UnaryFilterExpression unaryFilterExpression = DataProviderHelper.CreateSingleUpperBoundedUnaryFilterExpressionForOnlyMaxLimitMetadataSetting(rangeList, sourceField.ExpressionContext);
			if (unaryFilterExpression == null)
			{
				throw NavCSideFilterException.CreateForNoSingleRange(18023434, CultureInfo.CurrentCulture, nCLMetaField.FieldCaption, nCLMetaField.Parent.TableCaptions.GetValueOrDefault(), value.ToString());
			}
			return unaryFilterExpression;
		}
		return value.TransferToDifferentContext(sourceField.ExpressionContext);
	}

	internal static FilterFieldDictionary GetFilterFromMetaFilterCollection(NavSession session, int companyToken, IRecordBuffer currentRecord, FiltersAndMarks filtersAndMarks, SecurityFiltering securityFiltering, NCLMetaFilterCollection filters, int recursionLevel = 0)
	{
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = null;
		foreach (NCLMetaFilter filter in filters)
		{
			bool flag = false;
			NCLMetaFilterType filterType = filter.FilterType;
			FilterExpression filterExpression;
			if ((uint)filterType <= 1u)
			{
				filterExpression = filter.Expression;
			}
			else
			{
				NCLMetaFilterField nCLMetaFilterField = filter as NCLMetaFilterField;
				flag = nCLMetaFilterField.FilterModes == NCLMetaFilterModes.None;
				NCLMetaField nCLMetaField = (NCLMetaField)nCLMetaFilterField.ValueField;
				switch (nCLMetaFilterField.ValueField.FieldClass)
				{
				case FieldClass.Normal:
					filterExpression = GetValueBasedFilter(nCLMetaFilterField, currentRecord[nCLMetaField.FieldIndex]);
					break;
				case FieldClass.FlowField:
				{
					FieldDictionary<NavValue> fieldDictionary = CalcFields(session, companyToken, currentRecord, filtersAndMarks, new NCLMetaField[1] { nCLMetaField }, onlyFieldsSourcedFromVirtualTables: false, securityFiltering, recursionLevel);
					NavValue value = fieldDictionary[nCLMetaFilterField.ValueField];
					filterExpression = GetValueBasedFilter(nCLMetaFilterField, value);
					break;
				}
				case FieldClass.FlowFilter:
					filterExpression = GetFlowFilterBasedFilter(nCLMetaFilterField, filtersAndMarks.Filters);
					break;
				default:
					throw new NotSupportedException();
				}
			}
			if (filterExpression.IsNullOrConstantTrue())
			{
				continue;
			}
			FilterExpression value2;
			if (dictionary == null)
			{
				dictionary = new Dictionary<INavFieldMetadata, FilterExpression>();
			}
			else if (dictionary.TryGetValue(filter.SourceField, out value2))
			{
				dictionary.Remove(filter.SourceField);
				if (flag)
				{
					filterExpression = FilterExpression.And(filterExpression, value2);
				}
			}
			dictionary.Add(filter.SourceField, filterExpression);
		}
		if (dictionary != null)
		{
			return new FilterFieldDictionary(dictionary);
		}
		return null;
	}

	internal static NavValue NegateValue(NavValue value, NavType fieldType)
	{
		return fieldType switch
		{
			NavType.Decimal => NavDecimal.Create(-((NavDecimal)value).Value), 
			NavType.BigInteger => NavBigInteger.Create(-((NavBigInteger)value).Value), 
			NavType.Integer => NavInteger.Create(-((NavInteger)value).Value), 
			_ => value, 
		};
	}
}
