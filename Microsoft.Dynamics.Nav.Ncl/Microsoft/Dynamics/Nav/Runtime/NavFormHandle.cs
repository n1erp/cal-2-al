using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavFormHandle : NavApplicationObjectBaseHandle<NavForm>
{
	public class Factory : ElementFactory<NavFormHandle>
	{
		private int id;

		public Factory(ITreeObject parentForArray, int id)
			: base(parentForArray)
		{
			this.id = id;
		}

		public override NavFormHandle CreateElement()
		{
			return new NavFormHandle(ParentForElements, id);
		}

		public override void AssignElement(NavFormHandle destinationElement, NavFormHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavFormHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id);
		}
	}

	private NavForm activateTarget;

	public override NavNclType NclType => NavNclType.NavFormReference;

	public NavFormHandle(ITreeObject parent, NavForm target)
		: base(parent, target)
	{
	}

	public NavFormHandle(ITreeObject parent, int id)
		: base(parent, new ApplicationObjectId(ObjectType.Page, id))
	{
	}

	protected override NavForm CreateTarget()
	{
		return NavGlobal.NCLMetadata.GetMetaFormById(ObjectId.ObjectNumber).CreateObjectInstance();
	}

	public override void Clear()
	{
		if (HasTarget)
		{
			Target.Close();
			Target = null;
		}
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
		try
		{
			Target.Run(record, fieldNo);
		}
		finally
		{
			activateTarget = Target;
			Target = null;
		}
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
		try
		{
			return Target.RunModal(record, fieldNo);
		}
		catch
		{
			Target = null;
			throw;
		}
	}

	public void ALAssign(NavFormHandle other)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public bool Activate()
	{
		return Activate(refresh: false);
	}

	public bool Activate(bool refresh)
	{
		if (activateTarget != null)
		{
			activateTarget.Activate(refresh);
		}
		return true;
	}

	public NavFormHandle ALByValue(ITreeObject parentOfResult)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	protected override void Dispose(bool disposing)
	{
		activateTarget = null;
		base.Dispose(disposing);
	}
}
