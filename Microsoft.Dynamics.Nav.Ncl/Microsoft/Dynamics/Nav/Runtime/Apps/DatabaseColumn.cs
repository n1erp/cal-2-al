namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class DatabaseColumn
{
	public string Name { get; }

	public string SqlType { get; }

	public int Length { get; }

	public DatabaseColumn(string name, string sqlType, int length)
	{
		Name = name;
		SqlType = sqlType;
		Length = length;
	}
}
