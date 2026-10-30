namespace Microsoft.Dynamics.Nav.Types;

public class TenantPollLimits : IntegerConfiguration<TenantPollLimits>
{
	internal const int MinPollCount = 1;

	internal const int MaxPollCount = 100;

	public TenantPollLimits(string value)
		: base(value, string.Empty, 1, 100)
	{
	}

	public TenantPollLimits(string value, string settingName)
		: base(value, settingName, 1, 100)
	{
	}
}
