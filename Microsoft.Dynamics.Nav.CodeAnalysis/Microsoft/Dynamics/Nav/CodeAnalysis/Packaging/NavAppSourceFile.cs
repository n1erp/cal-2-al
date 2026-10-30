using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Packaging;

internal class NavAppSourceFile
{
	public string FileLocation { get; private set; }

	public bool PassedCheck
	{
		get
		{
			if (CheckerCategories.Any())
			{
				return ErrorCategories.All((KeyValuePair<string, HashSet<string>> p) => !p.Value.Any());
			}
			return false;
		}
	}

	public Dictionary<string, HashSet<string>> ErrorCategories { get; } = new Dictionary<string, HashSet<string>>();

	public Dictionary<string, HashSet<string>> WarningCategories { get; } = new Dictionary<string, HashSet<string>>();

	public HashSet<string> CheckerCategories { get; } = new HashSet<string>();

	public NavAppFileTypes FileTypes { get; set; }

	public NavAppSourceFile(string fileLocation)
	{
		FileLocation = fileLocation;
	}
}
