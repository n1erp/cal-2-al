namespace Microsoft.Dynamics.Nav.Types;

public class ExtendedTokenLifetime : IntegerConfiguration<ExtendedTokenLifetime>
{
	internal const int MinValue = 0;

	internal const int MaxValue = 24;

	public ExtendedTokenLifetime(string value)
		: base(value, string.Empty, 0, 24)
	{
	}

	public ExtendedTokenLifetime(string value, string settingName)
		: base(value, settingName, 0, 24)
	{
	}
}
