using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

internal class SqlTableDataProviderHelper
{
	internal static ReadOnlyCollection<NCLMetaQueryOrderBy> CreateOrderByListFromFieldList(NCLMetaQueryDataItem parentDataItem, SortingFieldList sortOrderFieldList)
	{
		return new ReadOnlyCollection<NCLMetaQueryOrderBy>(sortOrderFieldList.Select((INavSortingField sortingField) =>
		{
			NCLMetaField nCLMetaField = (NCLMetaField)sortingField.Field;
			try
			{
				return NCLMetaQueryOrderBy.Create(sortingField.Order, parentDataItem.ParentQueryDefinition.GetColumnByNo(nCLMetaField.FieldNo), extraColumnAutomaticallyAddedForUniqueness: false);
			}
			catch (NavNCLColumnNotFoundException)
			{
				throw RecordImplementationHelper.GetCannotSortOnCertainFieldsException(nCLMetaField.Parent, new NCLMetaField[1] { nCLMetaField }, Lang.CannotSortOnCertainFields);
			}
		}).ToArray());
	}

	internal static ReadOnlyRecordBuffer CreateTableBasedRecordBufferFromQueryResult(NCLMetaQueryDataItem dataItem, ReadOnlyRecordBuffer queryBuffer)
	{
		NCLMetaTable metaTable = dataItem.MetaTable;
		NavValue[] array = new NavValue[metaTable.FieldCount];
		Array.Copy(queryBuffer.ToArray(), array, queryBuffer.FieldCount);
		return new ReadOnlyRecordBuffer(metaTable, array);
	}

	internal static NCLMetaQueryColumn CreateQueryColumnFromFlowField(NCLMetaQueryDataItem parentDataItem, NCLMetaField flowField, int columnId)
	{
		FlowFieldsHelper.CheckFlowFieldProperties(flowField);
		string name = string.Empty;
		AggregationType aggregationType = AggregationType.None;
		switch (flowField.CalculationFormula.CalculationMethod)
		{
		case NCLMetaCalculationMethod.Average:
			aggregationType = AggregationType.Average;
			name = string.Format(CultureInfo.InvariantCulture, "{0}$AVG${1}", parentDataItem.Name, flowField.SourceTableField.FieldName);
			break;
		case NCLMetaCalculationMethod.Count:
			return NCLMetaQueryColumn.CreateCount(parentDataItem, "$CNT", columnId);
		case NCLMetaCalculationMethod.Sum:
			aggregationType = AggregationType.Sum;
			name = string.Format(CultureInfo.InvariantCulture, "{0}$SUM${1}", parentDataItem.Name, flowField.SourceTableField.FieldName);
			break;
		case NCLMetaCalculationMethod.Min:
			aggregationType = AggregationType.Min;
			name = string.Format(CultureInfo.InvariantCulture, "{0}$MIN${1}", parentDataItem.Name, flowField.SourceTableField.FieldName);
			break;
		case NCLMetaCalculationMethod.Max:
			aggregationType = AggregationType.Max;
			name = string.Format(CultureInfo.InvariantCulture, "{0}$MAX${1}", parentDataItem.Name, flowField.SourceTableField.FieldName);
			break;
		case NCLMetaCalculationMethod.Exists:
			return NCLMetaQueryColumn.CreateConstValue(parentDataItem, string.Format(CultureInfo.InvariantCulture, "{0}$EXISTS", parentDataItem.Name), NavType.Boolean, 0, columnId, NavBoolean.Create(value: true));
		case NCLMetaCalculationMethod.Lookup:
			name = string.Format(CultureInfo.InvariantCulture, "{0}$LOOKUP${1}", parentDataItem.Name, flowField.SourceTableField.FieldName);
			return NCLMetaQueryColumn.Create(parentDataItem, flowField.SourceTableField.FieldNo, flowField.FieldNavType, flowField.FieldDefinedLength, name, columnId);
		}
		return NCLMetaQueryColumn.CreateAggregate(parentDataItem, flowField.SourceTableField.FieldNo, name, aggregationType, flowField.FieldNavType, flowField.FieldDefinedLength, columnId, reverseSign: false);
	}

