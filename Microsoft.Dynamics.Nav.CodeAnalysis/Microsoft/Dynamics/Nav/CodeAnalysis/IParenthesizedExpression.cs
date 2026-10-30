namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IParenthesizedExpression : IOperation
{
	IOperation Operand { get; }
}
