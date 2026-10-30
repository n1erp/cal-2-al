using System;
using Microsoft.Dynamics.Nav.Types;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class UninitializedHandleScaffolding : Scaffolding<INavApplicationObjectBaseHandle>
{
	private ApplicationObjectId objectId;

	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => false;

	public UninitializedHandleScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, false, promotedChildren)
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

	protected override string GetStringValue(INavApplicationObjectBaseHandle instance)
	{
		return "<Uninitialized>";
	}

	protected override void ReadMetadata(INavApplicationObjectBaseHandle instance)
	{
		if (instance != null)
		{
			objectId = instance.ObjectId;
			typeString = ScaffoldingHelper.GetTypeString(objectId);
		}
		else
		{
			typeString = string.Empty;
		}
	}
}
