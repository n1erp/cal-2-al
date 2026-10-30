using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class DotNetScaffolding : NavValueScaffolding
{
	private const string DotNetTypeName = "DotNet";

	private const string ClientDotNetTypeName = "DotNet on Client";

	private const string UnknownValue = "<Unknown>";

	private string typeString;

	protected override bool ProbablyHasChildren => false;

	public override string TypeString => typeString;

	public DotNetScaffolding(string name, Type type, ValueBinder valueBinder)
		: base(name, type, NavNclType.NavBigText, valueBinder)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		NavDotNet instance2 = instance as NavDotNet;
		if (typeString == GetRuntimeTypeName(instance2))
		{
			return true;
		}
		return false;
	}

	protected override void ReadMetadata(NavValue instance)
	{
		if (instance != null)
		{
			NavDotNet instance2 = (NavDotNet)instance;
			typeString = GetRuntimeTypeName(instance2);
		}
		else
		{
			typeString = string.Empty;
		}
	}

	protected override string GetStringValue(NavValue instance)
	{
		NavDotNet navDotNet = (NavDotNet)instance;
		if (navDotNet == null || navDotNet.IsNull)
		{
			return "<Uninitialized>";
		}
		if (navDotNet.Value is Process { HasExited: not false })
		{
			return "<Unknown>";
		}
		if (!navDotNet.RunOnClient)
		{
			return navDotNet.Value.ToString();
		}
		return "<Unknown>";
	}

	protected override bool TryParse(string newValueAsString, out object newValueAsObject)
	{
		newValueAsObject = null;
		return false;
	}

	private static string GetRuntimeTypeName(NavDotNet instance)
	{
		if (instance == null)
		{
			return null;
		}
		if (instance.RunOnClient)
		{
			if (instance.TypeName != null)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} \"{1}\"", "DotNet on Client", instance.TypeName);
			}
			return "DotNet on Client";
		}
		if (instance.Value == null)
		{
			if (instance.TypeName != null)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} \"{1}\"", "DotNet", instance.TypeName);
			}
			return "DotNet";
		}
		return string.Format(CultureInfo.InvariantCulture, "{0} \"{1}\"", "DotNet", instance.Value.GetType().ToString());
	}
}
