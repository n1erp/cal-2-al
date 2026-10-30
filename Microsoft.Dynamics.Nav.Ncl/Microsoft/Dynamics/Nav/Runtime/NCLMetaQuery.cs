using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Runtime.Apps;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;
using Microsoft.Dynamics.Nav.XmlMetadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NCLMetaQuery : NCLMetaApplicationObject
{
	private delegate NavQuery CreateNavQuery(ITreeObject parent, SecurityFiltering securityFiltering, NCLMetaQuery metaQuery);

	private delegate NavQuery CreateLegacyNavQuery(ITreeObject parent, SecurityFiltering securityFiltering);

	private static readonly object syncObject = new object();

	private NCLMetaQueryDefinition queryDefinition;

	private ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>> columnFilters;

	private NCLCaptionStrings captionStrings;

	internal ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>> ColumnFilters
	{
		get
		{
			bool expressionHasVariables;
			return columnFilters ?? BuildFilterExpressionCollection(MetaQuery, out expressionHasVariables, allowExpressionEvaluation: true);
		}
		private set
		{
			columnFilters = value;
		}
	}

	internal string Caption => captionStrings.GetValueOrDefault();

	internal MetaQuery MetaQuery { get; private set; }

	internal int TopNumberOfRowsToReturn { get; private set; }

	public NCLMetaQueryDefinition QueryDefinition => queryDefinition;

	public string Name { get; private set; }

	public QueryType QueryType { get; private set; }

	public string ApiPublisher { get; private set; }

	public string ApiGroup { get; private set; }

	public ReadOnlyCollection<string> ApiVersionList { get; private set; }

	public string EntitySetName { get; private set; }

	public string EntityName { get; private set; }

	public string QueryCategory { get; private set; }

	protected override Type ApplicationObjectConstructorDelegateType => typeof(CreateNavQuery);

	protected override Type ApplicationObjectLegacyConstructorDelegateType => typeof(CreateLegacyNavQuery);

	public XmlDocument MetaQueryXml => GetMetadataFromLoader().Document;

	internal override bool CheckIsStale()
	{
		if (queryDefinition != null)
		{
			return queryDefinition.CheckIsStale();
		}
		return false;
	}

	private NCLMetaQuery(INCLMetaApplicationObjectLoader loader, int queryId, NavAppGroup metadataAppGroup, MetaQuery metaQuery = null, int appDependencyOrder = -1)
		: base(loader, ObjectType.Query, queryId, metadataAppGroup, appDependencyOrder)
	{
		MetaQuery = metaQuery;
	}

	internal static NCLMetaQuery CreateEmptyNCLMetaQuery(INCLMetaApplicationObjectLoader loader, int queryId, NavAppGroup appGroup, int appDependencyOrder = -1)
	{
		return new NCLMetaQuery(loader, queryId, appGroup, null, appDependencyOrder);
	}

	internal static NCLMetaQuery CreateEmptyNCLMetaQuery(INCLMetaApplicationObjectLoader loader, MetaQuery metaQuery, NavAppGroup appGroup = null, int appDependencyOrder = -1)
	{
		return new NCLMetaQuery(loader, metaQuery.Id, appGroup ?? NavAppGroup.BaseGroup, metaQuery, appDependencyOrder);
	}

	protected override void LoadMetadata()
	{
		base.LoadMetadata();
		try
		{
			NCLObjectXmlMetadata metadataFromLoader = GetMetadataFromLoader();
			Timestamp = metadataFromLoader.Timestamp;
			if (MetaQuery == null)
			{
				MetaQuery = new MetaQuery(metadataFromLoader.Document.DocumentElement, MetadataAppGroup.GroupId, MetadataAppGroup.GroupId);
				MetaQuery.ResolveColumnTypes(FieldTypeFromTableNoResolver);
				int languageId = NavCurrentThread.Session?.LCID ?? NavEnvironment.DefaultLanguage;
				string language = NavCurrentThread.Session?.AbbreviatedLanguageName ?? "ENU";
				MetaQuery.ApplyLanguage(language, LanguageHelper.GetAbbreviatedLanguageName(LanguageHelper.GetPrimaryLanguageId(languageId)), FieldFromTableNoResolver);
			}
			ParseMetadata(MetaQuery, allowExpressionEvaluation: false);
		}
		catch (NavMetadataNotFoundException ex)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("000028Z", Category.Runtime, ex);
			throw new NavNCLMetadataObjectNotFoundException(CultureInfo.CurrentCulture, ApplicationObjectId.ObjectType, ApplicationObjectId.ObjectNumber, ex.InnerException);
		}
	}

	internal static Func<int, MetaField> FieldFromTableNoResolver(int tableNo, int appGroupId = -1)
	{
		MetaTable metaTable = NavGlobal.MetadataProvider.GetTableMetadata(tableNo, appGroupId, requireCompiled: false);
		return FieldFromFieldNoResolver;
		MetaField FieldFromFieldNoResolver(int fieldNo)
		{
			return metaTable.GetFieldsById(fieldNo);
		}
	}

	internal static Func<int, NavType> FieldTypeFromTableNoResolver(int tableNo, int appGroupId = -1)
	{
		NCLMetaTable nclMetaTabe = NavGlobal.NCLMetadata.GetMetaTableById(tableNo, appGroupId, requireCompiled: false);
		return FieldTypeResolver;
		NavType FieldTypeResolver(int fieldNo)
		{
			if (nclMetaTabe.TryGetFieldByNo(fieldNo, out var result))
			{
				return result.FieldNavType;
			}
			return NavType.None;
		}
	}

	internal static Action<int> MetaFieldValidator(int tableNo, int appGroupId = -1)
	{
		NCLMetaTable nclMetaTabe = NavGlobal.NCLMetadata.GetMetaTableById(tableNo, appGroupId, requireCompiled: false);
		return FieldValidator;
		void FieldValidator(int fieldNo)
		{
			NCLMetaField fieldByNo = nclMetaTabe.GetFieldByNo(fieldNo);
			ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
		}
	}

	private static NCLMetaQueryDataItem CreateSubQueryForFlowFieldCalculation(NCLMetaQueryDataItem baseDataItem, MetaQueryColumn flowFieldColumn, int nextColumnId)
	{
		NCLMetaField flowField = baseDataItem.MetaTable.GetFieldByNo(flowFieldColumn.FieldNo);
		return NCLMetaQueryDataItem.CreateWithSubQuery(baseDataItem.ParentQueryDefinition, DataItemLinkType.OuterApply, (NCLMetaQueryDataItem dataItemBeingCreated) => NCLMetaQueryDefinition.Create(1, dataItemBeingCreated, (NCLMetaQueryDefinition subQueryBeingCreated) =>
		{
			DataLockState lockState = DataLockState.None;
			NCLMetaQueryDataItemCollection nCLMetaQueryDataItemCollection = SqlTableDataProviderHelper.CreateDataItemFromFlowField(subQueryBeingCreated, flowField, lockState, baseDataItem.Name, nextColumnId);
			if (nCLMetaQueryDataItemCollection == null)
			{
				List<NCLMetaFilterField> list = (from f in flowField.CalculationFormula.Filters.Where((NCLMetaFilter f) => f.FilterType == NCLMetaFilterType.Field).OfType<NCLMetaFilterField>()
					where f.SourceField.FieldNclType != f.ValueField.NclType
					select f).ToList();
				if (list.Count > 0)
				{
					NCLMetaField sourceField = list.First().SourceField;
					throw new NavCSideException(18023443, string.Format(CultureInfo.CurrentCulture, Lang.FieldTypeMismatchError, flowField.FieldName, sourceField.FieldName, flowField.Parent.TableName, sourceField.Parent.TableName, flowField.FieldNavType.ToString(), sourceField.FieldNavType.ToString()));
				}
				throw new NavNotSupportedException();
			}
			return nCLMetaQueryDataItemCollection;
		}, null), string.Format(CultureInfo.InvariantCulture, "SUB${0}${1}", baseDataItem.Name, flowField.FieldName), (NCLMetaQueryDataItem dataItemBeingCreated) =>
		{
			NavNclType fieldNclType = flowField.FieldNclType;
			if (flowFieldColumn.FieldDateMethod != DateMethodType.None && (fieldNclType == NavNclType.NavDate || fieldNclType == NavNclType.NavDateTime))
			{
				return new FieldList(NCLMetaQueryColumn.CreateWithDateMethod(dataItemBeingCreated, nextColumnId, flowFieldColumn.Name, flowFieldColumn.FieldDateMethod, flowFieldColumn.Id, flowFieldColumn.FilterOnly, flowFieldColumn.QueryColumnIndex));
			}
			bool flag = flowFieldColumn.ReverseSign;
			AggregationType aggregationType = flowFieldColumn.FieldTotalingMethod;
			if (flowField.CalculationFormula.NegateResult)
			{
				flag = !flag;
				switch (flowFieldColumn.FieldTotalingMethod)
				{
				case AggregationType.Min:
					aggregationType = AggregationType.Max;
					break;
				case AggregationType.Max:
					aggregationType = AggregationType.Min;
					break;
				}
			}
			return new FieldList(NCLMetaQueryColumn.CreateAggregate(dataItemBeingCreated, nextColumnId, flowFieldColumn.Name, aggregationType, flowFieldColumn.Id, flag, flowFieldColumn.FilterOnly, flowFieldColumn.QueryColumnIndex));
		}, null, baseDataItem.AppGroup);
	}

	internal static FiltersAndMarks CreateTableFiltersAndMarksFromDataItemFieldFilters(NCLMetaQueryDataItem dataItemBeingCreated, List<MetaQueryFieldFilter> fieldFilters, bool allowExpressionEvaluation, out bool tableFilterHasVariables)
	{
		tableFilterHasVariables = false;
		if (fieldFilters.Count == 0)
		{
			return null;
		}
		Dictionary<INavFieldMetadata, FilterExpression> dictionary = null;
		foreach (MetaQueryFieldFilter fieldFilter in fieldFilters)
		{
			NCLMetaField fieldByNo = dataItemBeingCreated.MetaTable.GetFieldByNo(fieldFilter.FieldNo);
			if (fieldByNo.FieldClass != FieldClass.Normal)
			{
				throw new NavNotSupportedException(string.Format(CultureInfo.CurrentCulture, Lang.QueryDataItemTableFilterOnFlowFieldNotSupported, fieldByNo.DescriptiveName, dataItemBeingCreated.Name));
			}
			FilterExpression filterExpression = null;
			if (fieldFilter.TypeOfFilter == FilterType.CONST)
			{
				if (fieldFilter.Value != null)
				{
					filterExpression = new UnaryFilterExpression(FilterExpressionType.Equal, ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, fieldByNo, fieldFilter.Value, 9), fieldByNo.ExpressionContext);
				}
			}
			else
			{
				filterExpression = FilterExpressionParser.Parse(fieldFilter.Value, fieldByNo.ExpressionContext, allowExpressionEvaluation, out var hasVariables);
				tableFilterHasVariables |= hasVariables;
				if (hasVariables && !allowExpressionEvaluation)
				{
					return NCLMetaQueryDataItem.FiltersAndMarksNotEvaluated;
				}
			}
			if (filterExpression != null)
			{
				if (dictionary == null)
				{
					dictionary = new Dictionary<INavFieldMetadata, FilterExpression>();
				}
				if (dictionary.TryGetValue(fieldByNo, out var value))
				{
					dictionary.Remove(fieldByNo);
					filterExpression = FilterExpression.And(value, filterExpression);
				}
				dictionary.Add(fieldByNo, filterExpression);
			}
		}
		if (dictionary != null)
		{
			return new FiltersAndMarks(new FilterFieldDictionary(dictionary));
		}
		return null;
	}

	public static ReadOnlyCollection<NCLMetaQueryDataItem> CreateDataItemsFromDesignMetadata(NCLMetaQueryDefinition parentQueryDefinition, MetaQuery metaQuery, bool allowExpressionEvaluation, out bool tableFilterHasVariables)
	{
		bool localHasVariables = false;
		DataLockState lockState = metaQuery.ReadState switch
		{
			QueryReadState.ReadExclusive => DataLockState.LockExclusive, 
			QueryReadState.ReadShared => DataLockState.Shared, 
			QueryReadState.ReadUncommitted => DataLockState.None, 
			_ => throw new ArgumentException("Unknown query ReadState value", "metaQuery"), 
		};
		int num = -1000000000;
		List<NCLMetaQueryDataItem> list = new List<NCLMetaQueryDataItem>();
		foreach (MetaQueryDataItem dataItem in metaQuery.DataItems)
		{
			NCLMetaQueryDataItem baseDataItem = NCLMetaQueryDataItem.CreateFromMetadata(parentQueryDefinition, dataItem.DataItemLinkType, dataItem.TableNo, lockState, dataItem.DataItemName, (NCLMetaQueryDataItem dataItemBeingCreated) =>
			{
				INavFieldMetadata[] fields = (from metaColumn in dataItem.QueryColumns
					where NavGlobal.NCLMetadata.GetMetaTableById(dataItemBeingCreated.TableNo, metaQuery.MetadataAppGroupId, requireCompiled: false).GetFieldByNo(metaColumn.FieldNo).FieldClass != FieldClass.FlowField
					select NCLMetaQueryColumn.CreateFromDesignMetadata(dataItemBeingCreated, metaColumn)).ToArray();
				return new FieldList(fields);
			}, (NCLMetaQueryDataItem dataItemBeingCreated) =>
			{
				FiltersAndMarks result = CreateTableFiltersAndMarksFromDataItemFieldFilters(dataItemBeingCreated, dataItem.FieldFilters, allowExpressionEvaluation, out var tableFilterHasVariables2);
				localHasVariables |= tableFilterHasVariables2;
				return result;
			}, (NCLMetaQueryDataItem dataItemBeingCreated) => (dataItem.DataItemLinks.Count == 0) ? null : new ReadOnlyCollection<NCLMetaDataItemLink>(dataItem.DataItemLinks.Select((MetaQueryDataItemLink metaDataItemLink) => NCLMetaDataItemLink.CreateFieldEqualsField(dataItemBeingCreated, metaDataItemLink.SourceDataItemName, metaDataItemLink.SourceFieldNo, metaDataItemLink.DestinationFieldNo, NCLMetaFilterModes.None)).ToArray()), dataItem.FieldFilters, metaQuery.MetadataAppGroupId);
			list.Add(baseDataItem);
			foreach (MetaQueryColumn item in dataItem.QueryColumns.Where((MetaQueryColumn column) => NavGlobal.NCLMetadata.GetMetaTableById(baseDataItem.TableNo, metaQuery.MetadataAppGroupId, requireCompiled: false).GetFieldByNo(column.FieldNo).FieldClass == FieldClass.FlowField))
			{
				list.Add(CreateSubQueryForFlowFieldCalculation(baseDataItem, item, num));
				num++;
			}
		}
		tableFilterHasVariables = localHasVariables;
		return new ReadOnlyCollection<NCLMetaQueryDataItem>(list);
	}

	private void ParseMetadata(MetaQuery metaQuery, bool allowExpressionEvaluation)
	{
		lock (syncObject)
		{
			bool localHasVariables = false;
			queryDefinition = NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryDefinitionBeingCreated) =>
			{
				NCLMetaQueryDataItemCollection result = NCLMetaQueryDataItemCollection.Create(CreateDataItemsFromDesignMetadata(queryDefinitionBeingCreated, metaQuery, allowExpressionEvaluation, out var tableFilterHasVariables));
				localHasVariables |= tableFilterHasVariables;
				return result;
			}, (NCLMetaQueryDefinition queryDefinitionBeingCreated) => (metaQuery.OrderBys.Count == 0) ? null : new ReadOnlyCollection<NCLMetaQueryOrderBy>(metaQuery.OrderBys.Select((MetaQueryOrderBy metaQueryOrderBy) => NCLMetaQueryOrderBy.CreateMetaOrderByFromDesignMetadata(queryDefinitionBeingCreated, metaQueryOrderBy)).ToArray()));
			ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>> readOnlyCollection = BuildFilterExpressionCollection(metaQuery, out var expressionHasVariables, allowExpressionEvaluation);
			localHasVariables |= expressionHasVariables;
			if (!localHasVariables)
			{
				ColumnFilters = readOnlyCollection;
			}
			captionStrings = NCLCaptionStrings.CreateNCLCaptionStrings(metaQuery.CaptionML ?? string.Empty, metaQuery.Name);
			Name = metaQuery.Name;
			ApiPublisher = metaQuery.APIPublisher;
			ApiGroup = metaQuery.APIGroup;
			ApiVersionList = WebServiceHelper.ParseApiVersion(metaQuery.APIVersion, Name, ObjectId, ApiPublisher, ApiGroup);
			QueryType = metaQuery.QueryType;
			TopNumberOfRowsToReturn = metaQuery.TopNumberOfRowsToReturn;
			EntitySetName = metaQuery.EntitySetName;
			EntityName = metaQuery.EntityName;
			QueryCategory = metaQuery.QueryCategory;
		}
	}

	private ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>> BuildFilterExpressionCollection(MetaQuery metaQuery, out bool expressionHasVariables, bool allowExpressionEvaluation)
	{
		bool localHasVariables = false;
		ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>> result = new ReadOnlyCollection<Tuple<NCLMetaQueryColumn, FilterExpression>>(metaQuery.ColumnFilters.Select((MetaQueryColumnFilter columnFilter) =>
		{
			NCLMetaQueryColumn columnByNo = queryDefinition.GetColumnByNo(columnFilter.QueryColumnId);
			FilterExpression item;
			switch (columnFilter.TypeOfFilter)
			{
			case FilterType.CONST:
			{
				NavValue value = (((object)columnFilter.Value == "''") ? columnByNo.EmptyValue : ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, columnByNo, columnFilter.Value, 9));
				item = new UnaryFilterExpression(FilterExpressionType.Equal, value, columnByNo.ExpressionContext);
				break;
			}
			case FilterType.FILTER:
			{
				item = FilterExpressionParser.Parse(columnFilter.Value, columnByNo.ExpressionContext, allowExpressionEvaluation, out var hasVariables);
				localHasVariables |= hasVariables;
				break;
			}
			default:
				throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "Invalid filter type {0}", columnFilter.TypeOfFilter));
			}
			return new Tuple<NCLMetaQueryColumn, FilterExpression>(columnByNo, item);
		}).ToArray());
		expressionHasVariables = localHasVariables;
		return result;
	}

	public NavQuery CreateObjectInstance(ITreeObject parent)
	{
		return CreateObjectInstance(parent, SecurityFiltering.Filtered);
	}

	public NavQuery CreateObjectInstance(ITreeObject parent, SecurityFiltering securityFiltering)
	{
		Delegate obj = ApplicationObjectConstructor;
		if (!ApplicationObjectLegacyConstructor)
		{
			return ((CreateNavQuery)obj)(parent, securityFiltering, this);
		}
		return ((CreateLegacyNavQuery)obj)(parent, securityFiltering);
	}
}
