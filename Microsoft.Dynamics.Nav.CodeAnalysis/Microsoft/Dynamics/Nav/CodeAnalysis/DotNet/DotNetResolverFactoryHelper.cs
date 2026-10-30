using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

internal static class DotNetResolverFactoryHelper
{
	internal static IDotNetResolverFactory CreateLocalDotNetResolverFactory(IReadOnlyList<string> assemblyProbingPaths = null)
	{
		return new DotNetResolverFactory(new AssemblyLocator(assemblyProbingPaths));
	}
}
