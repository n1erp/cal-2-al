using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class HttpRequestMessageScaffolding : Scaffolding<NavHttpRequestMessage>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private const string MethodName = "Method";

	private const string RequestUriName = "RequestUri";

	public override string TypeString => ScaffoldingHelper.GetTypeString(NavNclType.NavHttpRequestMessage, null);

	protected override bool ProbablyHasChildren => true;

	public HttpRequestMessageScaffolding(string name, Type type, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, false, promotedChildren)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavHttpRequestMessage;
	}

	protected override IEnumerable<Scaffolding> CreateChildren(NavHttpRequestMessage instance)
	{
		Type httpRequestMessageType = instance.GetType();
		PropertyInfo property = httpRequestMessageType.GetProperty("ALMethod", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("Method", typeof(string), new PropertyInfoValueBinder(property), instance, false);
		PropertyInfo property2 = httpRequestMessageType.GetProperty("ALGetRequestUri", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		yield return ScaffoldingFactory.GetScaffolding("RequestUri", typeof(string), new PropertyInfoValueBinder(property2), instance, false);
	}

	protected override string GetStringValue(NavHttpRequestMessage instance)
	{
		return instance.ALMethod + " " + instance.ALGetRequestUri;
	}
}
