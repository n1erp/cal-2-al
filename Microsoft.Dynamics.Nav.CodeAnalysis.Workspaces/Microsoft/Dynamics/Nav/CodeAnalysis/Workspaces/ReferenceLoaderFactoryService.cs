using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces;

internal class ReferenceLoaderFactoryService : IReferenceLoaderFactoryService, ILanguageService
{
	public ISymbolReferenceLoader GetSymbolReferenceLoader(string cachePath)
	{
		return new MemoryCachedSymbolReferenceLoader(new LocalCacheSymbolReferenceLoader(cachePath));
	}
}
