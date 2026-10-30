namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface IIfStatement : IOperation
{
	IOperation Condition { get; }

	IOperation IfTrueStatement { get; }

	IOperation IfFalseStatement { get; }
}
