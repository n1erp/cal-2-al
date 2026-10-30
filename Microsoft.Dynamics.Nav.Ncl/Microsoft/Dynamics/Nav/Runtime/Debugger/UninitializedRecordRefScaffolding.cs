using System;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class UninitializedRecordRefScaffolding : Scaffolding<NavRecordRef>
{
	public override string TypeString => "RecordRef";

	protected override bool ProbablyHasChildren => false;

	public UninitializedRecordRefScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, false, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavRecordRef navRecordRef)
		{
			return !navRecordRef.HasTarget;
		}
		return false;
	}

	protected override string GetStringValue(NavRecordRef instance)
	{
		return "<Uninitialized>";
	}
}
