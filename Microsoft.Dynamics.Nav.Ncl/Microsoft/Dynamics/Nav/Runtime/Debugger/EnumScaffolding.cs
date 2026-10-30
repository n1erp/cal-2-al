using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class EnumScaffolding : Scaffolding<Enum>
{
	private string typeString;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => false;

	public EnumScaffolding(string name, Type type, ValueBinder valueBinder)
		: base(name, type, valueBinder, true, false)
	{
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		return instance?.GetType().IsEnum ?? false;
	}

	protected override string GetStringValue(Enum instance)
	{
		return string.Concat(Convert.ChangeType(instance, instance.GetTypeCode(), CultureInfo.InvariantCulture), " - ", instance.ToString());
	}

	protected override void ReadMetadata(Enum instance)
	{
		typeString = ((instance != null) ? ScaffoldingHelper.GetTypeString(instance.GetType(), instance) : string.Empty);
	}
}
