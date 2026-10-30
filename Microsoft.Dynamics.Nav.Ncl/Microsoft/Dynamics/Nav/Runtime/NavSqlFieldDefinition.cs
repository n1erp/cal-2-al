namespace Microsoft.Dynamics.Nav.Runtime;

internal class NavSqlFieldDefinition
{
	public string Name { get; }

	public NavNclType Type { get; }

	public int Length { get; }

	internal NavSqlFieldDefinition(string name, NavNclType type, int length)
	{
		Name = name;
		Type = type;
		Length = length;
	}
}
