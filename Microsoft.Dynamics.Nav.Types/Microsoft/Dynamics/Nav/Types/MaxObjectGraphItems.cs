namespace Microsoft.Dynamics.Nav.Types;

public class MaxObjectGraphItems : IntegerConfiguration<MaxObjectGraphItems>
{
	private const int MinItems = 0;

	private const int MaxItems = 2147483;

	public static readonly int MaxValue = 2147483;

	public int ValueInOnes => Value * 1000;

	public MaxObjectGraphItems(string value)
		: base(value, string.Empty, 0, 2147483)
	{
	}

	public MaxObjectGraphItems(string value, string settingName)
		: base(value, settingName, 0, 2147483)
	{
	}
}
