namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IUnaryOperatorExpression : IOperation
{
	UnaryOperationKind UnaryOperationKind { get; }

	IOperation Operand { get; }
}
