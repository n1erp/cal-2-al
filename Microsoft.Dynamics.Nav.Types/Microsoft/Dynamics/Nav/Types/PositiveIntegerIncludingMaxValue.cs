namespace Microsoft.Dynamics.Nav.Types;

public class PositiveIntegerIncludingMaxValue : IntegerConfiguration<PositiveIntegerIncludingMaxValue>
{
	private const int MinimumValue = 1;

	public static readonly int MaxValue = int.MaxValue;

	public PositiveIntegerIncludingMaxValue(string value)
		: base(IntegerConfiguration<PositiveIntegerIncludingMaxValue>.MaxValueTextToInt(value, MaxValue), string.Empty, 1, MaxValue)
	{
	}

	public PositiveIntegerIncludingMaxValue(string value, string settingName)
		: base(IntegerConfiguration<PositiveIntegerIncludingMaxValue>.MaxValueTextToInt(value, MaxValue), settingName, 1, MaxValue)
	{
	}
}
