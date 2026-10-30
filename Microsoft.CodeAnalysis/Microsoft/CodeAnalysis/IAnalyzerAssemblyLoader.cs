using System.Reflection;

namespace Microsoft.CodeAnalysis;

public interface IAnalyzerAssemblyLoader
{
	Assembly LoadFromPath(string fullPath);

	void AddDependencyLocation(string fullPath);
}
