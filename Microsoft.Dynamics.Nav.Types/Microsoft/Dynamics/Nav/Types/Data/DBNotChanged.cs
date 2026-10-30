namespace Microsoft.Dynamics.Nav.Types.Data;

public sealed class DBNotChanged
{
	public static readonly DBNotChanged Value = new DBNotChanged();

	private DBNotChanged()
	{
	}

	public override bool Equals(object obj)
	{
		return obj is DBNotChanged;
	}

	public override int GetHashCode()
	{
		return 1;
	}
}
