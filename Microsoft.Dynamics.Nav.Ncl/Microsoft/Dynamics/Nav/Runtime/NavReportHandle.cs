using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavReportHandle : NavApplicationObjectBaseHandle<NavReport>
{
	public class Factory : ElementFactory<NavReportHandle>
	{
		private int id;

		public Factory(ITreeObject parentForArray, int id)
			: base(parentForArray)
		{
			this.id = id;
		}

		public override NavReportHandle CreateElement()
		{
			return new NavReportHandle(ParentForElements, id);
		}

		public override void AssignElement(NavReportHandle destinationElement, NavReportHandle sourceElement)
		{
			if (destinationElement == null)
			{
				throw new ArgumentNullException("destinationElement");
			}
			destinationElement.ALAssign(sourceElement);
		}

		public override ElementFactory<NavReportHandle> Clone(ITreeObject parentOfClone)
		{
			return new Factory(parentOfClone, id);
		}
	}

	public override NavNclType NclType => NavNclType.NavReportReference;

	public NavReportHandle(ITreeObject parent, int id)
		: base(parent, new ApplicationObjectId(ObjectType.Report, id))
	{
	}

	public NavReportHandle(ITreeObject parent, NavReport report)
		: base(parent, report)
	{
	}

	protected override NavReport CreateTarget()
	{
		return NavGlobal.NCLMetadata.GetMetaReportById(ObjectId.ObjectNumber).CreateObjectInstance(this);
	}

	public override void Clear()
	{
		if (HasTarget)
		{
			Target = null;
		}
	}

	public void ALAssign(NavReportHandle other)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public NavReportHandle ALByValue(ITreeObject parentOfResult)
	{
		throw new NavNCLNotSupportedOperationException();
	}

	public void Run()
	{
		try
		{
			Target.Run();
		}
		finally
		{
			Target = null;
		}
	}

	public void RunModal()
	{
		try
		{
			Target.RunModal();
		}
		catch
		{
			Target = null;
			throw;
		}
	}
}