	private static ReadOnlyCollection<NCLMetaDataItemLink> CreateJoinCriteriaFromFlowField(NCLMetaQueryDataItem dataItemBeingCreated, string flowFieldTableDataItemName, NCLMetaField flowField)
	{
		List<NCLMetaDataItemLink> list = new List<NCLMetaDataItemLink>();
		foreach (NCLMetaFilter filter in flowField.CalculationFormula.Filters)
		{
			switch (filter.FilterType)
			{
			case NCLMetaFilterType.Const:
			{
				NCLMetaFilterConst nCLMetaFilterConst = filter as NCLMetaFilterConst;
				list.Add(NCLMetaDataItemLink.CreateConst(dataItemBeingCreated, nCLMetaFilterConst.SourceFieldId, nCLMetaFilterConst.Value));
				break;
			}
			case NCLMetaFilterType.Expression:
			{
				NCLMetaFilterExpression nCLMetaFilterExpression = filter as NCLMetaFilterExpression;
				list.Add(NCLMetaDataItemLink.CreateExpression(dataItemBeingCreated, nCLMetaFilterExpression.SourceFieldId, nCLMetaFilterExpression.Expression));
				break;
			}
			case NCLMetaFilterType.Field:
			{
				NCLMetaFilterField nCLMetaFilterField = filter as NCLMetaFilterField;
				if (flowField.CalculationFormula.NonstandardJoinCriteriaFields != null && flowField.CalculationFormula.NonstandardJoinCriteriaFields.Contains(nCLMetaFilterField.ValueField))
				{
					break;
				}
				if (nCLMetaFilterField.SourceField.FieldNclType == NavNclType.NavCode || nCLMetaFilterField.SourceField.FieldNclType == NavNclType.NavText)
				{
					if (nCLMetaFilterField.ValueField.NclType != NavNclType.NavCode && nCLMetaFilterField.ValueField.NclType != NavNclType.NavText)
					{
						return null;
					}
					if (nCLMetaFilterField.ValueField.FieldClass != FieldClass.FlowFilter)
					{
						SqlDataType sqlDataType = ((nCLMetaFilterField.SourceField.SqlDataType != SqlDataType.Default) ? nCLMetaFilterField.SourceField.SqlDataType : SqlDataType.VarChar);
						SqlDataType sqlDataType2 = ((nCLMetaFilterField.ValueField.SqlDataType != SqlDataType.Default) ? nCLMetaFilterField.ValueField.SqlDataType : SqlDataType.VarChar);
						if (sqlDataType != sqlDataType2)
						{
							SendTraceTagJoinNotPossible(nCLMetaFilterField, sqlDataType, sqlDataType2);
							return null;
						}
					}
				}
				else if (nCLMetaFilterField.SourceField.FieldNclType != nCLMetaFilterField.ValueField.NclType || (nCLMetaFilterField.ValueField.FieldClass != FieldClass.FlowFilter && nCLMetaFilterField.SourceField.SqlDataType != nCLMetaFilterField.ValueField.SqlDataType))
				{
					return null;
				}
				list.Add(NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, flowFieldTableDataItemName, nCLMetaFilterField.ValueFieldId, nCLMetaFilterField.SourceFieldId, nCLMetaFilterField.FilterModes));
				break;
			}
			}
		}
		return new ReadOnlyCollection<NCLMetaDataItemLink>(list.ToArray());
	}

	private static void SendTraceTagJoinNotPossible(NCLMetaFilterField metaFilterField, SqlDataType sourceFieldSqlDataType, SqlDataType valueFieldSqlDataType)
	{
		string arg = $"{metaFilterField.SourceField.DescriptiveParentName} ({metaFilterField.SourceField.Parent?.ApplicationObjectId.ObjectNumber}) - {metaFilterField.SourceField.DescriptiveName} - {sourceFieldSqlDataType.ToString()}";
		string arg2 = $"{metaFilterField.ValueField.DescriptiveParentName} ({((NCLMetaField)metaFilterField.ValueField).Parent?.ApplicationObjectId.ObjectNumber}) - {metaFilterField.ValueField.DescriptiveName} - {valueFieldSqlDataType.ToString()}";
		string message = $"Could not join FlowField due to incompatible Sql data types from: \n{arg} \nto \n{arg2}";
		NavDiagnostics.GlobalInstance?.SendTraceTag("0000BQ8", Category.Runtime, Verbosity.Warning, PrivacyClassification.SystemMetadata, message);
	}

	public static NCLMetaQueryDataItemCollection CreateDataItemFromFlowField(NCLMetaQueryDefinition parentQueryDefinition, NCLMetaField flowField, DataLockState lockState, string flowFieldTableDataItemName, int nextColumnId)
	{
		NCLMetaTable parent = flowField.SourceTableField.Parent;
		if (parent.IsVirtualTable || parent.IsAppTable != flowField.Parent.IsAppTable || parent.TableType != TableType.Normal)
		{
			return null;
		}
		if (flowField.CalculationFormula.Filters.Any((NCLMetaFilter filter) => filter.SourceField.FieldClass == FieldClass.FlowField || (filter.FilterType == NCLMetaFilterType.Field && ((NCLMetaFilterField)filter).ValueField.FieldClass == FieldClass.FlowField)))
		{
			return null;
		}
		NCLMetaField sourceField = flowField.CalculationFormula.SourceField;
		NCLMetaQueryDataItem nCLMetaQueryDataItem = NCLMetaQueryDataItem.Create(parentQueryDefinition, sourceField.Parent, lockState, string.Format(CultureInfo.InvariantCulture, "{0}${1}", flowField.FieldName, sourceField.Parent.TableName), (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(CreateQueryColumnFromFlowField(dataItemBeingCreated, flowField, nextColumnId)), (NCLMetaQueryDataItem dataItemBeingCreated) => (FiltersAndMarks)null, (NCLMetaQueryDataItem dataItemBeingCreated) => CreateJoinCriteriaFromFlowField(dataItemBeingCreated, flowFieldTableDataItemName, flowField));
		if (nCLMetaQueryDataItem.DataItemLinks == null)
		{
			return null;
		}
		return NCLMetaQueryDataItemCollection.Create(nCLMetaQueryDataItem);
	}

	public static NCLMetaQueryDefinition BuildQueryDefinitionForAutoCalculationOfFlowFields(NavSession session, NCLMetaTable table, FiltersAndMarks tableFiltersAndMarks, int companyToken, DataLockState lockState, SortingFieldList sortingFields, IEnumerable<Tuple<NCLMetaField, bool>> flowFieldsToCalculate, out Tuple<NCLMetaField, bool>[] flowFieldsWithNonStandardJoinCondition, out Tuple<NCLMetaField, bool>[] nonCalculableFlowFields, out Tuple<NCLMetaField, bool>[] flowFieldsBeingCalculatedByQuery, bool checkSecurityFilterExistance)
	{
		List<Tuple<NCLMetaField, bool>> flowFieldsWithNonstandardJoinCriteria = new List<Tuple<NCLMetaField, bool>>();
		List<Tuple<NCLMetaField, bool>> nonCalculable = new List<Tuple<NCLMetaField, bool>>();
		List<Tuple<NCLMetaField, bool>> beingCalculated = new List<Tuple<NCLMetaField, bool>>();
		int nextColumnId = -1000000000;
		NCLMetaQueryDefinition nCLMetaQueryDefinition = NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(() =>
		{
			List<NCLMetaQueryDataItem> list = new List<NCLMetaQueryDataItem> { NCLMetaQueryDataItem.Create(queryBeingCreated, table, lockState, table.TableName, (NCLMetaQueryDataItem dataItemBeingCreated) => NCLMetaQueryDataItem.TableBasedColumnLayout, (NCLMetaQueryDataItem dataitemBeingCreated) => tableFiltersAndMarks, null) };
			foreach (Tuple<NCLMetaField, bool> flowFieldTuple in flowFieldsToCalculate)
			{
				NCLMetaField flowFieldToAutoCalculate = flowFieldTuple.Item1;
				if (checkSecurityFilterExistance && session.GetSecurityFilters(companyToken, flowFieldToAutoCalculate.CalculationFormula.TableId, SecurityFilterType.Read, NavCurrentThread.Session.CurrentMethodScope.ApplicationObject, null) != FilterFieldDictionary.Empty)
				{
					break;
				}
				NCLMetaQueryDataItem nCLMetaQueryDataItem = NCLMetaQueryDataItem.CreateWithSubQuery(queryBeingCreated, DataItemLinkType.OuterApply, (NCLMetaQueryDataItem dataItemBeingCreated) => NCLMetaQueryDefinition.Create(1, dataItemBeingCreated, (NCLMetaQueryDefinition subQueryBeingCreated) =>
				{
					DataAccess dataAccessForTable = session.DataAccessSource.GetDataAccessForTable(table);
					DataLockState tableLockState = dataAccessForTable.GetTableLockState(table.TableId, companyToken);
					return CreateDataItemFromFlowField(subQueryBeingCreated, flowFieldToAutoCalculate, tableLockState, table.TableName, nextColumnId);
				}, null), "SUB$" + flowFieldToAutoCalculate.FieldName, (NCLMetaQueryDataItem dataItemBeingCreated) => new FieldList(NCLMetaQueryColumn.Create(dataItemBeingCreated, nextColumnId, flowFieldToAutoCalculate.FieldName, flowFieldToAutoCalculate.FieldNo, !flowFieldTuple.Item2, flowFieldToAutoCalculate.ColumnIndex, flowFieldToAutoCalculate.CalculationFormula.NegateResult)), null);
				if (nCLMetaQueryDataItem.SubQueryDefinition.DataItems != null)
				{
					list.Add(nCLMetaQueryDataItem);
					if (flowFieldTuple.Item1.CalculationFormula.NonstandardJoinCriteriaFields != null)
					{
						flowFieldsWithNonstandardJoinCriteria.Add(flowFieldTuple);
					}
					beingCalculated.Add(flowFieldTuple);
				}
				else
				{
					nonCalculable.Add(flowFieldTuple);
				}
				int num = nextColumnId;
				nextColumnId = num + 1;
			}
			return list;
		}), (NCLMetaQueryDefinition queryBeingCreated) => CreateOrderByListFromFieldList(queryBeingCreated.DataItems[0], sortingFields));
		if (nCLMetaQueryDefinition.DataItems.Count <= 1)
		{
			flowFieldsWithNonStandardJoinCondition = null;
			nonCalculableFlowFields = null;
			flowFieldsBeingCalculatedByQuery = null;
			return null;
		}
		flowFieldsWithNonStandardJoinCondition = ((flowFieldsWithNonstandardJoinCriteria.Count == 0) ? null : flowFieldsWithNonstandardJoinCriteria.ToArray());
		nonCalculableFlowFields = ((nonCalculable.Count == 0) ? null : nonCalculable.ToArray());
		flowFieldsBeingCalculatedByQuery = ((beingCalculated.Count == 0) ? null : beingCalculated.ToArray());
		return nCLMetaQueryDefinition;
	}

	public static Tuple<NCLMetaField, bool>[] GetFieldsToAutoCalculate(PositionedFindProviderRequest request)
	{
		IEnumerable<INavFieldMetadata> first = (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks) ? (from filterTuple in request.FiltersAndMarks.Filters.Items
			select filterTuple.Item1 into field
			where field.FieldClass == FieldClass.FlowField
			select field) : Enumerable.Empty<INavFieldMetadata>());
		IEnumerable<INavFieldMetadata> enumerable = ((request.AutoCalcFields != null) ? request.AutoCalcFields.Where((INavFieldMetadata field) => field.FieldClass == FieldClass.FlowField) : Enumerable.Empty<INavFieldMetadata>());
		IEnumerable<INavFieldMetadata> second = ((request.SortingFields != null) ? (from field in request.SortingFields
			where field.Field.FieldClass == FieldClass.FlowField
			select field.Field) : Enumerable.Empty<INavFieldMetadata>());
		IEnumerable<INavFieldMetadata> source = first.Union(enumerable).Union(second);
		IEnumerable<INavFieldMetadata> includedInTheRecordbufferFields = enumerable.Union(second);
		return (from field in source
			where ((NCLMetaField)field).FieldActive
			select Tuple.Create((NCLMetaField)field, includedInTheRecordbufferFields.Contains(field))).ToArray();
	}

	private static bool CalculateAndValidateFlowField(NavSession session, PositionedFindProviderRequest request, ReadOnlyRecordBuffer inputBuffer, NCLMetaField flowField, out NavValue calculatedValue)
	{
		calculatedValue = null;
		FieldDictionary<NavValue> fieldDictionary = FlowFieldsHelper.CalcFields(session, request.CompanyToken, inputBuffer, request.FiltersAndMarks, new NCLMetaField[1] { flowField }, onlyFieldsSourcedFromVirtualTables: false, request.FlowFieldSecurityFiltering);
		calculatedValue = fieldDictionary[flowField];
		if (FlowFieldsHelper.AnyFiltersOnFlowFields(request.FiltersAndMarks) && request.FiltersAndMarks.Filters.TryGetValue(flowField, out var value) && !value.Evaluate(calculatedValue ?? flowField.InitValue))
		{
			return false;
		}
		return true;
	}

	public static ReadOnlyRecordBuffer ValidateFiltersAndValuesForAutoCalculatedFlowFields(NavSession session, PositionedFindProviderRequest request, ReadOnlyRecordBuffer inputBuffer, Tuple<NCLMetaField, bool>[] flowFields, bool calculateDependingOnNonstandardJoinCriteriaCondition)
	{
		NavValue[] array = null;
		foreach (Tuple<NCLMetaField, bool> tuple in flowFields)
		{
			NCLMetaField item = tuple.Item1;
			bool item2 = tuple.Item2;
			bool flag = true;
			if (calculateDependingOnNonstandardJoinCriteriaCondition)
			{
				flag = item.CalculationFormula.NonstandardJoinCriteriaFields.Any((NCLMetaField field) => inputBuffer[field.ColumnIndex] != null && !inputBuffer[field.ColumnIndex].IsZeroOrEmpty);
			}
			if (!flag)
			{
				continue;
			}
			if (!CalculateAndValidateFlowField(session, request, inputBuffer, item, out var calculatedValue))
			{
				return null;
			}
			if (item2)
			{
				if (array == null)
				{
					array = inputBuffer.ToArray();
				}
				array[item.FieldIndex] = calculatedValue;
			}
		}
		if (array == null)
		{
			return inputBuffer;
		}
		return new ReadOnlyRecordBuffer(inputBuffer.MetaApplicationObject, array);
	}

	public static FiltersAndMarks BuildQueryFiltersFromFilteredFlowFields(NCLMetaQueryDefinition queryDefinition, Tuple<NCLMetaField, bool>[] flowFieldsBeingCalculatedByQuery, FilterFieldDictionary filters)
	{
		if (filters == null)
		{
			return null;
		}
		List<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		foreach (Tuple<INavFieldMetadata, FilterExpression> item in filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> filterTuple) => filterTuple.Item1.FieldClass == FieldClass.FlowFilter))
		{
			NCLMetaField nCLMetaField = (NCLMetaField)item.Item1;
			NCLMetaQueryColumn columnByNo = queryDefinition.GetColumnByNo(nCLMetaField.FieldNo);
			list.Add(new Tuple<INavFieldMetadata, FilterExpression>(columnByNo, item.Item2.TransferToDifferentContext(columnByNo.ExpressionContext)));
		}
		int num = 1;
		foreach (NCLMetaField item2 in flowFieldsBeingCalculatedByQuery.Select((Tuple<NCLMetaField, bool> tuple) => tuple.Item1))
		{
			if (!filters.TryGetValue(item2, out var value))
			{
				continue;
			}
			INavFieldMetadata navFieldMetadata = queryDefinition.DataItems[num].DefinedColumns[0];
			FilterExpression filterExpression = value.TransferToDifferentContext(navFieldMetadata.ExpressionContext);
			if (item2.CalculationFormula.NonstandardJoinCriteriaFields != null)
			{
				foreach (NCLMetaField nonstandardJoinCriteriaField in item2.CalculationFormula.NonstandardJoinCriteriaFields)
				{
					NCLMetaQueryColumn columnByNo2 = queryDefinition.GetColumnByNo(nonstandardJoinCriteriaField.FieldNo);
					filterExpression = FilterExpression.Or(filterExpression, FilterExpression.NotEqual(columnByNo2.InitValue, columnByNo2.ExpressionContext));
				}
			}
			list.Add(Tuple.Create(navFieldMetadata, filterExpression));
			num++;
		}
		return new FiltersAndMarks(new FilterFieldDictionary(list));
	}

	public static NCLMetaQueryDefinition BuildSimpleTableBasedQueryDefinition(NCLMetaTable table, FiltersAndMarks tableFiltersAndMarks, DataLockState lockState, bool firstOnly, SortingFieldList sortingFields)
	{
		return NCLMetaQueryDefinition.Create(firstOnly ? 1 : 0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, table, lockState, table.TableName, (NCLMetaQueryDataItem dataItemBeingCreated) => NCLMetaQueryDataItem.TableBasedColumnLayout, (NCLMetaQueryDataItem dataItemBeingCreated) => tableFiltersAndMarks), (NCLMetaQueryDefinition queryBeingCreated) => CreateOrderByListFromFieldList(queryBeingCreated.DataItems[0], sortingFields));
	}
}
