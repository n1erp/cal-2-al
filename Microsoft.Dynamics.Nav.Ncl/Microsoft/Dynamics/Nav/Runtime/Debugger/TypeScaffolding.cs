using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Runtime.Debugger;

internal class TypeScaffolding(string name, Type type, ValueBinder valueBinder) : Scaffolding(name, type, valueBinder, childrenTypesCanChange: false, promotedChildren: false)
{
	private string typeString = string.Empty;

	public override string TypeString => typeString;

	protected override bool ProbablyHasChildren => false;

	protected override bool SetValueSupported => true;

	protected internal override string GetStringValue(object instance)
	{
		if (instance == null)
		{
			return "<Uninitialized>";
		}
		if (!(instance is IFormattable formattable))
		{
			return instance.ToString();
		}
		if (instance.GetType() == typeof(byte))
		{
			return ScaffoldingHelper.ByteToStringValue((byte)instance);
		}
		return formattable.ToString(null, NavCurrentThread.Session.WindowsCulture);
	}

	protected override bool IsCompatibleOverride(object instance)
	{
		if (instance != null)
		{
			return instance.GetType() == TargetType;
		}
		return false;
	}

	protected override bool TryParse(string newValueAsString, out object newValueAsObject)
	{
		if (typeof(IConvertible).IsAssignableFrom(TargetType))
		{
			try
			{
				newValueAsObject = Convert.ChangeType(newValueAsString, TargetType, CultureInfo.InvariantCulture);
				return true;
			}
			catch (FormatException)
			{
			}
			catch (OverflowException)
			{
			}
			catch (InvalidCastException)
			{
			}
		}
		return base.TryParse(newValueAsString, out newValueAsObject);
	}

	protected override void ReadMetadata(object instance)
	{
		if (instance != null)
		{
			typeString = ScaffoldingHelper.GetTypeString(TargetType, instance);
		}
		else
		{
			typeString = string.Empty;
		}
		base.ReadMetadata(instance);
	}
}
