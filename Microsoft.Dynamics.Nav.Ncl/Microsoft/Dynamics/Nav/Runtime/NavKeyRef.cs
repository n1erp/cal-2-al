using System;
using System.Text;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public sealed class NavKeyRef : NavComplexValue, ITreeObjectReference, ITreeObject
{
	public class Factory : ElementFactory<NavKeyRef>
	{
		public Factory(ITreeObject parentForArray)
			: base(parentForArray)
		{
		}

		public override NavKeyRef CreateElement()
		{
			return new NavKeyRef(ParentForElements);
		}

		public override void AssignElement(NavKeyRef destinationElement, NavKeyRef sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavKeyRef> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone);
		}
	}

	public const int ALPrimaryKeyIndex = 1;

	private NCLMetaKey metaKey;

	private bool isTempAllocation;

	public int ALFieldCount
	{
		get
		{
			CheckInitialized();
			return metaKey.KeyFieldCount;
		}
	}

	public bool ALActive
	{
		get
		{
			CheckInitialized();
			return metaKey.KeyIsActive;
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

	private NavRecord OwnerRecord
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

	public NavRecord Record
	{
		get
		{
			CheckInitialized();
			return OwnerRecord;
		}
	}

	internal NCLMetaKey MetaKey => metaKey;

	public string Value
	{
		get
		{
			CheckInitialized();
			StringBuilder stringBuilder = new StringBuilder(200);
			for (int i = 0; i < metaKey.KeyFieldCount; i++)
			{
				stringBuilder.Append("Field" + metaKey.GetKeyFieldByIndex(i).FieldNo.ToString(NavCurrentThread.Session.WindowsCulture) + ",");
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Length--;
			}
			return stringBuilder.ToString();
		}
	}

	public override bool IsInitialized => OwnerRecord != null;

	public override NavNclType NclType => NavNclType.NavKeyRef;

	public override int GetBytesSize => 0;

	public NavFieldRef ALFieldIndex(ITreeObject parentOfResult, int fieldIndex1Based)
	{
		CheckInitialized();
		using NavRecordRef navRecordRef = new NavRecordRef(parentOfResult, null, OwnerRecord.SecurityFiltering);
		navRecordRef.Record = OwnerRecord;
		return new NavFieldRef(parentOfResult, navRecordRef, metaKey.GetKeyFieldByIndex(fieldIndex1Based - 1), isTempAllocation: true);
	}

	public NavRecordRef ALRecord(ITreeObject parentOfClonedResult)
	{
		CheckInitialized();
		return new NavRecordRef(parentOfClonedResult, Record, isTempAllocation: true, Record.SecurityFiltering);
	}

	public void ALAssign(NavKeyRef other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		Target = other.Target;
		if (other.isTempAllocation)
		{
			other.Dispose();
		}
		metaKey = other.metaKey;
	}

	public NavKeyRef ALByValue(ITreeObject parentOfResult)
	{
		NavKeyRef navKeyRef = new NavKeyRef(parentOfResult);
		navKeyRef.ALAssign(this);
		return navKeyRef;
	}

	public NavKeyRef(ITreeObject parent)
		: base(parent)
	{
	}

	public NavKeyRef(ITreeObject parent, NavRecordRef ownerRecordRef, int keyIndex)
		: this(parent, ownerRecordRef, keyIndex, isTempAllocation: false)
	{
	}

	public NavKeyRef(ITreeObject parent, NavRecordRef ownerRecordRef, int keyIndex, bool isTempAllocation)
		: base(parent)
	{
		if (ownerRecordRef == null)
		{
			throw new ArgumentNullException("ownerRecordRef");
		}
		Target = ownerRecordRef.Target;
		metaKey = ownerRecordRef.Record.MetaTable.GetKeyByIndex(keyIndex);
		this.isTempAllocation = isTempAllocation;
	}

	private void CheckInitialized()
	{
		if (OwnerRecord == null)
		{
			throw new NavNCLNotInitializedException(GetType());
		}
	}

	public override void Clear()
	{
		Target = null;
	}

	public override int GetHashCode()
	{
		if (OwnerRecord != null)
		{
			return OwnerRecord.GetHashCode() ^ metaKey.GetHashCode();
		}
		return -1;
	}
}
