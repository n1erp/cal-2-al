namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IParameterReferenceExpression : IOperation
{
	IParameterSymbol Parameter { get; }
}
