using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Diagnostic;
using Microsoft.Dynamics.Nav.EventSubscription;
using Microsoft.Dynamics.Nav.Runtime.Diagnostics;
using Microsoft.Dynamics.Nav.Runtime.ExecutionTracking.RecordTracking;
using Microsoft.Dynamics.Nav.Runtime.Extensions;
using Microsoft.Dynamics.Nav.Security.Permissions;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerTypeProxy(typeof(NavRecordDebugView))]
public class NavRecord : NavApplicationObjectBase, INavRecordHandle, INavApplicationObjectBaseHandle
{
	public sealed class NavFieldRefShallow : NavValue
	{
		private readonly int extensionObjectId;

		internal NavRecord OwnerRecord { get; }

		internal NavType ALType => GetFieldValue().NavType;

		internal int FieldNo { get; }

		public override NavValue InnerValue => GetFieldValue();

		public override int GetBytesSize => GetFieldValue().GetBytesSize;

		public override bool IsZeroOrEmpty => GetFieldValue().IsZeroOrEmpty;

		public override NavNclType NclType => GetFieldValue().NclType;

		public NavFieldRefShallow(NavRecord record, int fieldNoNo)
			: this(record, 0, fieldNoNo)
		{
		}

		public NavFieldRefShallow(NavRecord record, int extensionObjectId, int fieldNoNo)
		{
			this.extensionObjectId = extensionObjectId;
			OwnerRecord = record;
			FieldNo = fieldNoNo;
		}

		public override int GetHashCode()
		{
			return GetFieldValue().GetHashCode();
		}

		public override Decimal18 ToDecimal()
		{
			return InnerValue.ToDecimal();
		}

		private NavValue GetFieldValue()
		{
			if (extensionObjectId != 0)
			{
				return OwnerRecord.GetFieldValue(extensionObjectId, FieldNo);
			}
			return OwnerRecord.GetFieldValue(FieldNo);
		}
	}

	private const string UseTextConstant = "Use FIELDERROR with a text constant to improve Telemetry details.";

	public const string FindDefault = "=><";

	public const string FindEqual = "=";

	public const string FindEqualOrGreater = "=>";

	public const string FindEqualOrLess = "=<";

	public const string FindMinus = "-";

	public const string FindPlus = "+";

	public const int UnknownFieldNumberErrNumber = 1005;

	private NCLMetaTable metaTable;

	private NavRecord oldRecordBase;

	private readonly RecordImplementation recordImplementation;

	private NavCompany parentCompany;

	private int currentFieldNumber;

	private string currentCompanyName;

	private bool globalVariablesInitialized;

	private readonly bool isTemporary;

	private readonly IDictionary<string, NavApplicationObjectBase> triggerMap = new Dictionary<string, NavApplicationObjectBase>();

	private List<NavRecordExtension> orderedTableExtensions = new List<NavRecordExtension>();

