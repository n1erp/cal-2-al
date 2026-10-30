using System;

namespace Microsoft.Dynamics.Nav.Types;

public sealed class SettingsChangedEventArgs : EventArgs
{
	public string SettingName { get; }

	internal SettingsChangedEventArgs(string settingName)
	{
		SettingName = settingName;
	}
}
