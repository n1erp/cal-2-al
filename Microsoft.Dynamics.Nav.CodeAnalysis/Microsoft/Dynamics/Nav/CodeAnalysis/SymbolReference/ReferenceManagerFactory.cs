using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.SymbolReference;

public static class ReferenceManagerFactory
{
	public static IReferenceManager Create(IEnumerable<SymbolReferenceSpecification> symbolReferences, Compilation compilation)
	{
		return ReferenceManager.Create(symbolReferences, compilation.ReferenceLoader, compilation.DotNetResolverFactory);
	}
}
