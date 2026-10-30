namespace Microsoft.Dynamics.Nav.Types;

public class ServiceBusConnectionString : RestrictedStringConfigSetting<ServiceBusConnectionString>, IRestrictedConfigurationProperty
{
	public ServiceBusConnectionString(string value)
		: base(value, string.Empty, true, -1, (string)null)
	{
	}

	public ServiceBusConnectionString(string value, string settingName)
		: base(value, settingName, true, -1, (string)null)
	{
	}

	public string RestrictedString()
	{
		if (string.IsNullOrWhiteSpace(Value))
		{
			return string.Empty;
		}
		int num = Value.IndexOf(';');
		if (num < 0)
		{
			return Value;
		}
		return Value.Substring(0, num) + "<redacted>";
	}
}
