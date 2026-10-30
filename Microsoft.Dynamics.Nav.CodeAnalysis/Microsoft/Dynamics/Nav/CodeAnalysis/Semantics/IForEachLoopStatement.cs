namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IForEachLoopStatement : ILoopStatement, IOperation
{
	IOperation IterationVariable { get; }

	IOperation Expression { get; }
}
