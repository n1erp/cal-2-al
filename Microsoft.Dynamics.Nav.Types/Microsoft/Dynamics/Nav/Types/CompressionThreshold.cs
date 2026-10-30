namespace Microsoft.Dynamics.Nav.Types;

public class CompressionThreshold : IntegerConfiguration<CompressionThreshold>
{
	private const int CompressionThresholdMin = 16;

	private const int CompressionThresholdMax = 2000;

	private const int BytesMultiplier = 1024;

	public int ValueKB => Value * 1024;

	public CompressionThreshold(string value)
		: base(value, string.Empty, 16, 2000)
	{
	}

	public CompressionThreshold(string value, string settingName)
		: base(value, settingName, 16, 2000)
	{
	}
}
