using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class SynthesizedStateMachineMethod : SynthesizedImplementationMethod, ISynthesizedMethodBodyImplementationSymbol
{
	private readonly bool _hasMethodBodyDependency;

	public StateMachineTypeSymbol StateMachineType => (StateMachineTypeSymbol)ContainingSymbol;

	public bool HasMethodBodyDependency => _hasMethodBodyDependency;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => StateMachineType.KickoffMethod;

	protected SynthesizedStateMachineMethod(string name, MethodSymbol interfaceMethod, StateMachineTypeSymbol stateMachineType, PropertySymbol associatedProperty, bool generateDebugInfo, bool hasMethodBodyDependency)
		: base(interfaceMethod, stateMachineType, name, generateDebugInfo, associatedProperty)
	{
		_hasMethodBodyDependency = hasMethodBodyDependency;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		return StateMachineType.KickoffMethod.CalculateLocalSyntaxOffset(localPosition, localTree);
	}
}
