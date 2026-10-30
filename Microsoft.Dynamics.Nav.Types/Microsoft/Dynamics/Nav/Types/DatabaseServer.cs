namespace Microsoft.Dynamics.Nav.Types;

public class DatabaseServer : RestrictedStringConfigSetting<DatabaseServer>
{
	private const int MaxLength = 255;

	public DatabaseServer(string value)
		: base(value, string.Empty, true, 255, string.Empty)
	{
	}

	public DatabaseServer(string value, string settingName)
		: base(value, settingName, true, 255, string.Empty)
	{
	}
}
