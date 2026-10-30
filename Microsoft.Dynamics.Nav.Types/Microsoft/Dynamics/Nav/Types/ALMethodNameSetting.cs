namespace Microsoft.Dynamics.Nav.Types;

public class ALMethodNameSetting : RestrictedStringConfigSetting<ALMethodNameSetting>
{
	private const string ValidStringRegex = "[^A-Za-z0-9_]";

	public ALMethodNameSetting(string value)
		: base(value, string.Empty, true, 128, "[^A-Za-z0-9_]")
	{
	}

	public ALMethodNameSetting(string value, string settingName)
		: base(value, settingName, true, 128, "[^A-Za-z0-9_]")
	{
	}
}
