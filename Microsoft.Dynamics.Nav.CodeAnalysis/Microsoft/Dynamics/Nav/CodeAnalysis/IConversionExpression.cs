namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IConversionExpression : IOperation
{
	IOperation Operand { get; }
}
