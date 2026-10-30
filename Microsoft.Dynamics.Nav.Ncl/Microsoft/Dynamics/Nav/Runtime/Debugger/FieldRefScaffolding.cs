using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class FieldRefScaffolding : Scaffolding<NavFieldRef>
{
	private NCLMetaField metaField;

	public override string TypeString
	{
		get
		{
			if (!Initialized)
			{
				return "FieldRef";
			}
			return string.Format(CultureInfo.InvariantCulture, "FieldRef {0}.{1} (Table {2})", metaField.Parent.TableName, metaField.FieldName, metaField.Parent.TableId);
		}
	}

	protected override bool ProbablyHasChildren => Initialized;

	private bool Initialized => metaField != null;

	public FieldRefScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavFieldRef navFieldRef)
		{
			return navFieldRef.IsInitialized == Initialized;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavFieldRef instance)
	{
		if (Initialized)
		{
			yield return new RecordScaffolding("Record", instance.OwnerRecord.GetType(), new PropertyAccessValueBinder<NavFieldRef>((NavFieldRef fieldRef) => fieldRef.OwnerRecord, "OwnerRecord"), promotedChildren: false);
		}
	}

	protected override string GetStringValue(NavFieldRef instance)
	{
		if (!instance.IsInitialized)
		{
			return "<Uninitialized>";
		}
		return instance.Value.ToString();
	}

	protected override void ReadMetadata(NavFieldRef instance)
	{
		if (instance != null && instance.IsInitialized)
		{
			metaField = instance.MetaField;
		}
	}
}
