namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IGlobalReferenceExpression : IOperation
{
	IVariableSymbol GlobalVariable { get; }
}
