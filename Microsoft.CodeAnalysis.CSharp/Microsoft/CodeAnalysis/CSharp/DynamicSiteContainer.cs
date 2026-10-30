using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DynamicSiteContainer : SynthesizedContainer, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly MethodSymbol _topLevelMethod;

	public override Symbol ContainingSymbol => _topLevelMethod.ContainingSymbol;

	public override TypeKind TypeKind => TypeKind.Class;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => _topLevelMethod;

	internal DynamicSiteContainer(string name, MethodSymbol topLevelMethod)
		: base(name, topLevelMethod)
	{
		_topLevelMethod = topLevelMethod;
	}
}
