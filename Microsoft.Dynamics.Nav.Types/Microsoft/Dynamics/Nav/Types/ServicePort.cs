namespace Microsoft.Dynamics.Nav.Types;

public class ServicePort : IntegerConfiguration<ServicePort>
{
	internal const int MinPortNumber = 1;

	internal const int MaxPortNumber = 65535;

	public ServicePort(string value)
		: base(value, string.Empty, 1, 65535)
	{
	}

	public ServicePort(string value, string settingName)
		: base(value, settingName, 1, 65535)
	{
	}
}
