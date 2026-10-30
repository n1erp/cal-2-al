using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Metadata;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavFieldRef : NavIndirectValue, ITreeObjectReference, ITreeObject
{
	public class Factory : ElementFactory<NavFieldRef>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavFieldRef CreateElement()
		{
			return new NavFieldRef(ParentForElements);
		}

		public override void AssignElement(NavFieldRef destinationElement, NavFieldRef sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavFieldRef> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	private NCLMetaField metaField;

	private bool isTempAllocation;

	public const int TimestampMaxSize = 8;

	public int ALNumber
	{
		get
		{
			CheckInitialized();
			return FieldNo;
		}
	}

	public NavType ALType
	{
		get
		{
			CheckInitialized();
			return FieldType;
		}
	}

	public int ALLength
	{
		get
		{
			CheckInitialized();
			return FieldLength;
		}
	}

	public string ALName
	{
		get
		{
			CheckInitialized();
			return MetaField.FieldName;
		}
	}

	public string ALCaption
	{
		get
		{
			CheckInitialized();
			NavText dynamicCaption = OwnerRecord.GetDynamicCaption(metaField.FieldNo);
			if (dynamicCaption != NavOemText.Empty)
			{
				return dynamicCaption.Value;
			}
			return MetaField.FieldCaption;
		}
	}

	public FieldClass ALClass
	{
		get
		{
			CheckInitialized();
			return MetaField.FieldClass;
		}
	}

	public string ALOptionString
	{
		get
		{
			CheckInitialized();
			if (MetaField.FieldNclType != NavNclType.NavOption)
			{
				return string.Empty;
			}
			return MetaField.FieldOptionMetadata.OptionString;
		}
	}

	public string ALOptionCaption
	{
		get
		{
			CheckInitialized();
			if (metaField.FieldNclType != NavNclType.NavOption)
			{
				return string.Empty;
			}
			return metaField.FieldOptionMetadata.OptionCaption;
		}
	}

	public bool ALActive
	{
		get
		{
			CheckInitialized();
			return MetaField.FieldActive;
		}
	}

	public NavValue ALValue
	{
		get
		{
			CheckInitialized();
			NavValue fieldValue = OwnerRecord.GetFieldValue(MetaField);
			if (isTempAllocation)
			{
				Dispose();
			}
			return fieldValue;
		}
		set
		{
			CheckInitialized();
			OwnerRecord.SetFieldValue(MetaField, value);
		}
	}

	public int ALRelation
	{
		get
		{
			CheckInitialized();
			return OwnerRecord.RecordImplementation.GetRelation(metaField);
		}
	}

	public string ALGetFilter
	{
		get
		{
			CheckInitialized();
			return OwnerRecord.ALGetFilter(MetaField.FieldNo);
		}
	}

	public NavValue ALGetRangeMin
	{
		get
		{
			CheckInitialized();
			return OwnerRecord.GetRangeMin(MetaField);
		}
	}

	public NavValue ALGetRangeMax
	{
		get
		{
			CheckInitialized();
			return OwnerRecord.GetRangeMax(MetaField);
		}
	}

	private bool HasTarget => Tree.GetReferenceTarget() != null;

	private SharedRecordRef Target
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

	internal NavRecord OwnerRecord
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

	public override bool IsInitialized => metaField != null;

	public override NavValue InnerValue
	{
		get
		{
			CheckInitialized();
			return OwnerRecord.GetFieldValue(metaField);
		}
	}

	public NavValue Value
	{
		get
		{
			return InnerValue;
		}
		set
		{
			CheckInitialized();
			OwnerRecord.SetFieldValue(metaField, value);
		}
	}

	public NCLMetaField MetaField
	{
		get
		{
			CheckInitialized();
			return metaField;
		}
	}

	public int FieldNo => MetaField.FieldNo;

	public NavType FieldType => MetaField.FieldNavType;

	public int FieldLength => MetaField.FieldDefinedLength;

	public override int GetBytesSize => Value.GetBytesSize;

	public override NavNclType NclType => NavNclType.NavFieldRef;

	public override object ValueAsObject => InnerValue.ValueAsObject;

	public NavRecordRef ALRecord(ITreeObject parentOfResult)
	{
		CheckInitialized();
		NavRecordRef navRecordRef = new NavRecordRef(parentOfResult, null, isTempAllocation: true, (OwnerRecord != null) ? OwnerRecord.SecurityFiltering : SecurityFiltering.Validated);
		navRecordRef.Record = OwnerRecord;
		return navRecordRef;
	}

	public void ALAssign(NavFieldRef other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Target = other.Target;
		metaField = other.metaField;
		if (other.isTempAllocation)
		{
			other.Dispose();
		}
	}

	public NavFieldRef ALByValue(ITreeObject parentForResult)
	{
		NavFieldRef navFieldRef = new NavFieldRef(parentForResult);
		navFieldRef.ALAssign(this);
		return navFieldRef;
	}

	public bool ALCalcField()
	{
		return ALCalcField(DataError.ThrowError);
	}

	public bool ALCalcField(DataError errorLevel)
	{
		CheckInitialized();
		return OwnerRecord.ALCalcFields(errorLevel, MetaField.FieldNo);
	}

	public bool ALCalcSum()
	{
		return ALCalcSum(DataError.ThrowError);
	}

	public bool ALCalcSum(DataError errorLevel)
	{
		CheckInitialized();
		return OwnerRecord.ALCalcSums(errorLevel, MetaField.FieldNo);
	}

	public void ALSetRange()
	{
		ALSetRange(null, null);
	}

	public void ALSetRange(NavValue fromValue)
	{
		ALSetRange(fromValue, null);
	}

	public void ALSetRange(NavValue fromValue, NavValue toValue)
	{
		CheckInitialized();
		OwnerRecord.SetRange(MetaField, fromValue, toValue);
	}

	public void ALSetFilter(string expression, params NavValue[] values)
	{
		CheckInitialized();
		OwnerRecord.SetFilter(MetaField, expression, values);
	}

	public void ALTestField()
	{
		CheckInitialized();
		OwnerRecord.TestFieldNotBlank(MetaField);
	}

	public void ALTestField(byte testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(bool testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(int testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(long testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(Decimal18 testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(Guid testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestField(string testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEquals(MetaField, testValue);
	}

	public void ALTestFieldNavValue(NavValue testValue)
	{
		CheckInitialized();
		OwnerRecord.TestFieldEqualsNavValue(MetaField, testValue);
	}

	public void ALFieldError()
	{
		ALFieldError(null);
	}

	public void ALFieldError(string errorMessage)
	{
		OwnerRecord.ALFieldError(MetaField.FieldNo, errorMessage);
	}

	public void ALValidate(NavRecord callerRecord)
	{
		ALValidate(null, callerRecord);
	}

	public void ALValidate(NavValue newValue, NavRecord callerRecord)
	{
		CheckInitialized();
		OwnerRecord.Validate(MetaField, newValue, callerRecord);
	}

	public void ALValidateSafe()
	{
		ALValidate(null, NavRecord.GetCallerRecord());
	}

	public void ALValidateSafe(NavValue newValue)
	{
		CheckInitialized();
		OwnerRecord.ALValidateSafe(MetaField.FieldNo, MetaField.FieldNavType, newValue);
	}

	public NavFieldRef(ITreeObject parent)
		: base(parent)
	{
	}

	public NavFieldRef(ITreeObject parent, NavRecordRef ownerRecordRef, NCLMetaField metaField)
		: this(parent, ownerRecordRef, metaField, isTempAllocation: false)
	{
	}

	public NavFieldRef(ITreeObject parent, NavRecordRef ownerRecordRef, NCLMetaField metaField, bool isTempAllocation)
		: base(parent)
	{
		Initialize(ownerRecordRef, metaField, isTempAllocation);
	}

	public NavFieldRef(ITreeObject parent, NavRecordRef ownerRecordRef, int fieldNo)
		: this(parent, ownerRecordRef, fieldNo, isTempAllocation: false)
	{
	}

	public NavFieldRef(ITreeObject parent, NavRecordRef ownerRecordRef, int fieldNo, bool isTempAllocation)
		: base(parent)
	{
		if (ownerRecordRef == null)
		{
			throw new ArgumentNullException("ownerRecordRef");
		}
		Initialize(ownerRecordRef, ownerRecordRef.Target.Record.MetaTable.GetFieldByNo(fieldNo), isTempAllocation);
	}

	internal void Initialize(NavRecordRef ownerRecordRef, NCLMetaField sourceMetaField, bool tempAllocation)
	{
		if (ownerRecordRef == null)
		{
			throw new ArgumentNullException("ownerRecordRef");
		}
		if (sourceMetaField == null)
		{
			throw new ArgumentNullException("sourceMetaField");
		}
		ObsolescenceGuard.ThrowIfObsoleted(sourceMetaField);
		Target = ownerRecordRef.Target;
		metaField = sourceMetaField;
		isTempAllocation = tempAllocation;
	}

	public override void Clear()
	{
		Target = null;
		metaField = null;
	}

	public override byte[] GetBytes()
	{
		CheckInitialized();
		return Value.GetBytes();
	}

	public override int GetHashCode()
	{
		if (IsInitialized)
		{
			return OwnerRecord.GetHashCode() ^ MetaField.GetHashCode();
		}
		return string.Empty.GetHashCode();
	}

	public override string ToString()
	{
		return NavFormatEvaluateHelper.Format(this);
	}
}
