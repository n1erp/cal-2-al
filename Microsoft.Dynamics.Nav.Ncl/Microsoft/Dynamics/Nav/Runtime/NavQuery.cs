using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Media;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavQuery : NavApplicationObjectBase, ITreeSharedObject, ITreeObject
{
	internal enum OutputType
	{
		Xml,
		Excel,
		Csv
	}

	private const string FieldExpressionSeparator = ": ";

	private const string FilterClauseSeparator = ", ";

	private ResultSetEnumerator currentRowEnumerator;

	private MutableRecordBuffer currentDataRow;

	private int currentRowCount;

	private int skipNumberOfRows;

	private int topNumberOfRowsToReturn;

	private bool checkPermissionsOnReadNextRow;

	private bool inOnBeforeOpenTrigger;

	private SecurityFiltering securityFiltering;

	private bool metadataChanged;

	public SecurityFiltering ALSecurityFiltering
	{
		get
		{
			return securityFiltering;
		}
		set
		{
			if (value == SecurityFiltering.Validated)
			{
				throw new NavNCLArgumentException(Lang.IllegalSecurityFilteringOnQuery);
			}
			securityFiltering = value;
			if (currentRowEnumerator != null)
			{
				metadataChanged = true;
			}
		}
	}

	public string ALGetFilters => GetFilters();

	public int ALSkipNumberOfRows
	{
		get
		{
			return skipNumberOfRows;
		}
		set
		{
			if (value < 0)
			{
				throw new NavQuerySkipValueNegativeException(Lang.SkipValueNegative);
			}
			if (value > 0 && NCLMetaQuery.QueryDefinition.OrderBy.Count <= 0)
			{
				throw new NavNCLArgumentException(Lang.SkipValueWithNoOrderyByNotSupported);
			}
			skipNumberOfRows = value;
			if (currentRowEnumerator != null)
			{
				metadataChanged = true;
			}
		}
	}

	public int ALTopNumberOfRowsToReturn
	{
		get
		{
			return topNumberOfRowsToReturn;
		}
		set
		{
			if (value < 0)
			{
				throw new NavQueryTopValueNegativeException(Lang.TopValueNegative);
			}
			topNumberOfRowsToReturn = value;
			if (currentRowEnumerator != null)
			{
				metadataChanged = true;
			}
		}
	}

	internal FilterFieldDictionary Filters { get; private set; }

	internal bool IsOpen => currentRowEnumerator != null;

	public bool ClientCanceled { get; set; }

	public NCLMetaQuery NCLMetaQuery { get; private set; }

	protected bool ViewerMustBeClosedOnQuit { get; set; }

	public bool Paging { get; set; }

	public Guid ExecutionGuid { get; protected set; }

	public IResultSetProcessor ResultSetProcessor { get; protected set; }

	public override NavNclType NclType => NavNclType.NavQuery;

	public override Type HandleType => typeof(NavQueryHandle);

	internal override NCLMetaApplicationObject MetaApplicationObject => NCLMetaQuery;

	internal MutableRecordBuffer CurrentDataRow => currentDataRow;

	public string Caption => GetCaption(withPrefix: false);

	public override string ObjectName => NCLMetaQuery.Name;

	public static bool ALSaveAsXml(DataError errorLevel, int queryId, string fileName)
	{
		return SaveToFile(OutputType.Xml, errorLevel, queryId, fileName, null, fixedFormat: false, null);
	}

	public static bool ALSaveAsXml(DataError errorLevel, int queryId, NavOutStream outStream)
	{
		return SaveToStream(OutputType.Xml, errorLevel, queryId, outStream, null, fixedFormat: false, null);
	}

	public static bool ALSaveAsExcel(DataError errorLevel, int queryId, string fileName)
	{
		return SaveToFile(OutputType.Xml, errorLevel, queryId, fileName, null, fixedFormat: false, null);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, string fileName)
	{
		return SaveToFile(OutputType.Csv, errorLevel, queryId, fileName, ",", fixedFormat: false, null);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, string fileName, int fixedFormat)
	{
		bool flag = fixedFormat == 0;
		if (flag && errorLevel == DataError.TrapError)
		{
			return false;
		}
		return SaveToFile(OutputType.Csv, errorLevel, queryId, fileName, ",", flag, null);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, string fileName, int fixedFormat, string formatArgument)
	{
		bool flag = fixedFormat == 0;
		string delimiter = (flag ? null : formatArgument);
		int[] positions = (flag ? StringToIntArray(errorLevel, formatArgument) : null);
		return SaveToFile(OutputType.Csv, errorLevel, queryId, fileName, delimiter, flag, positions);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, NavOutStream outStream)
	{
		return SaveToStream(OutputType.Csv, errorLevel, queryId, outStream, ",", fixedFormat: false, null);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, NavOutStream outStream, int fixedFormat)
	{
		bool flag = fixedFormat == 0;
		if (flag && errorLevel == DataError.TrapError)
		{
			return false;
		}
		return SaveToStream(OutputType.Csv, errorLevel, queryId, outStream, ",", flag, null);
	}

	public static bool ALSaveAsCsv(DataError errorLevel, int queryId, NavOutStream outStream, int fixedFormat, string formatArgument)
	{
		bool flag = fixedFormat == 0;
		string delimiter = (flag ? null : formatArgument);
		int[] positions = (flag ? StringToIntArray(errorLevel, formatArgument) : null);
		return SaveToStream(OutputType.Csv, errorLevel, queryId, outStream, delimiter, flag, positions);
	}

	public bool ALSaveAsXml(DataError errorLevel, string fileName)
	{
		return SaveToFile(OutputType.Xml, errorLevel, fileName, null, fixedFormat: false, null);
	}

	public bool ALSaveAsXml(DataError errorLevel, NavOutStream outStream)
	{
		return SaveToStream(this, OutputType.Xml, errorLevel, outStream, null, fixedFormat: false, null);
	}

	public bool ALSaveAsExcel(DataError errorLevel, string fileName)
	{
		return SaveToFile(OutputType.Excel, errorLevel, fileName, null, fixedFormat: false, null);
	}

	public bool ALSaveAsCsv(DataError errorLevel, string fileName)
	{
		return SaveToFile(OutputType.Csv, errorLevel, fileName, ",", fixedFormat: false, null);
	}

	public bool ALSaveAsCsv(DataError errorLevel, string fileName, int fixedFormat)
	{
		bool flag = fixedFormat == 0;
		if (flag && errorLevel == DataError.TrapError)
		{
			return false;
		}
		return SaveToFile(OutputType.Csv, errorLevel, fileName, ",", flag, null);
	}

	public bool ALSaveAsCsv(DataError errorLevel, string fileName, int fixedFormat, string formatArgument)
	{
		bool flag = fixedFormat == 0;
		string delimiter = (flag ? null : formatArgument);
		int[] positions = (flag ? StringToIntArray(errorLevel, formatArgument) : null);
		return SaveToFile(OutputType.Csv, errorLevel, fileName, delimiter, flag, positions);
	}

	public bool ALSaveAsCsv(DataError errorLevel, NavOutStream outStream)
	{
		return SaveToStream(this, OutputType.Csv, errorLevel, outStream, ",", fixedFormat: false, null);
	}

	public bool ALSaveAsCsv(DataError errorLevel, NavOutStream outStream, int fixedFormat)
	{
		bool flag = fixedFormat == 0;
		if (flag && errorLevel == DataError.TrapError)
		{
			return false;
		}
		return SaveToStream(this, OutputType.Csv, errorLevel, outStream, ",", flag, null);
	}

	public bool ALSaveAsCsv(DataError errorLevel, NavOutStream outStream, int fixedFormat, string formatArgument)
	{
		bool flag = fixedFormat == 0;
		string delimiter = (flag ? null : formatArgument);
		int[] positions = (flag ? StringToIntArray(errorLevel, formatArgument) : null);
		return SaveToStream(this, OutputType.Csv, errorLevel, outStream, delimiter, flag, positions);
	}

	public string ALColumnName(int columnNo)
	{
		return NCLMetaQuery.QueryDefinition.GetColumnByNo(columnNo).Name;
	}

	public string ALColumnCaption(int columnNo)
	{
		return NCLMetaQuery.QueryDefinition.GetColumnByNo(columnNo).Caption;
	}

	public int ALColumnNo(int columnNo)
	{
		return columnNo;
	}

	public void ALSetFilter(int columnNo, string expression, params NavValue[] values)
	{
		SetFilter(NCLMetaQuery.QueryDefinition.GetColumnByNo(columnNo), expression, values);
	}

	public void ALSetRangeSafe(int columnNo, NavType expectedType)
	{
		ALSetRangeSafe(columnNo, expectedType, null, null);
	}

	public void ALSetRangeSafe(int columnNo, NavType expectedType, NavValue fromValue)
	{
		ALSetRangeSafe(columnNo, expectedType, fromValue, null);
	}

	public void ALSetRangeSafe(int columnNo, NavType expectedType, NavValue fromValue, NavValue toValue)
	{
		SetRange(ValidateExpectedType(columnNo, expectedType), fromValue, toValue);
	}

	public string ALGetFilter(int columnNo)
	{
		return GetFilter(NCLMetaQuery.QueryDefinition.GetColumnByNo(columnNo));
	}

	public bool ALOpen(DataError errorLevel)
	{
		try
		{
			Open();
			return true;
		}
		catch (NavNCLDialogException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		catch (NavALException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	public bool ALRead(DataError errorLevel)
	{
		CheckMetadataHasNotChanged();
		if (currentRowEnumerator == null && errorLevel == DataError.ThrowError)
		{
			throw new NavQueryNoDataSetException(Lang.NoDataSetDefined);
		}
		bool nextRow = GetNextRow();
		if (!nextRow && errorLevel == DataError.ThrowError)
		{
			throw new NavQueryNoDataSetException(Lang.QueryReadFailed);
		}
		return nextRow;
	}

	public void ALClose()
	{
		if (inOnBeforeOpenTrigger)
		{
			throw new NavQueryIllegalCallInTriggerException(Lang.IllegalCallInTrigger);
		}
		Close();
	}

	protected NavQuery(ITreeObject parent, int objectId, SecurityFiltering securityFiltering)
		: base(parent, new ApplicationObjectId(ObjectType.Query, objectId))
	{
		this.securityFiltering = securityFiltering;
		NCLMetaQuery = Tree.Session.NCLMetadata.GetMetaQueryById(ObjectId.ObjectNumber, OriginalAppGroupId);
		NCLMetaQuery.MetaQuery.ValidateColumns(NCLMetaQuery.MetaFieldValidator);
		ExecutionGuid = Guid.NewGuid();
		Filters = ExtractDefaultRuntimeFilters(NCLMetaQuery);
		ALTopNumberOfRowsToReturn = NCLMetaQuery.TopNumberOfRowsToReturn;
	}

	protected NavQuery(ITreeObject parent, int objectId, SecurityFiltering securityFiltering, NCLMetaQuery metaQuery)
		: base(parent, new ApplicationObjectId(ObjectType.Query, objectId))
	{
		this.securityFiltering = securityFiltering;
		NCLMetaQuery = metaQuery;
		NCLMetaQuery.MetaQuery.ValidateColumns(NCLMetaQuery.MetaFieldValidator);
		ExecutionGuid = Guid.NewGuid();
		Filters = ExtractDefaultRuntimeFilters(NCLMetaQuery);
		ALTopNumberOfRowsToReturn = NCLMetaQuery.TopNumberOfRowsToReturn;
	}

	private static NCLMetaQuery CreateNCLMetaQueryFromMetaQuery(MetaQuery metaQuery)
	{
		NCLMetaQuery metaQueryById = NavCurrentThread.Session.NCLMetadata.GetMetaQueryById(metaQuery.Id);
		if (!metaQuery.CustomQuery)
		{
			return metaQueryById;
		}
		NCLMetaApplicationObjectLoader loader = NCLMetaApplicationObjectLoader.CreateFromNavGlobal();
		NCLMetaQuery nCLMetaQuery = NCLMetaQuery.CreateEmptyNCLMetaQuery(loader, metaQuery, metaQueryById.MetadataAppGroup);
		nCLMetaQuery.CopyClrType(metaQueryById);
		nCLMetaQuery.Populate();
		return nCLMetaQuery;
	}

	private void CheckMetadataHasNotChanged()
	{
		if (metadataChanged)
		{
			throw new NavQueryException(CultureInfo.CurrentCulture, Lang.QueryMetadataChangedSinceOpen);
		}
	}

	internal static FilterFieldDictionary ExtractDefaultRuntimeFilters(NCLMetaQuery metaQuery)
	{
		List<Tuple<INavFieldMetadata, FilterExpression>> list = new List<Tuple<INavFieldMetadata, FilterExpression>>();
		foreach (Tuple<NCLMetaQueryColumn, FilterExpression> columnFilter in metaQuery.ColumnFilters)
		{
			int num = list.FindIndex((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 == columnFilter.Item1);
			if (num >= 0)
			{
				list[num] = new Tuple<INavFieldMetadata, FilterExpression>(columnFilter.Item1, FilterExpression.And(list[num].Item2, columnFilter.Item2));
				continue;
			}
			Tuple<INavFieldMetadata, FilterExpression> item = new Tuple<INavFieldMetadata, FilterExpression>(columnFilter.Item1, columnFilter.Item2);
			list.Add(item);
		}
		return new FilterFieldDictionary(list);
	}

	internal static bool SaveToFile(OutputType filetype, DataError errorLevel, int queryId, string fileName, string delimiter, bool fixedFormat, int[] positions)
	{
		using (new NavScope(NavCurrentThread.Session))
		{
			NavQuery query;
			try
			{
				query = NavGlobal.NCLMetadata.GetMetaQueryById(queryId).CreateObjectInstance(NavCurrentThread.Session);
			}
			catch (NavBaseException) when (errorLevel == DataError.TrapError)
			{
				return false;
			}
			return SaveToFile(query, filetype, errorLevel, fileName, delimiter, fixedFormat, positions);
		}
	}

	internal static bool SaveToFile(NavQuery query, OutputType filetype, DataError errorLevel, string fileName, string delimiter, bool fixedFormat, int[] positions)
	{
		SaveAsRenderer saveAsRenderer = null;
		if (string.IsNullOrEmpty(fileName))
		{
			if (errorLevel == DataError.TrapError)
			{
				return false;
			}
			throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, Lang.QueryEmptyFilename));
		}
		try
		{
			switch (filetype)
			{
			case OutputType.Xml:
				saveAsRenderer = new SaveAsXmlRenderer(NavDataSetBuilder.CreateNavDataSet(query.NCLMetaQuery.MetaQuery), fileName);
				break;
			case OutputType.Csv:
				saveAsRenderer = new QuerySaveAsCsvRenderer(query, fileName, delimiter, fixedFormat, positions);
				break;
			}
			return query.SaveQueryToFile(saveAsRenderer);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			saveAsRenderer?.Dispose();
		}
	}

	internal static bool SaveToStream(OutputType filetype, DataError errorLevel, int queryId, NavOutStream outStream, string delimiter, bool fixedFormat, int[] positions)
	{
		using (new NavScope(NavCurrentThread.Session))
		{
			NavQuery query;
			try
			{
				query = NavGlobal.NCLMetadata.GetMetaQueryById(queryId).CreateObjectInstance(NavCurrentThread.Session);
			}
			catch (NavBaseException) when (errorLevel == DataError.TrapError)
			{
				return false;
			}
			return SaveToStream(query, filetype, errorLevel, outStream, delimiter, fixedFormat, positions);
		}
	}

	internal static bool SaveToStream(NavQuery query, OutputType filetype, DataError errorLevel, NavOutStream outStream, string delimiter, bool fixedFormat, int[] positions)
	{
		SaveAsRenderer saveAsRenderer = null;
		try
		{
			switch (filetype)
			{
			case OutputType.Xml:
				saveAsRenderer = new SaveAsXmlRenderer(NavDataSetBuilder.CreateNavDataSet(query.NCLMetaQuery.MetaQuery), outStream);
				break;
			case OutputType.Csv:
				saveAsRenderer = new QuerySaveAsCsvRenderer(query, outStream, delimiter, fixedFormat, positions);
				break;
			}
			return query.SaveQueryToFile(saveAsRenderer);
		}
		catch (NavBaseException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
		finally
		{
			saveAsRenderer?.Dispose();
		}
	}

	private static int[] StringToIntArray(DataError errorLevel, string positions)
	{
		if (positions == null)
		{
			if (errorLevel == DataError.ThrowError)
			{
				throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, string.Format(CultureInfo.CurrentCulture, Lang.QueryInvalidPositionsArgument)));
			}
			return null;
		}
		try
		{
			return (from stringPosition in positions.Split(new char[5] { ' ', ',', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
				select int.Parse(stringPosition, CultureInfo.InvariantCulture)).ToArray();
		}
		catch (ArgumentNullException)
		{
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
		if (errorLevel == DataError.TrapError)
		{
			return null;
		}
		throw new NavNCLQuerySaveAsException(string.Format(CultureInfo.CurrentCulture, Lang.QueryOutputCouldNotBeSaved, string.Format(CultureInfo.CurrentCulture, Lang.QueryInvalidPositionsArgument)));
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		NavQueryHandle navQueryHandle = new NavQueryHandle(parent, ObjectId.ObjectNumber, securityFiltering);
		navQueryHandle.Target = this;
		return navQueryHandle;
	}

	private void SetFilters(IList<NavFilter> filters)
	{
		foreach (NavFilter filter in filters)
		{
			SetFilter(NCLMetaQuery.QueryDefinition.GetColumnByNo(filter.FilterField), filter.FilterValue);
		}
	}

	public void SetFilter(NCLMetaQueryColumn metaColumn, string expression, params NavValue[] values)
	{
		if (metaColumn == null)
		{
			throw new ArgumentNullException("metaColumn");
		}
		FilterExpression filterExpression = FilterExpressionParser.Parse(expression, metaColumn.ExpressionContext, values);
		SetFilter(metaColumn, filterExpression);
	}

	public void SetFilter(NCLMetaQueryColumn metaColumn, FilterExpression filterExpression)
	{
		if (metaColumn == null)
		{
			throw new ArgumentNullException("metaColumn");
		}
		SetFilterInternal(metaColumn, filterExpression);
		if (currentRowEnumerator != null)
		{
			Close();
			metadataChanged = true;
		}
	}

	private void SetFilterInternal(NCLMetaQueryColumn metaColumn, FilterExpression filterExpression)
	{
		if (filterExpression.IsNullOrConstantTrue())
		{
			filterExpression = null;
		}
		if (Filters == null)
		{
			if (filterExpression != null)
			{
				Filters = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(metaColumn, filterExpression));
			}
			return;
		}
		IEnumerable<Tuple<INavFieldMetadata, FilterExpression>> enumerable = Filters.Items.Where((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 != metaColumn);
		if (filterExpression != null)
		{
			enumerable = enumerable.Concat(new Tuple<INavFieldMetadata, FilterExpression>[1]
			{
				new Tuple<INavFieldMetadata, FilterExpression>(metaColumn, filterExpression)
			});
		}
		Filters = ((enumerable.Count() > 0) ? new FilterFieldDictionary(enumerable) : null);
	}

	public string GetFilter(NCLMetaQueryColumn metaColumn)
	{
		if (metaColumn == null)
		{
			throw new ArgumentNullException("metaColumn");
		}
		if (Filters == null)
		{
			return string.Empty;
		}
		Tuple<INavFieldMetadata, FilterExpression> tuple = Filters.Items.SingleOrDefault((Tuple<INavFieldMetadata, FilterExpression> t) => t.Item1 == metaColumn);
		if (tuple == null)
		{
			return string.Empty;
		}
		return tuple.Item2.ToString();
	}

	public string GetFilters()
	{
		if (Filters == null)
		{
			return string.Empty;
		}
		return string.Join(", ", Filters.Items.Select((Tuple<INavFieldMetadata, FilterExpression> t) => ((NCLMetaQueryColumn)t.Item1).Caption + ": " + t.Item2.ToString()));
	}

	public void SetRange(NCLMetaQueryColumn metaColumn, NavValue fromValue, NavValue toValue)
	{
		if (metaColumn == null)
		{
			throw new ArgumentNullException("metaColumn");
		}
		if (fromValue != null && fromValue.NclType != metaColumn.NclType)
		{
			fromValue = NavValue.CreateNavValueFromObject(metaColumn, fromValue);
		}
		if (toValue != null && toValue.NclType != metaColumn.NclType)
		{
			toValue = NavValue.CreateNavValueFromObject(metaColumn, toValue);
		}
		FilterExpression filterExpression;
		if (toValue != null)
		{
			filterExpression = ((fromValue != null) ? FilterExpression.RangeBetweenInclusive(fromValue, toValue, metaColumn.ExpressionContext) : null);
		}
		else
		{
			filterExpression = ((fromValue != null) ? FilterExpression.Equal(fromValue, metaColumn.ExpressionContext) : null);
		}
		SetFilterInternal(metaColumn, filterExpression);
		if (currentRowEnumerator != null)
		{
			Close();
			metadataChanged = true;
		}
	}

	public static void Run(Guid executionId, IList<NavFilter> filters, MetaQuery metaQuery)
	{
		RunQuery(executionId, filters, metaQuery);
	}

	public static void Run(Guid executionId, int queryId, IList<NavFilter> filters)
	{
		RunQuery(executionId, queryId, filters, null);
	}

	public static void Run(Guid executionId, int queryId, IList<NavFilter> filters, int requestedTopNumberOfRows)
	{
		RunQuery(executionId, queryId, filters, requestedTopNumberOfRows);
	}

	public static void Run(Guid executionId, int queryId)
	{
		RunQuery(executionId, queryId, null, null);
	}

	private bool SaveToFile(OutputType filetype, DataError errorLevel, string fileName, string delimiter, bool fixedFormat, int[] positions)
	{
		return SaveToFile(this, filetype, errorLevel, fileName, delimiter, fixedFormat, positions);
	}

	private bool SaveQueryToFile(SaveAsRenderer saveAsRenderer)
	{
		ResultSetProcessor = saveAsRenderer;
		return RunQueryInternal(null);
	}

	internal void Open()
	{
		NavigateQueryInternal();
	}

	protected virtual void OnBeforeOpen()
	{
	}

	internal void RunOnBeforeOpentrigger()
	{
		bool commit = false;
		bool flag = false;
		try
		{
			if (inOnBeforeOpenTrigger)
			{
				throw new NavQueryIllegalCallInTriggerException(Lang.IllegalCallInTrigger);
			}
			inOnBeforeOpenTrigger = true;
			NavCurrentThread.Session.BeginTransaction();
			flag = true;
			OnBeforeOpen();
			commit = true;
		}
		finally
		{
			if (flag)
			{
				NavCurrentThread.Session.EndTransaction(commit);
			}
			inOnBeforeOpenTrigger = false;
			if (NavCurrentThread.Session.CallbackAllowed)
			{
				NavDialog.ProcessServerRequests();
			}
		}
	}

	private NCLMetaQueryColumn ValidateExpectedType(int columnId, NavType expectedType)
	{
		NCLMetaQueryColumn columnByNo = NCLMetaQuery.QueryDefinition.GetColumnByNo(columnId);
		if (columnByNo == null)
		{
			throw new NavQueryDefinitionChangedException(CultureInfo.CurrentCulture, columnId);
		}
		if (columnByNo.NavType != expectedType)
		{
			throw new NavQueryDefinitionChangedException(CultureInfo.CurrentCulture, columnByNo.Name, expectedType, columnByNo.NavType);
		}
		return columnByNo;
	}

	private NavValue GetColumnValue(NCLMetaQueryColumn columnMetadata)
	{
		if (columnMetadata == null)
		{
			throw new ArgumentNullException("columnMetadata");
		}
		if (CurrentDataRow == null)
		{
			if (inOnBeforeOpenTrigger)
			{
				throw new NavQueryIllegalCallInTriggerException(Lang.IllegalCallInTrigger);
			}
			throw new NavQueryNoDataSetException(Lang.NoDataRowDefined, columnMetadata.Name);
		}
		return CurrentDataRow[columnMetadata.ColumnIndex];
	}

	public NavValue GetColumnValueSafe(int columnId, NavType expectedType)
	{
		return GetColumnValue(ValidateExpectedType(columnId, expectedType));
	}

	private static NavValue GetColumnOption(NCLMetaQueryColumn columnMetadata)
	{
		if (columnMetadata == null)
		{
			throw new ArgumentNullException("columnMetadata");
		}
		return columnMetadata.InitValue;
	}

	public NavValue GetColumnOptionSafe(int columnId, NavType expectedType)
	{
		return GetColumnOption(ValidateExpectedType(columnId, expectedType));
	}

	private static bool RunQuery(Guid executionGuid, int queryId, IList<NavFilter> filters, int? requestedTopNumberOfRows)
	{
		using NavQuery navQuery = NavGlobal.NCLMetadata.GetMetaQueryById(queryId).CreateObjectInstance(NavCurrentThread.Session);
		navQuery.ExecutionGuid = executionGuid;
		navQuery.Paging = true;
		navQuery.ResultSetProcessor = new QueryClientCallback(navQuery);
		if (filters != null)
		{
			navQuery.SetFilters(filters);
		}
		return navQuery.RunQueryInternal(requestedTopNumberOfRows);
	}

	private static bool RunQuery(Guid executionGuid, IList<NavFilter> filters, MetaQuery metaQuery)
	{
		if (metaQuery == null)
		{
			throw new ArgumentException("Invalid parameter", "metaQuery");
		}
		NCLMetaQuery nCLMetaQuery = CreateNCLMetaQueryFromMetaQuery(metaQuery);
		using NavQuery navQuery = nCLMetaQuery.CreateObjectInstance(NavCurrentThread.Session);
		navQuery.ExecutionGuid = executionGuid;
		navQuery.NCLMetaQuery = nCLMetaQuery;
		navQuery.Filters = ExtractDefaultRuntimeFilters(nCLMetaQuery);
		navQuery.Paging = true;
		navQuery.ResultSetProcessor = new QueryClientCallback(navQuery);
		if (filters != null)
		{
			navQuery.SetFilters(filters);
		}
		return navQuery.RunQueryInternal(null);
	}

	public static NavDataSet RunQueryDataSet(Guid executionId, int queryId, IList<NavFilter> filters)
	{
		using NavQuery query = NavGlobal.NCLMetadata.GetMetaQueryById(queryId).CreateObjectInstance(NavCurrentThread.Session);
		QueryClientCallback queryClientCallback = RunQueryDataSetInternal(executionId, query, filters);
		return queryClientCallback.DataSet;
	}

	public static NavDataSet RunQueryDataSet(Guid executionId, MetaQuery metaQuery, IList<NavFilter> filters)
	{
		NCLMetaQuery nCLMetaQuery = CreateNCLMetaQueryFromMetaQuery(metaQuery);
		using NavQuery navQuery = nCLMetaQuery.CreateObjectInstance(NavCurrentThread.Session);
		navQuery.NCLMetaQuery = nCLMetaQuery;
		navQuery.Filters = ExtractDefaultRuntimeFilters(nCLMetaQuery);
		QueryClientCallback queryClientCallback = RunQueryDataSetInternal(executionId, navQuery, filters);
		return queryClientCallback.DataSet;
	}

	private static QueryClientCallback RunQueryDataSetInternal(Guid executionId, NavQuery query, IList<NavFilter> filters)
	{
		query.Paging = false;
		query.ExecutionGuid = executionId;
		QueryClientCallback result = (QueryClientCallback)(query.ResultSetProcessor = new QueryClientCallback(query));
		if (filters != null)
		{
			query.SetFilters(filters);
		}
		query.RunQueryInternal(null);
		return result;
	}

	public NavValue[][] RunQueryForOData(NavValue[] startingPosition, int pageSize, int skipSize, CancellationToken cancellationToken)
	{
		int totalCount;
		return RunQueryForOData(startingPosition, pageSize, skipSize, countAllRecords: false, out totalCount, cancellationToken);
	}

	public NavValue[][] RunQueryForOData(NavValue[] startingPosition, int pageSize, int skipSize, bool countAllRecords, out int totalCount, CancellationToken cancellationToken)
	{
		List<NavValue[]> list = new List<NavValue[]>();
		totalCount = 0;
		ALTopNumberOfRowsToReturn = ((ALTopNumberOfRowsToReturn > 0) ? Math.Min(ALTopNumberOfRowsToReturn, pageSize) : pageSize);
		ALSkipNumberOfRows = skipSize;
		RunOnBeforeOpentrigger();
		MutableRecordBuffer startingBuffer = ((startingPosition != null) ? new MutableRecordBuffer(new ReadOnlyRecordBuffer(NCLMetaQuery, startingPosition)) : null);
		FindData(startingBuffer);
		if (currentRowEnumerator == null)
		{
			return new NavValue[0][];
		}
		while (GetNextRow())
		{
			cancellationToken.ThrowIfCancellationRequested();
			list.Add(currentDataRow.ReadOnlyBuffer.ToArray());
		}
		if (countAllRecords)
		{
			totalCount = GetCount(startingBuffer);
		}
		return Enumerable.ToArray(list);
	}

	private bool RunQueryInternal(int? requestedTopNumberOfRows)
	{
		return ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			ResultSetProcessor.Start();
			RunOnBeforeOpentrigger();
			try
			{
				if (requestedTopNumberOfRows.HasValue && (topNumberOfRowsToReturn == 0 || topNumberOfRowsToReturn > requestedTopNumberOfRows.Value))
				{
					topNumberOfRowsToReturn = requestedTopNumberOfRows.Value;
					((QueryClientCallback)ResultSetProcessor).SetSpecialCaptionInformation(requestedTopNumberOfRows.Value);
				}
				FindData();
				ParseResultSetEnumerator();
			}
			catch (NavControlException ex)
			{
				if (ex.ControlStatement == NavControlStatement.ClientCancel)
				{
					ClientCanceled = true;
				}
				ViewerMustBeClosedOnQuit = true;
				ResultSetProcessor.CleanupOnError();
				return false;
			}
			catch (Exception)
			{
				ViewerMustBeClosedOnQuit = true;
				ResultSetProcessor.CleanupOnError();
				throw;
			}
			finally
			{
				if (!ClientCanceled && ViewerMustBeClosedOnQuit)
				{
					ResultSetProcessor.Abort();
				}
			}
			return true;
		});
	}

	private void ParseResultSetEnumerator()
	{
		while (GetNextRow())
		{
			foreach (NCLMetaQueryColumn item in NCLMetaQuery.QueryDefinition.DefinedColumns.Where((NCLMetaQueryColumn column) => column.ColumnType != QueryColumnType.FilterOnly))
			{
				SetResultSetProcessorValue(item, CurrentDataRow[item.ColumnIndex]);
			}
			ResultSetProcessor.EndOfRow();
		}
		ResultSetProcessor.Finish();
	}

	private void NavigateQueryInternal()
	{
		if (currentRowEnumerator != null)
		{
			Close();
		}
		RunOnBeforeOpentrigger();
		metadataChanged = false;
		FindData();
	}

	internal void FindData(MutableRecordBuffer startingBuffer = null, bool includeCurrent = false, SortingFieldList sortingFields = null)
	{
		NavSession navSession = NavCurrentThread.Session;
		bool flag = true;
		currentDataRow = null;
		if (startingBuffer == null)
		{
			Dictionary<int, FilterFieldDictionary> securityAndGlobalFilters = NavSqlQueryCommand.GetSecurityAndGlobalFilters(NCLMetaQuery.QueryDefinition, Session.Company.CompanyNameToken, securityFiltering);
			FindCacheRequest request = new FindCacheRequest(Session.Company.CompanyNameToken, NCLMetaQuery, cacheable: true, new FiltersAndMarks(Filters), (securityAndGlobalFilters == null) ? TableFilterDictionary.Empty : new TableFilterDictionary(securityAndGlobalFilters), securityFiltering, FieldList.Empty, sortingFields, FindType.Normal, ALTopNumberOfRowsToReturn, ALSkipNumberOfRows);
			DataAccess dataAccessForQuery = navSession.DataAccessSource.GetDataAccessForQuery(NCLMetaQuery);
			currentRowEnumerator = dataAccessForQuery.Find(request);
			currentRowCount = 0;
			checkPermissionsOnReadNextRow = true;
		}
		else
		{
			int num = ALTopNumberOfRowsToReturn;
			if (num != 0)
			{
				num -= currentRowCount;
				if (num <= 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				Dictionary<int, FilterFieldDictionary> securityAndGlobalFilters2 = NavSqlQueryCommand.GetSecurityAndGlobalFilters(NCLMetaQuery.QueryDefinition, Session.Company.CompanyNameToken, securityFiltering);
				PositionedFindCacheRequest request2 = new PositionedFindCacheRequest(Session.Company.CompanyNameToken, NCLMetaQuery, cacheable: true, new FiltersAndMarks(Filters), (securityAndGlobalFilters2 == null) ? TableFilterDictionary.Empty : new TableFilterDictionary(securityAndGlobalFilters2), securityFiltering, FieldList.Empty, sortingFields, FindType.Normal, startingBuffer, includeCurrent, num, ALSkipNumberOfRows);
				DataAccess dataAccessForQuery2 = navSession.DataAccessSource.GetDataAccessForQuery(NCLMetaQuery);
				currentRowEnumerator = dataAccessForQuery2.FindFromPosition(request2);
				if (currentRowCount == 0)
				{
					checkPermissionsOnReadNextRow = true;
				}
			}
			else
			{
				currentRowEnumerator = null;
			}
		}
		if ((currentRowEnumerator == null) & flag)
		{
			VerifyPermissions();
		}
	}

	internal int GetCount(MutableRecordBuffer startingBuffer = null)
	{
		VerifyPermissions();
		NavSession navSession = NavCurrentThread.Session;
		Dictionary<int, FilterFieldDictionary> securityAndGlobalFilters = NavSqlQueryCommand.GetSecurityAndGlobalFilters(NCLMetaQuery.QueryDefinition, Session.Company.CompanyNameToken, securityFiltering);
		DataAccess dataAccessForQuery = navSession.DataAccessSource.GetDataAccessForQuery(NCLMetaQuery);
		return dataAccessForQuery.DataProvider.GetQueryCount(NCLMetaQuery.QueryDefinition, Session.Company.CompanyNameToken, (NCLMetaQuery.TopNumberOfRowsToReturn > 0) ? NCLMetaQuery.TopNumberOfRowsToReturn : 0, 0, new FiltersAndMarks(Filters), (securityAndGlobalFilters == null) ? TableFilterDictionary.Empty : new TableFilterDictionary(securityAndGlobalFilters), startingBuffer, securityFiltering, addExplicitJoinHints: true);
	}

	internal bool GetNextRow()
	{
		if (currentRowEnumerator == null)
		{
			return false;
		}
		MutableRecordBuffer startingBuffer = currentDataRow;
		currentDataRow = null;
		bool result;
		switch (currentRowEnumerator.TryMoveNext(out currentDataRow))
		{
		case ResultSetMoveResult.Ok:
			result = true;
			currentRowCount++;
			break;
		case ResultSetMoveResult.NoMoreResults:
			result = false;
			break;
		case ResultSetMoveResult.ResultSetNoLongerValid:
			FindData(startingBuffer);
			currentDataRow = startingBuffer;
			return GetNextRow();
		default:
			result = false;
			break;
		}
		if (checkPermissionsOnReadNextRow)
		{
			VerifyPermissions();
			checkPermissionsOnReadNextRow = false;
		}
		return result;
	}

	internal void Close()
	{
		currentDataRow = null;
		currentRowEnumerator = null;
	}

	protected void SetResultSetProcessorValue(NCLMetaQueryColumn metaColumn, NavValue value)
	{
		object clientObject = value.ClientObject;
		if (clientObject != null)
		{
			switch (value.NavType)
			{
			case NavType.BLOB:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, new byte[0]);
				break;
			case NavType.Duration:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, value.ToString());
				break;
			case NavType.Option:
			{
				NavOption navOption = (NavOption)value;
				if (metaColumn.FieldNo != -1 && navOption.NavOptionMetadata.HasCaption)
				{
					ResultSetProcessor.SetValue(metaColumn.ColumnIndex, navOption.NavOptionMetadata.GetCaptionFromIndex(navOption.Value));
				}
				else
				{
					ResultSetProcessor.SetValue(metaColumn.ColumnIndex, navOption.NavOptionMetadata.GetOptionFromIndex(navOption.Value));
				}
				break;
			}
			case NavType.RecordID:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, ((NavRecordId)value).ToString());
				break;
			case NavType.Date:
			case NavType.Time:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, value.IsZeroOrEmpty ? DBNull.Value : clientObject);
				break;
			case NavType.DateTime:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, value.IsZeroOrEmpty ? DBNull.Value : ((object)((NavDateTime)value).ClientLocalValue));
				break;
			case NavType.Char:
			case NavType.Integer:
			case NavType.Byte:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, (int)clientObject);
				break;
			case NavType.BigInteger:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, (long)clientObject);
				break;
			case NavType.Decimal:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, (decimal)clientObject);
				break;
			case NavType.Media:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, new NSMediaWrapper(clientObject as NavMediaDescriptionCollection));
				break;
			case NavType.MediaSet:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, new NSMediaSetWrapper(clientObject as NavMediaDescriptionCollection));
				break;
			default:
				ResultSetProcessor.SetValue(metaColumn.ColumnIndex, clientObject);
				break;
			}
		}
		else
		{
			ResultSetProcessor.SetValue(metaColumn.ColumnIndex, DBNull.Value);
		}
	}

	private void VerifyPermissions()
	{
		Session.VerifyExecutePermission(ObjectId);
		if (!NCLMetaQuery.QueryDefinition.IncludedTables.Any())
		{
			return;
		}
		IEnumerable<ApplicationObjectId> applicationObjects = NCLMetaQuery.QueryDefinition.IncludedTables.Select((NCLMetaTable t) => new ApplicationObjectId(ObjectType.TableData, t.ObjectId));
		IPermissionSet permissionSet = Session.GetPermissionSet(this, Session.Company.CompanyNameToken, applicationObjects);
		permissionSet.VerifyPermissions(Session.CurrentMethodScope.ApplicationObject, PermissionMask.Read, Session.LimitedUserPermissionChecker, (int id) =>
		{
			DataAccess dataAccessForQuery = Session.DataAccessSource.GetDataAccessForQuery(NCLMetaQuery);
			ExistsCacheRequest request = new ExistsCacheRequest(Session.Company.CompanyNameToken, NCLMetaQuery.QueryDefinition.IncludedTables.First((NCLMetaTable t) => t.ObjectId == id), cacheable: true, FiltersAndMarks.Empty, SecurityFiltering.Ignored, forceNonLockingProviderRequest: true);
			return !dataAccessForQuery.Exists(request);
		});
	}

	internal string GetCaption(bool withPrefix)
	{
		string caption = NCLMetaQuery.Caption;
		if (withPrefix)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", "Query", caption);
		}
		return caption;
	}

	public override int GetHashCode()
	{
		return ObjectId.ObjectNumber;
	}
}
