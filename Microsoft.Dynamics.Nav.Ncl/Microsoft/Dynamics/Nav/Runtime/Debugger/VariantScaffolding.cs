using System;
using System.Collections.Generic;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class VariantScaffolding : Scaffolding<NavVariant>
{
	private Scaffolding innerScaffolding;

	private Type innerType;

	private bool isInitialized;

	public override string TypeString
	{
		get
		{
			if (!isInitialized)
			{
				return "Variant";
			}
			return string.Format(CultureInfo.CurrentCulture, "Variant ({0})", innerType.Name);
		}
	}

	protected override bool ProbablyHasChildren => isInitialized;

	public VariantScaffolding(string name, Type targetType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, targetType, valueBinder, false, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavVariant navVariant && isInitialized == navVariant.IsInitialized)
		{
			if (!isInitialized)
			{
				return true;
			}
			return navVariant.InnerValue.GetType() == innerType;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavVariant instance)
	{
		if (isInitialized)
		{
			yield return innerScaffolding;
		}
	}

	protected override string GetStringValue(NavVariant instance)
	{
		if (!isInitialized)
		{
			return "<Uninitialized>";
		}
		return innerScaffolding.GetStringValue(innerScaffolding.CorceValue(instance.InnerValue));
	}

	protected override void ReadMetadata(NavVariant instance)
	{
		if (instance == null)
		{
			return;
		}
		isInitialized = instance.IsInitialized;
		if (isInitialized)
		{
			innerType = instance.InnerValue.GetType();
			innerScaffolding = ScaffoldingFactory.GetScaffolding("InnerValue", instance.InnerValue.GetType(), new PropertyAccessValueBinder<NavVariant>((NavVariant var) => var.InnerValue, "InnerValue", isSetSupported: true), instance, false);
		}
	}
}
