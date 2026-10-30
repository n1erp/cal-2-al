namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface ILocalReferenceExpression : IOperation
{
	IVariableSymbol LocalVariable { get; }
}
