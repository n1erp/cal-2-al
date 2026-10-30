using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

internal class NullAssemblyLocator : IAssemblyLocator
{
	public IEnumerable<string> ProbingPaths { get; } = new List<string>();

	public string GetPathToAssembly(string fullyQualifiedAssemblyName)
	{
		return null;
	}
}
