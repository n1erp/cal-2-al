using System;
using Microsoft.Dynamics.Nav.Types;
using Microsoft.Dynamics.Nav.Types.Exceptions;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class UninitializedTestPageScaffolding : UninitializedHandleScaffolding
{
	private ApplicationObjectId objectId;

	private string typeString;

	public override string TypeString => typeString;

	public UninitializedTestPageScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is INavApplicationObjectBaseHandle { HasTarget: false } navApplicationObjectBaseHandle)
		{
			return navApplicationObjectBaseHandle.ObjectId == objectId;
		}
		return false;
	}

	protected override void ReadMetadata(INavApplicationObjectBaseHandle instance)
	{
		if (instance != null)
		{
			objectId = instance.ObjectId;
			string objectName;
			try
			{
				objectName = NavCurrentThread.Session.GetObjectName(objectId);
			}
			catch (NavNCLObjectNotFoundException)
			{
				objectName = null;
			}
			typeString = ScaffoldingHelper.GetTypeString("TestPage", objectName, objectId.ObjectNumber);
		}
		else
		{
			typeString = string.Empty;
		}
	}
}
