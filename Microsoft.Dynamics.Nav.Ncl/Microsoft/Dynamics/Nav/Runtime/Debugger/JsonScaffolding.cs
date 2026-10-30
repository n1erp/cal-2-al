using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class JsonScaffolding : Scaffolding<NavJsonToken>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string IsArrayName = "IsArray";

	private const string IsObjectname = "IsObject";

	private const string IsValueName = "IsValue";

	private readonly NavNclType nclType;

	public override string TypeString => ScaffoldingHelper.GetTypeString(nclType, null);

	protected override bool ProbablyHasChildren => nclType == NavNclType.NavJsonToken;

	public JsonScaffolding(string name, Type type, NavNclType nclType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, false, promotedChildren)
	{
		this.nclType = nclType;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance is NavJsonToken navJsonToken)
		{
			return navJsonToken.NclType == nclType;
		}
		return false;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavJsonToken instance)
	{
		if (instance.NclType == NavNclType.NavJsonToken)
		{
			Type jsonTokenType = instance.GetType();
			PropertyInfo property = jsonTokenType.GetProperty("ALIsArray", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			yield return ScaffoldingFactory.GetScaffolding("IsArray", typeof(bool), new PropertyInfoValueBinder(property), instance, false);
			PropertyInfo property2 = jsonTokenType.GetProperty("ALIsObject", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			yield return ScaffoldingFactory.GetScaffolding("IsObject", typeof(bool), new PropertyInfoValueBinder(property2), instance, false);
			PropertyInfo property3 = jsonTokenType.GetProperty("ALIsValue", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			yield return ScaffoldingFactory.GetScaffolding("IsValue", typeof(bool), new PropertyInfoValueBinder(property3), instance, false);
		}
	}

	protected override string GetStringValue(NavJsonToken instance)
	{
		if (instance != null)
		{
			return Scaffolding.TruncateIfNeeded(instance.ToString());
		}
		return "<Uninitialized>";
	}
}
