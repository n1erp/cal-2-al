namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public class ObjectChangeModelDefinition
{
	public ObjectChangeElement[] Added { get; set; }

	public ObjectChangeElement[] Modified { get; set; }

	public ObjectChangeElement[] Removed { get; set; }
}
