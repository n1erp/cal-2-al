using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public class NullDotNetResolverFactory : IDotNetResolverFactory, IDisposable
{
	public static IDotNetResolverFactory Instance { get; } = new NullDotNetResolverFactory();

	public void Dispose()
	{
	}

	public IDotNetResolver CreateResolver()
	{
		return NullDotNetResolver.Instance;
	}
}
