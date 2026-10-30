using System;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class DataItem : TreeObject
{
	private NavRecord record;

	private NavRecord tableViewRecord;

	private NavTrigger onPreDataItem;

	private NavTrigger onAfterGetRecord;

	private NavTrigger onPostDataItem;

	private DataItem parent;

	private List<DataItem> children = new List<DataItem>();

	private List<MetaReportLabel> labels = new List<MetaReportLabel>();

	private MetaDataItem metadata;

	private bool printOnlyIfDetail;

	private bool skipRecord;

	private bool breakDataItem;

	private bool alreadyAddedRow;

	private FilterFieldDictionary dataItemTableViewFieldFilters;

	private RecordDataItemLink recordDataItemLink;

	public bool LabelsProcessed { get; set; }

	internal int[] LabelDataSetIndexes { get; set; }

	public bool HasColumns
	{
		get
		{
			if (MetaData.DataItemColumns.Count <= 0)
			{
				return Labels.Count > 0;
			}
			return true;
		}
	}

	public NavTrigger OnPreDataItem
	{
		get
		{
			return onPreDataItem;
		}
		set
		{
			onPreDataItem = value;
		}
	}

	public NavTrigger OnAfterGetRecord
	{
		get
		{
			return onAfterGetRecord;
		}
		set
		{
			onAfterGetRecord = value;
		}
	}

	public NavTrigger OnPostDataItem
	{
		get
		{
			return onPostDataItem;
		}
		set
		{
			onPostDataItem = value;
		}
	}

	internal List<MetaReportLabel> Labels => labels;

	public string DefaultRecordView { get; private set; }

	public bool IsRowCalculated { get; set; }

	internal MetaDataItem MetaData
	{
		get
		{
			return metadata;
		}
		set
		{
			metadata = value;
		}
	}

	internal NavRecord Record => record;

	internal bool HasChildren => children.Count > 0;

	internal DataItem Parent
	{
		get
		{
			return parent;
		}
		set
		{
			parent = value;
		}
	}

	internal List<DataItem> Children => children;

	internal bool PrintOnlyIfDetail
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

	internal bool SkipRecord
	{
		get
		{
			return skipRecord;
		}
		set
		{
			skipRecord = value;
		}
	}

	internal bool BreakDataItem
	{
		get
		{
			return breakDataItem;
		}
		set
		{
			breakDataItem = value;
		}
	}

	internal bool AlreadyAddedRow
	{
		get
		{
			return alreadyAddedRow;
		}
		set
		{
			alreadyAddedRow = value;
		}
	}

	internal NavRecord TableViewRecord => tableViewRecord;

	public override int GetHashCode()
	{
		return metadata.DataItemVarName.GetHashCode() ^ metadata.DataItemTable;
	}

	public DataItem(ITreeObject parent, NavRecordHandle record)
		: base(parent)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		this.record = record.Target;
		tableViewRecord = record.Target.Clone(this);
		DefaultRecordView = this.record.ALGetView(useCaptions: false);
	}

	private int CheckSourceExpressionForField(string sourceExpression)
	{
		string fieldNameFromSourceExpression = GetFieldNameFromSourceExpression(MetaData.DataItemVarName, sourceExpression);
		if (Record.MetaTable.TryGetFieldByName(fieldNameFromSourceExpression, out var result))
		{
			return result.FieldNo;
		}
		return 0;
	}

	private static string GetFieldNameFromSourceExpression(string dataItemVarName, string sourceExpression)
	{
		if (!sourceExpression.Contains("."))
		{
			return sourceExpression;
		}
		if (!sourceExpression.Contains(dataItemVarName))
		{
			return sourceExpression;
		}
		if (sourceExpression.StartsWith(dataItemVarName + ".", StringComparison.OrdinalIgnoreCase))
		{
			return sourceExpression.Substring(dataItemVarName.Length + 1);
		}
		if (sourceExpression.StartsWith("\"" + dataItemVarName + "\".", StringComparison.OrdinalIgnoreCase))
		{
			return sourceExpression.Substring(dataItemVarName.Length + 3);
		}
		return sourceExpression;
	}

	public bool FirstRecord()
	{
		IsRowCalculated = false;
		return Record.ALFind(DataError.TrapError, "-");
	}

	public bool NextRecord()
	{
		IsRowCalculated = false;
		return Record.NextRecord();
	}

	private List<int> FindCalcFields()
	{
		List<int> list = new List<int>();
		IReadOnlyCollection<int> readOnlyCollection = MetaData.CalcFields();
		if (readOnlyCollection != null && readOnlyCollection.Count > 0)
		{
			list.AddRange(readOnlyCollection);
		}
		foreach (MetaDataItemColumn dataItemColumn in MetaData.DataItemColumns)
		{
			if (string.IsNullOrEmpty(dataItemColumn.SourceExpression))
			{
				continue;
			}
			int num = CheckSourceExpressionForField(dataItemColumn.SourceExpression);
			if (num > 0 && dataItemColumn.AutoCalcField)
			{
				NCLMetaField fieldByNo = Record.MetaTable.GetFieldByNo(num);
				if ((fieldByNo.FieldClass == FieldClass.FlowField || fieldByNo.FieldNclType == NavNclType.NavBlob) && !list.Contains(num))
				{
					list.Add(num);
				}
			}
		}
		return list;
	}

	internal void SetDataItemTableViewFieldFilters()
	{
		if (dataItemTableViewFieldFilters == null)
		{
			TableViewResolver tableViewResolver = new TableViewResolver(Record.MetaTable, MetaData.DataItemTableView);
			TableViewParser.ParseTableView(MetaData.DataItemTableView, tableViewResolver);
			IReadOnlyList<Tuple<INavFieldMetadata, FilterExpression>> filters = tableViewResolver.Filters;
			dataItemTableViewFieldFilters = ((filters.Count == 0) ? FilterFieldDictionary.Empty : new FilterFieldDictionary(filters));
		}
		Record.RecordImplementation.AddFiltersToFilterGroup(dataItemTableViewFieldFilters);
	}

	internal void SetAutoCalcFields()
	{
		int[] calcFields = FindCalcFields().ToArray();
		record.SetAutoCalcFields(DataError.ThrowError, calcFields, replace: false);
		tableViewRecord.SetAutoCalcFields(DataError.ThrowError, calcFields, replace: false);
	}

	internal void SetRecordDataItemLink(NavRecord fromRecord)
	{
		if (recordDataItemLink == null)
		{
			recordDataItemLink = new RecordDataItemLink(fromRecord.MetaTable, Record.MetaTable, MetaData.DataItemLink);
		}
		if (!string.IsNullOrEmpty(recordDataItemLink.Link) && recordDataItemLink.GetFilterFieldDictionary().Items.Length != 0)
		{
			Record.SetRecordDataItemLink(fromRecord, recordDataItemLink);
		}
	}
}
