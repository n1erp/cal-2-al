namespace Microsoft.CodeAnalysis;

internal struct UnifiedAssembly<TAssemblySymbol>(TAssemblySymbol targetAssembly, AssemblyIdentity originalReference) where TAssemblySymbol : class, IAssemblySymbol
{
	internal readonly AssemblyIdentity OriginalReference = originalReference;

	internal readonly TAssemblySymbol TargetAssembly = targetAssembly;
}
