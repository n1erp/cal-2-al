using System;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;

public interface IDotNetResolverFactory : IDisposable
{
	IDotNetResolver CreateResolver();
}
