namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.Integer)]
public class IntegerType : IModelType
{
	private readonly int? value;

	private const int LowCache = -128;

	private const int HighCache = 128;

	private static IntegerType[] cache;

	private static IntegerType empty;

	public DataTypes DataType => DataTypes.Integer;

	public bool HasValue => value.HasValue;

	public int Value
	{
		get
		{
			if (!value.HasValue)
			{
				return 0;
			}
			return value.Value;
		}
	}

	public CompiledState CompiledState => CompiledState.Compiled;

	public static IntegerType Empty => empty;

	private IntegerType(int? value)
	{
		this.value = value;
	}

	static IntegerType()
	{
		empty = new IntegerType(null);
		cache = new IntegerType[257];
		for (int i = -128; i <= 128; i++)
		{
			cache[i - -128] = new IntegerType(i);
		}
	}

	public static IntegerType Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Empty;
		}
		int num = int.Parse(value);
		if (num >= -128 && num <= 128)
		{
			return cache[num - -128];
		}
		return new IntegerType(num);
	}
}
