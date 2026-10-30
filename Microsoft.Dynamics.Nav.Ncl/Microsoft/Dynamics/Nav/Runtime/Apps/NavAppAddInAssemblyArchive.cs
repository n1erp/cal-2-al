using System.IO;

namespace Microsoft.Dynamics.Nav.Runtime.Apps;

public class NavAppAddInAssemblyArchive
{
	public string Name { get; }

	public Stream Stream { get; }

	public NavAppAddInAssemblyArchive(string name, Stream stream)
	{
		Name = name;
		Stream = stream;
	}
}
