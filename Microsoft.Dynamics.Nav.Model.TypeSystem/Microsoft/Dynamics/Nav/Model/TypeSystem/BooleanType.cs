using System.Globalization;

namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.Boolean)]
public class BooleanType : IModelType
{
	private readonly byte value;

	private static BooleanType empty = new BooleanType();

	private static BooleanType yes = new BooleanType(value: true);

	private static BooleanType no = new BooleanType(value: false);

	public const string YesString = "Yes";

	public const string NoString = "No";

	public DataTypes DataType => DataTypes.Boolean;

	public bool HasValue => value > 0;

	public bool Value => value == 1;

	public CompiledState CompiledState => CompiledState.Compiled;

	public static BooleanType Empty => empty;

	public static BooleanType True => yes;

	public static BooleanType Yes => yes;

	public static BooleanType False => no;

	public static BooleanType No => no;

	private BooleanType()
	{
	}

	private BooleanType(bool value)
	{
		this.value = (byte)(value ? 1 : 2);
	}

	public static BooleanType Create(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return Empty;
		}
		if (value == "Yes")
		{
			return yes;
		}
		if (value == "No")
		{
			return no;
		}
		if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			switch (result)
			{
			case 0:
				return false;
			case 1:
				return true;
			}
		}
		return bool.Parse(value);
	}

	public static implicit operator bool(BooleanType booleanType)
	{
		return booleanType.Value;
	}

	public static implicit operator BooleanType(bool boolean)
	{
		if (!boolean)
		{
			return No;
		}
		return Yes;
	}
}
