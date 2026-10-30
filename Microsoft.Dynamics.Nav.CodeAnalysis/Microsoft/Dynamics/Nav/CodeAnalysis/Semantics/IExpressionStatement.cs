namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IExpressionStatement : IOperation
{
	IOperation Expression { get; }
}
