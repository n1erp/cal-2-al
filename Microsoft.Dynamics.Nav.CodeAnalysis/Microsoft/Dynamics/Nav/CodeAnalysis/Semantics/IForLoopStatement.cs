namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IForLoopStatement : ILoopStatement, IOperation
{
	IOperation LoopVariable { get; }

	IOperation InitialValue { get; }

	IOperation EndValue { get; }

	bool IsDownTo { get; }
}
