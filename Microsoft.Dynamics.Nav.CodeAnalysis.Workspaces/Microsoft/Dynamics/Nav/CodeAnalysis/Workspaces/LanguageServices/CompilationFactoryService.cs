using Microsoft.Dynamics.Nav.CodeAnalysis.DotNet;
using Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Host;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.LanguageServices;

internal class CompilationFactoryService : ICompilationFactoryService, ILanguageService
{
	private static readonly CompilationOptions DefaultOptions = new CompilationOptions(concurrentBuild: false);

	private readonly IDotNetResolverFactory dotNetResolverFactory = DotNetResolverFactoryHelper.CreateLocalDotNetResolverFactory();

	Compilation ICompilationFactoryService.CreateCompilation(string assemblyName, CompilationOptions options)
	{
		return Compilation.Create(assemblyName, null, null, null, null, options ?? DefaultOptions, null, dotNetResolverFactory);
	}

	CompilationOptions ICompilationFactoryService.GetDefaultCompilationOptions()
	{
		return DefaultOptions;
	}
}
