using System.Reflection;

namespace Microsoft.Dynamics.Nav.Runtime;

public interface INavAddInProvider
{
	bool ExtractAddInResourceFromAddInTable(string assemblyFullName, bool includeJavaScript = false);

	Assembly ResolveAddInFromAddInTable(string assemblyFullName);
}
