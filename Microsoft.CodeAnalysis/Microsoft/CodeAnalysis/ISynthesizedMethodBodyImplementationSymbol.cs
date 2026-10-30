namespace Microsoft.CodeAnalysis;

internal interface ISynthesizedMethodBodyImplementationSymbol
{
	IMethodSymbol Method { get; }

	bool HasMethodBodyDependency { get; }
}
