using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class HttpClientScaffolding : Scaffolding<NavHttpClient>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string GetBaseAddressName = "BaseAddress";

	private const string DefaultRequestHeadersName = "DefaultRequestHeaders";

	private const string TimeoutName = "Timeout";

	public override string TypeString => ScaffoldingHelper.GetTypeString(NavNclType.NavHttpClient, null);

	protected override bool ProbablyHasChildren => true;

	public HttpClientScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, true, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavHttpClient;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavHttpClient instance)
	{
		Type httpClientType = instance.GetType();
		PropertyInfo property = httpClientType.GetProperty("ALGetBaseAddress", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("BaseAddress", typeof(string), new PropertyInfoValueBinder(property), instance, false);
		PropertyInfo property2 = httpClientType.GetProperty("ALDefaultRequestHeaders", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("DefaultRequestHeaders", typeof(NavHttpHeaders), new PropertyInfoValueBinder(property2), instance, false);
		PropertyInfo property3 = httpClientType.GetProperty("ALTimeout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Timeout", typeof(NavDuration), new PropertyInfoValueBinder(property3), instance, false);
	}

	protected override string GetStringValue(NavHttpClient instance)
	{
		return instance?.ALGetBaseAddress ?? string.Empty;
	}
}
