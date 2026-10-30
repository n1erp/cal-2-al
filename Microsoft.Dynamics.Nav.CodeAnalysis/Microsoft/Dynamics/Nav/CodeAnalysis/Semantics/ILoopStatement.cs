namespace Microsoft.Dynamics.Nav.CodeAnalysis.Semantics;

public interface ILoopStatement : IOperation
{
	LoopKind LoopKind { get; }

	IOperation Body { get; }
}
