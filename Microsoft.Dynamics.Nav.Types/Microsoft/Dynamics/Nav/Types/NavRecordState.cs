using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Dynamics.Nav.Common;
using Microsoft.Dynamics.Nav.Types.Data;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Types;

[DataContract(Namespace = "http://www.microsoft.com/Dynamics/NAV/51")]
public class NavRecordState : ICloneable, IExtensibleDataObject
{
	private NavBaseException formOpenException;

	public virtual ExtensionDataObject ExtensionData { get; set; }

	[DataMember]
	public bool RunFormOnRec { get; set; }

	[DataMember]
	public NavTableView TableView { get; set; }

	public int TableId
	{
		[DebuggerStepThrough]
		get
		{
			return TableView.TableId;
		}
		[DebuggerStepThrough]
		set
		{
			TableView.TableId = value;
		}
	}

	public bool Ascending
	{
		get
		{
			return TableView.Ascending;
		}
		set
		{
			TableView.Ascending = value;
		}
	}

	public NavFilterGroup[] CurrentFilters
	{
		get
		{
			return TableView.CurrentFilters;
		}
		set
		{
			TableView.CurrentFilters = value;
		}
	}

	public NavFilterGroup SearchFilter
	{
		get
		{
			return TableView.SearchFilter;
		}
		set
		{
			TableView.SearchFilter = value;
		}
	}

	[DataMember]
	public bool FlushDataCache { get; set; }

	[DataMember]
	public byte[] CurrentRecord { get; set; }

	[DataMember]
	public bool NavFormEditable { get; set; }

	[DataMember]
	public bool ValidateFieldsInOnNewRecord { get; set; }

	[DataMember]
	public byte[] InsertLowerBoundBookmark { get; set; }

	[DataMember]
	public byte[] InsertUpperBoundBookmark { get; set; }

	[DataMember]
	public bool AllSelected { get; set; }

	[DataMember]
	public byte[][] SelectedRecords { get; set; }

	[DataMember]
	public byte[][] NonSelectedRecords { get; set; }

	[DataMember]
	public Guid ServerFormHandle { get; set; }

	[DataMember]
	public int FormId { get; set; }

	[DataMember]
	public int ParentFormId { get; set; }

	public bool FormHasOpenException
	{
		[DebuggerStepThrough]
		get
		{
			return FormOpenException != null;
		}
	}

	[DataMember]
	private byte[] FormOpenExceptionData
	{
		get
		{
			if (formOpenException != null)
			{
				return ExceptionHandler.SerializeToByteArray(formOpenException);
			}
			return null;
		}
		set
		{
			if (value != null)
			{
				formOpenException = (NavBaseException)ExceptionHandler.DeserializeFromByteArray(value, typeof(NavBaseException));
			}
		}
	}

	public NavBaseException FormOpenException
	{
		[DebuggerStepThrough]
		get
		{
			return formOpenException;
		}
		set
		{
			if (value != null)
			{
				formOpenException = (NavBaseException)ExceptionHandler.CompatibleException(value);
			}
			else
			{
				formOpenException = null;
			}
		}
	}

	[DataMember]
	public string PersonalizationId { get; set; }

	[DataMember]
	public bool IsResourceDefinedForm { get; set; }

	[DataMember]
	public long Timeout { get; set; }

	[DataMember]
	public NavFormUpdateTypes FormUpdateRequest { get; set; }

	[DataMember]
	public NavFormUpdateParameters[] SubFormUpdateRequests { get; set; }

	[DataMember]
	public NavBookmarkChangeTracking[] Changes { get; set; }

	[DataMember]
	public string PageCaption { get; set; }

	[DataMember]
	public NavDataSet FormVariables { get; set; }

	[DataMember]
	public NavDataSet AutoKeyValues { get; set; }

	public int[] CurrentSortingFieldIds
	{
		get
		{
			return TableView.CurrentSortingFieldIds;
		}
		set
		{
			TableView.CurrentSortingFieldIds = value;
		}
	}

	[DataMember]
	public NavRecordOperationTypes RecordState { get; set; }

	public bool RecordInserted => (RecordState & NavRecordOperationTypes.Insert) != 0;

	public bool RecordModified => (RecordState & NavRecordOperationTypes.Modify) != 0;

