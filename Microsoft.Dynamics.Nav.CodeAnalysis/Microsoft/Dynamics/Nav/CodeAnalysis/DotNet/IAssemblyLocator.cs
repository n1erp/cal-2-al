using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public interface IAssemblyLocator
{
	IEnumerable<string> ProbingPaths { get; }

	string GetPathToAssembly(string fullyQualifiedAssemblyName);
}
