namespace Microsoft.Dynamics.Nav.Types;

public class DataCacheSize : IntegerConfiguration<DataCacheSize>
{
	private const int MinSize = 1;

	private const int Multiplier = 1048576;

	private const int MaxSize = 20;

	public const int DefaultSize = 10;

	public long CacheSize => (1L << Value) * 1048576;

	public DataCacheSize(string value)
		: base(value, string.Empty, 1, 20)
	{
	}

	public DataCacheSize(string value, string settingName)
		: base(value, settingName, 1, 20)
	{
	}
}