	public bool RecordDeleted => (RecordState & NavRecordOperationTypes.Delete) != 0;

	public bool RecordRenamed => (RecordState & NavRecordOperationTypes.Rename) != 0;

	public bool RecordPositionChanged => (RecordState & NavRecordOperationTypes.Navigation) != 0;

	public bool RecordMadeDirtyByServer => (RecordState & NavRecordOperationTypes.Dirty) != 0;

	public bool RecordSaved
	{
		get
		{
			if (!RecordInserted && !RecordModified)
			{
				return (FormUpdateRequest & NavFormUpdateTypes.RecordSaved) != 0;
			}
			return true;
		}
	}

	public bool RecordExistsInDatabase => (RecordState & NavRecordOperationTypes.InDatabase) != 0;

	public bool RecordOutsideOfFilters => (RecordState & NavRecordOperationTypes.OutsideOfFilters) != 0;

	[DataMember]
	public bool ValidateRequired { get; set; }

	[DataMember]
	public bool ClientRecordDraft { get; set; }

	[DataMember]
	public RenamingMode RenamingMode { get; set; }

	[DataMember]
	public int CurrentFilterGroup { get; set; }

	[DataMember]
	public bool IsSubFormUpdateRequest { get; set; }

	[DataMember]
	public bool MoreDataInReadDirection { get; set; }

	[DataMember]
	public bool MoreDataInOppositeDirection { get; set; }

	[DataMember]
	public bool UpdatePropagation { get; set; }

	[DataMember]
	public FormSelectionState[] SubFormSelectionStates { get; set; }

	[DataMember]
	public bool RecordTemporary { get; set; }

	[DataMember]
	public DataSourceType DataSourceType { get; set; }

	public NavRecordState()
	{
		ServerFormHandle = Guid.Empty;
		InitializeDefaultMembers();
	}

	public object Clone()
	{
		NavRecordState navRecordState = new NavRecordState
		{
			TableView = (NavTableView)TableView.Clone(),
			CurrentRecord = ((CurrentRecord != null) ? new byte[CurrentRecord.Length] : null),
			DataSourceType = DataSourceType
		};
		if (CurrentRecord != null)
		{
			Array.Copy(CurrentRecord, navRecordState.CurrentRecord, CurrentRecord.Length);
		}
		if (InsertLowerBoundBookmark != null)
		{
			navRecordState.InsertLowerBoundBookmark = new byte[InsertLowerBoundBookmark.Length];
			Array.Copy(InsertLowerBoundBookmark, navRecordState.InsertLowerBoundBookmark, InsertLowerBoundBookmark.Length);
		}
		if (InsertUpperBoundBookmark != null)
		{
			navRecordState.InsertUpperBoundBookmark = new byte[InsertUpperBoundBookmark.Length];
			Array.Copy(InsertUpperBoundBookmark, navRecordState.InsertUpperBoundBookmark, InsertUpperBoundBookmark.Length);
		}
		if (AutoKeyValues != null)
		{
			navRecordState.AutoKeyValues = AutoKeyValues.Clone();
		}
		navRecordState.SelectedRecords = new byte[SelectedRecords.Length][];
		for (int i = 0; i < SelectedRecords.Length; i++)
		{
			navRecordState.SelectedRecords[i] = new byte[SelectedRecords[i].Length];
			Array.Copy(SelectedRecords[i], navRecordState.SelectedRecords[i], SelectedRecords[i].Length);
		}
		if (SubFormUpdateRequests != null)
		{
			navRecordState.SubFormUpdateRequests = new NavFormUpdateParameters[SubFormUpdateRequests.Length];
			Array.Copy(SubFormUpdateRequests, navRecordState.SubFormUpdateRequests, SubFormUpdateRequests.Length);
		}
		navRecordState.Changes = ((Changes == null) ? null : ((NavBookmarkChangeTracking[])Changes.Clone()));
		return navRecordState;
	}

	private void InitializeDefaultMembers()
	{
		TableView = new NavTableView();
		SelectedRecords = new byte[0][];
		Ascending = true;
		ValidateRequired = true;
		NavFormEditable = true;
		ValidateFieldsInOnNewRecord = true;
		DataSourceType = DataSourceType.Table;
	}
}
