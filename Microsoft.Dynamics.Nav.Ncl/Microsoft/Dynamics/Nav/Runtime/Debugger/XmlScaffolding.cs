using System;
using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class XmlScaffolding : Scaffolding<NavXmlNode>
{
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

	private readonly NavNclType nclType;

	public override string TypeString => ScaffoldingHelper.GetTypeString(nclType, null);

	protected override bool ProbablyHasChildren => false;

	public XmlScaffolding(string name, Type type, NavNclType nclType, ValueBinder valueBinder, bool promotedChildren)
		: base(name, type, valueBinder, false, promotedChildren)
	{
		this.nclType = nclType;
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance is NavXmlNode;
	}

	protected override string GetStringValue(NavXmlNode instance)
	{
		if (instance != null && !instance.IsZeroOrEmpty)
		{
			return Scaffolding.TruncateIfNeeded(instance.DebuggerStringValue);
		}
		return "<Uninitialized>";
	}
}
