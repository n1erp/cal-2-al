namespace Microsoft.Dynamics.Nav.Types;

public class DatabaseName : RestrictedStringConfigSetting<DatabaseName>
{
	private const int MaxLength = 123;

	private const string ValidStringRegEx = "";

	public DatabaseName(string value)
		: base(value, string.Empty, true, 123, "")
	{
	}

	public DatabaseName(string value, string settingName)
		: base(value, settingName, true, 123, "")
	{
	}
}
