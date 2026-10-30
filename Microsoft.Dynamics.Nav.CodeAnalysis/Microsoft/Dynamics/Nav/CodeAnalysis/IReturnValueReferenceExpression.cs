namespace Microsoft.Dynamics.Nav.CodeAnalysis;

public interface IReturnValueReferenceExpression : IOperation
{
	IReturnValueSymbol ReturnValue { get; }
}
