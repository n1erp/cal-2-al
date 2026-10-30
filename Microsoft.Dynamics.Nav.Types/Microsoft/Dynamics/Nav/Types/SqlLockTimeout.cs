namespace Microsoft.Dynamics.Nav.Types;

public class SqlLockTimeout : IntegerConfiguration<SqlLockTimeout>
{
	private const int MinimumValue = -1;

	public static readonly int MaxValue = int.MaxValue;

	public SqlLockTimeout(string value)
		: base(IntegerConfiguration<SqlLockTimeout>.MaxValueTextToInt(value, MaxValue), string.Empty, -1, MaxValue)
	{
	}

	public SqlLockTimeout(string value, string settingName)
		: base(IntegerConfiguration<SqlLockTimeout>.MaxValueTextToInt(value, MaxValue), settingName, -1, MaxValue)
	{
	}
}
