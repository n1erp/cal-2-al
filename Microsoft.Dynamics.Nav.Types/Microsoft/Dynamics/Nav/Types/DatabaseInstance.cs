namespace Microsoft.Dynamics.Nav.Types;

public class DatabaseInstance : RestrictedStringConfigSetting<DatabaseInstance>
{
	private const int MaxLength = 16;

	private const string ValidStringRegEx = "(?i)^MSSQLServer$|(?i)^Default$|^[^\\p{L}_#&]|^.+[^\\p{L}\\p{Nd}_#$]";

	public DatabaseInstance(string value)
		: base(value, string.Empty, true, 16, "(?i)^MSSQLServer$|(?i)^Default$|^[^\\p{L}_#&]|^.+[^\\p{L}\\p{Nd}_#$]")
	{
	}

	public DatabaseInstance(string value, string settingName)
		: base(value, settingName, true, 16, "(?i)^MSSQLServer$|(?i)^Default$|^[^\\p{L}_#&]|^.+[^\\p{L}\\p{Nd}_#$]")
	{
	}
}
