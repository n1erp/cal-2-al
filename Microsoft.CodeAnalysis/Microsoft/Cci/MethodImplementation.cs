namespace Microsoft.Cci;

internal struct MethodImplementation(IMethodDefinition ImplementingMethod, IMethodReference ImplementedMethod)
{
	public readonly IMethodDefinition ImplementingMethod = ImplementingMethod;

	public readonly IMethodReference ImplementedMethod = ImplementedMethod;

	public ITypeDefinition ContainingType => ImplementingMethod.ContainingTypeDefinition;
}
