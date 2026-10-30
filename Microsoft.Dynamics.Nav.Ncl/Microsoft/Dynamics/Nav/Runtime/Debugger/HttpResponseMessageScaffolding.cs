using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class HttpResponseMessageScaffolding : Scaffolding<NavHttpResponseMessage>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string HttpStatusCodeName = "HttpStatusCode";

	private const string ReasonPhraseName = "ReasonPhrase";

	private const string IsSuccessStatusCodeName = "IsSuccessStatusCode";

	private const string IsBlockedByEnvironmentName = "IsBlockedByEnvironment";

	private const string HeadersName = "Headers";

	public override string TypeString => ScaffoldingHelper.GetTypeString(NavNclType.NavHttpResponseMessage, null);

	protected override bool ProbablyHasChildren => true;

	public HttpResponseMessageScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, false, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavHttpResponseMessage;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavHttpResponseMessage instance)
	{
		Type httpResponseMessageType = instance.GetType();
		PropertyInfo property = httpResponseMessageType.GetProperty("ALHttpStatusCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("HttpStatusCode", typeof(int), new PropertyInfoValueBinder(property), instance, false);
		PropertyInfo property2 = httpResponseMessageType.GetProperty("ALReasonPhrase", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("ReasonPhrase", typeof(string), new PropertyInfoValueBinder(property2), instance, false);
		PropertyInfo property3 = httpResponseMessageType.GetProperty("ALIsSuccessStatusCode", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("IsSuccessStatusCode", typeof(bool), new PropertyInfoValueBinder(property3), instance, false);
		PropertyInfo property4 = httpResponseMessageType.GetProperty("ALIsBlockedByEnvironment", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("IsBlockedByEnvironment", typeof(bool), new PropertyInfoValueBinder(property4), instance, false);
		PropertyInfo property5 = httpResponseMessageType.GetProperty("ALHeaders", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Headers", typeof(NavHttpHeaders), new PropertyInfoValueBinder(property5), instance, false);
	}

	protected override string GetStringValue(NavHttpResponseMessage instance)
	{
		return string.Concat(instance.ALHttpStatusCode, " ", instance.ALReasonPhrase);
	}
}
