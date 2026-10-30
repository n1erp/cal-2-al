using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class KeyRefScaffolding : Scaffolding<NavKeyRef>
{
	private NCLMetaKey metaKey;

	public override string TypeString
	{
		get
		{
			if (!Initialized)
			{
				return ScaffoldingHelper.GetTypeString(NavNclType.NavKeyRef, null);
			}
			return string.Format(CultureInfo.CurrentCulture, "KeyRef Table {0} ({1})", metaKey.Parent.TableName, metaKey.Parent.TableId);
		}
	}

	protected override bool ProbablyHasChildren => Initialized;

	private bool Initialized => metaKey != null;

	public KeyRefScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavKeyRef navKeyRef)
		{
			return navKeyRef.IsInitialized == Initialized;
		}
		return false;
	}

	protected override string GetStringValue(NavKeyRef instance)
	{
		if (!instance.IsInitialized)
		{
			return "<Uninitialized>";
		}
		return instance.ToString();
	}

	protected override void ReadMetadata(NavKeyRef instance)
	{
		if (instance != null && instance.IsInitialized)
		{
			metaKey = instance.MetaKey;
		}
	}
}
