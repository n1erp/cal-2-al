using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public sealed class AssemblyLocator : AssemblyLocatorBase
{
	public AssemblyLocator(IEnumerable<string> probingPaths = null, IReadOnlyDictionary<string, string> wellKnownAssemblies = null)
		: base(probingPaths ?? Enumerable.Empty<string>(), wellKnownAssemblies ?? GetDefaultWellKnownAssemblies())
	{
	}

	protected override bool TryRetrieveAssembly(AssemblyNameAdapter searchName)
	{
		return false;
	}

	private static IReadOnlyDictionary<string, string> GetDefaultWellKnownAssemblies()
	{
		return DotNetUtilities.GetLocationOfLoadedAssemblies(AppDomain.CurrentDomain, cachePathsToAssembliesInAppDomain: false);
	}
}
