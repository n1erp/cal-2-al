using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Dynamics.Internal.Nav;
using Microsoft.Dynamics.Nav.Apps.MetadataDeltas;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavForm : NavApplicationObjectBase, ITreeSharedObject, ITreeObject
{
	private class NavFormUIPart
	{
		public int FormId { get; set; }

		public NavFormHandle FormHandle { get; set; }
	}

	private struct NavSourceObject
	{
		public int SourceObjectId { get; set; }

		public NavRecord SourceObject { get; set; }

		public DataSourceType SourceObjectType { get; set; }
	}

	public enum Action
	{
		None,
		OK,
		Cancel,
		LookupOK,
		LookupCancel,
		Yes,
		No,
		Close,
		FormHelp,
		RunObject,
		RunSystem
	}

	private class ExtensionTriggerMapKey : IEquatable<ExtensionTriggerMapKey>
	{
		public string TargetName { get; set; }

		public string ExtensionTriggerName { get; set; }

		public ExtensionTriggerMapKey(string targetName, string extensionTriggerName)
		{
			TargetName = targetName;
			ExtensionTriggerName = extensionTriggerName;
		}

		public override bool Equals(object obj)
		{
			ExtensionTriggerMapKey other = obj as ExtensionTriggerMapKey;
			if (obj == null)
			{
				return false;
			}
			return Equals(other);
		}

		public bool Equals(ExtensionTriggerMapKey other)
		{
			if (string.Equals(TargetName, other.TargetName, StringComparison.Ordinal))
			{
				return string.Equals(ExtensionTriggerName, other.ExtensionTriggerName, StringComparison.Ordinal);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(TargetName.GetHashCode(), ExtensionTriggerName.GetHashCode());
		}
	}

	private class ExtensionTriggerMapValue : IEquatable<ExtensionTriggerMapValue>
	{
		public NavApplicationObjectBase NavAppObjectBase { get; set; }

		public string MethodName { get; set; }

		public ExtensionTriggerMapValue(NavApplicationObjectBase navApplicationObjectBase, string methodName)
		{
			NavAppObjectBase = navApplicationObjectBase;
			MethodName = methodName;
		}

		public override bool Equals(object obj)
		{
			ExtensionTriggerMapValue other = obj as ExtensionTriggerMapValue;
			if (obj == null)
			{
				return false;
			}
			return Equals(other);
		}

		public bool Equals(ExtensionTriggerMapValue other)
		{
			if (object.Equals(NavAppObjectBase, other.NavAppObjectBase))
			{
				return string.Equals(MethodName, other.MethodName);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCodeHelper.CombineHashCodes(NavAppObjectBase.GetHashCode(), MethodName.GetHashCode());
		}
	}

	private bool hasRun;

	private readonly int formId;

	private string formName;

	private NavSourceObject navSourceObject;

	private readonly Guid handle;

	private readonly IDictionary<string, NavFormSourceExpression> sourceExpressions = new Dictionary<string, NavFormSourceExpression>();

	private readonly IDictionary<string, NavFormUIPart> uiParts = new Dictionary<string, NavFormUIPart>();

	private IList<byte[]> selections = new List<byte[]>();

	private MasterPage masterPage;

	private bool editable;

	private string pageCaption = string.Empty;

	private BookmarkType bookmarkType;

	private NavRecord tempTable;

	private int[] calculatedFields;

	private ReadOnlyCollection<string> invisibleControls;

	private NavFormMetadataHelper metadataHelper;

	private bool saveValues;

	private bool metadataLoaded;

	private bool masterpageReloaded;

	private bool disableUpdateAllowedOperationsFromPermissions;

	private bool? isRequestPage;

	private string personalizationId;

	private bool includeInvisibleCalcFields;

	private ReportRequestOptions reportRequestOptions;

	private NCLMetaForm nclMetaForm;

	private readonly IDictionary<string, NavApplicationObjectBase> triggerMap = new Dictionary<string, NavApplicationObjectBase>();

	private readonly IDictionary<ExtensionTriggerMapKey, List<ExtensionTriggerMapValue>> extensionOnlyTriggerMap = new Dictionary<ExtensionTriggerMapKey, List<ExtensionTriggerMapValue>>();

	private NCLMetaTable nclMetaTable;

	public const int AutoSplitKeyIncrement = 10000;

	private NavText parameterSet = NavText.Empty;

	public bool Active
	{
		get
		{
			throw new NavNCLNotSupportedMethodOnPageException("ACTIVE");
		}
	}

	public NavFormMetadataHelper MetadataHelper
	{
		get
		{
			if (metadataHelper == null || masterpageReloaded)
			{
				metadataHelper = new NavFormMetadataHelper(MasterPage);
				masterpageReloaded = false;
			}
			return metadataHelper;
		}
	}

	private bool NeedAutoSplitKey
	{
		get
		{
			if (MasterPage.PageProperties == null || MasterPage.PageProperties.SourceObject == null)
			{
				return false;
			}
			return MasterPage.PageProperties.SourceObject.AutoSplitKey;
		}
	}

	private NavRecord SafeSourceTable
	{
		get
		{
			if (SourceTable == null)
			{
				throw new NavNCLFormSourceTableException(Name, ObjectID());
			}
			return SourceTable;
		}
	}

	private NavRecord TempTable
	{
		get
		{
			if (SourceTable == null)
			{
				return null;
			}
			if (tempTable == null)
			{
				tempTable = NavGlobal.NCLMetadata.GetMetaTableById(navSourceObject.SourceObjectId).CreateObjectInstance(this, isTemporary: true, null, string.Empty, SecurityFiltering.Ignored);
			}
			return tempTable;
		}
	}

	private ViewDefinition SourceTableView
	{
		get
		{
			if (MasterPage.PageProperties != null)
			{
				if (MasterPage.PageProperties.SourceObject != null)
				{
					return MasterPage.PageProperties.SourceObject.SourceTableView;
				}
				return null;
			}
			return null;
		}
	}

	protected NCLMetaTable QueryMetaTable
	{
		get
		{
			if (MasterPage.PageProperties.SourceObject.PageDataSource != DataSourceType.Query)
			{
				return null;
			}
			return nclMetaTable;
		}
	}

	protected RepeaterDefinition Repeater => masterPage.ContentArea.Find((ElementDefinition elementDefinition) => elementDefinition is RepeaterDefinition) as RepeaterDefinition;

	public NavRecordId InsertLowerBoundBookmark { get; set; }

	public NavRecordId InsertUpperBoundBookmark { get; set; }

	public object AutoKeyValue { get; set; }

	public MasterPage MasterPage
	{
		get
		{
			EnsureMetadataLoaded();
			return masterPage;
		}
	}

	public int FormId => formId;

	public ReportRequestOptions ReportRequestOptions => reportRequestOptions ?? (reportRequestOptions = new ReportRequestOptions());

	public bool ParametersOnly { get; set; }

	public bool SaveValues
	{
		get
		{
			EnsureMetadataLoaded();
			return saveValues;
		}
		set
		{
			EnsureMetadataLoaded();
			saveValues = value;
		}
	}

	public string Name
	{
		get
		{
			EnsureMetadataLoaded();
			return formName;
		}
	}

	public NCLMetaForm NclMetaForm
	{
		get
		{
			if (nclMetaForm == null)
			{
				nclMetaForm = Session.NCLMetadata.GetMetaFormById(ObjectId.ObjectNumber, OriginalAppGroupId);
			}
			return nclMetaForm;
		}
	}

	public Guid Handle => handle;

	public IDictionary<string, NavFormSourceExpression> SourceExpressions
	{
		get
		{
			EnsureMetadataLoaded();
			return sourceExpressions;
		}
	}

	public bool AllSelected { get; set; }

	public IList<byte[]> Selections
	{
		get
		{
			return selections;
		}
		set
		{
			selections = value;
		}
	}

	public NavRecord SourceTable
	{
		get
		{
			if (navSourceObject.SourceObject == null)
			{
				EnsureMetadataLoaded();
			}
			return navSourceObject.SourceObject;
		}
	}

	public virtual bool IsRequestPage
	{
		get
		{
			if (!isRequestPage.HasValue)
			{
				isRequestPage = GetType().Name == "RequestPage";
			}
			return isRequestPage ?? false;
		}
	}

	public bool HasOpenedDialogs
	{
		get
		{
			if (OpenDialogTracking != null)
			{
				return OpenDialogTracking.OpenedDialogsCount > 0;
			}
			return false;
		}
	}

	internal override bool ExecutePermissionsValidated
	{
		get
		{
			if (!IsRequestPage)
			{
				return base.ExecutePermissionsValidated;
			}
			return true;
		}
	}

	internal NavOpenDialogTracking OpenDialogTracking { get; private set; }

	public NavRecord OldRecord => SafeSourceTable.OldRecord;

	public bool Editable
	{
		get
		{
			EnsureMetadataLoaded();
			return editable;
		}
		set
		{
			EnsureMetadataLoaded();
			editable = value;
		}
	}

	public NavFormUpdatePropagation UpdatePropagation { get; set; }

	public bool InsertAllowed => masterPage.PageProperties?.SourceObject?.InsertAllowed == true;

	public bool ModifyAllowed => masterPage.PageProperties?.SourceObject?.ModifyAllowed == true;

	public bool DeleteAllowed => masterPage.PageProperties?.SourceObject?.DeleteAllowed == true;

	public bool IsAbleToOverwriteAllowedProperties
	{
		get
		{
			switch (masterPage.PageProperties?.PageType)
			{
			case PageType.Card:
			case PageType.List:
			case PageType.CardPart:
			case PageType.ListPart:
			case PageType.Document:
			case PageType.ListPlus:
				return true;
			default:
				return false;
			}
		}
	}

	public bool SetTableViewUsed { get; internal set; }

	public bool[] TableViewIsSet { get; internal set; }

	public bool LookupMode { get; set; }

	public bool ValidateFieldsInOnNewRecord { get; set; }

	private bool IsFormInitialized { get; set; }

	private bool IsOpen { get; set; }

	public string PageCaption
	{
		get
		{
			EnsureMetadataLoaded();
			return pageCaption;
		}
		set
		{
			EnsureMetadataLoaded();
			pageCaption = value;
		}
	}

	public RecordBookmark Bookmark
	{
		get
		{
			if (SourceTable != null)
			{
				return new RecordBookmark(SourceTable.ALRecordId.GetBytes());
			}
			return null;
		}
	}

	public BookmarkType BookmarkType
	{
		get
		{
			return bookmarkType;
		}
		protected set
		{
			bookmarkType = value;
		}
	}

	public ReadOnlyCollection<string> InvisibleControls
	{
		get
		{
			EnsureMetadataLoaded();
			return invisibleControls;
		}
	}

	public List<string> RemovedControls
	{
		get
		{
			EnsureMetadataLoaded();
			return masterPage.RemovedControls;
		}
	}

	public ICollection<NavFormExtension> PageExtensions { get; private set; }

	internal string MetadataXml
	{
		get
		{
			if (MasterPage == null)
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true
			};
			XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, settings);
			XmlSerializer xmlSerializer = new XmlSerializer(MasterPage.GetType());
			xmlSerializer.Serialize(xmlWriter, MasterPage);
			return stringBuilder.ToString();
		}
	}

	internal static NavText LatestSavedSettings => NavText.Create(Lang.RequestPageLatestSavedSettingsName);

	internal NavText ParameterSet
	{
		get
		{
			if (SaveValues && parameterSet == NavText.Empty)
			{
				parameterSet = LatestSavedSettings;
				if (FeatureSwitches.Instance.DisableReportSettingsEdit)
				{
					IsLatestSavedSettings = NavBoolean.True;
				}
			}
			return parameterSet;
		}
		set
		{
			parameterSet = value;
			if (FeatureSwitches.Instance.DisableReportSettingsEdit)
			{
				IsLatestSavedSettings = NavBoolean.Create(value == LatestSavedSettings);
			}
		}
	}

	internal NavBoolean IsLatestSavedSettings { get; set; }

	public override NavNclType NclType => NavNclType.NavForm;

	public override int GetBytesSize => 0;

	internal override NCLMetaApplicationObject MetaApplicationObject => NclMetaForm;

	public override Type HandleType => typeof(NavFormHandle);

	public override string ObjectName => masterPage?.Name ?? NclMetaForm.Name;

	public event EventHandler<NavRecordEventArgs> SourceTableOperationsNotify;

	public event EventHandler<UpdateRequestEventArgs> UpdateRequest;

	public event EventHandler<Dictionary<string, NavValue>> OnSaveValues;

	public event EventHandler<EventArgs<string>> ApplyReportOptions;

	public event EventHandler<GetReportOptionsEventArgs> GetReportOptions;

	public int XPos()
	{
		throw new NavNCLNotSupportedMethodOnPageException("XPOS");
	}

	public int XPos(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("XPOS");
	}

	public int YPos()
	{
		throw new NavNCLNotSupportedMethodOnPageException("YPOS");
	}

	public int YPos(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("YPOS");
	}

	public int Width()
	{
		throw new NavNCLNotSupportedMethodOnPageException("WIDTH");
	}

	public int Width(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("WIDTH");
	}

	public int Height()
	{
		throw new NavNCLNotSupportedMethodOnPageException("HEIGHT");
	}

	public int Height(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("HEIGHT");
	}

	public int LogWidth()
	{
		throw new NavNCLNotSupportedMethodOnPageException("LOGWIDTH");
	}

	public int LogWidth(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("LOGWIDTH");
	}

	public int LogHeight()
	{
		throw new NavNCLNotSupportedMethodOnPageException("LOGHEIGHT");
	}

	public int LogHeight(int pos)
	{
		throw new NavNCLNotSupportedMethodOnPageException("LOGHEIGHT");
	}

	public bool Visible()
	{
		throw new NavNCLNotSupportedMethodOnPageException("VISIBLE");
	}

	public bool Visible(bool newValue)
	{
		throw new NavNCLNotSupportedMethodOnPageException("VISIBLE");
	}

	public bool MinimizedOnOpen()
	{
		throw new NavNCLNotSupportedMethodOnPageException("MINIMIZEDONOPEN");
	}

	public bool MinimizedOnOpen(bool newValue)
	{
		throw new NavNCLNotSupportedMethodOnPageException("MINIMIZEDONOPEN");
	}

	public bool MaximizedOnOpen()
	{
		throw new NavNCLNotSupportedMethodOnPageException("MAXIMIZEDONOPEN");
	}

	public bool MaximizedOnOpen(bool newValue)
	{
		throw new NavNCLNotSupportedMethodOnPageException("MAXIMIZEDONOPEN");
	}

	public bool InlineEditing()
	{
		throw new NavNCLNotSupportedMethodOnPageException("INLINEEDITING");
	}

	public bool InlineEditing(bool newValue)
	{
		throw new NavNCLNotSupportedMethodOnPageException("INLINEEDITING");
	}

	public string Language()
	{
		throw new NavNCLNotSupportedMethodOnPageException("LANGUAGE");
	}

	public string Language(string newValue)
	{
		throw new NavNCLNotSupportedMethodOnPageException("LANGUAGE");
	}

	public Uri Url()
	{
		throw new NavNCLNotSupportedMethodOnPageException("Url");
	}

	public Uri Url(bool useNames)
	{
		throw new NavNCLNotSupportedMethodOnPageException("Url");
	}

	public void UpdateControls()
	{
		throw new NavNCLNotSupportedMethodOnPageException("UPDATECONTROLS");
	}

	public FormResult RunModal()
	{
		return RunModal(null, 0);
	}

	public FormResult RunModal(NavRecord record)
	{
		return RunModal(record, 0);
	}

	public FormResult RunModal(NavRecord record, int fieldNo)
	{
		FormResult action = FormResult.None;
		if (record != null)
		{
			SetSourceTable(record, clone: true, forceSecurityFilterFiltered: true);
			if (bookmarkType != BookmarkType.Record)
			{
				SyncTempTableWithSourceTable(clearTempTable: true);
			}
		}
		NavFormRuntimeParameters parameters = new NavFormRuntimeParameters
		{
			LookupMode = LookupMode
		};
		ReportRequestOptions.CopyTo(parameters.ReportRequestOptions);
		if (record != null && fieldNo > 0)
		{
			parameters.FieldName = record.MetaTable.GetFieldByNo(fieldNo).FieldName;
		}
		if (NavCurrentThread.Session.TestExecution.TestHandleModalForm(this, ref action, parameters))
		{
			return action;
		}
		if (hasRun)
		{
			throw new NavNCLObjectHasAlreadyBeenRunException(CultureInfo.CurrentCulture, ObjectId);
		}
		hasRun = true;
		NavCurrentThread.Session.BeginTransactionWorld();
		try
		{
			if (OpenDialogTracking == null)
			{
				OpenDialogTracking = new NavOpenDialogTracking();
			}
			IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
			NavCurrentThread.Session.Company.RegisterForm(this);
			return clientCallback.FormRunModal(this, parameters);
		}
		catch (NavNCLCallbackNotAllowedException)
		{
			NavCurrentThread.Session.Company.UnregisterForm(this);
			throw;
		}
		finally
		{
			NavCurrentThread.Session.EndTransactionWorld();
		}
	}

	public static FormResult RunModal(bool isInLookupTrigger, bool isLookup, int formId)
	{
		return RunModal(isInLookupTrigger, isLookup, formId, null, 0);
	}

	public static FormResult RunModal(bool isInLookupTrigger, bool isLookup, int formId, NavRecord record)
	{
		return RunModal(isInLookupTrigger, isLookup, formId, record, 0);
	}

	public static FormResult RunModal(bool isInLookupTrigger, bool isLookup, int formId, NavRecord record, NavFieldRef fieldRef)
	{
		return RunModal(isInLookupTrigger, isLookup, formId, record, (!fieldRef.IsNull) ? fieldRef.ALNumber : 0);
	}

	public static FormResult RunModal(bool isInLookupTrigger, bool isLookup, int formId, NavRecord record, int fieldNo)
	{
		if (formId == 0 && record != null)
		{
			formId = record.LookupFormId;
		}
		NavSession navSession = NavCurrentThread.Session;
		using NavFormHandle navFormHandle = new NavFormHandle(navSession, navSession.NCLMetadata.GetMetaFormById(formId).CreateObjectInstance(record));
		NavForm target = navFormHandle.Target;
		target.LookupMode = isLookup;
		FormResult formResult = target.RunModal(record, fieldNo);
		if (((record != null) & isLookup) && formResult != FormResult.Cancel && (!isInLookupTrigger || formResult == FormResult.LookupOK))
		{
			record.Assign(target.SourceTable);
		}
		return formResult;
	}

	public void Run()
	{
		Run(null, 0);
	}

	public void Run(NavRecord record)
	{
		Run(record, 0);
	}

	public void Run(NavRecord record, int fieldNo)
	{
		if (record != null)
		{
			SetSourceTable(record, clone: true, forceSecurityFilterFiltered: true);
			if (bookmarkType != BookmarkType.Record)
			{
				SyncTempTableWithSourceTable(clearTempTable: true);
			}
		}
		NavFormRuntimeParameters parameters = new NavFormRuntimeParameters
		{
			LookupMode = LookupMode
		};
		if (record != null && fieldNo > 0)
		{
			parameters.FieldName = record.MetaTable.GetFieldByNo(fieldNo).FieldName;
		}
		if (hasRun)
		{
			throw new NavNCLObjectHasAlreadyBeenRunException(CultureInfo.CurrentCulture, ObjectId);
		}
		hasRun = true;
		if (NavCurrentThread.Session.TestExecution.TestHandleForm(this, parameters))
		{
			return;
		}
		IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
		NavCurrentThread.Session.Company.RegisterForm(this);
		try
		{
			clientCallback.FormRun(this, parameters);
		}
		catch (NavNCLCallbackNotAllowedException)
		{
			NavCurrentThread.Session.Company.UnregisterForm(this);
			throw;
		}
	}

	public static void Run(int formId)
	{
		Run(formId, null, 0);
	}

	public static void Run(int formId, NavRecord record)
	{
		Run(formId, record, 0);
	}

	public static void Run(int formId, NavRecord record, int fieldNo)
	{
		if (formId == 0 && record != null)
		{
			formId = record.LookupFormId;
		}
		using NavFormHandle navFormHandle = new NavFormHandle(NavCurrentThread.Session, NavGlobal.NCLMetadata.GetMetaFormById(formId).CreateObjectInstance(record));
		navFormHandle.Target.Run(record, fieldNo);
	}

	public void SaveRecord()
	{
		if (SourceTable == null)
		{
			return;
		}
		NavRecordId aLRecordId = SafeSourceTable.ALRecordId;
		NavRecordId aLRecordId2 = SafeSourceTable.OldRecord.ALRecordId;
		if (aLRecordId != aLRecordId2 && SafeSourceTable.OldRecord.HasBeenInserted)
		{
			SafeSourceTable.Rename(showDialog: true, DataError.ThrowError);
			return;
		}
		if (!SafeSourceTable.CompareAllNormalFields(SafeSourceTable.OldRecord))
		{
			if (!SafeSourceTable.HasBeenInserted)
			{
				bool belowXRec = InsertLowerBoundBookmark != null;
				SplitKey();
				if (RaiseOnInsertRecord(belowXRec))
				{
					SafeSourceTable.Insert(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
				}
			}
			else if (RaiseOnModifyRecord())
			{
				SafeSourceTable.Modify(DataError.ThrowError, runApplicationTrigger: true, runGlobalTrigger: true);
			}
		}
		if (UpdateRequest != null)
		{
			UpdateRequestEventArgs e = new UpdateRequestEventArgs(NavFormUpdateTypes.RecordSaved);
			UpdateRequest(this, e);
		}
	}

	public void GetRecord(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		record.Assign(SafeSourceTable);
	}

	public void SetRecord(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		SafeSourceTable.Assign(record);
	}

	public void Close()
	{
		if (IsOpen)
		{
			CloseOpenDialogs();
			if (NavCurrentThread.Session.TestExecution == null || !NavCurrentThread.Session.TestExecution.ClosePage(this))
			{
				NavCurrentThread.Session.ClientCallback.FormClose(this);
			}
		}
	}

	public void Update(bool saveRecord)
	{
		NavFormUpdateTypes navFormUpdateTypes = NavFormUpdateTypes.Update;
		if (UpdatePropagation == NavFormUpdatePropagation.Both)
		{
			navFormUpdateTypes |= NavFormUpdateTypes.UpdateParent;
		}
		UpdateCore(navFormUpdateTypes, saveRecord);
	}

	public void Update()
	{
		Update(SourceTable != null);
	}

	private void UpdateCore(NavFormUpdateTypes updateType, bool saveRecord)
	{
		if (saveRecord)
		{
			SaveRecord();
			updateType |= NavFormUpdateTypes.RecordSaved;
		}
		if (UpdateRequest != null)
		{
			UpdateRequestEventArgs e = new UpdateRequestEventArgs(updateType);
			UpdateRequest(this, e);
		}
	}

	public void SetSelectionFilter(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		if (record.TableID == 0)
		{
			throw new ArgumentOutOfRangeException("record");
		}
		if (SourceTable == null || SourceTable.TableID == 0)
		{
			return;
		}
		if (SourceTable.TableID != record.TableID)
		{
			throw new NavNCLTableIdMismatchException();
		}
		if (SourceTable == record && AllSelected && Selections.Count == 0)
		{
			return;
		}
		NavRecord navRecord = ((SourceTable == record) ? SourceTable.CloneRecord(SourceTable.Parent, reset: false, keepCompany: true) : SourceTable);
		record.RecordImplementation.ClearResultSetMarksAndFilters();
		if (AllSelected && navRecord.IsTemporary)
		{
			ConvertAllSelectedOnTemporaryRecordToSelection(navRecord);
		}
		if (!AllSelected && Selections.Count <= 1)
		{
			SetTargetFilterFromSourceRow(record, navRecord);
			return;
		}
		record.SetTableViewUnconditionally(navRecord.GetTableView());
		if (!AllSelected || Selections.Count > 0)
		{
			record.ClearAndSetMarks(Selections.ToArray(), AllSelected);
			record.ALMarkedOnly = true;
		}
		record.ALFilterGroup = navRecord.ALFilterGroup;
	}

	public bool Activate()
	{
		return Activate(refresh: false);
	}

	public bool Activate(bool refresh)
	{
		if (IsOpen)
		{
			if (NavCurrentThread.Session.TestExecution != null && NavCurrentThread.Session.TestExecution.ActivatePage(Handle, refresh))
			{
				return true;
			}
			Session.ClientCallback.FormActivate(this, refresh);
		}
		return true;
	}

	public bool EnqueueBackgroundTask(DataError errorLevel, ByRef<int> taskId, int objectId)
	{
		NavDictionary<NavText, NavText> parameters = NavDictionary<NavText, NavText>.Default;
		ByRef<NavDictionary<NavText, NavText>> parameters2 = new ByRef<NavDictionary<NavText, NavText>>(() => parameters, (NavDictionary<NavText, NavText> setValue) =>
		{
			parameters = setValue;
		});
		return EnqueueBackgroundTask(errorLevel, taskId, objectId, parameters2);
	}

	public bool EnqueueBackgroundTask(DataError errorLevel, ByRef<int> taskId, int objectId, ByRef<NavDictionary<NavText, NavText>> parameters)
	{
		return EnqueueBackgroundTask(errorLevel, taskId, objectId, parameters, cancelOnRecordChange: true);
	}

	public bool EnqueueBackgroundTask(DataError errorLevel, ByRef<int> taskId, int objectId, ByRef<NavDictionary<NavText, NavText>> parameters, bool cancelOnRecordChange)
	{
		return EnqueueBackgroundTask(errorLevel, taskId, objectId, parameters, cancelOnRecordChange, 120);
	}

	public bool EnqueueBackgroundTask(DataError errorLevel, ByRef<int> taskId, int objectId, ByRef<NavDictionary<NavText, NavText>> parameters, bool cancelOnRecordChange, int timeout)
	{
		try
		{
			PageBackgroundTask pageBackgroundTask = new PageBackgroundTask(NavCurrentThread.Session, this, SourceTable.ALRecordId, objectId, (NavSession s) => new NavCodeunitHandle(s, objectId), parameters.Value, cancelOnRecordChange, timeout);
			pageBackgroundTask.TaskCompletionTrigger = new PageBackgroundTaskCompletionImmediateTrigger(pageBackgroundTask);
			taskId.Value = pageBackgroundTask.TaskId;
			EnqueueBackgroundTask(pageBackgroundTask);
		}
		catch (Exception ex)
		{
			if (DataError.ThrowError == errorLevel || !(ex is NavBaseException))
			{
				throw;
			}
			return false;
		}
		return true;
	}

	internal static void EnqueueBackgroundTask(PageBackgroundTask task)
	{
		NavSession parentSession = NavCurrentThread.Session;
		NavChildSessionTaskRuntime navChildSessionTaskRuntime = new NavChildSessionTaskRuntime(task.ChildSessionTask, parentSession);
		if (PageBackgroundTask.CanPageBackgroundTaskRunAsync(parentSession))
		{
			NavEnvironment.Instance.ChildSessionTaskScheduler.ScheduleChildSessionTaskRun(navChildSessionTaskRuntime);
		}
		else
		{
			navChildSessionTaskRuntime.Run();
		}
	}

	public bool CancelBackgroundTask(int taskId)
	{
		PageBackgroundTask pageBackgroundTask = Tree.ChildOfTypeFirstOrDefault((PageBackgroundTask t) => t.TaskId == taskId);
		if (pageBackgroundTask == null)
		{
			return false;
		}
		pageBackgroundTask.Cancel();
		return true;
	}

	public static void SetBackgroundTaskResult(NavDictionary<NavText, NavText> results)
	{
		if (NavCurrentThread.Session.PageBackgroundTask == null)
		{
			throw new NavNCLInvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidOperationNotChildSession, "PAGE.SETBACKGROUNDTASKRESULT"));
		}
		NavCurrentThread.Session.PageBackgroundTask.Results = results;
	}

	public static NavDictionary<NavText, NavText> GetBackgroundParameters()
	{
		if (NavCurrentThread.Session.PageBackgroundTask == null)
		{
			throw new NavNCLInvalidOperationException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidOperationNotChildSession, "PAGE.GETBACKGROUNDPARAMETERS"));
		}
		return NavCurrentThread.Session.PageBackgroundTask.Parameters;
	}

	private void SetTargetFilterFromSourceRow(NavRecord targetRecord, NavRecord sourceRecord)
	{
		targetRecord.ALFilterGroup = 0;
		foreach (NCLMetaField keyFields in sourceRecord.MetaTable.GetKeyByIndex(0).KeyFieldsList)
		{
			targetRecord.SetFilter(keyFields, "%1", sourceRecord.GetFieldValue(keyFields.FieldNo));
		}
		targetRecord.ClearAndSetMarks(Selections.ToArray(), allExceptThese: false);
	}

	private void ConvertAllSelectedOnTemporaryRecordToSelection(NavRecord sourceRecord)
	{
		List<byte[]> list = new List<byte[]>();
		if (sourceRecord.ALFindSet(DataError.TrapError))
		{
			do
			{
				list.Add(sourceRecord.ALRecordId.Value);
			}
			while (sourceRecord.ALNext() != 0);
		}
		selections = list;
		AllSelected = false;
	}

	public void SetIncludeInvisibleCalcFields()
	{
		includeInvisibleCalcFields = true;
		if (navSourceObject.SourceObject != null)
		{
			SetFieldsFromControlsMetadata();
		}
	}

	private void SyncTempTableWithSourceTable(bool clearTempTable)
	{
		if (clearTempTable)
		{
			TempTable.RecordImplementation.DeleteAllRecords();
		}
		TempTable.Assign(SourceTable);
		if (TempTable.Exists(TempTable.ALRecordId))
		{
			TempTable.RecordImplementation.ModifyRecord(DataError.TrapError);
		}
		else
		{
			TempTable.RecordImplementation.InsertRecord(DataError.TrapError);
		}
	}

	protected virtual string GetAutoFormatString(int controlId)
	{
		throw new NotSupportedException(Lang._00095_50083);
	}

	protected virtual string GetDecimalPlaces(int controlId)
	{
		throw new NotSupportedException(Lang._00095_50083);
	}

	public virtual string EvaluateCaptionClass(int controlId)
	{
		throw new NotSupportedException(Lang._00095_50083);
	}

	private void RaiseOnInit()
	{
		OnInit();
	}

	protected virtual void OnInit()
	{
	}

	private void RaiseOnOpenPage()
	{
		OnOpenPage();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnOpenPage();
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnOpenPageEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnOpenPageEvent(this, navSourceObject.SourceObject);
			});
		});
	}

	protected virtual void OnOpenPage()
	{
	}

	private bool RaiseOnQueryClosePage(FormResult closeAction)
	{
		bool result = OnQueryClosePage(closeAction);
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			result &= ext.OnQueryClosePage(closeAction);
		});
		bool result2 = true;
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnQueryClosePageEvent, () =>
			{
				ByRef<bool> byRef = new ByRefInstance<bool>(initialValue: true);
				nclMetaForm.PageTriggerEventHandler.OnQueryClosePageEvent(this, navSourceObject.SourceObject, closeAction, byRef);
				result2 = byRef.Value;
			});
		});
		return result & result2;
	}

	protected virtual bool OnQueryClosePage(FormResult closeAction)
	{
		return true;
	}

	private void RaiseOnClosePage()
	{
		OnClosePage();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnClosePage();
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnClosePageEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnClosePageEvent(this, navSourceObject.SourceObject);
			});
		});
	}

	protected virtual void OnClosePage()
	{
	}

	private bool RaiseOnFindRecord(NavText which)
	{
		return OnFindRecord(which);
	}

	protected virtual bool OnFindRecord(NavText which)
	{
		return SafeSourceTable.ALFind(DataError.TrapError, which);
	}

	internal int RaiseOnNextRecord(int steps)
	{
		return OnNextRecord(steps);
	}

	protected virtual int OnNextRecord(int steps)
	{
		return SafeSourceTable.ALNext(steps);
	}

	internal void RaiseOnAfterGetRecord()
	{
		OnAfterGetRecord();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnAfterGetRecord();
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnAfterGetRecordEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnAfterGetRecordEvent(this, navSourceObject.SourceObject);
			});
		});
	}

	protected virtual void OnAfterGetRecord()
	{
	}

	private void RaiseOnAfterGetCurrRecord()
	{
		OnAfterGetCurrRecord();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnAfterGetCurrRecord();
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnAfterGetCurrRecordEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnAfterGetCurrRecordEvent(this, navSourceObject.SourceObject);
			});
		});
	}

	protected virtual void OnAfterGetCurrRecord()
	{
	}

	private void RaiseOnNewRecord(bool belowXRec)
	{
		OnNewRecord(belowXRec);
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnNewRecord(belowXRec);
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnNewRecordEvent, () =>
			{
				NavBoolean navBoolean = NavBoolean.Create(belowXRec);
				nclMetaForm.PageTriggerEventHandler.OnNewRecordEvent(this, navSourceObject.SourceObject, navBoolean, SourceTable?.OldRecord);
			});
		});
	}

	protected virtual void OnNewRecord(bool belowXRec)
	{
	}

	private bool RaiseOnInsertRecord(bool belowXRec)
	{
		bool triggerResult = OnInsertRecord(belowXRec);
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			triggerResult &= ext.OnInsertRecord(belowXRec);
		});
		bool eventResult = true;
		RaiseTriggerEvent(NavTriggerEventType.OnInsertRecordEvent, () =>
		{
			ByRef<bool> byRef = new ByRefInstance<bool>(initialValue: true);
			nclMetaForm.PageTriggerEventHandler.OnInsertRecordEvent(this, navSourceObject.SourceObject, belowXRec, SourceTable?.OldRecord, byRef);
			eventResult = byRef.Value;
		});
		return triggerResult & eventResult;
	}

	protected virtual bool OnInsertRecord(bool belowXRec)
	{
		return true;
	}

	private bool RaiseOnModifyRecord()
	{
		bool triggerResult = OnModifyRecord();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			triggerResult &= ext.OnModifyRecord();
		});
		bool eventResult = true;
		RaiseTriggerEvent(NavTriggerEventType.OnModifyRecordEvent, () =>
		{
			ByRef<bool> byRef = new ByRefInstance<bool>(initialValue: true);
			nclMetaForm.PageTriggerEventHandler.OnModifyRecordEvent(this, navSourceObject.SourceObject, SourceTable?.OldRecord, byRef);
			eventResult = byRef.Value;
		});
		return triggerResult & eventResult;
	}

	protected virtual bool OnModifyRecord()
	{
		return true;
	}

	private bool RaiseOnDeleteRecord()
	{
		bool triggerResult = OnDeleteRecord();
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			triggerResult &= ext.OnDeleteRecord();
		});
		bool eventResult = true;
		RaiseTriggerEvent(NavTriggerEventType.OnDeleteRecordEvent, () =>
		{
			ByRef<bool> byRef = new ByRefInstance<bool>(initialValue: true);
			nclMetaForm.PageTriggerEventHandler.OnDeleteRecordEvent(this, navSourceObject.SourceObject, byRef);
			eventResult = byRef.Value;
		});
		return triggerResult & eventResult;
	}

	protected virtual bool OnDeleteRecord()
	{
		return true;
	}

	public void RaiseOnValidate(int? controlId, string validateTriggerMethodName)
	{
		if (!controlId.HasValue)
		{
			throw new ArgumentNullException("controlId");
		}
		string eventId = controlId.Value.ToString(CultureInfo.InvariantCulture);
		string elementName = (Session.IsEventSessionRecorderEnabled ? MetadataHelper.GetControlDefinitionById(controlId.Value).Name : string.Empty);
		RaiseControlTriggerEvent(new NavTriggerEventIdentity(NavTriggerEventType.OnBeforeValidateEvent, eventId, elementName, FormId, ObjectType.Page));
		RaiseOnValidateExtensionTrigger(controlId.Value, (TriggerStateType)0);
		if (!string.IsNullOrEmpty(validateTriggerMethodName))
		{
			NavApplicationMethod.InvokeMethod(this, validateTriggerMethodName, null, resolveHandler: true, throwOnNotFound: false);
		}
		RaiseOnValidateExtensionTrigger(controlId.Value, (TriggerStateType)1);
		RaiseControlTriggerEvent(new NavTriggerEventIdentity(NavTriggerEventType.OnAfterValidateEvent, eventId, elementName, FormId, ObjectType.Page));
	}

	private void RaiseOnValidateExtensionTrigger(int controlId, TriggerStateType triggerState)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RaiseExtensionTriggers(controlId, triggerState, TriggerType.OnValidate, "ControlDefinition", null);
	}

	public void RaiseOnAction(int? actionId, string actionTriggerMethodName)
	{
		if (!actionId.HasValue)
		{
			throw new ArgumentNullException("actionId");
		}
		string eventId = actionId.Value.ToString(CultureInfo.InvariantCulture);
		string elementName = (Session.IsEventSessionRecorderEnabled ? MetadataHelper.GetActionDefinitionById(actionId.Value).Name : string.Empty);
		RaiseActionTriggerEvent(new NavTriggerEventIdentity(NavTriggerEventType.OnBeforeActionEvent, eventId, elementName, FormId, ObjectType.Page));
		RaiseOnActionExtensionTrigger(actionId.Value, (TriggerStateType)0);
		if (!string.IsNullOrEmpty(actionTriggerMethodName) && actionTriggerMethodName != "_a45_OnValidate")
		{
			NavApplicationMethod.InvokeMethod(this, actionTriggerMethodName, null, resolveHandler: true, throwOnNotFound: false);
		}
		RaiseOnActionExtensionTrigger(actionId.Value, (TriggerStateType)1);
		RaiseActionTriggerEvent(new NavTriggerEventIdentity(NavTriggerEventType.OnAfterActionEvent, eventId, elementName, FormId, ObjectType.Page));
	}

	private void RaiseOnActionExtensionTrigger(int actionId, TriggerStateType triggerState)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		RaiseExtensionTriggers(actionId, triggerState, TriggerType.OnAction, "ActionDefinition", null);
	}

	protected virtual void OnPageBackgroundTaskCompleted(int taskId, NavDictionary<NavText, NavText> results)
	{
	}

	internal void RaiseOnPageBackgroundTaskCompleted(int taskId, NavDictionary<NavText, NavText> results)
	{
		OnPageBackgroundTaskCompleted(taskId, results);
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnPageBackgroundTaskCompleted(taskId, results);
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnPageBackgroundTaskCompletedEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnPageBackgroundTaskCompletedEvent(this, taskId, results);
			});
		});
	}

	protected virtual void OnPageBackgroundTaskError(int taskId, NavText errorCode, NavText errorText, NavText errorCallStack)
	{
	}

	internal void RaiseOnPageBackgroundTaskError(int taskId, NavText errorCode, NavText errorText, NavText errorCallStack)
	{
		OnPageBackgroundTaskError(taskId, errorCode, errorText, errorCallStack);
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnPageBackgroundTaskError(taskId, errorCode, errorText, errorCallStack);
		});
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			RaiseTriggerEvent(NavTriggerEventType.OnPageBackgroundTaskCompletedEvent, () =>
			{
				nclMetaForm.PageTriggerEventHandler.OnPageBackgroundTaskErrorEvent(this, taskId, errorCode, errorText, errorCallStack);
			});
		});
	}

	internal override NavApplicationObjectBase ResolveTriggerHandler(string triggerMethodName, object[] args)
	{
		if (!triggerMap.ContainsKey(triggerMethodName))
		{
			MethodInfo methodInfo = null;
			foreach (NavFormExtension pageExtension in PageExtensions)
			{
				methodInfo = NavApplicationMethod.FindMethod(pageExtension, triggerMethodName, args, out var _);
				if (methodInfo != null)
				{
					triggerMap.Add(triggerMethodName, pageExtension);
					break;
				}
			}
			if (methodInfo == null)
			{
				triggerMap.Add(triggerMethodName, this);
			}
		}
		return triggerMap[triggerMethodName];
	}

	private void RaiseExtensionTriggers(int elementId, TriggerStateType triggerStateType, TriggerType triggerType, string objectDefinition, object[] args)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		string triggerExtensionName = TriggerNameHelper.GetTriggerExtensionName(triggerType, triggerStateType);
		List<ExtensionTriggerMapValue> list = ResolveExtenionTrigger(elementId, triggerExtensionName, objectDefinition, args);
		if (list.Count > 0)
		{
			list.ForEach((ExtensionTriggerMapValue handler) =>
			{
				NavApplicationMethod.InvokeMethod(handler.NavAppObjectBase, handler.MethodName, args, resolveHandler: false);
			});
		}
	}

	private List<ExtensionTriggerMapValue> ResolveExtenionTrigger(int elementId, string extensionTriggerName, string objectDefinition, object[] args)
	{
		ExtensionTriggerMapKey key = new ExtensionTriggerMapKey(objectDefinition + "_" + elementId.ToString(CultureInfo.InvariantCulture), extensionTriggerName);
		if (!extensionOnlyTriggerMap.ContainsKey(key))
		{
			List<ExtensionTriggerMapValue> list = new List<ExtensionTriggerMapValue>();
			foreach (NavFormExtension pageExtension in PageExtensions)
			{
				ContentChangeDelta val = ((NavAppObjectMetadataDeltaCreator<Delta>)(object)pageExtension.NclPageExtension.GetDeltas()).AllDeltas.OfType<ContentChangeDelta>().FirstOrDefault((ContentChangeDelta x) => x.Context.TargetControlId.ControlRuntimeId == elementId && string.Equals(x.ControlType, objectDefinition));
				if (val != null)
				{
					string methodName = (from x in ((ObjectChangeContext)val.Context).PropertyChanges
						where string.Equals(x.Key, extensionTriggerName)
						select x.Value.PropertyValue).FirstOrDefault();
					MethodInfo methodInfo = NavApplicationMethod.FindMethod(pageExtension, methodName, args, out var _);
					if (methodInfo != null)
					{
						list.Add(new ExtensionTriggerMapValue(pageExtension, methodName));
					}
				}
			}
			extensionOnlyTriggerMap.Add(key, list);
		}
		return extensionOnlyTriggerMap[key];
	}

	private void RaiseTriggerEvent(NavTriggerEventType eventType, System.Action action)
	{
		if (EventReadyToFire(eventType))
		{
			action();
		}
	}

	private void RaiseControlTriggerEvent(NavTriggerEventIdentity eventIdentity)
	{
		if (EventReadyToFire(eventIdentity))
		{
			ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
			{
				nclMetaForm.PageTriggerEventHandler.FireOnValidateEvent(eventIdentity, this, navSourceObject.SourceObject, SourceTable?.OldRecord);
			});
		}
	}

	private void RaiseActionTriggerEvent(NavTriggerEventIdentity eventIdentity)
	{
		if (EventReadyToFire(eventIdentity))
		{
			ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
			{
				nclMetaForm.PageTriggerEventHandler.FireOnActionEvent(eventIdentity, this, navSourceObject.SourceObject);
			});
		}
	}

	private bool EventReadyToFire(NavTriggerEventType eventType)
	{
		NavTriggerEventIdentity eventIdentity = new NavTriggerEventIdentity(eventType, null, null, FormId, ObjectType.Page);
		return EventReadyToFire(eventIdentity);
	}

	private bool EventReadyToFire(NavTriggerEventIdentity eventIdentity)
	{
		EnsureMetadataLoaded();
		if (nclMetaForm != null)
		{
			return nclMetaForm.IsEventSubscribed(eventIdentity);
		}
		return false;
	}

	protected virtual void OnMetadataLoaded()
	{
	}

	public void RegisterAutoFormatExpression(int controlId, int autoFormatType, Func<string> getAutoFormatStringFunc)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "Control{0}_Format", controlId);
		NavText autoFormatString = ((getAutoFormatStringFunc != null) ? new NavText(getAutoFormatStringFunc()) : NavText.Empty);
		RegisterSourceExpression(text, text, typeof(string), SourceExpressionType.Property, () => ALCompiler.ConvertToDotNetFormatString(Session.SystemCodeunitFactory.UIHelperTriggers.InvokeAutoFormatTranslate(autoFormatType, autoFormatString)), null);
	}

	public void RegisterGetDecimalStringExpression(int controlId, INavRecordHandle record, int fieldId)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "Control{0}_Format", controlId);
		RegisterSourceExpression(text, text, typeof(string), SourceExpressionType.Property, () => ALCompiler.ConvertToDotNetFormatString(GetDecimalString(record, fieldId, controlId)), null);
	}

	public void RegisterDynamicCaptionExpression(int controlId)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "Control{0}_DynamicCaption", controlId);
		RegisterSourceExpression(text, text, typeof(NavText), SourceExpressionType.DynamicCaption, () => GetDynamicCaption(controlId), null);
	}

	public void RegisterFormatStringExpression(int controlId, string formatString)
	{
		string text = string.Format(CultureInfo.InvariantCulture, "Control{0}_Format", controlId);
		RegisterSourceExpression(text, text, typeof(string), SourceExpressionType.Property, () => ALCompiler.ConvertToDotNetFormatString(formatString), null);
	}

	protected internal void RegisterSourceExpression(string id, string name, Type type, SourceExpressionType expressionType, NavFormSourceExpressionGetter get, NavFormSourceExpressionSetter set, bool skipWriting = false)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id");
		}
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (get == null)
		{
			throw new ArgumentNullException("get");
		}
		NavFormSourceExpression navFormSourceExpression = new NavFormSourceExpression(id, name, expressionType, type, get, set);
		navFormSourceExpression.SkipOnSaveValues = skipWriting;
		DataFieldDefinition dataFieldDefinition = MasterPage.Expressions.Find((DataFieldDefinition f) => f.Name == id);
		if (dataFieldDefinition != null)
		{
			navFormSourceExpression.SetIsInRepeaterGroup(dataFieldDefinition.InRepeaterGroup);
		}
		sourceExpressions.Add(id, navFormSourceExpression);
	}

	public void RegisterUIPart(string uiPartName, int uiPartFormId)
	{
		if (uiPartName == null)
		{
			throw new ArgumentNullException("uiPartName");
		}
		NavFormUIPart value = new NavFormUIPart
		{
			FormId = uiPartFormId
		};
		uiParts[uiPartName] = value;
	}

	protected NavForm GetUIPart(string uiPartName)
	{
		if (uiPartName == null)
		{
			throw new ArgumentNullException("uiPartName");
		}
		if (!TryGetUIPart(uiPartName, out var form))
		{
			throw new NavNCLInvalidFormHandleException(Lang._00095_50061);
		}
		return form;
	}

	internal void SetSourceTable(NavRecord record, bool clone)
	{
		SetSourceTable(record, clone, forceSecurityFilterFiltered: false);
	}

	internal void SetSourceTable(NavRecord record, bool clone, bool forceSecurityFilterFiltered)
	{
		if (record == navSourceObject.SourceObject)
		{
			return;
		}
		if (navSourceObject.SourceObject != null)
		{
			navSourceObject.SourceObject.Notify -= SourceTable_Notify;
			navSourceObject.SourceObject.Dispose();
			navSourceObject.SourceObject = null;
		}
		if (record == null)
		{
			return;
		}
		if (record.TableID != navSourceObject.SourceObjectId)
		{
			EnsureMetadataLoaded();
			if (record.TableID != navSourceObject.SourceObjectId)
			{
				throw new NavNCLTableIdMismatchException();
			}
		}
		if (clone)
		{
			navSourceObject.SourceObject = record.CloneRecord(this, reset: false, keepCompany: true);
			if (forceSecurityFilterFiltered)
			{
				navSourceObject.SourceObject.SecurityFiltering = SecurityFiltering.Filtered;
			}
		}
		else
		{
			navSourceObject.SourceObject = record;
			navSourceObject.SourceObjectId = record.TableID;
			navSourceObject.SourceObjectType = DataSourceType.Table;
		}
		navSourceObject.SourceObject.Notify += SourceTable_Notify;
		SetAutoCalcFields(GetCalculatedFields());
	}

	private void SourceTable_Notify(object sender, NavRecordEventArgs eventArgs)
	{
		if (sender == null)
		{
			throw new ArgumentNullException("sender");
		}
		if (eventArgs == null)
		{
			throw new ArgumentNullException("eventArgs");
		}
		if (SourceTableOperationsNotify != null)
		{
			SourceTableOperationsNotify(sender, eventArgs);
		}
	}

	public int[] GetCalculatedFields()
	{
		EnsureMetadataLoaded();
		return calculatedFields;
	}

	public void CallInitializeComponentExtensionMethod()
	{
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.InitializeComponent();
		});
	}

	public void CallOnMetadataLoadedExtensionMethod()
	{
		if (metadataLoaded)
		{
			PageExtensions.ForEach((NavFormExtension ext) =>
			{
				ext.OnMetadataLoaded();
			});
		}
	}

	protected override void CallOnClearExtensionMethod()
	{
		PageExtensions.ForEach((NavFormExtension ext) =>
		{
			ext.OnClear();
		});
	}

	public bool CallGetAutoFormatStringExtensionMethod(int fieldNumber, ref string returnValue)
	{
		foreach (NavFormExtension pageExtension in PageExtensions)
		{
			string autoFormatString = pageExtension.GetAutoFormatString(fieldNumber, out var handled);
			if (handled)
			{
				returnValue = autoFormatString;
				return true;
			}
		}
		return false;
	}

	public bool CallGetDecimalPlacesExtensionMethod(int fieldNumber, ref string returnValue)
	{
		foreach (NavFormExtension pageExtension in PageExtensions)
		{
			string decimalPlaces = pageExtension.GetDecimalPlaces(fieldNumber, out var handled);
			if (handled)
			{
				returnValue = decimalPlaces;
				return true;
			}
		}
		return false;
	}

	public bool CallEvaluateCaptionClassExtensionMethod(int fieldNumber, ref string returnValue)
	{
		foreach (NavFormExtension pageExtension in PageExtensions)
		{
			string text = pageExtension.EvaluateCaptionClass(fieldNumber, out var handled);
			if (handled)
			{
				returnValue = text;
				return true;
			}
		}
		return false;
	}

	public object Invoke(int extensionId, int methodId, object[] arguments)
	{
		NavFormExtension navFormExtension = PageExtensions.SingleOrDefault((NavFormExtension x) => x.ObjectId.ObjectNumber == extensionId);
		if (navFormExtension == null)
		{
			throw new NavNCLCompilationException(CultureInfo.InvariantCulture, Lang.PageExtensionNotFound, extensionId, ObjectId.ObjectNumber);
		}
		return navFormExtension.Invoke(methodId, arguments);
	}

	public NavForm GetPart(int partId)
	{
		return GetUIPart(partId.ToString(CultureInfo.InvariantCulture));
	}

	public NavForm(ITreeObject parent, int objectId)
		: this(parent, objectId, null, null)
	{
	}

	public NavForm(ITreeObject parent, int objectId, NavRecord record)
		: this(parent, objectId, null, record)
	{
	}

	public NavForm(ITreeObject parent, MasterPage masterPage)
		: this(parent, masterPage.ID, masterPage, null)
	{
	}

	protected NavForm(ITreeObject parent, MasterPage masterPage, NCLMetaTable nclMetaTableForQuery)
		: this(parent, masterPage.ID, masterPage, null)
	{
		nclMetaTable = nclMetaTableForQuery;
	}

	private NavForm(ITreeObject parent, int objectId, MasterPage masterPage, NavRecord record)
		: base(parent, new ApplicationObjectId(ObjectType.Page, objectId))
	{
		this.masterPage = masterPage;
		personalizationId = NavCurrentThread.DrillDownPersonalizationId ?? NavCurrentThread.FormPersonalizationId;
		handle = Guid.NewGuid();
		formId = ObjectId.ObjectNumber;
		UpdatePropagation = NavFormUpdatePropagation.ChildPage;
		PageExtensions = new List<NavFormExtension>();
		if (record != null)
		{
			SetSourceTable(record, clone: true);
			if (bookmarkType != BookmarkType.Record)
			{
				SyncTempTableWithSourceTable(clearTempTable: true);
			}
		}
	}

	public string ObjectID()
	{
		return ObjectID(useCaption: true);
	}

	public string ObjectID(bool useCaption)
	{
		if (useCaption)
		{
			return string.Format(CultureInfo.InvariantCulture, "Page {0}", MasterPage.Caption);
		}
		return string.Format(CultureInfo.InvariantCulture, "Page {0}", formId);
	}

	public void InitializeForm()
	{
		if (IsFormInitialized)
		{
			throw new NavNCLPageAlreadyInitializedException();
		}
		RegisterSystemSourceExpressions();
		if (NavCurrentThread.Session.IsCompanyOpen)
		{
			RaiseOnInit();
		}
		if (IsRequestPage && SaveValues && NavCurrentThread.Session.ClientConnectionType != ConnectionType.Background && !NavCurrentThread.Session.SkipRestoreSavedReportSettings)
		{
			ApplyLatestValues();
		}
		IsFormInitialized = true;
	}

	private void RegisterSystemSourceExpressions()
	{
		if (!IsRequestPage)
		{
			return;
		}
		RegisterSourceExpression("ObjectOptions", "ParameterSet", typeof(NavText), SourceExpressionType.SourceExpression, () => ParameterSet, (NavValue setValue) =>
		{
			NavText navText = (NavText)setValue;
			if (IsFormInitialized && !(ParameterSet == navText) && !(navText == NavText.Empty))
			{
				ParameterSet = navText;
				ApplyLatestValues();
				Update(saveRecord: false);
			}
		}, skipWriting: true);
		if (FeatureSwitches.Instance.DisableReportSettingsEdit)
		{
			RegisterSourceExpression("IsLatestSavedSettings", "IsLatestSavedSettings", typeof(bool), SourceExpressionType.VariableIncludedInDataSet, () => IsLatestSavedSettings, (NavValue value) =>
			{
				IsLatestSavedSettings = (NavBoolean)value;
			});
		}
	}

	public void ApplySourceTableViewAndSavedValues()
	{
		if (IsOpen)
		{
			throw new NavNCLFormAlreadyOpenedException();
		}
		if (SourceTableView != null)
		{
			ApplySourceTableView();
		}
		if (SaveValues)
		{
			ApplyLatestValues();
		}
	}

	public void OpenForm()
	{
		if (IsOpen)
		{
			throw new NavNCLFormAlreadyOpenedException();
		}
		if (!MasterPage.PageProperties.IsSystemPage && MasterPage.PageProperties.SourceObject.PageDataSource != DataSourceType.Query)
		{
			NavCurrentThread.Session.VerifyExecutePermission(this);
		}
		if (MasterPage.PageProperties.SourceObject.SourceTable != 0 && !MasterPage.PageProperties.SourceObject.SourceTableTemporary)
		{
			NavCurrentThread.Session.VerifyExecutePermission(new ApplicationObjectId(MasterPage.PageProperties.SourceObject.PageDataSource.ToObjectType(), MasterPage.PageProperties.SourceObject.SourceTable));
		}
		if (!IsRequestPage)
		{
			ApplySourceTableViewAndSavedValues();
		}
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			RaiseOnOpenPage();
			commit = true;
		}
		catch (NavControlException ex)
		{
			throw new NavNCLControlException(ex.ControlStatement);
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
		IsOpen = true;
	}

	public void ForceClose()
	{
		if (IsOpen)
		{
			NavCurrentThread.Session.Company.UnregisterForm(this);
			IsOpen = false;
		}
	}

	public void CloseForm(FormResult formResult)
	{
		if (!IsOpen)
		{
			return;
		}
		try
		{
			bool commit = false;
			NavCurrentThread.Session.BeginTransaction();
			try
			{
				RaiseOnClosePage();
				if (formResult != FormResult.Cancel && formResult != FormResult.No && formResult != FormResult.None && formResult != FormResult.Parameters)
				{
					StoreSaveValues(formResult != FormResult.LookupOK && formResult != FormResult.LookupCancel);
				}
				commit = true;
			}
			finally
			{
				NavCurrentThread.Session.EndTransaction(commit);
			}
		}
		finally
		{
			CloseOpenDialogs();
			if (formResult != FormResult.Parameters)
			{
				NavCurrentThread.Session.Company.UnregisterForm(this);
			}
			IsOpen = false;
		}
	}

	public void QueryCloseForm(int closeActionValue)
	{
		if (!IsOpen)
		{
			throw new NavErrorOnQueryCloseException(Lang.PageCannotClose, new NavNCLFormNotOpenedException());
		}
		bool flag = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			flag = RaiseOnQueryClosePage((FormResult)closeActionValue);
			if (!flag)
			{
				throw new NavFormCloseNotAllowedException(Lang.PageCannotClose);
			}
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(flag);
		}
	}

	public void NewRecord(bool belowXRec)
	{
		if (SourceTable != null)
		{
			IEnumerable<int> fieldNumbers = SourceTable.InitializeFieldsFromFilters(MasterPage.PageProperties.SourceObject.PopulateAllFields, includeIdenticalFilters: true);
			if (ValidateFieldsInOnNewRecord)
			{
				SourceTable.ValidateFields(fieldNumbers, SourceTable);
			}
		}
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			if (belowXRec && SourceTable != null)
			{
				OldRecord.ALFindLast(DataError.TrapError);
			}
			RaiseOnNewRecord(belowXRec);
			if (SourceTable != null)
			{
				OldRecord.ALAssign(SourceTable);
			}
			commit = true;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
		if (bookmarkType != BookmarkType.Record)
		{
			TempTable.ALInit();
			SyncTempTableWithSourceTable(clearTempTable: false);
		}
	}

	public bool Insert(bool belowXRec)
	{
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			SplitKey();
			bool result = RaiseOnInsertRecord(belowXRec);
			if (SourceTable.HasBeenInserted)
			{
				OldRecord.ALAssign(SourceTable);
			}
			if (SourceTableOperationsNotify != null)
			{
				SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Insert));
			}
			commit = true;
			return result;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	public bool Modify()
	{
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			bool result = RaiseOnModifyRecord();
			if (SourceTableOperationsNotify != null)
			{
				SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Modify));
			}
			commit = true;
			return result;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	public void Rename()
	{
		if (SourceTableOperationsNotify != null)
		{
			SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Rename));
		}
	}

	public bool Delete()
	{
		bool commit = false;
		NavCurrentThread.Session.BeginTransaction();
		try
		{
			bool result = RaiseOnDeleteRecord();
			if (SourceTableOperationsNotify != null)
			{
				SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Delete));
			}
			commit = true;
			return result;
		}
		finally
		{
			NavCurrentThread.Session.EndTransaction(commit);
		}
	}

	public void AfterGetRecord()
	{
		AfterGetRecord(null);
	}

	public void AfterGetRecord(int[] calcFields)
	{
		if (SourceTable != null && calcFields != null)
		{
			SourceTable.CalcFields(DataError.TrapError, calcFields);
		}
		RaiseOnAfterGetRecord();
		if (SourceTable != null)
		{
			OldRecord.ALAssign(SourceTable);
		}
	}

	public void AfterGetCurrRecord()
	{
		RaiseOnAfterGetCurrRecord();
		if (SourceTable != null)
		{
			OldRecord.ALAssign(SourceTable);
		}
	}

	public void SetTableView(NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		SetTableViewUsed = true;
		SafeSourceTable.SetUnfilteredTableView(record.GetTableView());
		if (SourceTableView != null)
		{
			ApplySourceTableView();
		}
		SafeSourceTable.RecordImplementation.CopyFilters(record, CopyFilterOptions.CopyMarkedList);
	}

	public void SetAutoCalcFields(int[] calcFields)
	{
		if (SourceTable != null && calcFields != null)
		{
			SourceTable.SetAutoCalcFields(DataError.TrapError, calcFields, replace: true);
		}
	}

	public int[] GetAutoCalcFields()
	{
		if (navSourceObject.SourceObject == null)
		{
			return null;
		}
		return navSourceObject.SourceObject.GetAutoCalcFields();
	}

	public bool FindRecord(string which)
	{
		if (SourceTable == null)
		{
			return true;
		}
		NavRecordId other = ((SourceTableOperationsNotify == null) ? null : SourceTable.ALRecordId);
		bool flag = RaiseOnFindRecord(new NavText(which));
		if (SourceTableOperationsNotify != null && !SourceTable.ALRecordId.Equals(other))
		{
			SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		if (flag && bookmarkType != BookmarkType.Record)
		{
			SyncTempTableWithSourceTable(clearTempTable: false);
		}
		return flag;
	}

	public int NextRecord(int steps)
	{
		if (SourceTable == null)
		{
			return 0;
		}
		NavRecordId other = ((SourceTableOperationsNotify == null) ? null : SourceTable.ALRecordId);
		int num = RaiseOnNextRecord(steps);
		if (SourceTableOperationsNotify != null && !SourceTable.ALRecordId.Equals(other))
		{
			SourceTableOperationsNotify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		if (num != 0 && bookmarkType != BookmarkType.Record)
		{
			SyncTempTableWithSourceTable(clearTempTable: false);
		}
		return num;
	}

	public bool TryGetUIPart(string uiPartName, out NavForm form)
	{
		if (uiParts.TryGetValue(uiPartName, out var value))
		{
			NavFormHandle navFormHandle = value.FormHandle;
			if (navFormHandle == null)
			{
				navFormHandle = new NavFormHandle(this, value.FormId);
				navFormHandle.Target.UpdateRequest += form_UpdateRequest;
				value.FormHandle = navFormHandle;
			}
			form = navFormHandle.Target;
		}
		else
		{
			form = null;
		}
		return form != null;
	}

	public void SetBookmark(DataError dataError, byte[] bookmarkBytes, bool useFilters = false)
	{
		if (SourceTable == null)
		{
			return;
		}
		NavRecordId navRecordId = new NavRecordId(bookmarkBytes);
		bool flag = false;
		if (BookmarkType == BookmarkType.Record)
		{
			if ((dataError == DataError.ThrowError || navSourceObject.SourceObject.ALRecordId.StartsWithPrimaryKeyFieldTypes(navRecordId)) && SourceTable.Get(dataError, navRecordId, copyRecord: true, useFilters))
			{
				flag = true;
			}
		}
		else if (TempTable.Get(DataError.TrapError, navRecordId))
		{
			SourceTable.Assign(TempTable);
			flag = true;
		}
		else if (SourceTable.Get(dataError, navRecordId))
		{
			SyncTempTableWithSourceTable(clearTempTable: false);
			flag = true;
		}
		if (flag)
		{
			AfterGetRecord(GetCalculatedFields());
		}
	}

	public NavText GetDynamicCaption(int controlId)
	{
		string text = EvaluateCaptionClass(controlId);
		if (text == NavText.Empty)
		{
			return NavText.Empty;
		}
		return Session.SystemCodeunitFactory.UIHelperTriggers.InvokeCaptionClassTranslate(NavCurrentThread.Session.LocalLanguage, new NavText(text));
	}

	public NavText GetDynamicCueStyle(int controlId, int fieldNo)
	{
		if (!CueStyleHelper.UseCodeunitTriggerForControl(controlId, metadataHelper))
		{
			return NavText.Empty;
		}
		if (!CueStyleHelper.TryGetValueAsDecimal(navSourceObject.SourceObject, fieldNo, out var valueAsDecimal))
		{
			return NavText.Empty;
		}
		return Session.SystemCodeunitFactory.UIHelperTriggers.InvokeGetCueStyle(NavInteger.Create(navSourceObject.SourceObject.MetaTable.TableId), NavInteger.Create(fieldNo), valueAsDecimal);
	}

	public string GetDecimalString(INavRecordHandle record, int fieldId, int controlId)
	{
		string text = GetAutoFormatString(controlId);
		if (string.IsNullOrEmpty(text))
		{
			text = record.Target.GetAutoFormatString(fieldId);
			if (string.IsNullOrEmpty(text))
			{
				text = GetDecimalPlaces(controlId);
				if (!string.IsNullOrEmpty(text))
				{
					string decimalPlaces = record.Target.GetDecimalPlaces(fieldId);
					if (string.IsNullOrEmpty(decimalPlaces))
					{
						return text;
					}
					int decimalMax = GetDecimalMax(text);
					int decimalMax2 = GetDecimalMax(decimalPlaces);
					if (decimalMax < decimalMax2)
					{
						return text;
					}
					return decimalPlaces;
				}
				text = record.Target.GetDecimalPlaces(fieldId);
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			return Session.SystemCodeunitFactory.UIHelperTriggers.InvokeAutoFormatTranslate(1, NavText.Empty);
		}
		return text;
	}

	public NavFormHandle CreateNavFormHandle(ITreeObject parent)
	{
		return new NavFormHandle(parent, FormId)
		{
			Target = this
		};
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !IsDisposed)
			{
				SetSourceTable(null, clone: false);
				Tree.DisposeAllChildren();
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void ApplyLatestValues()
	{
		if (ApplyReportOptions != null)
		{
			ApplyReportOptions(this, new EventArgs<string>(ParameterSet));
			return;
		}
		Dictionary<string, NavValue> variables = NavPageDataPersonalizationHelper.ReadValues(FormId, MasterPage.PageProperties.PageType, PageDataIdentifier.SaveValues);
		ApplyLatestValues(variables);
	}

	internal void ApplyLatestValues(Dictionary<string, NavValue> variables)
	{
		if (variables != null && VerifySourceExpressionsExists(variables))
		{
			ApplySourceExpressions(variables);
		}
	}

	private bool VerifySourceExpressionsExists(Dictionary<string, NavValue> variables)
	{
		foreach (KeyValuePair<string, NavValue> variable in variables)
		{
			if (FindSourceExpression(variable.Key, variable.Value) == null)
			{
				return false;
			}
		}
		return true;
	}

	private object FindSourceExpression(string valueName, INavValueMetadata metadata)
	{
		return SourceExpressions.Values.FirstOrDefault((NavFormSourceExpression expression) =>
		{
			if (expression.Name.Equals(valueName))
			{
				NavValue navValue = expression.Get();
				if (metadata.NclType != navValue.NclType)
				{
					return false;
				}
				if (metadata.NclType == NavNclType.NavOption)
				{
					return metadata.NavOptionMetadata.OptionString == navValue.NavOptionMetadata.OptionString;
				}
				return navValue.CompareTypeAndMetadata(metadata);
			}
			return false;
		});
	}

	private void ApplySourceExpressions(Dictionary<string, NavValue> variables)
	{
		foreach (NavFormSourceExpression value2 in SourceExpressions.Values)
		{
			if (value2.SourceExpressionType == SourceExpressionType.SourceExpression && value2.Writable)
			{
				NavValue value = null;
				if (variables.TryGetValue(value2.Name, out value))
				{
					value2.Set(value);
				}
			}
		}
	}

	private void StoreSaveValues(bool presetApplied)
	{
		Dictionary<string, NavValue> dictionary = new Dictionary<string, NavValue>();
		foreach (NavFormSourceExpression value in SourceExpressions.Values)
		{
			if (value != null && !value.SkipOnSaveValues && value.SourceExpressionType == SourceExpressionType.SourceExpression && value.Writable && !dictionary.ContainsKey(value.Name))
			{
				dictionary.Add(value.Name, value.Get());
			}
		}
		if (dictionary.Count > 0 && OnSaveValues != null)
		{
			OnSaveValues(this, dictionary);
		}
		if (ReportRequestOptions.ParametersOnly)
		{
			return;
		}
		if (ParameterSet != LatestSavedSettings)
		{
			if (IsRequestPage & presetApplied)
			{
				SaveReportOptions(LatestSavedSettings);
			}
		}
		else if (SaveValues)
		{
			if (GetReportOptions != null)
			{
				SaveReportOptions(ParameterSet);
			}
			else
			{
				NavPageDataPersonalizationHelper.SaveValues(FormId, MasterPage.PageProperties.PageType, PageDataIdentifier.SaveValues, dictionary);
			}
		}
	}

	private void SaveReportOptions(NavText parameters)
	{
		if (GetReportOptions != null)
		{
			GetReportOptionsEventArgs e = new GetReportOptionsEventArgs();
			GetReportOptions(this, e);
			NavObjectOptions navObjectOptions = new NavObjectOptions(e.ObjectType, formId, parameters);
			navObjectOptions.Save(e.ReportOptions);
		}
	}

	private void ApplySourceTableView()
	{
		if (SourceTable == null)
		{
			return;
		}
		ViewDefinitionSorting sorting = SourceTableView.Sorting;
		List<FilterDefinition> tableFilters = SourceTableView.TableFilters;
		if (sorting != null)
		{
			if (sorting.KeyFieldsSetByView && MasterPage.PageProperties.SourceObject.SourceTableSpecified)
			{
				MetaTable tableMetadata = NavGlobal.MetadataProvider.GetTableMetadata(MasterPage.PageProperties.SourceObject.SourceTable);
				int aLCurrentKeyIndex = tableMetadata.CalcIndexFromKeyFields(sorting.KeyFields);
				if (!string.IsNullOrEmpty(sorting.KeyFields))
				{
					SourceTable.ALSetCurrentKey(MetaTable.GetKeyFieldIds(sorting.KeyFields));
				}
				else
				{
					SourceTable.ALCurrentKeyIndex = aLCurrentKeyIndex;
				}
			}
			if (sorting.AscendingSetByView)
			{
				SourceTable.ALAscending = sorting.Ascending;
			}
		}
		if (tableFilters == null)
		{
			return;
		}
		int aLFilterGroup = SourceTable.ALFilterGroup;
		try
		{
			SourceTable.ALFilterGroup = 2;
			foreach (FilterDefinition item in tableFilters)
			{
				SourceTable.SetFilter(SourceTable.MetaTable.GetFieldByNo(item.FieldID), item.FilterValue);
			}
		}
		finally
		{
			SourceTable.ALFilterGroup = aLFilterGroup;
		}
	}

	private void form_UpdateRequest(object sender, UpdateRequestEventArgs e)
	{
		if (UpdateRequest != null)
		{
			UpdateRequest(sender, e);
		}
	}

	private void SplitKey()
	{
		if (!NeedAutoSplitKey)
		{
			return;
		}
		NCLMetaKey keyByIndex = SafeSourceTable.MetaTable.GetKeyByIndex(0);
		NCLMetaField nCLMetaField = keyByIndex.KeyFieldsList[keyByIndex.KeyFieldCount - 1];
		if (nCLMetaField.FieldNavType != NavType.GUID && nCLMetaField.FieldNavType != NavType.Integer && nCLMetaField.FieldNavType != NavType.BigInteger && nCLMetaField.FieldNavType != NavType.Decimal)
		{
			throw NavNCLNotSupportedTypeException.CreateSplitKeyType(CultureInfo.InvariantCulture, nCLMetaField.FieldNavType.ToString());
		}
		if (!SafeSourceTable.GetFieldValue(nCLMetaField.FieldNo).IsZeroOrEmpty)
		{
			return;
		}
		if (AutoKeyValue != null)
		{
			using NavRecord navRecord = new NavRecord(SafeSourceTable.Parent, SafeSourceTable.TableID);
			navRecord.Assign(SafeSourceTable);
			NavValue newValue = NavValue.CreateNavValueFromObject(nCLMetaField, AutoKeyValue);
			navRecord.SetFieldValue(nCLMetaField.FieldNo, newValue);
			if (!navRecord.Exists(navRecord.ALRecordId))
			{
				SafeSourceTable.SetFieldValue(nCLMetaField.FieldNo, newValue);
				return;
			}
			InsertLowerBoundBookmark = navRecord.ALRecordId;
			NCLMetaKey keyByIndex2 = navRecord.MetaTable.GetKeyByIndex(0);
			NavValue[] array = new NavValue[keyByIndex2.KeyFieldCount - 1];
			for (int i = 0; i < keyByIndex2.KeyFieldCount - 1; i++)
			{
				array[i] = navRecord.GetFieldValue(keyByIndex2.GetKeyFieldByIndex(i));
			}
			bool flag = false;
			if (navRecord.ALNext() > 0)
			{
				for (int j = 0; j < keyByIndex2.KeyFieldCount - 1; j++)
				{
					if (!navRecord.GetFieldValue(keyByIndex2.GetKeyFieldByIndex(j)).Equals(array[j]))
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			InsertUpperBoundBookmark = (flag ? null : navRecord.ALRecordId);
		}
		object lowerBound = null;
		object obj = null;
		if (InsertLowerBoundBookmark != null)
		{
			SafeSourceTable.SetPrimaryKeyFieldValues(InsertLowerBoundBookmark);
			lowerBound = SafeSourceTable.GetFieldValue(nCLMetaField.FieldNo);
		}
		if (InsertUpperBoundBookmark != null)
		{
			SafeSourceTable.SetPrimaryKeyFieldValues(InsertUpperBoundBookmark);
			obj = SafeSourceTable.GetFieldValue(nCLMetaField.FieldNo);
		}
		NavValue newValue2 = CalculateAutoSplitKeyValue(nCLMetaField, obj, lowerBound);
		SafeSourceTable.SetFieldValue(nCLMetaField.FieldNo, newValue2);
		using (NavRecord navRecord2 = new NavRecord(SafeSourceTable.Parent, SafeSourceTable.TableID))
		{
			navRecord2.Assign(SafeSourceTable);
			if (navRecord2.Exists(navRecord2.ALRecordId))
			{
				navRecord2.ALSetRecFilter();
				navRecord2.RecordImplementation.SetFieldFilter(nCLMetaField, (FilterExpression)null);
				if (navRecord2.ALFindLast(DataError.TrapError))
				{
					obj = null;
					lowerBound = navRecord2.GetFieldValue(nCLMetaField.FieldNo);
					newValue2 = CalculateAutoSplitKeyValue(nCLMetaField, obj, lowerBound);
					SafeSourceTable.SetFieldValue(nCLMetaField.FieldNo, newValue2);
				}
			}
		}
		if (obj != null)
		{
			InsertLowerBoundBookmark = InsertUpperBoundBookmark;
		}
		else
		{
			InsertLowerBoundBookmark = SafeSourceTable.ALRecordId;
		}
		InsertUpperBoundBookmark = null;
	}

	private static NavValue CalculateAutoSplitKeyValue(NCLMetaField keymetafield, object upperBound, object lowerBound)
	{
		switch (keymetafield.FieldNavType)
		{
		case NavType.GUID:
			return new NavGuid(Guid.NewGuid());
		case NavType.Integer:
		{
			int num2 = ((lowerBound != null) ? ((int)(NavInteger)lowerBound) : 0);
			if (upperBound == null)
			{
				return NavInteger.Create(num2 + 10000);
			}
			return NavInteger.Create((num2 + (int)(NavInteger)upperBound) / 2);
		}
		case NavType.BigInteger:
		{
			long num = ((lowerBound == null) ? 0 : ((long)(NavBigInteger)lowerBound));
			if (upperBound == null)
			{
				return NavBigInteger.Create(num + 10000);
			}
			return NavBigInteger.Create((num + (long)(NavBigInteger)upperBound) / 2);
		}
		case NavType.Decimal:
		{
			Decimal18 @decimal = ((lowerBound == null) ? ((Decimal18)0) : ((Decimal18)(NavDecimal)lowerBound));
			if (upperBound == null)
			{
				return NavDecimal.Create(@decimal + 10000);
			}
			return NavDecimal.Create((@decimal + (NavDecimal)upperBound) / 2);
		}
		default:
			throw NavNCLNotSupportedTypeException.CreateSplitKeyType(CultureInfo.InvariantCulture, keymetafield.FieldNavType.ToString());
		}
	}

	private MasterPage GetMasterPage()
	{
		MasterPage masterPage = NavGlobal.MetadataProvider.GetMasterPage(ObjectId.ObjectNumber, navSourceObject.SourceObjectType, personalizationId, applyPersonalization: true);
		if (masterPage == null)
		{
			throw new NavNCLObjectNotFoundException();
		}
		return masterPage;
	}

	private static int GetDecimalMax(string formatString)
	{
		int num = formatString.IndexOf(":", StringComparison.Ordinal) + 1;
		if (num == 0)
		{
			num = formatString.IndexOf(",", StringComparison.Ordinal) + 1;
		}
		int num2 = formatString.IndexOf(">", StringComparison.Ordinal);
		if (num == num2)
		{
			return 15;
		}
		string value = formatString.Substring(num, num2 - num);
		return Convert.ToInt32(value, CultureInfo.InvariantCulture);
	}

	private static bool RequiresExecutePermissionCheck(MasterPage masterPage)
	{
		if (masterPage.PageProperties != null)
		{
			PageType pageType = masterPage.PageProperties.PageType;
			if ((uint)(pageType - 13) <= 2u)
			{
				return false;
			}
		}
		return true;
	}

	private void SetFieldsFromControlsMetadata()
	{
		List<int> list = new List<int>();
		List<string> list2 = new List<string>();
		HashSet<string> visibleControlsSet = new HashSet<string>();
		foreach (ControlDefinition controlDefinition in MetadataHelper.ControlDefinitions)
		{
			if (string.IsNullOrEmpty(controlDefinition.DataColumnName))
			{
				continue;
			}
			int result = 0;
			bool flag = ControlIsVisible(controlDefinition);
			if (int.TryParse(controlDefinition.DataColumnName, out result) && result > 0)
			{
				if (SourceTable == null)
				{
					throw new NavNSInvalidPageDefinitionException(string.Format(CultureInfo.CurrentCulture, Lang.FaultyRequestPageDefinition, GetType().FullName));
				}
				NCLMetaField fieldByNo = SourceTable.MetaTable.GetFieldByNo(result);
				if (fieldByNo.ObsoleteState == ObsoleteState.Removed && !fieldByNo.FieldObsoleted)
				{
					controlDefinition.Visible = bool.FalseString;
					flag = false;
				}
				if ((fieldByNo.FieldClass == FieldClass.FlowField || fieldByNo.FieldNclType == NavNclType.NavBlob) && (includeInvisibleCalcFields | flag) && !fieldByNo.FieldObsoleted)
				{
					list.Add(result);
				}
			}
			if (!flag && !visibleControlsSet.Contains(controlDefinition.DataColumnName))
			{
				list2.Add(controlDefinition.DataColumnName);
			}
			else
			{
				visibleControlsSet.Add(controlDefinition.DataColumnName);
			}
		}
		list2.RemoveAll((string c) => visibleControlsSet.Contains(c));
		calculatedFields = list.ToArray();
		SetAutoCalcFields(GetCalculatedFields());
		invisibleControls = new ReadOnlyCollection<string>(list2);
	}

	public void ApplyUpdatePropagation(UpdatePropagation updatePropagation)
	{
		UpdatePropagation = ((updatePropagation != Microsoft.Dynamics.Nav.Types.Metadata.UpdatePropagation.Both) ? NavFormUpdatePropagation.ChildPage : NavFormUpdatePropagation.Both);
	}

	internal static bool ControlIsVisible(ControlDefinition control)
	{
		if (control.Visible.ToUpper(CultureInfo.InvariantCulture) != "FALSE")
		{
			return control.Visible.ToUpper(CultureInfo.InvariantCulture) != "0";
		}
		return false;
	}

	private void InitializeFromMetadata()
	{
		ExecutePermissionsValidated = !RequiresExecutePermissionCheck(masterPage);
		saveValues = masterPage.PageProperties.SourceObject.SaveValues;
		formName = masterPage.Name;
		PageCaption = MasterPage.Caption;
		Editable = masterPage.PageProperties == null || masterPage.PageProperties.Editable;
		bool isTemporary = false;
		if (masterPage.PageProperties != null && masterPage.PageProperties.SourceObject != null)
		{
			navSourceObject.SourceObjectId = masterPage.PageProperties.SourceObject.SourceTable;
			navSourceObject.SourceObjectType = masterPage.PageProperties.SourceObject.PageDataSource;
			isTemporary = masterPage.PageProperties.SourceObject.SourceTableTemporary;
		}
		if (navSourceObject.SourceObject == null && navSourceObject.SourceObjectId > 0)
		{
			NavRecord record;
			if (navSourceObject.SourceObjectType == DataSourceType.Query)
			{
				record = new NavRecord(this, navSourceObject.SourceObjectId, nclMetaTable, isTemporary: false, null, Session.Company.CompanyName, SecurityFiltering.Filtered);
			}
			else
			{
				NCLMetaTable metaTableById = Session.NCLMetadata.GetMetaTableById(navSourceObject.SourceObjectId);
				record = metaTableById.CreateObjectInstance(this, isTemporary, null, string.Empty, SecurityFiltering.Filtered);
			}
			SetSourceTable(record, clone: false);
		}
		SetFieldsFromControlsMetadata();
		UpdateAllowedOperationsFromPermissions();
		OnMetadataLoaded();
	}

	private void EnsureMetadataLoaded()
	{
		ApplicationObjectRootScope.AddApplicationObjectRootScope(this, () =>
		{
			bool flag = NavCurrentThread.Session.StalePages.Contains(masterPage?.ID ?? (-1));
			if (!metadataLoaded | flag)
			{
				if (masterPage == null || (flag && !SystemPages.IsServerSystemPageId(masterPage.ID)))
				{
					masterPage = GetMasterPage();
					if (flag)
					{
						NavCurrentThread.Session.StalePages.Remove(masterPage.ID);
						masterpageReloaded = true;
					}
				}
				if (!flag)
				{
					if (masterPage.PageSupportsEvents() && nclMetaForm == null && masterPage.PageProperties.SourceObject.PageDataSource == DataSourceType.Table)
					{
						NavGlobal.NCLMetadata.TryGetMetaFormById(formId, out nclMetaForm, OriginalAppGroupId);
					}
					metadataLoaded = true;
					InitializeFromMetadata();
				}
			}
		});
	}

	private void UpdateAllowedOperationsFromPermissions()
	{
		if (!IsAbleToOverwriteAllowedProperties || disableUpdateAllowedOperationsFromPermissions)
		{
			return;
		}
		NavForm navForm = this;
		PermissionMask additionalIndirectPermissions = PermissionMask.None;
		PageProperties pageProperties = MasterPage?.PageProperties;
		SourceObjectDefinition sourceObjectDefinition = pageProperties?.SourceObject;
		if (pageProperties == null || sourceObjectDefinition == null)
		{
			return;
		}
		if (pageProperties.CardFormIDSpecified && pageProperties.CardFormID != FormId)
		{
			IPermissionSet permissionSet = Session.Permissions.GetPermissionSet(Session.Company.CompanyNameToken, navForm, new ApplicationObjectId(ObjectType.Page, pageProperties.CardFormID));
			if (!permissionSet.HasPermissions(null, PermissionMask.Execute, Session.LimitedUserPermissionChecker, null, PermissionMask.None, countAsLimitedUserAllowedTable: false, PermissionValidationCategory.SystemActionAccessByPermission))
			{
				SetSystemActionsAllowed(insertAllowed: false, modifyAllowed: false, deleteAllowed: false);
				return;
			}
			MasterPage masterPage = NavGlobal.MetadataProvider.GetMasterPage(pageProperties.CardFormID, DataSourceType.Table, personalizationId, applyPersonalization: false);
			pageProperties = masterPage.PageProperties;
			sourceObjectDefinition = pageProperties?.SourceObject;
			if (pageProperties == null || sourceObjectDefinition == null)
			{
				return;
			}
			additionalIndirectPermissions = PermissionHelper.GetIndirectPermissionMask(sourceObjectDefinition, sourceObjectDefinition.SourceTable);
			SetSystemActionsAllowed(sourceObjectDefinition.InsertAllowed, sourceObjectDefinition.ModifyAllowed, sourceObjectDefinition.DeleteAllowed);
			navForm = new NavForm(this, masterPage)
			{
				disableUpdateAllowedOperationsFromPermissions = true
			};
			using (NavMetadataAccessContext.GetAccessToRemoved(NavCurrentThread.Session, Guid.NewGuid().ToString("B")))
			{
				navForm.EnsureMetadataLoaded();
			}
		}
		if (!sourceObjectDefinition.SourceTableSpecified)
		{
			SetSystemActionsAllowed(insertAllowed: false, modifyAllowed: false, deleteAllowed: false);
		}
		else if (!sourceObjectDefinition.SourceTableTemporary && (InsertAllowed || ModifyAllowed || DeleteAllowed))
		{
			IPermissionSet permissionSet2 = Session.Permissions.GetPermissionSet(Session.Company.CompanyNameToken, navForm, new ApplicationObjectId(ObjectType.TableData, sourceObjectDefinition.SourceTable));
			if (InsertAllowed)
			{
				MasterPage.PageProperties.SourceObject.InsertAllowed = permissionSet2.HasPermissions(navForm.SourceTable, PermissionMask.Insert, Session.LimitedUserPermissionChecker, null, additionalIndirectPermissions, countAsLimitedUserAllowedTable: false, PermissionValidationCategory.SystemActionAccessByPermission);
			}
			if (ModifyAllowed)
			{
				MasterPage.PageProperties.SourceObject.ModifyAllowed = permissionSet2.HasPermissions(navForm.SourceTable, PermissionMask.Modify, Session.LimitedUserPermissionChecker, null, additionalIndirectPermissions, countAsLimitedUserAllowedTable: false, PermissionValidationCategory.SystemActionAccessByPermission);
			}
			if (DeleteAllowed)
			{
				MasterPage.PageProperties.SourceObject.DeleteAllowed = permissionSet2.HasPermissions(navForm.SourceTable, PermissionMask.Delete, Session.LimitedUserPermissionChecker, null, additionalIndirectPermissions, countAsLimitedUserAllowedTable: false, PermissionValidationCategory.SystemActionAccessByPermission);
			}
		}
	}

	private void SetSystemActionsAllowed(bool insertAllowed, bool modifyAllowed, bool deleteAllowed)
	{
		masterPage.PageProperties.SourceObject.InsertAllowed = insertAllowed;
		masterPage.PageProperties.SourceObject.ModifyAllowed = modifyAllowed;
		masterPage.PageProperties.SourceObject.DeleteAllowed = deleteAllowed;
	}

	public override int GetHashCode()
	{
		return FormId;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		return new NavFormHandle(parent, this);
	}

	private void CloseOpenDialogs()
	{
		if (OpenDialogTracking != null)
		{
			OpenDialogTracking.Dispose();
			OpenDialogTracking = null;
		}
	}
}
