using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public abstract class DataItemIterator : NavApplicationObjectBase
{
	public enum DataItemIteratorType
	{
		Report,
		Query
	}

	public delegate void QueryParameters();

	public const string XmlDataSet = "ReportDataSet";

	public const string XmlLabels = "Labels";

	public const string XmlLabel = "Label";

	public const string XmlDataItems = "DataItems";

	public const string XmlDataItem = "DataItem";

	public const string XmlColumns = "Columns";

	public const string XmlColumn = "Column";

	public const string XmlAttributeName = "name";

	public const string XmlAttributeId = "id";

	private int rowsProcessed;

	private static readonly IResultSetProcessor nullResultSetProcessor = new NullResultSetProcessor();

	private readonly List<DataItem> dataItems = new List<DataItem>();

	private readonly List<DataItem> metaDataItems = new List<DataItem>();

	private NCLCaptionStrings captionML;

	private int localLanguage = -1;

	private bool isRunning;

	private bool printOnlyIfDetail;

	private Guid executionGuid = Guid.NewGuid();

	private bool disposed;

	protected bool RecalculateSpecificData { get; set; }

	public bool SetTableViewUsed { get; private set; }

	public bool[] TableViewIsSet { get; internal set; }

	protected abstract DataItemIteratorType IteratorType { get; }

	protected static IResultSetProcessor NullResultSetProcessor => nullResultSetProcessor;

	protected bool ViewerMustBeClosedOnQuit { get; set; }

	public bool Paging { get; set; }

	public IList<string> TextualFilters { get; private set; }

	public IResultSetProcessor ResultSetProcessor { get; protected set; }

	public bool IsSaveAsXmlTreeProcessor { get; set; }

	protected List<DataItem> MetaDataItems => metaDataItems;

	public Guid ExecutionGuid
	{
		get
		{
			return executionGuid;
		}
		protected set
		{
			executionGuid = value;
		}
	}

	public List<DataItem> DataItems => dataItems;

	public int Language
	{
		get
		{
			if (isRunning)
			{
				return NavCurrentThread.Session.LocalLanguage;
			}
			return localLanguage;
		}
		set
		{
			if (value == 0)
			{
				throw new NavNCLArgumentException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidReportLanguageID, 0));
			}
			NavCurrentThread.Session.TestReportLanguagePermission(value);
			if (WindowsLanguageDataProvider.FindValidCulture(value) == null)
			{
				throw NavNCLReportLanguageNotFoundException.Create(CultureInfo.InvariantCulture, value.ToString(CultureInfo.CurrentCulture));
			}
			if (isRunning)
			{
				NavCurrentThread.Session.LocalLanguage = value;
			}
			else
			{
				localLanguage = value;
			}
		}
	}

	public MetaReport Metadata { get; protected set; }

	public bool PrintOnlyIfDetail
	{
		get
		{
			return printOnlyIfDetail;
		}
		set
		{
			printOnlyIfDetail = value;
		}
	}

	public int ReportId => ObjectId.ObjectNumber;

	public string Caption => GetCaption(withPrefix: false);

	private string GetCaption(bool withPrefix)
	{
		string valueOrDefault = captionML.GetValueOrDefault();
		if (withPrefix)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0} {1}", IteratorType, valueOrDefault);
		}
		return valueOrDefault;
	}

	protected void SetTextualFilters(IList<string> reportTextualFilters)
	{
		TextualFilters = reportTextualFilters;
	}

	protected virtual void Add(DataItem dataItem)
	{
		dataItems.Add(dataItem);
	}

	protected void ApplyDataItemTableViewAndRequestFormFilters()
	{
		foreach (DataItem dataItem in dataItems)
		{
			if (!string.IsNullOrEmpty(dataItem.MetaData.DataItemTableView))
			{
				dataItem.TableViewRecord.SecurityFiltering = dataItem.Record.SecurityFiltering;
				NavTableView tableView;
				NavTableView tableView2;
				using (NavRecord navRecord = new NavRecord(dataItem.Record.Session, dataItem.Record.TableID, dataItem.Record.SecurityFiltering))
				{
					tableView = dataItem.TableViewRecord.GetTableView();
					navRecord.ALSetView(dataItem.MetaData.DataItemTableView);
					tableView2 = navRecord.GetTableView();
				}
				if (tableView2.Ascending != tableView.Ascending)
				{
					dataItem.TableViewRecord.ALAscending = (dataItem.MetaData.DataItemTableView.Contains("ORDER") ? tableView2.Ascending : tableView.Ascending);
				}
				if (!tableView2.CompareCurrentSortingFieldIds(tableView.CurrentSortingFieldIds))
				{
					dataItem.TableViewRecord.SetCurrentKey(dataItem.MetaData.DataItemTableView.Contains("SORTING") ? tableView2 : tableView);
				}
			}
		}
	}

	private static void DataItemTriggerSwitch(DataItem dataItem, NavTrigger dataItemTriggerDelegate)
	{
		switch (ExecuteTrigger(dataItemTriggerDelegate))
		{
		case NavControlStatement.Break:
			if (dataItem.OnPostDataItem != dataItemTriggerDelegate)
			{
				dataItem.BreakDataItem = true;
			}
			break;
		case NavControlStatement.Skip:
			if (dataItem.OnAfterGetRecord == dataItemTriggerDelegate)
			{
				dataItem.SkipRecord = true;
			}
			break;
		case NavControlStatement.None:
		case NavControlStatement.BreakUnbound:
			break;
		}
	}

	private void FindDataItemChildren(DataItem dataItem)
	{
		dataItem.Children.Clear();
		int num = dataItems.FindIndex((DataItem dataItemVarName) => dataItemVarName.MetaData.DataItemVarName == dataItem.MetaData.DataItemVarName);
		for (int num2 = num + 1; num2 < dataItems.Count && dataItems[num2].MetaData.DataItemIndent > dataItem.MetaData.DataItemIndent; num2++)
		{
			if (dataItems[num2].MetaData.DataItemIndent > dataItem.MetaData.DataItemIndent && dataItems[num2].MetaData.DataItemIndent == dataItem.MetaData.DataItemIndent + 1)
			{
				dataItem.Children.Add(dataItems[num2]);
			}
		}
	}

	private void FindDataItemParent(DataItem dataItem)
	{
		dataItem.Parent = null;
		int num = dataItems.FindIndex((DataItem dataItemVarName) => dataItemVarName.MetaData.DataItemVarName == dataItem.MetaData.DataItemVarName);
		for (int num2 = num; num2 != -1; num2--)
		{
			if (dataItems[num2].MetaData.DataItemIndent < dataItem.MetaData.DataItemIndent)
			{
				dataItem.Parent = dataItems[num2];
				break;
			}
		}
	}

	protected void LoopRootDataItems()
	{
		foreach (DataItem item in dataItems.FindAll((DataItem dataItemIndentZero) => dataItemIndentZero.MetaData.DataItemIndent == 0))
		{
			if (item.MetaData.DataItemIndent != 0)
			{
				throw new NavNCLReportDataItemIndentionException();
			}
			ExecuteDataItemLoop(item);
		}
	}

	private void LoopChildDataItems(DataItem dataItem)
	{
		foreach (DataItem child in dataItem.Children)
		{
			child.AlreadyAddedRow = false;
			ExecuteDataItemLoop(child);
			if (child.AlreadyAddedRow)
			{
				dataItem.AlreadyAddedRow = true;
			}
			if (child.SkipRecord || child.BreakDataItem || !dataItem.AlreadyAddedRow)
			{
				if (metaDataItems.Count > 0 && !dataItem.PrintOnlyIfDetail && !dataItem.AlreadyAddedRow && dataItem.HasColumns)
				{
					ExecuteReportDataItem();
					dataItem.AlreadyAddedRow = true;
				}
				child.SkipRecord = false;
				child.BreakDataItem = false;
			}
		}
		if (dataItem.Parent != null && dataItem.AlreadyAddedRow)
		{
			dataItem.Parent.AlreadyAddedRow = true;
		}
		dataItem.AlreadyAddedRow = false;
	}

	private static void SetDataItemTableView(DataItem dataItem, FilterGroupType filterGroup)
	{
		dataItem.Record.RecordImplementation.ResetRecordInternal();
		dataItem.TableViewRecord.SecurityFiltering = dataItem.Record.SecurityFiltering;
		dataItem.Record.RecordImplementation.CopyFiltersSortingMarkList(dataItem.TableViewRecord);
		if (!string.IsNullOrEmpty(dataItem.MetaData.DataItemTableView))
		{
			int aLFilterGroup = dataItem.Record.ALFilterGroup;
			try
			{
				dataItem.Record.ALFilterGroup = (int)filterGroup;
				dataItem.SetDataItemTableViewFieldFilters();
			}
			finally
			{
				dataItem.Record.ALFilterGroup = aLFilterGroup;
			}
		}
	}

	private void SetDataItemLink(DataItem dataItem, FilterGroupType filterGroup)
	{
		if (string.IsNullOrEmpty(dataItem.MetaData.DataItemLink))
		{
			return;
		}
		foreach (DataItem dataItem2 in dataItems)
		{
			if (dataItem2.MetaData.DataItemVarName == dataItem.MetaData.DataItemLinkReference)
			{
				int aLFilterGroup = dataItem.Record.ALFilterGroup;
				try
				{
					dataItem.Record.ALFilterGroup = (int)filterGroup;
					dataItem.SetRecordDataItemLink(dataItem2.Record);
					break;
				}
				finally
				{
					dataItem.Record.ALFilterGroup = aLFilterGroup;
				}
			}
		}
	}

	private void ExecuteReportDataItem()
	{
		foreach (DataItem metaDataItem in metaDataItems)
		{
			if (!metaDataItem.IsRowCalculated)
			{
				ResultSetProcessor.StartLevel("DataItem", metaDataItem.MetaData.ExternalizedName);
				if (metaDataItem.MetaData.DataItemColumns.Count > 0)
				{
					ResultSetProcessor.StartLevel("Columns", null);
					foreach (MetaDataItemColumn dataItemColumn in metaDataItem.MetaData.DataItemColumns)
					{
						if (!IsSaveAsXmlTreeProcessor || !dataItemColumn.IsDecimalFormatter)
						{
							EvaluateDataColumn(metaDataItem.Record, dataItemColumn);
						}
					}
					ResultSetProcessor.EndLevel();
				}
				if (metaDataItem.HasChildren)
				{
					ResultSetProcessor.StartLevel("DataItems", null);
				}
				AppendDataItemSpecificData(metaDataItem);
				if (IsSaveAsXmlTreeProcessor)
				{
					metaDataItem.IsRowCalculated = true;
				}
			}
			else
			{
				ResultSetProcessor.InsertRepeatedData(metaDataItem.LabelDataSetIndexes);
			}
		}
		AppendSpecificData();
		RecalculateSpecificData = false;
		rowsProcessed++;
		ResultSetProcessor.EndOfRow();
	}

	private void ExecuteDataItemLoop(DataItem dataItem)
	{
		int num = 0;
		printOnlyIfDetail = dataItem.PrintOnlyIfDetail;
		SetDataItemTableView(dataItem, FilterGroupType.TableViewFilters);
		SetDataItemLink(dataItem, FilterGroupType.SubFormLinkFilters);
		DataItemTriggerSwitch(dataItem, dataItem.OnPreDataItem);
		if (!dataItem.BreakDataItem && dataItem.FirstRecord())
		{
			metaDataItems.Add(dataItem);
			do
			{
				dataItem.SkipRecord = false;
				if (dataItem.MetaData.MaxIteration != 0 && num == dataItem.MetaData.MaxIteration)
				{
					break;
				}
				num++;
				DataItemTriggerSwitch(dataItem, dataItem.OnAfterGetRecord);
				if (dataItem.SkipRecord)
				{
					continue;
				}
				if (dataItem.BreakDataItem)
				{
					break;
				}
				dataItem.PrintOnlyIfDetail = printOnlyIfDetail;
				if (dataItem.HasChildren)
				{
					LoopChildDataItems(dataItem);
				}
				else
				{
					if (dataItem.PrintOnlyIfDetail)
					{
						dataItem.SkipRecord = true;
						continue;
					}
					if (dataItem.HasColumns)
					{
						ExecuteReportDataItem();
						dataItem.AlreadyAddedRow = true;
					}
				}
				printOnlyIfDetail = dataItem.PrintOnlyIfDetail;
				if (dataItem.IsRowCalculated)
				{
					ResultSetProcessor.EndLevel();
					if (dataItem.HasChildren)
					{
						ResultSetProcessor.EndLevel();
					}
				}
			}
			while (dataItem.NextRecord());
		}
		else if (dataItem.PrintOnlyIfDetail && !dataItem.HasChildren)
		{
			dataItem.SkipRecord = true;
		}
		else if (metaDataItems.Count > 0 && !dataItem.Parent.PrintOnlyIfDetail && dataItem.HasColumns && !dataItem.BreakDataItem)
		{
			ExecuteReportDataItem();
			dataItem.AlreadyAddedRow = true;
		}
		metaDataItems.Remove(dataItem);
		DataItemTriggerSwitch(dataItem, dataItem.OnPostDataItem);
	}

	protected void ProcessTextualFilters()
	{
		NavTextualFilterGroup textualFilterGroup = NavTextualFilterGroup.GetTextualFilterGroup(TextualFilters, this);
		Dictionary<DataItem, NavTableView> dataItemsTableViews = textualFilterGroup.GetDataItemsTableViews();
		foreach (DataItem key in dataItemsTableViews.Keys)
		{
			key.Record.SetTableView(dataItemsTableViews[key]);
		}
	}

	protected string BuildViewParameter(NavRecord record)
	{
		string result = string.Empty;
		_ = string.Empty;
		if (record != null)
		{
			string text = record.ALGetView(useCaptions: false);
			if (!string.IsNullOrEmpty(text))
			{
				result = "view" + record.TableID.ToString(CultureInfo.InvariantCulture) + "=" + text;
			}
		}
		else
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (DataItem dataItem in dataItems)
			{
				string text = dataItem.Record.ALGetView(useCaptions: false);
				if (!string.IsNullOrEmpty(text) && !string.Equals(text, dataItem.DefaultRecordView))
				{
					if (stringBuilder.Length > 0)
					{
						stringBuilder.Append('&');
					}
					stringBuilder.Append("view" + dataItem.Record.TableID.ToString(CultureInfo.InvariantCulture) + "=" + text);
				}
			}
			result = stringBuilder.ToString();
		}
		return result;
	}

	protected void PopLocalLanguage()
	{
		isRunning = false;
		localLanguage = NavCurrentThread.Session.PopLocalLanguage();
		RecalculateSpecificData = true;
	}

	protected void PushLocalLanguage()
	{
		isRunning = true;
		RecalculateSpecificData = true;
		int localLanguageNoFallback = NavCurrentThread.Session.LocalLanguageNoFallback;
		if (localLanguageNoFallback != -1 && localLanguage == -1)
		{
			NavCurrentThread.Session.PushLocalLanguage(localLanguageNoFallback);
		}
		else
		{
			NavCurrentThread.Session.PushLocalLanguage(localLanguage);
		}
	}

	protected DataItemIterator(ITreeObject parent, ApplicationObjectId objectId)
		: base(parent, objectId)
	{
		Paging = true;
	}

	public override int GetHashCode()
	{
		return ObjectId.ObjectNumber;
	}

	protected internal static NavControlStatement ExecuteTrigger(NavTrigger reportTriggerDelegate)
	{
		NavControlStatement result = NavControlStatement.None;
		if (reportTriggerDelegate != null)
		{
			try
			{
				reportTriggerDelegate();
			}
			catch (NavControlException ex) when (ex.ControlStatement != NavControlStatement.Quit)
			{
				result = ex.ControlStatement;
			}
		}
		return result;
	}

	protected string GetDecimalString(INavRecordHandle record, int? fieldId, int controlId)
	{
		string autoFormatString = GetAutoFormatString(controlId);
		if (!string.IsNullOrEmpty(autoFormatString))
		{
			return autoFormatString;
		}
		if (fieldId.HasValue)
		{
			autoFormatString = record.Target.GetAutoFormatString(fieldId.Value);
			if (!string.IsNullOrEmpty(autoFormatString))
			{
				return autoFormatString;
			}
		}
		autoFormatString = GetDecimalPlaces(controlId);
		if (!string.IsNullOrEmpty(autoFormatString))
		{
			return autoFormatString;
		}
		if (fieldId.HasValue)
		{
			autoFormatString = record.Target.GetDecimalPlaces(fieldId.Value);
			if (!string.IsNullOrEmpty(autoFormatString))
			{
				return autoFormatString;
			}
		}
		return Session.SystemCodeunitFactory.UIHelperTriggers.InvokeAutoFormatTranslate(1, NavText.Empty);
	}

	protected virtual string GetAutoFormatString(int controlId)
	{
		return string.Empty;
	}

	protected virtual string GetDecimalPlaces(int controlId)
	{
		return string.Empty;
	}

	protected virtual void BeginInitialization()
	{
		captionML = new NCLCaptionStrings(Metadata.CaptionML.ToArray(), Metadata.Name);
	}

	protected virtual void EndInitialization()
	{
		dataItems.ForEach((DataItem dataItem) =>
		{
			FindDataItemChildren(dataItem);
			FindDataItemParent(dataItem);
		});
		TableViewIsSet = new bool[DataItems.Count];
	}

	protected void ExecuteDataItemIterator()
	{
		rowsProcessed = 0;
		ViewerMustBeClosedOnQuit = true;
		ApplyDataItemTableViewAndRequestFormFilters();
		OnPreTrigger();
		using (NavCurrentThread.Session.Diagnostics.CreateTraceScope("00009AF", DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "DataItem Iterator for object {0} {1}", ObjectId.ObjectType, ObjectId.ObjectNumber)), (MonitorType)458755, Category.Runtime))
		{
			ResultSetProcessor.Start();
			LoopRootDataItems();
			ResultSetProcessor.Finish();
		}
		DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000016C", Category.Runtime, Verbosity.Normal, PrivacyClassification.SystemMetadata, "Report {0} generated, processed {1} number of rows.", ReportId, rowsProcessed);
		ViewerMustBeClosedOnQuit = false;
		OnPostTrigger();
	}

	protected virtual void OnPreTrigger()
	{
	}

	protected virtual void OnPostTrigger()
	{
	}

	protected abstract void EvaluateDataColumn(NavRecord record, MetaDataItemColumn metaColumn);

	protected abstract void AppendDataItemSpecificData(DataItem dataItem);

	protected abstract void AppendSpecificData();

	protected override void Dispose(bool disposing)
	{
		if (!disposed)
		{
			disposed = true;
		}
		if (ResultSetProcessor != null)
		{
			ResultSetProcessor.Dispose();
			ResultSetProcessor = null;
		}
		base.Dispose(disposing);
	}

	public string ObjectID()
	{
		return ObjectID(useCaption: true);
	}

	public string ObjectID(bool useCaption)
	{
		if (useCaption)
		{
			return GetCaption(withPrefix: true);
		}
		return string.Format(CultureInfo.InvariantCulture, "{0} {1}", IteratorType, ObjectId.ObjectNumber);
	}

	public void SetTableView(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		for (int i = 0; i < dataItems.Count; i++)
		{
			DataItem dataItem = dataItems[i];
			if (dataItem.Record.TableID == record.TableID)
			{
				SetTableViewUsed = true;
				TableViewIsSet[i] = true;
				dataItem.TableViewRecord.SecurityFiltering = dataItem.Record.SecurityFiltering;
				dataItem.TableViewRecord.RecordImplementation.CopyFiltersSortingMarkList(record);
				ApplyDataItemTableViewAndRequestFormFilters();
				return;
			}
		}
		if (IteratorType == DataItemIteratorType.Query)
		{
			throw new NavNCLQueryNoDataItemException(Metadata.Name, record.TableID, record.ALTableName);
		}
		throw new NavNCLReportNoDataItemException(Metadata.Name, record.TableID, record.ALTableName);
	}

	protected void ApplySetTableViewForAllDataItems()
	{
		ApplyDataItemTableViewAndRequestFormFilters();
		dataItems.ForEach((DataItem item) =>
		{
			SetDataItemTableView(item, FilterGroupType.TableViewFilters);
		});
	}

	protected void StoreSetTableViewForAllDataItems()
	{
		dataItems.ForEach((DataItem item) =>
		{
			item.TableViewRecord.RecordImplementation.CopyFiltersSortingMarkList(item.Record);
		});
	}
}
