using System;
using System.Diagnostics;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

[DebuggerTypeProxy(typeof(NavRecordHandleDebugView))]
public class NavRecordHandle : NavApplicationObjectBaseHandle<NavRecord>, INavRecordHandle, INavApplicationObjectBaseHandle
{
	public class Factory : ElementFactory<NavRecordHandle>
	{
		private readonly int id;

		private bool temporary;

		private readonly SecurityFiltering securityFiltering;

		private NavRecord firstRecord;

		internal bool Temporary
		{
			set
			{
				temporary = value;
			}
		}

		internal NavRecord FirstRecord
		{
			get
			{
				return firstRecord;
			}
			set
			{
				firstRecord = value;
			}
		}

		public Factory(ITreeObject parentForArray, int id, bool temporary, SecurityFiltering securityFiltering)
			: base(parentForArray)
		{
			this.id = id;
			this.temporary = temporary;
			this.securityFiltering = securityFiltering;
		}

		public override NavRecordHandle CreateElement()
		{
			return new NavRecordHandle(ParentForElements, id, temporary, securityFiltering, this);
		}

		public override void AssignElement(NavRecordHandle destinationElement, NavRecordHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavRecordHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id, temporary, securityFiltering);
		}
	}

	private readonly bool temp;

	private readonly Factory factory;

	private readonly SecurityFiltering securityFiltering;

	public override NavNclType NclType => NavNclType.NavRecordReference;

	public NavRecordHandle(ITreeObject parent, int id, bool temporary, SecurityFiltering securityFiltering)
		: base(parent, new ApplicationObjectId(ObjectType.Table, id))
	{
		temp = temporary;
		this.securityFiltering = securityFiltering;
	}

	internal NavRecordHandle(ITreeObject parent, int id, bool temporary, SecurityFiltering securityFiltering, Factory factory)
		: this(parent, id, temporary, securityFiltering)
	{
		this.factory = factory;
	}

	protected override NavRecord CreateTarget()
	{
		NavRecord navRecord;
		if (factory == null || !temp)
		{
			navRecord = NavGlobal.NCLMetadata.GetMetaTableById(ObjectId.ObjectNumber).CreateObjectInstance(this, temp, null, string.Empty, securityFiltering);
		}
		else if (factory.FirstRecord != null)
		{
			navRecord = NavGlobal.NCLMetadata.GetMetaTableById(ObjectId.ObjectNumber).CreateObjectInstance(this, temp, factory.FirstRecord, string.Empty, securityFiltering);
		}
		else
		{
			navRecord = NavGlobal.NCLMetadata.GetMetaTableById(ObjectId.ObjectNumber).CreateObjectInstance(this, temp, null, string.Empty, securityFiltering);
			factory.FirstRecord = navRecord;
		}
		return navRecord;
	}

	public NavRecordHandle CloneReference(ITreeObject parentForResult, bool cloneAsTemporary)
	{
		NavRecordHandle navRecordHandle = new NavRecordHandle(parentForResult, ObjectId.ObjectNumber, cloneAsTemporary, securityFiltering);
		navRecordHandle.Target.ALAssign(Target);
		return navRecordHandle;
	}

	public void ALAssign(INavRecordHandle other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (ObjectId != other.ObjectId)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		Target.Assign(other.Target);
	}

	public NavRecordHandle ALByValue(ITreeObject parentOfResult, bool cloneAsTemporary)
	{
		return CloneReference(parentOfResult, cloneAsTemporary);
	}

	public INavRecordHandle Clone(ITreeObject parentOfClone)
	{
		return CloneReference(parentOfClone, temp);
	}
}
