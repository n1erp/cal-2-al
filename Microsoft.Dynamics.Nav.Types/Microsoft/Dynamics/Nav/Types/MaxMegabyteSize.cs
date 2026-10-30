namespace Microsoft.Dynamics.Nav.Types;

public class MaxMegabyteSize : IntegerConfiguration<MaxMegabyteSize>
{
	private const int MinSize = 1;

	private const int BytesMultiplier = 1048576;

	private const int MaxSize = 2047;

	public int ValueInBytes => Value * 1048576;

	public MaxMegabyteSize(string value)
		: base(value, string.Empty, 1, 2047)
	{
	}

	public MaxMegabyteSize(string value, string settingName)
		: base(value, settingName, 1, 2047)
	{
	}
}
