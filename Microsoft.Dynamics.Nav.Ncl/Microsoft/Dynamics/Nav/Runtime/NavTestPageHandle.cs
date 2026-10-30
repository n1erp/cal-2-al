using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime;

public class NavTestPageHandle : NavApplicationObjectBaseHandle<NavTestPage>
{
	public override NavNclType NclType => NavNclType.NavTestPageReference;

	public NavTestPageHandle(ITreeObject parent, int id)
		: base(parent, new ApplicationObjectId(ObjectType.Page, id))
	{
	}

	public NavTestPageHandle(ITreeObject parent, NavTestPage target)
		: base(parent, target)
	{
	}

	public void ALAssign(NavTestPageHandle other)
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

	public void ClearReference()
	{
		Target = null;
	}

	protected override NavTestPage CreateTarget()
	{
		return new NavTestPage(NavCurrentThread.Session.Company.SharedObjects, ObjectId);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
