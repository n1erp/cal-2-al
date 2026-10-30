using System;
using System.Globalization;

namespace Microsoft.Dynamics.Nav.Types.Metadata;

public class UIElementIdentifier
{
	private static int DefaultContainerId;

	public string ControlALName { get; }

	public int ControlRuntimeId { get; }

	public bool IsSpecified { get; }

	public UIElementIdentifier()
		: this(string.Empty)
	{
	}

	public UIElementIdentifier(string name)
		: this(DefaultContainerId, name)
	{
	}

	public UIElementIdentifier(int controlRuntimeId, string controlName = null)
	{
		ControlRuntimeId = controlRuntimeId;
		ControlALName = controlName;
		IsSpecified = !string.IsNullOrWhiteSpace(ControlALName) || ControlRuntimeId != DefaultContainerId;
	}

	public override bool Equals(object obj)
	{
		if (obj is UIElementIdentifier uIElementIdentifier)
		{
			if (!string.IsNullOrWhiteSpace(ControlALName))
			{
				return ControlALName.Equals(uIElementIdentifier.ControlALName, StringComparison.OrdinalIgnoreCase);
			}
			return ControlRuntimeId == uIElementIdentifier.ControlRuntimeId;
		}
		return false;
	}

	public bool Identifies(IDElementDefinition elementDefinition)
	{
		return ControlRuntimeId == elementDefinition?.ID;
	}

	public bool Identifies(NamedElementDefinition elementDefinition)
	{
		if (!(elementDefinition is ControlGroupDefinition))
		{
			string value = elementDefinition?.Name;
			if (!string.IsNullOrWhiteSpace(ControlALName) && !string.IsNullOrWhiteSpace(value))
			{
				return ControlALName.Equals(value, StringComparison.OrdinalIgnoreCase);
			}
		}
		return Identifies((IDElementDefinition)elementDefinition);
	}

	public override int GetHashCode()
	{
		if (!string.IsNullOrWhiteSpace(ControlALName))
		{
			return ControlALName.GetHashCode();
		}
		return ControlRuntimeId.GetHashCode();
	}

	public string ToString(CultureInfo cultureInfo)
	{
		if (string.IsNullOrWhiteSpace(ControlALName))
		{
			return ControlRuntimeId.ToString(cultureInfo);
		}
		return ControlALName?.ToString(cultureInfo);
	}

	public override string ToString()
	{
		return ToString(CultureInfo.CurrentCulture);
	}
}