	public bool ALHasLinks
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.HasLinks();
		}
	}

	public string ALTableName
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.TableName;
		}
	}

	public string ALTableCaption
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			if (Session.IsLocalLanguage)
			{
				return MetaTable.GetTableCaption(Session.LocalLanguage);
			}
			return MetaTable.GetTableCaption(Session.GlobalLanguage);
		}
	}

	public bool ALAscending
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetTableAscending();
		}
		set
		{
			CheckRecordIsOpenAndNotStale();
			recordImplementation.SetTableAscending(value);
		}
	}

	public int ALCount
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetRecordCount();
		}
	}

	public int ALCountApprox
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetRecordCountApprox();
		}
	}

	public bool ALIsEmpty
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.IsEmpty();
		}
	}

	public int ALCurrentKeyIndex
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetCurrentKeyIndex();
		}
		set
		{
			CheckRecordIsOpenAndNotStale();
			recordImplementation.SetCurrentKeyIndex(value);
		}
	}

	public NavRecordId ALRecordId
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetRecordId();
		}
	}

	public string ALGetFilters
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetFilters();
		}
	}

	internal string GetUrlFilters
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetUrlFilters();
		}
	}

	public string ALCurrentKey
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetCurrentKey();
		}
	}

	public bool ALReadPermission
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.HasReadPermission(PermissionValidationCategory.ReadPermission);
		}
	}

	public bool ALWritePermission
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.HasWritePermission(PermissionValidationCategory.WritePermission);
		}
	}

	public bool ALReadConsistency
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.ReadConsistency;
		}
	}

	public bool ALRecordLevelLocking
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.RecordLevelLocking;
		}
	}

	public int ALFilterGroup
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetFilterGroup();
		}
		set
		{
			CheckRecordIsOpenAndNotStale();
			recordImplementation.SetFilterGroup(value);
		}
	}

	public bool ALMarkedOnly
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.GetMarkedOnly();
		}
		set
		{
			CheckRecordIsOpenAndNotStale();
			recordImplementation.SetMarkedOnly(value);
		}
	}

	public int ALKeyCount
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.KeyCount;
		}
	}

	public SecurityFiltering ALSecurityFiltering
	{
		get
		{
			return SecurityFiltering;
		}
		set
		{
			SecurityFiltering = value;
		}
	}

	public int CurrFieldNo
	{
		get
		{
			return currentFieldNumber;
		}
		set
		{
			currentFieldNumber = value;
		}
	}

	public int ALFieldCount
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.FieldCount - 1;
		}
	}

	public bool ALHasFilter
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.HasFilter();
		}
	}

	public bool ALIsTemporary => IsTemporary;

	public string ALCurrentCompany => CompanyName;

	internal int RequiredSessionId { get; }

	public override NavNclType NclType => NavNclType.NavRecord;

	public override int GetBytesSize => 0;

	public ITreeObject Parent => Tree.Parent;

	public NavSession ParentSession => Session;

	public NavCompany ParentCompany => parentCompany;

	public NCLMetaTable MetaTable => metaTable;

	public int LookupFormId => MetaTable.LookupFormId;

	public NavRecord OldRecord
	{
		get
		{
			if (oldRecordBase == null)
			{
				CheckRecordIsOpenAndNotStale();
				if (GetType() == typeof(NavRecord))
				{
					oldRecordBase = new NavRecord(this, TableID, metaTable, IsTemporary, this, currentCompanyName, RecordImplementation.SecurityFiltering);
				}
				else
				{
					oldRecordBase = metaTable.CreateObjectInstance(this, IsTemporary, this, currentCompanyName, RecordImplementation.SecurityFiltering);
				}
			}
			return oldRecordBase;
		}
	}

	public int TableID
	{
		get
		{
			if (IsDisposed)
			{
				throw new ObjectDisposedException("NavRecord");
			}
			return MetaTable.TableId;
		}
	}

	public string CompanyName
	{
		get
		{
			if (IsOpen)
			{
				if (string.IsNullOrEmpty(currentCompanyName))
				{
					return ParentSession.CompanyName;
				}
				return currentCompanyName;
			}
			return string.Empty;
		}
	}

	public bool IsOpen => recordImplementation.IsOpen;

	public bool IsTemporary => isTemporary;

	internal RecordImplementation RecordImplementation => recordImplementation;

	private string PrimaryKeyString
	{
		get
		{
			NCLMetaKey keyByIndex = MetaTable.GetKeyByIndex(0);
			if (keyByIndex != null && keyByIndex.KeyFieldCount > 0)
			{
				StringBuilder stringBuilder = new StringBuilder(100);
				for (int i = 0; i < keyByIndex.KeyFieldCount; i++)
				{
					NCLMetaField keyFieldByIndex = keyByIndex.GetKeyFieldByIndex(i);
					NavValue fieldValue = GetFieldValue(keyFieldByIndex);
					string arg = ((fieldValue != null) ? fieldValue.ToString() : string.Empty);
					stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0}={1}", ALFieldCaption(keyFieldByIndex.FieldNo), arg);
					stringBuilder.Append(", ");
				}
				return stringBuilder.ToString(0, stringBuilder.Length - ", ".Length);
			}
			return string.Empty;
		}
	}

	internal NavTableInstanceIdentifier InstanceIdentifier => recordImplementation.InstanceIdentifier;

	public bool HasBeenInserted
	{
		get
		{
			if (!IsTemporary)
			{
				return !GetFieldValue(MetaTable.TimestampField).IsZeroOrEmpty;
			}
			return Exists(ALRecordId);
		}
	}

	public SecurityFiltering SecurityFiltering
	{
		get
		{
			return recordImplementation.SecurityFiltering;
		}
		set
		{
			recordImplementation.SecurityFiltering = value;
		}
	}

	public int? SearchTimeout
	{
		get
		{
			return recordImplementation.SearchTimeout;
		}
		set
		{
			recordImplementation.SearchTimeout = value;
		}
	}

	public bool IsBasedOnSharedTable { get; private set; }

	public bool SuppressEventListeners { get; set; }

	public int[] CurrentSortingFieldIds => recordImplementation.CurrentSortingFieldIds;

	public NavRecord Target => this;

	NavApplicationObjectBase INavApplicationObjectBaseHandle.Target => Target;

	public bool HasTarget => true;

	public IReadOnlyList<NavRecordExtension> OrderedTableExtensions => orderedTableExtensions.AsReadOnly();

	internal override NCLMetaApplicationObject MetaApplicationObject => MetaTable;

	public override Type HandleType => typeof(NavRecordHandle);

	public override string ObjectName
	{
		get
		{
			CheckRecordIsOpenAndNotStale();
			return MetaTable.TableName;
		}
	}

	public event EventHandler<NavRecordEventArgs> Notify;

	public void ALDeleteLink(int linkId)
	{
		CheckTryFunction("DELETELINK");
		CheckRecordIsOpenAndNotStale();
		recordImplementation.DeleteLink(linkId);
	}

	public void ALDeleteLinks()
	{
		CheckTryFunction("DELETELINK");
		CheckRecordIsOpenAndNotStale();
		recordImplementation.DeleteLinks();
	}

	public bool ALFieldExists(int fieldNo)
	{
		CheckRecordIsOpenAndNotStale();
		int fieldIndex;
		return MetaTable.TryGetFieldIndexByNo(fieldNo, out fieldIndex);
	}

	public int ALAddLink(string url)
	{
		return ALAddLink(url, string.Empty);
	}

	public int ALAddLink(string url, string description)
	{
		CheckTryFunction("ADDLINK");
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.AddLink(url, description);
	}

	public void ALValidate(int fieldNo, NavRecord callerRecord)
	{
		ALValidate(fieldNo, null, callerRecord);
	}

	public void ALValidate(int fieldNo, NavValue newValue, NavRecord callerRecord)
	{
		Validate(MetaTable.GetFieldByNo(fieldNo), newValue, callerRecord);
	}

	internal static NavRecord GetCallerRecord()
	{
		NavApplicationObjectBase applicationObject = NavCurrentThread.Session.CurrentMethodScope.ApplicationObject;
		if (applicationObject != null)
		{
			return applicationObject.NavType switch
			{
				NavType.Record => applicationObject as NavRecord, 
				NavType.TableExtension => ((NavRecordExtension)applicationObject).ParentObject, 
				NavType.Form => ((NavForm)applicationObject).SourceTable, 
				NavType.PageExtension => ((NavFormExtension)applicationObject).ParentObject.SourceTable, 
				_ => null, 
			};
		}
		return null;
	}

	public void ALValidateSafe(int fieldNo, NavType expectedType)
	{
		ALValidateSafe(fieldNo, expectedType, null, GetCallerRecord());
	}

	public void ALValidateSafe(int fieldNo, NavType expectedType, NavValue newValue)
	{
		Validate(ValidateExpectedType(fieldNo, expectedType), newValue, GetCallerRecord());
	}

	public void ALValidateSafe(int fieldNo, NavType expectedType, NavRecord callerRecord)
	{
		ALValidateSafe(fieldNo, expectedType, null, callerRecord);
	}

	public void ALValidateSafe(int fieldNo, NavType expectedType, NavValue newValue, NavRecord callerRecord)
	{
		Validate(ValidateExpectedType(fieldNo, expectedType), newValue, callerRecord);
	}

	public virtual string GetAutoFormatString(int fieldNumber)
	{
		return string.Empty;
	}

	public virtual string GetDecimalPlaces(int fieldNumber)
	{
		return string.Empty;
	}

	public int ALRelation(int fieldNo)
	{
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
		int relation = recordImplementation.GetRelation(fieldByNo);
		if (relation == 0)
		{
			throw new NavNCLNoRelationshipException(ALFieldCaption(fieldNo), MetaTable.TableCaptionSafe);
		}
		return relation;
	}

	public void ALModifyAll(int fieldNo, NavValue newValue)
	{
		ALModifyAll(fieldNo, newValue, runTrigger: false);
	}

	public void ALModifyAll(int fieldNo, NavValue newValue, bool runTrigger)
	{
		NCLMetaField fieldByNo = MetaTable.GetFieldByNo(fieldNo);
		ModifyAll(fieldByNo, newValue, runTrigger);
	}

	public void ALModifyAllSafe(int fieldNo, NavType expectedType, NavValue newValue)
	{
		ALModifyAllSafe(fieldNo, expectedType, newValue, runTrigger: false);
	}

	public void ALModifyAllSafe(int fieldNo, NavType expectedType, NavValue newValue, bool runTrigger)
	{
		ModifyAll(ValidateExpectedType(fieldNo, expectedType), newValue, runTrigger);
	}

	public void ALSetRange(int fieldNo)
	{
		ALSetRange(fieldNo, null, null);
	}

	public void ALSetRange(int fieldNo, NavValue fromValue)
	{
		ALSetRange(fieldNo, fromValue, null);
	}

	public void ALSetRange(int fieldNo, NavValue fromValue, NavValue toValue)
	{
		SetRange(MetaTable.GetFieldByNo(fieldNo), fromValue, toValue);
	}

	public void ALSetRangeSafe(int fieldNo, NavType expectedType)
	{
		ALSetRangeSafe(fieldNo, expectedType, null, null);
	}

	public void ALSetRangeSafe(int fieldNo, NavType expectedType, NavValue fromValue)
	{
		ALSetRangeSafe(fieldNo, expectedType, fromValue, null);
	}

	public void ALSetRangeSafe(int fieldNo, NavType expectedType, NavValue fromValue, NavValue toValue)
	{
		SetRange(ValidateExpectedType(fieldNo, expectedType), fromValue, toValue);
	}

	public void ALSetFilter(int fieldNo, string expression, params NavValue[] values)
	{
		if (ALFilterGroup != 6)
		{
			NCLMetaField fieldByNo = MetaTable.GetFieldByNo(fieldNo);
			ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
			SetFilter(fieldByNo, expression, values);
		}
	}

	public string ALGetFilter(int fieldNo)
	{
		CheckRecordIsOpenAndNotStale();
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
		return recordImplementation.GetFieldFilter(fieldByNo);
	}

	public NavValue ALGetRangeMin(int fieldNo)
	{
		CheckRecordIsOpenAndNotStale();
		return GetRangeMin(MetaTable.GetFieldByNo(fieldNo));
	}

	public NavValue ALGetRangeMinSafe(int fieldNo, NavType expectedType)
	{
		CheckRecordIsOpenAndNotStale();
		return GetRangeMin(ValidateExpectedType(fieldNo, expectedType));
	}

	public NavValue ALGetRangeMax(int fieldNo)
	{
		return GetRangeMax(MetaTable.GetFieldByNo(fieldNo));
	}

	public NavValue ALGetRangeMaxSafe(int fieldNo, NavType expectedType)
	{
		return GetRangeMax(ValidateExpectedType(fieldNo, expectedType));
	}

	public void ALCopyFilter(int fromField, NavRecord toRecord, int toField)
	{
		if (toRecord == null)
		{
			throw new ArgumentNullException("toRecord");
		}
		recordImplementation.CopyFilter(metaTable.GetFieldByNo(fromField), toRecord, toRecord.MetaTable.GetFieldByNo(toField));
	}

	public void ALTestField(int fieldNo)
	{
		TestFieldNotBlank(MetaTable.GetFieldByNo(fieldNo));
	}

	public bool ALTestFieldNotBlankOrZero(int fieldNo)
	{
		return !GetFieldValue(MetaTable.GetFieldByNo(fieldNo)).IsZeroOrEmpty;
	}

	public void ALTestFieldMustBeBlankOrZero(int fieldNo)
	{
		TestFieldEqualsNavValue(MetaTable.GetFieldByNo(fieldNo), new NavText(string.Empty));
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType)
	{
		TestFieldNotBlank(ValidateExpectedType(fieldNo, expectedType));
	}

	public void ALTestField(NCLMetaField metaField)
	{
		TestFieldNotBlank(metaField);
	}

	public void ALTestField(int fieldNo, byte testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, byte testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, bool testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, bool testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, int testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, int testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, long testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, long testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, Decimal18 testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, Decimal18 testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, Guid testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, Guid testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestField(int fieldNo, string testValue)
	{
		TestFieldEquals(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldSafe(int fieldNo, NavType expectedType, string testValue)
	{
		TestFieldEquals(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALTestFieldNavValue(int fieldNo, NavValue testValue)
	{
		TestFieldEqualsNavValue(MetaTable.GetFieldByNo(fieldNo), testValue);
	}

	public void ALTestFieldNavValueSafe(int fieldNo, NavType expectedType, NavValue testValue)
	{
		TestFieldEqualsNavValue(ValidateExpectedType(fieldNo, expectedType), testValue);
	}

	public void ALFieldError(int fieldNo)
	{
		ALFieldError(fieldNo, null);
	}

	public void ALFieldError(int fieldNo, string errorText)
	{
		string diagnosticErrorText = GetDiagnosticErrorText(errorText);
		ALFieldError(fieldNo, errorText, diagnosticErrorText);
	}

	public void ALFieldError(int fieldNo, NavTextConstant errorText)
	{
		LogALErrorTelemetry(errorText);
		ALFieldError(fieldNo, errorText, errorText);
	}

	private void ALFieldError(int fieldNo, string originalText, string diagnosticsText)
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.FieldError(metaTable.GetFieldByNo(fieldNo), originalText, diagnosticsText);
	}

	private static string GetDiagnosticErrorText(string errorText)
	{
		if (string.IsNullOrEmpty(errorText))
		{
			return string.Empty;
		}
		LogALErrorTelemetry("Use FIELDERROR with a text constant to improve Telemetry details.");
		return "Use FIELDERROR with a text constant to improve Telemetry details.";
	}

	private static void LogALErrorTelemetry(string errorMessage)
	{
		if (!string.IsNullOrEmpty(errorMessage))
		{
			string aLCallStack = SessionContextHelper.GetALCallStack();
			DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000022I", Category.AL, Verbosity.Normal, PrivacyClassification.SystemMetadata, "ALError: {0}\r\n{1}", errorMessage, aLCallStack);
		}
	}

	public string ALFieldName(int fieldNo)
	{
		return MetaTable.GetFieldByNo(fieldNo).FieldName;
	}

	public string ALFieldCaption(int fieldNo)
	{
		if (NavCurrentThread.Session.PushDynamicCaptionStack(TableID, fieldNo))
		{
			try
			{
				NavText dynamicCaption = GetDynamicCaption(fieldNo);
				if (dynamicCaption != NavOemText.Empty)
				{
					return dynamicCaption.Value;
				}
			}
			finally
			{
				NavCurrentThread.Session.PopDynamicCaptionStack();
			}
		}
		return MetaTable.GetFieldByNo(fieldNo).FieldCaption;
	}

	public bool ALFieldActive(int fieldNo)
	{
		return MetaTable.GetFieldByNo(fieldNo).FieldActive;
	}

	public int ALFieldNo(string field)
	{
		return MetaTable.GetFieldNoByName(field);
	}

	public int ALFieldNo(int fieldNo)
	{
		return fieldNo;
	}

	public void ALSetAscending(int fieldNo, bool ascending)
	{
		recordImplementation.SetFieldAscending(fieldNo, ascending);
	}

	public bool ALGetAscending(int fieldNo)
	{
		return recordImplementation.GetFieldAscending(fieldNo);
	}

	public bool ALGet(NavRecordId recordId)
	{
		return ALGet(DataError.ThrowError, recordId);
	}

	public bool ALGet(DataError errorLevel, NavRecordId recordId)
	{
		return Get(errorLevel, recordId, copyRecord: true);
	}

	public bool ALMark()
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.IsMarked();
	}

	public bool ALMark(bool markValue)
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.SetMarked(markValue);
		return markValue;
	}

	public void ALInit()
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.InitRecord();
	}

	public void ALFind()
	{
		ALFind(DataError.ThrowError, "=");
	}

	public bool ALFind(DataError errorLevel)
	{
		return ALFind(errorLevel, "=");
	}

	public void ALFind(string searchMethod)
	{
		ALFind(DataError.ThrowError, searchMethod);
	}

	public bool ALFind(DataError errorLevel, string searchMethod)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool flag = recordImplementation.FindRecord(errorLevel, searchMethod);
		if (flag && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return flag;
	}

	public void ALFindFirst()
	{
		ALFindFirst(DataError.ThrowError);
	}

	public bool ALFindFirst(DataError errorLevel)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool flag = recordImplementation.FindFirstRecord(errorLevel);
		if (flag && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return flag;
	}

	public void ALFindLast()
	{
		ALFindLast(DataError.ThrowError);
	}

	public bool ALFindLast(DataError errorLevel)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool flag = recordImplementation.FindLastRecord(errorLevel);
		if (flag && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return flag;
	}

	public void ALFindSet()
	{
		ALFindSet(DataError.ThrowError, forUpdate: false, updateKey: false);
	}

	public bool ALFindSet(DataError errorLevel)
	{
		return ALFindSet(errorLevel, forUpdate: false, updateKey: false);
	}

	public void ALFindSet(bool forUpdate)
	{
		ALFindSet(DataError.ThrowError, forUpdate, updateKey: false);
	}

	public bool ALFindSet(DataError errorLevel, bool forUpdate)
	{
		return ALFindSet(errorLevel, forUpdate, updateKey: false);
	}

	public void ALFindSet(bool forUpdate, bool updateKey)
	{
		ALFindSet(DataError.ThrowError, forUpdate, updateKey);
	}

	public bool ALFindSet(DataError errorLevel, bool forUpdate, bool updateKey)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool flag = recordImplementation.FindRecordSet(errorLevel, forUpdate);
		if (flag && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return flag;
	}

	public int ALNext()
	{
		return ALNext(1);
	}

	public int ALNext(int steps)
	{
		return Next(steps, 0);
	}

	internal int NextWithoutPrefetch(int steps)
	{
		return Next(steps, Math.Abs(steps));
	}

	private int Next(int steps, int setSize)
	{
		CheckRecordIsOpenAndNotStale();
		if (steps == 0)
		{
			return 0;
		}
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		int num = recordImplementation.NextRecord(steps, setSize);
		if (num != 0 && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return num;
	}

	public void ALReset()
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		if (globalVariablesInitialized)
		{
			ClearAll();
		}
		recordImplementation.ResetRecord();
		if (Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
	}

	public void ALLockTable()
	{
		ALLockTable(wait: true, versionCheck: false);
	}

	public void ALLockTable(bool wait)
	{
		ALLockTable(wait, versionCheck: false);
	}

	public void ALLockTable(bool wait, bool versionCheck)
	{
		CheckTryFunction("LOCKTABLE");
		CheckRecordIsOpenAndNotStale();
		recordImplementation.LockTable(wait, versionCheck);
	}

	public void ALConsistent(bool isConsistent)
	{
		CheckRecordIsOpenAndNotStale();
		if (isConsistent)
		{
			Session.SetRecordConsistent(this);
		}
		else
		{
			Session.SetRecordInconsistent(this);
		}
	}

	public string ALGetPosition()
	{
		return ALGetPosition(useCaptions: true);
	}

	public string ALGetPosition(bool useCaptions)
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetPosition(useCaptions);
	}

	public void ALSetPosition(string position)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		recordImplementation.SetPosition(position);
		if (Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
	}

	public string ALGetView()
	{
		return ALGetView(useCaptions: true);
	}

	public string ALGetView(bool useCaptions)
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetView(useCaptions);
	}

	public void ALSetRecFilter()
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.SetFilterFromCurrentKey();
	}

	public void ALSetPermissionFilter()
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.SetPermissionFilter();
	}

	public void ALClearMarks()
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.ClearMarks();
	}

	public void ALGet(params NavValue[] values)
	{
		ALGet(DataError.ThrowError, values);
	}

	public void ALGetSafe(int compilerHashCode, params NavValue[] values)
	{
		ALGetSafe(DataError.ThrowError, compilerHashCode, values);
	}

	public bool ALGet(DataError errorLevel, params NavValue[] values)
	{
		CheckRecordIsOpenAndNotStale();
		if (values == null)
		{
			throw new ArgumentNullException("values");
		}
		if (values.Length == 1 && values[0] is NavRecordId)
		{
			return ALGet(errorLevel, (NavRecordId)values[0]);
		}
		NCLMetaKey keyByIndex = metaTable.GetKeyByIndex(0);
		if (keyByIndex.KeyFieldCount < values.Length)
		{
			throw new NavCSideException(23396552, string.Format(CultureInfo.CurrentCulture, Lang.PrimaryKeysLessThanValues, metaTable.TableName, keyByIndex.KeyFieldCount));
		}
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool record = recordImplementation.GetRecord(errorLevel, values);
		if (record && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return record;
	}

	public bool ALGetSafe(DataError errorLevel, int compilerHashCode, params NavValue[] values)
	{
		ValidateExpectedPrimaryKey(compilerHashCode);
		return ALGet(errorLevel, values);
	}

	public bool ALSetCurrentKey(params int[] fields)
	{
		return ALSetCurrentKey(DataError.ThrowError, fields);
	}

	public bool ALSetCurrentKey(DataError errorLevel, params int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		NCLMetaField[] array = new NCLMetaField[fields.Length];
		for (int i = 0; i < fields.Length; i++)
		{
			array[i] = metaTable.GetFieldByNo(fields[i]);
		}
		return recordImplementation.SetCurrentKey(errorLevel, array);
	}

	public bool ALCalcFields(params int[] fields)
	{
		return ALCalcFields(DataError.ThrowError, fields);
	}

	public bool ALCalcFields(DataError errorLevel, params int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		return CalcFields(errorLevel, FieldsWithoutIntellectualPropertyFields(fields));
	}

	public bool ALSetAutoCalcFields(params int[] fields)
	{
		return ALSetAutoCalcFields(DataError.ThrowError, fields);
	}

	public bool ALSetAutoCalcFields(DataError errorLevel, params int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		return SetAutoCalcFields(errorLevel, FieldsWithoutIntellectualPropertyFields(fields), replace: false);
	}

	public bool ALCalcSums(params int[] fields)
	{
		return ALCalcSums(DataError.ThrowError, fields);
	}

	public bool ALCalcSums(DataError errorLevel, params int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		NCLMetaField[] metaFields = GetMetaFields(errorLevel, fields);
		if (metaFields == null)
		{
			return false;
		}
		try
		{
			return recordImplementation.CalcSums(metaFields);
		}
		catch (NavErrorCodeException ex) when (errorLevel == DataError.TrapError && ex.ErrorNumber == 1005)
		{
			return false;
		}
	}

	public bool ALChangeCompany()
	{
		return ALChangeCompany(DataError.ThrowError, string.Empty);
	}

	public bool ALChangeCompany(DataError errorLevel)
	{
		return ALChangeCompany(errorLevel, string.Empty);
	}

	public bool ALChangeCompany(string companyName)
	{
		return ALChangeCompany(DataError.ThrowError, companyName);
	}

	public bool ALChangeCompany(DataError errorLevel, string companyName)
	{
		bool flag = recordImplementation.ChangeCompany(errorLevel, companyName);
		if (flag)
		{
			currentCompanyName = companyName;
			if (oldRecordBase != null)
			{
				oldRecordBase.ALChangeCompany(errorLevel, companyName);
			}
		}
		return flag;
	}

	public void ALInsert()
	{
		ALInsert(DataError.ThrowError, runApplicationTrigger: false);
	}

	public bool ALInsert(DataError errorLevel)
	{
		return ALInsert(errorLevel, runApplicationTrigger: false);
	}

	public void ALInsert(bool runApplicationTrigger)
	{
		ALInsert(DataError.ThrowError, runApplicationTrigger);
	}

	public bool ALInsert(DataError errorLevel, bool runApplicationTrigger)
	{
		RecordState recordState = null;
		bool flag = (runApplicationTrigger && MetaTable.IsInsertTriggerDefined) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeInsertEvent) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnAfterInsertEvent) || IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseInsert);
		if (flag)
		{
			recordState = OldRecord.CreateRecordState();
			recordState.BackupRecordHandle();
			OldRecord.ALAssign(this);
		}
		try
		{
			return Insert(errorLevel, runApplicationTrigger, runGlobalTrigger: false);
		}
		finally
		{
			if (flag)
			{
				recordState.RestoreRecord();
			}
		}
	}

	public void ALDelete()
	{
		ALDelete(DataError.ThrowError, runApplicationTrigger: false, isBulkDelete: false);
	}

	public bool ALDelete(DataError errorLevel)
	{
		return ALDelete(errorLevel, runApplicationTrigger: false, isBulkDelete: false);
	}

	public bool ALDelete(bool runApplicationTrigger)
	{
		return ALDelete(DataError.ThrowError, runApplicationTrigger, isBulkDelete: false);
	}

	public bool ALDelete(DataError errorLevel, bool runApplicationTrigger)
	{
		return ALDelete(errorLevel, runApplicationTrigger, isBulkDelete: false);
	}

	public bool ALDelete(DataError errorLevel, bool runApplicationTrigger, bool isBulkDelete)
	{
		RecordState recordState = null;
		bool flag = (runApplicationTrigger && MetaTable.IsDeleteTriggerDefined) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeDeleteEvent) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnAfterDeleteEvent) || IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseDelete);
		if (flag)
		{
			recordState = OldRecord.CreateRecordState();
			recordState.BackupRecordHandle();
			OldRecord.ALAssign(this);
		}
		try
		{
			return Delete(errorLevel, runApplicationTrigger, isCalledFromUI: false, isBulkDelete);
		}
		finally
		{
			if (flag)
			{
				recordState.RestoreRecord();
			}
		}
	}

	public void ALDeleteAll()
	{
		ALDeleteAll(runTrigger: false);
	}

	public void ALDeleteAll(bool runTrigger)
	{
		DeleteAll(runTrigger);
	}

	public bool ALModify()
	{
		return ALModify(DataError.ThrowError, runApplicationTrigger: false);
	}

	public bool ALModify(DataError errorLevel)
	{
		return ALModify(errorLevel, runApplicationTrigger: false, isBulkModify: false);
	}

	public void ALModify(bool runApplicationTrigger)
	{
		ALModify(DataError.ThrowError, runApplicationTrigger, isBulkModify: false);
	}

	public bool ALModify(DataError errorLevel, bool runApplicationTrigger)
	{
		return ALModify(errorLevel, runApplicationTrigger, isBulkModify: false);
	}

	public bool ALModify(DataError errorLevel, bool runApplicationTrigger, bool isBulkModify)
	{
		CheckRecordIsOpenAndNotStale();
		RecordState recordState = null;
		bool flag = (runApplicationTrigger && MetaTable.IsModifyTriggerDefined) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeModifyEvent) || MetaTable.IsEventSubscribed(NavTriggerEventType.OnAfterModifyEvent) || IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseModify);
		if (flag)
		{
			recordState = OldRecord.CreateRecordState();
			recordState.BackupRecordHandle();
			OldRecord.ALAssign(this);
		}
		try
		{
			return Modify(errorLevel, runApplicationTrigger, runGlobalTrigger: false, isBulkModify);
		}
		finally
		{
			if (flag)
			{
				recordState.RestoreRecord();
			}
		}
	}

	public void ALRename(params NavValue[] values)
	{
		ALRename(DataError.ThrowError, values);
	}

	public virtual bool ALRename(DataError errorLevel, params NavValue[] values)
	{
		return Rename(errorLevel, runApplicationTrigger: true, runGlobalTrigger: true, values);
	}

	public void ALRenameSafe(int compilerHashCode, params NavValue[] values)
	{
		ALRenameSafe(DataError.ThrowError, compilerHashCode, values);
	}

	public bool ALRenameSafe(DataError errorLevel, int compilerHashCode, params NavValue[] values)
	{
		ValidateExpectedPrimaryKey(compilerHashCode);
		return ALRename(errorLevel, values);
	}

	public void ALCopy(NavRecord fromRecord)
	{
		CopyRecord(fromRecord);
	}

	public void ALCopy(NavRecord fromRecord, bool shareTableHandle)
	{
		CopyRecord(fromRecord, shareTableHandle);
	}

	public void ALCopyLinks(NavRecord fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		CheckRecordIsOpenAndNotStale();
		recordImplementation.CopyLinks(fromRecord);
	}

	public void ALCopyLinks(NavRecordRef fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		CheckRecordIsOpenAndNotStale();
		recordImplementation.CopyLinks(fromRecord.Record);
	}

	public void ALCopyFilters(NavRecord fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		recordImplementation.CopyFilters(fromRecord);
	}

	public void ALTransferFields(NavRecord fromRecord)
	{
		ALTransferFields(fromRecord, initPrimaryKeyFields: true);
	}

	public void ALTransferFields(NavRecord fromRecord, bool initPrimaryKeyFields)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		recordImplementation.TransferFields(fromRecord, initPrimaryKeyFields);
		if (Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
	}

	public void ALSetView(string view)
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.SetView(view);
	}

	private void ValidateExpectedPrimaryKey(int compilerHashCode)
	{
		CheckRecordIsOpenAndNotStale();
		NCLMetaKey keyByIndex = MetaTable.GetKeyByIndex(0);
		if (compilerHashCode != keyByIndex.CompilerHashCode)
		{
			throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, MetaTable.TableCaptionSafe);
		}
	}

	private NCLMetaField ValidateExpectedType(int fieldNo, NavType expectedType)
	{
		CheckRecordIsOpenAndNotStale();
		NCLMetaField fieldByNo = MetaTable.GetFieldByNo(fieldNo);
		ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
		if (fieldByNo.FieldNavType != expectedType)
		{
			throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, fieldByNo.FieldName, expectedType, fieldByNo.FieldNavType);
		}
		return fieldByNo;
	}

	private NCLMetaField ValidateExpectedType(int extensionObjectId, int fieldNo, NavType expectedType)
	{
		CheckRecordIsOpenAndNotStale();
		NCLMetaField fieldByNo = MetaTable.GetFieldByNo(extensionObjectId, fieldNo);
		ObsolescenceGuard.ThrowIfObsoleted(fieldByNo);
		if (fieldByNo.FieldNavType != expectedType)
		{
			throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, fieldByNo.FieldName, expectedType, fieldByNo.FieldNavType);
		}
		return fieldByNo;
	}

	protected virtual void DeleteAll(bool runApplicationTrigger)
	{
		CheckTryFunction("DELETEALL");
		CheckRecordIsOpenAndNotStale();
		DataModificationListener.Instance?.OnBeforeDeleteAll(this);
		bool flag = metaTable.NormalFieldsOwnedByCompanionTables.Any() && !RecordImplementation.TableState.FiltersAndMarks.IsEmpty;
		if ((!runApplicationTrigger || !metaTable.IsDeleteTriggerDefined) && (!runApplicationTrigger || !IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalDelete)) && !IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseDelete) && !metaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeDeleteEvent) && !metaTable.IsEventSubscribed(NavTriggerEventType.OnAfterDeleteEvent) && !SystemTableTriggers.TableHasSystemDeleteTrigger(TableID) && !recordImplementation.RequiresSecurityFiltersValidation(SecurityFilterType.Delete) && metaTable.MediaFieldCount <= 0 && !flag)
		{
			RecordImplementation.DeleteAllRecords();
			if (Notify != null)
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Delete));
			}
			ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.DeleteAll, this, null);
			return;
		}
		using NavRecord navRecord = CloneRecord(this, reset: false, keepCompany: true);
		navRecord.ALLockTable();
		if (navRecord.ALFind(DataError.TrapError, "-"))
		{
			do
			{
				navRecord.ALDelete(DataError.ThrowError, runApplicationTrigger, isBulkDelete: true);
			}
			while (navRecord.NextRecord());
		}
		if (!navRecord.IsTemporary && !navRecord.IsBasedOnSharedTable && !navRecord.MetaTable.IsVirtualTable)
		{
			navRecord.RecordImplementation.MediaPurger.PurgeMediaLinks();
		}
	}

	private static bool CanUseBulkModifyAll(bool runApplicationTrigger, NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		NCLMetaTable nCLMetaTable = record.MetaTable;
		if ((!runApplicationTrigger || !nCLMetaTable.IsModifyTriggerDefined) && (!runApplicationTrigger || !record.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalModify)) && !record.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseModify) && !nCLMetaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeModifyEvent) && !nCLMetaTable.IsEventSubscribed(NavTriggerEventType.OnAfterModifyEvent) && !SystemTableTriggers.TableHasSystemModifyTrigger(nCLMetaTable.TableId))
		{
			if (!record.recordImplementation.RequiresSecurityFiltersValidation(SecurityFilterType.Modify) && nCLMetaTable.MediaFieldCount <= 0)
			{
				return !nCLMetaTable.NormalFieldsOwnedByCompanionTables.Any();
			}
			return false;
		}
		return false;
	}

	private static bool CanUseBulkRenameAll(bool runApplicationTrigger, NavRecord record)
	{
		if (record == null)
		{
			throw new ArgumentNullException("record");
		}
		NCLMetaTable nCLMetaTable = record.MetaTable;
		if ((!runApplicationTrigger || !nCLMetaTable.IsRenameTriggerDefined) && (!runApplicationTrigger || !record.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.GlobalRename)) && !record.IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers.DatabaseRename) && !nCLMetaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeRenameEvent) && !nCLMetaTable.IsEventSubscribed(NavTriggerEventType.OnAfterRenameEvent) && nCLMetaTable.MediaFieldCount <= 0 && nCLMetaTable.GetReferencingRelations().Length == 0 && !record.RecordImplementation.TableHasLinks())
		{
			return !nCLMetaTable.NormalFieldsOwnedByCompanionTables.Any();
		}
		return false;
	}

	protected virtual void ModifyAll(NCLMetaField metaField, NavValue newValue, bool runApplicationTrigger)
	{
		CheckTryFunction("MODIFYALL");
		CheckRecordIsOpenAndNotStale();
		DataModificationListener.Instance?.OnBeforeModifyAll(this);
		if (newValue.NclType != metaField.FieldNclType)
		{
			newValue = NavValue.CreateNavValueFromObject(metaField, newValue);
		}
		if (CanUseBulkModifyAll(runApplicationTrigger, this))
		{
			RecordImplementation.ModifyAllRecords(metaField, newValue);
			if (Notify != null)
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Modify));
			}
			ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.ModifyAll, this, null);
			return;
		}
		using NavRecord navRecord = CloneRecord(this, reset: false, keepCompany: true);
		navRecord.ALLockTable();
		if (navRecord.ALFind(DataError.TrapError, "-"))
		{
			if (metaField.FieldIsPartOfPrimaryKey)
			{
				throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.CannotModifyAllOnPrimaryKey, ALFieldCaption(metaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
			}
			do
			{
				using NavRecord navRecord2 = navRecord.CloneRecord(navRecord, reset: false, keepCompany: true);
				navRecord2.SetFieldValue(metaField, newValue);
				navRecord2.ALModify(DataError.ThrowError, runApplicationTrigger, isBulkModify: true);
			}
			while (navRecord.NextRecord());
		}
		if (!navRecord.IsTemporary && !navRecord.IsBasedOnSharedTable && !navRecord.MetaTable.IsVirtualTable)
		{
			navRecord.recordImplementation.MediaPurger.PurgeMediaLinks();
		}
	}

	private void CloseRecord()
	{
		if (IsOpen && oldRecordBase != null)
		{
			oldRecordBase.Dispose();
			oldRecordBase = null;
		}
	}

	internal void CopyRecord(NavRecord fromRecord)
	{
		CopyRecord(fromRecord, shareTableHandle: false);
	}

	private void CopyRecord(NavRecord fromRecord, bool shareTableHandle)
	{
		CheckRecordIsOpenAndNotStale();
		if (RecordImplementation.Equals(fromRecord.RecordImplementation))
		{
			return;
		}
		CopyRecordOptions copyRecordOptions = CopyRecordOptions.CopyFiltersSortingMarked;
		if (metaTable != fromRecord.metaTable)
		{
			throw new NavNCLArgumentException(Lang.TargetAndSourceTableAreNotTheSame);
		}
		if (shareTableHandle)
		{
			if (!IsTemporary || !fromRecord.IsTemporary)
			{
				throw new NavNCLArgumentException(Lang.ShareTableHandleOnlyAllowedForTemporaryRecords);
			}
			copyRecordOptions |= CopyRecordOptions.ShareTableData;
		}
		RecordImplementation.CopyRecord(fromRecord, copyRecordOptions);
	}

	private bool IsFilterGroupAllowed(FilterGroupType filterGroupType)
	{
		switch (filterGroupType)
		{
		case FilterGroupType.OrFilters:
		case FilterGroupType.UserFilters:
		case FilterGroupType.TableViewFilters:
		case FilterGroupType.SubFormLinkFilters:
		case FilterGroupType.TempFilters:
			return true;
		case FilterGroupType.FormViewFilters:
			return !ALHasFilter;
		default:
			return false;
		}
	}

	private void SetTableViewFilters(NavFilterGroup[] filterGroups, Predicate<FilterGroupType> condition)
	{
		if (filterGroups == null)
		{
			return;
		}
		foreach (NavFilterGroup navFilterGroup in filterGroups)
		{
			if (navFilterGroup.Filters == null)
			{
				continue;
			}
			int aLFilterGroup = ALFilterGroup;
			try
			{
				ALFilterGroup = navFilterGroup.FilterGroupNo;
				if (condition != null && !condition((FilterGroupType)navFilterGroup.FilterGroupNo))
				{
					continue;
				}
				NavFilter[] filters = navFilterGroup.Filters;
				foreach (NavFilter navFilter in filters)
				{
					NCLMetaField fieldByNo = metaTable.GetFieldByNo(navFilter.FilterField);
					if (navFilter.IsExactValue)
					{
						NavValue fromValue;
						if (navFilter.FilterValue == "''")
						{
							fromValue = fieldByNo.EmptyValue;
						}
						else
						{
							fromValue = ((fieldByNo.FieldNclType != NavNclType.NavOption) ? ALSystemVariable.EvaluateIntoNavValue(DataError.ThrowError, fieldByNo, navFilter.FilterValue, 0) : NavOption.CreateFromObject(fieldByNo.FieldOptionMetadata, navFilter.FilterValue));
						}
						SetRange(fieldByNo, fromValue, null);
					}
					else if (navFilterGroup.FilterGroupNo != -1)
					{
						SetFilter(fieldByNo, navFilter.FilterValue);
					}
					else
					{
						TrySetFilter(fieldByNo, navFilter.FilterValue);
					}
				}
			}
			finally
			{
				ALFilterGroup = aLFilterGroup;
			}
		}
	}

	public NavRecord CloneRecord(ITreeObject parentOfClone, bool reset = false, bool keepCompany = false)
	{
		CheckRecordIsOpenAndNotStale();
		NavRecord navRecord;
		if (IsTemporary)
		{
			navRecord = MetaTable.CreateObjectInstance(parentOfClone, isTemporary: true, this, string.Empty, RecordImplementation.SecurityFiltering);
		}
		else
		{
			navRecord = ((!(GetType() == typeof(NavRecord))) ? MetaTable.CreateObjectInstance(parentOfClone, isTemporary: false, null, keepCompany ? recordImplementation.GetActiveCompany() : string.Empty, RecordImplementation.SecurityFiltering) : new NavRecord(parentOfClone, TableID, MetaTable, isTemporary: false, null, keepCompany ? recordImplementation.GetActiveCompany() : string.Empty, RecordImplementation.SecurityFiltering));
		}
		navRecord.CopyRecord(this);
		if (reset)
		{
			navRecord.ALReset();
		}
		return navRecord;
	}

	private void ValidateField(FieldTriggerHandler<NavApplicationObjectBase> fieldEvent, NCLMetaField metaField, bool isUserInput, List<FieldTriggerHandler<NavApplicationObjectBase>> beforeHandlers, List<FieldTriggerHandler<NavApplicationObjectBase>> afterHandlers)
	{
		if (metaField.FieldNo != 0)
		{
			RecordImplementation.ValidateNonFlowField(metaField, isUserInput);
		}
		WriteEventRaised(NavTriggerEventType.OnBeforeValidateEvent, metaField.FieldName);
		if (metaField.IsEventSubscribed(NavTriggerEventType.OnBeforeValidateEvent))
		{
			NavTableTriggerEventHandler.FireOnValidateEvent(NavTriggerEventType.OnBeforeValidateEvent, this, metaField);
		}
		beforeHandlers.ForEach(InvokeFieldTriggerHandler);
		InvokeFieldTriggerHandler(fieldEvent);
		afterHandlers.ForEach(InvokeFieldTriggerHandler);
		WriteEventRaised(NavTriggerEventType.OnAfterValidateEvent, metaField.FieldName);
		if (metaField.IsEventSubscribed(NavTriggerEventType.OnAfterValidateEvent))
		{
			NavTableTriggerEventHandler.FireOnValidateEvent(NavTriggerEventType.OnAfterValidateEvent, this, metaField);
		}
		if (metaField.FieldNo != 0 && metaField.FieldClass == FieldClass.FlowField)
		{
			ValidateFlowField(metaField);
		}
	}

	private void ValidateFlowField(NCLMetaField metaField)
	{
		if (metaField.FieldNavType != NavType.Decimal)
		{
			throw DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001X8", Category.Runtime, new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The type of the {0} FlowField in the {1} table must be Decimal.", ALFieldCaption(metaField.FieldNo), MetaTable.TableCaptionSafe)), Verbosity.Normal, DiagnosticParameter.SystemMetadata(string.Format(CultureInfo.InvariantCulture, "Cannot validate a flowfield of field type {0}", metaField.FieldNavType)));
		}
		NCLMetaCalculationFormula calculationFormula = metaField.CalculationFormula;
		if (calculationFormula.CalculationMethod != NCLMetaCalculationMethod.Sum)
		{
			throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The Calculation formula for the {0} FlowField in the {1} table must start with 'Sum(...'.", ALFieldCaption(metaField.FieldNo), MetaTable.TableCaptionSafe));
		}
		NCLMetaField sourceField = calculationFormula.SourceField;
		NCLMetaTable parent = sourceField.Parent;
		if (sourceField.FieldClass != FieldClass.Normal)
		{
			throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The FieldClass for the {0} field in the {1} table must be Normal.", ALFieldCaption(sourceField.FieldNo), parent.TableCaptionSafe));
		}
		if (sourceField.FieldNavType != NavType.Decimal)
		{
			throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The type of the {0} field in the {1} table must be Decimal.", ALFieldCaption(sourceField.FieldNo), parent.TableCaptionSafe));
		}
		using NavRecord navRecord = parent.CreateObjectInstance(this, IsTemporary, null, recordImplementation.GetActiveCompany(), RecordImplementation.SecurityFiltering);
		NavDecimal navDecimal = (NavDecimal)GetFieldValue(metaField);
		ALCalcFields(metaField.FieldNo);
		NavDecimal navDecimal2 = (NavDecimal)GetFieldValue(metaField);
		Decimal18 @decimal = Decimal18.Subtract(navDecimal, navDecimal2);
		SetFieldValue(metaField, navDecimal);
		if (@decimal == Decimal18.Zero)
		{
			return;
		}
		if (calculationFormula.NegateResult)
		{
			@decimal = Decimal18.Negate(@decimal);
		}
		int[] fieldNumbers = recordImplementation.PrepareSubTableFromFlowField(metaField, navRecord);
		navRecord.ValidateFields(fieldNumbers);
		navRecord.Validate(sourceField, NavDecimal.Create(@decimal), navRecord);
		if (!navRecord.recordImplementation.RecordCompliesWithFilters())
		{
			throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.CurrentCulture, Lang.ValidateFlowFieldDifferenceOutsideFilter, ALFieldCaption(metaField.FieldNo), metaTable.TableCaptions.GetValueOrDefault()));
		}
		IList<NCLMetaField> keyFieldsList = parent.GetKeyByIndex(0).KeyFieldsList;
		if (keyFieldsList.Count != 1 || keyFieldsList[0].FieldNavType != NavType.Integer)
		{
			throw new NavNCLFlowFieldValidationException(string.Format(CultureInfo.InvariantCulture, "The primary key in the {0} table can contain only one field, and its type must be Integer.", parent.TableCaptionSafe));
		}
		int num = 0;
		using (NavRecord navRecord2 = parent.CreateObjectInstance(this, IsTemporary, null, recordImplementation.GetActiveCompany(), SecurityFiltering.Ignored))
		{
			if (navRecord2.ALFindLast(DataError.TrapError))
			{
				num = navRecord2.GetFieldValue(keyFieldsList[0]).ToInt32();
			}
		}
		navRecord.SetFieldValue(keyFieldsList[0], NavInteger.Create(num + 1));
		navRecord.ALInsert(runApplicationTrigger: true);
	}

	private void InvokeFieldTriggerHandler(FieldTriggerHandler<NavApplicationObjectBase> handler)
	{
		handler?.HandlerAction?.Invoke((NavApplicationObjectBase)(((object)orderedTableExtensions.FirstOrDefault((NavRecordExtension ext) => ext.GetType() == handler.HandlerType)) ?? ((object)this)));
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && !Tree.IsDisposed && metaTable != null && recordImplementation != null)
			{
				if ((NavCurrentThread.SessionId != -1 && RequiredSessionId != NavCurrentThread.SessionId) || (Tree.Session != null && RequiredSessionId != Tree.Session.Id))
				{
					DiagnosticsResolver.GetMostSpecificInstance().SendTraceTag("000017E", Category.Runtime, Verbosity.Error, "Disposing Record in the wrong session, created in sessionId:{0}, current sessionId:{1}, parent tree sessionId:{2}, company:{3}", DiagnosticParameter.SystemMetadata(RequiredSessionId), DiagnosticParameter.SystemMetadata(NavCurrentThread.SessionId), DiagnosticParameter.SystemMetadata(Tree.Session.Id), DiagnosticParameter.Oii(Tree.Session.CompanyName));
				}
				Tree.DisposeAllChildrenExcept(recordImplementation);
				oldRecordBase = null;
				CloseRecord();
				recordImplementation.Dispose();
			}
			parentCompany = null;
			metaTable = null;
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void CheckRecordIsOpenAndNotStale()
	{
		if (MetaTable != null)
		{
			if (MetaTable.Stale)
			{
				throw new NavObjectDefinitionChangedException(CultureInfo.CurrentCulture, NavCurrentThread.Session.GetObjectName(MetaTable.ApplicationObjectId), MetaTable.ApplicationObjectType);
			}
			MetaTable.CheckApplicationObjectIsValid(this);
		}
		if (!IsOpen)
		{
			throw new NavNCLRecordNotOpenedException();
		}
	}

	protected virtual void OnInsert()
	{
	}

	protected virtual void OnModify()
	{
	}

	protected virtual void OnDelete()
	{
	}

	protected virtual void OnRename()
	{
	}

	protected virtual void InitializeGlobalVariables()
	{
	}

	public NavRecord(ITreeObject parent, int tableId, SecurityFiltering securityFiltering = SecurityFiltering.Validated)
		: this(parent, tableId, null, isTemporary: false, null, string.Empty, securityFiltering)
	{
	}

	public NavRecord(ITreeObject parent, int tableId, NCLMetaTable metaTable, bool isTemporary, NavRecord sharedTable, string companyName, SecurityFiltering securityFiltering = SecurityFiltering.Validated)
		: base(parent, new ApplicationObjectId(ObjectType.Table, tableId))
	{
		RequiredSessionId = Session?.Id ?? (-1);
		if (metaTable == null)
		{
			metaTable = ParentSession.NCLMetadata.GetMetaTableById(tableId, -1, requireCompiled: false);
		}
		ObsolescenceGuard.ThrowIfObsoleted(metaTable);
		this.metaTable = metaTable;
		parentCompany = Session?.Company;
		currentCompanyName = companyName;
		this.isTemporary = isTemporary;
		recordImplementation = new RecordImplementation(this);
		if (sharedTable != null)
		{
			NavRecord navRecord = parent as NavRecord;
			if (!isTemporary && (navRecord == null || navRecord != sharedTable))
			{
				throw new NavNCLWrongParentRecordException();
			}
		}
		IsBasedOnSharedTable = sharedTable != null;
		if (IsBasedOnSharedTable)
		{
			recordImplementation.Initialize(isTemporary, sharedTable, securityFiltering);
		}
		else
		{
			recordImplementation.Initialize(companyName ?? string.Empty, securityFiltering);
		}
	}

	public override int GetHashCode()
	{
		return recordImplementation.GetHashCode();
	}

	public override void Clear()
	{
		if (globalVariablesInitialized)
		{
			base.Clear();
		}
		if (IsOpen)
		{
			ALChangeCompany();
			recordImplementation.ClearRecord();
		}
	}

	public bool CompareAllNormalFields(NavRecord fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		CheckRecordIsOpenAndNotStale();
		fromRecord.CheckRecordIsOpenAndNotStale();
		if (this == fromRecord)
		{
			return true;
		}
		if (metaTable != fromRecord.metaTable)
		{
			ThrowUnableToCompareException(fromRecord);
		}
		foreach (NCLMetaField normalField in metaTable.NormalFields)
		{
			if (normalField.FieldIndex != 0 && !GetFieldValue(normalField).Equals(fromRecord.GetFieldValue(normalField)))
			{
				return false;
			}
		}
		return true;
	}

	protected internal override void EnsureGlobalVariablesInitialized()
	{
		if (!globalVariablesInitialized)
		{
			InitializeGlobalVariables();
			CallInitializeGlobalVariablesExtensionMethod();
			globalVariablesInitialized = true;
		}
	}

	public virtual bool Insert(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		CheckTryFunction("INSERT");
		CheckRecordIsOpenAndNotStale();
		DataModificationListener.Instance?.OnBeforeInsert(this);
		WriteEventRaised(NavTriggerEventType.OnBeforeInsertEvent);
		if (metaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeInsertEvent) && !SuppressEventListeners)
		{
			EnsureGlobalVariablesInitialized();
			metaTable.TableTriggerEventHandler.OnBeforeInsertEvent(this, runApplicationTrigger);
		}
		if (runApplicationTrigger && metaTable.IsInsertTriggerDefined)
		{
			EnsureGlobalVariablesInitialized();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnBeforeInsert();
			});
			OnInsert();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnInsert();
			});
		}
		NavGlobalTriggers.Insert(this, runGlobalTrigger);
		bool flag = recordImplementation.InsertRecord(errorLevel);
		if (flag)
		{
			if (runApplicationTrigger)
			{
				EnsureGlobalVariablesInitialized();
				orderedTableExtensions.ForEach((NavRecordExtension ext) =>
				{
					ext.OnAfterInsert();
				});
			}
			ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.Insert, this, null);
			if (Notify != null)
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Insert));
			}
			WriteEventRaised(NavTriggerEventType.OnAfterInsertEvent);
			if (metaTable.IsEventSubscribed(NavTriggerEventType.OnAfterInsertEvent) && !SuppressEventListeners)
			{
				EnsureGlobalVariablesInitialized();
				metaTable.TableTriggerEventHandler.OnAfterInsertEvent(this, runApplicationTrigger);
			}
		}
		return flag;
	}

	public virtual bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger)
	{
		return Modify(errorLevel, runApplicationTrigger, runGlobalTrigger, isBulkModify: false);
	}

	public virtual bool Modify(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger, bool isBulkModify)
	{
		CheckTryFunction("MODIFY");
		CheckRecordIsOpenAndNotStale();
		if (!isBulkModify)
		{
			DataModificationListener.Instance?.OnBeforeModify(this);
		}
		WriteEventRaised(NavTriggerEventType.OnBeforeModifyEvent);
		if (metaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeModifyEvent) && !SuppressEventListeners)
		{
			EnsureGlobalVariablesInitialized();
			metaTable.TableTriggerEventHandler.OnBeforeModifyEvent(this, OldRecord, runApplicationTrigger);
		}
		if (runApplicationTrigger && metaTable.IsModifyTriggerDefined)
		{
			EnsureGlobalVariablesInitialized();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnBeforeModify();
			});
			OnModify();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnModify();
			});
		}
		NavGlobalTriggers.Modify(this, OldRecord, runApplicationTrigger & runGlobalTrigger);
		bool flag = recordImplementation.ModifyRecord(errorLevel);
		if (flag)
		{
			if (runApplicationTrigger)
			{
				EnsureGlobalVariablesInitialized();
				orderedTableExtensions.ForEach((NavRecordExtension ext) =>
				{
					ext.OnAfterModify();
				});
			}
			ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.Modify, this, null);
			if (Notify != null)
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Modify));
			}
			WriteEventRaised(NavTriggerEventType.OnAfterModifyEvent);
			if (metaTable.IsEventSubscribed(NavTriggerEventType.OnAfterModifyEvent) && !SuppressEventListeners)
			{
				EnsureGlobalVariablesInitialized();
				metaTable.TableTriggerEventHandler.OnAfterModifyEvent(this, OldRecord, runApplicationTrigger);
			}
		}
		if (!isBulkModify && !IsTemporary && !IsBasedOnSharedTable && !MetaTable.IsVirtualTable)
		{
			recordImplementation.MediaPurger.PurgeMediaLinks();
		}
		return flag;
	}

	public virtual bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI)
	{
		return Delete(errorLevel, runApplicationTrigger, isCalledFromUI, isBulkDelete: false);
	}

	public virtual bool Delete(DataError errorLevel, bool runApplicationTrigger, bool isCalledFromUI, bool isBulkDelete)
	{
		CheckTryFunction("DELETE");
		CheckRecordIsOpenAndNotStale();
		if (!isBulkDelete)
		{
			DataModificationListener.Instance?.OnBeforeDelete(this);
		}
		WriteEventRaised(NavTriggerEventType.OnBeforeDeleteEvent);
		if (metaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeDeleteEvent) && !SuppressEventListeners)
		{
			EnsureGlobalVariablesInitialized();
			metaTable.TableTriggerEventHandler.OnBeforeDeleteEvent(this, runApplicationTrigger);
		}
		if (runApplicationTrigger && metaTable.IsDeleteTriggerDefined)
		{
			EnsureGlobalVariablesInitialized();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnBeforeDelete();
			});
			OnDelete();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnDelete();
			});
		}
		bool flag = runApplicationTrigger & isCalledFromUI;
		NavGlobalTriggers.Delete(this, flag);
		if (flag)
		{
			recordImplementation.DeleteLinks();
		}
		bool flag2 = recordImplementation.DeleteRecord(errorLevel);
		if (flag2)
		{
			if (runApplicationTrigger)
			{
				EnsureGlobalVariablesInitialized();
				orderedTableExtensions.ForEach((NavRecordExtension ext) =>
				{
					ext.OnAfterDelete();
				});
			}
			ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.Delete, this, null);
			if (Notify != null)
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Delete));
			}
			WriteEventRaised(NavTriggerEventType.OnAfterDeleteEvent);
			if (metaTable.IsEventSubscribed(NavTriggerEventType.OnAfterDeleteEvent) && !SuppressEventListeners)
			{
				EnsureGlobalVariablesInitialized();
				metaTable.TableTriggerEventHandler.OnAfterDeleteEvent(this, runApplicationTrigger);
			}
		}
		if (!isBulkDelete && !IsTemporary && !IsBasedOnSharedTable && !MetaTable.IsVirtualTable)
		{
			recordImplementation.MediaPurger.PurgeMediaLinks();
		}
		return flag2;
	}

	public bool Rename(bool showDialog, DataError errorLevel)
	{
		NCLMetaKey keyByIndex = MetaTable.GetKeyByIndex(0);
		NavValue[] array = new NavValue[keyByIndex.KeyFieldCount];
		for (int i = 0; i < array.Length; i++)
		{
			NCLMetaField keyFieldByIndex = keyByIndex.GetKeyFieldByIndex(i);
			array[i] = GetFieldValue(keyFieldByIndex);
			SetFieldValue(keyFieldByIndex, OldRecord.GetFieldValue(keyFieldByIndex));
		}
		if (showDialog)
		{
			IClientCallback clientCallback = NavCurrentThread.Session.ClientCallback;
			if (!clientCallback.DialogConfirm(Lang.NavRenameMessage, defaultValue: false, NavWindowIdHelper.ConfirmDefaultId))
			{
				return false;
			}
		}
		Rename(errorLevel, runApplicationTrigger: true, runGlobalTrigger: true, array);
		return true;
	}

	public bool Rename(DataError errorLevel, bool runApplicationTrigger, bool runGlobalTrigger, params NavValue[] values)
	{
		CheckTryFunction("RENAME");
		if (values == null)
		{
			values = new NavValue[0];
		}
		NCLMetaKey keyByIndex = MetaTable.GetKeyByIndex(0);
		if (keyByIndex.KeyFieldCount != values.Length)
		{
			throw new NavNCLRenameArgumentException();
		}
		OldRecord.ALAssign(this);
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		using NavRecord navRecord = CloneRecord(this, reset: false, keepCompany: true);
		using NavRecord navRecord2 = CloneRecord(this, reset: false, keepCompany: true);
		IList<NCLMetaField> changedFields = SetKeyValues(navRecord2, keyByIndex, values);
		ParentCompany.TrackChanges.TrackChange(NavRecordOperationTypes.Rename, this, navRecord2);
		CopyRecord(navRecord2);
		DataModificationListener.Instance?.OnBeforeRename(this);
		WriteEventRaised(NavTriggerEventType.OnBeforeRenameEvent);
		if (metaTable.IsEventSubscribed(NavTriggerEventType.OnBeforeRenameEvent) && !SuppressEventListeners)
		{
			EnsureGlobalVariablesInitialized();
			metaTable.TableTriggerEventHandler.OnBeforeRenameEvent(this, OldRecord, runApplicationTrigger);
		}
		if (runApplicationTrigger && metaTable.IsRenameTriggerDefined)
		{
			EnsureGlobalVariablesInitialized();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnBeforeRename();
			});
			OnRename();
			orderedTableExtensions.ForEach((NavRecordExtension ext) =>
			{
				ext.OnRename();
			});
		}
		if (!OldRecord.IsTemporary)
		{
			NavGlobalTriggers.Rename(this, navRecord, runApplicationTrigger & runGlobalTrigger);
			recordImplementation.MoveLinks(navRecord);
		}
		navRecord2.CopyRecord(this);
		CopyRecord(navRecord);
		bool flag = recordImplementation.RenameRecord(errorLevel, navRecord2);
		if (flag)
		{
			if (runApplicationTrigger)
			{
				EnsureGlobalVariablesInitialized();
				orderedTableExtensions.ForEach((NavRecordExtension ext) =>
				{
					ext.OnAfterRename();
				});
			}
			if (TableID == 2000000006)
			{
				string value = ((NavText)navRecord.GetFieldValue(1)).Value;
				if (Session.CompanyName == value)
				{
					string value2 = ((NavText)navRecord2.GetFieldValue(1)).Value;
					Session.ChangeCurrentCompany(value2);
				}
			}
			if (Notify != null && !ALRecordId.Equals(other))
			{
				Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Rename));
			}
			if (!OldRecord.IsTemporary)
			{
				UpdateReferencesOnRename(changedFields, navRecord);
			}
			WriteEventRaised(NavTriggerEventType.OnAfterRenameEvent);
			if (metaTable.IsEventSubscribed(NavTriggerEventType.OnAfterRenameEvent) && !SuppressEventListeners)
			{
				EnsureGlobalVariablesInitialized();
				metaTable.TableTriggerEventHandler.OnAfterRenameEvent(this, OldRecord, runApplicationTrigger);
			}
		}
		return flag;
	}

	private static IList<NCLMetaField> SetKeyValues(NavRecord record, NCLMetaKey key, NavValue[] values)
	{
		List<NCLMetaField> list = new List<NCLMetaField>();
		for (int i = 0; i < key.KeyFieldCount; i++)
		{
			NCLMetaField keyFieldByIndex = key.GetKeyFieldByIndex(i);
			if (values[i].NclType != keyFieldByIndex.FieldNclType)
			{
				values[i] = NavValue.CreateNavValueFromObject(keyFieldByIndex, values[i]);
			}
			if (!record.GetFieldValue(keyFieldByIndex).Equals(values[i]))
			{
				list.Add(keyFieldByIndex);
			}
			record.SetFieldValue(keyFieldByIndex, values[i]);
		}
		return list;
	}

	private void UpdateReferencesOnRename(IList<NCLMetaField> changedFields, NavRecord originalValues)
	{
		if (originalValues.IsTemporary)
		{
			return;
		}
		HashSet<string> companiesCache = null;
		NCLMetaFieldRelation[] referencingRelations = metaTable.GetReferencingRelations();
		if (metaTable.TableId == 2000000006)
		{
			PushAccessControlRelationFirst(referencingRelations);
		}
		NCLMetaFieldRelation[] array = referencingRelations;
		foreach (NCLMetaFieldRelation nCLMetaFieldRelation in array)
		{
			NCLMetaTable referencingTable = nCLMetaFieldRelation.ReferencingTable;
			if (metaTable.IsAppTable != referencingTable.IsAppTable)
			{
				continue;
			}
			NCLMetaField referencingField = nCLMetaFieldRelation.ReferencingField;
			if (!referencingField.ValidateRelations || referencingField.FieldClass != FieldClass.Normal)
			{
				continue;
			}
			NCLMetaField nCLMetaField = ((nCLMetaFieldRelation.SourceFieldId == 0) ? metaTable.PrimaryKey.GetKeyFieldByIndex(0) : metaTable.GetFieldByNo(nCLMetaFieldRelation.SourceFieldId));
			if (changedFields.Contains(nCLMetaField))
			{
				IEnumerable<NCLMetaFilter> source = nCLMetaFieldRelation.SourceTableFilters.Where((NCLMetaFilter f) => f.FilterType != NCLMetaFilterType.Field);
				if (source.All((NCLMetaFilter f) => f.Expression.Evaluate(originalValues.GetFieldValue(f.SourceField))))
				{
					companiesCache = UpdateReferencingTableOnRename(nCLMetaFieldRelation, referencingTable, referencingField, nCLMetaField, originalValues, companiesCache);
				}
			}
		}
	}

	private static void PushAccessControlRelationFirst(NCLMetaFieldRelation[] relations)
	{
		for (int i = 0; i < relations.Length; i++)
		{
			if (relations[i].ReferencingTableId == 2000000053)
			{
				NCLMetaFieldRelation nCLMetaFieldRelation = relations[0];
				relations[0] = relations[i];
				relations[i] = nCLMetaFieldRelation;
				break;
			}
		}
	}

	private HashSet<string> UpdateReferencingTableOnRename(NCLMetaFieldRelation relation, NCLMetaTable referencingTable, NCLMetaField referencingField, NCLMetaField thisField, NavRecord originalValues, HashSet<string> companiesCache)
	{
		if (!metaTable.DataPerCompany && referencingTable.DataPerCompany)
		{
			if (companiesCache == null)
			{
				companiesCache = Session.GetAllCompanies(useSecurityFilter: false);
			}
			foreach (string item in companiesCache)
			{
				try
				{
					UpdateReferencingTableOnRename(relation, referencingTable, referencingField, thisField, originalValues, item);
				}
				catch (NavCSideDataException ex) when (ex.ErrorCode == 18023729)
				{
					throw new NavCSideDataException(18023729, string.Format(CultureInfo.CurrentCulture, Lang.RenameRecordUpdateReferencesUserDoesNotHaveAccessToCompany), ex);
				}
			}
		}
		else
		{
			UpdateReferencingTableOnRename(relation, referencingTable, referencingField, thisField, originalValues, CompanyName);
		}
		return companiesCache;
	}

	private void UpdateReferencingTableOnRename(NCLMetaFieldRelation relation, NCLMetaTable referencingTable, NCLMetaField referencingField, NCLMetaField thisField, NavRecord originalValues, string companyName)
	{
		NavValue navValue = GetFieldValue(thisField);
		if (navValue.NclType != referencingField.FieldNclType)
		{
			try
			{
				navValue = NavValue.CreateNavValueFromObject(referencingField, navValue);
			}
			catch (NavNCLNotSupportedTypeException innerException)
			{
				throw new NavNCLInvalidTypeException(string.Format(Session.Culture, Lang.PrimaryKeyRenameReferenceFieldDataTypeIncompatible, referencingField.Parent.TableCaptionSafe, referencingField.FieldCaption, thisField.FieldNavType, referencingField.FieldNavType), innerException);
			}
		}
		if (navValue.IsZeroOrEmpty)
		{
			throw new NavCSideException(string.Format(CultureInfo.CurrentCulture, Lang.InvalidRenameNewValue, ALFieldCaption(thisField.FieldNo), metaTable.TableCaptions.GetValueOrDefault(), navValue));
		}
		NavValue navValue2 = originalValues.GetFieldValue(thisField);
		if (navValue2.NclType != referencingField.FieldNclType)
		{
			navValue2 = NavValue.CreateNavValueFromObject(referencingField, navValue2);
		}
		using (new MaximizedPermissionScope(Session, new int[1] { referencingTable.TableId }))
		{
			using NavRecord navRecord = referencingTable.CreateObjectInstance(this, isTemporary: false, null, companyName, SecurityFiltering.Ignored);
			FilterFieldDictionary filtersForReferencingTable = GetFiltersForReferencingTable(relation, referencingField, originalValues, navValue2);
			if (filtersForReferencingTable != null)
			{
				Tuple<INavFieldMetadata, FilterExpression>[] items = filtersForReferencingTable.Items;
				foreach (Tuple<INavFieldMetadata, FilterExpression> tuple in items)
				{
					navRecord.RecordImplementation.SetFieldFilter((NCLMetaField)tuple.Item1, tuple.Item2);
				}
			}
			if (!referencingField.FieldIsPartOfPrimaryKey)
			{
				navRecord.ModifyAll(referencingField, navValue, runApplicationTrigger: false);
			}
			else if (CanUseBulkModifyAll(runApplicationTrigger: true, navRecord) && CanUseBulkRenameAll(runApplicationTrigger: true, navRecord))
			{
				navRecord.recordImplementation.ModifyOrRenameAllRecords(new FieldDictionary<NavValue>(new Tuple<INavFieldMetadata, NavValue>(referencingField, navValue)));
			}
			else
			{
				UpdateReferencesOnRename(navRecord, referencingTable, referencingField, navValue);
			}
		}
	}

	private static void UpdateReferencesOnRename(NavRecord referencingRecord, NCLMetaTable referencingTable, NCLMetaField referencingField, NavValue valueToSet)
	{
		NCLMetaKey primaryMetaKey = referencingTable.PrimaryKey;
		int num = primaryMetaKey.KeyFieldsList.IndexOf(referencingField);
		NCLMetaTable table = referencingRecord.MetaTable;
		NCLMetaQueryDefinition queryDefinition = NCLMetaQueryDefinition.Create(0, (NCLMetaQueryDefinition queryBeingCreated) => NCLMetaQueryDataItemCollection.Create(queryBeingCreated, table, DataLockState.LockExclusive, "ToRename", (NCLMetaQueryDataItem dataItem) =>
		{
			INavFieldMetadata[] fields = primaryMetaKey.KeyFieldsList.Select((NCLMetaField keyField) => NCLMetaQueryColumn.Create(dataItem, keyField, filterOnly: false)).ToArray();
			return new FieldList(fields);
		}, (NCLMetaQueryDataItem columnBeingCreated) => referencingRecord.RecordImplementation.TableState.FiltersAndMarks), null);
		IEnumerable<ReadOnlyRecordBuffer> queryResult = referencingRecord.RecordImplementation.DataAccess.DataProvider.GetQueryResult(queryDefinition, referencingRecord.RecordImplementation.TableState.CompanyNameToken, 0, 0, FiltersAndMarks.Empty, null, null, referencingRecord.SecurityFiltering, addExplicitJoinHints: true);
		referencingRecord.ALLockTable();
		foreach (ReadOnlyRecordBuffer item in queryResult)
		{
			NavValue[] array = item.ToArray();
			string aLCurrentCompany = referencingRecord.ALCurrentCompany;
			referencingRecord.Clear();
			referencingRecord.ALChangeCompany(aLCurrentCompany);
			referencingRecord.ALGet(DataError.ThrowError, array);
			array[num] = valueToSet;
			referencingRecord.Rename(DataError.ThrowError, runApplicationTrigger: false, runGlobalTrigger: false, array);
		}
	}

	private static FilterFieldDictionary GetFiltersForReferencingTable(NCLMetaFieldRelation relation, NCLMetaField referencingField, NavRecord originalValues, NavValue originalValue)
	{
		FilterFieldDictionary expressionDictionary = relation.ExpressionDictionary;
		FilterFieldDictionary filterFieldDictionary = new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(referencingField, FilterExpression.Equal(originalValue, referencingField.ExpressionContext)));
		IEnumerable<NCLMetaFilterField> source = relation.SourceTableFilters.Where((NCLMetaFilter f) => f.FilterType == NCLMetaFilterType.Field).Cast<NCLMetaFilterField>();
		IEnumerable<FilterFieldDictionary> second = source.Select((NCLMetaFilterField f) => new FilterFieldDictionary(new Tuple<INavFieldMetadata, FilterExpression>(f.ValueField, FilterExpression.Equal(NavValue.CreateNavValueFromObject(f.ValueField, originalValues.GetFieldValue(f.SourceField)), f.ValueField.ExpressionContext))));
		return DataProviderHelper.MergeFilterFieldDictionaries(new FilterFieldDictionary[2] { expressionDictionary, filterFieldDictionary }.Concat(second).ToArray());
	}

	public NavValue GetFieldValue(int fieldNo)
	{
		return GetFieldValue(MetaTable.GetFieldByNo(fieldNo));
	}

	public NavValue GetFieldValue(int extensionObjectId, int fieldNo)
	{
		return GetFieldValue(MetaTable.GetFieldByNo(extensionObjectId, fieldNo));
	}

	public NavValue GetFieldValueSafe(int fieldNo, NavType expectedType)
	{
		return GetFieldValue(ValidateExpectedType(fieldNo, expectedType));
	}

	public NavValue GetFieldValueSafe(int extensionObjectId, int fieldNo, NavType expectedType)
	{
		return GetFieldValue(ValidateExpectedType(extensionObjectId, fieldNo, expectedType));
	}

	public NavFieldRefShallow GetFieldRefSafe(int fieldNo, NavType expectedType)
	{
		ValidateExpectedType(fieldNo, expectedType);
		return new NavFieldRefShallow(this, fieldNo);
	}

	public NavFieldRefShallow GetFieldRefSafe(int extensionObjectId, int fieldNo, NavType expectedType)
	{
		ValidateExpectedType(extensionObjectId, fieldNo, expectedType);
		return new NavFieldRefShallow(this, extensionObjectId, fieldNo);
	}

	internal NCLMetaTable GetNCLMetaTable()
	{
		return metaTable;
	}

	internal string GetDecimalString(int fieldId)
	{
		string autoFormatString = Target.GetAutoFormatString(fieldId);
		if (string.IsNullOrEmpty(autoFormatString))
		{
			autoFormatString = Target.GetDecimalPlaces(fieldId);
			string.IsNullOrEmpty(autoFormatString);
			return autoFormatString;
		}
		return autoFormatString;
	}

	public void SetFieldValueSafe(int fieldNo, NavType expectedType, NavValue newValue)
	{
		SetFieldValue(ValidateExpectedType(fieldNo, expectedType), newValue);
	}

	public void SetFieldValueSafe(int extensionObjectId, int fieldNo, NavType expectedType, NavValue newValue)
	{
		SetFieldValue(ValidateExpectedType(extensionObjectId, fieldNo, expectedType), newValue);
	}

	public void SetFieldValue(int fieldNo, NavValue newValue)
	{
		SetFieldValue(MetaTable.GetFieldByNo(fieldNo), newValue);
	}

	public void ClearFieldValue(int fieldNo)
	{
		NCLMetaField fieldByNo = MetaTable.GetFieldByNo(fieldNo);
		if (fieldByNo.FieldNclType == NavNclType.NavBlob)
		{
			((NavBLOB)GetFieldValue(fieldNo)).Clear();
			return;
		}
		NavValue newValue = NavValue.CreateNavValueFromObject(fieldByNo, null);
		SetFieldValue(fieldNo, newValue);
	}

	public void ClearFieldValue(int extensionObjectId, int fieldNo)
	{
		NCLMetaField fieldByNo = MetaTable.GetFieldByNo(extensionObjectId, fieldNo);
		if (fieldByNo.FieldNclType == NavNclType.NavBlob)
		{
			((NavBLOB)GetFieldValue(fieldByNo)).Clear();
			return;
		}
		NavValue newValue = NavValue.CreateNavValueFromObject(fieldByNo, null);
		SetFieldValue(fieldByNo, newValue);
	}

	public void RemoveAllowedFilters()
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.RemoveAllowedFilters();
	}

	public bool Get(DataError errorLevel, NavRecordId recordId)
	{
		return Get(errorLevel, recordId, copyRecord: true);
	}

	public bool Get(DataError errorLevel, NavRecordId recordId, bool copyRecord, bool useFilters = false, bool calcAutoCalcFields = true)
	{
		CheckRecordIsOpenAndNotStale();
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		bool record = recordImplementation.GetRecord(errorLevel, recordId, copyRecord, useFilters, calcAutoCalcFields);
		if (record && Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return record;
	}

	internal void InitFromRecordId(NavRecordId recordId)
	{
		CheckRecordIsOpenAndNotStale();
		recordImplementation.InitFromRecordId(recordId);
	}

	public bool Exists(NavRecordId recordId)
	{
		return Get(DataError.TrapError, recordId, copyRecord: false, useFilters: false, calcAutoCalcFields: false);
	}

	public void SetPrimaryKeyFieldValues(NavRecordId recordId)
	{
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		CheckRecordIsOpenAndNotStale();
		recordImplementation.SetPrimaryKeyFromRecordId(recordId);
	}

	public void ClearAndSetMarks(byte[][] marked, bool allExceptThese)
	{
		recordImplementation.ResetMarks(marked, allExceptThese);
	}

	public IEnumerable<int> InitializeFieldsFromFilters(bool allFields)
	{
		return InitializeFieldsFromFilters(allFields, includeIdenticalFilters: false);
	}

	public IEnumerable<int> InitializeFieldsFromFilters(bool includeNonPrimaryKeyFields, bool includeIdenticalFilters)
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.InitRecordFromFilters(includeNonPrimaryKeyFields, includeIdenticalFilters);
	}

	public void ValidateFields(IEnumerable<int> fieldNumbers)
	{
		ValidateFields(fieldNumbers, null);
	}

	public void ValidateFields(IEnumerable<int> fieldNumbers, NavRecord callerRecord)
	{
		foreach (int fieldNumber in fieldNumbers)
		{
			NCLMetaField fieldByNo = MetaTable.GetFieldByNo(fieldNumber);
			bool commit = false;
			NavCurrentThread.Session.BeginTransaction();
			try
			{
				Validate(fieldByNo, GetFieldValue(fieldByNo), callerRecord);
				commit = true;
			}
			finally
			{
				NavCurrentThread.Session.EndTransaction(commit);
			}
		}
	}

	public bool NextRecord()
	{
		return ALNext(1) != 0;
	}

	public bool NextRecordWithoutPrefetch()
	{
		return NextWithoutPrefetch(1) != 0;
	}

	public bool PreviousRecord()
	{
		return ALNext(-1) != 0;
	}

	public bool PreviousRecordWithoutPrefetch()
	{
		return NextWithoutPrefetch(-1) != 0;
	}

	public NavTableView GetTableView()
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetTableView();
	}

	public int[] CurrentSortingFieldIdsByIndex(int keyIndex)
	{
		return metaTable.GetKeyByIndex(keyIndex).KeyFieldsList.Select((NCLMetaField metaFiled) => metaFiled.FieldNo).ToArray();
	}

	public void SetTableView(NavTableView view)
	{
		SetTableView(view, addSearchFilter: true, unconditionally: false);
	}

	internal void SetTableViewUnconditionally(NavTableView view)
	{
		SetTableView(view, addSearchFilter: true, unconditionally: true);
	}

	public void SetUnfilteredTableView(NavTableView view)
	{
		SetTableView(view, addSearchFilter: false, unconditionally: true);
	}

	internal void SetCurrentKey(NavTableView view)
	{
		if (view == null)
		{
			throw new ArgumentNullException("view");
		}
		if (view.CurrentSortingFieldIds != null && view.CurrentSortingFieldIds.Length != 0)
		{
			ALSetCurrentKey(view.CurrentSortingFieldIds);
		}
		else
		{
			ALCurrentKeyIndex = 1;
		}
	}

	private void TestFieldError(NCLMetaField metaField, string shouldBeValue)
	{
		string currentValue = "<N/A>";
		try
		{
			currentValue = GetFieldValue(metaField).ToString();
		}
		catch (NavBaseException)
		{
		}
		if (metaField.FieldNclType == NavNclType.NavOption && int.TryParse(shouldBeValue, out var result))
		{
			shouldBeValue = NavOption.Create(metaField.FieldOptionMetadata, result).ToString();
		}
		throw NavTestFieldException.CreateMustBeEqualTo(NavCurrentThread.Session.WindowsCulture, ALFieldCaption(metaField.FieldNo), metaField.Parent.TableCaptionSafe, shouldBeValue, currentValue, PrimaryKeyString);
	}

	public void TestFieldNotBlank(NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (GetFieldValue(metaField).IsZeroOrEmpty)
		{
			if (metaField.FieldNclType == NavNclType.NavOption && metaField.FieldOptionMetadata.Count > 0)
			{
				throw NavTestFieldException.CreateNonblankOption(NavCurrentThread.Session.WindowsCulture, ALFieldCaption(metaField.FieldNo), metaField.FieldOptionMetadata.GetCaptionFromIndex(0), metaField.Parent.TableCaptionSafe, PrimaryKeyString);
			}
			throw NavTestFieldException.CreateNonblank(NavCurrentThread.Session.WindowsCulture, ALFieldCaption(metaField.FieldNo), metaField.Parent.TableCaptionSafe, PrimaryKeyString);
		}
	}

	public void TestFieldEquals(NCLMetaField metaField, byte testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToByte() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString(NavCurrentThread.Session.WindowsCulture));
	}

	public void TestFieldEquals(NCLMetaField metaField, bool testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToBoolean() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		NavBoolean navBoolean = NavBoolean.Create(testValue);
		TestFieldError(metaField, navBoolean.ToString());
	}

	public void TestFieldEquals(NCLMetaField metaField, int testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToInt32() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString(NavCurrentThread.Session.WindowsCulture));
	}

	public void TestFieldEquals(NCLMetaField metaField, long testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToInt64() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString(NavCurrentThread.Session.WindowsCulture));
	}

	public void TestFieldEquals(NCLMetaField metaField, Decimal18 testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToDecimal() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString(NavCurrentThread.Session.WindowsCulture));
	}

	public void TestFieldEquals(NCLMetaField metaField, Guid testValue)
	{
		try
		{
			if (GetFieldValue(metaField).ToGuid() == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString());
	}

	public void TestFieldEquals(NCLMetaField metaField, string testValue)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (testValue == null)
		{
			throw new ArgumentNullException("testValue");
		}
		try
		{
			string text = GetFieldValue(metaField).ToString();
			if (metaField.FieldNclType == NavNclType.NavOemCode || metaField.FieldNclType == NavNclType.NavCode)
			{
				if (string.Compare(text, testValue, ignoreCase: true, NavCurrentThread.Session.WindowsCulture) == 0)
				{
					return;
				}
			}
			else if (text == testValue)
			{
				return;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue);
	}

	public void TestFieldEqualsNavValue(NCLMetaField metaField, NavValue testValue)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (testValue == null)
		{
			throw new ArgumentNullException("testValue");
		}
		try
		{
			if (testValue is NavIndirectValue && testValue.IsInitialized)
			{
				testValue = testValue.InnerValue;
			}
			switch (metaField.FieldNclType)
			{
			case NavNclType.NavBoolean:
				TestFieldEquals(metaField, testValue.ToBoolean());
				return;
			case NavNclType.NavBigInteger:
				TestFieldEquals(metaField, testValue.ToInt64());
				return;
			case NavNclType.NavByte:
				TestFieldEquals(metaField, testValue.ToByte());
				return;
			case NavNclType.NavDecimal:
				TestFieldEquals(metaField, testValue.ToDecimal());
				return;
			case NavNclType.NavGuid:
				TestFieldEquals(metaField, testValue.ToGuid());
				return;
			case NavNclType.NavInteger:
				TestFieldEquals(metaField, testValue.ToInt32());
				return;
			case NavNclType.NavOemCode:
			case NavNclType.NavCode:
				TestFieldEquals(metaField, testValue.ToString());
				return;
			default:
				if (GetFieldValue(metaField).Equals(testValue))
				{
					return;
				}
				break;
			}
		}
		catch (NavBaseException)
		{
		}
		TestFieldError(metaField, testValue.ToString());
	}

	public void ValidateUserInput(int fieldNo, NavValue newValue, NavRecord callerRecord)
	{
		Validate(MetaTable.GetFieldByNo(fieldNo), newValue, callerRecord, isUserInput: true);
	}

	internal void Validate(NCLMetaField metaField, NavValue newValue, NavRecord callerRecord)
	{
		Validate(metaField, newValue, callerRecord, isUserInput: false);
	}

	private void Validate(NCLMetaField metaField, NavValue newValue, NavRecord callerRecord, bool isUserInput)
	{
		CheckRecordIsOpenAndNotStale();
		using RecordState recordState = CreateRecordState();
		using RecordState recordState2 = OldRecord.CreateRecordState();
		NavValue navValue = null;
		FieldTriggerHandler<NavApplicationObjectBase> fieldTriggerHandler = GetFieldTriggerHandler(FieldTriggerType.OnValidate, metaField);
		List<FieldTriggerHandler<NavApplicationObjectBase>> fieldTriggerBeforeAfterHandlers = GetFieldTriggerBeforeAfterHandlers(FieldTriggerType.OnBeforeValidate, metaField);
		List<FieldTriggerHandler<NavApplicationObjectBase>> fieldTriggerBeforeAfterHandlers2 = GetFieldTriggerBeforeAfterHandlers(FieldTriggerType.OnAfterValidate, metaField);
		bool flag = fieldTriggerHandler != null || fieldTriggerBeforeAfterHandlers.Any() || fieldTriggerBeforeAfterHandlers2.Any() || metaField.IsEventSubscribed(NavTriggerEventType.OnBeforeValidateEvent) || metaField.IsEventSubscribed(NavTriggerEventType.OnAfterValidateEvent);
		if (flag)
		{
			recordState.BackupRecordHandle();
			recordState2.BackupRecordHandle();
			if (callerRecord == null || !recordImplementation.TableEquals(callerRecord.RecordImplementation))
			{
				OldRecord.Assign(this);
			}
		}
		if (newValue != null)
		{
			if (!flag)
			{
				navValue = GetFieldValue(metaField);
			}
			SetFieldValue(metaField, newValue);
		}
		bool flag2 = false;
		try
		{
			ValidateField(fieldTriggerHandler, metaField, isUserInput, fieldTriggerBeforeAfterHandlers, fieldTriggerBeforeAfterHandlers2);
			flag2 = true;
		}
		finally
		{
			if (!flag2)
			{
				if (navValue != null)
				{
					SetFieldValue(metaField, navValue);
				}
				else
				{
					recordState.RestoreRecord();
					recordState2.RestoreRecord();
				}
			}
		}
		if (flag && (callerRecord == null || !recordImplementation.TableEquals(callerRecord.RecordImplementation)))
		{
			recordState2.RestoreRecord();
		}
	}

	public bool CalcFields(DataError errorLevel, int[] fields)
	{
		if (fields == null)
		{
			throw new ArgumentNullException("fields");
		}
		if (fields.Length == 0)
		{
			return true;
		}
		NCLMetaField[] metaFields = GetMetaFields(errorLevel, fields);
		if (metaFields == null)
		{
			return false;
		}
		try
		{
			return recordImplementation.CalcFields(errorLevel, metaFields);
		}
		catch (NavErrorCodeException ex) when (errorLevel == DataError.TrapError && ex.ErrorNumber == 1005)
		{
			return false;
		}
	}

	public bool SetAutoCalcFields(DataError errorLevel, int[] calcFields, bool replace)
	{
		if (calcFields == null)
		{
			throw new ArgumentNullException("calcFields");
		}
		NCLMetaField[] metaFields = GetMetaFields(errorLevel, calcFields);
		if (metaFields == null)
		{
			return false;
		}
		try
		{
			return recordImplementation.SetAutoCalcFields(errorLevel, metaFields, replace);
		}
		catch (NavErrorCodeException) when (errorLevel == DataError.TrapError)
		{
			return false;
		}
	}

	internal NCLMetaField[] GetMetaFields(DataError errorLevel, int[] fields)
	{
		try
		{
			return Array.ConvertAll(fields, (int fieldNo) => metaTable.GetFieldByNo(fieldNo));
		}
		catch (NavNCLFieldNotFoundException) when (errorLevel == DataError.TrapError)
		{
			return null;
		}
	}

	public int[] GetAutoCalcFields()
	{
		NCLMetaField[] autoCalcFields = recordImplementation.GetAutoCalcFields();
		if (autoCalcFields == null)
		{
			return new int[0];
		}
		return Array.ConvertAll(autoCalcFields, (NCLMetaField field) => field.FieldNo);
	}

	public NavFilterGroup[] GetCurrentFilters()
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetCurrentFilters();
	}

	protected virtual FieldTriggerHandler<NavApplicationObjectBase> GetFieldTriggerHandler(FieldTriggerType triggerType, NCLMetaField field)
	{
		if (GetType() == typeof(NavRecord))
		{
			return null;
		}
		FieldTriggerHandler<NavApplicationObjectBase> fieldTriggerHandler = null;
		switch (triggerType)
		{
		case FieldTriggerType.OnLookup:
			fieldTriggerHandler = field.LookupHandler;
			break;
		case FieldTriggerType.OnValidate:
			fieldTriggerHandler = field.ValidateHandler;
			break;
		}
		if (fieldTriggerHandler != null)
		{
			EnsureGlobalVariablesInitialized();
		}
		return fieldTriggerHandler;
	}

	private List<FieldTriggerHandler<NavApplicationObjectBase>> GetFieldTriggerBeforeAfterHandlers(FieldTriggerType triggerType, NCLMetaField field)
	{
		if (GetType() == typeof(NavRecord))
		{
			return new List<FieldTriggerHandler<NavApplicationObjectBase>>();
		}
		List<FieldTriggerHandler<NavApplicationObjectBase>> list = triggerType switch
		{
			FieldTriggerType.OnBeforeValidate => field.OnBeforeValidateHandlers, 
			FieldTriggerType.OnAfterValidate => field.OnAfterValidateHandlers, 
			_ => throw new NavInvalidCallException(), 
		};
		if (list == null)
		{
			list = new List<FieldTriggerHandler<NavApplicationObjectBase>>();
		}
		if (list.Any())
		{
			EnsureGlobalVariablesInitialized();
		}
		return list;
	}

	public NavRecord Assign(INavRecordHandle fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		recordImplementation.CopyRecord(fromRecord.Target, CopyRecordOptions.None);
		if (Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return this;
	}

	public NavRecord AssignForVariant(INavRecordHandle fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		CheckRecordIsOpenAndNotStale();
		NavRecordId other = ((Notify == null) ? null : ALRecordId);
		recordImplementation.CopyRecord(fromRecord.Target, CopyRecordOptions.CopyFiltersSortingMarked);
		if (Notify != null && !ALRecordId.Equals(other))
		{
			Notify(this, new NavRecordEventArgs(NavRecordOperationTypes.Navigation));
		}
		return this;
	}

	public void SetRange(NCLMetaField metaField, NavValue fromValue, NavValue toValue)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		CheckRecordIsOpenAndNotStale();
		if (fromValue != null && fromValue.NclType != metaField.FieldNclType)
		{
			fromValue = ((metaField.FieldNclType != NavNclType.NavCode || fromValue.NclType != NavNclType.NavText) ? NavValue.CreateNavValueFromObject(metaField, fromValue) : NavCode.CreateFromObject(2048, fromValue));
		}
		if (toValue != null && toValue.NclType != metaField.FieldNclType)
		{
			toValue = ((metaField.FieldNclType != NavNclType.NavText || toValue.NclType != NavNclType.NavCode) ? NavValue.CreateNavValueFromObject(metaField, toValue) : NavText.CreateFromObject(int.MaxValue, toValue));
		}
		recordImplementation.SetFieldRange(metaField, fromValue, toValue);
	}

	public void SetFilter(NCLMetaField metaField, string expression, params NavValue[] values)
	{
		CheckRecordIsOpenAndNotStale();
		try
		{
			recordImplementation.SetFieldFilter(metaField, expression, values);
		}
		catch (NavCSideException inner)
		{
			throw NavFilterException.CreateFromInner(inner);
		}
	}

	public bool TrySetFilter(NCLMetaField metaField, string expression, params NavValue[] values)
	{
		try
		{
			CheckRecordIsOpenAndNotStale();
			return recordImplementation.TrySetFieldFilter(metaField, expression, values);
		}
		catch (NavBaseException exception)
		{
			DiagnosticsResolver.GetMostSpecificInstance().SendExceptionTag("00001BZ", Category.Runtime, exception);
			return false;
		}
	}

	public NavValue GetRangeMin(NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetFieldRangeMin(metaField);
	}

	public NavValue GetRangeMax(NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.GetFieldRangeMax(metaField);
	}

	public NavRecord Clone(ITreeObject parentOfClone)
	{
		return CloneRecord(parentOfClone);
	}

	public NavRecord CloneValues(ITreeObject parentOfClone)
	{
		return CloneRecord(parentOfClone, reset: true);
	}

	public NavValue GetFieldValue(NCLMetaField metaField)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		CheckRecordIsOpenAndNotStale();
		if (!metaField.FieldActive || metaField.FieldObsoleted)
		{
			return NavValue.GetDefaultNavValue(metaField);
		}
		NavValue fieldValue = recordImplementation.GetFieldValue(metaField);
		return UpdateMediaFieldInformation(metaField, fieldValue);
	}

	private NavValue UpdateMediaFieldInformation(NCLMetaField metaField, NavValue fieldValue)
	{
		int companyNameToken = ParentCompany?.CompanyNameToken ?? (-1);
		if (metaField.FieldNclType == NavNclType.NavMedia)
		{
			if (!(fieldValue is NavMedia other))
			{
				throw new NavNCLConversionException(fieldValue.GetType(), typeof(NavMedia));
			}
			NavMedia navMedia = new NavMedia(other);
			if (currentCompanyName != ParentCompany?.CompanyName)
			{
				companyNameToken = recordImplementation?.TableState?.CompanyNameToken ?? (-1);
			}
			navMedia.SetOwnerRecordInformation(this, companyNameToken, TableID, metaField.FieldNo);
			return navMedia;
		}
		if (metaField.FieldNclType == NavNclType.NavMediaSet)
		{
			if (!(fieldValue is NavMediaSet other2))
			{
				throw new NavNCLConversionException(fieldValue.GetType(), typeof(NavMediaSet));
			}
			NavMediaSet navMediaSet = new NavMediaSet(other2);
			if (currentCompanyName != ParentCompany?.CompanyName)
			{
				companyNameToken = recordImplementation?.TableState?.CompanyNameToken ?? (-1);
			}
			navMediaSet.SetOwnerRecordInformation(this, companyNameToken, TableID, metaField.FieldNo);
			return navMediaSet;
		}
		return fieldValue;
	}

	public void SetFieldValue(NCLMetaField metaField, NavValue newValue)
	{
		if (metaField == null)
		{
			throw new ArgumentNullException("metaField");
		}
		if (newValue == null)
		{
			throw new ArgumentNullException("newValue");
		}
		CheckRecordIsOpenAndNotStale();
		if (!metaField.FieldActive || metaField.FieldObsoleted)
		{
			return;
		}
		if (metaField.FieldIndex == 0)
		{
			long recordTimestamp = newValue.ToInt64();
			recordImplementation.SetRecordTimestamp(recordTimestamp);
			return;
		}
		if (!newValue.CompareTypeAndMetadata(metaField))
		{
			newValue = NavValue.CreateNavValueFromObject(metaField, newValue);
		}
		if (metaField.SqlDataType == SqlDataType.Variant)
		{
			switch (metaField.FieldNclType)
			{
			case NavNclType.NavCode:
				newValue = ((NavCode)newValue).UpdateWithSqlDataType(metaField.SqlDataType);
				break;
			case NavNclType.NavOemCode:
				newValue = ((NavOemCode)newValue).UpdateWithSqlDataType(metaField.SqlDataType);
				break;
			}
		}
		if (newValue.NclType == NavNclType.NavBlob)
		{
			NavBLOB newBlob = (NavBLOB)newValue;
			SetBlobValue(metaField, newBlob);
		}
		else
		{
			recordImplementation.StoreFieldValue(metaField, newValue);
		}
	}

	public void SetRecordDataItemLink(NavRecord fromRecord, RecordDataItemLink recordDataItemLink)
	{
		CheckRecordIsOpenAndNotStale();
		if (fromRecord != null)
		{
			recordImplementation.SetDataItemLink(fromRecord, recordDataItemLink);
			return;
		}
		throw new NavNCLArgumentException(Lang._00095_50082);
	}

	public void Lookup(int fieldNo)
	{
		CheckRecordIsOpenAndNotStale();
		NCLMetaField fieldByNo = metaTable.GetFieldByNo(fieldNo);
		FieldTriggerHandler<NavApplicationObjectBase> fieldTriggerHandler = GetFieldTriggerHandler(FieldTriggerType.OnLookup, fieldByNo);
		InvokeFieldTriggerHandler(fieldTriggerHandler);
	}

	private void SetAllowedTableViewFilters(NavFilterGroup[] filterGroups, Predicate<FilterGroupType> filterCondition)
	{
		RemoveAllowedFilters();
		SetTableViewFilters(filterGroups, filterCondition);
	}

	public void SetAllowedTableViewFilters(NavFilterGroup[] filterGroups)
	{
		SetAllowedTableViewFilters(filterGroups, IsFilterGroupAllowed);
	}

	public int EvaluateRelation(int fieldId)
	{
		CheckRecordIsOpenAndNotStale();
		return recordImplementation.EvaluateRelation(metaTable.GetFieldByNo(fieldId));
	}

	public NavText GetDynamicCaption(int fieldNumber)
	{
		string value = EvaluateCaptionClass(fieldNumber);
		if (string.IsNullOrEmpty(value) || NavCurrentThread.Session == null)
		{
			return NavText.Empty;
		}
		return NavCurrentThread.Session.SystemCodeunitFactory.UIHelperTriggers.InvokeCaptionClassTranslate(NavCurrentThread.Session.LocalLanguage, new NavText(value));
	}

	public virtual string EvaluateCaptionClass(int fieldNumber)
	{
		return string.Empty;
	}

	public static bool IsSystemTable(int objId)
	{
		return objId >= 2000000001;
	}

	public static bool IsTableVirtual(int objId)
	{
		if (objId <= 2000000006)
		{
			return false;
		}
		if (objId > 2000000006 && objId <= 2000000049)
		{
			return true;
		}
		if (objId >= 2000000055 && objId <= 2000000058)
		{
			return true;
		}
		switch (objId)
		{
		case 2000000063:
			return true;
		case 2000000101:
		case 2000000102:
		case 2000000103:
			return true;
		default:
			switch (objId)
			{
			case 2000000135:
				return true;
			case 2000000136:
				return true;
			case 2000000137:
				return true;
			case 2000000138:
				return true;
			case 2000000139:
				return true;
			case 2000000140:
				return true;
			case 2000000164:
				return true;
			case 2000000141:
				return true;
			case 2000000167:
				return true;
			case 2000000178:
				return true;
			case 2000000171:
				return true;
			case 2000000172:
				return true;
			case 2000000154:
				return true;
			case 2000000193:
				return true;
			case 2000000097:
				return true;
			case 2000000192:
				return true;
			case 2000000142:
				return true;
			case 2000000143:
				return true;
			case 2000000157:
				return true;
			case 2000000202:
				return true;
			case 2000000203:
				return true;
			case 2000000204:
				return true;
			case 2000000110:
				if (ServerUserSettings.Instance.TrackSessionsInMemory.Value)
				{
					return true;
				}
				break;
			}
			return false;
		}
	}

	public bool IsWithinTheFilters()
	{
		using NavRecord navRecord = CloneRecord(Parent, reset: false, keepCompany: true);
		navRecord.SetAutoCalcFields(DataError.ThrowError, new int[0], replace: true);
		return navRecord.ALFind(DataError.TrapError, "=");
	}

	INavRecordHandle INavRecordHandle.Clone(ITreeObject parentOfClone)
	{
		return Clone(parentOfClone);
	}

	public void ALAssign(INavRecordHandle other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Assign(other.Target);
	}

	public NavRecordHandle ALByValue(ITreeObject parentOfResult, bool cloneAsTemporary)
	{
		NavRecordHandle navRecordHandle = new NavRecordHandle(parentOfResult, ObjectId.ObjectNumber, cloneAsTemporary, RecordImplementation.SecurityFiltering);
		navRecordHandle.Target.Assign(this);
		return navRecordHandle;
	}

	public NavRecordHandle CloneForVariant(ITreeObject parentOfResult, bool cloneAsTemporary)
	{
		NavRecordHandle navRecordHandle = new NavRecordHandle(parentOfResult, ObjectId.ObjectNumber, cloneAsTemporary, RecordImplementation.SecurityFiltering);
		navRecordHandle.Target.AssignForVariant(this);
		return navRecordHandle;
	}

	public NavRecord CloneForVariant(ITreeObject parentOfClone)
	{
		return CloneRecord(parentOfClone);
	}

	public void RegisterTableExtension(NavRecordExtension extension)
	{
		orderedTableExtensions.Add(extension);
	}

	public object Invoke(int extensionId, int methodId, object[] arguments)
	{
		NavRecordExtension navRecordExtension = orderedTableExtensions.SingleOrDefault((NavRecordExtension x) => x.ObjectId.ObjectNumber == extensionId);
		if (navRecordExtension == null)
		{
			throw new NavNCLCompilationException(CultureInfo.InvariantCulture, Lang.TableExtensionNotFound, extensionId, ObjectId.ObjectNumber);
		}
		return navRecordExtension.Invoke(methodId, arguments);
	}

	public void CallInitializeGlobalVariablesExtensionMethod()
	{
		orderedTableExtensions.ForEach((NavRecordExtension ext) =>
		{
			ext.InitializeGlobalVariables();
		});
	}

	protected override void CallOnClearExtensionMethod()
	{
		orderedTableExtensions.ForEach((NavRecordExtension ext) =>
		{
			ext.OnClear();
		});
	}

	public bool CallGetAutoFormatStringExtensionMethod(int fieldNumber, ref string returnValue)
	{
		foreach (NavRecordExtension orderedTableExtension in orderedTableExtensions)
		{
			string autoFormatString = orderedTableExtension.GetAutoFormatString(fieldNumber, out var handled);
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
		foreach (NavRecordExtension orderedTableExtension in orderedTableExtensions)
		{
			string decimalPlaces = orderedTableExtension.GetDecimalPlaces(fieldNumber, out var handled);
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
		foreach (NavRecordExtension orderedTableExtension in orderedTableExtensions)
		{
			string text = orderedTableExtension.EvaluateCaptionClass(fieldNumber, out var handled);
			if (handled)
			{
				returnValue = text;
				return true;
			}
		}
		return false;
	}

	public string EvaluateCaptionClass(int extensionObjectId, int fieldNumber)
	{
		foreach (NavRecordExtension orderedTableExtension in orderedTableExtensions)
		{
			string result = orderedTableExtension.EvaluateCaptionClass(extensionObjectId, fieldNumber, out var handled);
			if (handled)
			{
				return result;
			}
		}
		return string.Empty;
	}

	public override NavComplexValue CreateHandle(ITreeObject parent)
	{
		return new NavRecordHandle(parent, ObjectId.ObjectNumber, temporary: false, RecordImplementation.SecurityFiltering)
		{
			Target = this
		};
	}

	internal override NavApplicationObjectBase ResolveTriggerHandler(string triggerMethodName, object[] args)
	{
		if (!triggerMap.ContainsKey(triggerMethodName))
		{
			MethodInfo methodInfo = null;
			foreach (NavRecordExtension orderedTableExtension in orderedTableExtensions)
			{
				methodInfo = NavApplicationMethod.FindMethod(orderedTableExtension, triggerMethodName, args, out var _);
				if (methodInfo != null)
				{
					triggerMap.Add(triggerMethodName, orderedTableExtension);
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

	public RecordState CreateRecordState()
	{
		return recordImplementation.CreateRecordState();
	}

	private void SetBlobValue(NCLMetaField metaField, NavBLOB newBlob)
	{
		NavBLOB navBLOB = (NavBLOB)recordImplementation.GetFieldValue(metaField);
		if (NavBLOB.CompareBlobs(navBLOB, newBlob) == 1)
		{
			navBLOB.IsDirty = true;
			return;
		}
		navBLOB.AssignFromStream(new NavBLOB.InternalStream(newBlob));
		navBLOB.IsDirty = true;
	}

	private bool IsGlobalTriggerImplemented(NavSystemCodeunitGlobalTriggers.Triggers triggers)
	{
		if (!isTemporary)
		{
			return MetaTable.IsGlobalTriggerImplemented(triggers);
		}
		return false;
	}

	private void CheckTryFunction(string writeMethodName)
	{
		if (!IsTemporary && MetaTable.TableType == TableType.Normal)
		{
			Session.CheckTryFunction(writeMethodName);
		}
	}

	private void SetTableView(NavTableView view, bool addSearchFilter, bool unconditionally)
	{
		if (view == null)
		{
			throw new ArgumentNullException("view");
		}
		SetCurrentKey(view);
		ALAscending = view.Ascending;
		NavFilterGroup[] array = view.CurrentFilters;
		if (addSearchFilter && view.SearchFilter != null)
		{
			array = array.Concat(new NavFilterGroup[1] { view.SearchFilter }).ToArray();
		}
		if (unconditionally)
		{
			SetAllowedTableViewFilters(array, null);
		}
		else
		{
			SetAllowedTableViewFilters(array);
		}
	}

	internal void SetSearchTimeout(NavRecordState state)
	{
		if (state != null && state.TableView != null && state.TableView.SearchFilter != null && state.TableView.SearchFilter.Filters.Count() > 0)
		{
			SearchTimeout = ServerUserSettings.Instance.SearchTimeout.TotalSeconds;
		}
	}

	internal void ResetSearchTimeout()
	{
		if (SearchTimeout.HasValue)
		{
			SearchTimeout = null;
		}
	}

	private void WriteEventRaised(NavTriggerEventType triggerEventType, string elementName = null)
	{
		if (NavCurrentThread.Session.IsEventSessionRecorderEnabled)
		{
			NavServerEventWriter.WriteEventRaised(NavCurrentThread.Session.EventTelemetryRegistrar, objectId: ObjectId.ObjectNumber, objectType: (int)ObjectId.ObjectType, eventName: triggerEventType.ToString(), elementName: elementName ?? string.Empty, eventType: 2);
		}
	}

	public int[] FieldsWithoutIntellectualPropertyFields(int[] fields)
	{
		if (TableID > 2000000001 && ServerUserSettings.Instance.ProtectNavAppSourceFiles.Value)
		{
			return fields.Where((int field) => !IsFieldIntellectualPropertyField(TableID, field)).ToArray();
		}
		return fields;
	}

	private bool IsFieldIntellectualPropertyField(int tableId, int fieldNo)
	{
		if (SystemTables.TablesWithIntellectualProperty.TryGetValue(tableId, out var value) && value.Contains(fieldNo))
		{
			if (tableId == 2000000150 && fieldNo == 7)
			{
				using NavRecord navRecord = new NavRecord(this, 2000000160, SecurityFiltering);
				NavValue[] primaryKeyValues = new NavValue[1] { GetFieldValueSafe(1, NavType.GUID) };
				NavRecordId recordId = new NavRecordId(navRecord.TableID, primaryKeyValues);
				if (navRecord.Get(DataError.TrapError, recordId) && navRecord.GetFieldValueSafe(29, NavType.Boolean).ToBoolean())
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}
}
