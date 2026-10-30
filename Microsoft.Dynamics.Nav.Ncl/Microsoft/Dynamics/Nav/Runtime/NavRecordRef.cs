using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.Common.Language;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerTypeProxy(typeof(NavRecordRefDebugView))]
public sealed class NavRecordRef : NavComplexValue, ITreeObjectReference, ITreeObject
{
	public class Factory : ElementFactory<NavRecordRef>
	{
		private readonly SecurityFiltering securityFiltering;

		public Factory(ITreeObject parentForArray, SecurityFiltering securityFiltering)
			: base(parentForArray)
		{
			this.securityFiltering = securityFiltering;
		}

		public override NavRecordRef CreateElement()
		{
			return new NavRecordRef(ParentForElements, null, securityFiltering);
		}

		public override void AssignElement(NavRecordRef destinationElement, NavRecordRef sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavRecordRef> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, securityFiltering);
		}
	}

	private NCLMetaField[] fieldsWithoutObsoleteStateRemoved;

	private int countOfFieldsWithObsoleteStateRemoved = -1;

	private int[] keyIndexMapForKeysWithoutObsoleteStateRemoved;

	private int countOfKeysWithObsoleteStateRemoved = -1;

	private bool isTempAllocation;

	private SecurityFiltering securityFiltering;

	private SecurityFiltering orginalFiltering;

	public bool ALAscending
	{
		get
		{
			return SafeRecord.ALAscending;
		}
		set
		{
			SafeRecord.ALAscending = value;
		}
	}

	public string ALCaption => SafeRecord.ALTableCaption;

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

	public int ALCount => SafeRecord.ALCount;

	public int ALCountApprox => SafeRecord.ALCountApprox;

	public string ALCurrentKey => SafeRecord.ALCurrentKey;

	public int ALCurrentKeyIndex
	{
		get
		{
			return SafeRecord.ALCurrentKeyIndex;
		}
		set
		{
			SafeRecord.ALCurrentKeyIndex = value;
		}
	}

	public int ALFieldCount
	{
		get
		{
			if (ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState.Removed))
			{
				EnsureCacheOfFieldsWithoutObsoleteStateRemoved();
				return SafeRecord.ALFieldCount - countOfFieldsWithObsoleteStateRemoved;
			}
			return SafeRecord.ALFieldCount;
		}
	}

	public int ALFilterGroup
	{
		get
		{
			return SafeRecord.ALFilterGroup;
		}
		set
		{
			SafeRecord.ALFilterGroup = value;
		}
	}

	public string ALGetFilters => SafeRecord.ALGetFilters;

	internal string GetUrlFilters => SafeRecord.GetUrlFilters;

	public bool ALHasFilter => SafeRecord.ALHasFilter;

	public bool ALHasLinks => SafeRecord.ALHasLinks;

	public bool ALIsEmpty => SafeRecord.ALIsEmpty;

	public int ALKeyCount
	{
		get
		{
			if (ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState.Removed))
			{
				EnsureCacheOfKeysWithoutObsoleteStateRemoved();
				return SafeRecord.ALKeyCount - countOfKeysWithObsoleteStateRemoved;
			}
			return SafeRecord.ALKeyCount;
		}
	}

	public string ALName => SafeRecord.ALTableName;

	public int ALNumber
	{
		get
		{
			if (!IsOpen)
			{
				return 0;
			}
			if (MetaTable.IsAppArchiveTable)
			{
				return 0;
			}
			return SafeRecord.TableID;
		}
	}

	public bool ALReadConsistency => SafeRecord.ALReadConsistency;

	public bool ALReadPermission => SafeRecord.ALReadPermission;

	public NavRecordId ALRecordId => SafeRecord.ALRecordId;

	public bool ALRecordLevelLocking => SafeRecord.ALRecordLevelLocking;

	public bool ALWritePermission => SafeRecord.ALWritePermission;

	public bool ALIsTemporary => SafeRecord.IsTemporary;

	public string ALCurrentCompany => SafeRecord.CompanyName;

	internal bool HasTarget => Tree.GetReferenceTarget() != null;

	internal SharedRecordRef Target
	{
		get
		{
			SharedRecordRef sharedRecordRef = (SharedRecordRef)Tree.GetReferenceTarget();
			if (sharedRecordRef == null)
			{
				sharedRecordRef = new SharedRecordRef();
				Tree.SetReferenceTarget(sharedRecordRef);
			}
			return sharedRecordRef;
		}
		set
		{
			Tree.SetReferenceTarget(value);
		}
	}

	public NavRecord Record
	{
		get
		{
			if (!HasTarget)
			{
				return null;
			}
			return Target.Record;
		}
		set
		{
			Target.Record = value;
		}
	}

	public NavRecord SafeRecord
	{
		get
		{
			if (!IsOpen)
			{
				throw new NavNCLRecordNotOpenedException();
			}
			return Record;
		}
	}

	public bool IsOpen
	{
		get
		{
			NavRecord record = Record;
			if (record != null)
			{
				return !record.IsDisposed;
			}
			return false;
		}
	}

	public NCLMetaTable MetaTable => SafeRecord.MetaTable;

	public SecurityFiltering SecurityFiltering
	{
		get
		{
			return securityFiltering;
		}
		set
		{
			securityFiltering = value;
			if (HasTarget)
			{
				Target.Record.SecurityFiltering = value;
			}
		}
	}

	public override NavNclType NclType => NavNclType.NavRecordRef;

	public override int GetBytesSize => 0;

	public void ALOpen(int tableId)
	{
		ALOpen(tableId, isTemporary: false);
	}

	public void ALOpen(CompilationTarget compilationTarget, int tableId)
	{
		CheckIsOpenAllowed(compilationTarget, tableId);
		ALOpen(tableId);
	}

	public void ALOpen(int tableId, bool isTemporary)
	{
		ALOpen(tableId, isTemporary, string.Empty);
	}

	public void ALOpen(CompilationTarget compilationTarget, int tableId, bool isTemporary)
	{
		CheckIsOpenAllowed(compilationTarget, tableId);
		ALOpen(tableId, isTemporary);
	}

	public void ALOpen(int tableId, bool isTemporary, string companyName)
	{
		if (IsOpen)
		{
			throw new NavNCLRecordAlreadyOpenedException();
		}
		SetRecord(tableId, isTemporary, companyName);
	}

	public void ALOpen(CompilationTarget compilationTarget, int tableId, bool isTemporary, string companyName)
	{
		CheckIsOpenAllowed(compilationTarget, tableId);
		ALOpen(tableId, isTemporary, companyName);
	}

	public void ALClose()
	{
		ClearObsoleteStateInformation();
		Target = null;
	}

	public void ALInit()
	{
		SafeRecord.ALInit();
	}

	public int ALAddLink(string url)
	{
		return SafeRecord.ALAddLink(url);
	}

	public int ALAddLink(string url, string description)
	{
		return SafeRecord.ALAddLink(url, description);
	}

	public void ALCopyLinks(NavRecord fromRecord)
	{
		SafeRecord.ALCopyLinks(fromRecord);
	}

	public void ALCopyLinks(NavRecordRef fromRecord)
	{
		SafeRecord.ALCopyLinks(fromRecord);
	}

	public void ALCopyLinks(NavVariant fromRecordOrRecordRef)
	{
		if (fromRecordOrRecordRef == null)
		{
			throw new ArgumentNullException("fromRecordOrRecordRef");
		}
		ALCopyLinks(ALCompiler.NavIndirectValueToINavRecordHandle(fromRecordOrRecordRef).Target);
	}

	public void ALDelete()
	{
		SafeRecord.ALDelete();
	}

	public bool ALDelete(DataError errorLevel)
	{
		return SafeRecord.ALDelete(errorLevel);
	}

	public void ALDelete(bool runTrigger)
	{
		SafeRecord.ALDelete(runTrigger);
	}

	public bool ALDelete(DataError errorLevel, bool runTrigger)
	{
		return SafeRecord.ALDelete(errorLevel, runTrigger);
	}

	public void ALDeleteAll(bool runTrigger)
	{
		SafeRecord.ALDeleteAll(runTrigger);
	}

	public void ALDeleteAll()
	{
		SafeRecord.ALDeleteAll();
	}

	public void ALDeleteLink(int linkId)
	{
		SafeRecord.ALDeleteLink(linkId);
	}

	public void ALDeleteLinks()
	{
		SafeRecord.ALDeleteLinks();
	}

	public NavRecordRef ALDuplicate(ITreeObject parentOfClone)
	{
		if (IsOpen)
		{
			return new NavRecordRef(parentOfClone, Record, isTempAllocation: true, Record.SecurityFiltering);
		}
		throw new NavNCLRecordNotOpenedException();
	}

	public void ALAssign(NavRecordRef other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (Target?.Record?.ObjectId != other.Target?.Record?.ObjectId)
		{
			ClearObsoleteStateInformation();
		}
		Target = other.Target;
		if (other.isTempAllocation)
		{
			other.Dispose();
		}
	}

	public NavRecordRef ALByValue(ITreeObject parentOfResult)
	{
		NavRecordRef navRecordRef = new NavRecordRef(parentOfResult, SecurityFiltering);
		navRecordRef.ALAssign(this);
		return navRecordRef;
	}

	public NavFieldRef ALField(ITreeObject parentOfResult, int fieldNo)
	{
		if (!IsOpen)
		{
			throw new NavNCLRecordNotOpenedException();
		}
		return new NavFieldRef(parentOfResult, this, fieldNo, isTempAllocation: true);
	}

	public void ALFind(string searchMethod)
	{
		SafeRecord.ALFind(searchMethod);
	}

	public bool ALFind(DataError errorLevel)
	{
		return SafeRecord.ALFind(errorLevel);
	}

	public bool ALFind(DataError errorLevel, string searchMethod)
	{
		return SafeRecord.ALFind(errorLevel, searchMethod);
	}

	public void ALFind()
	{
		SafeRecord.ALFind();
	}

	public void ALFindFirst()
	{
		SafeRecord.ALFindFirst();
	}

	public bool ALFindFirst(DataError errorLevel)
	{
		return SafeRecord.ALFindFirst(errorLevel);
	}

	public bool ALFindLast(DataError errorLevel)
	{
		return SafeRecord.ALFindLast(errorLevel);
	}

	public void ALFindLast()
	{
		SafeRecord.ALFindLast();
	}

	public void ALFindSet()
	{
		SafeRecord.ALFindSet();
	}

	public void ALFindSet(bool forUpdate, bool updateKey)
	{
		SafeRecord.ALFindSet(forUpdate, updateKey);
	}

	public bool ALFindSet(DataError errorLevel)
	{
		return SafeRecord.ALFindSet(errorLevel);
	}

	public bool ALFindSet(DataError errorLevel, bool forUpdate, bool updateKey)
	{
		return SafeRecord.ALFindSet(errorLevel, forUpdate, updateKey);
	}

	public void ALFindSet(bool forUpdate)
	{
		SafeRecord.ALFindSet(forUpdate);
	}

	public bool ALFindSet(DataError errorLevel, bool forUpdate)
	{
		return SafeRecord.ALFindSet(errorLevel, forUpdate);
	}

	public bool ALFieldExists(int fieldNo)
	{
		bool flag = SafeRecord.ALFieldExists(fieldNo);
		if (flag && ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState.Removed))
		{
			return MetaTable.GetFieldByNo(fieldNo).ObsoleteState != ObsoleteState.Removed;
		}
		return flag;
	}

	public NavFieldRef ALFieldIndex(ITreeObject parentOfResult, int index)
	{
		if (!IsOpen)
		{
			throw new NavNCLRecordNotOpenedException();
		}
		if (ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState.Removed))
		{
			EnsureCacheOfFieldsWithoutObsoleteStateRemoved();
			if (countOfFieldsWithObsoleteStateRemoved > 0)
			{
				if (index < 0 || index >= fieldsWithoutObsoleteStateRemoved.Length)
				{
					throw new NavNCLIndexOutOfBoundsException();
				}
				return new NavFieldRef(parentOfResult, this, fieldsWithoutObsoleteStateRemoved[index], isTempAllocation: true);
			}
		}
		return new NavFieldRef(parentOfResult, this, Record.MetaTable.GetFieldByIndex(index), isTempAllocation: true);
	}

	public bool ALGet(NavRecordId recordId)
	{
		return ALGet(DataError.ThrowError, recordId);
	}

	public bool ALGet(DataError errorLevel, NavRecordId recordId)
	{
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		bool flag;
		if (recordId.TableNo == 0 && errorLevel == DataError.TrapError)
		{
			flag = false;
		}
		else if (!IsOpen || SafeRecord.TableID != recordId.TableNo)
		{
			string companyName = string.Empty;
			if (IsOpen)
			{
				companyName = SafeRecord.RecordImplementation.GetActiveCompany();
			}
			NCLMetaTable metaTable;
			if (errorLevel == DataError.TrapError)
			{
				if (!NavGlobal.NCLMetadata.TryGetMetaTableById(recordId.TableNo, out metaTable))
				{
					return false;
				}
			}
			else
			{
				metaTable = NavGlobal.NCLMetadata.GetMetaTableById(recordId.TableNo);
			}
			NavRecord navRecord = metaTable.CreateObjectInstance(Target, isTemporary: false, null, companyName, SecurityFiltering);
			flag = navRecord.Get(errorLevel, recordId, copyRecord: true);
			if (flag)
			{
				SetRecord(navRecord);
			}
		}
		else
		{
			flag = SafeRecord.Get(errorLevel, recordId, copyRecord: true);
		}
		return flag;
	}

	public string ALGetFilter(int fieldNo)
	{
		return SafeRecord.ALGetFilter(fieldNo);
	}

	public string ALGetPosition()
	{
		return SafeRecord.ALGetPosition();
	}

	public string ALGetPosition(bool useCaptions)
	{
		return SafeRecord.ALGetPosition(useCaptions);
	}

	public void ALGetTable(NavRecord fromRecord)
	{
		if (fromRecord == null)
		{
			throw new ArgumentNullException("fromRecord");
		}
		SetRecord(fromRecord);
	}

	public string ALGetView()
	{
		return SafeRecord.ALGetView();
	}

	public string ALGetView(bool useCaptions)
	{
		return SafeRecord.ALGetView(useCaptions);
	}

	public void ALInsert()
	{
		SafeRecord.ALInsert();
	}

	public bool ALInsert(DataError errorLevel, bool runTrigger)
	{
		return SafeRecord.ALInsert(errorLevel, runTrigger);
	}

	public void ALInsert(bool runTrigger)
	{
		SafeRecord.ALInsert(runTrigger);
	}

	public bool ALInsert(DataError errorLevel)
	{
		return SafeRecord.ALInsert(errorLevel);
	}

	public NavKeyRef ALKeyIndex(ITreeObject parentOfResult, int index1Based)
	{
		if (!IsOpen)
		{
			throw new NavNCLRecordNotOpenedException();
		}
		if (ObsolescenceGuard.ObsolescenceIsEffective(ObsoleteState.Removed))
		{
			EnsureCacheOfKeysWithoutObsoleteStateRemoved();
			if (countOfKeysWithObsoleteStateRemoved > 0)
			{
				return new NavKeyRef(parentOfResult, this, keyIndexMapForKeysWithoutObsoleteStateRemoved[index1Based - 1], isTempAllocation: true);
			}
		}
		return new NavKeyRef(parentOfResult, this, index1Based - 1, isTempAllocation: true);
	}

	public void ALLockTable()
	{
		SafeRecord.ALLockTable();
	}

	public void ALLockTable(bool wait)
	{
		SafeRecord.ALLockTable(wait);
	}

	public void ALLockTable(bool wait, bool versionCheck)
	{
		SafeRecord.ALLockTable(wait, versionCheck);
	}

	public void ALModify()
	{
		SafeRecord.ALModify();
	}

	public bool ALModify(DataError errorLevel)
	{
		return SafeRecord.ALModify(errorLevel);
	}

	public void ALModify(bool runTrigger)
	{
		SafeRecord.ALModify(runTrigger);
	}

	public bool ALModify(DataError errorLevel, bool runTrigger)
	{
		return SafeRecord.ALModify(errorLevel, runTrigger);
	}

	public void ALModifyAll(int fieldNo, NavValue newValue)
	{
		SafeRecord.ALModifyAll(fieldNo, newValue);
	}

	public void ALModifyAll(int fieldNo, NavValue newValue, bool runTrigger)
	{
		SafeRecord.ALModifyAll(fieldNo, newValue, runTrigger);
	}

	public int ALNext()
	{
		return SafeRecord.ALNext();
	}

	public int ALNext(int steps)
	{
		return SafeRecord.ALNext(steps);
	}

	public void ALRename(params NavValue[] values)
	{
		SafeRecord.ALRename(values);
	}

	public bool ALRename(DataError errorLevel, params NavValue[] values)
	{
		return SafeRecord.ALRename(errorLevel, values);
	}

	public void ALReset()
	{
		securityFiltering = orginalFiltering;
		SafeRecord.ALReset();
	}

	public void ALSetPermissionFilter()
	{
		SafeRecord.ALSetPermissionFilter();
	}

	public void ALSetPosition(string position)
	{
		SafeRecord.ALSetPosition(position);
	}

	public void ALSetRecFilter()
	{
		SafeRecord.ALSetRecFilter();
	}

	public void ALSetTable(NavRecord toRecord)
	{
		if (toRecord == null)
		{
			throw new ArgumentNullException("toRecord");
		}
		if (SafeRecord.TableID != toRecord.TableID)
		{
			throw new NavNCLTableIdMismatchException();
		}
		toRecord.ALCopy(Record);
	}

	public void ALSetView(string view)
	{
		SafeRecord.ALSetView(view);
	}

	public bool ALChangeCompany()
	{
		return SafeRecord.ALChangeCompany();
	}

	public bool ALChangeCompany(DataError errorLevel)
	{
		return SafeRecord.ALChangeCompany(errorLevel);
	}

	public bool ALChangeCompany(string companyName)
	{
		return SafeRecord.ALChangeCompany(companyName);
	}

	public bool ALChangeCompany(DataError errorLevel, string companyName)
	{
		return SafeRecord.ALChangeCompany(errorLevel, companyName);
	}

	private static void CheckIsOpenAllowed(CompilationTarget compilationTarget, int tableId)
	{
		if (!IsOpenAllowed(compilationTarget, tableId))
		{
			string message = string.Format(CultureInfo.CurrentCulture, Lang.NotAllowedRecordRefCompilationTarget, tableId, compilationTarget);
			throw new NavNCLNotAllowedForCompilationTargetException(message);
		}
	}

	private void EnsureCacheOfFieldsWithoutObsoleteStateRemoved()
	{
		if (countOfFieldsWithObsoleteStateRemoved != -1)
		{
			return;
		}
		countOfFieldsWithObsoleteStateRemoved = MetaTable.AllFields.Count((NCLMetaField t) => t.ObsoleteState == ObsoleteState.Removed);
		if (countOfFieldsWithObsoleteStateRemoved > 0)
		{
			fieldsWithoutObsoleteStateRemoved = MetaTable.AllFields.Where((NCLMetaField t) => t.ObsoleteState != ObsoleteState.Removed).ToArray();
		}
	}

	private void EnsureCacheOfKeysWithoutObsoleteStateRemoved()
	{
		if (countOfKeysWithObsoleteStateRemoved != -1)
		{
			return;
		}
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < SafeRecord.MetaTable.KeyCount; i++)
		{
			if (SafeRecord.MetaTable.Keys[i].ObsoleteState == ObsoleteState.Removed)
			{
				num++;
			}
			else
			{
				list.Add(i);
			}
		}
		if (num > 0)
		{
			keyIndexMapForKeysWithoutObsoleteStateRemoved = list.ToArray();
		}
		countOfKeysWithObsoleteStateRemoved = num;
	}

	private void ClearObsoleteStateInformation()
	{
		fieldsWithoutObsoleteStateRemoved = null;
		countOfFieldsWithObsoleteStateRemoved = -1;
		keyIndexMapForKeysWithoutObsoleteStateRemoved = null;
		countOfKeysWithObsoleteStateRemoved = -1;
	}

	private static bool IsOpenAllowed(CompilationTarget compilationTarget, int tableId)
	{
		if (compilationTarget != CompilationTarget.Internal)
		{
			return !SystemTables.InternalTables.Contains(tableId);
		}
		return true;
	}

	public NavRecordRef(ITreeObject parent)
		: this(parent, SecurityFiltering.Validated)
	{
	}

	public NavRecordRef(ITreeObject parent, SecurityFiltering securityFiltering)
		: base(parent)
	{
		this.securityFiltering = securityFiltering;
		orginalFiltering = this.securityFiltering;
	}

	public NavRecordRef(ITreeObject parent, int tableId)
		: this(parent, tableId, isTempAllocation: false, SecurityFiltering.Validated)
	{
	}

	public NavRecordRef(ITreeObject parent, int tableId, bool isTempAllocation, SecurityFiltering securityFiltering)
		: base(parent)
	{
		this.isTempAllocation = isTempAllocation;
		this.securityFiltering = securityFiltering;
		orginalFiltering = this.securityFiltering;
		SetRecord(tableId, isTemporary: false, string.Empty);
	}

	public NavRecordRef(ITreeObject parent, int tableId, bool isTemporary, string companyName)
		: base(parent)
	{
		securityFiltering = SecurityFiltering.Validated;
		orginalFiltering = securityFiltering;
		SetRecord(tableId, isTemporary, companyName);
	}

	public NavRecordRef(ITreeObject parent, NavRecord fromRecordToBeCloned)
		: this(parent, fromRecordToBeCloned, isTempAllocation: false, SecurityFiltering.Validated)
	{
	}

	public NavRecordRef(ITreeObject parent, NavRecord fromRecordToBeCloned, SecurityFiltering securityFiltering)
		: this(parent, fromRecordToBeCloned, isTempAllocation: false, securityFiltering)
	{
	}

	public NavRecordRef(ITreeObject parent, NavRecord fromRecordToBeCloned, bool isTempAllocation, SecurityFiltering securityFiltering)
		: base(parent)
	{
		this.isTempAllocation = isTempAllocation;
		this.securityFiltering = securityFiltering;
		orginalFiltering = this.securityFiltering;
		if (fromRecordToBeCloned != null && !fromRecordToBeCloned.Tree.IsDisposed)
		{
			SetRecord(fromRecordToBeCloned);
		}
	}

	private void SetRecord(NavRecord recordToBeCloned)
	{
		if (recordToBeCloned == null)
		{
			Record = null;
			return;
		}
		if (Target?.Record?.ObjectId != recordToBeCloned.Target?.ObjectId)
		{
			ClearObsoleteStateInformation();
		}
		Record = recordToBeCloned.CloneRecord(Target, reset: false, keepCompany: true);
	}

	private void SetRecord(int tableID, bool isTemporary, string companyName)
	{
		if (tableID == 0)
		{
			Record = null;
			return;
		}
		NavRecord navRecord = NavGlobal.NCLMetadata.GetMetaTableById(tableID).CreateObjectInstance(Target, isTemporary, null, companyName, securityFiltering);
		if (Target?.Record?.ObjectId != navRecord.Target?.ObjectId)
		{
			ClearObsoleteStateInformation();
		}
		Record = navRecord;
	}

	public NavRecordRef Clone(ITreeObject parentOfClone)
	{
		NavRecordRef navRecordRef = new NavRecordRef(parentOfClone, securityFiltering);
		navRecordRef.Target = Target;
		return navRecordRef;
	}

	internal void InitFromRecordId(NavRecordId recordId, CompilationTarget compilationTarget = CompilationTarget.Internal)
	{
		if (recordId == null)
		{
			throw new ArgumentNullException("recordId");
		}
		CheckIsOpenAllowed(compilationTarget, recordId.TableNo);
		SafeRecord.InitFromRecordId(recordId);
	}

	public override int GetHashCode()
	{
		if (IsOpen)
		{
			return Record.GetHashCode();
		}
		return -1;
	}

	public override string ToString()
	{
		if (IsOpen)
		{
			return Record.ToString();
		}
		return string.Empty;
	}

	public override void Clear()
	{
		securityFiltering = orginalFiltering;
		if (IsOpen)
		{
			ALClose();
		}
	}

	public NavValue GetFieldValue(int fieldNo)
	{
		return SafeRecord.GetFieldValue(fieldNo);
	}

	public void SetFieldValue(int fieldNo, NavValue newValue)
	{
		SafeRecord.SetFieldValue(fieldNo, newValue);
	}

	public bool NextRecord()
	{
		return SafeRecord.NextRecord();
	}

	public bool PreviousRecord()
	{
		return SafeRecord.PreviousRecord();
	}
}
