namespace Microsoft.Dynamics.Nav.Types;

public class NonnegativeIntegerIncludingMaxValue : IntegerConfiguration<NonnegativeIntegerIncludingMaxValue>
{
	private const int MinimumValue = 0;

	public static readonly int MaxValue = int.MaxValue;

	public NonnegativeIntegerIncludingMaxValue(string value)
		: base(IntegerConfiguration<NonnegativeIntegerIncludingMaxValue>.MaxValueTextToInt(value, MaxValue), string.Empty, 0, MaxValue)
	{
	}

	public NonnegativeIntegerIncludingMaxValue(string value, string settingName)
		: base(IntegerConfiguration<NonnegativeIntegerIncludingMaxValue>.MaxValueTextToInt(value, MaxValue), settingName, 0, MaxValue)
	{
	}
}
