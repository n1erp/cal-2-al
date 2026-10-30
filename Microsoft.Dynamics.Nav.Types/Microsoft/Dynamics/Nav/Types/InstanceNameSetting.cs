namespace Microsoft.Dynamics.Nav.Types;

public class InstanceNameSetting : RestrictedStringConfigSetting<InstanceNameSetting>
{
	private const int MaxLength = 250;

	private const string ValidStringRegEx = "[^A-Za-z0-9_-]";

	public InstanceNameSetting(string value)
		: base(value, string.Empty, false, 250, "[^A-Za-z0-9_-]")
	{
	}

	public InstanceNameSetting(string value, string settingName)
		: base(value, settingName, false, 250, "[^A-Za-z0-9_-]")
	{
	}

	public static bool IsValidServerInstanceName(string instanceName, out char[] invalidChars)
	{
		invalidChars = null;
		return SettingsHelper.IsValidString(instanceName, "[^A-Za-z0-9_-]", out invalidChars);
	}
}
