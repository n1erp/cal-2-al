namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IWhileRepeatLoopStatement : ILoopStatement, IOperation
{
	IOperation Condition { get; }
}
