using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerDisplay("{metadata.NodeName} : {metadata.NodeType} -> {metadata.SourceType} - Table: {record.MetaTable.TableName}/{record.MetaTable.TableId}")]
public class NavXmlPortTableNode : NavXmlPortNode
{
	private NavRecord record;

	private NavRecord sourceTableViewRecord;

	private NavXmlPortTableNode linkTableNode;

	private List<NCLMetaField> fieldsForUpdate = new List<NCLMetaField>();

	private RecordDataItemLink recordDataItemLink;

	private FilterFieldDictionary dataItemTableViewFieldFilters;

	private NavTrigger onPreXmlItem;

	private NavTrigger onAfterGetRecord;

	private NavTrigger onAfterInitRecord;

	private NavTrigger onBeforeInsertRecord;

	private NavTrigger onAfterInsertRecord;

	private NavTrigger onBeforeModifyRecord;

	private NavTrigger onAfterModifyRecord;

	public NavTrigger OnPreXmlItem
	{
		get
		{
			return onPreXmlItem;
		}
		set
		{
			onPreXmlItem = value;
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

	public NavTrigger OnAfterInitRecord
	{
		get
		{
			return onAfterInitRecord;
		}
		set
		{
			onAfterInitRecord = value;
		}
	}

	public NavTrigger OnBeforeInsertRecord
	{
		get
		{
			return onBeforeInsertRecord;
		}
		set
		{
			onBeforeInsertRecord = value;
		}
	}

	public NavTrigger OnAfterInsertRecord
	{
		get
		{
			return onAfterInsertRecord;
		}
		set
		{
			onAfterInsertRecord = value;
		}
	}

	public NavTrigger OnBeforeModifyRecord
	{
		get
		{
			return onBeforeModifyRecord;
		}
		set
		{
			onBeforeModifyRecord = value;
		}
	}

	public NavTrigger OnAfterModifyRecord
	{
		get
		{
			return onAfterModifyRecord;
		}
		set
		{
			onAfterModifyRecord = value;
		}
	}

	internal NavRecord Record => record;

	internal NavRecord SourceTableViewRecord => sourceTableViewRecord;

	internal NavXmlPortTableNode LinkTableNode
	{
		get
		{
			return linkTableNode;
		}
		set
		{
			linkTableNode = value;
		}
	}

	private NavRecord CreateReferenceRecord()
	{
		if (Record.IsTemporary)
		{
			return new NavRecord(Record, Record.TableID, Record.GetNCLMetaTable(), isTemporary: true, Record, Record.CompanyName, SecurityFiltering.Filtered);
		}
		return new NavRecord(Record.ParentSession, Record.TableID);
	}

	internal void SetRecordDataItemLink()
	{
		if (recordDataItemLink == null)
		{
			recordDataItemLink = new RecordDataItemLink(LinkTableNode.Record.MetaTable, Record.MetaTable, ((MetaXmlPortTableNode)MetaData).LinkFields);
		}
		if (!string.IsNullOrEmpty(recordDataItemLink.Link) && recordDataItemLink.GetFilterFieldDictionary().Items.Length != 0)
		{
			Record.SetRecordDataItemLink(LinkTableNode.Record, recordDataItemLink);
		}
	}

	internal void SetDataItemTableViewFieldFilters()
	{
		if (dataItemTableViewFieldFilters == null)
		{
			string sourceTableView = ((MetaXmlPortTableNode)MetaData).SourceTableView;
			TableViewResolver tableViewResolver = new TableViewResolver(Record.MetaTable, sourceTableView);
			TableViewParser.ParseTableView(sourceTableView, tableViewResolver);
			IReadOnlyList<Tuple<INavFieldMetadata, FilterExpression>> filters = tableViewResolver.Filters;
			dataItemTableViewFieldFilters = ((filters.Count == 0) ? FilterFieldDictionary.Empty : new FilterFieldDictionary(filters));
		}
		Record.RecordImplementation.AddFiltersToFilterGroup(dataItemTableViewFieldFilters);
	}

	internal static NavControlStatement FireExportTrigger(NavTrigger trigger)
	{
		NavControlStatement result = NavControlStatement.None;
		if (trigger != null)
		{
			try
			{
				trigger();
			}
			catch (NavControlException ex)
			{
				result = ex.ControlStatement;
			}
		}
		return result;
	}

	internal static NavControlStatement FireImportTrigger(NavTrigger trigger)
	{
		NavControlStatement result = NavControlStatement.None;
		if (trigger != null)
		{
			try
			{
				trigger();
			}
			catch (NavControlException ex) when (ex.ControlStatement != NavControlStatement.Quit)
			{
				result = ex.ControlStatement;
			}
		}
		return result;
	}

	internal bool WriteRecord(bool alwaysModify)
	{
		MetaXmlPortTableNode metaXmlPortTableNode = MetaData as MetaXmlPortTableNode;
		bool flag = true;
		if (alwaysModify || metaXmlPortTableNode.AutoReplace || metaXmlPortTableNode.AutoUpdate)
		{
			using NavRecord navRecord = CreateReferenceRecord();
			if (navRecord.Get(DataError.TrapError, Record.ALRecordId))
			{
				if (metaXmlPortTableNode.AutoUpdate)
				{
					foreach (NCLMetaField item in fieldsForUpdate)
					{
						NCLMetaField fieldByNo = navRecord.MetaTable.GetFieldByNo(item.FieldNo);
						navRecord.SetFieldValue(fieldByNo, Record.GetFieldValue(item));
					}
					Record.ALTransferFields(navRecord, initPrimaryKeyFields: false);
				}
				if (NavXmlPortImporter.XmlPortNodeTriggerSwitch(this, FireImportTrigger, OnBeforeModifyRecord) != NavControlStatement.None)
				{
					return false;
				}
				if (metaXmlPortTableNode.AutoSave)
				{
					Record.ALModify(DataError.ThrowError, !Record.IsTemporary);
				}
				NavXmlPortImporter.XmlPortNodeTriggerSwitch(this, FireImportTrigger, OnAfterModifyRecord);
				flag = false;
			}
		}
		if (flag && RecordInitialized)
		{
			if (NavXmlPortImporter.XmlPortNodeTriggerSwitch(this, FireImportTrigger, OnBeforeInsertRecord) != NavControlStatement.None)
			{
				return false;
			}
			if (metaXmlPortTableNode.AutoSave)
			{
				Record.ALInsert(DataError.ThrowError, !Record.IsTemporary);
			}
			NavXmlPortImporter.XmlPortNodeTriggerSwitch(this, FireImportTrigger, OnAfterInsertRecord);
		}
		return true;
	}

	internal void Initialize()
	{
		Record.ALInit();
		RecordInitialized = true;
		fieldsForUpdate.Clear();
	}

	internal void AssignField(NCLMetaField field, NavValue value)
	{
		Record.SetFieldValue(field, value);
		if ((MetaData as MetaXmlPortTableNode).AutoUpdate && !field.FieldIsPartOfPrimaryKey)
		{
			fieldsForUpdate.Add(field);
		}
	}

	public NavXmlPortTableNode(NavRecordHandle record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		this.record = record.Target;
		sourceTableViewRecord = record.Target.Clone(record);
	}
}
