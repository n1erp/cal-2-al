using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet.Cecil;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public class DotNetResolverFactory : IDotNetResolverFactory, IDisposable
{
	private readonly IDotNetTypeLoader cachedTypeLoader;

	public DotNetResolverFactory(IAssemblyLocator assemblyLocator)
	{
		cachedTypeLoader = new CecilDotNetTypeLoader(assemblyLocator);
	}

	[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "cachedTypeLoader")]
	public void Dispose()
	{
		cachedTypeLoader?.Dispose();
	}

	public IDotNetResolver CreateResolver()
	{
		return new DotNetResolver(cachedTypeLoader, new DotNetTypeMapper());
	}
}
