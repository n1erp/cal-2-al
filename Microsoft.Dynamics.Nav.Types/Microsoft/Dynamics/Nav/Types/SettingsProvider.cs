using System;
using System.Collections.Generic;
using System.Configuration;

namespace Microsoft.Dynamics.Nav.Types;

[CLSCompliant(true)]
public abstract class SettingsProvider
{
	public abstract bool TryGetConfigurationSettingValue(string settingName, out string value, HashSet<string> validValues = null, System.Configuration.Configuration config = null);

	public abstract void Refresh();
}
