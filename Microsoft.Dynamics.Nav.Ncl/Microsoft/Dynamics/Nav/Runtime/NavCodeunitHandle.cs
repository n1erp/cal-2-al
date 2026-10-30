using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavCodeunitHandle : NavApplicationObjectBaseHandle<NavCodeunit>
{
	public class Factory : ElementFactory<NavCodeunitHandle>
	{
		private int id;

		public Factory(ITreeObject parentForArray, int id)
			: base(parentForArray)
		{
			this.id = id;
		}

		public override NavCodeunitHandle CreateElement()
		{
			return new NavCodeunitHandle(ParentForElements, id);
		}

		public override void AssignElement(NavCodeunitHandle destinationElement, NavCodeunitHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavCodeunitHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id);
		}
	}

	public override NavNclType NclType => NavNclType.NavCodeunitReference;

	public NavCodeunitHandle(ITreeObject parent, int id)
		: base(parent, new ApplicationObjectId(ObjectType.CodeUnit, id))
	{
	}

	public NavCodeunitHandle(ITreeObject parent, NavCodeunit initialTarget)
		: base(parent, initialTarget)
	{
	}

	protected override NavCodeunit CreateTarget()
	{
		return NavGlobal.NCLMetadata.GetMetaCodeunitById(ObjectId.ObjectNumber).CreateObjectInstance();
	}

	public void ClearReference()
	{
		Target = null;
	}

	public NavCodeunitHandle CloneReference(ITreeObject parentOfResult)
	{
		NavCodeunitHandle navCodeunitHandle = new NavCodeunitHandle(parentOfResult, ObjectId.ObjectNumber);
		navCodeunitHandle.Target = Target;
		return navCodeunitHandle;
	}

	public void ALAssign(NavCodeunitHandle other)
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

	public NavCodeunitHandle ALByValue(ITreeObject parentOfResult)
	{
		return CloneReference(parentOfResult);
	}
}
