using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedClosureEnvironmentConstructor : SynthesizedInstanceConstructor, ISynthesizedMethodBodyImplementationSymbol
{
	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => ((ISynthesizedMethodBodyImplementationSymbol)ContainingSymbol).Method;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => false;

	internal SynthesizedClosureEnvironmentConstructor(SynthesizedClosureEnvironment frame)
		: base(frame)
	{
	}
}
