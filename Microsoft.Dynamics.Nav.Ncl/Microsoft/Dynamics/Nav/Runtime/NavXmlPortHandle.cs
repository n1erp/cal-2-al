using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavXmlPortHandle : NavApplicationObjectBaseHandle<NavXmlPort>
{
	public class Factory : ElementFactory<NavXmlPortHandle>
	{
		private int id;

		public Factory(ITreeObject parentForArray, int id)
			: base(parentForArray)
		{
			this.id = id;
		}

		public override NavXmlPortHandle CreateElement()
		{
			return new NavXmlPortHandle(ParentForElements, id);
		}

		public override void AssignElement(NavXmlPortHandle destinationElement, NavXmlPortHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavXmlPortHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id);
		}
	}

	public override NavNclType NclType => NavNclType.NavXmlPortReference;

	public NavXmlPortHandle(ITreeObject parent, int id)
		: base(parent, new ApplicationObjectId(ObjectType.XmlPort, id))
	{
	}

	protected override NavXmlPort CreateTarget()
	{
		return NavGlobal.NCLMetadata.GetMetaXmlPortById(ObjectId.ObjectNumber).CreateObjectInstance(this);
	}

	public override void Clear()
	{
		if (HasTarget)
		{
			Target = null;
		}
	}

	public void ALAssign(NavXmlPortHandle other)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public NavXmlPortHandle ALByValue(ITreeObject parentOfResult)
	{
		throw new NavNCLNotSupportedOperationException();
	}
}
