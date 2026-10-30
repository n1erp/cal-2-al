namespace Microsoft.Dynamics.Nav.Types;

public class WebServiceMaxMessageSize : IntegerConfiguration<WebServiceMaxMessageSize>
{
	private const int MinSize = 64;

	private const int MaxSize = 100000;

	public int ValueInBytes => Value * 1024;

	public WebServiceMaxMessageSize(string value)
		: base(value, string.Empty, 64, 100000)
	{
	}

	public WebServiceMaxMessageSize(string value, string settingName)
		: base(value, settingName, 64, 100000)
	{
	}
}
