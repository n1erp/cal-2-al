namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppAddInRawAssembly
{
	public string Name { get; }

	public byte[] Content { get; }

	public NavAppAddInRawAssembly(string name, byte[] content)
	{
		Name = name;
		Content = content;
	}
}
