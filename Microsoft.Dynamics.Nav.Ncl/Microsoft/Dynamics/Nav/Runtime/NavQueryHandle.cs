using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavQueryHandle : NavApplicationObjectBaseHandle<NavQuery>
{
	public class Factory : ElementFactory<NavQueryHandle>
	{
		private readonly int id;

		private readonly SecurityFiltering securityFiltering;

		public Factory(ITreeObject parentForArray, int id, SecurityFiltering securityFiltering)
			: base(parentForArray)
		{
			this.id = id;
			this.securityFiltering = securityFiltering;
		}

		public override NavQueryHandle CreateElement()
		{
			return new NavQueryHandle(ParentForElements, id, securityFiltering);
		}

		public override void AssignElement(NavQueryHandle destinationElement, NavQueryHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavQueryHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id, securityFiltering);
		}
	}

	private readonly SecurityFiltering securityFiltering;

	public override NavNclType NclType => NavNclType.NavQueryReference;

	public NavQueryHandle(ITreeObject parent, int id)
		: this(parent, id, SecurityFiltering.Filtered)
	{
	}

	public NavQueryHandle(ITreeObject parent, int id, SecurityFiltering securityFiltering)
		: base(parent, new ApplicationObjectId(ObjectType.Query, id))
	{
		this.securityFiltering = securityFiltering;
	}

	protected override NavQuery CreateTarget()
	{
		return NavGlobal.NCLMetadata.GetMetaQueryById(ObjectId.ObjectNumber).CreateObjectInstance(this, securityFiltering);
	}

	public override void Clear()
	{
		if (HasTarget)
		{
			Target = null;
		}
	}

	public void ALAssign(NavQueryHandle other)
	{
		if (other == null)
		{
			throw new ArgumentNullException("other");
		}
		if (ObjectId != other.ObjectId)
		{
			throw new NavNCLNotSupportedOperationException();
		}
		Target = other.Target;
	}

	public NavQueryHandle ALByValue(ITreeObject parentOfResult)
	{
		throw new NavNCLNotSupportedOperationException();
	}
}
