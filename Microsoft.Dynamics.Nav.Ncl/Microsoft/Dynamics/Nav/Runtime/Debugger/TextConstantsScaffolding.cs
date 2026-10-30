using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class TextConstantsScaffolding : Scaffolding
{
	public const string GlobalTextConstantsNodeName = "<Global Text Constants>";

	public const string LocalTextConstantsNodeName = "<Local Text Constants>";

	public override string TypeString => string.Empty;

	protected override bool ProbablyHasChildren => true;

	public TextConstantsScaffolding(string name, Type targetType, ValueBinder valueBinder)
		: base(name, targetType, valueBinder, childrenTypesCanChange: false, promotedChildren: true)
	{
	}

	protected internal override string GetStringValue(object instance)
	{
		return string.Empty;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance != null)
		{
			return instance.GetType() == TargetType;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(object instance)
	{
		return from field in instance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
			where field.FieldType == typeof(NavTextConstant)
			let valueBinder = new FieldInfoValueBinder(field)
			select (Scaffolding)new NavValueScaffolding(field.AlName(), field.FieldType, NavNclType.NavTextConstant, valueBinder);
	}
}
